using JetBrains.Annotations;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;

namespace Rx.IB2.Models;

public record ContractModel {
    [UsedImplicitly]
    public required int Id { get; init; }

    [UsedImplicitly]
    public required SecurityType SecurityType { get; init; }

    [UsedImplicitly]
    public required string LocalSymbol { get; init; }

    [UsedImplicitly]
    public required string Exchange { get; init; }

    [UsedImplicitly]
    public required decimal Multiplier { get; init; }

    [UsedImplicitly]
    public required ContractDetailsModel? Details { get; init; }

    public string ExchangeForOptionDefinitionsQuery => SecurityType == SecurityType.Stocks ? "" : Exchange;

    public string SecTypeForOptionDefinitionsQuery =>
        SecurityType == SecurityType.ContinuousFutures
            ? SecurityType.Futures.ToIbApiSecurityType()
            : SecurityType.ToIbApiSecurityType();
}