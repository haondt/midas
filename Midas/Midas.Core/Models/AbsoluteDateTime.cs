namespace Midas.Core.Models
{
    public readonly record struct AbsoluteDateTime(long UnixTimeSeconds) : IComparable<AbsoluteDateTime>
    {
        public readonly DateTimeOffset DateTimeOffset => DateTimeOffset.FromUnixTimeSeconds(UnixTimeSeconds).ToUniversalTime();
        public readonly DateTime LocalTime => DateTimeOffset.DateTime.ToLocalTime();
        public readonly DateTime UtcTime => DateTimeOffset.DateTime;
        public static AbsoluteDateTime Now => AbsoluteDateTime.Create(DateTime.UtcNow);

        public override string ToString()
        {
            return UnixTimeSeconds.ToString();
        }

        public static AbsoluteDateTime Create(long unixTimeSeconds)
        {
            return new AbsoluteDateTime(unixTimeSeconds);
        }

        public static AbsoluteDateTime Create(DateTimeOffset offset)
        {
            return new AbsoluteDateTime(offset.ToUnixTimeSeconds());
        }

        public static AbsoluteDateTime Create(DateTime dateTime)
        {
            return new AbsoluteDateTime(((DateTimeOffset)dateTime).ToUnixTimeSeconds());
        }

        public static readonly AbsoluteDateTime MinValue = Create(new DateTime(1, 1, 1));
        public static readonly AbsoluteDateTime MaxValue = Create(new DateTime(9999, 12, 31));

        public static TimeSpan operator -(AbsoluteDateTime a, AbsoluteDateTime b)
        {
            var difference = a.UnixTimeSeconds - b.UnixTimeSeconds;
            return TimeSpan.FromSeconds(difference);
        }

        public static bool operator <=(AbsoluteDateTime a, AbsoluteDateTime b) => a.UnixTimeSeconds <= b.UnixTimeSeconds;
        public static bool operator >=(AbsoluteDateTime a, AbsoluteDateTime b) => a.UnixTimeSeconds >= b.UnixTimeSeconds;
        public static bool operator <(AbsoluteDateTime a, AbsoluteDateTime b) => a.UnixTimeSeconds < b.UnixTimeSeconds;
        public static bool operator >(AbsoluteDateTime a, AbsoluteDateTime b) => a.UnixTimeSeconds > b.UnixTimeSeconds;
        public int CompareTo(AbsoluteDateTime other)
        {
            return UnixTimeSeconds.CompareTo(other.UnixTimeSeconds);
        }

        public AbsoluteDateTime FloorToLocalDay()
        {
            var localTime = LocalTime;
            var dt = new DateTime(localTime.Year, localTime.Month, localTime.Day, 0, 0, 0, DateTimeKind.Local);
            return AbsoluteDateTime.Create(dt);
        }

        public AbsoluteDateTime FloorToLocalMonth()
        {
            var localTime = LocalTime;
            var dt = new DateTime(localTime.Year, localTime.Month, 1, 0, 0, 0, DateTimeKind.Local);
            return AbsoluteDateTime.Create(dt);
        }
        public AbsoluteDateTime FloorToLocalYear()
        {
            var localTime = LocalTime;
            var dt = new DateTime(localTime.Year, 1, 1, 0, 0, 0, DateTimeKind.Local);
            return AbsoluteDateTime.Create(dt);
        }

        public AbsoluteDateTime AddDays(int days) => AbsoluteDateTime.Create(UtcTime.AddDays(days));
        public AbsoluteDateTime AddMonths(int months) => AbsoluteDateTime.Create(UtcTime.AddMonths(months));
        public AbsoluteDateTime AddYears(int years) => AbsoluteDateTime.Create(UtcTime.AddYears(years));
        public AbsoluteDateTime AddLocalDays(int days) => AbsoluteDateTime.Create(LocalTime.AddDays(days));
        public AbsoluteDateTime AddLocalMonths(int months) => AbsoluteDateTime.Create(LocalTime.AddMonths(months));
        public AbsoluteDateTime AddLocalYears(int years) => AbsoluteDateTime.Create(LocalTime.AddYears(years));

    }

}
