[Service]
public record class DateTimeService(IOptions<GeneralConfig> GeneralConfig)
{
    public readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById(GeneralConfig.Value.TZ);

    public DateTimeOffset Local(DateTimeOffset d) => d.ToOffset(TimeZone.GetUtcOffset(d));
    public DateTimeOffset Local(DateTime d)
    {
        var date = DateTime.SpecifyKind(d, DateTimeKind.Unspecified); 
        var offset = TimeZone.GetUtcOffset(date);
        return new DateTimeOffset(date, offset);
    }
}