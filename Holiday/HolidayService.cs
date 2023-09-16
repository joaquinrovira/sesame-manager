using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;


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
        var holidays = result.Value;

        // Include extra holidays from config
        foreach (var item in AdditionalHolidaysConfig.Value) 
            holidays.Add(new DateTime(year, item.Month, item.Day));

        Logger.LogInformation("Retrieved the following holidays: \n\t{holidays}", string.Join("\n\t", holidays));
        return (IReadOnlySet<DateTime>)holidays;
    }

    private void TerminateApplication(string message) {
            Logger.LogCritical(message);
            Environment.ExitCode = 1;
            HostApplicationLifetime.StopApplication();
    }
}