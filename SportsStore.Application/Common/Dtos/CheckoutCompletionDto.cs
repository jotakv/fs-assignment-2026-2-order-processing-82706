namespace SportsStore.Application.Common.Dtos;

public sealed class CheckoutCompletionDto
{
    public int OrderID { get; set; }

    public string SessionId { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public DateTime? PaidAtUtc { get; set; }

    public decimal TotalAmount { get; set; }

    public int ItemCount { get; set; }
}
