
public class TriggerListener : ITriggerListener
{
    public string Name => Guid.NewGuid().ToString();

    public Task TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"{trigger}, {context}, {triggerInstructionCode}");
        return Task.CompletedTask;
    }

    public Task TriggerFired(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"{trigger}, {context}");
        return Task.CompletedTask;
    }

    public Task TriggerMisfired(ITrigger trigger, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"{trigger}");
        return Task.CompletedTask;
    }

    public Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"{trigger}, {context}");
        return Task.FromResult(true);
    }
}
public class JobListener : IJobListener
{
    public string Name => Guid.NewGuid().ToString();

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"{context}");
        return Task.CompletedTask;
    }

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"{context}");
        return Task.CompletedTask;
    }

    public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"{context}, {jobException}");
        return Task.CompletedTask;
    }
}
