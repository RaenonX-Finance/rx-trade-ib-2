using IBApi;
using Microsoft.AspNetCore.SignalR;
using Rx.IB2.Hubs;
using ILogger = Serilog.ILogger;

namespace Rx.IB2.Services.IbApiHandlers;

public partial class IbApiHandler : EWrapper {
    // NOTE: Event handlers of `EWrapper` could be handled on different thread

    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(IbApiHandler));

    private IHubContext<SignalRHub> Hub { get; }

    private IbApiRequestManager RequestManager { get; }

    private IbApiHistoryPxRequestManager HistoryPxRequestManager { get; }

    private IbApiContractDetailsManager ContractDetailsManager { get; }

    private IbApiOptionDefinitionsManager OptionDefinitionsManager { get; }

    public EReaderSignal ReaderSignal { get; }

    public EClientSocket ClientSocket { get; }

    public IbApiHandler(
        IHubContext<SignalRHub> hub, IbApiRequestManager requestManager,
        IbApiHistoryPxRequestManager historyPxRequestManager,
        IbApiContractDetailsManager contractDetailsManager,
        IbApiOptionDefinitionsManager optionDefinitionsManager
    ) {
        ReaderSignal = new EReaderMonitorSignal();
        ClientSocket = new EClientSocket(this, ReaderSignal);
        Hub = hub;
        RequestManager = requestManager;
        HistoryPxRequestManager = historyPxRequestManager;
        ContractDetailsManager = contractDetailsManager;
        OptionDefinitionsManager = optionDefinitionsManager;
    }
}