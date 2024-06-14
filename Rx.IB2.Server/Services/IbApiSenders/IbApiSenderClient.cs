using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;

namespace Rx.IB2.Services.IbApiSenders;

public partial class IbApiSender {
    private static Task Throttle() {
        // Throttle to avoid hitting IBKR API message count limit
        return Task.Delay(50);
    }

    private void CancelRequests(string account) {
        Log.Information("Cancelling all requests of {Account}", account);
        RequestManager.CancelRequest(
            account,
            request => {
                Log.Information("#{RequestId}: Cancelling {RequestType} request", request.Id, request.Type);
                switch (request.Type) {
                    case IbApiRequestType.Realtime:
                        ClientSocket.cancelMktData(request.Id);
                        break;
                    case IbApiRequestType.PnL:
                        ClientSocket.cancelPnL(request.Id);
                        break;
                    case IbApiRequestType.PnLSingle:
                        ClientSocket.cancelPnLSingle(request.Id);
                        break;
                    case IbApiRequestType.History:
                        ClientSocket.cancelHistoricalData(request.Id);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(request),
                            $"Unhandled request type to cancel: {request.Type}"
                        );
                }

                RequestManager.MarkRequestCancelled(request.Id);
            }
        );
    }

    public void Connect() {
        ClientSocket.Connect(Config);

        RequestOrders();
    }

    public void Disconnect() {
        Log.Information("Unsubscribing all account updates before IB API disconnection");

        foreach (var account in RequestManager.Accounts) {
            UnsubscribeAccountUpdates(account);
        }

        Log.Information("Disconnecting IB API");
        ClientSocket.eDisconnect();
    }

    public IEnumerable<ContractDetails> RequestContractDetails(Contract contract) {
        var requestId = ClientSocket.RequestContractDetails(RequestManager, contract);

        ContractDetailsManager.EnterLock(requestId);
        return ContractDetailsManager.WaitAndGetData(requestId);
    }
}