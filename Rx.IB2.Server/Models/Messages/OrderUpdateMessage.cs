using JetBrains.Annotations;

namespace Rx.IB2.Models.Messages;

public struct OrderUpdateMessage {
    [UsedImplicitly]
    public required string Account { get; init; }

    [UsedImplicitly]
    public required int OrderId { get; init; }

    [UsedImplicitly]
    public required decimal FilledQuantity { get; init; }

    [UsedImplicitly]
    public required decimal TargetQuantity { get; init; }
}