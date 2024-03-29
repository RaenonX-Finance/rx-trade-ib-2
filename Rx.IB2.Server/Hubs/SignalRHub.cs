﻿using Microsoft.AspNetCore.SignalR;
using Rx.IB2.Extensions;
using Rx.IB2.Models;
using Rx.IB2.Models.Requests;
using Rx.IB2.Models.Responses;
using Rx.IB2.Services;

namespace Rx.IB2.Hubs;

public class SignalRHub : Hub {
    private readonly ILogger<SignalRHub> _logger;

    private IbApiSender Sender { get; }

    private IbApiRequestManager RequestManager { get; }

    public SignalRHub(ILogger<SignalRHub> logger, IbApiSender sender, IbApiRequestManager requestManager) {
        _logger = logger;
        Sender = sender;
        RequestManager = requestManager;
    }

    public override Task OnConnectedAsync() {
        _logger.Log(LogLevel.Information, "Received signalR connection");
        Sender.RequestOrders();

        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception) {
        _logger.Log(LogLevel.Information, "Received signalR disconnection");

        return Task.CompletedTask;
    }

    public Task InitAccountList() {
        Sender.RequestManagedAccounts();

        return Task.CompletedTask;
    }

    public Task InitAccount(string account) {
        _logger.Log(LogLevel.Information, "Received init account request for {Account}", account);
        Sender.SubscribeAccountUpdates(account);
        Sender.RequestCompletedOrders();

        return Task.CompletedTask;
    }

    public Task InitOptionChain(InitOptionChainRequest request) {
        _logger.Log(LogLevel.Information, "Received init option chain request of {Symbol}", request.Symbol);
        Sender.InitOptionChain(request);

        return Task.CompletedTask;
    }

    public Task RequestPxTick(PxDataRequest pxDataRequest) {
        var contract = RequestManager.GetContractFromPool(pxDataRequest.ContractId);

        if (contract is null) {
            _logger.Log(
                LogLevel.Warning,
                "Received price tick request of [{ContractId}] in {Account}, but corresponding contract not found",
                pxDataRequest.ContractId,
                pxDataRequest.Account
            );
            return Task.CompletedTask;
        }

        Sender.RequestRealtime(pxDataRequest.Account, contract);
        return Task.CompletedTask;
    }

    public Task CancelPxTick(CancelPxRequest request) {
        Sender.CancelRealtime(request.Account, request.ContractId);
        return Task.CompletedTask;
    }

    public Task<int?> RequestPxHistory(HistoryPxRequestForQuote request) {
        return Task.FromResult(Sender.RequestPxHistoryForQuote(request));
    }

    public Task CancelHistory(CancelApiRequest request) {
        Sender.CancelHistory(request.RequestId);
        return Task.CompletedTask;
    }

    public Task<OptionPxResponse> RequestPxOfOptionChain(OptionPxSubscribeRequest request) {
        return Task.FromResult(Sender.RequestOptionChainPrice(request));
    }

    public Task<IEnumerable<ContractModel>> RequestContractDetails(string symbol) {
        return Task.FromResult(Sender.RequestContractDetails(symbol.ToContract()).Select(x => x.ToContractModel()));
    }

    public Task RequestPnl(PnlRequest pnlRequest) {
        _logger.Log(
            LogLevel.Information,
            "Received PnL subscription request of {Account} on {ContractId}",
            pnlRequest.Account,
            pnlRequest.ContractId
        );
        Sender.RequestSinglePositionPnL(pnlRequest.Account, pnlRequest.ContractId);

        return Task.CompletedTask;
    }

    public Task DisconnectAccount(string account) {
        _logger.Log(LogLevel.Information, "Received disconnection for account {Account}", account);
        Sender.UnsubscribeAccountUpdates(account);

        return Task.CompletedTask;
    }
}