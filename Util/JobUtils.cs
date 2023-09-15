public static class JobUtils
{
    public static JobKey Of<T>(string? group = null) {
        return new JobKey(typeof(T).Name, group);
    }
}
