using System.Collections.Concurrent;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;
using Rx.IB2.Models;
using Rx.IB2.Models.Messages;
using Rx.IB2.Services.Base;
using ILogger = Serilog.ILogger;

namespace Rx.IB2.Services;

public class IbApiOptionDefinitionsManager : IbApiOneTimeRequestManager<OptionDefinitionModel> {
    protected override ILogger Log => Serilog.Log.ForContext(typeof(IbApiOptionDefinitionsManager));

    protected override string Name => "option definition";

    // Key = Request ID; Value = Origin
    private readonly ConcurrentDictionary<int, OptionPxRequestOrigin> _requestOrigin = new();

    public void AddOptionParam(
        int requestId,
        int underlyingContractId,
        string exchange,
        string tradingClass,
        HashSet<string> expirations,
        HashSet<double> strikes
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

    public void RecordRequestOrigin(int requestId, OptionPxRequestOrigin origin) {
        _requestOrigin.AddOrUpdate(requestId, _ => origin, (_, _) => origin);
    }

    public OptionDefinitionMessage? GetMessage(int requestId) {
        ReleaseLock(requestId);

        var originFound = _requestOrigin.TryGetValue(requestId, out var origin);
        if (originFound) {
            return WaitAndGetData(requestId).Aggregate(
                new OptionDefinitionMessage {
                    Origin = origin,
                    TradingClass = [],
                    UnderlyingContractId = 0,
                    Exchange = [],
                    Expiry = [],
                    Strike = []
                },
                (message, model) => new OptionDefinitionMessage {
                    Origin = origin,
                    TradingClass = message.TradingClass.AddAndReturn(model.TradingClass),
                    UnderlyingContractId = model.UnderlyingContractId,
                    Exchange = message.Exchange.AddAndReturn(model.Exchange),
                    Expiry = message.Expiry.UnionAndReturn(model.Expiry),
                    Strike = message.Strike.UnionAndReturn(model.Strike)
                }
            );
        }

        Log.Error("#{RequestId}: Option definition request origin not found", requestId);
        return null;

    }
}