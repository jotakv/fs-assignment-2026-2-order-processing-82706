using Microsoft.Extensions.Caching.Memory;
using SportsStore.Application.Abstractions.Caching;
using System.Collections.Concurrent;

namespace SportsStore.Infrastructure.Caching;

public sealed class MemoryCatalogCache : ICatalogCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public MemoryCatalogCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken)
        where T : class
    {
        if (_memoryCache.TryGetValue(key, out T? cached) && cached is not null)
        {
            return cached;
        }

        T? created = await factory(cancellationToken);

        if (created is null)
        {
            return created;
        }

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
        };

        options.RegisterPostEvictionCallback((cacheKey, _, _, _) =>
        {
            if (cacheKey is string typedKey)
            {
                _keys.TryRemove(typedKey, out _);
            }
        });

        _memoryCache.Set(key, created, options);
        _keys[key] = 0;

        return created;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        _keys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        foreach (string key in _keys.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal)))
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
