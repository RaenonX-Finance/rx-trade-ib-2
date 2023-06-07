using IBApi;
using JetBrains.Annotations;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;
using Rx.IB2.Interfaces;

namespace Rx.IB2.Models.Requests;

public readonly struct HistoryPxRequestFromApi : IHistoryPxRequest {
    [UsedImplicitly]
    public required SecurityType SecurityType { get; init; }

    [UsedImplicitly]
    public required string Symbol { get; init; }

    [UsedImplicitly]
    public required HistoryDataType DataType { get; init; }

    [UsedImplicitly]
    public required BarSize BarSize { get; init; }

    [UsedImplicitly]
    public required int DurationValue { get; init; }

    [UsedImplicitly]
    public required DurationUnit DurationUnit { get; init; }

    [UsedImplicitly]
    public required bool RthOnly { get; init; }

    [UsedImplicitly]
    public required bool IsSubscription { get; init; }

    public Contract Contract {
        get {
            return SecurityType switch {
                SecurityType.Stocks => Symbol.ToUsStockContract(),
                SecurityType.Futures => Symbol.ToFuturesContract(),
                SecurityType.Options => Symbol.ToOptionsContractFromOcc(),
                SecurityType.ContinuousFutures => Symbol.ToContinuousFuturesContract(),
                _ => throw new ArgumentOutOfRangeException(nameof(SecurityType),
                    $"Unhandled contract conversion for security type of {SecurityType}")
            };
        }
    }

    public string Duration => $"{DurationValue} {DurationUnit.ToString()}";
}