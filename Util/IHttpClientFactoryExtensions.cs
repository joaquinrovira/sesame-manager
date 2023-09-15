namespace System.Net.Http;

public static class IHttpClientFactoryExtensions 
{
    public static HttpClient CreateClient<T>(this IHttpClientFactory factory) 
        => factory.CreateClient(typeof(NagerHolidayProvider).ToString());
}