using IBApi;
using Rx.IB2.Enums;

namespace Rx.IB2.Interfaces;

public interface IHistoryPxRequest {
    public Contract Contract { get; }

    public HistoryDataType DataType { get; }

    public BarSize BarSize { get; }

    public bool RthOnly { get; }

    public bool IsSubscription { get; }

    public string Duration { get; }
}