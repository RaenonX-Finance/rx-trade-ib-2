using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Services;
using ILogger = Serilog.ILogger;

namespace Rx.IB2.Extensions;

public static class ClientSocketExtensions {
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(ClientSocketExtensions));

    public static void Connect(this EClientSocket clientSocket, IConfiguration config) {
        var section = config.GetSection("TWS");
        var port = section.GetValue<int>("Port");
        var clientId = section.GetValue<int>("ClientId");

        Log.Information("Connecting IB API at port {Port} of client ID {ClientId}", port, clientId);
        clientSocket.eConnect("localhost", port, clientId);
    }

    public static void CheckMargin(
        this EClientSocket clientSocket,
        IbApiRequestManager requestManager,
        Contract contract,
        OrderSide side
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

    public static int RequestContractDetails(
        this EClientSocket clientSocket,
        IbApiRequestManager requestManager,
        Contract contract
    ) {
        var requestId = requestManager.GetNextRequestIdNoCancel();

        Log.Information(
            "#{RequestId}: Requesting contract details of {ContractCustomSymbol}",
            requestId,
            contract.ToCustomContractSymbol()
        );

        clientSocket.reqContractDetails(requestId, contract);

        return requestId;
    }
}