using Microsoft.Extensions.Caching.Memory;
using SportsStore.Infrastructure.Caching;

namespace SportsStore.Tests.Infrastructure;

public class MemoryCatalogCacheTests
{
    [Fact]
    public async Task RemoveByPrefix_Invalidates_Cached_Catalog_Entries()
    {
        var cache = new MemoryCatalogCache(new MemoryCache(new MemoryCacheOptions()));

        string? first = await cache.GetOrCreateAsync(
            "catalog:products:category=all:page=1:size=4",
            _ => Task.FromResult<string?>("first"),
            TimeSpan.FromMinutes(5),
            CancellationToken.None);

        await cache.RemoveByPrefixAsync("catalog:", CancellationToken.None);

        string? second = await cache.GetOrCreateAsync(
            "catalog:products:category=all:page=1:size=4",
            _ => Task.FromResult<string?>("second"),
            TimeSpan.FromMinutes(5),
            CancellationToken.None);

        Assert.Equal("first", first);
        Assert.Equal("second", second);
    }
}
