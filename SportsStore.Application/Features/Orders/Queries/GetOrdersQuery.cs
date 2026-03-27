using AutoMapper;
using MediatR;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Dtos;

namespace SportsStore.Application.Features.Orders.Queries;

public sealed record GetOrdersQuery() : IRequest<IReadOnlyList<OrderDto>>;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }
}
