public interface IHolidayProvider {
    Result<IReadOnlySet<DateTime>> Retrieve(int year);
    Task<Result<IReadOnlySet<DateTime>>> RetrieveAsync(int year);
}