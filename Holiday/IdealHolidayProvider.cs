using HtmlAgilityPack;

[Service(ServiceLifetime.Singleton)]
public record class IdealHolidayProvider(ILogger<IdealHolidayProvider> Logger, IHttpClientFactory HttpClientFactory) : IHolidayProvider
{
    HttpClient HttpClient = HttpClientFactory.CreateClient<IdealHolidayProvider>();

    async public Task<Result<ISet<YearDay>, Error>> RetrieveAsync(int Year)
    {
        // Request data
        var response = await HttpClient.GetAsync($"https://calendarios.ideal.es/laboral/comunidad-valenciana/valencia/valencia/{Year}");
        if (!response.IsSuccessStatusCode)
        {
            Logger.LogWarning($"error fetching data from https://calendarios.ideal.es");
            var body = new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEnd();
            return Result.Failure<ISet<YearDay>, Error>(new(body));
        }

        // Parse data

        return Functional.Catch(() =>
        {
            ISet<YearDay> Data = new HashSet<YearDay>();
            var DOM = new HtmlDocument();
            DOM.Load(response.Content.ReadAsStream());

            var month = 0;
            // Select all month tables from the jumbotron-calendar
            var MonthNodes = DOM.DocumentNode
                .SelectSingleNode(".//div[contains(@class, 'jumbotron-calendar')]")
                .SelectNodes(".//table[contains(@class, 'bm-calendar')]");

            foreach (var MonthNode in MonthNodes)
            {
                month++;
                // Select: <td title="<SomeTitle>" />
                var HolidayNodes = MonthNode.SelectNodes(".//td[@title]");
                if (HolidayNodes is null) continue;
                foreach (var HolidayNode in HolidayNodes)
                {
                    // Inner text contains the day value
                    var day = int.Parse(HolidayNode.InnerText);
                    Data.Add(new() { Month = month, Day = day });
                }
            }

            return Data;
        });
    }
}