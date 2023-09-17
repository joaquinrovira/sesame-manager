using System.Text.Json;
using Microsoft.Extensions.Hosting;


[Service(ServiceLifetime.Singleton)]
public record class StartupInformationLogger(
    ILogger<SchedulerService> Logger,
    IOptions<GeneralConfig> GeneralConfig,
    IOptions<WeeklyScheduleConfig> WeeklyScheduleConfig,
    IOptions<AdditionalHolidaysConfig> AdditionalHolidaysConfig,
    DateTimeService DateTimeService
) : IHostedService
{

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var (message, parameters) = ConfigurationMessage();
        Logger.LogInformation(message, parameters);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private (string, object?[]) ConfigurationMessage()
    {
        var parameters = new List<object?>
        {
            JsonSerializer.Serialize(DateTimeService.TimeZone, new JsonSerializerOptions() { WriteIndented = true }),
            JsonSerializer.Serialize(WeeklyScheduleConfig.Value, new JsonSerializerOptions() { }),
            JsonSerializer.Serialize(AdditionalHolidaysConfig.Value, new JsonSerializerOptions() { })
        };
        var message = @"============= CONFIGURATION =============

TimeZone:
{timezone}

Weekly schedule:
{schedule}

Additional holidays:
{holidays}

=========================================
";
        return (message, parameters.ToArray());
    }
}