using JetBrains.Annotations;

namespace Rx.IB2.Models.Messages;

public struct PositionUpdateMessage {
    [UsedImplicitly]
    public required string Account { get; init; }

    [UsedImplicitly]
    public required ContractModel Contract { get; init; }

    [UsedImplicitly]
    public required decimal Quantity { get; init; }

    [UsedImplicitly]
    public required double AvgPx { get; init; }

    [UsedImplicitly]
    public required double UnrealizedPnl { get; init; }

    [UsedImplicitly]
    public required double MarketValue { get; init; }
}