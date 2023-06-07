namespace Rx.IB2.Models.Requests; 

public record CancelApiRequest {
    public required int RequestId { get; init; }
}