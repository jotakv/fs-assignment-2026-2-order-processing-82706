using System.Text.Json;
using Microsoft.JSInterop;

namespace SportsStore.Blazor.Services.Browser;

public sealed class SessionStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IJSRuntime _jsRuntime;

    public SessionStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        string? json = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", key);
        return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public Task SetAsync<T>(string key, T value) =>
        _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, JsonSerializer.Serialize(value, JsonOptions)).AsTask();

    public Task RemoveAsync(string key) =>
        _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", key).AsTask();
}
