using Microsoft.Extensions.Options;
using SportsStore.Blazor.Components;
using SportsStore.Blazor.Services.Api;
using SportsStore.Blazor.Services.Browser;
using SportsStore.Blazor.Services.Cart;
using SportsStore.Blazor.Services.Orders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));

builder.Services.AddHttpClient<CatalogApiClient>((services, client) =>
{
    ApiOptions options = services.GetRequiredService<IOptions<ApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddHttpClient<OrdersApiClient>((services, client) =>
{
    ApiOptions options = services.GetRequiredService<IOptions<ApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddScoped<SessionStorageService>();
builder.Services.AddScoped<CartState>();
builder.Services.AddScoped<RecentOrdersState>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
