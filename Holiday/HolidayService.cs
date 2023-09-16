using Microsoft.Extensions.Hosting;


[Service(ServiceLifetime.Singleton)]
public record class HolidayService(
    ILogger<HolidayService> Logger, 
    IHolidayProvider HolidayProvider, 
    IHostApplicationLifetime HostApplicationLifetime
) {
    public IReadOnlySet<DateTime> Retrieve(int year) {
        Logger.LogInformation("Gathering holday data");
        // TODO: Multiple providers (IEnumerable<IHolidayProvider>) to provide alternatives in case one fails
        var result = HolidayProvider.Retrieve(year);
        if(result.IsFailure) TerminateApplication(result.Error);
        var holidays = result.Value;
        Logger.LogInformation("Retrieved the following holidays: \n\t{holidays}", string.Join("\n\t", holidays));
        return holidays;
    }

    private void TerminateApplication(string message) {
            Logger.LogCritical(message);
            Environment.ExitCode = 1;
            HostApplicationLifetime.StopApplication();
    }
}