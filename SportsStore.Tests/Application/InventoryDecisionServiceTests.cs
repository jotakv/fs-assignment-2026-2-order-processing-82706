using SportsStore.Application.Contracts.Messaging;

namespace SportsStore.Tests.Application;

public class InventoryDecisionServiceTests
{
    [Fact]
    public void Evaluate_Returns_Confirmed_When_All_Items_Fit_Available_Stock()
    {
        var service = new InventoryDecisionService();
        var integrationEvent = new OrderSubmittedIntegrationEvent
        {
            OrderId = 10,
            CorrelationId = "corr-1",
            Items =
            [
                new OrderSubmittedLineItem { ProductId = 1, Quantity = 1, UnitPrice = 10m },
                new OrderSubmittedLineItem { ProductId = 2, Quantity = 2, UnitPrice = 20m }
            ]
        };

        InventoryDecisionResult result = service.Evaluate(integrationEvent);

        Assert.True(result.Succeeded);
        Assert.False(string.IsNullOrWhiteSpace(result.ReservationReference));
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public void Evaluate_Returns_Failed_When_Any_Item_Exceeds_Available_Stock()
    {
        var service = new InventoryDecisionService();
        var integrationEvent = new OrderSubmittedIntegrationEvent
        {
            OrderId = 11,
            CorrelationId = "corr-2",
            Items =
            [
                new OrderSubmittedLineItem { ProductId = 1, Quantity = 99, UnitPrice = 10m }
            ]
        };

        InventoryDecisionResult result = service.Evaluate(integrationEvent);

        Assert.False(result.Succeeded);
        Assert.Null(result.ReservationReference);
        Assert.Equal("Insufficient stock for one or more products.", result.FailureReason);
    }
}
