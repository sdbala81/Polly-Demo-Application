using OrderService;
using Serilog;

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

services.ConfigurePollyWithHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();

app.Run();