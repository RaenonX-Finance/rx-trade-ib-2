namespace Rx.IB2.Enums;

public class BarSize {
    private BarSize(string value, string interval) {
        Value = value;
        Interval = interval;
    }

    private string Value { get; }

    public string Interval { get; private init; }

    public static BarSize Sec1 => new("1 sec", "1s");

    public static BarSize Sec5 => new("5 secs", "5s");

    public static BarSize Sec15 => new("15 secs", "15s");

    public static BarSize Sec30 => new("30 secs", "30s");

    public static BarSize Min1 => new("1 min", "1m");

    public static BarSize Min2 => new("2 mins", "2m");

    public static BarSize Min3 => new("3 mins", "3m");

    public static BarSize Min5 => new("5 mins", "5m");

    public static BarSize Min15 => new("15 mins", "15m");

    public static BarSize Min30 => new("30 mins", "30m");

    public static BarSize Hour => new("1 hour", "1h");

    public static BarSize Day => new("1 day", "1d");
    
    public static readonly IList<BarSize> AvailableValues = new List<BarSize> {
        Sec1,
        Sec5,
        Sec15,
        Sec30,
        Min1,
        Min2,
        Min3,
        Min5,
        Min15,
        Min30,
        Hour,
        Day
    };

    public static bool operator ==(BarSize a, BarSize b) => a.Equals(b);

    public static bool operator !=(BarSize a, BarSize b) => !a.Equals(b);

    private bool Equals(BarSize other) {
        return Value == other.Value;
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(null, obj)) {
            return false;
        }

        if (ReferenceEquals(this, obj)) {
            return true;
        }

        return obj.GetType() == GetType() && Equals((BarSize)obj);
    }

    public override int GetHashCode() {
        return Value.GetHashCode();
    }

    public override string ToString() {
        return Value;
    }
}