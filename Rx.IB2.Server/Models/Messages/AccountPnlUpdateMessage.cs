using JetBrains.Annotations;

namespace Rx.IB2.Models.Messages;

public struct AccountPnlUpdateMessage {
    [UsedImplicitly]
    public required string Account { get; init; }

    [UsedImplicitly]
    public required double DailyPnl { get; init; }

    [UsedImplicitly]
    public required double UnrealizedPnl { get; init; }

    [UsedImplicitly]
    public required double RealizedPnl { get; init; }
}