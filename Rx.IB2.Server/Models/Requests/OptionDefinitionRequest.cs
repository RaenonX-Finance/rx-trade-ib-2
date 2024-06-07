﻿using JetBrains.Annotations;
using Rx.IB2.Enums;

namespace Rx.IB2.Models.Requests;

public struct OptionDefinitionRequest {
    [UsedImplicitly]
    public required OptionPxRequestOrigin Origin { get; init; }

    [UsedImplicitly]
    public required string Account { get; init; }

    [UsedImplicitly]
    public required string Symbol { get; init; }

    [UsedImplicitly]
    public required int? InUseContractId { get; init; }

    [UsedImplicitly]
    public required List<int> InUsePxRequestIds { get; init; }
}