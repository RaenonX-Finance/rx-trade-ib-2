using JetBrains.Annotations;

namespace Rx.IB2.Models.Messages;

public record ChartDataBar {
    [UsedImplicitly]
    public required long EpochSec { get; init; }
    
    [UsedImplicitly]
    public required double? Open { get; init; }

    [UsedImplicitly]
    public required double? High { get; init; }

    [UsedImplicitly]
    public required double? Low { get; init; }

    [UsedImplicitly]
    public required double? Close { get; init; }

    [UsedImplicitly]
    public double? Diff => Close - Open;
}