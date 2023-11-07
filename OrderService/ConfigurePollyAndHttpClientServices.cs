using Polly;

namespace OrderService;

public static class ConfigurePollyAndHttpClientServices
{
    private static readonly IAsyncPolicy<HttpResponseMessage> fallbackPolicy =
        Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .FallbackAsync(FallbackAction, OnFallbackAsync);


    private static readonly IAsyncPolicy<HttpResponseMessage> httpWaitAndRetryPolicy =
        Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .RetryAsync(1);

    private static readonly IAsyncPolicy<HttpResponseMessage> wrapOfRetryAndFallback =
        Policy.WrapAsync(fallbackPolicy, httpWaitAndRetryPolicy);


    public static void ConfigurePollyWithHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient("InventoryService", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }).AddPolicyHandler(wrapOfRetryAndFallback);
    }


    private static Task OnFallbackAsync(DelegateResult<HttpResponseMessage> response, Context context)
    {
        Console.WriteLine("About to call the fallback action. This is a good place to do some logging");
        return Task.CompletedTask;
    }

    private static Task<HttpResponseMessage> FallbackAction(DelegateResult<HttpResponseMessage> responseToFailedRequest,
        Context context, CancellationToken cancellationToken)
    {
        Console.WriteLine("Fallback action is executing");

        var httpResponseMessage = new HttpResponseMessage(responseToFailedRequest.Result.StatusCode)
        {
            Content = new StringContent(
                $"The fallback executed, the original error was {responseToFailedRequest.Result.ReasonPhrase}")
        };
        return Task.FromResult(httpResponseMessage);
    }
}