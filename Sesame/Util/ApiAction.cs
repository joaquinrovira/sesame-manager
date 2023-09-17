
using System.Net.Http.Json;
using System.Text.Json;

namespace SesameApi;


internal abstract class ApiActionOnlyResponse<TResponse> : BaseApiAction
{
    protected ApiActionOnlyResponse(HttpClient client, Sesame.SesameContext context) : base(client, context) { }

    protected async Task<Result<TResponse, Error>> Do(HttpMethod method)
    {
        var (result, response) = await Do(method, null, typeof(TResponse));
        if (result.IsFailure) return result.Error;
        if (response is not TResponse value)
        {
            var error = new InvalidCastException($"unable to cast '{response?.GetType().Name}' into '{typeof(TResponse).Name}'");
            return Error.From(error);
        }
        return value;
    }
}

internal abstract class ApiActionOnlyRequest<TRequest> : BaseApiAction
{
    protected ApiActionOnlyRequest(HttpClient client, Sesame.SesameContext context) : base(client, context) { }

    protected async Task<UnitResult<Error>> Do(HttpMethod method, TRequest body)
    {
        var (result, _) = await Do(method, body, null);
        if (result.IsFailure) return result.Error;
        return UnitResult.Success<Error>();
    }
}

internal abstract class ApiAction<TRequest, TResponse> : BaseApiAction
{
    protected ApiAction(HttpClient client, Sesame.SesameContext context) : base(client, context) { }

    protected async Task<Result<TResponse, Error>> Do(HttpMethod method, TRequest body)
    {
        var (result, response) = await Do(method, body, typeof(TResponse));
        if (result.IsFailure) return result.Error;
        if (response is not TResponse value)
        {
            var error = new InvalidCastException($"unable to cast '{response?.GetType().Name}' into '{typeof(TResponse).Name}'");
            return Error.From(error);
        }
        return value;
    }
}

internal abstract class BaseApiAction : IDisposable
{
    protected HttpClient Client;
    protected Sesame.SesameContext Context;
    protected abstract string Path();
    public void Dispose() { Client.Dispose(); }

    protected BaseApiAction(HttpClient client, Sesame.SesameContext context)
    {
        Client = client;
        Context = context;
    }


    protected async Task<(UnitResult<Error>, object?)> Do(HttpMethod method, object? body, Type? responseType)
    {
        // Build message
        var request = new HttpRequestMessage(method, $"{Context.BaseUrl}/{Path().TrimStart('/')}");
        if (body is not null) request.Content = JsonContent.Create(body);

        // Send message
        var response = await Client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var repsonseBody = new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEnd();
            return (UnitResult.Failure(new Error(repsonseBody)), null);
        }

        // No response message expected
        if (responseType is null) return (UnitResult.Success<Error>(), null);


        try
        {
            using var contentStream = response.Content.ReadAsStream();
            var value = await JsonSerializer.DeserializeAsync(contentStream, responseType, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            return (UnitResult.Success<Error>(), value);
        }
        catch (Exception e)
        {
            return (UnitResult.Failure(new Error($"error desderalizing {GetType()} response as {responseType}", e)), null);
        }

    }
}
