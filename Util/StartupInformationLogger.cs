using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;


[Service(ServiceLifetime.Singleton)]
public record class StartupInformationLogger(
    ILogger<SchedulerService> Logger,
    IOptions<GeneralConfig> GeneralConfig,
    IOptions<WeeklyScheduleConfig> WeeklyScheduleConfig,
    IOptions<AdditionalHolidaysConfig> AdditionalHolidaysConfig,
    IOptions<IdealHolidayProviderConfiguration> IdealHolidayProviderConfiguration,
    IOptions<NagerHolidayProviderConfiguration> NagerHolidayProviderConfiguration,
    DateTimeService DateTimeService
) : IHostedService
{
    List<ConfigMessageItem> Sections = new List<ConfigMessageItem>{
        new("TimeZone", DateTimeService.TimeZone, true),
        new("Weekly schedule", WeeklyScheduleConfig.Value),
        new("Holiday Provider - nager.com", NagerHolidayProviderConfiguration.Value),
        new("Holiday Provider - ideal.es", IdealHolidayProviderConfiguration.Value),
        new("Additional holidays", AdditionalHolidaysConfig.Value),
    };

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation(ConfigurationMessage());
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private string ConfigurationMessage()
    {
        var builder = new StringBuilder();
        builder.AppendLine("============= CONFIGURATION =============");
        Sections.ForEach(e => builder.AppendLine(e + "\n"));
        builder.AppendLine("=========================================");
        return builder.ToString();
    }

    private record ConfigMessageItem(string name, object value, bool pretty = false) {
        public override string ToString() => $"{name}:\n{JSON(value, pretty)}";
        public static implicit operator string(ConfigMessageItem i) => i.ToString();
    }
    private static string JSON<T>(T value, bool WriteIndented = false) => JsonSerializer.Serialize(value, new JsonSerializerOptions() { WriteIndented = WriteIndented });
}