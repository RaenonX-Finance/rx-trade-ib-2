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

        var isDateOnly = timestampComponents.Length == 1;
        
        var datetimeString = isDateOnly ? 
            $"{timestampComponents[0]} 00:00:00" : 
            string.Join(" ", timestampComponents[..2]);
        var ianaTimezone = isDateOnly ? "America/New_York" : timestampComponents[2];

        return TimeZoneInfo.ConvertTimeToUtc(
            DateTime.ParseExact(datetimeString, "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture),
            TimeZoneInfo.FindSystemTimeZoneById(ianaTimezone)
        );
    }
}