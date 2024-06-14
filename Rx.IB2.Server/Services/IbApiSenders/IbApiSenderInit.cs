using IBApi;
using Rx.IB2.Enums;
using Rx.IB2.Services.IbApiHandlers;
using ILogger = Serilog.ILogger;

namespace Rx.IB2.Services.IbApiSenders;

public partial class IbApiSender(
    IConfiguration config,
    IbApiHandler handler,
    IbApiRequestManager requestManager,
    IbApiHistoryPxRequestManager historyPxRequestManager,
    IbApiContractDetailsManager contractDetailsManager,
    IbApiOptionDefinitionsManager optionDefinitionsManager,
    IbApiOneTimePxRequestManager oneTimePxRequestManager
) {
    // NOTE: Event handlers of `EWrapper` could be handled on different thread
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(IbApiSender));

    private EClientSocket ClientSocket { get; } = handler.ClientSocket;

    private IbApiRequestManager RequestManager { get; } = requestManager;

    private IbApiHistoryPxRequestManager HistoryPxRequestManager { get; } = historyPxRequestManager;

    private IbApiContractDetailsManager ContractDetailsManager { get; } = contractDetailsManager;

    private IbApiOptionDefinitionsManager OptionDefinitionsManager { get; } = optionDefinitionsManager;

    private IbApiOneTimePxRequestManager OneTimePxRequestManager { get; } = oneTimePxRequestManager;

    private IConfiguration Config { get; } = config;

    private static readonly PxTick[] OptionPxTargetTick = [
        PxTick.ModelOptionPx,
        PxTick.Mark,
        PxTick.Delta,
        PxTick.Gamma,
        PxTick.Theta,
        PxTick.Vega,
        PxTick.ImpliedVolatility,
        PxTick.PvDividend,
        PxTick.OptionCallOpenInterest,
        PxTick.OptionPutOpenInterest
    ];
}