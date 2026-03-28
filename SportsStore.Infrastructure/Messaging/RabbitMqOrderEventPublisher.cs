using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SportsStore.Application.Abstractions.Messaging;
using SportsStore.Application.Contracts.Messaging;
using SportsStore.Infrastructure.Options;

namespace SportsStore.Infrastructure.Messaging;

public sealed class RabbitMqOrderEventPublisher : IOrderEventPublisher
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqOrderEventPublisher> _logger;

    public RabbitMqOrderEventPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqOrderEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task PublishOrderSubmittedAsync(OrderSubmittedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        using IConnection connection = factory.CreateConnection();
        using IModel channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null);

        IBasicProperties properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.CorrelationId = integrationEvent.CorrelationId;
        properties.Type = nameof(OrderSubmittedIntegrationEvent);

        byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(integrationEvent));

        _logger.LogInformation(
            "Publishing {EventType} for OrderId={OrderId}, CustomerId={CustomerId}, CorrelationId={CorrelationId}, Exchange={Exchange}, RoutingKey={RoutingKey}",
            nameof(OrderSubmittedIntegrationEvent),
            integrationEvent.OrderId,
            integrationEvent.CustomerId,
            integrationEvent.CorrelationId,
            _options.ExchangeName,
            _options.OrderSubmittedRoutingKey);

        channel.BasicPublish(
            exchange: _options.ExchangeName,
            routingKey: _options.OrderSubmittedRoutingKey,
            basicProperties: properties,
            body: payload);

        return Task.CompletedTask;
    }
}
