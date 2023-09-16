namespace CSharpFunctionalExtensions;

public static class MoreCSharpFunctionalExtensions {
    public static UnitResult<E> ToUnitResult<T,E>(this Result<T,E> r) {
        if (r.IsFailure) return UnitResult.Failure(r.Error);
        return UnitResult.Success<E>();
    } 
    public static async Task<UnitResult<E>> ToUnitResult<T,E>(this Task<Result<T,E>> r) => ToUnitResult(await r);
    
    public static UnitResult<string> ToUnitResult<T>(this Result<T> r) {
        if (r.IsFailure) return UnitResult.Failure(r.Error);
        return UnitResult.Success<string>();
    }
    public static async Task<UnitResult<string>> ToUnitResult<T>(this Task<Result<T>> r) => ToUnitResult(await r);

    public static void SuccessOrThrow(this UnitResult<string> r) 
    {
        if(r.IsFailure) throw new Exception(r.Error);
        return;
    }
    public static async Task SuccessOrThrow(this Task<UnitResult<string>> r) => SuccessOrThrow(await r);
}