using AutoMapper;
using MediatR;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Dtos;
using SportsStore.Domain.Entities;

namespace SportsStore.Application.Features.Orders.Queries;

public sealed record GetOrdersByStatusQuery(OrderStatus Status) : IRequest<IReadOnlyList<OrderDto>>;

public sealed class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersByStatusQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByStatusAsync(request.Status, cancellationToken);
        return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }
}
