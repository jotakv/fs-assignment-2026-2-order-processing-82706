using MediatR;
using Microsoft.AspNetCore.Mvc;
using SportsStore.Application.Common.Dtos;
using SportsStore.Application.Features.Orders.Queries;

namespace SportsStore.Api.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{customerId:int}/orders")]
    [ProducesResponseType<IReadOnlyList<OrderDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetCustomerOrders(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OrderDto> result = await _sender.Send(new GetCustomerOrdersQuery(customerId), cancellationToken);
        return Ok(result);
    }
}
