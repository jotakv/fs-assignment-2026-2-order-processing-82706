namespace SportsStore.Application.Common.Dtos;

public sealed class CheckoutSessionDto
{
    public string SessionId { get; set; } = string.Empty;

    public string CheckoutUrl { get; set; } = string.Empty;

    public string PendingCheckoutId { get; set; } = string.Empty;
}
