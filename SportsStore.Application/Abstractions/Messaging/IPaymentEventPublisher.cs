using SportsStore.Application.Contracts.Messaging;

namespace SportsStore.Application.Abstractions.Messaging;

public interface IPaymentEventPublisher
{
    Task PublishApprovedAsync(PaymentApprovedIntegrationEvent integrationEvent, CancellationToken cancellationToken);

    Task PublishRejectedAsync(PaymentRejectedIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
