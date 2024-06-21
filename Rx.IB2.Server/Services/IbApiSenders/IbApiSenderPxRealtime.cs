using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;

namespace Rx.IB2.Services.IbApiSenders;

public partial class IbApiSender {
    public void CancelRealtime(string account, int contractId) {
        Log.Information("Received request to cancel market data of contract [{ContractId}]", contractId);

        var requestIds = RequestManager.GetRequestIdsByContractIdAndType(
            IbApiRequestType.Realtime,
            account,
            contractId
        );
        foreach (var requestId in requestIds) {
            Log.Information("#{RequestId}: Cancelling realtime data of [{ContractId}]", requestId, contractId);
            CancelRealtime(requestId);
        }

        RequestManager.ClearByContractId(IbApiRequestType.Realtime, account, contractId);
    }

    public void CancelRealtime(int requestId) {
        Log.Information("#{RequestId}: Cancelling market data of the request", requestId);
        ClientSocket.cancelMktData(requestId);
    }

    private int? SubscribeRealtimeFrozen(string account, ContractDetails contractDetails) {
        var securityType = contractDetails.Contract.SecType.ToSecurityType();
        if (
            securityType != SecurityType.Options ||
            contractDetails.ToTradingDateIntervals().Any(x => x.IsCurrent())
        ) {
            return null;
        }

        return SubscribeRealtime(account, contractDetails.Contract, MarketDataType.Frozen);
    }

    private int? SubscribeRealtime(string account, ContractDetails contractDetails) {
        return SubscribeRealtime(account, contractDetails.Contract, MarketDataType.Live);
    }

    private int? SubscribeRealtime(
        string account,
        Contract contract,
        MarketDataType marketDataType,
        Action<int>? onBeforeSubscriptionRequest
    ) {
        var isSubscribing = RequestManager.IsContractSubscribingRealtime(account, contract);
        if (isSubscribing && marketDataType != MarketDataType.Frozen) {
            Log.Information(
                "Contract [{ContractId}] already subscribed to market data, not re-subscribing",
                contract.ConId
            );
            return null;
        }

        var securityType = contract.SecType.ToSecurityType();
        var requestId = RequestManager.GetNextRequestId(IbApiRequestType.Realtime, account, contract.ConId);
        var tickToRequest = new List<MarketPxRequestTick> { MarketPxRequestTick.Mark };

        if (securityType == SecurityType.Options) {
            tickToRequest.Add(MarketPxRequestTick.OptionOpenInterest);
            tickToRequest.Add(MarketPxRequestTick.OptionVolume);
        }

        Log.Information(
            "#{RequestId}: Subscribing realtime data of {CustomContractSymbol} ({MarketDataType})",
            requestId,
            contract.ToCustomContractSymbol(),
            marketDataType
        );

        // Make sure `contract` has exchange set
        contract.AddExchangeOnContract();

        onBeforeSubscriptionRequest?.Invoke(requestId);

        ClientSocket.reqMarketDataType((int)marketDataType);
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

    public int? SubscribeRealtime(string account, Contract contract, MarketDataType marketDataType) {
        return SubscribeRealtime(account, contract, marketDataType, null);
    }

    private void RequestRealtime(
        string account,
        ContractDetails contractDetails,
        MarketDataType marketDataType,
        IEnumerable<PxTick> targetTicks
    ) {
        SubscribeRealtime(
            account,
            contractDetails.Contract,
            marketDataType,
            (requestId) => OneTimePxRequestManager.RecordTarget(requestId, targetTicks)
        );
    }

    private void RequestRealtimeFrozen(string account, ContractDetails contractDetails, IEnumerable<PxTick> pxTicks) {
        var securityType = contractDetails.Contract.SecType.ToSecurityType();
        if (
            securityType != SecurityType.Options ||
            contractDetails.ToTradingDateIntervals().Any(x => x.IsCurrent())
        ) {
            return;
        }

        RequestRealtime(account, contractDetails, MarketDataType.Frozen, pxTicks);
    }

    private List<int> SubscribeRealtimeFromContract(
        string account,
        Contract contract,
        Action<Contract> onObtainedContract
    ) {
        var requestIds = new List<int>();

        foreach (var contractDetail in RequestContractDetails(contract)) {
            var contractFromDetail = contractDetail.Contract;

            onObtainedContract(contractFromDetail);
            var requestId = SubscribeRealtime(account, contractDetail);
            if (requestId is null) {
                continue;
            }

            requestIds.Add(requestId.Value);
        }

        return requestIds;
    }

    private List<int> SubscribeFrozenRealtimeFromContract(
        string account,
        Contract contract
    ) {
        return RequestContractDetails(contract)
            .Select(contractDetail => SubscribeRealtimeFrozen(account, contractDetail))
            .Where(requestId => requestId is not null)
            .OfType<int>()
            .ToList();
    }

    private void RequestRealtimeFromContract(
        string account,
        Contract contract,
        Action<Contract> onObtainedContract,
        ICollection<PxTick> targetTicks
    ) {
        foreach (var contractDetail in RequestContractDetails(contract)) {
            var contractFromDetail = contractDetail.Contract;

            onObtainedContract(contractFromDetail);
            RequestRealtime(account, contractDetail, MarketDataType.Live, targetTicks);
        }
    }

    private void RequestFrozenRealtimeFromContract(
        string account,
        Contract contract,
        ICollection<PxTick> targetTicks
    ) {
        foreach (var contractDetail in RequestContractDetails(contract)) {
            RequestRealtimeFrozen(account, contractDetail, targetTicks);
        }
    }
}