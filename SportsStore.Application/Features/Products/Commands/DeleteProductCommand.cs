using MediatR;
using SportsStore.Application.Abstractions.Caching;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Caching;

namespace SportsStore.Application.Features.Products.Commands;

public sealed record DeleteProductCommand(long ProductID) : IRequest<bool>;

public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogCache _catalogCache;

    public DeleteProductCommandHandler(IProductRepository productRepository, ICatalogCache catalogCache)
    {
        _productRepository = productRepository;
        _catalogCache = catalogCache;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        bool deleted = await _productRepository.DeleteAsync(request.ProductID, cancellationToken);

        if (!deleted)
        {
            throw new KeyNotFoundException($"Product {request.ProductID} was not found.");
        }

        await _catalogCache.RemoveByPrefixAsync(CacheKeys.CatalogPrefix, cancellationToken);
        return true;
    }
}
