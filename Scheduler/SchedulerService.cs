using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Quartz.Impl;
using Quartz.Logging;

[Service(ServiceLifetime.Singleton)]
public record class SchedulerService(ILogger<SchedulerService> Logger, HolidayService HolidayService, QuartzHostedService Quartz) : IHostedService
{
    private IScheduler Scheduler => Quartz.Scheduler; 

    private readonly IReadOnlyDictionary<DayOfWeek, (TimeOfDay, TimeOfDay)> Calendar = new Dictionary<DayOfWeek, (TimeOfDay, TimeOfDay)>()
    {
        {DayOfWeek.Monday, (TimeOfDay.HourAndMinuteOfDay(8,30), TimeOfDay.HourAndMinuteOfDay(17,30))},
        {DayOfWeek.Tuesday, (TimeOfDay.HourAndMinuteOfDay(8,30), TimeOfDay.HourAndMinuteOfDay(17,30))},
        {DayOfWeek.Wednesday, (TimeOfDay.HourAndMinuteOfDay(8,30), TimeOfDay.HourAndMinuteOfDay(17,30))},
        {DayOfWeek.Thursday, (TimeOfDay.HourAndMinuteOfDay(8,30), TimeOfDay.HourAndMinuteOfDay(17,30))},
        {DayOfWeek.Friday, (TimeOfDay.HourAndMinuteOfDay(8,0), TimeOfDay.HourAndMinuteOfDay(14,0))}
    };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Initializing schedule");
        await ConfigureScheduler();
    }

    public async Task ConfigureScheduler()
    {
        // Preload Jobs
        await Scheduler.AddJob(JobBuilder.Create<SignIn>().WithIdentity(SignIn.Key).Build(), true, true);
        await Scheduler.AddJob(JobBuilder.Create<SignOut>().WithIdentity(SignOut.Key).Build(), true, true);
        await Scheduler.AddJob(JobBuilder.Create<NextSignInSignOut>().WithIdentity(NextSignInSignOut.Key).Build(), true, true);

        await Scheduler.ScheduleJob(TriggerBuilder.Create().ForJob(SignIn.Key)
            .StartNow()
            .Build());
        
        var Date = DateTime.Now.AddDays(1).Date;
        var EndDate = new DateTime(Date.Year + 1, 1, 1);
        for (; Date < EndDate; Date = Date.AddDays(1))
        {
            if (IsWeekend(Date.DayOfWeek)) continue;
            if (IsHoliday(Date)) continue;
            if (!Calendar.ContainsKey(Date.DayOfWeek)) continue;

            var (SignInTime, SignOutTime) = Calendar[Date.DayOfWeek];
            await Scheduler.ScheduleJob(TriggerBuilder.Create().ForJob(SignIn.Key)
                .WithCronSchedule($"0 {SignInTime.Minute} {SignInTime.Hour} {Date.Day} {Date.Month} ? {Date.Year}")
                .Build());
            await Scheduler.ScheduleJob(TriggerBuilder.Create().ForJob(SignOut.Key)
                .WithCronSchedule($"0 {SignOutTime.Minute} {SignOutTime.Hour} {Date.Day} {Date.Month} ? {Date.Year}")
                .Build());
        }

        await Scheduler.ScheduleJob(TriggerBuilder.Create().ForJob(NextSignInSignOut.Key)
            .StartNow()
            .Build());  


        // Skip weekend or holiday
        // Register a SignIn  job at the start of the day for that day of the week
        // Register a SignOut job at the end   of the day for that day of the week
    }

    private bool IsWeekend(DayOfWeek dow) => dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;
    private bool IsHoliday(DateTime date) => HolidayService.Holidays.Contains(date.Date);

    public async Task StopAsync(CancellationToken cancellationToken)
    {

        Logger.LogInformation("Terminating scheduler");
        await Scheduler.Shutdown();
    }
}