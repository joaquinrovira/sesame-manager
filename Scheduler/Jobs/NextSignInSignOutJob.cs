using Microsoft.Extensions.Hosting;

public record class NextSignInSignOutJob(QuartzHostedService Quartz, ILogger<NextSignInSignOutJob> Logger, IHostApplicationLifetime HostApplicationLifetime) : IJob
{
    public static readonly JobKey Key = JobUtils.Of<NextSignInSignOutJob>();
    public async Task Execute(IJobExecutionContext context)
    {
        var t0 = await GetNextFireTimeForJob(Quartz.Scheduler, SignInJob.Key);
        if(t0.IsFailure) {
            TerminateApplication(t0.Error);
            return;
        }

        var t1 = await GetNextFireTimeForJob(Quartz.Scheduler, SignOutJob.Key);
        if(t1.IsFailure) {
            TerminateApplication(t1.Error);
            return;
        }
        Logger.LogInformation("Next execution SignInJob - {t0}\nNext execution SignOutJob - {t1}", t0.Value, t1.Value);

        // Reschedule for after next SignOutJob is executed
        await context.Scheduler.RescheduleJob(
            context.Trigger.Key, 
            TriggerBuilder
                .Create()
                .ForJob(context.JobDetail)
                .WithCronSchedule(t1.Value.AddSeconds(2).AsCron())
                .Build());
    }

    private async Task<Result<DateTime>> GetNextFireTimeForJob(IScheduler Scheduler, JobKey Key)
    {
        var failure = () => Result.Failure<DateTime>($"Error retrieving next fire time for job '{Key}'");
        
        if (!await Scheduler.CheckExists(Key)) return failure();

        var triggers = await Scheduler.GetTriggersOfJob(Key);
        if (triggers.Count == 0) return failure();

        var nextFireTimeUtc = triggers.First().GetNextFireTimeUtc();
        if(nextFireTimeUtc is null ) return failure();

        return nextFireTimeUtc.Value.DateTime.ToLocalTime();
    }

    private void TerminateApplication(string message) {
            Logger.LogCritical(message);
            Environment.ExitCode = 1;
            HostApplicationLifetime.StopApplication();
    }
}
