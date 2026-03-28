using SportsStore.Application.Contracts.Messaging;
using SportsStore.Domain.Entities;

public interface IInventoryDecisionService
{
    InventoryDecisionResult Evaluate(OrderSubmittedIntegrationEvent integrationEvent);
}

public sealed class InventoryDecisionService : IInventoryDecisionService
{
    public InventoryDecisionResult Evaluate(OrderSubmittedIntegrationEvent integrationEvent)
    {
        bool allAvailable = integrationEvent.Items.All(item => item.Quantity <= GetAvailableQuantity(item.ProductId));
        string reservationReference = $"INV-{integrationEvent.OrderId}-{Guid.NewGuid():N}";

        return allAvailable
            ? InventoryDecisionResult.Confirmed(reservationReference)
            : InventoryDecisionResult.Failed("Insufficient stock for one or more products.");
    }

    private static int GetAvailableQuantity(long productId) =>
        (int)(Math.Abs(productId % 5) + 1);
}

public sealed class InventoryDecisionResult
{
    private InventoryDecisionResult(bool succeeded, string? reservationReference, string? failureReason)
    {
        Succeeded = succeeded;
        ReservationReference = reservationReference;
        FailureReason = failureReason;
    }

    public bool Succeeded { get; }

    public string? ReservationReference { get; }

    public string? FailureReason { get; }

    public static InventoryDecisionResult Confirmed(string reservationReference) =>
        new(true, reservationReference, null);

    public static InventoryDecisionResult Failed(string failureReason) =>
        new(false, null, failureReason);
}
