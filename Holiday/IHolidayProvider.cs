public interface IHolidayProvider {
    Result<ISet<DateTimeOffset>> Retrieve(int year);
    Task<Result<ISet<DateTimeOffset>>> RetrieveAsync(int year);
}