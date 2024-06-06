﻿using JetBrains.Annotations;

namespace Rx.IB2.Models.Requests; 

public record CancelApiRequest {
    [UsedImplicitly]
    public required int RequestId { get; init; }
}