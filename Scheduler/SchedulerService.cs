using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;


[Service(ServiceLifetime.Singleton)]
public record class SchedulerService(
    ILogger<SchedulerService> Logger, 
    HolidayService HolidayService, 
    QuartzHostedService Quartz,
    IConfiguration Configuration,
    IOptions<GeneralConfig> GeneralConfig,
    IOptions<WeeklyScheduleConfig> WeeklyScheduleConfig
) : IHostedService
{
    private IScheduler Scheduler => Quartz.Scheduler; 

    private readonly IReadOnlyDictionary<DayOfWeek, (TimeOfDay, TimeOfDay)> Calendar = (IReadOnlyDictionary<DayOfWeek, (TimeOfDay, TimeOfDay)>)WeeklyScheduleConfig.Value.ToDict();
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Initializing schedule.");
        Logger.LogInformation("Running with weekly schedule:\n{schedule}", JsonSerializer.Serialize(WeeklyScheduleConfig.Value, new JsonSerializerOptions(){WriteIndented = true}));
        await ConfigureScheduler();
    }

    private async Task ConfigureScheduler()
    {
        await PreLoadJobs();
        await RegisterSignInOut();
        await RegisterUtilityJobs();
    }

    private async Task PreLoadJobs() {
        await Scheduler.AddJob(JobBuilder.Create<NextSignInSignOutJob>().WithIdentity(NextSignInSignOutJob.Key).Build(), true, true);
        await Scheduler.AddJob(JobBuilder.Create<PrepareNextYearJob>().WithIdentity(PrepareNextYearJob.Key).Build(), true, true);
    }

    private async Task RegisterUtilityJobs() {
        await Scheduler.ScheduleJob(TriggerBuilder.Create().ForJob(NextSignInSignOutJob.Key)
            .StartNow()
            .Build());

        await Scheduler.ScheduleJob(TriggerBuilder.Create().ForJob(PrepareNextYearJob.Key)
            .WithCronSchedule(PrepareNextYearJob.CronExpression(DateTime.Now.Year))
            .Build());
    }

    public async Task RegisterSignInOut(DateTime? t = null) {
        // Setup data
        var Now = DateTime.Now;
        var Date = t ?? Now.AddDays(1);
        Date = Now.CompareTo(Date) > 0 ? Now : Date; // NOTE: sanity check - work with dates before today
        var EndDate = new DateTime(Date.Year + 1, 1, 1);
        var Holidays = HolidayService.Retrieve(Date.Year);

        Logger.LogInformation("Registering SignIn and SignOut events for the year {year}.", Date.Year);
        // Register jobs based on holidays and dates
        for (; Date < EndDate; Date = Date.AddDays(1))
        {
            if (IsWeekend(Date.DayOfWeek)) continue;
            if (IsHoliday(Date, Holidays)) continue;
            if (!Calendar.ContainsKey(Date.DayOfWeek)) continue;

            var (SignInTime, SignOutTime) = Calendar[Date.DayOfWeek];
            SignInTime = SignInTime.Jitter(300);
            SignOutTime = SignOutTime.Jitter(300);

            var SiteName = WeeklyScheduleConfig.Value.For(Date.DayOfWeek).Map(e => e.SignInSite).GetValueOrDefault();

            await RegisterJobWithSite<SignInJob>(Scheduler, Date.At(SignInTime), SiteName);
            await RegisterJobWithSite<SignOutJob>(Scheduler, Date.At(SignOutTime), SiteName);
        }

        if (!await Scheduler.ScheduledJobTriggers().AnyAsync())
        Logger.LogWarning("No jobs have been registered! Check the configuration, perhaps the WeeklySchedule configuration is missing.");
    }

    private Task RegisterJobWithSite<T>(IScheduler Scheduler, DateTime Date, string? SiteName) where T:IJob 
    {
        Logger.LogInformation("[{type}] {date}", typeof(T).Name, Date);
        var job = JobBuilder.Create<T>()
            .UsingJobData("Site", SiteName)
            .Build();
        var trigger = TriggerBuilder.Create()
            .WithCronSchedule($"{Date.Second} {Date.Minute} {Date.Hour} {Date.Day} {Date.Month} ? {Date.Year}")
            .Build();
        return Scheduler.ScheduleJob(job, trigger);
    }

    private bool IsWeekend(DayOfWeek dow) => dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;
    private bool IsHoliday(DateTime date, IReadOnlySet<DateTime> Holidays) => Holidays.Contains(date.Date);

    public async Task StopAsync(CancellationToken cancellationToken)
    {

        Logger.LogInformation("Terminating scheduler");
        await Scheduler.Shutdown();
    }
}