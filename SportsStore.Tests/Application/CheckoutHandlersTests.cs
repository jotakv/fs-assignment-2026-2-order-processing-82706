using Moq;
using Microsoft.Extensions.Logging;
using SportsStore.Application.Abstractions.Checkout;
using SportsStore.Application.Abstractions.Messaging;
using SportsStore.Application.Abstractions.Payments;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Dtos;
using SportsStore.Application.Contracts.Messaging;
using SportsStore.Application.Features.Orders.Commands;
using SportsStore.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace SportsStore.Tests.Application;

public class CheckoutHandlersTests
{
    [Fact]
    public async Task Checkout_Throws_When_Cart_Is_Empty()
    {
        var handler = new CheckoutOrderCommandHandler(
            Mock.Of<IProductRepository>(),
            Mock.Of<IPaymentService>(),
            Mock.Of<IPendingCheckoutStore>(),
            Mock.Of<ICheckoutUrlFactory>());

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new CheckoutOrderCommand(), CancellationToken.None));
    }

    [Fact]
    public async Task Complete_Checkout_Saves_Order_After_Paid_Session()
    {
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(repo => repo.GetByStripeSessionIdAsync("cs_test_123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);
        orderRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken _) =>
            {
                order.OrderID = 42;
                return order;
            });

        var productRepository = new Mock<IProductRepository>();
        productRepository
            .Setup(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Product { ProductID = 1, Name = "Ball", Price = 25m, Category = "Soccer" }
            ]);

        var paymentService = new Mock<IPaymentService>();
        paymentService
            .Setup(service => service.GetCheckoutSessionAsync("cs_test_123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentSessionDto("cs_test_123", "https://stripe.example/session", "paid", "pi_123"));

        var pendingStore = new Mock<IPendingCheckoutStore>();
        pendingStore
            .Setup(store => store.GetAsync("pending_123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PendingCheckout
            {
                PendingCheckoutId = "pending_123",
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                ShippingDetails = new CheckoutShippingDetails
                {
                    Name = "Kevin",
                    Line1 = "Main Street 1",
                    City = "Madrid",
                    State = "Madrid",
                    Country = "Spain"
                },
                Lines = [new CheckoutLineItemDto { ProductID = 1, Quantity = 2 }]
            });

        var publisher = new Mock<IOrderEventPublisher>();

        var handler = new CompleteCheckoutCommandHandler(
            orderRepository.Object,
            productRepository.Object,
            paymentService.Object,
            pendingStore.Object,
            publisher.Object,
            Mock.Of<ILogger<CompleteCheckoutCommandHandler>>());

        CheckoutCompletionDto result = await handler.Handle(
            new CompleteCheckoutCommand
            {
                SessionId = "cs_test_123",
                PendingCheckoutId = "pending_123"
            },
            CancellationToken.None);

        Assert.Equal(42, result.OrderID);
        Assert.Equal("paid", result.PaymentStatus);
        Assert.Equal(50m, result.TotalAmount);
        orderRepository.Verify(repo => repo.AddAsync(It.Is<Order>(order => order.Lines.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(pub => pub.PublishOrderSubmittedAsync(
            It.Is<OrderSubmittedIntegrationEvent>(evt =>
                evt.OrderId == 42 &&
                evt.CorrelationId == "cs_test_123" &&
                evt.Items.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
        pendingStore.Verify(store => store.RemoveAsync("pending_123", It.IsAny<CancellationToken>()), Times.Once);
    }
}
