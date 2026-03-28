using System.ComponentModel.DataAnnotations;

namespace SportsStore.Domain.Entities;

public class ShipmentRecord
{
    public int ShipmentRecordId { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    [MaxLength(100)]
    public string? ShipmentReference { get; set; }

    [MaxLength(100)]
    public string? Carrier { get; set; }

    [MaxLength(100)]
    public string? TrackingNumber { get; set; }

    public DateTime? CreatedAtUtc { get; set; }

    public DateTime? ShippedAtUtc { get; set; }

    [MaxLength(500)]
    public string? FailureReason { get; set; }
}
