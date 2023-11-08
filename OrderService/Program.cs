using Polly;
using Polly.Extensions.Http;

using Serilog;

namespace OrderService;

public static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Starting up Inventory Service.....");

        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog(
            (hostBuilderContext, services, loggerConfiguration) => loggerConfiguration.ReadFrom
                .Configuration(hostBuilderContext.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());

        // Add services to the container.
        var services = builder.Services;

        services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError() // HttpRequestException, 5XX and 408  
            .RetryAsync(1);

        var fallbackPolicy = HttpPolicyExtensions.HandleTransientHttpError() // HttpRequestException, 5XX and 408  
            .FallbackAsync(FallbackAction, OnFallbackAsync)
            .WrapAsync(retryPolicy);

        services.AddHttpClient(
                "InventoryService",
                client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                })
            .AddPolicyHandler(fallbackPolicy);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();

        app.Run();

        return;

        Task<HttpResponseMessage> FallbackAction(DelegateResult<HttpResponseMessage> responseToFailedRequest,
            Context context,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("Fallback action is executing");

            var httpResponseMessage = new HttpResponseMessage(responseToFailedRequest.Result.StatusCode)
            {
                Content = new StringContent($"The fallback executed, the original error was {responseToFailedRequest.Result.ReasonPhrase}")
            };

            return Task.FromResult(httpResponseMessage);
        }

        Task OnFallbackAsync(DelegateResult<HttpResponseMessage> response, Context context)
        {
            Console.WriteLine("About to call the fallback action. This is a good place to do some logging");

            return Task.CompletedTask;
        }
    }
}
