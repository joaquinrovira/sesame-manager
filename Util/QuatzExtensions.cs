using Quartz.Impl.Matchers;

public static class QuartzExtensions
{
    public static async IAsyncEnumerable<(IJobDetail, ITrigger)> ScheduledJobTriggers(this IScheduler scheduler, Func<IJobDetail, bool>? filter = null) {
        foreach (var group in await scheduler.GetJobGroupNames())
        {
            var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
            foreach (var jobKey in await scheduler.GetJobKeys(groupMatcher))
            {
                var detail = await scheduler.GetJobDetail(jobKey);
                if (detail is null) continue;
                if (filter is not null && !filter(detail)) continue;

                var triggers = await scheduler.GetTriggersOfJob(jobKey);
                foreach (ITrigger trigger in triggers)
                {
                    yield return (detail, trigger);
                }
            }
        }
    }
}