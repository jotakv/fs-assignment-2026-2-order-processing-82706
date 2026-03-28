using AutoMapper;
using Moq;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Dtos;
using SportsStore.Application.Features.Orders.Queries;
using SportsStore.Application.Mapping;
using SportsStore.Domain.Entities;

namespace SportsStore.Tests.Application;

public class OrderQueriesTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<StoreMappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task GetCustomerOrders_Returns_Mapped_Orders()
    {
        var repository = new Mock<IOrderRepository>();
        repository
            .Setup(repo => repo.GetByCustomerIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Order
                {
                    OrderID = 10,
                    CustomerId = 7,
                    Status = OrderStatus.InventoryPending,
                    Lines = [new CartLine { Product = new Product { ProductID = 1, Name = "Ball", Price = 5m, Category = "Soccer", Description = "d" }, Quantity = 2 }]
                }
            ]);

        var handler = new GetCustomerOrdersQueryHandler(repository.Object, CreateMapper());

        IReadOnlyList<OrderDto> result = await handler.Handle(new GetCustomerOrdersQuery(7), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(10, result[0].OrderID);
        Assert.Equal(nameof(OrderStatus.InventoryPending), result[0].Status);
    }

    [Fact]
    public async Task GetOrderStatus_Returns_Null_When_Order_Does_Not_Exist()
    {
        var repository = new Mock<IOrderRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var handler = new GetOrderStatusQueryHandler(repository.Object);

        OrderStatusDto? result = await handler.Handle(new GetOrderStatusQuery(99), CancellationToken.None);

        Assert.Null(result);
    }
}
