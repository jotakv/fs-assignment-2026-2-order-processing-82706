using MediatR;
using Microsoft.AspNetCore.Mvc;
using SportsStore.Application.Common.Dtos;
using SportsStore.Application.Features.Orders.Commands;
using SportsStore.Application.Features.Orders.Queries;
using SportsStore.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace SportsStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OrderDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrders(
        [FromQuery] OrderStatus? status,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OrderDto> result = status is null
            ? await _sender.Send(new GetOrdersQuery(), cancellationToken)
            : await _sender.Send(new GetOrdersByStatusQuery(status.Value), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{orderId:int}")]
    [ProducesResponseType<OrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrderById(int orderId, CancellationToken cancellationToken = default)
    {
        OrderDto? result = await _sender.Send(new GetOrderByIdQuery(orderId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{orderId:int}/status")]
    [ProducesResponseType<OrderStatusDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderStatusDto>> GetOrderStatus(int orderId, CancellationToken cancellationToken = default)
    {
        OrderStatusDto? result = await _sender.Send(new GetOrderStatusQuery(orderId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{orderId:int}/admin")]
    [ProducesResponseType<AdminOrderDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminOrderDetailsDto>> GetAdminOrderDetails(int orderId, CancellationToken cancellationToken = default)
    {
        AdminOrderDetailsDto? result = await _sender.Send(new GetAdminOrderDetailsQuery(orderId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("checkout")]
    [ProducesResponseType<CheckoutSessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CheckoutSessionDto>> Checkout(
        [FromBody] CheckoutOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        command.CorrelationId = HttpContext.TraceIdentifier;

        try
        {
            CheckoutSessionDto result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPost("complete")]
    [ProducesResponseType<CheckoutCompletionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CheckoutCompletionDto>> CompleteCheckout(
        [FromBody] CompleteCheckoutCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            CheckoutCompletionDto result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { error = exception.Message });
        }
    }

    [HttpPost("{orderId:int}/ship")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkShipped(int orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _sender.Send(new MarkOrderShippedCommand(orderId), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
