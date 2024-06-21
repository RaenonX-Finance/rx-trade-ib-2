using JetBrains.Annotations;

namespace Rx.IB2.Models.Requests;

public readonly struct CancelRealtimeRequest {
    [UsedImplicitly]
    public required int[] RequestIds { get; init; }
}