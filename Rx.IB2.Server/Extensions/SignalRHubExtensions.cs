using IBApi;
using Microsoft.AspNetCore.SignalR;
using Rx.IB2.Enums;
using Rx.IB2.Hubs;
using Rx.IB2.Models;
using Rx.IB2.Models.Messages;
using ILogger = Serilog.ILogger;

namespace Rx.IB2.Extensions;

public static class SignalRHubExtensions {
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(SignalRHubExtensions));

    public static void SendAccountList(this IHubContext<SignalRHub> hub, IEnumerable<string> accountList) {
        Log.Information("SignalR sends account list");
        hub.Clients.All.SendAsync(
            SignalREvents.AccountList.ToString(),
            new AccountListMessage {
                Accounts = accountList
            }
        );
    }

    public static void SendAccountSummary(
        this IHubContext<SignalRHub> hub, AccountSummaryKey summaryKey,
        string account, string currency, string value
    ) {
        Log.Information(
            "SignalR sends account summary of {Account} - {Key}: {Value}",
            account,
            summaryKey,
            value
        );
        hub.Clients.All.SendAsync(
            SignalREvents.AccountUpdate.ToString(),
            new AccountDataUpdateMessage {
                Account = account,
                Currency = currency,
                Key = summaryKey,
                Value = value
            }
        );
    }

    public static void SendAccountPnLUpdate(
        this IHubContext<SignalRHub> hub, int requestId,
        string account, double dailyPnl, double unrealizedPnl, double realizedPnl
    ) {
        Log.Information(
            "#{RequestId}: SignalR sends account update of {Account} - Daily PnL {DailyPnL:+0.00;-#.00}",
            requestId,
            account,
            dailyPnl
        );
        hub.Clients.All.SendAsync(
            SignalREvents.AccountPnlUpdate.ToString(),
            new AccountPnlUpdateMessage {
                Account = account,
                DailyPnl = dailyPnl,
                UnrealizedPnl = unrealizedPnl,
                RealizedPnl = realizedPnl
            }
        );
    }

    public static void SendError(this IHubContext<SignalRHub> hub, string message) {
        Log.Information("SignalR sends error: {ErrorMessage}", message);
        hub.Clients.All.SendAsync(
            SignalREvents.Error.ToString(),
            new ErrorMessage {
                Message = message
            }
        );
    }

    public static void SendMarginInfo(
        this IHubContext<SignalRHub> hub,
        int contractId,
        decimal initialMargin,
        decimal maintenanceMargin
    ) {
        Log.Information(
            "SignalR sends margin info of [{ContractId}]: Initial {Initial:0.00} Maintenance {Maintenance:0.00}",
            contractId,
            initialMargin,
            maintenanceMargin
        );
        hub.Clients.All.SendAsync(
            SignalREvents.MarginInfo.ToString(),
            new MarginInfoMessage {
                ContractId = contractId,
                InitialMargin = initialMargin,
                MaintenanceMargin = maintenanceMargin
            }
        );
    }

    public static void SendOptionChainParams(
        this IHubContext<SignalRHub> hub,
        int requestId,
        OptionDefinitionMessage message
    ) {
        Log.Information(
            "#{RequestId}: SignalR sends option chain params [{UnderlyingContractId}] ({TradingClass})",
            requestId,
            message.UnderlyingContractId,
            message.TradingClass
        );
        hub.Clients.All.SendAsync(SignalREvents.OptionChainParams.ToString(), message);
    }

    private static void SendOrder(
        IHubContext<SignalRHub> hub, SignalREvents signalREvent, int orderId, Contract contract, Order order,
        bool isCompletedOrder
    ) {
        var orderPx = order.LmtPrice.MaxValueAsZero();
        if (orderPx == 0) {
            orderPx = order.AuxPrice.MaxValueAsZero();
        }

        var filledQuantity = order.FilledQuantity.MaxValueAsZero();
        var totalQuantity = order.TotalQuantity.MaxValueAsZero();

        // Completed order could have `TotalQuantity` of 0 instead
        if (isCompletedOrder) {
            totalQuantity = filledQuantity;
        }

        hub.Clients.All.SendAsync(
            signalREvent.ToString(),
            new OrderRecordMessage {
                Account = order.Account,
                OrderId = orderId,
                Contract = contract.ToContractModel(),
                Side = order.Action.ToOrderSide(),
                Tif = order.Tif,
                Type = order.OrderType,
                Price = orderPx,
                FilledQuantity = filledQuantity,
                TargetQuantity = totalQuantity
            }
        );
    }

    public static void SendCompletedOrderRecord(
        this IHubContext<SignalRHub> hub, int orderPermId, Contract contract, Order order
    ) {
        Log.Information(
            "SignalR sends order completed of [{ContractId}]: {OrderInfo}",
            contract.ConId,
            order.ToOrderInfo()
        );
        SendOrder(hub, SignalREvents.OrderRecordCompleted, orderPermId, contract, order, true);
    }

    public static void SendOpenOrderRecord(
        this IHubContext<SignalRHub> hub, int orderPermId, Contract contract, Order order
    ) {
        Log.Information(
            "SignalR sends order open of [{ContractId}]: {OrderInfo}",
            contract.ConId,
            order.ToOrderInfo()
        );
        SendOrder(hub, SignalREvents.OrderRecordOpen, orderPermId, contract, order, false);
    }

    public static void SendOrderUpdate(
        this IHubContext<SignalRHub> hub, int orderPermId, string account, decimal filled, decimal remaining
    ) {
        filled = filled.MaxValueAsZero();
        remaining = remaining.MaxValueAsZero();

        var targetQuantity = filled + remaining;

        hub.Clients.All.SendAsync(
            SignalREvents.OrderUpdate.ToString(),
            new OrderUpdateMessage {
                Account = account,
                OrderId = orderPermId,
                FilledQuantity = filled,
                TargetQuantity = targetQuantity
            }
        );
    }

    public static void SendOrderFilled(this IHubContext<SignalRHub> hub, Order order) {
        Log.Information("SignalR sends order filled: {OrderInfo}", order.ToOrderInfo());
        hub.Clients.All.SendAsync(
            SignalREvents.OrderFilled.ToString(),
            new OrderFilledMessage {
                Account = order.Account,
                OrderId = order.PermId
            }
        );
    }

    public static void SendOrderCancelled(this IHubContext<SignalRHub> hub, Order order) {
        Log.Information("SignalR sends order cancelled: {OrderInfo}", order.ToOrderInfo());
        hub.Clients.All.SendAsync(
            SignalREvents.OrderCancelled.ToString(),
            new OrderCancelledMessage {
                Account = order.Account,
                OrderId = order.PermId
            }
        );
    }

    public static void SendPositionUpdate(
        this IHubContext<SignalRHub> hub, string account, ContractModel contract,
        decimal quantity, double avgPx, double unrealizedPnl, double marketValue
    ) {
        Log.Information(
            "SignalR sends position update of {Account}: {Symbol} x {Quantity} @ {AvgPx:0.00} ({UnrealizedPnL:+0.00;-#.00})",
            account,
            contract.LocalSymbol,
            quantity,
            avgPx,
            unrealizedPnl
        );
        hub.Clients.All.SendAsync(
            SignalREvents.PositionUpdate.ToString(),
            new PositionUpdateMessage {
                Account = account,
                Contract = contract,
                Quantity = quantity,
                AvgPx = avgPx / (double)contract.Multiplier,
                UnrealizedPnl = unrealizedPnl,
                MarketValue = marketValue
            }
        );
    }

    public static void SendPositionPnLUpdate(
        this IHubContext<SignalRHub> hub, int requestId, string account, int contractId, decimal quantity,
        double dailyPnl, double unrealizedPnl, double realizedPnl, double marketValue
    ) {
        Log.Information(
            "#{RequestId}: SignalR sends position PnL update of {Account}: [{ContractId}] x {Quantity} " +
            "- Daily {DailyPnL:+0.00;-#.00} / Unrealized {UnrealizedPnL:+0.00;-#.00}",
            requestId,
            account,
            contractId,
            quantity,
            dailyPnl,
            unrealizedPnl
        );
        hub.Clients.All.SendAsync(
            SignalREvents.PositionPnlUpdate.ToString(),
            new PositionPnlUpdateMessage {
                Account = account,
                ContractId = contractId,
                Quantity = quantity,
                DailyPnl = dailyPnl,
                UnrealizedPnl = unrealizedPnl,
                RealizedPnl = realizedPnl,
                MarketValue = marketValue
            }
        );
    }

    public static void SendPxUpdate(
        this IHubContext<SignalRHub> hub, int requestId, int contractId, PxTick tick, double value
    ) {
        Log.Information(
            "#{RequestId}: SignalR sends single price update of [{ContractId}]: {Tick} as {Value:0.00}",
            requestId,
            contractId,
            tick,
            value
        );
        hub.Clients.All.SendAsync(
            SignalREvents.PxUpdate.ToString(),
            new PxUpdateMessage {
                ContractId = contractId,
                Update = new Dictionary<PxTick, double> {
                    { tick, value }
                }
            }
        );
    }

    public static void SendPxUpdate(
        this IHubContext<SignalRHub> hub, int requestId, int contractId, Dictionary<PxTick, double> update
    ) {
        Log.Information(
            "#{RequestId}: SignalR sends multiple price update of [{ContractId}]: {@Update}",
            requestId,
            contractId,
            update
        );
        hub.Clients.All.SendAsync(
            SignalREvents.PxUpdate.ToString(),
            new PxUpdateMessage {
                ContractId = contractId,
                Update = update
            }
        );
    }

    public static void SendPxHistory(
        this IHubContext<SignalRHub> hub,
        int requestId,
        bool isInit,
        IList<PxDataBarModel> bars,
        IbApiHistoryPxRequestMeta meta
    ) {
        var initOrUpdate = isInit ? "init" : "update";

        if (bars.Count == 0) {
            Log.Warning(
                "#{RequestId}: SignalR skips sending history price {InitOrUpdate}: No bars to send",
                requestId,
                initOrUpdate
            );
            return;
        }

        Log.Information(
            "#{RequestId}: SignalR sends history price {InitOrUpdate} of [{ContractId}] @ {Interval}: {Close:0.00}",
            requestId,
            initOrUpdate,
            meta.ContractId,
            meta.Interval,
            bars[^1].Close
        );
        hub.Clients.All.SendAsync(
            (isInit ? SignalREvents.PxHistoryInit : SignalREvents.PxHistoryUpdate).ToString(),
            new PxHistoryMessage {
                Meta = meta,
                Bars = bars.Select(x => new ChartDataBar {
                    EpochSec = x.Timestamp.ToEpochSeconds(),
                    Open = x.Open == 0 ? null : x.Open,
                    High = x.High == 0 ? null : x.High,
                    Low = x.Low == 0 ? null : x.Low,
                    Close = x.Close == 0 ? null : x.Close
                })
            }
        );
    }
}