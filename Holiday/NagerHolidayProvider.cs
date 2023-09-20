using System.Text.Json;

[Service(ServiceLifetime.Singleton)]
public record class NagerHolidayProvider(
    ILogger<NagerHolidayProvider> Logger, 
    IHttpClientFactory HttpClientFactory,
    IOptions<NagerHolidayProviderConfiguration> Config
) : IHolidayProvider
{
    HttpClient HttpClient = HttpClientFactory.CreateClient<NagerHolidayProvider>();

    string CountryCode => Config.Value.CountryCode.ToUpperInvariant();
    Maybe<string> CountyCode => (Config.Value.CountyCode?.ToUpperInvariant()).AsMaybe();

    async public Task<Result<ISet<YearDay>, Error>> RetrieveAsync(int Year)
    {
        // Request data
        var response = await HttpClient.GetAsync($"https://date.nager.at/api/v3/publicholidays/{Year}/{CountryCode}");
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
                        .Where(d => d.Global || IsInCounty(d, CountyCode))
                        .Select(d => new YearDay() { Day = d.Date.Day, Month = d.Date.Month })
                        .ToHashSet() as ISet<YearDay>
                    );
    }

    private bool IsInCounty(HolidayResult value, Maybe<string> county)
        => county.Map(county => value.Counties?.Contains(county) ?? false).GetValueOrDefault(true);
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