using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SportsStore.Application.Abstractions.Messaging;
using SportsStore.Application.Contracts.Messaging;
using SportsStore.Infrastructure.Options;

namespace SportsStore.Infrastructure.Messaging;

public sealed class RabbitMqShippingEventPublisher : IShippingEventPublisher
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqShippingEventPublisher> _logger;

    public RabbitMqShippingEventPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqShippingEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task PublishCreatedAsync(ShippingCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        PublishAsync(integrationEvent, _options.ShippingCreatedRoutingKey, cancellationToken);

    public Task PublishFailedAsync(ShippingFailedIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        PublishAsync(integrationEvent, _options.ShippingFailedRoutingKey, cancellationToken);

    private Task PublishAsync(object integrationEvent, string routingKey, CancellationToken cancellationToken)
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

        channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false, arguments: null);

        IBasicProperties properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.CorrelationId = (string?)integrationEvent.GetType().GetProperty("CorrelationId")?.GetValue(integrationEvent);
        properties.Type = integrationEvent.GetType().Name;

        byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(integrationEvent));

        _logger.LogInformation(
            "Publishing event. ServiceName={ServiceName}, EventType={EventType}, OrderId={OrderId}, CustomerId={CustomerId}, RoutingKey={RoutingKey}, CorrelationId={CorrelationId}",
            "ShippingPublisher",
            integrationEvent.GetType().Name,
            integrationEvent.GetType().GetProperty("OrderId")?.GetValue(integrationEvent),
            integrationEvent.GetType().GetProperty("CustomerId")?.GetValue(integrationEvent),
            routingKey,
            properties.CorrelationId);

        channel.BasicPublish(_options.ExchangeName, routingKey, properties, payload);
        return Task.CompletedTask;
    }
}
