namespace SportsStore.Application.Common.Dtos;

public sealed class OrderLineDto
{
    public long ProductID { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}
