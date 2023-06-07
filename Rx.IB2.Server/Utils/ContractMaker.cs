using IBApi;
using Rx.IB2.Const;
using Rx.IB2.Enums;
using Rx.IB2.Extensions;

namespace Rx.IB2.Utils;

public static class ContractMaker {
    public static Contract MakeOptions(
        string symbol, string expiry, OptionRight right, double strike,
        string? tradingClass = null
    ) {
        return symbol.Contains(SymbolPrefixes.Futures)
            ? MakeUsFuturesOptions(symbol, expiry, right, strike, tradingClass)
            : MakeUsStockOptions(symbol, expiry, right, strike);
    }

    public static Contract MakeUsStock(string symbol) {
        return new Contract {
            Symbol = symbol,
            SecType = "STK",
            Currency = "USD",
            Exchange = "SMART"
        };
    }

    public static Contract MakeUsStockOptions(string symbol, string expiry, OptionRight right, double strike) {
        return new Contract {
            Symbol = symbol,
            SecType = "OPT",
            Exchange = "SMART",
            Currency = "USD",
            LastTradeDateOrContractMonth = expiry,
            Strike = strike,
            Right = right.ToCallPutOfContract()
        };
    }

    public static Contract MakeUsContinuousFutures(string symbol) {
        return new Contract {
            Symbol = symbol,
            SecType = "CONTFUT"
        };
    }

    public static Contract MakeUsFutures(string symbol) {
        return new Contract {
            LocalSymbol = symbol,
            SecType = "FUT"
        };
    }

    private static Contract MakeUsFuturesOptions(
        string symbol, string expiry, OptionRight right, double strike,
        string? tradingClass = null
    ) {
        var info = symbol.ToFuturesInfo();

        var contract = new Contract {
            Symbol = info.Symbol,
            SecType = "FOP",
            Exchange = info.Exchange,
            Currency = "USD",
            LastTradeDateOrContractMonth = expiry,
            Strike = strike,
            Right = right.ToCallPutOfContract()
        };

        if (tradingClass is not null) {
            contract.TradingClass = tradingClass;
        }

        return contract;
    }
}