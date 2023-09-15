using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Quartz.Impl.Matchers;
using Quartz.Spi;

[Service]
public class QuartzHostedService : IHostedService
{
    [NotNull] public IScheduler? Scheduler { get; private set; }// NOTE: initialized on StartAsync, null warning can be safely ignored
    private readonly ISchedulerFactory SchedulerFactory;
    private readonly IJobFactory JobFactory;

    public QuartzHostedService(
        ISchedulerFactory schedulerFactory,
        IJobFactory jobFactory)
    {
        SchedulerFactory = schedulerFactory;
        JobFactory = jobFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Scheduler = await SchedulerFactory.GetScheduler(cancellationToken);
        // Scheduler.ListenerManager.AddJobListener(new JobListener(), GroupMatcher<JobKey>.AnyGroup());
        // Scheduler.ListenerManager.AddTriggerListener(new TriggerListener(), GroupMatcher<TriggerKey>.AnyGroup());
        Scheduler.JobFactory = JobFactory;
        await Scheduler.Start(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Scheduler.Shutdown(cancellationToken);
    }
}