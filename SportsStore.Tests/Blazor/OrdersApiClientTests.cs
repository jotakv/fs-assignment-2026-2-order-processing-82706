using System.Net;
using System.Net.Http;
using System.Text;
using SportsStore.Application.Common.Dtos;
using SportsStore.Blazor.Services.Api;

namespace SportsStore.Tests.Blazor;

public class OrdersApiClientTests
{
    [Fact]
    public async Task GetOrderStatusAsync_Returns_Null_For_NotFound()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = new OrdersApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") });

        OrderStatusDto? result = await client.GetOrderStatusAsync(123);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrderStatusAsync_Deserializes_Status_Response()
    {
        var payload = """
        {
          "orderID": 42,
          "status": "Completed",
          "updatedAtUtc": "2026-03-28T10:00:00Z",
          "completedAtUtc": "2026-03-28T11:00:00Z",
          "failedAtUtc": null,
          "failureReason": null,
          "lastEvent": "ShippingCreated",
          "timeline": []
        }
        """;

        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        });

        var client = new OrdersApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") });

        OrderStatusDto? result = await client.GetOrderStatusAsync(42);

        Assert.NotNull(result);
        Assert.Equal(42, result!.OrderID);
        Assert.Equal("Completed", result.Status);
        Assert.Equal("ShippingCreated", result.LastEvent);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_handler(request));
    }
}
