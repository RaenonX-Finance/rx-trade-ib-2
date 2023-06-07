using Rx.IB2.Extensions;
using Rx.IB2.Models;
using Rx.IB2.Models.Messages;
using Rx.IB2.Services.Base;
using ILogger = Serilog.ILogger;

namespace Rx.IB2.Services;

public class IbApiOptionDefinitionsManager : IbApiOneTimeRequestManager<OptionDefinitionModel> {
    protected override ILogger Log => Serilog.Log.ForContext(typeof(IbApiOptionDefinitionsManager));

    protected override string Name => "option definition";

    public void AddOptionParam(
        int requestId, int underlyingContractId, string exchange, string tradingClass, 
        HashSet<string> expirations, HashSet<double> strikes
    ) {
        AddData(
            requestId,
            new OptionDefinitionModel {
                TradingClass = tradingClass,
                UnderlyingContractId = underlyingContractId,
                Exchange = exchange,
                Expiry = expirations,
                Strike = strikes
            }
        );
    }

    public OptionDefinitionMessage GetMessage(int requestId) {
        ReleaseLock(requestId);

        return WaitAndGetData(requestId).Aggregate(
            new OptionDefinitionMessage {
                TradingClass = new HashSet<string>(),
                UnderlyingContractId = 0,
                Exchange = new HashSet<string>(),
                Expiry = new HashSet<string>(),
                Strike = new HashSet<double>()
            },
            (message, model) => new OptionDefinitionMessage {
                TradingClass = message.TradingClass.AddAndReturn(model.TradingClass),
                UnderlyingContractId = model.UnderlyingContractId,
                Exchange = message.Exchange.AddAndReturn(model.Exchange),
                Expiry = message.Expiry.UnionAndReturn(model.Expiry),
                Strike = message.Strike.UnionAndReturn(model.Strike)
            }
        );
    }
}