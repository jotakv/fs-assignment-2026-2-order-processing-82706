using SportsStore.Application.Common.Dtos;

namespace SportsStore.Blazor.Services.Api;

public sealed class CatalogApiClient
{
    private readonly HttpClient _httpClient;

    public CatalogApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedProductsDto> GetProductsAsync(string? category, int page, int pageSize)
    {
        string url = "api/products";
        List<string> queryParts = [];

        if (!string.IsNullOrWhiteSpace(category))
        {
            queryParts.Add($"category={Uri.EscapeDataString(category)}");
        }

        queryParts.Add($"productPage={page}");
        queryParts.Add($"pageSize={pageSize}");

        if (queryParts.Count > 0)
        {
            url = $"{url}?{string.Join("&", queryParts)}";
        }

        return await _httpClient.GetFromJsonAsync<PagedProductsDto>(url)
            ?? new PagedProductsDto();
    }

    public async Task<ProductDto?> GetProductAsync(long productId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/products/{productId}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>();
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync() =>
        await _httpClient.GetFromJsonAsync<string[]>("api/products/categories") ?? [];
}
