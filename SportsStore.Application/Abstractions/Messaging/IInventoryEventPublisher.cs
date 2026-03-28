using SportsStore.Application.Contracts.Messaging;

namespace SportsStore.Application.Abstractions.Messaging;

public interface IInventoryEventPublisher
{
    Task PublishConfirmedAsync(InventoryConfirmedIntegrationEvent integrationEvent, CancellationToken cancellationToken);

    Task PublishFailedAsync(InventoryFailedIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
