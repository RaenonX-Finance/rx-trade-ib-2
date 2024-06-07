using IBApi;
using Rx.IB2.Extensions;

namespace Rx.IB2.Services.IbApiHandlers;

public partial class IbApiHandler {
    public void contractDetails(int requestId, ContractDetails contractDetails) {
        Log.Information(
            "#{RequestId}: Received contract details of [{ContractId}]: {ContractDetails}",
            requestId,
            contractDetails.Contract.ConId,
            contractDetails.ToContractInfo()
        );
        ContractDetailsManager.AddData(requestId, contractDetails);
        RequestManager.AddContractToPool(contractDetails.Contract);
    }

    public void contractDetailsEnd(int requestId) {
        Log.Information("#{RequestId}: End of contract details retrieval", requestId);

        if (ContractDetailsManager.ReleaseLock(requestId)) {
            Log.Information("#{RequestId}: Contract details lock released", requestId);
        }
    }

    public void securityDefinitionOptionParameter(
        int requestId,
        string exchange,
        int underlyingContractId,
        string tradingClass,
        string multiplier,
        HashSet<string> expirations,
        HashSet<double> strikes
    ) {
        Log.Information(
            "#{RequestId}: Received option definitions of {UnderlyingSymbol} " +
            "({ExpirationCount} expirations / {StrikeCount} strikes / {Exchange})",
            requestId,
            tradingClass,
            expirations.Count,
            strikes.Count,
            exchange
        );
        OptionDefinitionsManager.AddOptionParam(
            requestId,
            underlyingContractId,
            exchange,
            tradingClass,
            expirations,
            strikes
        );
    }

    public void securityDefinitionOptionParameterEnd(int requestId) {
        var message = OptionDefinitionsManager.GetMessage(requestId);
        if (message is null) {
            return;
        }
        
        Hub.SendOptionDefinitions(requestId, message.Value);
    }

    public void tickReqParams(int requestId, double minTick, string bboExchange, int snapshotPermissions) {
        // https://interactivebrokers.github.io/tws-api/md_receive.html#smart_mapping
        Log.Information(
            "#{RequestId}: Minimum tick = {MinTick} / Exchange: {BboExchange}",
            requestId,
            minTick,
            bboExchange
        );
    }
}