using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;
using Rx.IB2.Interfaces;
using Rx.IB2.Models;
using Rx.IB2.Models.Options;
using Rx.IB2.Models.Requests;
using Rx.IB2.Models.Responses;
using Rx.IB2.Services.IbApiHandlers;
using Rx.IB2.Utils;
using ILogger = Serilog.ILogger;

namespace Rx.IB2.Services;

public class IbApiSender {
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(IbApiSender));

    private EClientSocket ClientSocket { get; }

    private IbApiRequestManager RequestManager { get; }

    private IbApiHistoryPxRequestManager HistoryPxRequestManager { get; }

    private IbApiContractDetailsManager ContractDetailsManager { get; }

    private IbApiOptionDefinitionsManager OptionDefinitionsManager { get; }

    private IConfiguration Config { get; }

    public IbApiSender(
        IConfiguration config,
        IbApiHandler handler,
        IbApiRequestManager requestManager,
        IbApiHistoryPxRequestManager historyPxRequestManager,
        IbApiContractDetailsManager contractDetailsManager,
        IbApiOptionDefinitionsManager optionDefinitionsManager
    ) {
        Config = config;
        ClientSocket = handler.ClientSocket;
        RequestManager = requestManager;
        HistoryPxRequestManager = historyPxRequestManager;
        ContractDetailsManager = contractDetailsManager;
        OptionDefinitionsManager = optionDefinitionsManager;
    }

    private void CancelRequests(string account) {
        Log.Information("Cancelling all requests of {Account}", account);
        RequestManager.CancelRequest(
            account,
            request => {
                Log.Information("#{RequestId}: Cancelling {RequestType} request", request.Id, request.Type);
                switch (request.Type) {
                    case IbApiRequestType.Realtime:
                        ClientSocket.cancelMktData(request.Id);
                        break;
                    case IbApiRequestType.PnL:
                        ClientSocket.cancelPnL(request.Id);
                        break;
                    case IbApiRequestType.PnLSingle:
                        ClientSocket.cancelPnLSingle(request.Id);
                        break;
                    case IbApiRequestType.History:
                        ClientSocket.cancelHistoricalData(request.Id);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(request),
                            $"Unhandled request type to cancel: {request.Type}"
                        );
                }
            }
        );
    }

    public void Connect() {
        var section = Config.GetSection("TWS");
        var port = section.GetValue<int>("Port");
        var clientId = section.GetValue<int>("ClientId");

        Log.Information("Connecting IB API at port {Port} of client ID {ClientId}", port, clientId);
        ClientSocket.eConnect("localhost", port, clientId);

        RequestOrders();
    }

    public void Disconnect() {
        Log.Information("Unsubscribing all account updates before IB API disconnection");

        foreach (var account in RequestManager.Accounts) {
            UnsubscribeAccountUpdates(account);
        }

        Log.Information("Disconnecting IB API");
        ClientSocket.eDisconnect();
    }

    public void RequestManagedAccounts() {
        Log.Information("Requesting managed account numbers");
        ClientSocket.reqManagedAccts();
    }

    public void SubscribeAccountUpdates(string account) {
        Log.Information("Subscribing account update of {Account}", account);

        ClientSocket.reqAccountUpdates(true, account);
        RequestAccountPnL(account);
    }

    public void UnsubscribeAccountUpdates(string account) {
        Log.Information("Unsubscribing account update of {Account}", account);
        ClientSocket.reqAccountUpdates(false, account);
        CancelRequests(account);
    }

    public int? RequestRealtime(string account, Contract contract) {
        var isSubscribing = RequestManager.IsContractSubscribingRealtime(account, contract);
        if (isSubscribing) {
            Log.Information("Contract [{ContractId}] is already subscribed to market data", contract.ConId);
            return null;
        }

        var requestId = RequestManager.GetNextRequestId(IbApiRequestType.Realtime, account, contract.ConId);
        var tickToRequest = new[] { MarketPxRequestTick.Mark };

        Log.Information(
            "#{RequestId}: Subscribing realtime data of {ContractString}",
            requestId,
            contract.ToString()
        );

        // Make sure `contract` has exchange set
        contract.AddExchangeOnContract();

        ClientSocket.reqMktData(
            requestId,
            contract,
            string.Join(",", tickToRequest.Select(x => Convert.ToInt32(x))),
            false,
            false,
            null
        );

        return requestId;
    }

    public void CancelRealtime(string account, int contractId) {
        Log.Information("Received request to cancel market data of contract [{ContractId}]", contractId);

        var requestIds = RequestManager.GetRequestIdsByContractIdAndType(
            IbApiRequestType.Realtime,
            account,
            contractId
        );
        foreach (var requestId in requestIds) {
            Log.Information("#{RequestId}: Cancelling realtime data of [{ContractId}]", requestId, contractId);
            ClientSocket.cancelMktData(requestId);
        }

        RequestManager.ClearByContractId(IbApiRequestType.Realtime, account, contractId);
    }

    private void RequestAccountPnL(string account) {
        var requestId = RequestManager.GetNextRequestId(IbApiRequestType.PnL, account);

        Log.Information(
            "#{RequestId}: Subscribing PnL of account {Account}",
            requestId,
            account
        );
        ClientSocket.reqPnL(requestId, account, "");
    }

    public void RequestSinglePositionPnL(string account, int contractId) {
        var requestId = RequestManager.GetNextRequestId(IbApiRequestType.PnLSingle, account, contractId);

        Log.Information(
            "#{RequestId}: Subscribing PnL of contract [{ContractId}]",
            requestId,
            contractId
        );

        ClientSocket.reqPnLSingle(
            requestId,
            account,
            "",
            contractId
        );
    }

    public void RequestCompletedOrders() {
        ClientSocket.reqCompletedOrders(false);
    }

    public void RequestOrders() {
        ClientSocket.reqOpenOrders();
        RequestCompletedOrders();
    }

    public int? RequestPxHistoryForQuote(HistoryPxRequestForQuote request) {
        var requestId = RequestManager.GetNextRequestId(
            IbApiRequestType.History,
            request.Account,
            request.ContractId
        );

        Log.Information(
            "#{RequestId}: Requesting {DataType} price of [{ContractId}] @ {Interval} in {Duration} (from Quote)",
            requestId,
            request.DataType,
            request.ContractId,
            request.Interval,
            request.Duration
        );

        return RequestPxHistory(request, requestId, useLock: false);
    }

    private int? RequestPxHistory(IHistoryPxRequest request, int requestId, bool useLock = false) {
        ClientSocket.reqHistoricalData(
            requestId,
            request.Contract,
            request.IsSubscription ? "" : DateTime.Now.ToUniversalTime().ToIbApiFormat(),
            request.Duration,
            request.BarSize.ToString(),
            request.DataType.ToString().ToUpper(),
            Convert.ToInt32(request.RthOnly),
            1, // 1 for "yyyyMMdd HH:mm:ss"; 2 for system format
            request.IsSubscription,
            null
        );

        if (useLock) {
            HistoryPxRequestManager.EnterLock(requestId);
        }

        if (request.Contract.ConId == default) {
            Log.Error("#{RequestId}: Contract ID on history Px request unavailable", requestId);
            return null;
        }

        HistoryPxRequestManager.RecordMeta(requestId, new IbApiHistoryPxRequestMeta {
            ContractId = request.Contract.ConId,
            Interval = request.BarSize.Interval,
            DataType = request.DataType,
            IsSubscription = request.IsSubscription
        });

        return requestId;
    }

    public void CancelHistory(int requestId) {
        Log.Information("#{RequestId}: Cancelling history px subscription", requestId);
        ClientSocket.cancelHistoricalData(requestId);
        RequestManager.ClearByRequestId(requestId);
    }

    public IEnumerable<ContractDetails> RequestContractDetails(Contract contract) {
        var requestId = RequestManager.GetNextRequestIdNoCancel();
        Log.Information(
            "#{RequestId}: Requesting contract details of {Symbol}",
            requestId,
            contract.Symbol
        );

        ClientSocket.reqContractDetails(requestId, contract);
        ContractDetailsManager.EnterLock(requestId);
        return ContractDetailsManager.WaitAndGetData(requestId);
    }

    public void InitOptionChain(InitOptionChainRequest request) {
        Log.Information("Requesting option chain of {Symbol}", request.Symbol);

        if (request.InUsePxRequestIds.Count > 0) {
            Log.Information(
                "To cancel market data subscription of the following requests: {@RequestIds}",
                request.InUsePxRequestIds
            );
        }

        foreach (var pxRequestId in request.InUsePxRequestIds) {
            ClientSocket.cancelMktData(pxRequestId);
        }

        if (request.InUseContractId is not null) {
            CancelRealtime(request.Account, request.InUseContractId.Value);
        }

        var options = new ToContractOptions {
            AutoConvertOptionsToStocks = true
        };
        foreach (var contractDetail in RequestContractDetails(request.Symbol.ToContract(options))) {
            var requestId = RequestManager.GetNextRequestIdNoCancel();
            var contract = contractDetail.Contract;

            // Request price quote at the same time to let UI decide what strikes to use
            RequestRealtime(request.Account, contract);

            OptionDefinitionsManager.EnterLock(requestId);

            Log.Information(
                "#{RequestId}: Requesting options param of {Symbol}",
                requestId,
                request.Symbol
            );
            ClientSocket.reqSecDefOptParams(
                requestId,
                contract.Symbol,
                contract.Exchange,
                contract.SecType,
                contract.ConId
            );
        }
    }

    private IEnumerable<int> RequestRealtimeFromContract(
        string account, Contract contract, Action<Contract> onObtainedContract
    ) {
        var requestIds = new List<int>();

        foreach (var contractDetail in RequestContractDetails(contract)) {
            var contractFromDetail = contractDetail.Contract;

            onObtainedContract(contractFromDetail);
            var realtimeRequestId = RequestRealtime(account, contractDetail.Contract);

            if (realtimeRequestId is null) {
                continue;
            }

            requestIds.Add(realtimeRequestId.Value);
        }

        return requestIds;
    }

    public OptionPxResponse RequestOptionChainPrice(OptionPxSubscribeRequest request) {
        Log.Information(
            "Received option Px subscription request of {Symbol} expiring {Expiry} at {@Strikes}",
            request.Symbol,
            request.Expiry,
            request.Strikes
        );

        var realtimeRequestIds = new List<int>();
        var callContracts = new Dictionary<double, Contract>();
        var putContracts = new Dictionary<double, Contract>();

        foreach (var strike in request.Strikes) {
            var callContract = ContractMaker.MakeOptions(
                request.Symbol,
                request.Expiry,
                OptionRight.Call,
                strike,
                request.TradingClass
            );
            realtimeRequestIds.AddRange(RequestRealtimeFromContract(
                request.Account,
                callContract,
                contract => callContracts.Add(strike, contract)
            ));

            var putContract = ContractMaker.MakeOptions(
                request.Symbol,
                request.Expiry,
                OptionRight.Put,
                strike,
                request.TradingClass
            );
            realtimeRequestIds.AddRange(RequestRealtimeFromContract(
                request.Account,
                putContract,
                contract => putContracts.Add(strike, contract)
            ));
        }

        return new OptionPxResponse {
            RealtimeRequestIds = realtimeRequestIds,
            ContractIdPairs = callContracts.Keys
                .Concat(putContracts.Keys)
                .Distinct()
                .Select(x => new OptionContractIdPair {
                    Strike = x,
                    Call = callContracts[x].ConId,
                    Put = putContracts[x].ConId
                })
                .ToList()
        };
    }
}