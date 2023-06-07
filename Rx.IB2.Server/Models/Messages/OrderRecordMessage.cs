using JetBrains.Annotations;
using Rx.IB2.Enums;

namespace Rx.IB2.Models.Messages;

public struct OrderRecordMessage {
    [UsedImplicitly]
    public required string Account { get; init; }

    [UsedImplicitly]
    public required int OrderId { get; init; }

    [UsedImplicitly]
    public required ContractModel Contract { get; init; }

    [UsedImplicitly]
    public required OrderSide Side { get; init; }

    [UsedImplicitly]
    public required string Tif { get; init; }

    [UsedImplicitly]
    public required string Type { get; init; }

    [UsedImplicitly]
    public required double Price { get; init; }

    [UsedImplicitly]
    public required decimal FilledQuantity { get; init; }

    [UsedImplicitly]
    public required decimal TargetQuantity { get; init; }
}