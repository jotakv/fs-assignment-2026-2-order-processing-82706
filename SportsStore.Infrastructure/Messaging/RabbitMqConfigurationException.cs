namespace SportsStore.Infrastructure.Messaging;

public sealed class RabbitMqConfigurationException : InvalidOperationException
{
    public RabbitMqConfigurationException(string message)
        : base(message)
    {
    }
}

