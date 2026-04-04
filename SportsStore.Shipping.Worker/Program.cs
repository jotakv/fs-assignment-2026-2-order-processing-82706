using Serilog;
using SportsStore.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "ShippingService")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({ServiceName}) CorrelationId={CorrelationId} OrderId={OrderId} EventType={EventType} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Services.AddSerilog();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<ShippingPaymentApprovedWorker>();
builder.Services.AddScoped<IShipmentCreationService, ShipmentCreationService>();

var host = builder.Build();

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Shipping worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
