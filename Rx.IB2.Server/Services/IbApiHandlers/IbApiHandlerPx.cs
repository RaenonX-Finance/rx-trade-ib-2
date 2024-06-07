using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;

namespace Rx.IB2.Services.IbApiHandlers;

public partial class IbApiHandler {
    public void tickPrice(int requestId, int pxTickInt, double price, TickAttrib attribs) {
        if (price <= 0) {
            // Field unavailable
            return;
        }

        var contract = RequestManager.GetContractByRequestId(requestId);
        if (contract is null) {
            Log.Warning(
                "#{RequestId}: Received price tick but no corresponding contract found",
                requestId
            );
            return;
        }

        var tick = pxTickInt.ToPxTick();
        if (!tick.IsPxTickIncluded(contract)) {
            return;
        }

        Hub.SendPxUpdate(requestId, contract.ConId, tick, price);
    }

    public void tickSize(int requestId, int pxTickInt, decimal size) {
        var actualSize = size.MaxValueAsNull();
        if (actualSize is null) {
            // Field unavailable
            return;
        }

        var contract = RequestManager.GetContractByRequestId(requestId);
        var tick = pxTickInt.ToPxTick();
        if (contract is null || !tick.IsPxTickIncluded(contract)) {
            return;
        }

        Hub.SendPxUpdate(
            requestId,
            contract.ConId,
            tick,
            decimal.ToDouble(actualSize.Value)
        );
    }

    public void tickString(int requestId, int pxTickInt, string value) {
        if (value == "") {
            // Field unavailable
            return;
        }

        var contract = RequestManager.GetContractByRequestId(requestId);
        var tick = pxTickInt.ToPxTick();
        if (contract is null || !tick.IsPxTickIncluded(contract)) {
            return;
        }

        Log.Debug(
            "#{RequestId}: Received tick type {TickTypeInt} of string {Value}",
            requestId,
            tick,
            value
        );
    }

    public void tickGeneric(int requestId, int pxTickInt, double value) {
        if (value < 0) {
            // Field unavailable
            return;
        }

        var contract = RequestManager.GetContractByRequestId(requestId);
        var tick = pxTickInt.ToPxTick();
        if (contract is null || !tick.IsPxTickIncluded(contract)) {
            return;
        }

        Log.Debug(
            "#{RequestId}: Received tick type {TickTypeInt} of generic {Value}",
            requestId,
            tick,
            value
        );
    }

    public void tickOptionComputation(
        int requestId,
        int pxTickInt,
        int tickAttrib,
        double impliedVolatility,
        double delta,
        double optPrice,
        double pvDividend,
        double gamma,
        double vega,
        double theta,
        double underlyingPx
    ) {
        var contract = RequestManager.GetContractByRequestId(requestId);

        if (contract is null) {
            Log.Warning(
                "#{RequestId}: Received option compute tick but no corresponding contract found",
                requestId
            );
            return;
        }

        var tick = pxTickInt.ToPxTick();
        if (!tick.IsPxTickIncluded(contract)) {
            return;
        }

        optPrice = optPrice.MaxValueAsZero();
        delta = delta.MaxValueAsZero();

        if (optPrice <= 0) {
            return;
        }

        // Tick here usually is either one of Bid/Ask/Last/Model
        // Bid/Ask/Last here shouldn't be used to report because they are sent after greeks computation,
        // which means it will lag behind
        Hub.SendPxUpdate(
            requestId,
            contract.ConId,
            new Dictionary<PxTick, double> {
                { tick, optPrice },
                { PxTick.Delta, delta },
                { PxTick.Vega, vega },
                { PxTick.Gamma, gamma },
                { PxTick.Theta, theta },
                { PxTick.OptionsUnderlyingPx, underlyingPx },
                { PxTick.PvDividend, pvDividend },
                { PxTick.ImpliedVolatility, impliedVolatility }
            }
        );
    }

    public void historicalData(int requestId, Bar bar) {
        HistoryPxRequestManager.AddBar(requestId, bar);
    }

    public void historicalDataUpdate(int requestId, Bar bar) {
        var meta = HistoryPxRequestManager.GetMeta(requestId);

        if (meta is null) {
            Log.Warning("#{RequestId}: History Px does not have associated metadata", requestId);
            return;
        }

        Hub.SendPxHistory(requestId, false, new[] { bar.ToPxDataBarModel() }, meta);
    }

    public void historicalDataEnd(int requestId, string start, string end) {
        Log.Information(
            "#{RequestId}: End of history Px request from {Start} to {End}",
            requestId,
            start,
            end
        );

        if (HistoryPxRequestManager.ReleaseLock(requestId)) {
            Log.Information("#{RequestId}: History Px lock released", requestId);
        }

        if (!HistoryPxRequestManager.IsSubscription(requestId)) {
            return;
        }

        var meta = HistoryPxRequestManager.GetMeta(requestId);

        if (meta is null) {
            Log.Warning("#{RequestId}: History Px does not have associated metadata", requestId);
            return;
        }

        Hub.SendPxHistory(requestId, true, HistoryPxRequestManager.GetData(requestId), meta);
    }

    public void marketDataType(int requestId, int marketDataType) {
        Log.Information(
            "#{RequestId}: Market data type is {MarketDataType}",
            requestId,
            (MarketDataType)marketDataType
        );
    }
}