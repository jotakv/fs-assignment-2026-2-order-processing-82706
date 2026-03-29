using Serilog;
using SportsStore.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({ServiceName}) CorrelationId={CorrelationId} OrderId={OrderId} EventType={EventType} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Services.AddSerilog();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<PaymentInventoryConfirmedWorker>();
builder.Services.AddScoped<IPaymentDecisionService, PaymentDecisionService>();

var host = builder.Build();
host.Run();
