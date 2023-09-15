using Microsoft.Extensions.Hosting;


[Service(ServiceLifetime.Singleton)]
public record class HolidayService (ILogger<HolidayService> Logger, IHolidayProvider HolidayProvider): IHostedService
{
    public IReadOnlySet<DateTime> Holidays { get; private set; } = new HashSet<DateTime>();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Gathering holday data");
        // TODO: add backup if provider fails - i.e. add another IHolidayProvider that retrieves alternate holiday calendar from config
        var result = await HolidayProvider.RetrieveAsync();
        if (result.IsFailure) throw new Exception("unable to retrieve holiday values");
        Holidays = (IReadOnlySet<DateTime>) result.Value;
        Logger.LogInformation("Retrieved the following holidays: \n\t{p}", string.Join("\n\t",Holidays));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}