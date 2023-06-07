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
}