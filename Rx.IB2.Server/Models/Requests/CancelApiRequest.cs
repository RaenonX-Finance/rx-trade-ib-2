using JetBrains.Annotations;

namespace Rx.IB2.Models.Requests; 

public readonly struct CancelApiRequest {
    [UsedImplicitly]
    public required int RequestId { get; init; }
}