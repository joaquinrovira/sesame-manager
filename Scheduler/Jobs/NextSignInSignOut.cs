public record class NextSignInSignOut(QuartzHostedService Quartz, ILogger<NextSignInSignOut> Logger) : IJob
{
    public static readonly JobKey Key = JobUtils.Of<NextSignInSignOut>();
    public async Task Execute(IJobExecutionContext context)
    {
        var t0 = await GetNextFireTimeForJob(Quartz.Scheduler, SignIn.Key);
        var t1 = await GetNextFireTimeForJob(Quartz.Scheduler, SignOut.Key);
        Logger.LogInformation("Next execution SignIn - {t0} || SignOut - {t1}", t0, t1);

        // TODO: Reschedule to after next signout
        await context.Scheduler.UnscheduleJob(context.Trigger.Key);
    }

    private async Task<DateTime> GetNextFireTimeForJob(IScheduler Scheduler, JobKey Key)
    {
        DateTime nextFireTime = DateTime.MinValue;

        bool isJobExisting = await Scheduler.CheckExists(Key);
        if (isJobExisting)
        {
            var detail = await Scheduler.GetJobDetail(Key);
            var triggers = await Scheduler.GetTriggersOfJob(Key);

            if (triggers.Count > 0)
            {
                var nextFireTimeUtc = triggers.First().GetNextFireTimeUtc();
                if(nextFireTimeUtc.HasValue) nextFireTime = nextFireTimeUtc.Value.DateTime.ToLocalTime();
            }
        }

        return nextFireTime;
    }
}
