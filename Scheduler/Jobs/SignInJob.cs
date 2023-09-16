using SesameApi;

public record class SignInJob(IOptions<GeneralConfig> GeneralConfig, IOptions<WeeklyScheduleConfig> WeeklyScheduleConfig) : IJob
{
    public static readonly JobKey Key = JobUtils.Of<SignInJob>();
    public async Task Execute(IJobExecutionContext context)
    {
        var siteName = context.JobDetail.JobDataMap.GetString("Site");
        var s = new Sesame(GeneralConfig.Value.Email, GeneralConfig.Value.Password);
        await SesameUtil.CheckInOutInfo(s, siteName).Bind(e => s.CheckIn(e.UserId, e.SiteId)).SuccessOrThrow();
        await context.Scheduler.UnscheduleJob(context.Trigger.Key);
    }
}
