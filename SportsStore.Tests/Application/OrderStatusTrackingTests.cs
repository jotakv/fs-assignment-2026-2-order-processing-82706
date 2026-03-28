using Moq;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Dtos;
using SportsStore.Application.Features.Orders.Queries;
using SportsStore.Domain.Entities;

namespace SportsStore.Tests.Application;

public class OrderStatusTrackingTests
{
    [Fact]
    public async Task GetOrderStatus_Returns_Timeline_And_Failure_Reason()
    {
        var repository = new Mock<IOrderRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Order
            {
                OrderID = 42,
                Status = OrderStatus.Failed,
                CreatedAtUtc = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAtUtc = new DateTime(2026, 1, 1, 10, 15, 0, DateTimeKind.Utc),
                FailedAtUtc = new DateTime(2026, 1, 1, 10, 15, 0, DateTimeKind.Utc),
                InventoryRecords =
                [
                    new InventoryRecord
                    {
                        Succeeded = true,
                        ReservationReference = "INV-42",
                        ProcessedAtUtc = new DateTime(2026, 1, 1, 10, 5, 0, DateTimeKind.Utc)
                    }
                ],
                PaymentRecords =
                [
                    new PaymentRecord
                    {
                        Status = "rejected",
                        FailureReason = "Card declined",
                        ProcessedAtUtc = new DateTime(2026, 1, 1, 10, 15, 0, DateTimeKind.Utc)
                    }
                ]
            });

        var handler = new GetOrderStatusQueryHandler(repository.Object);

        OrderStatusDto? result = await handler.Handle(new GetOrderStatusQuery(42), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(nameof(OrderStatus.Failed), result!.Status);
        Assert.Equal("Card declined", result.FailureReason);
        Assert.Equal("PaymentRejected", result.LastEvent);
        Assert.Equal(3, result.Timeline.Count);
    }

    [Fact]
    public async Task GetOrderStatus_Returns_Completed_When_Shipping_Succeeds()
    {
        var repository = new Mock<IOrderRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Order
            {
                OrderID = 7,
                Status = OrderStatus.Completed,
                CreatedAtUtc = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAtUtc = new DateTime(2026, 1, 2, 10, 30, 0, DateTimeKind.Utc),
                CompletedAtUtc = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                ShipmentRecords =
                [
                    new ShipmentRecord
                    {
                        ShipmentReference = "SHP-7",
                        TrackingNumber = "TRK-7",
                        CreatedAtUtc = new DateTime(2026, 1, 2, 10, 30, 0, DateTimeKind.Utc)
                    }
                ]
            });

        var handler = new GetOrderStatusQueryHandler(repository.Object);

        OrderStatusDto? result = await handler.Handle(new GetOrderStatusQuery(7), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(nameof(OrderStatus.Completed), result!.Status);
        Assert.Equal("ShippingCreated", result.LastEvent);
        Assert.Null(result.FailureReason);
    }
}
