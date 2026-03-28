using SportsStore.Application.Contracts.Messaging;

namespace SportsStore.Tests.Application;

public class ShipmentCreationServiceTests
{
    [Fact]
    public void Create_Returns_Shipping_Created_For_Most_Orders()
    {
        var service = new ShipmentCreationService();
        var integrationEvent = new PaymentApprovedIntegrationEvent
        {
            OrderId = 21,
            CorrelationId = "corr-ship-1",
            PaymentReference = "PAY-21"
        };

        ShipmentCreationResult result = service.Create(integrationEvent);

        Assert.True(result.Succeeded);
        Assert.False(string.IsNullOrWhiteSpace(result.ShipmentReference));
        Assert.False(string.IsNullOrWhiteSpace(result.TrackingNumber));
        Assert.NotNull(result.EstimatedDispatchDateUtc);
        Assert.Null(result.Reason);
    }

    [Fact]
    public void Create_Returns_Failed_For_Deterministic_Failure_Orders()
    {
        var service = new ShipmentCreationService();
        var integrationEvent = new PaymentApprovedIntegrationEvent
        {
            OrderId = 20,
            CorrelationId = "corr-ship-2",
            PaymentReference = "PAY-20"
        };

        ShipmentCreationResult result = service.Create(integrationEvent);

        Assert.False(result.Succeeded);
        Assert.Null(result.ShipmentReference);
        Assert.Null(result.TrackingNumber);
        Assert.Null(result.EstimatedDispatchDateUtc);
        Assert.Equal("Simulated shipping allocation failure.", result.Reason);
    }
}
