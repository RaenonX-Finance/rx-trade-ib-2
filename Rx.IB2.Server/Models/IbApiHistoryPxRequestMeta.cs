using JetBrains.Annotations;
using Rx.IB2.Enums;

namespace Rx.IB2.Models;

public record IbApiHistoryPxRequestMeta {
    [UsedImplicitly]
    public required int ContractId { get; init; }

    [UsedImplicitly]
    public required string Interval { get; init; }

    [UsedImplicitly]
    public required HistoryDataType DataType { get; init; }

    [UsedImplicitly]
    public required bool IsSubscription { get; init; }

    public double ValueMultiplier =>
        DataType switch {
            HistoryDataType.Midpoint or HistoryDataType.Trades => 1,
            HistoryDataType.OptionImpliedVolatility or HistoryDataType.HistoricalVolatility => 100,
            _ => throw new ArgumentOutOfRangeException(
                nameof(DataType),
                $"Invalid history data type for getting the value multiplier: {DataType}"
            )
        };
}