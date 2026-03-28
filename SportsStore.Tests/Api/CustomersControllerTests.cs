using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SportsStore.Api.Controllers;
using SportsStore.Application.Common.Dtos;
using SportsStore.Application.Features.Orders.Queries;

namespace SportsStore.Tests.Api;

public class CustomersControllerTests
{
    [Fact]
    public async Task GetCustomerOrders_Forwards_Query_To_Mediator()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(mediator => mediator.Send(It.IsAny<GetCustomerOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var controller = new CustomersController(sender.Object);

        ActionResult<IReadOnlyList<OrderDto>> response = await controller.GetCustomerOrders(7, CancellationToken.None);

        Assert.IsType<OkObjectResult>(response.Result);
        sender.Verify(mediator => mediator.Send(
            It.Is<GetCustomerOrdersQuery>(query => query.CustomerId == 7),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
