using Microsoft.EntityFrameworkCore;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Domain.Entities;

namespace SportsStore.Infrastructure.Persistence.Repositories;

public sealed class EfOrderRepository : IOrderRepository
{
    private readonly StoreDbContext _context;

    public EfOrderRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken) =>
        await DetailedOrdersQuery()
            .OrderByDescending(order => order.PaidAtUtc)
            .ThenByDescending(order => order.OrderID)
            .ToListAsync(cancellationToken);

    public Task<IReadOnlyList<Order>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken) =>
        DetailedOrdersQuery()
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ContinueWith(task => (IReadOnlyList<Order>)task.Result, cancellationToken);

    public Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken) =>
        DetailedOrdersQuery()
            .Where(order => order.Status == status)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ContinueWith(task => (IReadOnlyList<Order>)task.Result, cancellationToken);

    public Task<Order?> GetByIdAsync(int orderId, CancellationToken cancellationToken) =>
        DetailedOrdersQuery().FirstOrDefaultAsync(order => order.OrderID == orderId, cancellationToken);

    public Task<Order?> GetByStripeSessionIdAsync(string stripeSessionId, CancellationToken cancellationToken) =>
        _context.Orders
            .AsNoTracking()
            .Include(order => order.Lines)
            .ThenInclude(line => line.Product)
            .FirstOrDefaultAsync(order => order.StripeSessionId == stripeSessionId, cancellationToken);

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken)
    {
        _context.AttachRange(order.Lines.Select(line => line.Product));

        foreach (OrderItem item in order.Items.Where(item => item.Product is not null))
        {
            _context.Attach(item.Product);
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<bool> MarkShippedAsync(int orderId, CancellationToken cancellationToken)
    {
        Order? order = await _context.Orders.FirstOrDefaultAsync(candidate => candidate.OrderID == orderId, cancellationToken);

        if (order is null)
        {
            return false;
        }

        order.Shipped = true;
        order.Status = OrderStatus.Completed;
        order.CompletedAtUtc ??= DateTime.UtcNow;
        order.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<Order> DetailedOrdersQuery() =>
        _context.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(order => order.Customer)
            .Include(order => order.Lines)
            .ThenInclude(line => line.Product)
            .Include(order => order.Items)
            .ThenInclude(item => item.Product)
            .Include(order => order.InventoryRecords)
            .Include(order => order.PaymentRecords)
            .Include(order => order.ShipmentRecords);
}
