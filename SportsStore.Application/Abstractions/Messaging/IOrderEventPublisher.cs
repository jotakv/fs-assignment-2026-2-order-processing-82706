using SportsStore.Application.Contracts.Messaging;

namespace SportsStore.Application.Abstractions.Messaging;

public interface IOrderEventPublisher
{
    Task PublishOrderSubmittedAsync(OrderSubmittedIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
