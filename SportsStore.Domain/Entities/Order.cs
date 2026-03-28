using System.ComponentModel.DataAnnotations;

namespace SportsStore.Domain.Entities;

public class Order
{
    public int OrderID { get; set; }

    public int? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public ICollection<CartLine> Lines { get; set; } = new List<CartLine>();

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public ICollection<InventoryRecord> InventoryRecords { get; set; } = new List<InventoryRecord>();

    public ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();

    public ICollection<ShipmentRecord> ShipmentRecords { get; set; } = new List<ShipmentRecord>();

    [Required(ErrorMessage = "Please enter a name")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Please enter the first address line")]
    public string? Line1 { get; set; }

    public string? Line2 { get; set; }

    public string? Line3 { get; set; }

    [Required(ErrorMessage = "Please enter a city name")]
    public string? City { get; set; }

    [Required(ErrorMessage = "Please enter a state name")]
    public string? State { get; set; }

    public string? Zip { get; set; }

    [Required(ErrorMessage = "Please enter a country name")]
    public string? Country { get; set; }

    public bool GiftWrap { get; set; }

    public bool Shipped { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Submitted;

    public string? StripeSessionId { get; set; }

    public string? StripePaymentIntentId { get; set; }

    public string? StripePaymentStatus { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public DateTime? FailedAtUtc { get; set; }
}
