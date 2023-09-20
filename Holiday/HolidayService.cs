using System.Collections.Immutable;
using Microsoft.Extensions.Hosting;


[Service(ServiceLifetime.Singleton)]
public record class HolidayService(
    ILogger<HolidayService> Logger,
    IEnumerable<IHolidayProvider> HolidayProviders,
    IHostApplicationLifetime HostApplicationLifetime,
    IOptions<AdditionalHolidaysConfig> AdditionalHolidaysConfig,
    DateTimeService DateTimeService
)
{
    public Result<IReadOnlySet<DateTime>, Error> Retrieve(int year)
    {
        Logger.LogInformation("Gathering holday data");
        var providerHolidays = RetrieveFromProviders(year);

        // Include extra holidays from config
        foreach (var item in AdditionalHolidaysConfig.Value)
            if (item is not null)
                providerHolidays.Add(item);

        var holidays = providerHolidays
            .Select(e => new DateTime(year, e.Month, e.Day))
            .ToHashSet();
        Logger.LogInformation("Retrieved holidays: \n\t{holidays}", string.Join("\n\t", holidays.ToImmutableSortedSet()));
        return holidays;
    }

    // Aggregate data from all providers
    private record Data(IEnumerable<YearDay> Holidays, bool Failure) { }
    private ISet<YearDay> RetrieveFromProviders(int year)
    {
        var cominedHolidays = new HashSet<YearDay>();
        foreach (var provider in HolidayProviders)
        {
            provider
            .Retrieve(year)
            .TapError(err => Logger.LogWarning("error retrieving data from holiday provider {provider}: {err}", provider.GetType(), err))
            .Tap(cominedHolidays.UnionWith);
        }
        return cominedHolidays;
    }
}