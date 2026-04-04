using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog.Context;
using SportsStore.Application.Abstractions.Messaging;
using SportsStore.Application.Contracts.Messaging;
using SportsStore.Domain.Entities;
using SportsStore.Infrastructure.Messaging;
using SportsStore.Infrastructure.Options;
using SportsStore.Infrastructure.Persistence;

public sealed class ShippingPaymentApprovedWorker : BackgroundService
{
    private const int MaxRetryDelaySeconds = 30;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly IRabbitMqConnectionFactoryProvider _connectionFactoryProvider;
    private readonly ILogger<ShippingPaymentApprovedWorker> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public ShippingPaymentApprovedWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        IRabbitMqConnectionFactoryProvider connectionFactoryProvider,
        ILogger<ShippingPaymentApprovedWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _connectionFactoryProvider = connectionFactoryProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int attempt = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            RabbitMqConnectionContext? connectionContext = null;

            try
            {
                attempt++;
                connectionContext = _connectionFactoryProvider.Create("sportsstore.shipping.worker");

                _logger.LogInformation(
                    "Starting RabbitMQ consumer. Host={Host} Port={Port} VirtualHost={VirtualHost} UseTls={UseTls} Source={Source} Exchange={Exchange} Queue={Queue} RoutingKey={RoutingKey} Attempt={Attempt}",
                    connectionContext.ConnectionInfo.HostName,
                    connectionContext.ConnectionInfo.Port,
                    connectionContext.ConnectionInfo.VirtualHost,
                    connectionContext.ConnectionInfo.UseTls,
                    connectionContext.ConnectionInfo.Source,
                    _options.ExchangeName,
                    _options.ShippingQueueName,
                    _options.PaymentApprovedRoutingKey,
                    attempt);

                _connection = connectionContext.Factory.CreateConnection();
                _channel = _connection.CreateModel();

                _logger.LogInformation(
                    "Declaring RabbitMQ topology. Exchange={Exchange} Queue={Queue} RoutingKey={RoutingKey}",
                    _options.ExchangeName,
                    _options.ShippingQueueName,
                    _options.PaymentApprovedRoutingKey);

                _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
                _channel.QueueDeclare(_options.ShippingQueueName, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(_options.ShippingQueueName, _options.ExchangeName, _options.PaymentApprovedRoutingKey);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += HandleReceivedAsync;

                _channel.BasicConsume(_options.ShippingQueueName, autoAck: false, consumer);

                _logger.LogInformation(
                    "RabbitMQ consumer is ready. Exchange={Exchange} Queue={Queue} RoutingKey={RoutingKey}",
                    _options.ExchangeName,
                    _options.ShippingQueueName,
                    _options.PaymentApprovedRoutingKey);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                TimeSpan retryDelay = GetRetryDelay(attempt);

                _logger.LogError(
                    ex,
                    "RabbitMQ consumer startup failed. Host={Host} Port={Port} VirtualHost={VirtualHost} UseTls={UseTls} Exchange={Exchange} Queue={Queue} RoutingKey={RoutingKey} RetryingInSeconds={RetryDelaySeconds}",
                    connectionContext?.ConnectionInfo.HostName ?? "(unresolved)",
                    connectionContext?.ConnectionInfo.Port ?? 0,
                    connectionContext?.ConnectionInfo.VirtualHost ?? "(unresolved)",
                    connectionContext?.ConnectionInfo.UseTls ?? false,
                    _options.ExchangeName,
                    _options.ShippingQueueName,
                    _options.PaymentApprovedRoutingKey,
                    retryDelay.TotalSeconds);

                DisposeRabbitMqResources();

                try
                {
                    await Task.Delay(retryDelay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        DisposeRabbitMqResources();
    }

    public override void Dispose()
    {
        DisposeRabbitMqResources();
        base.Dispose();
    }

    private async Task HandleReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        IModel? channel = _channel;
        if (channel is null)
        {
            return;
        }

        string payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

        try
        {
            PaymentApprovedIntegrationEvent? integrationEvent = JsonSerializer.Deserialize<PaymentApprovedIntegrationEvent>(payload);

            if (integrationEvent is null)
            {
                _logger.LogWarning(
                    "Received an empty or invalid payment approved message. Queue={Queue}",
                    _options.ShippingQueueName);
                channel.BasicAck(eventArgs.DeliveryTag, false);
                return;
            }

            using (LogContext.PushProperty("CorrelationId", integrationEvent.CorrelationId))
            using (LogContext.PushProperty("OrderId", integrationEvent.OrderId))
            using (LogContext.PushProperty("CustomerId", integrationEvent.CustomerId))
            using (LogContext.PushProperty("EventType", nameof(PaymentApprovedIntegrationEvent)))
            using (LogContext.PushProperty("ServiceName", "ShippingService"))
            {
                _logger.LogInformation("Consumed payment approved event.");
                await ProcessAsync(integrationEvent, CancellationToken.None);
            }

            channel.BasicAck(eventArgs.DeliveryTag, false);
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize payment approved message. Queue={Queue}",
                _options.ShippingQueueName);
            channel.BasicAck(eventArgs.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process payment approved message. Queue={Queue}",
                _options.ShippingQueueName);
            channel.BasicNack(eventArgs.DeliveryTag, false, requeue: true);
        }
    }

    private async Task ProcessAsync(PaymentApprovedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        var shipmentService = scope.ServiceProvider.GetRequiredService<IShipmentCreationService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IShippingEventPublisher>();

        ShipmentCreationResult decision = shipmentService.Create(integrationEvent);
        _logger.LogInformation("Shipping creation completed. Succeeded={Succeeded}", decision.Succeeded);
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

    private static TimeSpan GetRetryDelay(int attempt)
    {
        int delaySeconds = Math.Min(Math.Max(attempt, 1) * 5, MaxRetryDelaySeconds);
        return TimeSpan.FromSeconds(delaySeconds);
    }

    private void DisposeRabbitMqResources()
    {
        _channel?.Dispose();
        _channel = null;

        _connection?.Dispose();
        _connection = null;
    }
}
