namespace SportsStore.Application.Common.Dtos;

public sealed class OrderDto
{
    public int OrderID { get; set; }

    public IReadOnlyList<OrderLineDto> Lines { get; set; } = [];

    public string? Name { get; set; }

    public string? Line1 { get; set; }

    public string? Line2 { get; set; }

    public string? Line3 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string? Country { get; set; }

    public bool GiftWrap { get; set; }

    public bool Shipped { get; set; }

    public string? StripeSessionId { get; set; }

    public string? StripePaymentIntentId { get; set; }

    public string? StripePaymentStatus { get; set; }

    public DateTime? PaidAtUtc { get; set; }

    public decimal TotalAmount { get; set; }

    public int ItemCount { get; set; }
}
