using SportsStore.Application.Common.Dtos;
using SportsStore.Application.Features.Orders.Commands;

namespace SportsStore.Blazor.Services.Api;

public sealed class OrdersApiClient
{
    private readonly HttpClient _httpClient;

    public OrdersApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CheckoutSessionDto> CheckoutAsync(CheckoutOrderCommand command)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/orders/checkout", command);
        return await ReadResponseAsync<CheckoutSessionDto>(response);
    }

    public async Task<CheckoutCompletionDto> CompleteAsync(CompleteCheckoutCommand command)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/orders/complete", command);
        return await ReadResponseAsync<CheckoutCompletionDto>(response);
    }

    public async Task<OrderDto?> GetOrderAsync(int orderId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/orders/{orderId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        return await ReadResponseAsync<OrderDto>(response);
    }

    public async Task<OrderStatusDto?> GetOrderStatusAsync(int orderId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/orders/{orderId}/status");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        return await ReadResponseAsync<OrderStatusDto>(response);
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            T? result = await response.Content.ReadFromJsonAsync<T>();
            return result ?? throw new InvalidOperationException("The API returned an empty response.");
        }

        ApiError? error = await response.Content.ReadFromJsonAsync<ApiError>();
        throw new InvalidOperationException(error?.Error ?? response.ReasonPhrase ?? "The API request failed.");
    }

    private sealed class ApiError
    {
        public string? Error { get; set; }
    }
}
