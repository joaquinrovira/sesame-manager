using Microsoft.Extensions.Hosting;


[Service(ServiceLifetime.Singleton)]
public record class HolidayService(
    ILogger<HolidayService> Logger, 
    IHolidayProvider HolidayProvider, 
    IHostApplicationLifetime HostApplicationLifetime,
    IOptions<AdditionalHolidaysConfig> AdditionalHolidaysConfig
) {
    public IReadOnlySet<DateTimeOffset> Retrieve(int year) {
        Logger.LogInformation("Gathering holday data");
        var result = HolidayProvider.Retrieve(year);// TODO: Multiple providers (IEnumerable<IHolidayProvider>) to provide alternatives in case one fails
        if(result.IsFailure) TerminateApplication(result.Error);
        var holidays = result.Value;

        // Include extra holidays from config
        foreach (var item in AdditionalHolidaysConfig.Value) 
            holidays.Add(new DateTimeOffset(new DateTime(year, item.Month, item.Day, 0, 0, 0, DateTimeKind.Local)));

        Logger.LogInformation("Retrieved holidays: \n\t{holidays}", string.Join("\n\t", holidays));
        return (IReadOnlySet<DateTimeOffset>)holidays;
    }

    private void TerminateApplication(string message) {
            Logger.LogCritical(message);
            Environment.ExitCode = 1;
            HostApplicationLifetime.StopApplication();
    }
}