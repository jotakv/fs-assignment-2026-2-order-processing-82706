using Serilog;
using SportsStore.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<InventoryOrderSubmittedWorker>();
builder.Services.AddScoped<IInventoryDecisionService, InventoryDecisionService>();

var host = builder.Build();
host.Run();
