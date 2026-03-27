using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SportsStore.Api.Controllers;
using SportsStore.Application.Common.Dtos;
using SportsStore.Application.Features.Products.Queries;

namespace SportsStore.Tests.Api;

public class ProductsControllerTests
{
    [Fact]
    public async Task GetProducts_Delegates_To_Mediator()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(mediator => mediator.Send(It.IsAny<GetProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedProductsDto
            {
                Products = [new ProductDto { ProductID = 1, Name = "Ball" }]
            });

        var controller = new ProductsController(sender.Object);

        ActionResult<PagedProductsDto> response = await controller.GetProducts("Soccer", 1, 4, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<PagedProductsDto>(ok.Value);
        Assert.Single(payload.Products);
        sender.Verify(mediator => mediator.Send(It.Is<GetProductsQuery>(query =>
            query.Category == "Soccer" && query.ProductPage == 1 && query.PageSize == 4), It.IsAny<CancellationToken>()), Times.Once);
    }
}
