namespace Rx.IB2.Services.IbApiHandlers;

public partial class IbApiHandler {
    private void CancelMarketData(int requestId) {
        RequestManager.MarkRequestCancelled(requestId);
        ClientSocket.cancelMktData(requestId);
    }
}