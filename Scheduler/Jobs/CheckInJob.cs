using SesameApi;

public record class CheckInJob(
    ILogger<CheckInJob> Logger, 
    IOptions<GeneralConfig> GeneralConfig, 
    IOptions<WeeklyScheduleConfig> WeeklyScheduleConfig
) : IJob
{
    public static readonly JobKey Key = JobUtils.Of<CheckInJob>();
    public async Task Execute(IJobExecutionContext context)
    {
        var site = context.JobDetail.JobDataMap.GetString(JobDataKeys.Site);
        Logger.LogInformation("Checking in from location: {site}", site);
        var s = new Sesame(GeneralConfig.Value.Email, GeneralConfig.Value.Password);
        await SesameUtil.CheckInOutInfo(s, site)
            .Tap(e => Logger.LogInformation("Successfully retrieved data: {e}", e))
            .Bind(e => s.CheckIn(e.UserId, e.SiteId))
            .Tap(() => Logger.LogInformation("Successfully checked in!"))
            .SuccessOrThrow();
        await context.Scheduler.UnscheduleJob(context.Trigger.Key);
    }
}
