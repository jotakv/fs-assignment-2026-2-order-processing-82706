using SportsStore.Application.Abstractions.Checkout;

namespace SportsStore.Infrastructure.Checkout;

public sealed class InMemoryPendingCheckoutStore : IPendingCheckoutStore
{
    private readonly Dictionary<string, PendingCheckout> _pendingCheckouts = new();
    private readonly Lock _lock = new();

    public Task SaveAsync(PendingCheckout pendingCheckout, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            CleanupExpired();
            _pendingCheckouts[pendingCheckout.PendingCheckoutId] = pendingCheckout;
        }

        return Task.CompletedTask;
    }

    public Task<PendingCheckout?> GetAsync(string pendingCheckoutId, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            CleanupExpired();
            _pendingCheckouts.TryGetValue(pendingCheckoutId, out PendingCheckout? pendingCheckout);
            return Task.FromResult(pendingCheckout);
        }
    }

    public Task RemoveAsync(string pendingCheckoutId, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _pendingCheckouts.Remove(pendingCheckoutId);
        }

        return Task.CompletedTask;
    }

    private void CleanupExpired()
    {
        string[] expiredKeys = _pendingCheckouts
            .Where(pair => pair.Value.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            .Select(pair => pair.Key)
            .ToArray();

        foreach (string expiredKey in expiredKeys)
        {
            _pendingCheckouts.Remove(expiredKey);
        }
    }
}
