namespace Rx.IB2.Models; 

public struct OptionDefinitionModel {
    public required string TradingClass { get; init; }

    public required int UnderlyingContractId { get; init; }
    
    public required string Exchange { get; init; }
    
    public required HashSet<string> Expiry { get; init; }
    
    public required HashSet<double> Strike { get; init; }
}