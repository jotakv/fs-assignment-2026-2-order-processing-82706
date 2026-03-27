using AutoMapper;
using MediatR;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Dtos;

namespace SportsStore.Application.Features.Orders.Queries;

public sealed record GetOrderByIdQuery(int OrderID) : IRequest<OrderDto?>;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderID, cancellationToken);
        return order is null ? null : _mapper.Map<OrderDto>(order);
    }
}
