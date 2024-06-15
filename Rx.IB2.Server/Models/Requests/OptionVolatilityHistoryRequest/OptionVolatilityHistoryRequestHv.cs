using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Interfaces;

namespace Rx.IB2.Models.Requests.OptionVolatilityHistoryRequest;

public readonly struct OptionVolatilityHistoryRequestHv : IHistoryPxRequest {
    public required Contract Contract { get; init; }

    public HistoryDataType DataType => HistoryDataType.HistoricalVolatility;

    public BarSize BarSize => BarSize.Day;

    public bool RthOnly => true;

    public bool IsSubscription => false;

    public string Duration => "1 Y";
}