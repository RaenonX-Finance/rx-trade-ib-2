﻿using JetBrains.Annotations;

namespace Rx.IB2.Models.Requests;

public readonly struct PnlRequest {
    [UsedImplicitly]
    public required string Account { get; init; }

    [UsedImplicitly]
    public required int ContractId { get; init; }
}