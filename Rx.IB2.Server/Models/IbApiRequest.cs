using Rx.IB2.Enums;

namespace Rx.IB2.Models;

public struct IbApiRequest {
    public required IbApiRequestType Type { get; init; }

    public required int Id { get; init; }
}