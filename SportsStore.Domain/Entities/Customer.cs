using System.ComponentModel.DataAnnotations;

namespace SportsStore.Domain.Entities;

public class Customer
{
    public int CustomerId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(320)]
    public string? Email { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
