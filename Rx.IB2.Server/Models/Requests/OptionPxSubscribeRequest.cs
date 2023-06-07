using JetBrains.Annotations;

namespace Rx.IB2.Models.Requests;

public struct OptionPxSubscribeRequest {
    [UsedImplicitly]
    public required string Account { get; init; }

    [UsedImplicitly]
    public required string Symbol { get; init; }

    [UsedImplicitly]
    public required string Expiry { get; init; }

    [UsedImplicitly]
    public required string TradingClass { get; init; }

    [UsedImplicitly]
    public required HashSet<double> Strikes { get; init; }
}