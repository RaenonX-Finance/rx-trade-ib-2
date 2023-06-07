using JetBrains.Annotations;

namespace Rx.IB2.Models.Messages;

public record PxHistoryMessage {
    [UsedImplicitly]
    public required IbApiHistoryPxRequestMeta Meta { get; init; }

    [UsedImplicitly]
    public required IEnumerable<ChartDataBar> Bars { get; init; }
}