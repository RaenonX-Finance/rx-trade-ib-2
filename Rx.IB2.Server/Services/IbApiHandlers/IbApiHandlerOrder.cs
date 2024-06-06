using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;

namespace Rx.IB2.Services.IbApiHandlers;

public partial class IbApiHandler {
    public void openOrder(int _, Contract contract, Order order, OrderState orderState) {
        RequestManager.AddOrderToPoolByPermId(order.PermId, order);
        RequestManager.AddContractToPool(contract);

        if (order.WhatIf) {
            Hub.SendMarginInfo(
                contract.ConId,
                Math.Abs(decimal.Parse(orderState.InitMarginChange)),
                Math.Abs(decimal.Parse(orderState.MaintMarginChange))
            );
            return;
        }

        Hub.SendOpenOrderRecord(order.PermId, contract, order);
    }

    public void orderStatus(
        int orderId, string status, decimal filled, decimal remaining, double avgFillPrice,
        int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice
    ) {
        var order = RequestManager.GetOrderByPermId(permId);

        if (order is null) {
            Log.Warning("Order #{OrderId}: No associated order info found while updating order status", permId);
            return;
        }

        var orderStatus = status.ToOrderStatus();

        switch (orderStatus) {
            case OrderStatus.Filled:
                Hub.SendOrderFilled(order);
                break;
            case OrderStatus.Cancelled:
                Hub.SendOrderCancelled(order);
                break;
            default:
                Hub.SendOrderUpdate(permId, order.Account, filled, remaining);
                break;
        }
    }

    public void openOrderEnd() {
        Log.Information("End of open order retrieval");
    }

    public void orderBound(long orderId, int apiClientId, int apiOrderId) {
        Log.Information(
            "Order #{OrderId}: Bound to {ApiClientId} / API Order# {ApiOrderId}",
            orderId,
            apiClientId,
            apiOrderId
        );
    }

    public void completedOrder(Contract contract, Order order, OrderState orderState) {
        RequestManager.AddOrderToPoolByPermId(order.PermId, order);
        RequestManager.AddContractToPool(contract);

        if (contract.ComboLegs is not null) {
            // FIXME: Handle option combo position/order
            foreach (var comboLeg in contract.ComboLegs) {
                ClientSocket.RequestContractDetails(RequestManager, new Contract{ConId = comboLeg.ConId});
            }
        }

        Hub.SendCompletedOrderRecord(order.PermId, contract, order);
    }

    public void completedOrdersEnd() {
        Log.Information("Done fetching completed orders");
    }

    public void execDetails(int requestId, Contract contract, Execution execution) {
        Log.Information(
            "#{RequestId}: Received execution details for order #{OrderId} - {LocalSymbol} @ {Price} x {Quantity}",
            requestId,
            execution.OrderId,
            contract.LocalSymbol,
            execution.Price,
            execution.CumQty
        );

        // Refresh completed orders on any order executed
        ClientSocket.reqCompletedOrders(false);
    }

    public void execDetailsEnd(int requestId) {
        Log.Information("#{RequestId}: End of execution details", requestId);
    }

    public void commissionReport(CommissionReport commissionReport) {
        Log.Information(
            "Execution #{ExecutionId}: Realized {RealizedPnL} with commission of {CommissionCurrency} {Commission}",
            commissionReport.ExecId,
            commissionReport.RealizedPNL.MaxValueAsZero(),
            commissionReport.Currency,
            commissionReport.Commission
        );
    }
}