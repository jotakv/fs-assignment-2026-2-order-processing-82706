using AutoMapper;
using SportsStore.Application.Mapping;
using SportsStore.Domain.Entities;

namespace SportsStore.Tests.Domain;

public class OrderDomainModelTests
{
    [Fact]
    public void NewOrder_HasSubmittedStatusByDefault()
    {
        var order = new Order();

        Assert.Equal(OrderStatus.Submitted, order.Status);
        Assert.Empty(order.Items);
        Assert.Empty(order.InventoryRecords);
        Assert.Empty(order.PaymentRecords);
        Assert.Empty(order.ShipmentRecords);
    }

    [Fact]
    public void Mapping_OrderToDto_UsesOrderItemsForTotalsAndStatus()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<StoreMappingProfile>());
        var mapper = configuration.CreateMapper();

        var product = new Product
        {
            ProductID = 10,
            Name = "Ball",
            Price = 12.5m,
            Description = "desc",
            Category = "cat"
        };

        var order = new Order
        {
            Status = OrderStatus.Failed,
            Items =
            [
                new OrderItem
                {
                    Product = product,
                    ProductId = product.ProductID,
                    Quantity = 2,
                    UnitPrice = product.Price,
                    LineTotal = 25m
                }
            ],
            InventoryRecords =
            [
                new InventoryRecord
                {
                    ReservationReference = "INV-10",
                    Succeeded = true,
                    ProcessedAtUtc = DateTime.UtcNow.AddMinutes(-3)
                }
            ],
            PaymentRecords =
            [
                new PaymentRecord
                {
                    Status = "approved",
                    ProcessedAtUtc = DateTime.UtcNow.AddMinutes(-2)
                }
            ],
            ShipmentRecords =
            [
                new ShipmentRecord
                {
                    Carrier = "DHL",
                    ShipmentReference = "SHIP-10",
                    TrackingNumber = "TRACK-10",
                    CreatedAtUtc = DateTime.UtcNow.AddMinutes(-1),
                    FailureReason = "Simulated shipping failure."
                }
            ]
        };

        var dto = mapper.Map<SportsStore.Application.Common.Dtos.OrderDto>(order);

        Assert.Equal(nameof(OrderStatus.Failed), dto.Status);
        Assert.Equal(2, dto.ItemCount);
        Assert.Equal(25m, dto.TotalAmount);
        Assert.Equal("approved", dto.PaymentStatus);
        Assert.Equal("Confirmed", dto.InventoryResult);
        Assert.Equal("DHL", dto.ShipmentCarrier);
        Assert.Equal("Simulated shipping failure.", dto.FailureReason);
    }
}
