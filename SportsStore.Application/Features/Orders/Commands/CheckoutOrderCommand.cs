using MediatR;
using SportsStore.Application.Abstractions.Checkout;
using SportsStore.Application.Abstractions.Payments;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Dtos;
using System.ComponentModel.DataAnnotations;

namespace SportsStore.Application.Features.Orders.Commands;

public sealed class CheckoutOrderCommand : IRequest<CheckoutSessionDto>
{
    [Required(ErrorMessage = "Please enter a name")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Please enter the first address line")]
    public string? Line1 { get; set; }

    public string? Line2 { get; set; }

    public string? Line3 { get; set; }

    [Required(ErrorMessage = "Please enter a city name")]
    public string? City { get; set; }

    [Required(ErrorMessage = "Please enter a state name")]
    public string? State { get; set; }

    public string? Zip { get; set; }

    [Required(ErrorMessage = "Please enter a country name")]
    public string? Country { get; set; }

    public bool GiftWrap { get; set; }

    public string? CustomerEmail { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public List<CheckoutLineItemDto> Lines { get; set; } = [];
}

public sealed class CheckoutOrderCommandHandler : IRequestHandler<CheckoutOrderCommand, CheckoutSessionDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IPaymentService _paymentService;
    private readonly IPendingCheckoutStore _pendingCheckoutStore;
    private readonly ICheckoutUrlFactory _checkoutUrlFactory;

    public CheckoutOrderCommandHandler(
        IProductRepository productRepository,
        IPaymentService paymentService,
        IPendingCheckoutStore pendingCheckoutStore,
        ICheckoutUrlFactory checkoutUrlFactory)
    {
        _productRepository = productRepository;
        _paymentService = paymentService;
        _pendingCheckoutStore = pendingCheckoutStore;
        _checkoutUrlFactory = checkoutUrlFactory;
    }

    public async Task<CheckoutSessionDto> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Lines.Count == 0)
        {
            throw new ValidationException("Sorry, your cart is empty!");
        }

        List<long> productIds = request.Lines.Select(line => line.ProductID).Distinct().ToList();
        var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);

        if (products.Count != productIds.Count)
        {
            throw new ValidationException("One or more products could not be found.");
        }

        string pendingCheckoutId = Guid.NewGuid().ToString("N");
        CheckoutRedirectUrls urls = _checkoutUrlFactory.Create(pendingCheckoutId);

        var pendingCheckout = new PendingCheckout
        {
            PendingCheckoutId = pendingCheckoutId,
            CustomerEmail = request.CustomerEmail,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            ShippingDetails = new CheckoutShippingDetails
            {
                Name = request.Name,
                Line1 = request.Line1,
                Line2 = request.Line2,
                Line3 = request.Line3,
                City = request.City,
                State = request.State,
                Zip = request.Zip,
                Country = request.Country,
                GiftWrap = request.GiftWrap
            },
            Lines = request.Lines
        };

        await _pendingCheckoutStore.SaveAsync(pendingCheckout, cancellationToken);

        try
        {
            var paymentItems = request.Lines
                .Join(
                    products,
                    line => line.ProductID,
                    product => product.ProductID,
                    (line, product) => new PaymentCheckoutItem(
                        product.Name,
                        (long)(product.Price * 100m),
                        line.Quantity))
                .ToArray();

            PaymentSessionDto session = await _paymentService.CreateCheckoutSessionAsync(
                paymentItems,
                urls.SuccessUrl,
                urls.CancelUrl,
                request.CustomerEmail,
                request.CorrelationId,
                cancellationToken);

            return new CheckoutSessionDto
            {
                SessionId = session.SessionId,
                CheckoutUrl = session.CheckoutUrl,
                PendingCheckoutId = pendingCheckoutId
            };
        }
        catch
        {
            await _pendingCheckoutStore.RemoveAsync(pendingCheckoutId, cancellationToken);
            throw;
        }
    }
}
