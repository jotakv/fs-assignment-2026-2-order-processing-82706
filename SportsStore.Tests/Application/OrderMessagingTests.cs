using Moq;
using SportsStore.Application.Abstractions.Checkout;
using SportsStore.Application.Abstractions.Messaging;
using SportsStore.Application.Abstractions.Payments;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Dtos;
using SportsStore.Application.Contracts.Messaging;
using SportsStore.Application.Features.Orders.Commands;
using SportsStore.Domain.Entities;

namespace SportsStore.Tests.Application;

public class OrderMessagingTests
{
    [Fact]
    public async Task CompleteCheckout_Publishes_After_Order_Is_Persisted()
    {
        var sequence = new MockSequence();
        var orderRepository = new Mock<IOrderRepository>();
        var publisher = new Mock<IOrderEventPublisher>();

        orderRepository
            .InSequence(sequence)
            .Setup(repo => repo.GetByStripeSessionIdAsync("cs_test_456", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        orderRepository
            .InSequence(sequence)
            .Setup(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken _) =>
            {
                order.OrderID = 100;
                return order;
            });

        publisher
            .InSequence(sequence)
            .Setup(pub => pub.PublishOrderSubmittedAsync(It.IsAny<OrderSubmittedIntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var productRepository = new Mock<IProductRepository>();
        productRepository
            .Setup(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Product { ProductID = 2, Name = "Shoes", Price = 40m, Category = "Running", Description = "desc" }
            ]);

        var paymentService = new Mock<IPaymentService>();
        paymentService
            .Setup(service => service.GetCheckoutSessionAsync("cs_test_456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentSessionDto("cs_test_456", "https://stripe.example/session", "paid", "pi_456"));

        var pendingStore = new Mock<IPendingCheckoutStore>();
        pendingStore
            .Setup(store => store.GetAsync("pending_456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PendingCheckout
            {
                PendingCheckoutId = "pending_456",
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                CustomerEmail = "kevin@example.com",
                ShippingDetails = new CheckoutShippingDetails
                {
                    Name = "Kevin",
                    Line1 = "Main Street 1",
                    City = "Dublin",
                    State = "Dublin",
                    Country = "Ireland"
                },
                Lines = [new CheckoutLineItemDto { ProductID = 2, Quantity = 1 }]
            });

        var handler = new CompleteCheckoutCommandHandler(
            orderRepository.Object,
            productRepository.Object,
            paymentService.Object,
            pendingStore.Object,
            publisher.Object);

        await handler.Handle(
            new CompleteCheckoutCommand
            {
                SessionId = "cs_test_456",
                PendingCheckoutId = "pending_456"
            },
            CancellationToken.None);

        orderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(pub => pub.PublishOrderSubmittedAsync(
            It.Is<OrderSubmittedIntegrationEvent>(evt =>
                evt.OrderId == 100 &&
                evt.CorrelationId == "cs_test_456" &&
                evt.TotalAmount == 40m &&
                evt.Items.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
