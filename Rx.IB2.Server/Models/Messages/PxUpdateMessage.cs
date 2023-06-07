using JetBrains.Annotations;
using Rx.IB2.Enums;

namespace Rx.IB2.Models.Messages;

public struct PxUpdateMessage {
    [UsedImplicitly]
    public required int ContractId { get; init; }

    [UsedImplicitly]
    public required Dictionary<PxTick, double> Update { get; init; }
}