using MediatR;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Dtos;

namespace SportsStore.Application.Features.Orders.Queries;

public sealed record GetOrderStatusQuery(int OrderId) : IRequest<OrderStatusDto?>;

public sealed class GetOrderStatusQueryHandler : IRequestHandler<GetOrderStatusQuery, OrderStatusDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderStatusQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderStatusDto?> Handle(GetOrderStatusQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        return order is null
            ? null
            : new OrderStatusDto
            {
                OrderID = order.OrderID,
                Status = order.Status.ToString(),
                UpdatedAtUtc = order.UpdatedAtUtc,
                CompletedAtUtc = order.CompletedAtUtc,
                FailedAtUtc = order.FailedAtUtc
            };
    }
}
