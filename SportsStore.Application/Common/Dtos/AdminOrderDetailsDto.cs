namespace SportsStore.Application.Common.Dtos;

public sealed class AdminOrderDetailsDto
{
    public int OrderID { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? CustomerName { get; set; }

    public string? PaymentStatus { get; set; }

    public string? PaymentReference { get; set; }

    public string? InventoryResult { get; set; }

    public string? InventoryReference { get; set; }

    public string? ShipmentReference { get; set; }

    public string? TrackingNumber { get; set; }

    public string? FailureReason { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public IReadOnlyList<OrderLineDto> Lines { get; set; } = [];
}
