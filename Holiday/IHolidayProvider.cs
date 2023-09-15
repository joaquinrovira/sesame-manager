public interface IHolidayProvider {
    Result<ISet<DateTime>> Retrieve(int? Year = null);
    Task<Result<ISet<DateTime>>> RetrieveAsync(int? Year = null);
}