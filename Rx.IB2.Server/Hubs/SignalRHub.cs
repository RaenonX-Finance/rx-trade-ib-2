using Microsoft.AspNetCore.SignalR;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;
using Rx.IB2.Models;
using Rx.IB2.Models.Requests;
using Rx.IB2.Models.Responses;
using Rx.IB2.Services;
using Rx.IB2.Services.IbApiSenders;

namespace Rx.IB2.Hubs;

public class SignalRHub(
    ILogger<SignalRHub> logger,
    IbApiSender sender,
    IbApiRequestManager requestManager
) : Hub {
    private IbApiSender Sender { get; } = sender;

    private IbApiRequestManager RequestManager { get; } = requestManager;

    public override Task OnConnectedAsync() {
        logger.Log(LogLevel.Information, "Received signalR connection");
        Sender.RequestOrders();

        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception) {
        logger.Log(LogLevel.Information, "Received signalR disconnection");

        return Task.CompletedTask;
    }

    public Task InitAccountList() {
        Sender.RequestManagedAccounts();

        return Task.CompletedTask;
    }

    public Task InitAccount(string account) {
        logger.Log(LogLevel.Information, "Received init account request for {Account}", account);
        Sender.SubscribeAccountUpdates(account);
        Sender.RequestCompletedOrders();

        return Task.CompletedTask;
    }

    public Task RequestPnl(PnlRequest pnlRequest) {
        logger.Log(
            LogLevel.Information,
            "Received PnL subscription request of {Account} on {ContractId}",
            pnlRequest.Account,
            pnlRequest.ContractId
        );
        Sender.RequestSinglePositionPnL(pnlRequest.Account, pnlRequest.ContractId);

        return Task.CompletedTask;
    }

    public Task SubscribePxTick(PxDataRequest pxDataRequest) {
        var contract = RequestManager.GetContractFromPool(pxDataRequest.ContractId);

        if (contract is null) {
            logger.Log(
                LogLevel.Warning,
                "Received price tick request of [{ContractId}] in {Account}, but corresponding contract not found",
                pxDataRequest.ContractId,
                pxDataRequest.Account
            );
            return Task.CompletedTask;
        }

        Sender.SubscribeRealtime(pxDataRequest.Account, contract, MarketDataType.Live);
        return Task.CompletedTask;
    }

    public Task<int?> SubscribePxHistory(HistoryPxRequestForQuote request) {
        return Task.FromResult(Sender.SubscribePxHistoryForQuote(request));
    }

    public Task<OptionPxResponse> SubscribePxOfOptions(OptionPxRequest request) {
        return Sender.SubscribeOptionsPx(request);
    }

    public Task<OptionPxResponse> RequestPxOfOptions(OptionPxRequest request) {
        return Sender.RequestOptionsPx(request);
    }

    public Task<IEnumerable<ContractModel>> RequestContractDetails(string symbol) {
        return Task.FromResult(Sender.RequestContractDetails(symbol.ToContract()).Select(x => x.ToContractModel()));
    }

    public Task RequestOptionDefinitions(OptionDefinitionRequest request) {
        logger.Log(LogLevel.Information, "Received option definition request of {Symbol}", request.Symbol);
        Sender.RequestOptionDefinitions(request);

        return Task.CompletedTask;
    }

    public Task CancelPxTick(CancelPxRequest request) {
        Sender.CancelRealtime(request.Account, request.ContractId);
        return Task.CompletedTask;
    }

    public Task CancelHistory(CancelApiRequest request) {
        Sender.CancelHistory(request.RequestId);
        return Task.CompletedTask;
    }

    public Task DisconnectAccount(string account) {
        logger.Log(LogLevel.Information, "Received disconnection for account {Account}", account);
        Sender.UnsubscribeAccountUpdates(account);

        return Task.CompletedTask;
    }
}