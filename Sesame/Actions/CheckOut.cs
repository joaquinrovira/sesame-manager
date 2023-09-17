namespace SesameApi;

internal class CheckOut : ApiActionOnlyRequest<CheckInOrOutRequest>
{
    public CheckOut(HttpClient client, Sesame.SesameContext context) : base(client, context) { }

    Maybe<string> UserId = Maybe.None;
    protected override string Path() => $"/api/v3/employees/{UserId}/check-out";
    public Task<UnitResult<Error>> Do(string userId, Maybe<string> siteId)
    {
        UserId = userId;
        return Do(HttpMethod.Post, CheckInOrOutRequest.From(siteId));
    }
}