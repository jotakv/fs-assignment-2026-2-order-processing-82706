using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SportsStore.Api.Controllers;
using SportsStore.Application.Features.Orders.Commands;
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

        ActionResult<SportsStore.Application.Common.Dtos.CheckoutSessionDto> response =
            await controller.Checkout(new CheckoutOrderCommand(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response.Result);
        sender.Verify(mediator => mediator.Send(It.IsAny<CheckoutOrderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
