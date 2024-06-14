namespace Rx.IB2.Services.IbApiHandlers;

public partial class IbApiHandler {
    public void error(Exception e) {
        Log.Error(e, "Error occurred in IB API");
        throw e;
    }

    public void error(string message) {
        Log.Error("Error message captured in IB API: {Message}", message);
    }

    public void error(int requestId, int errorCode, string errorMsg, string advancedOrderRejectJson) {
        // Avoid deadlock
        HistoryPxRequestManager.ReleaseLock(requestId);
        ContractDetailsManager.ReleaseLock(requestId);

        if (requestId == -1) {
            // Not a true error - notification only
            Log.Information("IB API notification [{Code}]: {Message}", errorCode, errorMsg);
            return;
        }

        if (RequestManager.IsRequestCancelled(requestId)) {
            // Request already cancelled, error from it should be fine getting disregarded
            return;
        }
        
        Log.Error("#{RequestId}: [{Code}] {Message}", requestId, errorCode, errorMsg);
    }

    public void nextValidId(int id) {
        Log.Information("Updating next valid ID to {NewValidId}", id);
        RequestManager.SetRequestId(id);
    }

    public void connectAck() {
        Log.Information("Connected to IB API");
    }

    public void connectionClosed() {
        Log.Information("Connection to IB API closed");
    }
}