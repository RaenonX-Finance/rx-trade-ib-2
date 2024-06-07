namespace Rx.IB2.Enums;

/// <summary>
/// Check `TickType.getField()` for the field description.
/// Related online doc:
/// - https://interactivebrokers.github.io/tws-api/tick_types.html
/// 
/// Note, some tick may require generic tick to get the tick. Corresponding enum is <see cref="MarketPxRequestTick"/>.
/// </summary>
// ReSharper disable UnusedMember.Global
public enum PxTick {
    BidSize = 0,
    Bid = 1,
    Ask = 2,
    AskSize = 3,
    Last = 4,
    LastSize = 5,
    High = 6,
    Low = 7,
    Volume = 8,
    Close = 9,
    BidOfOptions = 10,
    AskOfOptions = 11,
    LastOfOptions = 12,
    ModelOptionPx = 13,
    OpenTick = 14, // Current session opening price, previous day open before open
    OptionCallOpenInterest = 27,
    OptionPutOpenInterest = 28,
    OptionCallVolume = 29,
    OptionPutVolume = 30,
    BidExchange = 32,
    AskExchange = 33,
    Mark = 37,
    LastEpochSec = 45,
    RtVolume = 48,
    Halted = 49,
    // Any ticks with value of 10000+ is custom tick to be used on UI
    Delta = 10001,
    Gamma = 10002,
    Vega = 10003,
    Theta = 10004,
    OptionsUnderlyingPx = 10005,
    PvDividend = 10006, // Present value of dividends expected on the underlying
    ImpliedVolatility = 10007
}
// ReSharper restore UnusedMember.Global