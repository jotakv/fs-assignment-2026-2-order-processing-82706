namespace SportsStore.Domain.Entities;

public enum OrderStatus
{
    Submitted = 0,
    InventoryPending = 1,
    InventoryConfirmed = 2,
    InventoryFailed = 3,
    PaymentPending = 4,
    PaymentApproved = 5,
    PaymentFailed = 6,
    ShippingPending = 7,
    ShippingCreated = 8,
    Completed = 9,
    Failed = 10
}
