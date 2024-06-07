using JetBrains.Annotations;
using Rx.IB2.Enums;

namespace Rx.IB2.Models.Responses;

public struct OptionContractIdPair {
    [UsedImplicitly]
    public required string Expiry { get; init; }

    [UsedImplicitly]
    public required double Strike { get; init; }

    [UsedImplicitly]
    public required int Call { get; init; }

    [UsedImplicitly]
    public required int Put { get; init; }
}

public struct OptionPxResponse {
    [UsedImplicitly]
    public required OptionPxRequestOrigin Origin { get; init; }

    [UsedImplicitly]
    public required List<int> RealtimeRequestIds { get; init; }

    [UsedImplicitly]
    public required List<OptionContractIdPair> ContractIdPairs { get; init; }
}