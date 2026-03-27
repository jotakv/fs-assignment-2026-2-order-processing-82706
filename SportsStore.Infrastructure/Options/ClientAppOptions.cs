namespace SportsStore.Infrastructure.Options;

public sealed class ClientAppOptions
{
    public const string SectionName = "ClientApp";

    public string BaseUrl { get; set; } = "https://localhost:7085";

    public string CheckoutSuccessPath { get; set; } = "/checkout/complete";

    public string CheckoutCancelPath { get; set; } = "/checkout/cancel";
}
