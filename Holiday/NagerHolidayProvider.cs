using System.Text.Json;
using Microsoft.Extensions.Logging;

[Service(ServiceLifetime.Singleton)]
public record class NagerHolidayProvider(ILogger<NagerHolidayProvider> Logger, IHttpClientFactory HttpClientFactory) : IHolidayProvider
{
    HttpClient HttpClient = HttpClientFactory.CreateClient<NagerHolidayProvider>();
    async public Task<Result<ISet<DateTime>>> RetrieveAsync(int? Year = null) {
        Year ??= DateTime.Now.Year;
        var response = await HttpClient.GetAsync($"https://date.nager.at/api/v3/publicholidays/{Year}/ES");

        if (!response.IsSuccessStatusCode) {
            Logger.LogWarning($"error fetching data from https://date.nager.at");
            var body = new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEnd();
            return Result.Failure<ISet<DateTime>>(body);
        }

        using var contentStream =
                response.Content.ReadAsStream();
        
        var result = (await JsonSerializer.DeserializeAsync<HolidayResult[]>(contentStream, new JsonSerializerOptions(){ PropertyNameCaseInsensitive = true })).AsMaybe();
        
        return result.ToResult("error desderalizing response from https://date.nager.at")
                    .Map( days => days
                        .Where(d => d.Global || (d.Counties?.Contains("ES-VC") ?? false))
                        .Select(d => d.Date.Date)
                        .ToHashSet() as ISet<DateTime>
                    );
    }
    public Result<ISet<DateTime>> Retrieve(int? Year = null) => RetrieveAsync(Year).GetAwaiter().GetResult();
}


public record HolidayResult(
    DateTime Date,
    string LocalName,
    string Name,
    string CountryCode,
    bool Fixed,
    bool Global,
    string[]? Counties,
    int? LaunchYear 
) {}