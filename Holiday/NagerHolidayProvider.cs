using System.Text.Json;

[Service(ServiceLifetime.Singleton)]
public record class NagerHolidayProvider(ILogger<NagerHolidayProvider> Logger, IHttpClientFactory HttpClientFactory) : IHolidayProvider
{
    HttpClient HttpClient = HttpClientFactory.CreateClient<NagerHolidayProvider>();
    
    public Result<ISet<YearDay>> Retrieve(int Year) => RetrieveAsync(Year).GetAwaiter().GetResult();
    async public Task<Result<ISet<YearDay>>> RetrieveAsync(int Year) {
        // Request data
        var response = await HttpClient.GetAsync($"https://date.nager.at/api/v3/publicholidays/{Year}/ES");
        if (!response.IsSuccessStatusCode) {
            Logger.LogWarning($"error fetching data from https://date.nager.at");
            var body = new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEnd();
            return Result.Failure<ISet<YearDay>>(body);
        }

        // Parse data
        using var contentStream = response.Content.ReadAsStream();
        var result = (await JsonSerializer.DeserializeAsync<HolidayResult[]>(contentStream, new JsonSerializerOptions(){ PropertyNameCaseInsensitive = true })).AsMaybe();
        
        // Filter data
        return result.ToResult("error desderalizing response from https://date.nager.at")
                    .Map( days => days
                        .Where(d => d.Global || (d.Counties?.Contains("ES-VC") ?? false))
                        .Select(d => {
                            var date = d.Date.ToLocalTime().Date;
                            return new YearDay() {Day = date.Day, Month = date.Month};
                        })
                        .ToHashSet() as ISet<YearDay>
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