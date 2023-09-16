public static class DateTimeExtensions
{
    public static string AsCron(this DateTime d) => $"{d.Second} {d.Minute} {d.Hour} {d.Day} {d.Month} ? {d.Year}";
}
