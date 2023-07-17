using System.Text.RegularExpressions;
using IBApi;
using Rx.IB2.Const;
using Rx.IB2.Enums;
using Rx.IB2.Models;
using Rx.IB2.Models.Options;
using Rx.IB2.Utils;

namespace Rx.IB2.Extensions;

public static partial class ContractExtensions {
    private static Contract ApplyOptionValues(this Contract contract, ToContractOptions? options = null) {
        contract.Exchange = options?.Exchange ?? contract.Exchange;
        contract.ConId = options?.ContractId ?? contract.ConId;

        return contract;
    }
    
    private static decimal ToDecimalMultiplier(this string multiplier, SecurityType securityType) {
        if (securityType == SecurityType.Stocks) {
            return 1;
        }

        return string.IsNullOrEmpty(multiplier) ? decimal.Zero : decimal.Parse(multiplier);
    }

    private static string GetSymbol(this Contract contract, SecurityType securityType) {
        return securityType switch {
            SecurityType.OptionsCombo => $"{contract.LocalSymbol} (Combo)",
            _ => contract.LocalSymbol
        };
    }
    
    private static ContractModel ToContractModel(Contract contract, ContractDetailsModel? details) {
        var securityType = contract.SecType.ToSecurityType();
        
        return new ContractModel {
            Id = contract.ConId,
            SecurityType = securityType,
            LocalSymbol = contract.GetSymbol(securityType),
            Exchange = contract.Exchange,
            Multiplier = contract.Multiplier.ToDecimalMultiplier(securityType),
            Details = details
        };
    }

    public static ContractModel ToContractModel(this Contract contract) {
        return ToContractModel(contract, null);
    }

    public static ContractModel ToContractModel(this ContractDetails contractDetails) {
        return ToContractModel(
            contractDetails.Contract,
            new ContractDetailsModel {
                MinTick = contractDetails.MinTick
            }
        );
    }

    public static void AddExchangeOnContract(this Contract contract) {
        if (string.IsNullOrEmpty(contract.Exchange)) {
            contract.Exchange = string.IsNullOrEmpty(contract.PrimaryExch) ? "SMART" : contract.PrimaryExch;
        }
    }

    public static Contract ToOptionsContractFromOcc(this string occSymbol) {
        var symbol = occSymbol[..6].TrimEnd();
        var date = $"20{occSymbol[6..12]}";
        var right = occSymbol[12].ToString().ToOptionRight();
        var strike = Convert.ToDouble(occSymbol[13..]) / 1000;

        return ContractMaker.MakeUsStockOptions(symbol, date, right, strike);
    }

    [GeneratedRegex("(?<Symbol>[A-Z]+)(?<Expiry>\\d+)(?<CallPut>C|P)(?<Strike>[\\d.]+)")]
    private static partial Regex FidelityOptionRegex();

    private static Contract ToOptionsContractFromFidelity(this string fidelityStyleSymbol) {
        var groups = FidelityOptionRegex().Match(fidelityStyleSymbol).Groups;

        var symbol = groups["Symbol"].Value;
        var date = $"20{groups["Expiry"].Value}";
        var right = groups["CallPut"].Value.ToOptionRight();
        var strike = Convert.ToDouble(groups["Strike"].Value);

        return ContractMaker.MakeUsStockOptions(symbol, date, right, strike);
    }

    public static Contract ToContract(this string symbol, ToContractOptions? options = null) {
        Contract contract;
        
        if (symbol.StartsWith(SymbolPrefixes.Futures)) {
            // Futures
            symbol = symbol[1..];

            contract = char.IsNumber(symbol[^1]) ? 
                symbol.ToFuturesContract() : 
                symbol.ToContinuousFuturesContract();
        } else if (symbol.StartsWith(SymbolPrefixes.Options)) {
            // Options
            symbol = symbol[1..];

            contract = options?.AutoConvertOptionsToStocks ?? false
                ? FidelityOptionRegex().Match(symbol).Groups["Symbol"].Value.ToUsStockContract()
                : symbol.ToOptionsContractFromFidelity();
        } else {
            // Stocks
            contract = symbol.ToUsStockContract();
        }

        return contract.ApplyOptionValues(options);
    }
    
    public static Contract ToFuturesContract(this string symbol) {
        return ContractMaker.MakeUsFutures(symbol);
    }

    public static Contract ToContinuousFuturesContract(this string symbol) {
        return ContractMaker.MakeUsContinuousFutures(symbol);
    }

    public static Contract ToUsStockContract(this string symbol) {
        return ContractMaker.MakeUsStock(symbol);
    }

    public static string ToContractInfo(this ContractDetails contractDetails) {
        return contractDetails.Contract.LocalSymbol;
    }
}