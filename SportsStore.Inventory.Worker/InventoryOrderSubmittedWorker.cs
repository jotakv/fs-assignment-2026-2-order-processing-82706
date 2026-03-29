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
using SportsStore.Infrastructure.Options;
using SportsStore.Infrastructure.Persistence;

public sealed class InventoryOrderSubmittedWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<InventoryOrderSubmittedWorker> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public InventoryOrderSubmittedWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<InventoryOrderSubmittedWorker> logger)
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
        _channel.QueueDeclare(_options.InventoryQueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_options.InventoryQueueName, _options.ExchangeName, _options.OrderSubmittedRoutingKey);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, eventArgs) =>
        {
            string payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            OrderSubmittedIntegrationEvent? integrationEvent = JsonSerializer.Deserialize<OrderSubmittedIntegrationEvent>(payload);

            if (integrationEvent is null)
            {
                _channel.BasicAck(eventArgs.DeliveryTag, false);
                return;
            }

            using (LogContext.PushProperty("CorrelationId", integrationEvent.CorrelationId))
            using (LogContext.PushProperty("OrderId", integrationEvent.OrderId))
            using (LogContext.PushProperty("CustomerId", integrationEvent.CustomerId))
            using (LogContext.PushProperty("EventType", nameof(OrderSubmittedIntegrationEvent)))
            using (LogContext.PushProperty("ServiceName", "InventoryService"))
            {
                _logger.LogInformation("Consumed order submitted event.");
                await ProcessAsync(integrationEvent, stoppingToken);
            }
            _channel.BasicAck(eventArgs.DeliveryTag, false);
        };

        _channel.BasicConsume(_options.InventoryQueueName, autoAck: false, consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
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
}
