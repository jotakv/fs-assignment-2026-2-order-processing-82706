using SportsStore.Domain.Entities;

namespace SportsStore.Application.Abstractions.Persistence;

public interface IProductRepository
{
    Task<PagedProductsResult> GetPagedAsync(string? category, int page, int pageSize, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(long productId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<long> productIds, CancellationToken cancellationToken);

    Task<Product> AddAsync(Product product, CancellationToken cancellationToken);

    Task<Product?> UpdateAsync(Product product, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(long productId, CancellationToken cancellationToken);
}

public sealed record PagedProductsResult(IReadOnlyList<Product> Products, int TotalItems);
