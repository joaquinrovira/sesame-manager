public interface IHolidayProvider
{
    Result<ISet<YearDay>, Error> Retrieve(int year) => RetrieveAsync(year).GetAwaiter().GetResult();
    Task<Result<ISet<YearDay>, Error>> RetrieveAsync(int year);
}