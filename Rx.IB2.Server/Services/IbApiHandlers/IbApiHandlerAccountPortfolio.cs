using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;

namespace Rx.IB2.Services.IbApiHandlers;

public partial class IbApiHandler {
    public void managedAccounts(string accountsList) {
        var accountList = accountsList.Split(",").Where(x => !string.IsNullOrEmpty(x)).ToList();

        Hub.SendAccountList(accountList);
    }

    public void updateAccountValue(string key, string value, string currency, string accountName) {
        var summaryKey = key.ToAccountSummaryKey();

        if (summaryKey is null) {
            return;
        }

        Hub.SendAccountSummary(summaryKey.Value, accountName, currency, value);
    }

    public void updatePortfolio(
        Contract contract, decimal quantity, double marketPrice, double marketValue, double avgPx,
        double unrealizedPnl, double realizedPnl, string account
    ) {
        var contractModel = contract.ToContractModel();

        RequestManager.AddContractToPool(contract);

        Hub.SendPositionUpdate(account, contractModel, quantity, avgPx, unrealizedPnl, marketValue);
        // Sending margin check whenever there's a position update to make sure margin info is up-to-date
        // Only send margin check for futures
        if (contract.SecType == "FUT") {
            ClientSocket.CheckMargin(RequestManager, contract, OrderSide.Buy);
        }
    }

    public void updateAccountTime(string timestamp) {
        Log.Information("Account updated at {Timestamp}", timestamp);
    }

    public void accountDownloadEnd(string account) {
        Log.Information("Account update of {Account} completed", account);
    }

    public void pnl(int requestId, double dailyPnL, double unrealizedPnL, double realizedPnL) {
        var account = RequestManager.GetAccountOfRequest(requestId);

        if (account is null) {
            Log.Warning("#{RequestId}: No associated account of request found", requestId);
            return;
        }

        Hub.SendAccountPnLUpdate(requestId, account, dailyPnL, unrealizedPnL, realizedPnL);
    }

    public void pnlSingle(
        int requestId, decimal quantity, double dailyPnL, double unrealizedPnL, double realizedPnL, double value
    ) {
        dailyPnL = dailyPnL.MaxValueAsZero();
        unrealizedPnL = unrealizedPnL.MaxValueAsZero();
        realizedPnL = realizedPnL.MaxValueAsZero();

        var contractAssociation = RequestManager.GetContractAssociation(requestId);

        if (contractAssociation is null) {
            Log.Warning("#{RequestId}: No associated contract found for request", requestId);
            return;
        }

        Hub.SendPositionPnLUpdate(
            requestId,
            contractAssociation.Value.Account,
            contractAssociation.Value.ContractId,
            quantity,
            dailyPnL,
            unrealizedPnL,
            realizedPnL,
            value
        );
    }
}