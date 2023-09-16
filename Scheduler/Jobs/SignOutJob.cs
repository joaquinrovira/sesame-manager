using SesameApi;

public record class SignOutJob(IOptions<GeneralConfig> GeneralConfig, IOptions<WeeklyScheduleConfig> WeeklyScheduleConfig) : IJob
{
    public static readonly JobKey Key = JobUtils.Of<SignOutJob>();
    public async Task Execute(IJobExecutionContext context)
    {
        var siteName = context.JobDetail.JobDataMap.GetString("Site");
        var s = new Sesame(GeneralConfig.Value.Email, GeneralConfig.Value.Password);
        await SesameUtil.CheckInOutInfo(s, siteName).Bind(e => s.CheckOut(e.UserId, e.SiteId)).SuccessOrThrow();
        await context.Scheduler.UnscheduleJob(context.Trigger.Key);
    }
}
