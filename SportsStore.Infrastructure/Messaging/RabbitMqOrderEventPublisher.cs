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
    private readonly IRabbitMqConnectionFactoryProvider _connectionFactoryProvider;
    private readonly ILogger<RabbitMqOrderEventPublisher> _logger;

    public RabbitMqOrderEventPublisher(
        IOptions<RabbitMqOptions> options,
        IRabbitMqConnectionFactoryProvider connectionFactoryProvider,
        ILogger<RabbitMqOrderEventPublisher> logger)
    {
        _options = options.Value;
        _connectionFactoryProvider = connectionFactoryProvider;
        _logger = logger;
    }

    public Task PublishOrderSubmittedAsync(OrderSubmittedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        RabbitMqConnectionContext connectionContext = _connectionFactoryProvider.Create("sportsstore.api.order-publisher");

        try
        {
            _logger.LogInformation(
                "Connecting to RabbitMQ for publish. Host={Host} Port={Port} VirtualHost={VirtualHost} UseTls={UseTls} Source={Source}",
                connectionContext.ConnectionInfo.HostName,
                connectionContext.ConnectionInfo.Port,
                connectionContext.ConnectionInfo.VirtualHost,
                connectionContext.ConnectionInfo.UseTls,
                connectionContext.ConnectionInfo.Source);

            using IConnection connection = connectionContext.Factory.CreateConnection();
            using IModel channel = connection.CreateModel();

            _logger.LogInformation(
                "Declaring RabbitMQ exchange. Exchange={Exchange} EventType={EventType}",
                _options.ExchangeName,
                nameof(OrderSubmittedIntegrationEvent));

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
                "Publishing event. ServiceName={ServiceName}, EventType={EventType}, OrderId={OrderId}, CustomerId={CustomerId}, CorrelationId={CorrelationId}, Exchange={Exchange}, RoutingKey={RoutingKey}",
                "OrderPublisher",
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

            _logger.LogInformation(
                "RabbitMQ publish succeeded. EventType={EventType} Exchange={Exchange} RoutingKey={RoutingKey}",
                nameof(OrderSubmittedIntegrationEvent),
                _options.ExchangeName,
                _options.OrderSubmittedRoutingKey);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "RabbitMQ publish failed. Host={Host} Port={Port} VirtualHost={VirtualHost} UseTls={UseTls} EventType={EventType}",
                connectionContext.ConnectionInfo.HostName,
                connectionContext.ConnectionInfo.Port,
                connectionContext.ConnectionInfo.VirtualHost,
                connectionContext.ConnectionInfo.UseTls,
                nameof(OrderSubmittedIntegrationEvent));
            throw;
        }
    }
}
