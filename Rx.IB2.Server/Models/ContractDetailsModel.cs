using JetBrains.Annotations;

namespace Rx.IB2.Models;

public record ContractDetailsModel {
    [UsedImplicitly]
    public required double MinTick { get; init; }
}