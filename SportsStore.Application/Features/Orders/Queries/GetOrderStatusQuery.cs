using MediatR;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Dtos;
using SportsStore.Domain.Entities;

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
        if (order is null)
        {
            return null;
        }

        List<OrderStatusTimelineEntryDto> timeline = BuildTimeline(order)
            .OrderBy(entry => entry.OccurredAtUtc)
            .ToList();

        return new OrderStatusDto
        {
            OrderID = order.OrderID,
            Status = order.Status.ToString(),
            UpdatedAtUtc = order.UpdatedAtUtc,
            CompletedAtUtc = order.CompletedAtUtc,
            FailedAtUtc = order.FailedAtUtc,
            FailureReason = order.ShipmentRecords.LastOrDefault(record => !string.IsNullOrWhiteSpace(record.FailureReason))?.FailureReason
                ?? order.PaymentRecords.LastOrDefault(record => !string.IsNullOrWhiteSpace(record.FailureReason))?.FailureReason
                ?? order.InventoryRecords.LastOrDefault(record => !string.IsNullOrWhiteSpace(record.FailureReason))?.FailureReason,
            LastEvent = timeline.LastOrDefault()?.EventType,
            Timeline = timeline
        };
    }

    private static IEnumerable<OrderStatusTimelineEntryDto> BuildTimeline(Order order)
    {
        yield return new OrderStatusTimelineEntryDto
        {
            EventType = "OrderCreated",
            Status = OrderStatus.Submitted.ToString(),
            OccurredAtUtc = order.CreatedAtUtc
        };

        foreach (InventoryRecord record in order.InventoryRecords)
        {
            yield return new OrderStatusTimelineEntryDto
            {
                EventType = record.Succeeded ? "InventoryConfirmed" : "InventoryFailed",
                Status = (record.Succeeded ? OrderStatus.InventoryConfirmed : OrderStatus.InventoryFailed).ToString(),
                OccurredAtUtc = record.ProcessedAtUtc ?? order.UpdatedAtUtc,
                Detail = record.Succeeded ? record.ReservationReference : record.FailureReason
            };
        }

        foreach (PaymentRecord record in order.PaymentRecords)
        {
            bool approved = string.Equals(record.Status, "approved", StringComparison.OrdinalIgnoreCase)
                || string.Equals(record.Status, "paid", StringComparison.OrdinalIgnoreCase);

            yield return new OrderStatusTimelineEntryDto
            {
                EventType = approved ? "PaymentApproved" : "PaymentRejected",
                Status = (approved ? OrderStatus.PaymentApproved : OrderStatus.PaymentFailed).ToString(),
                OccurredAtUtc = record.ProcessedAtUtc ?? order.UpdatedAtUtc,
                Detail = approved ? record.ExternalPaymentId : record.FailureReason
            };
        }

        foreach (ShipmentRecord record in order.ShipmentRecords)
        {
            bool created = string.IsNullOrWhiteSpace(record.FailureReason);

            yield return new OrderStatusTimelineEntryDto
            {
                EventType = created ? "ShippingCreated" : "ShippingFailed",
                Status = (created ? OrderStatus.Completed : OrderStatus.Failed).ToString(),
                OccurredAtUtc = record.CreatedAtUtc ?? order.UpdatedAtUtc,
                Detail = created ? record.TrackingNumber : record.FailureReason
            };
        }
    }
}
