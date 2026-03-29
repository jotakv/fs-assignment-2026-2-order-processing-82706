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

public sealed class PaymentInventoryConfirmedWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<PaymentInventoryConfirmedWorker> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public PaymentInventoryConfirmedWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<PaymentInventoryConfirmedWorker> logger)
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
        _channel.QueueDeclare(_options.PaymentQueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_options.PaymentQueueName, _options.ExchangeName, _options.InventoryConfirmedRoutingKey);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, eventArgs) =>
        {
            string payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            InventoryConfirmedIntegrationEvent? integrationEvent = JsonSerializer.Deserialize<InventoryConfirmedIntegrationEvent>(payload);

            if (integrationEvent is null)
            {
                _channel.BasicAck(eventArgs.DeliveryTag, false);
                return;
            }

            using (LogContext.PushProperty("CorrelationId", integrationEvent.CorrelationId))
            using (LogContext.PushProperty("OrderId", integrationEvent.OrderId))
            using (LogContext.PushProperty("CustomerId", integrationEvent.CustomerId))
            using (LogContext.PushProperty("EventType", nameof(InventoryConfirmedIntegrationEvent)))
            using (LogContext.PushProperty("ServiceName", "PaymentService"))
            {
                _logger.LogInformation("Consumed inventory confirmed event.");
                await ProcessAsync(integrationEvent, stoppingToken);
            }
            _channel.BasicAck(eventArgs.DeliveryTag, false);
        };

        _channel.BasicConsume(_options.PaymentQueueName, autoAck: false, consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }

    private async Task ProcessAsync(InventoryConfirmedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        var decisionService = scope.ServiceProvider.GetRequiredService<IPaymentDecisionService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPaymentEventPublisher>();

        PaymentDecisionResult decision = decisionService.Evaluate(integrationEvent);
        _logger.LogInformation("Payment evaluation completed. Approved={Approved}", decision.Approved);
        Order? order = await dbContext.Orders
            .Include(candidate => candidate.PaymentRecords)
            .FirstOrDefaultAsync(candidate => candidate.OrderID == integrationEvent.OrderId, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning(
                "Payment worker could not find OrderId={OrderId}, CorrelationId={CorrelationId}",
                integrationEvent.OrderId,
                integrationEvent.CorrelationId);
            return;
        }

        var record = new PaymentRecord
        {
            OrderId = order.OrderID,
            Provider = "SimulatedPaymentService",
            ExternalPaymentId = decision.PaymentReference,
            Status = decision.Approved ? "approved" : "rejected",
            FailureReason = decision.Reason,
            ProcessedAtUtc = DateTime.UtcNow
        };

        order.PaymentRecords.Add(record);
        order.Status = decision.Approved ? OrderStatus.PaymentApproved : OrderStatus.Failed;
        order.UpdatedAtUtc = DateTime.UtcNow;
        if (!decision.Approved)
        {
            order.FailedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment decision persisted. ServiceName={ServiceName}, EventType={EventType}, OrderId={OrderId}, CorrelationId={CorrelationId}",
            "PaymentService",
            decision.Approved ? nameof(PaymentApprovedIntegrationEvent) : nameof(PaymentRejectedIntegrationEvent),
            integrationEvent.OrderId,
            integrationEvent.CorrelationId);

        if (decision.Approved)
        {
            await publisher.PublishApprovedAsync(new PaymentApprovedIntegrationEvent
            {
                OrderId = integrationEvent.OrderId,
                CustomerId = integrationEvent.CustomerId,
                CorrelationId = integrationEvent.CorrelationId,
                PaymentReference = decision.PaymentReference ?? string.Empty,
                OccurredAtUtc = DateTime.UtcNow
            }, cancellationToken);
        }
        else
        {
            await publisher.PublishRejectedAsync(new PaymentRejectedIntegrationEvent
            {
                OrderId = integrationEvent.OrderId,
                CustomerId = integrationEvent.CustomerId,
                CorrelationId = integrationEvent.CorrelationId,
                Reason = decision.Reason ?? "Payment rejected.",
                OccurredAtUtc = DateTime.UtcNow
            }, cancellationToken);
        }
    }
}
