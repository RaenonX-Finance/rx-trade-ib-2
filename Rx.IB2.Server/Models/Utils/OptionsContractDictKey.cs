using Rx.IB2.Enums;

namespace Rx.IB2.Models.Utils;

public struct OptionsContractDictKey {
    public required double Strike { get; init; }

    public required string Expiry { get; init; }
    
    public required OptionRight Right { get; init; }
}