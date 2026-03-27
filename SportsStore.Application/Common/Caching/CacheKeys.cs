namespace SportsStore.Application.Common.Caching;

public static class CacheKeys
{
    public const string CatalogPrefix = "catalog:";
    public const string Categories = $"{CatalogPrefix}categories";

    public static string Products(string? category, int page, int pageSize) =>
        $"{CatalogPrefix}products:category={category ?? "all"}:page={page}:size={pageSize}";

    public static string Product(long productId) => $"{CatalogPrefix}product:{productId}";
}
