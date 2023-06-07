using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Services;
using ILogger = Serilog.ILogger;

namespace Rx.IB2.Extensions;

public static class ClientSocketExtensions {
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(ClientSocketExtensions));

    public static void CheckMargin(
        this EClientSocket clientSocket, IbApiRequestManager requestManager, Contract contract, OrderSide side
    ) {
        var requestId = requestManager.GetNextRequestIdNoCancel();
        var order = new Order {
            Action = side.ToString().ToUpper(),
            OrderType = "MKT",
            TotalQuantity = 1,
            WhatIf = true
        };

        Log.Information(
            "#{RequestId}: Requesting margin check for {LocalSymbol}",
            requestId,
            contract.LocalSymbol
        );

        contract.AddExchangeOnContract();
        clientSocket.placeOrder(requestId, contract, order);
    }
}