using SportsStore.Application.Contracts.Messaging;

public interface IPaymentDecisionService
{
    PaymentDecisionResult Evaluate(InventoryConfirmedIntegrationEvent integrationEvent, string? cardNumber = null);
}

public sealed class PaymentDecisionService : IPaymentDecisionService
{
    private static readonly HashSet<string> RejectedTestCards =
    [
        "4000000000000002",
        "4000000000009995",
        "4000000000000341"
    ];

    public PaymentDecisionResult Evaluate(InventoryConfirmedIntegrationEvent integrationEvent, string? cardNumber = null)
    {
        if (!string.IsNullOrWhiteSpace(cardNumber) && RejectedTestCards.Contains(cardNumber))
        {
            return PaymentDecisionResult.Rejected("Rejected test card.");
        }

        bool approved = Math.Abs(integrationEvent.OrderId % 10) < 8;
        return approved
            ? PaymentDecisionResult.Success($"PAY-{integrationEvent.OrderId}-{Guid.NewGuid():N}")
            : PaymentDecisionResult.Rejected("Simulated payment rejection.");
    }
}

public sealed class PaymentDecisionResult
{
    private PaymentDecisionResult(bool approved, string? paymentReference, string? reason)
    {
        Approved = approved;
        PaymentReference = paymentReference;
        Reason = reason;
    }

    public bool Approved { get; }

    public string? PaymentReference { get; }

    public string? Reason { get; }

    public static PaymentDecisionResult Success(string paymentReference) =>
        new(true, paymentReference, null);

    public static PaymentDecisionResult Rejected(string reason) =>
        new(false, null, reason);
}
