using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SportsStore.Application.Abstractions.Messaging;
using SportsStore.Application.Contracts.Messaging;
using SportsStore.Domain.Entities;
using SportsStore.Infrastructure.Options;
using SportsStore.Infrastructure.Persistence;

public sealed class ShippingPaymentApprovedWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<ShippingPaymentApprovedWorker> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public ShippingPaymentApprovedWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<ShippingPaymentApprovedWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
        _channel.QueueDeclare(_options.ShippingQueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_options.ShippingQueueName, _options.ExchangeName, _options.PaymentApprovedRoutingKey);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, eventArgs) =>
        {
            string payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            PaymentApprovedIntegrationEvent? integrationEvent = JsonSerializer.Deserialize<PaymentApprovedIntegrationEvent>(payload);

            if (integrationEvent is null)
            {
                _channel.BasicAck(eventArgs.DeliveryTag, false);
                return;
            }

            await ProcessAsync(integrationEvent, stoppingToken);
            _channel.BasicAck(eventArgs.DeliveryTag, false);
        };

        _channel.BasicConsume(_options.ShippingQueueName, autoAck: false, consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }

    private async Task ProcessAsync(PaymentApprovedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        var shipmentService = scope.ServiceProvider.GetRequiredService<IShipmentCreationService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IShippingEventPublisher>();

        ShipmentCreationResult decision = shipmentService.Create(integrationEvent);
        Order? order = await dbContext.Orders
            .Include(candidate => candidate.ShipmentRecords)
            .FirstOrDefaultAsync(candidate => candidate.OrderID == integrationEvent.OrderId, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning(
                "Shipping worker could not find OrderId={OrderId}, CorrelationId={CorrelationId}",
                integrationEvent.OrderId,
                integrationEvent.CorrelationId);
            return;
        }

        var shipmentRecord = new ShipmentRecord
        {
            OrderId = order.OrderID,
            ShipmentReference = decision.ShipmentReference,
            Carrier = "SimulatedCarrier",
            TrackingNumber = decision.TrackingNumber,
            CreatedAtUtc = DateTime.UtcNow,
            ShippedAtUtc = decision.EstimatedDispatchDateUtc,
            FailureReason = decision.Reason
        };

        order.ShipmentRecords.Add(shipmentRecord);
        order.Status = decision.Succeeded ? OrderStatus.Completed : OrderStatus.Failed;
        order.UpdatedAtUtc = DateTime.UtcNow;
        if (decision.Succeeded)
        {
            order.CompletedAtUtc = decision.EstimatedDispatchDateUtc;
            order.Shipped = true;
        }
        else
        {
            order.FailedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Shipping decision persisted. EventType={EventType}, OrderId={OrderId}, CorrelationId={CorrelationId}",
            decision.Succeeded ? nameof(ShippingCreatedIntegrationEvent) : nameof(ShippingFailedIntegrationEvent),
            integrationEvent.OrderId,
            integrationEvent.CorrelationId);

        if (decision.Succeeded)
        {
            await publisher.PublishCreatedAsync(new ShippingCreatedIntegrationEvent
            {
                OrderId = integrationEvent.OrderId,
                CustomerId = integrationEvent.CustomerId,
                CorrelationId = integrationEvent.CorrelationId,
                ShipmentReference = decision.ShipmentReference ?? string.Empty,
                TrackingNumber = decision.TrackingNumber ?? string.Empty,
                EstimatedDispatchDateUtc = decision.EstimatedDispatchDateUtc ?? DateTime.UtcNow,
                OccurredAtUtc = DateTime.UtcNow
            }, cancellationToken);
        }
        else
        {
            await publisher.PublishFailedAsync(new ShippingFailedIntegrationEvent
            {
                OrderId = integrationEvent.OrderId,
                CustomerId = integrationEvent.CustomerId,
                CorrelationId = integrationEvent.CorrelationId,
                Reason = decision.Reason ?? "Shipping creation failed.",
                OccurredAtUtc = DateTime.UtcNow
            }, cancellationToken);
        }
    }
}
