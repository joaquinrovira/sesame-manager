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
        var result = RetrieveFromProviders(year);
        if (result.HasNoValue) return new Error("failed to obtain holiday data from any provider");
        var holidaysRaw = result.Value;

        // Include extra holidays from config
        foreach (var item in AdditionalHolidaysConfig.Value)
            if (item is not null)
                holidaysRaw.Add(item);

        var holidays = holidaysRaw
            .Select(e => new DateTime(year, e.Month, e.Day))
            .ToHashSet();
        Logger.LogInformation("Retrieved holidays: \n\t{holidays}", string.Join("\n\t", holidays.ToImmutableSortedSet()));
        return holidays;
    }

    // Aggregate data from all providers
    private record Data(IEnumerable<YearDay> Holidays, bool Failure) { }
    private Maybe<ISet<YearDay>> RetrieveFromProviders(int year)
    {
        var process = HolidayProviders
        .Select(p => p.Retrieve(year))
        .Aggregate(
            seed: new Data(new List<YearDay>(), true),
            func: (acc, data) => data
                    .TapError(err => Logger.LogWarning("error retrieving data from holiday provider: {err}", err))
                    .Map(data => new Data(acc.Holidays.Union(data), false))
                    .Compensate(_ => Result.Success<Data, Error>(acc)).Value
        );
        if (process.Failure) return Maybe.None;
        return process.Holidays.ToHashSet();
    }
}