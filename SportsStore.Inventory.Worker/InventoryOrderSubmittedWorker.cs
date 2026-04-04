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

public sealed class InventoryOrderSubmittedWorker : BackgroundService
{
    private const int MaxRetryDelaySeconds = 30;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly IRabbitMqConnectionFactoryProvider _connectionFactoryProvider;
    private readonly ILogger<InventoryOrderSubmittedWorker> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public InventoryOrderSubmittedWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        IRabbitMqConnectionFactoryProvider connectionFactoryProvider,
        ILogger<InventoryOrderSubmittedWorker> logger)
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
                connectionContext = _connectionFactoryProvider.Create("sportsstore.inventory.worker");

                _logger.LogInformation(
                    "Starting RabbitMQ consumer. Host={Host} Port={Port} VirtualHost={VirtualHost} UseTls={UseTls} Source={Source} Exchange={Exchange} Queue={Queue} RoutingKey={RoutingKey} Attempt={Attempt}",
                    connectionContext.ConnectionInfo.HostName,
                    connectionContext.ConnectionInfo.Port,
                    connectionContext.ConnectionInfo.VirtualHost,
                    connectionContext.ConnectionInfo.UseTls,
                    connectionContext.ConnectionInfo.Source,
                    _options.ExchangeName,
                    _options.InventoryQueueName,
                    _options.OrderSubmittedRoutingKey,
                    attempt);

                _connection = connectionContext.Factory.CreateConnection();
                _channel = _connection.CreateModel();

                _logger.LogInformation(
                    "Declaring RabbitMQ topology. Exchange={Exchange} Queue={Queue} RoutingKey={RoutingKey}",
                    _options.ExchangeName,
                    _options.InventoryQueueName,
                    _options.OrderSubmittedRoutingKey);

                _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
                _channel.QueueDeclare(_options.InventoryQueueName, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(_options.InventoryQueueName, _options.ExchangeName, _options.OrderSubmittedRoutingKey);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += HandleReceivedAsync;

                _channel.BasicConsume(_options.InventoryQueueName, autoAck: false, consumer);

                _logger.LogInformation(
                    "RabbitMQ consumer is ready. Exchange={Exchange} Queue={Queue} RoutingKey={RoutingKey}",
                    _options.ExchangeName,
                    _options.InventoryQueueName,
                    _options.OrderSubmittedRoutingKey);

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
                    _options.InventoryQueueName,
                    _options.OrderSubmittedRoutingKey,
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
            OrderSubmittedIntegrationEvent? integrationEvent = JsonSerializer.Deserialize<OrderSubmittedIntegrationEvent>(payload);

            if (integrationEvent is null)
            {
                _logger.LogWarning(
                    "Received an empty or invalid order submitted message. Queue={Queue}",
                    _options.InventoryQueueName);
                channel.BasicAck(eventArgs.DeliveryTag, false);
                return;
            }

            using (LogContext.PushProperty("CorrelationId", integrationEvent.CorrelationId))
            using (LogContext.PushProperty("OrderId", integrationEvent.OrderId))
            using (LogContext.PushProperty("CustomerId", integrationEvent.CustomerId))
            using (LogContext.PushProperty("EventType", nameof(OrderSubmittedIntegrationEvent)))
            using (LogContext.PushProperty("ServiceName", "InventoryService"))
            {
                _logger.LogInformation("Consumed order submitted event.");
                await ProcessAsync(integrationEvent, CancellationToken.None);
            }

            channel.BasicAck(eventArgs.DeliveryTag, false);
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize order submitted message. Queue={Queue}",
                _options.InventoryQueueName);
            channel.BasicAck(eventArgs.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process order submitted message. Queue={Queue}",
                _options.InventoryQueueName);
            channel.BasicNack(eventArgs.DeliveryTag, false, requeue: true);
        }
    }

    private async Task ProcessAsync(OrderSubmittedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        var decisionService = scope.ServiceProvider.GetRequiredService<IInventoryDecisionService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IInventoryEventPublisher>();

        InventoryDecisionResult decision = decisionService.Evaluate(integrationEvent);
        _logger.LogInformation("Inventory validation completed. Succeeded={Succeeded}", decision.Succeeded);
        Order? order = await dbContext.Orders
            .Include(candidate => candidate.InventoryRecords)
            .FirstOrDefaultAsync(candidate => candidate.OrderID == integrationEvent.OrderId, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning(
                "Inventory worker could not find OrderId={OrderId}, CorrelationId={CorrelationId}",
                integrationEvent.OrderId,
                integrationEvent.CorrelationId);
            return;
        }

        var record = new InventoryRecord
        {
            OrderId = order.OrderID,
            ReservationReference = decision.ReservationReference,
            Succeeded = decision.Succeeded,
            FailureReason = decision.FailureReason,
            ProcessedAtUtc = DateTime.UtcNow
        };

        order.InventoryRecords.Add(record);
        order.Status = decision.Succeeded ? OrderStatus.InventoryConfirmed : OrderStatus.Failed;
        order.UpdatedAtUtc = DateTime.UtcNow;
        if (!decision.Succeeded)
        {
            order.FailedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Inventory decision persisted. ServiceName={ServiceName}, EventType={EventType}, OrderId={OrderId}, CorrelationId={CorrelationId}",
            "InventoryService",
            decision.Succeeded ? nameof(InventoryConfirmedIntegrationEvent) : nameof(InventoryFailedIntegrationEvent),
            integrationEvent.OrderId,
            integrationEvent.CorrelationId);

        if (decision.Succeeded)
        {
            await publisher.PublishConfirmedAsync(new InventoryConfirmedIntegrationEvent
            {
                OrderId = integrationEvent.OrderId,
                CustomerId = integrationEvent.CustomerId,
                CorrelationId = integrationEvent.CorrelationId,
                ReservationReference = decision.ReservationReference ?? string.Empty,
                OccurredAtUtc = DateTime.UtcNow
            }, cancellationToken);
        }
        else
        {
            await publisher.PublishFailedAsync(new InventoryFailedIntegrationEvent
            {
                OrderId = integrationEvent.OrderId,
                CustomerId = integrationEvent.CustomerId,
                CorrelationId = integrationEvent.CorrelationId,
                FailureReason = decision.FailureReason ?? "Inventory validation failed.",
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
