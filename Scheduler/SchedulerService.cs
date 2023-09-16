using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;


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
        Logger.LogInformation("Initializing schedule");
        await ConfigureScheduler();
    }

    private async Task ConfigureScheduler()
    {
        await PreLoadJobs();
        await RegisterSignInOut();
        await RegisterUtilityJobs();
    }

    private async Task PreLoadJobs() {
        await Scheduler.AddJob(JobBuilder.Create<SignInJob>().WithIdentity(SignInJob.Key).Build(), true, true);
        await Scheduler.AddJob(JobBuilder.Create<SignOutJob>().WithIdentity(SignOutJob.Key).Build(), true, true);
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

        if (GeneralConfig.Value.Environment == HostEnvironment.Development)
        await Scheduler.ScheduleJob(TriggerBuilder.Create().ForJob(PrepareNextYearJob.Key)
            .StartNow()
            .Build());
    }

    public async Task RegisterSignInOut(DateTime? t = null) {
        // Setup data
        var Now = DateTime.Now;
        var Date = t ?? Now.AddDays(1);
        Date = Now.CompareTo(Date) > 0 ? Now : Date; // NOTE: sanity check - work with dates before today
        var EndDate = new DateTime(Date.Year + 1, 1, 1);
        var Holidays = HolidayService.Retrieve(Date.Year);


        // Register jobs based on holidays and dates
        for (; Date < EndDate; Date = Date.AddDays(1))
        {
            if (IsWeekend(Date.DayOfWeek)) continue;
            if (IsHoliday(Date, Holidays)) continue;
            if (!Calendar.ContainsKey(Date.DayOfWeek)) continue;

            var (SignInTime, SignOutTime) = Calendar[Date.DayOfWeek];
            SignInTime = SignInTime.Jitter(600);
            SignOutTime = SignOutTime.Jitter(600);
            await Scheduler.ScheduleJob(TriggerBuilder.Create().ForJob(SignInJob.Key)
                .WithCronSchedule($"{SignInTime.Second} {SignInTime.Minute} {SignInTime.Hour} {Date.Day} {Date.Month} ? {Date.Year}")
                .Build());
            await Scheduler.ScheduleJob(TriggerBuilder.Create().ForJob(SignOutJob.Key)
                .WithCronSchedule($"{SignOutTime.Second} {SignOutTime.Minute} {SignOutTime.Hour} {Date.Day} {Date.Month} ? {Date.Year}")
                .Build());
        }
    }

    private bool IsWeekend(DayOfWeek dow) => dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;
    private bool IsHoliday(DateTime date, IReadOnlySet<DateTime> Holidays) => Holidays.Contains(date.Date);

    public async Task StopAsync(CancellationToken cancellationToken)
    {

        Logger.LogInformation("Terminating scheduler");
        await Scheduler.Shutdown();
    }
}