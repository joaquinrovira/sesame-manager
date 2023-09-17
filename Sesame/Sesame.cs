namespace SesameApi;

public class Sesame : IDisposable {
    SesameContext Context;
    HttpClientHandler HttpClientHandler;

    public Sesame(string email, string password, string baseUrl = "https://back-eu1.sesametime.com")
    {
        Context = new(email, password, baseUrl.TrimEnd('/'));
        HttpClientHandler = new()
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = System.Net.DecompressionMethods.All,
            CookieContainer = new(){},
            UseCookies = true,
        };
    }

    public Task<UnitResult<Error>> Login() => new Login(HttpClient(), Context).Do();
    public Task<Result<UserInfo[], Error>> CurrentUserInfo() => new CurrentUser(HttpClient(), Context).Do().Map(e => e.Data);
    public Task<Result<SiteInfo[], Error>> SignInSites(string userId) => new SignInSites(HttpClient(), Context).Do(userId).Map(e => e.Data);
    public Task<UnitResult<Error>> CheckIn(UserInfo u, Maybe<SiteInfo> s) => CheckIn(u.id, s.Map(s=> s.id));
    public Task<UnitResult<Error>> CheckIn(string userId, Maybe<string> siteId) => new CheckIn(HttpClient(), Context).Do(userId, siteId);
    public Task<UnitResult<Error>> CheckOut(UserInfo u, Maybe<SiteInfo> s) => CheckOut(u.id, s.Map(s=> s.id));
    public Task<UnitResult<Error>> CheckOut(string userId, Maybe<string> siteId) => new CheckOut(HttpClient(), Context).Do(userId, siteId);
    public void Dispose() { HttpClientHandler.Dispose(); }
    
    public static readonly string[] AndroidVersions = { "10", "11", "12", "13" };
    public static readonly string[] FirefoxVersions = { "117.0.1", "116.0.3", "115.2.1", "114.0.2" };
    private string UserAgent() {
        var AndroidVersion = AndroidVersions[Random.Shared.Next(AndroidVersions.Length)];
        var FirefoxVersion = FirefoxVersions[Random.Shared.Next(FirefoxVersions.Length)];
        return $"Mozilla/5.0 (Android {AndroidVersion}; Mobile; rv:{FirefoxVersion}) Gecko/{FirefoxVersion} Firefox/{FirefoxVersion}";
    }
    private string RSRC() => (Random.Shared.Next(10) + 20).ToString();

    HttpClient HttpClient() {
        var client = new HttpClient(HttpClientHandler) {};
        client.DefaultRequestHeaders.Add("User-Agent", UserAgent());
        client.DefaultRequestHeaders.Add("Accept","application/json");
        client.DefaultRequestHeaders.Add("Accept-Language","en-GB,en;q=0.5");
        client.DefaultRequestHeaders.Add("Accept-Encoding","gzip, deflate, br");
        client.DefaultRequestHeaders.Add("rsrc",RSRC());
        client.DefaultRequestHeaders.Add("traceparent", Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add("Origin","https://app.sesametime.com");
        client.DefaultRequestHeaders.Add("Referer","https://app.sesametime.com/");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Dest","empty");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Mode","cors");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Site","same-site");
        client.DefaultRequestHeaders.Add("TE","trailers");
        return client;
    }
    internal record SesameContext(
        string Email, 
        string Password,
        string BaseUrl
    ){}
}
