using System.ComponentModel.DataAnnotations;

namespace SportsStore.Domain.Entities;

public class InventoryRecord
{
    public int InventoryRecordId { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    [MaxLength(100)]
    public string? ReservationReference { get; set; }

    public bool Succeeded { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }

    [MaxLength(500)]
    public string? FailureReason { get; set; }
}
