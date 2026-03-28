using SportsStore.Application.Contracts.Messaging;

namespace SportsStore.Tests.Application;

public class PaymentDecisionServiceTests
{
    [Fact]
    public void Evaluate_Approves_Most_Payments()
    {
        var service = new PaymentDecisionService();
        var integrationEvent = new InventoryConfirmedIntegrationEvent
        {
            OrderId = 11,
            CorrelationId = "corr-pay-1",
            ReservationReference = "INV-11"
        };

        PaymentDecisionResult result = service.Evaluate(integrationEvent);

        Assert.True(result.Approved);
        Assert.False(string.IsNullOrWhiteSpace(result.PaymentReference));
        Assert.Null(result.Reason);
    }

    [Fact]
    public void Evaluate_Rejects_Specific_Test_Card_Number()
    {
        var service = new PaymentDecisionService();
        var integrationEvent = new InventoryConfirmedIntegrationEvent
        {
            OrderId = 12,
            CorrelationId = "corr-pay-2",
            ReservationReference = "INV-12"
        };

        PaymentDecisionResult result = service.Evaluate(integrationEvent, "4000000000000002");

        Assert.False(result.Approved);
        Assert.Null(result.PaymentReference);
        Assert.Equal("Rejected test card.", result.Reason);
    }

    [Fact]
    public void Evaluate_Rejects_Deterministic_Sample_Orders()
    {
        var service = new PaymentDecisionService();
        var integrationEvent = new InventoryConfirmedIntegrationEvent
        {
            OrderId = 19,
            CorrelationId = "corr-pay-3",
            ReservationReference = "INV-19"
        };

        PaymentDecisionResult result = service.Evaluate(integrationEvent);

        Assert.False(result.Approved);
        Assert.Null(result.PaymentReference);
        Assert.Equal("Simulated payment rejection.", result.Reason);
    }
}
