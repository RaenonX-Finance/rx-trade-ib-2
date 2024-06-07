using Rx.IB2.Enums;

namespace Rx.IB2.Extensions;

public static class EnumExtensions {
    public static OptionRight ToOptionRight(this string callPut) {
        return callPut.ToUpper() switch {
            "C" => OptionRight.Call,
            "CALL" => OptionRight.Call,
            "P" => OptionRight.Put,
            "PUT" => OptionRight.Put,
            _ => throw new ArgumentOutOfRangeException(
                nameof(callPut),
                $"Invalid call or put value for conversion: {callPut}"
            )
        };
    }

    public static string ToCallPutOfContract(this OptionRight right) {
        return right.ToString()[..1];
    }

    public static string ToDefaultDuration(this BarSize size) {
        if (size == BarSize.Sec1) {
            return "14400 S"; // 4 Hr
        }

        if (size == BarSize.Sec5) {
            return "1 D";
        }

        if (size == BarSize.Sec15) {
            return "1 D";
        }

        if (size == BarSize.Sec30) {
            return "2 D";
        }

        if (size == BarSize.Min1) {
            return "1 W";
        }

        if (size == BarSize.Min2) {
            return "2 W";
        }

        if (size == BarSize.Min3) {
            return "2 W";
        }

        if (size == BarSize.Min5) {
            return "1 M";
        }

        if (size == BarSize.Min15) {
            return "2 M";
        }

        if (size == BarSize.Min30) {
            return "3 M";
        }

        if (size == BarSize.Hour) {
            return "1 Y";
        }

        if (size == BarSize.Day) {
            return "3 Y";
        }

        throw new ArgumentException($"Unhandled size-to-default-duration conversion for {size}");
    }
    
    public static string ToIbApiSecurityType(this SecurityType securityType) {
        // Reverse mapping of `StringExtension.ToSecurityType()`
        return securityType switch {
            SecurityType.Stocks => "STK",
            SecurityType.Futures => "FUT",
            SecurityType.ContinuousFutures => "CONTFUT",
            SecurityType.Options => "OPT",
            SecurityType.OptionsCombo => "BAG",
            _ => throw new ArgumentOutOfRangeException(nameof(securityType), $"Invalid security type: {securityType}")
        };
    }

    public static bool IsFuturesType(this SecurityType securityType) {
        return securityType is SecurityType.Futures or SecurityType.ContinuousFutures;
    }
}