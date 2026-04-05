using RabbitMQ.Client;

namespace SportsStore.Infrastructure.Messaging;

public interface IRabbitMqConnectionFactoryProvider
{
    RabbitMqConnectionContext Create(string clientProvidedName);
}

public sealed record RabbitMqConnectionContext(ConnectionFactory Factory, RabbitMqConnectionInfo ConnectionInfo);

public sealed record RabbitMqConnectionInfo(
    string HostName,
    int Port,
    string VirtualHost,
    bool UseTls,
    string Source);

