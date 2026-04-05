using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SportsStore.Application.Abstractions.Messaging;
using SportsStore.Application.Contracts.Messaging;
using SportsStore.Infrastructure.Options;

namespace SportsStore.Infrastructure.Messaging;

public sealed class RabbitMqPaymentEventPublisher : IPaymentEventPublisher
{
    private readonly RabbitMqOptions _options;
    private readonly IRabbitMqConnectionFactoryProvider _connectionFactoryProvider;
    private readonly ILogger<RabbitMqPaymentEventPublisher> _logger;

    public RabbitMqPaymentEventPublisher(
        IOptions<RabbitMqOptions> options,
        IRabbitMqConnectionFactoryProvider connectionFactoryProvider,
        ILogger<RabbitMqPaymentEventPublisher> logger)
    {
        _options = options.Value;
        _connectionFactoryProvider = connectionFactoryProvider;
        _logger = logger;
    }

    public Task PublishApprovedAsync(PaymentApprovedIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        PublishAsync(integrationEvent, _options.PaymentApprovedRoutingKey, cancellationToken);

    public Task PublishRejectedAsync(PaymentRejectedIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        PublishAsync(integrationEvent, _options.PaymentRejectedRoutingKey, cancellationToken);

    private Task PublishAsync(object integrationEvent, string routingKey, CancellationToken cancellationToken)
    {
        RabbitMqConnectionContext connectionContext = _connectionFactoryProvider.Create("sportsstore.payment.publisher");

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
                integrationEvent.GetType().Name);

            channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false, arguments: null);

            IBasicProperties properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.CorrelationId = (string?)integrationEvent.GetType().GetProperty("CorrelationId")?.GetValue(integrationEvent);
            properties.Type = integrationEvent.GetType().Name;

            byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(integrationEvent));

            _logger.LogInformation(
                "Publishing event. ServiceName={ServiceName}, EventType={EventType}, OrderId={OrderId}, CustomerId={CustomerId}, RoutingKey={RoutingKey}, CorrelationId={CorrelationId}",
                "PaymentPublisher",
                integrationEvent.GetType().Name,
                integrationEvent.GetType().GetProperty("OrderId")?.GetValue(integrationEvent),
                integrationEvent.GetType().GetProperty("CustomerId")?.GetValue(integrationEvent),
                routingKey,
                properties.CorrelationId);

            channel.BasicPublish(_options.ExchangeName, routingKey, properties, payload);

            _logger.LogInformation(
                "RabbitMQ publish succeeded. EventType={EventType} Exchange={Exchange} RoutingKey={RoutingKey}",
                integrationEvent.GetType().Name,
                _options.ExchangeName,
                routingKey);

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
                integrationEvent.GetType().Name);
            throw;
        }
    }
}
