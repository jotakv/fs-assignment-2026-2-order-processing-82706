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

public sealed class PaymentInventoryConfirmedWorker : BackgroundService
{
    private const int MaxRetryDelaySeconds = 30;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly IRabbitMqConnectionFactoryProvider _connectionFactoryProvider;
    private readonly ILogger<PaymentInventoryConfirmedWorker> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public PaymentInventoryConfirmedWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        IRabbitMqConnectionFactoryProvider connectionFactoryProvider,
        ILogger<PaymentInventoryConfirmedWorker> logger)
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
                connectionContext = _connectionFactoryProvider.Create("sportsstore.payment.worker");

                _logger.LogInformation(
                    "Starting RabbitMQ consumer. Host={Host} Port={Port} VirtualHost={VirtualHost} UseTls={UseTls} Source={Source} Exchange={Exchange} Queue={Queue} RoutingKey={RoutingKey} Attempt={Attempt}",
                    connectionContext.ConnectionInfo.HostName,
                    connectionContext.ConnectionInfo.Port,
                    connectionContext.ConnectionInfo.VirtualHost,
                    connectionContext.ConnectionInfo.UseTls,
                    connectionContext.ConnectionInfo.Source,
                    _options.ExchangeName,
                    _options.PaymentQueueName,
                    _options.InventoryConfirmedRoutingKey,
                    attempt);

                _connection = connectionContext.Factory.CreateConnection();
                _channel = _connection.CreateModel();

                _logger.LogInformation(
                    "Declaring RabbitMQ topology. Exchange={Exchange} Queue={Queue} RoutingKey={RoutingKey}",
                    _options.ExchangeName,
                    _options.PaymentQueueName,
                    _options.InventoryConfirmedRoutingKey);

                _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
                _channel.QueueDeclare(_options.PaymentQueueName, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(_options.PaymentQueueName, _options.ExchangeName, _options.InventoryConfirmedRoutingKey);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += HandleReceivedAsync;

                _channel.BasicConsume(_options.PaymentQueueName, autoAck: false, consumer);

                _logger.LogInformation(
                    "RabbitMQ consumer is ready. Exchange={Exchange} Queue={Queue} RoutingKey={RoutingKey}",
                    _options.ExchangeName,
                    _options.PaymentQueueName,
                    _options.InventoryConfirmedRoutingKey);

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
                    _options.PaymentQueueName,
                    _options.InventoryConfirmedRoutingKey,
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
            InventoryConfirmedIntegrationEvent? integrationEvent = JsonSerializer.Deserialize<InventoryConfirmedIntegrationEvent>(payload);

            if (integrationEvent is null)
            {
                _logger.LogWarning(
                    "Received an empty or invalid inventory confirmed message. Queue={Queue}",
                    _options.PaymentQueueName);
                channel.BasicAck(eventArgs.DeliveryTag, false);
                return;
            }

            using (LogContext.PushProperty("CorrelationId", integrationEvent.CorrelationId))
            using (LogContext.PushProperty("OrderId", integrationEvent.OrderId))
            using (LogContext.PushProperty("CustomerId", integrationEvent.CustomerId))
            using (LogContext.PushProperty("EventType", nameof(InventoryConfirmedIntegrationEvent)))
            using (LogContext.PushProperty("ServiceName", "PaymentService"))
            {
                _logger.LogInformation("Consumed inventory confirmed event.");
                await ProcessAsync(integrationEvent, CancellationToken.None);
            }

            channel.BasicAck(eventArgs.DeliveryTag, false);
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize inventory confirmed message. Queue={Queue}",
                _options.PaymentQueueName);
            channel.BasicAck(eventArgs.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process inventory confirmed message. Queue={Queue}",
                _options.PaymentQueueName);
            channel.BasicNack(eventArgs.DeliveryTag, false, requeue: true);
        }
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
