namespace SportsStore.Application.Contracts.Messaging;

public sealed class PaymentApprovedIntegrationEvent
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public string PaymentReference { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }
}
