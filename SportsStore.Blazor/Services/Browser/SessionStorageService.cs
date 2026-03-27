using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace SportsStore.Blazor.Services.Browser;

public sealed class SessionStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<SessionStorageService> _logger;

    public SessionStorageService(IJSRuntime jsRuntime, ILogger<SessionStorageService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            string? json = await _jsRuntime.InvokeAsync<string?>("sportsStoreStorage.get", key);
            return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to read session storage key {StorageKey}", key);
            throw;
        }
    }

    public async Task SetAsync<T>(string key, T value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("sportsStoreStorage.set", key, JsonSerializer.Serialize(value, JsonOptions));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to write session storage key {StorageKey}", key);
            throw;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("sportsStoreStorage.remove", key);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to remove session storage key {StorageKey}", key);
            throw;
        }
    }
}
