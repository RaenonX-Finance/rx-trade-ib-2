using Rx.IB2.Enums;

namespace Rx.IB2.Services.IbApiSenders;

public partial class IbApiSender {
    public void RequestManagedAccounts() {
        Log.Information("Requesting managed account numbers");
        ClientSocket.reqManagedAccts();
    }

    public void SubscribeAccountUpdates(string account) {
        Log.Information("Subscribing account update of {Account}", account);

        ClientSocket.reqAccountUpdates(true, account);
        RequestAccountPnL(account);
    }

    public void UnsubscribeAccountUpdates(string account) {
        Log.Information("Unsubscribing account update of {Account}", account);
        ClientSocket.reqAccountUpdates(false, account);
        CancelRequests(account);
    }

    private void RequestAccountPnL(string account) {
        var requestId = RequestManager.GetNextRequestId(IbApiRequestType.PnL, account);

        Log.Information(
            "#{RequestId}: Subscribing PnL of account {Account}",
            requestId,
            account
        );
        ClientSocket.reqPnL(requestId, account, "");
    }

    public void RequestSinglePositionPnL(string account, int contractId) {
        var requestId = RequestManager.GetNextRequestId(IbApiRequestType.PnLSingle, account, contractId);

        Log.Information(
            "#{RequestId}: Subscribing PnL of contract [{ContractId}]",
            requestId,
            contractId
        );

        ClientSocket.reqPnLSingle(
            requestId,
            account,
            "",
            contractId
        );
    }

    public void RequestCompletedOrders() {
        ClientSocket.reqCompletedOrders(false);
    }

    public void RequestOrders() {
        ClientSocket.reqOpenOrders();
        RequestCompletedOrders();
    }
}