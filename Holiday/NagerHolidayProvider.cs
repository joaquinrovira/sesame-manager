using System.Text.Json;

[Service(ServiceLifetime.Singleton)]
public record class NagerHolidayProvider(ILogger<NagerHolidayProvider> Logger, IHttpClientFactory HttpClientFactory) : IHolidayProvider
{
    HttpClient HttpClient = HttpClientFactory.CreateClient<NagerHolidayProvider>();
    
    public Result<ISet<DateTimeOffset>> Retrieve(int Year) => RetrieveAsync(Year).GetAwaiter().GetResult();
    async public Task<Result<ISet<DateTimeOffset>>> RetrieveAsync(int Year) {
        // Request data
        var response = await HttpClient.GetAsync($"https://date.nager.at/api/v3/publicholidays/{Year}/ES");
        if (!response.IsSuccessStatusCode) {
            Logger.LogWarning($"error fetching data from https://date.nager.at");
            var body = new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEnd();
            return Result.Failure<ISet<DateTimeOffset>>(body);
        }

        // Parse data
        using var contentStream = response.Content.ReadAsStream();
        var result = (await JsonSerializer.DeserializeAsync<HolidayResult[]>(contentStream, new JsonSerializerOptions(){ PropertyNameCaseInsensitive = true })).AsMaybe();
        
        // Filter data
        return result.ToResult("error desderalizing response from https://date.nager.at")
                    .Map( days => days
                        .Where(d => d.Global || (d.Counties?.Contains("ES-VC") ?? false))
                        .Select(d => new DateTimeOffset(d.Date.ToLocalTime().Date))
                        .ToHashSet() as ISet<DateTimeOffset>
                    );
    }
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