using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportsStore.Application.Abstractions.Caching;
using SportsStore.Application.Abstractions.Checkout;
using SportsStore.Application.Abstractions.Messaging;
using SportsStore.Application.Abstractions.Payments;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Infrastructure.Caching;
using SportsStore.Infrastructure.Checkout;
using SportsStore.Infrastructure.Messaging;
using SportsStore.Infrastructure.Options;
using SportsStore.Infrastructure.Payments;
using SportsStore.Infrastructure.Persistence;
using SportsStore.Infrastructure.Persistence.Repositories;

namespace SportsStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.Configure<ClientAppOptions>(configuration.GetSection(ClientAppOptions.SectionName));
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddDbContext<StoreDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SportsStoreConnection")));

        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("IdentityConnection")));

        services.AddScoped<IProductRepository, EfProductRepository>();
        services.AddScoped<IOrderRepository, EfOrderRepository>();
        services.AddSingleton<ICatalogCache, MemoryCatalogCache>();
        services.AddSingleton<IPendingCheckoutStore, InMemoryPendingCheckoutStore>();
        services.AddSingleton<ICheckoutUrlFactory, ConfigurationCheckoutUrlFactory>();
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<IOrderEventPublisher, RabbitMqOrderEventPublisher>();
        services.AddScoped<IInventoryEventPublisher, RabbitMqInventoryEventPublisher>();

        return services;
    }
}
