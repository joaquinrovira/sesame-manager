namespace SesameApi;

internal class CurrentUser : ApiActionOnlyResponse<DataResponse<UserInfo[]>>
{
    public CurrentUser(HttpClient client, Sesame.SesameContext context) : base(client, context) { }

    protected override string Path() => "/api/v3/security/me";
    public Task<Result<DataResponse<UserInfo[]>, Error>> Do() => Do(HttpMethod.Get);
}
public record class UserInfo(string id) {}
