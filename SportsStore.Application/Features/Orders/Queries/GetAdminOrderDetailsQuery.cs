using MediatR;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Dtos;

namespace SportsStore.Application.Features.Orders.Queries;

public sealed record GetAdminOrderDetailsQuery(int OrderId) : IRequest<AdminOrderDetailsDto?>;

public sealed class GetAdminOrderDetailsQueryHandler : IRequestHandler<GetAdminOrderDetailsQuery, AdminOrderDetailsDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetAdminOrderDetailsQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<AdminOrderDetailsDto?> Handle(GetAdminOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var latestInventory = order.InventoryRecords.OrderBy(record => record.ProcessedAtUtc).LastOrDefault();
        var latestPayment = order.PaymentRecords.OrderBy(record => record.ProcessedAtUtc).LastOrDefault();
        var latestShipment = order.ShipmentRecords.OrderBy(record => record.CreatedAtUtc).LastOrDefault();

        return new AdminOrderDetailsDto
        {
            OrderID = order.OrderID,
            Status = order.Status.ToString(),
            CustomerName = order.Customer?.Name ?? order.Name,
            PaymentStatus = latestPayment?.Status ?? order.StripePaymentStatus,
            PaymentReference = latestPayment?.ExternalPaymentId ?? order.StripePaymentIntentId,
            InventoryResult = latestInventory is null ? null : latestInventory.Succeeded ? "Confirmed" : "Failed",
            InventoryReference = latestInventory?.ReservationReference,
            ShipmentReference = latestShipment?.ShipmentReference,
            TrackingNumber = latestShipment?.TrackingNumber,
            FailureReason = latestShipment?.FailureReason ?? latestPayment?.FailureReason ?? latestInventory?.FailureReason,
            UpdatedAtUtc = order.UpdatedAtUtc,
            Lines = order.Lines.Select(line => new OrderLineDto
            {
                ProductID = line.Product.ProductID,
                Name = line.Product.Name,
                Price = line.Product.Price,
                Quantity = line.Quantity,
                LineTotal = line.Product.Price * line.Quantity
            }).ToArray()
        };
    }
}
