using JetBrains.Annotations;

namespace Rx.IB2.Models;

public struct PxDataBarModel {
    [UsedImplicitly]
    public required double Open { get; init; }

    [UsedImplicitly]
    public required double High { get; init; }

    [UsedImplicitly]
    public required double Low { get; init; }

    [UsedImplicitly]
    public required double Close { get; init; }

    [UsedImplicitly]
    public required DateTime Timestamp { get; init; }
}