namespace SesameApi;

internal class Login : ApiActionOnlyRequest<LoginRequest>
{
    public Login(HttpClient client, Sesame.SesameContext context) : base(client, context) { }

    protected override string Path() => "/api/v3/security/login";
    public Task<UnitResult<Error>> Do() => Do(HttpMethod.Post, new(Context.Email, Context.Password, new()));
}
internal record class LoginRequest(string email, string password, LoginRequestPlatformData platformData) { }
internal record class LoginRequestPlatformData(string platformName = "Firefox", string platformSystem = "Linux", string platformVersion = "102") { }