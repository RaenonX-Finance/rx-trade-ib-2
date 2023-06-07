using IBApi;
using Rx.IB2.Extensions;
using Rx.IB2.Services.Base;
using ILogger = Serilog.ILogger;

namespace Rx.IB2.Services;

public class IbApiContractDetailsManager : IbApiOneTimeRequestManager<ContractDetails> {
    protected override ILogger Log => Serilog.Log.ForContext(typeof(IbApiContractDetailsManager));

    protected override string Name => "contract detail requester";

    public override void AddData(int requestId, ContractDetails data) {
        var contract = data.Contract;
        var securityType = contract.SecType.ToSecurityType();

        if (securityType.IsFuturesType() && contract.Exchange == "QBALGO") {
            // For example, NQ, 2 contract details are returned with `contract.Exchange` being
            // either QBALGO or CME, while both of these contracts share the same ID.
            // This is undesired because we don't care about the quote from QBALGO.
            return;
        }
        
        base.AddData(requestId, data);
    }
}