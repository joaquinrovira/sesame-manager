using System.Net.Http.Json;

[Service(ServiceLifetime.Transient)]
public record class LogInRequest(
    ILogger<LogInRequest> Logger,
    IHttpClientFactory HttpClientFactory,
    IOptions<GeneralConfig> GeneralConfig
)
{
    HttpClient HttpClient = HttpClientFactory.CreateClient<LogInRequest>();

    async public Task<UnitResult<string>> SignIn()
    {
        // Request data
        var request = new HttpRequestMessage(HttpMethod.Post, "https://back-eu1.sesametime.com/api/v3/security/login");
        SesameRequestConfig.Apply(request);
        request.Content = JsonContent.Create(
            new LogInRequestData(
                GeneralConfig.Value.Email!, 
                GeneralConfig.Value.Password!, 
                new("Firefox", "Linux", "102")
        ));

        var response = await HttpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            Logger.LogWarning($"Unable to log in");
            var body = new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEnd();
            return UnitResult.Failure(body);
        } else {
            Logger.LogInformation($"Successful login request");
        }

        return UnitResult.Success<string>();
    }
}

public record class LogInRequestData(
    string email, 
    string password, 
    PlatformData platformData
) {}

public record class PlatformData(
    string platformName,
    string platformSystem,
    string platformVersion
) {}
