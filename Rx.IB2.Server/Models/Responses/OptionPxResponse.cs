﻿using JetBrains.Annotations;

namespace Rx.IB2.Models.Responses;

public struct OptionContractIdPair {
    [UsedImplicitly]
    public required double Strike { get; init; }

    [UsedImplicitly]
    public required int Call { get; init; }

    [UsedImplicitly]
    public required int Put { get; init; }
}

public struct OptionPxResponse {
    [UsedImplicitly]
    public required List<int> RealtimeRequestIds { get; init; }

    [UsedImplicitly]
    public required List<OptionContractIdPair> ContractIdPairs { get; init; }
}