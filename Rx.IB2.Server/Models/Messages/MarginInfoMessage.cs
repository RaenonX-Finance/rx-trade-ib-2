using JetBrains.Annotations;

namespace Rx.IB2.Models.Messages;

public struct MarginInfoMessage {
    [UsedImplicitly]
    public required int ContractId { get; init; }

    [UsedImplicitly]
    public required decimal InitialMargin { get; init; }

    [UsedImplicitly]
    public required decimal MaintenanceMargin { get; init; }
}