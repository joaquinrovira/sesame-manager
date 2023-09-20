namespace CSharpFunctionalExtensions;

public static class MoreCSharpFunctionalExtensions
{
    public static void SuccessOrThrow<T>(this UnitResult<T> r) where T : Error
    {
        if (r.IsFailure) throw r.Error;
        return;
    }
    public static async Task SuccessOrThrow<T>(this Task<UnitResult<T>> r) where T : Error => SuccessOrThrow(await r);
    public static T GetOrThrow<T, E>(this Result<T, E> r) where E : Error
    {
        if (r.IsFailure) throw r.Error;
        return r.Value;
    }
    public static async Task GetOrThrow<T, E>(this Task<Result<T, E>> r) where E : Error => GetOrThrow(await r);
}

public static class Functional
{
    public static async Task<Result<T, E>> Catch<T, E>(this Func<Task<T>> fn)
        where E : Exception
    {
        try { return await fn(); }
        catch (E ex) { return ex; }
    }

#pragma warning disable CS1998
    public static Result<T, E> Catch<T, E>(this Func<T> fn) where E : Exception
        => Catch<T, E>(async () => fn()).GetAwaiter().GetResult();
#pragma warning restore CS1998
    public static Task<Result<T, Error>> Catch<T>(this Func<Task<T>> fn) => Catch<T, Exception>(fn).MapError(Error.From);
    public static Result<T, Error> Catch<T>(this Func<T> fn) => Catch<T, Exception>(fn).MapError(Error.From);
}