using Microsoft.Extensions.Options;
using SportsStore.Application.Abstractions.Checkout;
using SportsStore.Infrastructure.Options;

namespace SportsStore.Infrastructure.Checkout;

public sealed class ConfigurationCheckoutUrlFactory : ICheckoutUrlFactory
{
    private readonly ClientAppOptions _options;

    public ConfigurationCheckoutUrlFactory(IOptions<ClientAppOptions> options)
    {
        _options = options.Value;
    }

    public CheckoutRedirectUrls Create(string pendingCheckoutId)
    {
        string baseUrl = _options.BaseUrl.TrimEnd('/');
        string successPath = _options.CheckoutSuccessPath.StartsWith('/') ? _options.CheckoutSuccessPath : $"/{_options.CheckoutSuccessPath}";
        string cancelPath = _options.CheckoutCancelPath.StartsWith('/') ? _options.CheckoutCancelPath : $"/{_options.CheckoutCancelPath}";

        return new CheckoutRedirectUrls(
            $"{baseUrl}{successPath}?pendingCheckoutId={pendingCheckoutId}&session_id={{CHECKOUT_SESSION_ID}}",
            $"{baseUrl}{cancelPath}?pendingCheckoutId={pendingCheckoutId}");
    }
}
