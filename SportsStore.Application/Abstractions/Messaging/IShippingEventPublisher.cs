using SportsStore.Application.Contracts.Messaging;

namespace SportsStore.Application.Abstractions.Messaging;

public interface IShippingEventPublisher
{
    Task PublishCreatedAsync(ShippingCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken);

    Task PublishFailedAsync(ShippingFailedIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
