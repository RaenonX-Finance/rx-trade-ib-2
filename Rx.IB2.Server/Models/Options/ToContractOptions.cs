namespace Rx.IB2.Models.Options; 

public readonly struct ToContractOptions {
    public bool AutoConvertOptionsToStocks { get; init; }

    public int ContractId { get; init; }

    public string Exchange { get; init; }
}