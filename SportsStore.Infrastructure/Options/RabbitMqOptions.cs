namespace SportsStore.Infrastructure.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string ExchangeName { get; set; } = "sportsstore.orders";

    public string OrderSubmittedRoutingKey { get; set; } = "order.submitted";

    public string InventoryConfirmedRoutingKey { get; set; } = "inventory.confirmed";

    public string InventoryFailedRoutingKey { get; set; } = "inventory.failed";

    public string InventoryQueueName { get; set; } = "sportsstore.inventory";

    public string PaymentApprovedRoutingKey { get; set; } = "payment.approved";

    public string PaymentRejectedRoutingKey { get; set; } = "payment.rejected";

    public string PaymentQueueName { get; set; } = "sportsstore.payment";
}
