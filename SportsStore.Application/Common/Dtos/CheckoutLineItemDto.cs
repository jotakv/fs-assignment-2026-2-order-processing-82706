using System.ComponentModel.DataAnnotations;

namespace SportsStore.Application.Common.Dtos;

public sealed class CheckoutLineItemDto
{
    [Range(1, long.MaxValue)]
    public long ProductID { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive")]
    public int Quantity { get; set; }
}
