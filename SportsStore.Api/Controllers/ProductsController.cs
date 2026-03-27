using MediatR;
using Microsoft.AspNetCore.Mvc;
using SportsStore.Application.Common.Dtos;
using SportsStore.Application.Features.Products.Commands;
using SportsStore.Application.Features.Products.Queries;

namespace SportsStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType<PagedProductsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedProductsDto>> GetProducts(
        [FromQuery] string? category,
        [FromQuery] int productPage = 1,
        [FromQuery] int pageSize = 4,
        CancellationToken cancellationToken = default)
    {
        PagedProductsDto result = await _sender.Send(
            new GetProductsQuery(category, productPage, pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("categories")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCategories(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<string> result = await _sender.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{productId:long}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductById(long productId, CancellationToken cancellationToken = default)
    {
        ProductDto? result = await _sender.Send(new GetProductByIdQuery(productId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<ProductDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ProductDto result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProductById), new { productId = result.ProductID }, result);
    }

    [HttpPut("{productId:long}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        long productId,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        command.ProductID = productId;

        try
        {
            ProductDto result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{productId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(long productId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _sender.Send(new DeleteProductCommand(productId), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
