using System.Globalization;

namespace Rx.IB2.Models.Utils;

public record DateInterval {
    private const string IbApiMessageDateFormat = "yyyyMMdd:HHmm";

    public required DateTime Start { get; init; }

    public required DateTime End { get; init; }

    private DateTime StartUtc => Start.ToUniversalTime();

    private DateTime EndUtc => End.ToUniversalTime();

    public bool IsCurrent() {
        var now = DateTime.UtcNow.ToUniversalTime();

        return StartUtc <= now && now <= EndUtc;
    }

    public static DateInterval[] FromIbApiMessage(string timezoneId, string maybeIntervals) {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);

        return maybeIntervals
            .Split(";")
            // Excludes the case where a trading session is "closed"
            // Sample message: 20240619:CLOSED;20240620:0930-20240620:1600
            .Where(interval => interval.Split(":")[1] != "CLOSED")
            .Select(interval => interval.Split("-"))
            .Select(splitInterval => new DateInterval {
                Start = TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.ParseExact(splitInterval[0], IbApiMessageDateFormat, CultureInfo.InvariantCulture),
                    timezone
                ),
                End = TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.ParseExact(splitInterval[1], IbApiMessageDateFormat, CultureInfo.InvariantCulture),
                    timezone
                )
            })
            .ToArray();
    }
}