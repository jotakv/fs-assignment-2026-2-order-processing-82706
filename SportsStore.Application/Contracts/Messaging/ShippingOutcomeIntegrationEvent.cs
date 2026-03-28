namespace SportsStore.Application.Contracts.Messaging;

public sealed class ShippingCreatedIntegrationEvent
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public string ShipmentReference { get; set; } = string.Empty;

    public string TrackingNumber { get; set; } = string.Empty;

    public DateTime EstimatedDispatchDateUtc { get; set; }

    public DateTime OccurredAtUtc { get; set; }
}

public sealed class ShippingFailedIntegrationEvent
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }
}
