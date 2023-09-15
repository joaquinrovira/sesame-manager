public class SignOut : IJob
{
    public static readonly JobKey Key = JobUtils.Of<SignOut>();
    public async Task Execute(IJobExecutionContext context)
    {
        await Console.Out.WriteLineAsync("Greetings from HelloJob!");
        await context.Scheduler.UnscheduleJob(context.Trigger.Key);
    }
}
