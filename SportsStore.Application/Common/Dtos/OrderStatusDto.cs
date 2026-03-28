namespace SportsStore.Application.Common.Dtos;

public sealed class OrderStatusDto
{
    public int OrderID { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public DateTime? FailedAtUtc { get; set; }
}
