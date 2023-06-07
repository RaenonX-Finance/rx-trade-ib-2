using JetBrains.Annotations;

namespace Rx.IB2.Models.Messages;

public struct PositionPnlUpdateMessage {
    [UsedImplicitly]
    public required string Account { get; init; }

    [UsedImplicitly]
    public required int ContractId { get; init; }

    [UsedImplicitly]
    public required decimal Quantity { get; init; }

    [UsedImplicitly]
    public required double DailyPnl { get; init; }

    [UsedImplicitly]
    public required double UnrealizedPnl { get; init; }

    [UsedImplicitly]
    public required double RealizedPnl { get; init; }

    [UsedImplicitly]
    public required double MarketValue { get; init; }
}