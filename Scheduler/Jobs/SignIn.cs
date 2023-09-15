public class SignIn : IJob
{
    public static readonly JobKey Key = JobUtils.Of<SignIn>();
    public async Task Execute(IJobExecutionContext context)
    {
        await Console.Out.WriteLineAsync("Greetings from HelloJob!");
        await context.Scheduler.UnscheduleJob(context.Trigger.Key);
    }
}
