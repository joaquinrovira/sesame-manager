using Microsoft.Extensions.Hosting;
using Quartz.Impl.Matchers;

public record class NextSignInSignOutJob(QuartzHostedService Quartz, ILogger<NextSignInSignOutJob> Logger, IHostApplicationLifetime HostApplicationLifetime) : IJob
{
    public static readonly JobKey Key = JobUtils.Of<NextSignInSignOutJob>();
    public async Task Execute(IJobExecutionContext context)
    {
        var t0 = await NextFireTime<CheckInJob>(Quartz.Scheduler);
        if (t0.IsFailure)
        {
            TerminateApplication(t0.Error.Message);
            return;
        }

        var t1 = await NextFireTime<CheckOutJob>(Quartz.Scheduler);
        if (t1.IsFailure)
        {
            TerminateApplication(t1.Error.Message);
            return;
        }

        var t2 = await NextFireTime<PrepareNextYearJob>(Quartz.Scheduler);
        if (t2.IsFailure)
        {
            TerminateApplication(t2.Error.Message);
            return;
        }
        Logger.LogInformation("Next job triggers:\n[SignInJob]         \t{t0}\n[SignOutJob]        \t{t1}\n[PrepareNextYearJob] \t{t2}", t0.Value.ToLocalTime(), t1.Value.ToLocalTime(), t2.Value.ToLocalTime());

        // Reschedule for after next SignOutJob is executed
        await context.Scheduler.RescheduleJob(
            context.Trigger.Key,
            TriggerBuilder
                .Create()
                .ForJob(context.JobDetail)
                .StartAt(t1.Value.AddSeconds(5))
                .Build());
    }

    private async Task<Result<DateTimeOffset, Error>> NextFireTime<T>(IScheduler Scheduler) where T:IJob
    {
        var seed = DateTimeOffset.MaxValue;
        var time = await FindJobTriggers<T>(Scheduler)
            .AggregateAsync(seed, (next, tuple) => {
                var NextFireTime = tuple.Item2.GetNextFireTimeUtc();
                if(!NextFireTime.HasValue) return next;
                else if (NextFireTime >= next) return next;
                return NextFireTime.Value;
            });

        if(time == seed) {
            var msg = $"error retrieving next fire time for job '{typeof(T).Name}'";
            return Result.Failure<DateTimeOffset,Error>(new Error(msg));
        }

        return time;
    }

    private async IAsyncEnumerable<(IJobDetail, ITrigger)> FindJobTriggers<T>(IScheduler Scheduler) 
    {
        var Type = typeof(T);
        var items = Scheduler.ScheduledJobTriggers(detail => detail?.JobType == Type);
        await foreach (var item in items) yield return item;
    }

    private void TerminateApplication(string message)
    {
        Logger.LogCritical(message);
        Environment.ExitCode = 1;
        HostApplicationLifetime.StopApplication();
    }
}
