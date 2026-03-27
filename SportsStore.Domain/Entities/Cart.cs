namespace SportsStore.Domain.Entities;

public class Cart
{
    public List<CartLine> Lines { get; set; } = [];

    public virtual void AddItem(Product product, int quantity)
    {
        CartLine? line = Lines.FirstOrDefault(p => p.Product.ProductID == product.ProductID);

        if (line is null)
        {
            Lines.Add(new CartLine
            {
                Product = product,
                Quantity = quantity
            });
            return;
        }

        line.Quantity += quantity;
    }

    public virtual void RemoveLine(Product product) =>
        Lines.RemoveAll(l => l.Product.ProductID == product.ProductID);

    public decimal ComputeTotalValue() =>
        Lines.Sum(e => e.Product.Price * e.Quantity);

    public virtual void Clear() => Lines.Clear();
}

public class CartLine
{
    public int CartLineID { get; set; }

    public Product Product { get; set; } = new();

    public int Quantity { get; set; }
}
