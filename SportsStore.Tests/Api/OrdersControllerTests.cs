using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SportsStore.Api.Controllers;
using SportsStore.Application.Common.Dtos;
using SportsStore.Application.Features.Orders.Commands;
using SportsStore.Application.Features.Orders.Queries;
using SportsStore.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace SportsStore.Tests.Api;

public class OrdersControllerTests
{
    [Fact]
    public async Task Checkout_Returns_BadRequest_When_Handler_Rejects_Request()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(mediator => mediator.Send(It.IsAny<CheckoutOrderCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Sorry, your cart is empty!"));

        var controller = new OrdersController(sender.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        ActionResult<CheckoutSessionDto> response =
            await controller.Checkout(new CheckoutOrderCommand(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response.Result);
        sender.Verify(mediator => mediator.Send(It.IsAny<CheckoutOrderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrders_Uses_Status_Query_When_Filter_Is_Provided()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(mediator => mediator.Send(It.IsAny<GetOrdersByStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var controller = new OrdersController(sender.Object);

        ActionResult<IReadOnlyList<OrderDto>> response =
            await controller.GetOrders(OrderStatus.PaymentApproved, CancellationToken.None);

        Assert.IsType<OkObjectResult>(response.Result);
        sender.Verify(mediator => mediator.Send(
            It.Is<GetOrdersByStatusQuery>(query => query.Status == OrderStatus.PaymentApproved),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrderStatus_Returns_NotFound_When_Order_Is_Missing()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(mediator => mediator.Send(It.IsAny<GetOrderStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderStatusDto?)null);

        var controller = new OrdersController(sender.Object);

        ActionResult<OrderStatusDto> response = await controller.GetOrderStatus(123, CancellationToken.None);

        Assert.IsType<NotFoundResult>(response.Result);
    }
}
