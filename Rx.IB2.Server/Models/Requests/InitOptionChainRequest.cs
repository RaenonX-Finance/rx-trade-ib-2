using JetBrains.Annotations;

namespace Rx.IB2.Models.Requests;

public struct InitOptionChainRequest {
    [UsedImplicitly]
    public required string Account { get; init; }

    [UsedImplicitly]
    public required string Symbol { get; init; }

    [UsedImplicitly]
    public required int? InUseContractId { get; init; }

    [UsedImplicitly]
    public required List<int> InUsePxRequestIds { get; init; }
}