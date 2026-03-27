using SportsStore.Application.Common.Dtos;
using SportsStore.Blazor.Services.Browser;

namespace SportsStore.Blazor.Services.Cart;

public sealed class CartState
{
    private const string StorageKey = "sportsstore.cart";
    private readonly SessionStorageService _storage;
    private readonly List<CartItem> _items = [];
    private bool _initialized;

    public CartState(SessionStorageService storage)
    {
        _storage = storage;
    }

    public event Action? Changed;

    public IReadOnlyList<CartItem> Items => _items;

    public int ItemCount => _items.Sum(item => item.Quantity);

    public decimal TotalAmount => _items.Sum(item => item.LineTotal);

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        List<CartItem>? storedItems = await _storage.GetAsync<List<CartItem>>(StorageKey);
        _items.Clear();
        if (storedItems is not null)
        {
            _items.AddRange(storedItems);
        }

        _initialized = true;
        NotifyChanged();
    }

    public async Task AddAsync(ProductDto product)
    {
        await InitializeAsync();

        CartItem? item = _items.FirstOrDefault(existing => existing.ProductID == product.ProductID);
        if (item is null)
        {
            _items.Add(new CartItem
            {
                ProductID = product.ProductID,
                Name = product.Name,
                Category = product.Category,
                Price = product.Price,
                Quantity = 1
            });
        }
        else
        {
            item.Quantity += 1;
        }

        await PersistAsync();
    }

    public async Task IncrementAsync(long productId)
    {
        await InitializeAsync();
        CartItem? item = _items.FirstOrDefault(existing => existing.ProductID == productId);
        if (item is null)
        {
            return;
        }

        item.Quantity += 1;
        await PersistAsync();
    }

    public async Task DecrementAsync(long productId)
    {
        await InitializeAsync();
        CartItem? item = _items.FirstOrDefault(existing => existing.ProductID == productId);
        if (item is null)
        {
            return;
        }

        item.Quantity -= 1;
        if (item.Quantity <= 0)
        {
            _items.Remove(item);
        }

        await PersistAsync();
    }

    public async Task RemoveAsync(long productId)
    {
        await InitializeAsync();
        _items.RemoveAll(existing => existing.ProductID == productId);
        await PersistAsync();
    }

    public async Task ClearAsync()
    {
        await InitializeAsync();
        _items.Clear();
        await _storage.RemoveAsync(StorageKey);
        NotifyChanged();
    }

    private async Task PersistAsync()
    {
        await _storage.SetAsync(StorageKey, _items);
        NotifyChanged();
    }

    private void NotifyChanged() => Changed?.Invoke();
}
