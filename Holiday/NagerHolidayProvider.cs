using System.Text.Json;

[Service(ServiceLifetime.Singleton)]
public record class NagerHolidayProvider(ILogger<NagerHolidayProvider> Logger, IHttpClientFactory HttpClientFactory) : IHolidayProvider
{
    HttpClient HttpClient = HttpClientFactory.CreateClient<NagerHolidayProvider>();

    async public Task<Result<ISet<YearDay>, Error>> RetrieveAsync(int Year)
    {
        // Request data
        var response = await HttpClient.GetAsync($"https://date.nager.at/api/v3/publicholidays/{Year}/ES");
        if (!response.IsSuccessStatusCode)
        {
            Logger.LogWarning($"error fetching data from https://date.nager.at");
            var body = new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEnd();
            return Result.Failure<ISet<YearDay>, Error>(new(body));
        }

        // Parse data
        using var contentStream = response.Content.ReadAsStream();
        var result = (await JsonSerializer.DeserializeAsync<HolidayResult[]>(contentStream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })).AsMaybe();

        // Filter data
        return result.ToResult(new Error("error desderalizing response from https://date.nager.at"))
                    .Map(days => days
                        .Where(d => d.Global || (d.Counties?.Contains("ES-VC") ?? false))
                        .Select(d => new YearDay() { Day = d.Date.Day, Month = d.Date.Month })
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
)
{ }