public static class TimeExtensions
{
    public static string AsCron(this DateTime d) => $"{d.Second} {d.Minute} {d.Hour} {d.Day} {d.Month} ? {d.Year}";
    public static string AsCron(this DateTimeOffset d) => $"{d.Second} {d.Minute} {d.Hour} {d.Day} {d.Month} ? {d.Year}";
    public static DateTime At(this DateTime d, TimeOfDay tod) => new DateTime(d.Year, d.Month, d.Day, tod.Hour, tod.Minute, tod.Second, d.Kind);
    public static DateTimeOffset At(this DateTimeOffset d, TimeOfDay tod) => new DateTimeOffset(d.Year, d.Month, d.Day, tod.Hour, tod.Minute, tod.Second, d.Offset);
}
