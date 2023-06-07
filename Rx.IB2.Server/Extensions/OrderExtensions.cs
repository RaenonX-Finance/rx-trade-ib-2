using IBApi;

namespace Rx.IB2.Extensions;

public static class OrderExtensions {
    public static string ToOrderInfo(this Order order) =>
        $"{order.Account} {order.Action} {order.OrderType} @ {order.LmtPrice} x {order.TotalQuantity} ({order.Tif})";
}