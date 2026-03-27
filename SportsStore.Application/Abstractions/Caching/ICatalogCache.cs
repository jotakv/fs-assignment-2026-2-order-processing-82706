namespace SportsStore.Application.Abstractions.Caching;

public interface ICatalogCache
{
    Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken)
        where T : class;

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
