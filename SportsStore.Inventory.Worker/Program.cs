using Serilog;
using SportsStore.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "InventoryService")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({ServiceName}) CorrelationId={CorrelationId} OrderId={OrderId} EventType={EventType} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Services.AddSerilog();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<InventoryOrderSubmittedWorker>();
builder.Services.AddScoped<IInventoryDecisionService, InventoryDecisionService>();

var host = builder.Build();

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Inventory worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
