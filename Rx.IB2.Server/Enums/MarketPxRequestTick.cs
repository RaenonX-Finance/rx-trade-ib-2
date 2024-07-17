namespace Rx.IB2.Enums; 

// ReSharper disable UnusedMember.Global
/// <summary>
/// Generic tick to send in the realtime data request for obtaining the corresponding data.
/// </summary>
public enum MarketPxRequestTick {
    OptionVolume = 100,
    OptionOpenInterest = 101,
    StockStats = 165,
    Mark = 221,
    RtVolume = 233  // Last Trade Px/Size, Volume, VWAP
}
// ReSharper restore UnusedMember.Global