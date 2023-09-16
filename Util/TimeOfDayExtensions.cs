public static class TimeOfDayExtensions
{
    public static TimeOfDay Jitter(this TimeOfDay t, int seconds) {
        var jitter = Random.Shared.Next(seconds) - seconds/2;
        return t.GetTimeOfDayForDate(DateTime.Now).AddSeconds(jitter).TimeOfDay.ToQuartz();
    }
    private static TimeOfDay ToQuartz(this TimeSpan t) => new(t.Hours, t.Minutes, t.Seconds);
}
