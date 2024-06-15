using IBApi;
using JetBrains.Annotations;
using Rx.IB2.Extensions;
using Rx.IB2.Models.Options;

namespace Rx.IB2.Models.Requests.OptionVolatilityHistoryRequest;

public readonly struct OptionVolatilityHistoryRequest {
    [UsedImplicitly]
    public required string Symbol { get; init; }

    [UsedImplicitly]
    public required string Account { get; init; }

    [UsedImplicitly]
    public required int ContractId { get; init; }

    private Contract Contract => Symbol.ToContract(new ToContractOptions { ContractId = ContractId });

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