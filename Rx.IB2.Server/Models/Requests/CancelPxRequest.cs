namespace Rx.IB2.Models.Requests;

public record CancelPxRequest {
    public required string Account { get; init; }

    public required int ContractId { get; init; }
}