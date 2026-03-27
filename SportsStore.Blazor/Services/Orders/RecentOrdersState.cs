using SportsStore.Blazor.Services.Browser;

namespace SportsStore.Blazor.Services.Orders;

public sealed class RecentOrdersState
{
    private const string StorageKey = "sportsstore.recent-orders";
    private readonly SessionStorageService _storage;
    private readonly List<int> _orderIds = [];
    private bool _initialized;

    public RecentOrdersState(SessionStorageService storage)
    {
        _storage = storage;
    }

    public IReadOnlyList<int> OrderIds => _orderIds;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        List<int>? stored = await _storage.GetAsync<List<int>>(StorageKey);
        _orderIds.Clear();
        if (stored is not null)
        {
            _orderIds.AddRange(stored);
        }

        _initialized = true;
    }

    public async Task AddAsync(int orderId)
    {
        await InitializeAsync();

        if (!_orderIds.Contains(orderId))
        {
            _orderIds.Insert(0, orderId);
            await _storage.SetAsync(StorageKey, _orderIds.Take(10).ToList());
            if (_orderIds.Count > 10)
            {
                _orderIds.RemoveRange(10, _orderIds.Count - 10);
            }
        }
    }
}
