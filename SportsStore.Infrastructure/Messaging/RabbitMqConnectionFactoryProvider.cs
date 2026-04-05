using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using SportsStore.Infrastructure.Options;

namespace SportsStore.Infrastructure.Messaging;

public sealed class RabbitMqConnectionFactoryProvider : IRabbitMqConnectionFactoryProvider
{
    private const int DefaultAmqpPort = 5672;
    private const int DefaultAmqpsPort = 5671;
    private const string AmqpScheme = "amqp";
    private const string AmqpsScheme = "amqps";

    private readonly RabbitMqOptions _options;

    public RabbitMqConnectionFactoryProvider(Microsoft.Extensions.Options.IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public RabbitMqConnectionContext Create(string clientProvidedName)
    {
        if (!string.IsNullOrWhiteSpace(_options.Uri))
        {
            return CreateFromUri(clientProvidedName);
        }

        return CreateFromFields(clientProvidedName);
    }

    private RabbitMqConnectionContext CreateFromUri(string clientProvidedName)
    {
        if (!System.Uri.TryCreate(_options.Uri, UriKind.Absolute, out Uri? uri))
        {
            throw new RabbitMqConfigurationException(
                $"RabbitMQ configuration is invalid. '{RabbitMqOptions.SectionName}:Uri' must be a valid absolute AMQP URI.");
        }

        string scheme = uri.Scheme.ToLowerInvariant();
        if (scheme is not AmqpScheme and not AmqpsScheme)
        {
            throw new RabbitMqConfigurationException(
                $"RabbitMQ configuration is invalid. '{RabbitMqOptions.SectionName}:Uri' must use the '{AmqpScheme}' or '{AmqpsScheme}' scheme.");
        }

        bool useTls = _options.UseTls || scheme == AmqpsScheme;
        int port = ResolvePort(uri.Port, useTls);
        string virtualHost = ResolveVirtualHost(uri.AbsolutePath, _options.VirtualHost);

        var factory = new ConnectionFactory
        {
            Uri = uri,
            Port = port,
            VirtualHost = virtualHost,
            ClientProvidedName = clientProvidedName,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            DispatchConsumersAsync = true
        };

        ConfigureTls(factory, uri.Host, useTls);

        return new RabbitMqConnectionContext(
            factory,
            new RabbitMqConnectionInfo(uri.Host, port, virtualHost, useTls, "Uri"));
    }

    private RabbitMqConnectionContext CreateFromFields(string clientProvidedName)
    {
        string[] missingSettings =
        [
            string.IsNullOrWhiteSpace(_options.HostName) ? $"{RabbitMqOptions.SectionName}:HostName" : string.Empty,
            _options.Port <= 0 ? $"{RabbitMqOptions.SectionName}:Port" : string.Empty,
            string.IsNullOrWhiteSpace(_options.UserName) ? $"{RabbitMqOptions.SectionName}:UserName" : string.Empty,
            string.IsNullOrWhiteSpace(_options.Password) ? $"{RabbitMqOptions.SectionName}:Password" : string.Empty,
            string.IsNullOrWhiteSpace(_options.VirtualHost) ? $"{RabbitMqOptions.SectionName}:VirtualHost" : string.Empty
        ];

        string[] failures = missingSettings.Where(setting => !string.IsNullOrWhiteSpace(setting)).ToArray();
        if (failures.Length > 0)
        {
            throw new RabbitMqConfigurationException(
                $"RabbitMQ configuration is missing required values. Configure '{RabbitMqOptions.SectionName}:Uri' or set all of these fields: {string.Join(", ", failures)}.");
        }

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            ClientProvidedName = clientProvidedName,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            DispatchConsumersAsync = true
        };

        ConfigureTls(factory, _options.HostName, _options.UseTls);

        return new RabbitMqConnectionContext(
            factory,
            new RabbitMqConnectionInfo(_options.HostName, _options.Port, _options.VirtualHost, _options.UseTls, "Fields"));
    }

    private static void ConfigureTls(ConnectionFactory factory, string hostName, bool useTls)
    {
        if (!useTls)
        {
            return;
        }

        factory.Ssl = new SslOption
        {
            Enabled = true,
            ServerName = hostName
        };
    }

    private static int ResolvePort(int port, bool useTls)
    {
        if (port > 0)
        {
            return port;
        }

        return useTls ? DefaultAmqpsPort : DefaultAmqpPort;
    }

    private static string ResolveVirtualHost(string absolutePath, string configuredVirtualHost)
    {
        string uriVirtualHost = absolutePath.Trim('/');
        if (!string.IsNullOrWhiteSpace(uriVirtualHost))
        {
            return Uri.UnescapeDataString(uriVirtualHost);
        }

        if (!string.IsNullOrWhiteSpace(configuredVirtualHost))
        {
            return configuredVirtualHost;
        }

        return "/";
    }
}
