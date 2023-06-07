using System.Globalization;

namespace Rx.IB2.Extensions;

public static class DatetimeExtensions {
    public static string ToIbApiFormat(this DateTime dateTime) {
        return dateTime.ToString("yyyyMMdd-HH:mm:ss");
    }
    
    public static long ToEpochSeconds(this DateTime datetime) {
        return ((DateTimeOffset)datetime).ToUnixTimeSeconds();
    }

    public static DateTime FromHistoricalPxTimestampToUtc(this string timestamp) {
        var timestampComponents = timestamp.Split(" ");
        var datetimeString = string.Join(" ", timestampComponents[..2]);
        var ianaTimezone = timestampComponents[2];

        return TimeZoneInfo.ConvertTimeToUtc(
            DateTime.ParseExact(datetimeString, "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture),
            TimeZoneInfo.FindSystemTimeZoneById(ianaTimezone)
        );
    }
}