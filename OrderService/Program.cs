using Microsoft.Extensions.Options;
using OrderService.Clients;
using OrderService.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var services = builder.Services;


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

services.AddHttpClient<IInventoryClient, InventoryClient>().ConfigureHttpClient(
    (serviceProvider, httpClient) =>
    {
        var httpClientOptions = serviceProvider.GetRequiredService<InventoryClientOptions>();

        httpClient.BaseAddress = httpClientOptions.BaseAddress;
        httpClient.Timeout = httpClientOptions.Timeout;
    });

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