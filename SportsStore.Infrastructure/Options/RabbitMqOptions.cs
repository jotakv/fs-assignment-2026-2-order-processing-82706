namespace SportsStore.Infrastructure.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Uri { get; set; } = string.Empty;

    public string HostName { get; set; } = string.Empty;

    public int Port { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string VirtualHost { get; set; } = string.Empty;

    public bool UseTls { get; set; }

    public string ExchangeName { get; set; } = "sportsstore.orders";

    public string OrderSubmittedRoutingKey { get; set; } = "order.submitted";

    public string InventoryConfirmedRoutingKey { get; set; } = "inventory.confirmed";

    public string InventoryFailedRoutingKey { get; set; } = "inventory.failed";

    public string InventoryQueueName { get; set; } = "sportsstore.inventory";

    public string PaymentApprovedRoutingKey { get; set; } = "payment.approved";

    public string PaymentRejectedRoutingKey { get; set; } = "payment.rejected";

    public string PaymentQueueName { get; set; } = "sportsstore.payment";

    public string ShippingCreatedRoutingKey { get; set; } = "shipping.created";

    public string ShippingFailedRoutingKey { get; set; } = "shipping.failed";

    public string ShippingQueueName { get; set; } = "sportsstore.shipping";
}
