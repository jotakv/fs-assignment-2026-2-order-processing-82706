using SportsStore.Domain.Entities;

namespace SportsStore.Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(int orderId, CancellationToken cancellationToken);

    Task<Order?> GetByStripeSessionIdAsync(string stripeSessionId, CancellationToken cancellationToken);

    Task<Order> AddAsync(Order order, CancellationToken cancellationToken);

    Task<bool> MarkShippedAsync(int orderId, CancellationToken cancellationToken);
}
