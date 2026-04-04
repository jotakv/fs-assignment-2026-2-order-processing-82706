using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SportsStore.Infrastructure;
using SportsStore.Infrastructure.Messaging;
using SportsStore.Infrastructure.Options;

namespace SportsStore.Tests.Infrastructure;

public class RabbitMqConnectionFactoryProviderTests
{
    [Fact]
    public void Create_UsesAmqpsUriAndEnablesTls()
    {
        var provider = CreateProvider(new RabbitMqOptions
        {
            Uri = "amqps://brugqmwu:gziF90imcO6E--xEWP8QBRqWfbBxvlQM@seal.lmq.cloudamqp.com/brugqmwu"
        });

        RabbitMqConnectionContext context = provider.Create("sportsstore.test-uri");

        Assert.Equal("seal.lmq.cloudamqp.com", context.ConnectionInfo.HostName);
        Assert.Equal(5671, context.ConnectionInfo.Port);
        Assert.Equal("brugqmwu", context.ConnectionInfo.VirtualHost);
        Assert.True(context.ConnectionInfo.UseTls);
        Assert.Equal("Uri", context.ConnectionInfo.Source);
        Assert.Equal("sportsstore.test-uri", context.Factory.ClientProvidedName);
        Assert.True(context.Factory.AutomaticRecoveryEnabled);
        Assert.True(context.Factory.DispatchConsumersAsync);
        Assert.True(context.Factory.Ssl.Enabled);
    }

    [Fact]
    public void Create_UsesExplicitFields_WhenUriIsMissing()
    {
        var provider = CreateProvider(new RabbitMqOptions
        {
            HostName = "seal.lmq.cloudamqp.com",
            Port = 5671,
            UserName = "brugqmwu",
            Password = "secret",
            VirtualHost = "brugqmwu",
            UseTls = true
        });

        RabbitMqConnectionContext context = provider.Create("sportsstore.test-fields");

        Assert.Equal("seal.lmq.cloudamqp.com", context.ConnectionInfo.HostName);
        Assert.Equal(5671, context.ConnectionInfo.Port);
        Assert.Equal("brugqmwu", context.ConnectionInfo.VirtualHost);
        Assert.True(context.ConnectionInfo.UseTls);
        Assert.Equal("Fields", context.ConnectionInfo.Source);
        Assert.Equal("sportsstore.test-fields", context.Factory.ClientProvidedName);
        Assert.Equal("seal.lmq.cloudamqp.com", context.Factory.HostName);
        Assert.Equal(5671, context.Factory.Port);
        Assert.Equal("brugqmwu", context.Factory.VirtualHost);
        Assert.True(context.Factory.Ssl.Enabled);
    }

    [Fact]
    public void Create_ThrowsClearException_WhenRequiredFieldsAreMissing()
    {
        var provider = CreateProvider(new RabbitMqOptions());

        RabbitMqConfigurationException exception = Assert.Throws<RabbitMqConfigurationException>(
            () => provider.Create("sportsstore.test-missing"));

        Assert.Contains("RabbitMq:HostName", exception.Message);
        Assert.Contains("RabbitMq:Password", exception.Message);
        Assert.Contains("RabbitMq:VirtualHost", exception.Message);
    }

    [Fact]
    public void AddInfrastructure_BindsRabbitMqOptionsFromConfiguration()
    {
        Dictionary<string, string?> settings = new()
        {
            ["ConnectionStrings:SportsStoreConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=SportsStore;MultipleActiveResultSets=true",
            ["ConnectionStrings:IdentityConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=Identity;MultipleActiveResultSets=true",
            ["RabbitMq:HostName"] = "seal.lmq.cloudamqp.com",
            ["RabbitMq:Port"] = "5671",
            ["RabbitMq:UserName"] = "brugqmwu",
            ["RabbitMq:Password"] = "secret",
            ["RabbitMq:VirtualHost"] = "brugqmwu",
            ["RabbitMq:UseTls"] = "true",
            ["RabbitMq:ExchangeName"] = "sportsstore.orders"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();
        RabbitMqOptions options = provider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

        Assert.Equal("seal.lmq.cloudamqp.com", options.HostName);
        Assert.Equal(5671, options.Port);
        Assert.Equal("brugqmwu", options.UserName);
        Assert.Equal("secret", options.Password);
        Assert.Equal("brugqmwu", options.VirtualHost);
        Assert.True(options.UseTls);
        Assert.Equal("sportsstore.orders", options.ExchangeName);
    }

    private static RabbitMqConnectionFactoryProvider CreateProvider(RabbitMqOptions options)
    {
        return new RabbitMqConnectionFactoryProvider(Options.Create(options));
    }
}
