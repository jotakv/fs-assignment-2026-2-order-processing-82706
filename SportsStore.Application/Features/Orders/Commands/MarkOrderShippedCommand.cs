using MediatR;
using SportsStore.Application.Abstractions.Persistence;

namespace SportsStore.Application.Features.Orders.Commands;

public sealed record MarkOrderShippedCommand(int OrderID) : IRequest<bool>;

public sealed class MarkOrderShippedCommandHandler : IRequestHandler<MarkOrderShippedCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    public MarkOrderShippedCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<bool> Handle(MarkOrderShippedCommand request, CancellationToken cancellationToken)
    {
        bool marked = await _orderRepository.MarkShippedAsync(request.OrderID, cancellationToken);

        if (!marked)
        {
            throw new KeyNotFoundException($"Order {request.OrderID} was not found.");
        }

        return true;
    }
}
