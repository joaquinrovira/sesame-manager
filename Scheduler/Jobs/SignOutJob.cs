using SesameApi;

public record class SignOutJob(
    ILogger<SignOutJob> Logger, 
    IOptions<GeneralConfig> GeneralConfig, 
    IOptions<WeeklyScheduleConfig> WeeklyScheduleConfig
    ) : IJob
{
    public static readonly JobKey Key = JobUtils.Of<SignOutJob>();
    public async Task Execute(IJobExecutionContext context)
    {
        var site = context.JobDetail.JobDataMap.GetString("Site");
        Logger.LogInformation("Signing out from location: {site}", site);
        var s = new Sesame(GeneralConfig.Value.Email, GeneralConfig.Value.Password);
        await SesameUtil.CheckInOutInfo(s, site)
            .Tap(e => Logger.LogInformation("Successfully retrieved data: {e}", e))
            .Bind(e => s.CheckOut(e.UserId, e.SiteId))
            .Tap(() => Logger.LogInformation("Successfully checked out!"))
            .SuccessOrThrow();
        await context.Scheduler.UnscheduleJob(context.Trigger.Key);
    }
}
