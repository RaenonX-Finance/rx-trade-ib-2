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