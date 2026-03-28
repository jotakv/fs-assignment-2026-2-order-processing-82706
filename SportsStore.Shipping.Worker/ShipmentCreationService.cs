using SportsStore.Application.Contracts.Messaging;

public interface IShipmentCreationService
{
    ShipmentCreationResult Create(PaymentApprovedIntegrationEvent integrationEvent);
}

public sealed class ShipmentCreationService : IShipmentCreationService
{
    public ShipmentCreationResult Create(PaymentApprovedIntegrationEvent integrationEvent)
    {
        bool success = Math.Abs(integrationEvent.OrderId % 20) != 0;
        if (!success)
        {
            return ShipmentCreationResult.Failed("Simulated shipping allocation failure.");
        }

        return ShipmentCreationResult.Success(
            shipmentReference: $"SHP-{integrationEvent.OrderId}-{Guid.NewGuid():N}",
            trackingNumber: $"TRK-{integrationEvent.OrderId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
            estimatedDispatchDateUtc: DateTime.UtcNow.Date.AddDays((integrationEvent.OrderId % 3) + 1));
    }
}

public sealed class ShipmentCreationResult
{
    private ShipmentCreationResult(bool succeeded, string? shipmentReference, string? trackingNumber, DateTime? estimatedDispatchDateUtc, string? reason)
    {
        Succeeded = succeeded;
        ShipmentReference = shipmentReference;
        TrackingNumber = trackingNumber;
        EstimatedDispatchDateUtc = estimatedDispatchDateUtc;
        Reason = reason;
    }

    public bool Succeeded { get; }

    public string? ShipmentReference { get; }

    public string? TrackingNumber { get; }

    public DateTime? EstimatedDispatchDateUtc { get; }

    public string? Reason { get; }

    public static ShipmentCreationResult Success(string shipmentReference, string trackingNumber, DateTime estimatedDispatchDateUtc) =>
        new(true, shipmentReference, trackingNumber, estimatedDispatchDateUtc, null);

    public static ShipmentCreationResult Failed(string reason) =>
        new(false, null, null, null, reason);
}
