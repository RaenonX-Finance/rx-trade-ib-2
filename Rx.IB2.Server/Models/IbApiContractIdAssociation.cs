namespace Rx.IB2.Models; 

public struct IbApiContractIdAssociation {
    public required string Account { get; init; }

    public required int ContractId { get; init; }
}