using Microsoft.Extensions.Hosting;

public record class PrepareNextYearJob(
    QuartzHostedService Quartz, 
    ILogger<PrepareNextYearJob> Logger, 
    IHostApplicationLifetime HostApplicationLifetime, 
    SchedulerService SchedulerService
    ) : IJob
{
    public static string CronExpression(int year) => new DateTime(year, 12, 31, 0, 0, 0).AsCron();
    public static readonly JobKey Key = JobUtils.Of<PrepareNextYearJob>();
    public async Task Execute(IJobExecutionContext context)
    {
        var nextYear = DateTime.Now.Year + 1;
        Logger.LogInformation($"Starting setup for next year ({nextYear})");
        // Register SingIn and SingOut for next year's data
        await SchedulerService.RegisterSignInOut(StartOfYear(nextYear));

        // Register itself for 31 dec of next year
        Logger.LogInformation($"Succesfully configured jobs for next year ({nextYear})");
        Logger.LogInformation($"Rescheduling next {Key} for '{CronExpression(nextYear)}'");
        await context.Scheduler.RescheduleJob(
            context.Trigger.Key, 
            TriggerBuilder
                .Create()
                .ForJob(context.JobDetail)
                .WithCronSchedule(CronExpression(nextYear))
                .Build());
    }

    public static DateTime StartOfYear(int year) => new DateTime(year, 1, 1);
}
