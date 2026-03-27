using SportsStore.Application.Common.Dtos;

namespace SportsStore.Application.Abstractions.Checkout;

public interface IPendingCheckoutStore
{
    Task SaveAsync(PendingCheckout pendingCheckout, CancellationToken cancellationToken);

    Task<PendingCheckout?> GetAsync(string pendingCheckoutId, CancellationToken cancellationToken);

    Task RemoveAsync(string pendingCheckoutId, CancellationToken cancellationToken);
}

public sealed class PendingCheckout
{
    public string PendingCheckoutId { get; init; } = string.Empty;

    public CheckoutShippingDetails ShippingDetails { get; init; } = new();

    public IReadOnlyList<CheckoutLineItemDto> Lines { get; init; } = [];

    public string? CustomerEmail { get; init; }

    public DateTimeOffset ExpiresAtUtc { get; init; }
}
