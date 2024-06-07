using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Models;

namespace Rx.IB2.Extensions;

public static class PxExtensions {
    private static readonly HashSet<PxTick> PxTicksToIncludeForAll = [PxTick.Ask, PxTick.Bid, PxTick.Close];

    private static readonly HashSet<PxTick> PxTicksToIncludeForStocks = [PxTick.Last];

    private static readonly HashSet<PxTick> PxTicksToIncludeForOptions = [
        PxTick.Mark,
        PxTick.Delta,
        PxTick.Theta,
        PxTick.Gamma,
        PxTick.ModelOptionPx,
        PxTick.OptionCallOpenInterest,
        PxTick.OptionPutOpenInterest,
    ];

    private static readonly HashSet<PxTick> PxTicksToIncludeForFutures = [PxTick.Last];

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