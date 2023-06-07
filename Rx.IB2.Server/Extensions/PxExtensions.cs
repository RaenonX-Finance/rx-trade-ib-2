using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Models;

namespace Rx.IB2.Extensions;

public static class PxExtensions {
    private static readonly ISet<PxTick> PxTicksToIncludeForAll = new HashSet<PxTick> {
        PxTick.Close
    };

    private static readonly ISet<PxTick> PxTicksToIncludeForStocks = new HashSet<PxTick> {
        PxTick.Ask, PxTick.Bid, PxTick.Last
    };

    private static readonly ISet<PxTick> PxTicksToIncludeForOptions = new HashSet<PxTick> {
        PxTick.Ask, PxTick.Bid, PxTick.Mark, PxTick.Delta, PxTick.Theta
    };

    private static readonly ISet<PxTick> PxTicksToIncludeForFutures = new HashSet<PxTick> {
        PxTick.Last
    };

    public static bool IsPxTickIncluded(this PxTick pxTick, Contract contract) {
        if (PxTicksToIncludeForAll.Contains(pxTick)) {
            return true;
        }

        return contract.SecType.ToSecurityType() switch {
            SecurityType.Stocks => PxTicksToIncludeForStocks.Contains(pxTick),
            SecurityType.Futures => PxTicksToIncludeForFutures.Contains(pxTick),
            SecurityType.ContinuousFutures => PxTicksToIncludeForFutures.Contains(pxTick),
            SecurityType.Options => PxTicksToIncludeForOptions.Contains(pxTick),
            SecurityType.OptionsCombo => PxTicksToIncludeForOptions.Contains(pxTick),
            _ => false
        };
    }

    public static PxDataBarModel ToPxDataBarModel(this Bar bar) {
        return new PxDataBarModel {
            Open = bar.Open,
            High = bar.High,
            Low = bar.Low,
            Close = bar.Close,
            Timestamp = bar.Time.FromHistoricalPxTimestampToUtc()
        };
    }
}