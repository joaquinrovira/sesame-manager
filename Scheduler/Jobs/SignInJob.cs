public class SignInJob : IJob
{
    public static readonly JobKey Key = JobUtils.Of<SignInJob>();
    public async Task Execute(IJobExecutionContext context)
    {
        await Console.Out.WriteLineAsync("Greetings from HelloJob!");
        await context.Scheduler.UnscheduleJob(context.Trigger.Key);
    }
}
