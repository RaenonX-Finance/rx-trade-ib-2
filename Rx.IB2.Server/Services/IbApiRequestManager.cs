using System.Collections.Concurrent;
using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;
using Rx.IB2.Models;

namespace Rx.IB2.Services;

public class IbApiRequestManager {
    private int _requestId;

    // Key = Account, Value = Requests
    private readonly ConcurrentDictionary<string, ConcurrentBag<IbApiRequest>> _requests = new();

    // Key = Request ID, Value = Account
    private readonly ConcurrentDictionary<int, string> _requestOfAccount = new();

    // Key = Request ID, Value = Contract ID
    private readonly ConcurrentDictionary<int, IbApiContractIdAssociation> _contractAssociationByRequest = new();

    // Key = Contract ID, Value = Contract
    private readonly ConcurrentDictionary<int, Contract> _contractPool = new();

    private readonly ConcurrentDictionary<int, Order> _orderPoolByPermId = new();

    public IEnumerable<string> Accounts => _requests.Keys;

    private ConcurrentBag<IbApiRequest> GetBagOfAccount(string account) {
        return _requests.GetOrAdd(account, _ => new ConcurrentBag<IbApiRequest>());
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
            .Where(x => x.Type == IbApiRequestType.Realtime)
            .Select(x => x.Id);

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
        GetBagOfAccount(account).Add(
            new IbApiRequest {
                Type = type,
                Id = toReturn
            }
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
                if (x.Type != type) {
                    return false;
                }

                var hasContractAssociation = _contractAssociationByRequest.TryGetValue(
                    x.Id,
                    out var contractIdAssociation
                );
                if (!hasContractAssociation) {
                    return false;
                }

                return contractIdAssociation.ContractId == contractId;
            })
            .Select(x => x.Id);
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
        foreach (var request in GetBagOfAccount(account)) {
            requestDisposer(request);
        }

        GetBagOfAccount(account).Clear();
    }
}