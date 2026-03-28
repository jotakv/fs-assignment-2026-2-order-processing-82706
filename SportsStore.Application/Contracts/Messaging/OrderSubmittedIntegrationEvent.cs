namespace SportsStore.Application.Contracts.Messaging;

public sealed class OrderSubmittedIntegrationEvent
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }

    public decimal TotalAmount { get; set; }

    public IReadOnlyList<OrderSubmittedLineItem> Items { get; set; } = [];
}

public sealed class OrderSubmittedLineItem
{
    public long ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
