﻿using System.Globalization;
using System.Text;
using Rx.IB2.Enums;

namespace Rx.IB2.Extensions;

public static class StringExtensions {
    private static T? EnumToString<T>(this string value) where T : struct, Enum {
        var converted = Enum.TryParse(value, out T outSummaryKey);

        if (!converted) {
            return null;
        }

        return outSummaryKey;
    }

    private static T EnumToStringNonNull<T>(this string value) where T : struct, Enum {
        var stringInEnum = EnumToString<T>(value);

        if (stringInEnum is null) {
            throw new ArgumentOutOfRangeException(nameof(value), $"Invalid enum to string conversion: {value}");
        }

        return stringInEnum.Value;
    }

    private static string ToTitleCase(this string str) {
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str.ToLower());
    }

    public static string ToSnakeCase(this string text) {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length < 2) {
            return text.ToLowerInvariant();
        }

        var sb = new StringBuilder();
        sb.Append(char.ToLowerInvariant(text[0]));
        for (var i = 1; i < text.Length; ++i) {
            var c = text[i];

            if (char.IsUpper(c)) {
                sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            } else {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    public static AccountSummaryKey? ToAccountSummaryKey(this string summaryKey) {
        return EnumToString<AccountSummaryKey>(summaryKey.Replace("-", "").Replace("+", ""));
    }

    public static OrderStatus? ToOrderStatus(this string orderStatus) {
        return EnumToString<OrderStatus>(orderStatus);
    }

    public static DurationUnit ToDurationUnit(this string durationUnit) {
        return EnumToStringNonNull<DurationUnit>(durationUnit.ToUpper());
    }

    public static OrderSide ToOrderSide(this string side) {
        return EnumToStringNonNull<OrderSide>(side.ToTitleCase());
    }

    public static SecurityType ToSecurityType(this string securityType) {
        // Reverse mapping of `EnumExtension.ToIbApiSecurityType()`
        return securityType switch {
            "STK" => SecurityType.Stocks,
            "FUT" => SecurityType.Futures,
            "FOP" => SecurityType.FuturesOptions,
            "CONTFUT" => SecurityType.ContinuousFutures,
            "OPT" => SecurityType.Options,
            "BAG" => SecurityType.OptionsCombo,
            _ => throw new ArgumentOutOfRangeException(nameof(securityType), $"Invalid security type: {securityType}")
        };
    }

    public static BarSize ToBarSize(this string barSize) {
        foreach (var availableBarSize in BarSize.AvailableValues) {
            if (barSize == availableBarSize.Interval) {
                return availableBarSize;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(barSize), $"Invalid bar size: {barSize}");
    }

    public static (string Symbol, string Exchange) ToFuturesInfo(this string symbolAtExchange) {
        var futuresInfo = symbolAtExchange.Split("@");

        if (futuresInfo.Length < 2) {
            throw new ArgumentException(
                $"Futures symbol ({symbolAtExchange}) invalid. An example would be NQ@CME."
            );
        }

        return (futuresInfo[0], futuresInfo[1]);
    }
}