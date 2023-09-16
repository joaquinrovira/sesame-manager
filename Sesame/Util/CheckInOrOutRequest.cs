namespace SesameApi;

public record class CheckInOrOutRequest(object coordinates, string origin, string? workCheckTypeId) {
    public static CheckInOrOutRequest From(Maybe<string> siteId) => new CheckInOrOutRequest(new(), "web", siteId.GetValueOrDefault());
}