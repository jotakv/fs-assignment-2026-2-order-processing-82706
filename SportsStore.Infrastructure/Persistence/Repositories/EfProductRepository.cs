using Microsoft.EntityFrameworkCore;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Domain.Entities;

namespace SportsStore.Infrastructure.Persistence.Repositories;

public sealed class EfProductRepository : IProductRepository
{
    private readonly StoreDbContext _context;

    public EfProductRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<PagedProductsResult> GetPagedAsync(
        string? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(product => product.Category == category);
        }

        int totalItems = await query.CountAsync(cancellationToken);
        List<Product> products = await query
            .OrderBy(product => product.ProductID)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedProductsResult(products, totalItems);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        await _context.Products
            .AsNoTracking()
            .Select(product => product.Category)
            .Distinct()
            .OrderBy(category => category)
            .ToListAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(long productId, CancellationToken cancellationToken) =>
        _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.ProductID == productId, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<long> productIds, CancellationToken cancellationToken)
    {
        long[] ids = productIds.Distinct().ToArray();

        return await _context.Products
            .Where(product => ids.Contains(product.ProductID))
            .ToListAsync(cancellationToken);
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product?> UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        Product? existing = await _context.Products.FirstOrDefaultAsync(
            candidate => candidate.ProductID == product.ProductID,
            cancellationToken);

        if (existing is null)
        {
            return null;
        }

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Category = product.Category;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(long productId, CancellationToken cancellationToken)
    {
        Product? existing = await _context.Products.FirstOrDefaultAsync(
            product => product.ProductID == productId,
            cancellationToken);

        if (existing is null)
        {
            return false;
        }

        _context.Products.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
