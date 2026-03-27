namespace SportsStore.Application.Abstractions.Payments;

public interface IPaymentService
{
    Task<PaymentSessionDto> CreateCheckoutSessionAsync(
        IEnumerable<PaymentCheckoutItem> items,
        string successUrl,
        string cancelUrl,
        string? customerEmail,
        string correlationId,
        CancellationToken cancellationToken);

    Task<PaymentSessionDto> GetCheckoutSessionAsync(string sessionId, CancellationToken cancellationToken);
}

public sealed record PaymentCheckoutItem(string Name, long UnitAmountCents, int Quantity);

public sealed record PaymentSessionDto(
    string SessionId,
    string CheckoutUrl,
    string PaymentStatus,
    string? PaymentIntentId);
