using IBApi;
using JetBrains.Annotations;
using Rx.IB2.Extensions;

namespace Rx.IB2.Models.Requests.OptionVolatilityHistoryRequest;

public readonly struct OptionVolatilityHistoryRequest {
    [UsedImplicitly]
    public required string Symbol { get; init; }
    
    [UsedImplicitly]
    public required string Account { get; init; }

    private Contract Contract => Symbol.ToContract();

    public OptionVolatilityHistoryRequestHv AsHistoryVolatilityRequestHv() {
        return new OptionVolatilityHistoryRequestHv {
            Contract = Contract
        };
    }

    public OptionVolatilityHistoryRequestIv AsHistoryVolatilityRequestIv() {
        return new OptionVolatilityHistoryRequestIv {
            Contract = Contract
        };
    }
}