using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;

namespace Rx.IB2.Services.IbApiHandlers;

public partial class IbApiHandler {
    private Contract? OnTickReceived(int requestId, PxTick tick, bool warnOnContractNotFound = false) {
        var requestIdToCancel = OneTimePxRequestManager.RecordReceivedSingle(requestId, tick);

        var contract = RequestManager.GetContractByRequestId(requestId);
        if (contract is null) {
            if (warnOnContractNotFound) {
                Log.Warning(
                    "#{RequestId}: Received price tick {Tick} but no corresponding contract found",
                    requestId,
                    tick
                );
            }

            return null;
        }

        if (!tick.IsPxTickIncluded(contract)) {
            return null;
        }

        if (requestIdToCancel is null) {
            return contract;
        }

        Log.Information(
            "#{RequestId}: All target ticks have been received, cancelling market data subscription ({Active} active left - {@ActiveRequestIds})",
            requestIdToCancel,
            OneTimePxRequestManager.ActiveRequests.Count,
            OneTimePxRequestManager.ActiveRequests
        );
        CancelMarketData(requestId);

        return contract;
    }

    public void tickPrice(int requestId, int pxTickInt, double price, TickAttrib attribs) {
        if (price <= 0) {
            // Field unavailable
            return;
        }

        var tick = pxTickInt.ToPxTick();
        var contract = OnTickReceived(requestId, tick, warnOnContractNotFound: true);
        if (contract is null) {
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

        var tick = pxTickInt.ToPxTick();
        var contract = OnTickReceived(requestId, tick, warnOnContractNotFound: true);
        if (contract is null) {
            return;
        }

        var actualSizeOfDouble = decimal.ToDouble(actualSize.Value);

        Hub.SendPxUpdate(
            requestId,
            contract.ConId,
            tick,
            tick == PxTick.AverageVolume ? actualSizeOfDouble * 100 : actualSizeOfDouble
        );
    }

    public void tickString(int requestId, int pxTickInt, string value) {
        if (value == "") {
            // Field unavailable
            return;
        }

        var tick = pxTickInt.ToPxTick();
        var contract = OnTickReceived(requestId, tick, warnOnContractNotFound: false);
        if (contract is null) {
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

        var tick = pxTickInt.ToPxTick();
        var contract = OnTickReceived(requestId, tick, warnOnContractNotFound: false);
        if (contract is null) {
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
        var tick = pxTickInt.ToPxTick();
        var contract = OnTickReceived(requestId, tick, warnOnContractNotFound: false);
        if (contract is null) {
            return;
        }

        optPrice = optPrice.MaxValueAsZero();
        delta = delta.MaxValueAsZero();

        if (optPrice <= 0) {
            return;
        }

        var requestIdToCancel = OneTimePxRequestManager.RecordReceivedMultiple(requestId, new HashSet<PxTick> {
            PxTick.Delta,
            PxTick.Gamma,
            PxTick.Theta,
            PxTick.Vega,
            PxTick.OptionsUnderlyingPx,
            PxTick.PvDividend,
            PxTick.ImpliedVolatility
        });

        if (requestIdToCancel is not null) {
            Log.Information(
                "#{RequestId}: All target ticks have been received, cancelling market data subscription ({Active} active left - {@ActiveRequestIds})",
                requestIdToCancel,
                OneTimePxRequestManager.ActiveRequests.Count,
                OneTimePxRequestManager.ActiveRequests
            );
            CancelMarketData(requestId);
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
                { PxTick.Gamma, gamma },
                { PxTick.Theta, theta },
                { PxTick.Vega, vega },
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