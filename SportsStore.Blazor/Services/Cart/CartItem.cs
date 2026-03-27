namespace SportsStore.Blazor.Services.Cart;

public sealed class CartItem
{
    public long ProductID { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal => Price * Quantity;
}
