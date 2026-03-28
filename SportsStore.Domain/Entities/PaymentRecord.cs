using System.ComponentModel.DataAnnotations;

namespace SportsStore.Domain.Entities;

public class PaymentRecord
{
    public int PaymentRecordId { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    [MaxLength(200)]
    public string? Provider { get; set; }

    [MaxLength(200)]
    public string? ExternalPaymentId { get; set; }

    [MaxLength(100)]
    public string? Status { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }

    [MaxLength(500)]
    public string? FailureReason { get; set; }
}
