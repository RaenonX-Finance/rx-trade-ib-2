using JetBrains.Annotations;

namespace Rx.IB2.Models.Messages;

public struct OptionDefinitionMessage {
    [UsedImplicitly]
    public required HashSet<string> TradingClass { get; init; }

    [UsedImplicitly]
    public required int UnderlyingContractId { get; init; }

    [UsedImplicitly]
    public required HashSet<string> Exchange { get; init; }

    [UsedImplicitly]
    public required HashSet<string> Expiry { get; init; }

    [UsedImplicitly]
    public required HashSet<double> Strike { get; init; }
}