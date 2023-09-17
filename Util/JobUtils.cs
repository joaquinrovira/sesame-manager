public static class JobUtils
{
    public static JobKey Of<T>(string? group = null)
    {
        var name = typeof(T).Name;
        return new JobKey(name, group ?? name);
    }
}
