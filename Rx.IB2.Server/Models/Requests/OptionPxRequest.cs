using JetBrains.Annotations;
using Rx.IB2.Enums;

namespace Rx.IB2.Models.Requests;

public struct OptionPxRequest {
    [UsedImplicitly]
    public required OptionPxRequestOrigin Origin { get; init; }

    [UsedImplicitly]
    public required OptionPxRequestType Type { get; init; }

    [UsedImplicitly]
    public required string Account { get; init; }

    [UsedImplicitly]
    public required string Symbol { get; init; }

    [UsedImplicitly]
    public required HashSet<string> Expiry { get; init; }

    [UsedImplicitly]
    public required string TradingClass { get; init; }

    [UsedImplicitly]
    public required HashSet<double> Strikes { get; init; }
}