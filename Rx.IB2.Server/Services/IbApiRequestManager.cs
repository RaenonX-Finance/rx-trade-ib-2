using System.Collections.Concurrent;
using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;
using Rx.IB2.Models;

namespace Rx.IB2.Services;

public class IbApiRequestManager {
    private int _requestId;

    // Key = Account, Value = Requests
    // > Using dictionary for easy single element removal - https://stackoverflow.com/q/3029818
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<IbApiRequest, bool>> _requests = new();

    // Key = Request ID, Value = Account
    private readonly ConcurrentDictionary<int, string> _requestOfAccount = new();

    // Key = Request ID, Value = Contract ID
    private readonly ConcurrentDictionary<int, IbApiContractIdAssociation> _contractAssociationByRequest = new();

    // Key = Contract ID, Value = Contract
    private readonly ConcurrentDictionary<int, Contract> _contractPool = new();

    // Key = Order Perm ID, Value = Order
    private readonly ConcurrentDictionary<int, Order> _orderPoolByPermId = new();

    public IEnumerable<string> Accounts => _requests.Keys;

    private void ClearByAccount(string account) {
        GetBagOfAccount(account).Clear();

        foreach (var entryToRemove in _requestOfAccount.Where(x => x.Value == account)) {
            _requestOfAccount.TryRemove(entryToRemove);
        }

        foreach (var entryToRemove in _contractAssociationByRequest.Where(x => x.Value.Account == account)) {
            _contractAssociationByRequest.TryRemove(entryToRemove);
        }
    }

    public void ClearByContractId(IbApiRequestType requestType, string account, int contractId) {
        var requestBag = GetBagOfAccount(account);
        foreach (var request in requestBag.Where(x => x.Key.Type == requestType)) {
            var requestId = request.Key.Id;
            
            if (!_contractAssociationByRequest.TryGetValue(requestId, out var contractIdAssociation)) {
                continue;
            }

            if (contractIdAssociation.ContractId != contractId) {
                continue;
            }
            
            requestBag.Remove(request.Key, out _);
            _requestOfAccount.Remove(requestId, out _);
            _contractAssociationByRequest.Remove(requestId, out _);
        }
    }

    public void ClearByRequestId(int requestId) {
        var removed = false;
        
        foreach (var requestsByAccount in _requests) {
            foreach (var request in requestsByAccount.Value) {
                if (request.Key.Id != requestId) {
                    continue;
                }

                _requests[requestsByAccount.Key].TryRemove(request);
                removed = true;
                break;
            }

            if (removed) {
                break;
            }
        }
        
        _requestOfAccount.Remove(requestId, out _);
        _contractAssociationByRequest.Remove(requestId, out _);
    }

    private ConcurrentDictionary<IbApiRequest, bool> GetBagOfAccount(string account) {
        return _requests.GetOrAdd(account, _ => new ConcurrentDictionary<IbApiRequest, bool>());
    }

    public string? GetAccountOfRequest(int requestId) {
        return _requestOfAccount.TryGetValueOrNull(requestId);
    }

    public void AddContractToPool(Contract contract) {
        _contractPool.TryAdd(contract.ConId, contract);
    }

    public Contract? GetContractFromPool(int contractId) {
        return _contractPool.TryGetValueOrNull(contractId);
    }

    public bool IsContractSubscribingRealtime(string account, Contract contract) {
        var realtimeRequestIds = GetBagOfAccount(account)
            .Where(x => x.Key.Type == IbApiRequestType.Realtime)
            .Select(x => x.Key.Id);

        return realtimeRequestIds.Any(x => {
            var contractIdAssociation = _contractAssociationByRequest.TryGetValueOrNull(x);

            if (contractIdAssociation is null) {
                return false;
            }

            return contractIdAssociation.Value.ContractId == contract.ConId;
        });
    }

    public void SetRequestId(int requestId) {
        _requestId = requestId;
    }

    private int GetNextRequestId() {
        return ++_requestId;
    }

    public int GetNextRequestIdNoCancel() {
        return GetNextRequestId();
    }

    public int GetNextRequestId(IbApiRequestType type, string account) {
        var toReturn = GetNextRequestId();

        _requestOfAccount.TryAdd(toReturn, account);
        GetBagOfAccount(account).AddOrUpdate(
            new IbApiRequest {
                Type = type,
                Id = toReturn
            },
            _ => true,
            (_, _) => true
        );

        return toReturn;
    }

    public int GetNextRequestId(IbApiRequestType type, string account, int contractId) {
        var requestId = GetNextRequestId(type, account);

        // This might be indirectly called by a method with a manually created contract
        if (contractId != default) {
            _contractAssociationByRequest.TryAdd(requestId, new IbApiContractIdAssociation {
                Account = account,
                ContractId = contractId
            });
        }

        return requestId;
    }

    public Contract? GetContractByRequestId(int requestId) {
        var contractIdAssociation = _contractAssociationByRequest.TryGetValueOrNull(requestId);

        return contractIdAssociation is null
            ? null
            : _contractPool.TryGetValueOrNull(contractIdAssociation.Value.ContractId);
    }

    public IEnumerable<int> GetRequestIdsByContractIdAndType(IbApiRequestType type, string account, int contractId) {
        return GetBagOfAccount(account)
            .Where(x => {
                if (x.Key.Type != type) {
                    return false;
                }

                var hasContractAssociation = _contractAssociationByRequest.TryGetValue(
                    x.Key.Id,
                    out var contractIdAssociation
                );
                if (!hasContractAssociation) {
                    return false;
                }

                return contractIdAssociation.ContractId == contractId;
            })
            .Select(x => x.Key.Id);
    }

    public IbApiContractIdAssociation? GetContractAssociation(int requestId) {
        return _contractAssociationByRequest.TryGetValueOrNull(requestId);
    }

    public void AddOrderToPoolByPermId(int orderPermId, Order order) {
        _orderPoolByPermId.TryAdd(orderPermId, order);
    }

    public Order? GetOrderByPermId(int orderPermId) {
        return _orderPoolByPermId.TryGetValueOrNull(orderPermId);
    }

    public void CancelRequest(string account, Action<IbApiRequest> requestDisposer) {
        foreach (var request in GetBagOfAccount(account).Keys) {
            requestDisposer(request);
        }

        ClearByAccount(account);
    }
}