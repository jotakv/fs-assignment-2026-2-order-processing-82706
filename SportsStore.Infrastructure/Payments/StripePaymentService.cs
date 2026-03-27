using Microsoft.Extensions.Options;
using SportsStore.Application.Abstractions.Payments;
using SportsStore.Infrastructure.Options;
using Stripe;
using Stripe.Checkout;

namespace SportsStore.Infrastructure.Payments;

public sealed class StripePaymentService : IPaymentService
{
    private readonly SessionService _sessionService = new();
    private readonly StripeOptions _options;

    public StripePaymentService(IOptions<StripeOptions> options)
    {
        _options = options.Value;
    }

    public async Task<PaymentSessionDto> CreateCheckoutSessionAsync(
        IEnumerable<PaymentCheckoutItem> items,
        string successUrl,
        string cancelUrl,
        string? customerEmail,
        string correlationId,
        CancellationToken cancellationToken)
    {
        EnsureApiKey();

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ClientReferenceId = correlationId,
            CustomerEmail = string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail,
            LineItems = items.Select(item => new SessionLineItemOptions
            {
                Quantity = item.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "eur",
                    UnitAmount = item.UnitAmountCents,
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Name
                    }
                }
            }).ToList()
        };

        Session session = await _sessionService.CreateAsync(options, cancellationToken: cancellationToken);
        return Map(session);
    }

    public async Task<PaymentSessionDto> GetCheckoutSessionAsync(string sessionId, CancellationToken cancellationToken)
    {
        EnsureApiKey();
        Session session = await _sessionService.GetAsync(sessionId, cancellationToken: cancellationToken);
        return Map(session);
    }

    private void EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException("Stripe SecretKey is missing. Set it in configuration or user secrets.");
        }

        StripeConfiguration.ApiKey = _options.SecretKey;
    }

    private static PaymentSessionDto Map(Session session) =>
        new(
            session.Id,
            session.Url ?? string.Empty,
            session.PaymentStatus ?? string.Empty,
            session.PaymentIntentId);
}
