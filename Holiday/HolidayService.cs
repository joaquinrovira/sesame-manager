using System.Collections.Immutable;
using Microsoft.Extensions.Hosting;


[Service(ServiceLifetime.Singleton)]
public record class HolidayService(
    ILogger<HolidayService> Logger, 
    IHolidayProvider HolidayProvider, 
    IHostApplicationLifetime HostApplicationLifetime,
    IOptions<AdditionalHolidaysConfig> AdditionalHolidaysConfig
) {
    public IReadOnlySet<DateTime> Retrieve(int year) {
        Logger.LogInformation("Gathering holday data");
        var result = HolidayProvider.Retrieve(year);// TODO: Multiple providers (IEnumerable<IHolidayProvider>) to provide alternatives in case one fails
        if(result.IsFailure) TerminateApplication(result.Error);
        var holidaysRaw = result.Value;

        // Include extra holidays from config
        foreach (var item in AdditionalHolidaysConfig.Value) 
            if(item is not null)
                holidaysRaw.Add(item);

        var holidays = holidaysRaw.Select(e => new DateTime(year, e.Month, e.Day, 0, 0, 0, DateTimeKind.Local)).ToHashSet();
        Logger.LogInformation("Retrieved holidays: \n\t{holidays}", string.Join("\n\t", holidays.ToImmutableSortedSet()));
        return holidays;
    }

    private void TerminateApplication(string message) {
            Logger.LogCritical(message);
            Environment.ExitCode = 1;
            HostApplicationLifetime.StopApplication();
    }
}