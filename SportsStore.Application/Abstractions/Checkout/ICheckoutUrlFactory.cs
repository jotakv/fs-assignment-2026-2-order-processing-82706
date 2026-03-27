namespace SportsStore.Application.Abstractions.Checkout;

public interface ICheckoutUrlFactory
{
    CheckoutRedirectUrls Create(string pendingCheckoutId);
}
