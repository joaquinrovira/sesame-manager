[Service(ServiceLifetime.Singleton)]
public record class ConifgurationHolidayProvider(
    IOptions<AdditionalHolidaysConfig> Config
) : IHolidayProvider
{
    public Task<Result<ISet<YearDay>, Error>> RetrieveAsync(int Year)
        => Task.FromResult<Result<ISet<YearDay>, Error>>(Config.Value.ToHashSet());
}