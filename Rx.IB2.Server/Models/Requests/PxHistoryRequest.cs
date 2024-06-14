using IBApi;
using JetBrains.Annotations;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;
using Rx.IB2.Interfaces;
using Rx.IB2.Models.Options;

namespace Rx.IB2.Models.Requests;

public readonly struct PxHistoryRequest : IHistoryPxRequest {
    [UsedImplicitly]
    public required string Account { get; init; }
    
    [UsedImplicitly]
    public required int ContractId { get; init; }
    
    [UsedImplicitly]
    public required string Symbol { get; init; }

    [UsedImplicitly]
    public required string Exchange { get; init; }

    [UsedImplicitly]
    public required string Interval { get; init; }

    [UsedImplicitly]
    public required bool RthOnly { get; init; }

    public bool IsSubscription => true;

    public HistoryDataType DataType =>
        Contract.SecType.ToSecurityType() == SecurityType.Options ? HistoryDataType.Midpoint : HistoryDataType.Trades;

    public BarSize BarSize => Interval.ToBarSize();

    public string Duration => BarSize.ToDefaultDuration();

    // Can't simply use `ContractId` to create a contract because some other fields depend on information on `Contract`
    // For example, `DataType`
    public Contract Contract => Symbol.ToContract(new ToContractOptions { ContractId = ContractId, Exchange = Exchange });
}