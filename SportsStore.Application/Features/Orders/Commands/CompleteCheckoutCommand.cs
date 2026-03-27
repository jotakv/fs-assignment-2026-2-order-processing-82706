using MediatR;
using SportsStore.Application.Abstractions.Checkout;
using SportsStore.Application.Abstractions.Payments;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Dtos;
using SportsStore.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace SportsStore.Application.Features.Orders.Commands;

public sealed class CompleteCheckoutCommand : IRequest<CheckoutCompletionDto>
{
    [Required]
    public string SessionId { get; set; } = string.Empty;

    [Required]
    public string PendingCheckoutId { get; set; } = string.Empty;
}

public sealed class CompleteCheckoutCommandHandler : IRequestHandler<CompleteCheckoutCommand, CheckoutCompletionDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPaymentService _paymentService;
    private readonly IPendingCheckoutStore _pendingCheckoutStore;

    public CompleteCheckoutCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IPaymentService paymentService,
        IPendingCheckoutStore pendingCheckoutStore)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _paymentService = paymentService;
        _pendingCheckoutStore = pendingCheckoutStore;
    }

    public async Task<CheckoutCompletionDto> Handle(CompleteCheckoutCommand request, CancellationToken cancellationToken)
    {
        var existingOrder = await _orderRepository.GetByStripeSessionIdAsync(request.SessionId, cancellationToken);
        if (existingOrder is not null)
        {
            return MapCompletion(existingOrder);
        }

        PaymentSessionDto session = await _paymentService.GetCheckoutSessionAsync(request.SessionId, cancellationToken);
        if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Payment has not completed.");
        }

        PendingCheckout? pendingCheckout = await _pendingCheckoutStore.GetAsync(request.PendingCheckoutId, cancellationToken);
        if (pendingCheckout is null)
        {
            throw new KeyNotFoundException("Pending checkout could not be found.");
        }

        var products = await _productRepository.GetByIdsAsync(
            pendingCheckout.Lines.Select(line => line.ProductID),
            cancellationToken);

        if (products.Count != pendingCheckout.Lines.Count)
        {
            throw new ValidationException("One or more products could not be found.");
        }

        var order = new Order
        {
            Name = pendingCheckout.ShippingDetails.Name,
            Line1 = pendingCheckout.ShippingDetails.Line1,
            Line2 = pendingCheckout.ShippingDetails.Line2,
            Line3 = pendingCheckout.ShippingDetails.Line3,
            City = pendingCheckout.ShippingDetails.City,
            State = pendingCheckout.ShippingDetails.State,
            Zip = pendingCheckout.ShippingDetails.Zip,
            Country = pendingCheckout.ShippingDetails.Country,
            GiftWrap = pendingCheckout.ShippingDetails.GiftWrap,
            StripeSessionId = session.SessionId,
            StripePaymentIntentId = session.PaymentIntentId,
            StripePaymentStatus = session.PaymentStatus,
            PaidAtUtc = DateTime.UtcNow,
            Lines = pendingCheckout.Lines
                .Join(
                    products,
                    line => line.ProductID,
                    product => product.ProductID,
                    (line, product) => new CartLine
                    {
                        Product = product,
                        Quantity = line.Quantity
                    })
                .ToArray()
        };

        Order savedOrder = await _orderRepository.AddAsync(order, cancellationToken);
        await _pendingCheckoutStore.RemoveAsync(request.PendingCheckoutId, cancellationToken);

        return MapCompletion(savedOrder);
    }

    private static CheckoutCompletionDto MapCompletion(Order order) =>
        new()
        {
            OrderID = order.OrderID,
            SessionId = order.StripeSessionId ?? string.Empty,
            PaymentStatus = order.StripePaymentStatus ?? string.Empty,
            PaidAtUtc = order.PaidAtUtc,
            ItemCount = order.Lines.Sum(line => line.Quantity),
            TotalAmount = order.Lines.Sum(line => line.Quantity * line.Product.Price)
        };
}
