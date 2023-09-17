public interface IHolidayProvider {
    Result<ISet<YearDay>> Retrieve(int year);
    Task<Result<ISet<YearDay>>> RetrieveAsync(int year);
}