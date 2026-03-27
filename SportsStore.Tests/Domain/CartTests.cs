using SportsStore.Domain.Entities;

namespace SportsStore.Tests.Domain;

public class CartTests
{
    [Fact]
    public void Can_Add_New_Lines()
    {
        var cart = new Cart();
        var kayak = new Product { ProductID = 1, Name = "Kayak" };
        var ball = new Product { ProductID = 2, Name = "Ball" };

        cart.AddItem(kayak, 1);
        cart.AddItem(ball, 2);

        Assert.Equal(2, cart.Lines.Count);
        Assert.Equal("Kayak", cart.Lines[0].Product.Name);
        Assert.Equal("Ball", cart.Lines[1].Product.Name);
    }

    [Fact]
    public void Can_Add_Quantity_For_Existing_Line()
    {
        var cart = new Cart();
        var kayak = new Product { ProductID = 1, Name = "Kayak" };

        cart.AddItem(kayak, 1);
        cart.AddItem(kayak, 3);

        Assert.Single(cart.Lines);
        Assert.Equal(4, cart.Lines[0].Quantity);
    }

    [Fact]
    public void Can_Remove_Line()
    {
        var cart = new Cart();
        var kayak = new Product { ProductID = 1, Name = "Kayak" };
        var ball = new Product { ProductID = 2, Name = "Ball" };

        cart.AddItem(kayak, 1);
        cart.AddItem(ball, 2);
        cart.RemoveLine(kayak);

        Assert.Single(cart.Lines);
        Assert.Equal(ball.ProductID, cart.Lines[0].Product.ProductID);
    }

    [Fact]
    public void Can_Compute_Total_And_Clear()
    {
        var cart = new Cart();
        cart.AddItem(new Product { ProductID = 1, Name = "Kayak", Price = 275m }, 1);
        cart.AddItem(new Product { ProductID = 2, Name = "Ball", Price = 19.5m }, 2);

        Assert.Equal(314m, cart.ComputeTotalValue());

        cart.Clear();

        Assert.Empty(cart.Lines);
    }
}
