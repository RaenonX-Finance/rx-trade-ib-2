using JetBrains.Annotations;
using Rx.IB2.Enums;

namespace Rx.IB2.Models.Messages;

public struct AccountDataUpdateMessage {
    [UsedImplicitly]
    public required string Account { get; init; }

    [UsedImplicitly]
    public required string Currency { get; init; }

    [UsedImplicitly]
    public required AccountSummaryKey Key { get; init; }

    [UsedImplicitly]
    public required string Value { get; init; }
}