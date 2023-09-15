using Microsoft.Extensions.Options;
using OrderService.Clients;
using OrderService.Options;
using Polly;
using Polly.Extensions.Http;
using Serilog;


Log.Logger = new LoggerConfiguration().Enrich.FromLogContext()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up Inventory Service.....");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (hostBuilderContext, services, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostBuilderContext.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

// Add services to the container.
var services = builder.Services;


// Create the retry policy we want
var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError() // HttpRequestException, 5XX and 408  
    .WaitAndRetryAsync(4, retryAttempts => TimeSpan.FromSeconds(Math.Pow(5, retryAttempts)/2));

// Register the InventoryClient with Polly policies
services.AddHttpClient<IInventoryClient, InventoryClient>().ConfigureHttpClient(
    (serviceProvider, httpClient) =>
    {
        var httpClientOptions = serviceProvider.GetRequiredService<InventoryClientOptions>();

        httpClient.BaseAddress = httpClientOptions.BaseAddress;
        httpClient.Timeout = httpClientOptions.Timeout;
        
    }).AddPolicyHandler(retryPolicy);

services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddOptions<InventoryClientOptions>()
    .BindConfiguration("CatalogClient")
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddSingleton(
    resolver => resolver.GetRequiredService<IOptions<InventoryClientOptions>>()
        .Value);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();