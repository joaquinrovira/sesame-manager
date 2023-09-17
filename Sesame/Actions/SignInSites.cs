namespace SesameApi;

internal class SignInSites : ApiActionOnlyResponse<DataResponse<SiteInfo[]>>
{
    public SignInSites(HttpClient client, Sesame.SesameContext context) : base(client, context) { }

    Maybe<string> UserId = Maybe.None;
    protected override string Path() => $"/api/v3/employees/{UserId.Value}/assigned-work-check-types";
    public Task<Result<DataResponse<SiteInfo[]>, Error>> Do(string userId)
    {
        UserId = userId;
        return Do(HttpMethod.Get);
    }
}
public record class SiteInfo(string id, string name) { }
