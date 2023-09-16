public interface IHolidayProvider {
    Result<ISet<DateTime>> Retrieve(int year);
    Task<Result<ISet<DateTime>>> RetrieveAsync(int year);
}