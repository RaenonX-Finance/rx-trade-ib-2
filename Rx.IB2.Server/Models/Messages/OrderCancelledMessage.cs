using JetBrains.Annotations;

namespace Rx.IB2.Models.Messages;

public struct OrderCancelledMessage {
    [UsedImplicitly]
    public required string Account { get; init; }

    [UsedImplicitly]
    public required int OrderId { get; init; }
}