namespace SportsStore.Application.Contracts.Messaging;

public sealed class PaymentRejectedIntegrationEvent
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }
}
