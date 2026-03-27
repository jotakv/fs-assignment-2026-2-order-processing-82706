using MediatR;
using SportsStore.Application.Abstractions.Caching;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Caching;

namespace SportsStore.Application.Features.Products.Queries;

public sealed record GetCategoriesQuery() : IRequest<IReadOnlyList<string>>;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<string>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogCache _catalogCache;

    public GetCategoriesQueryHandler(IProductRepository productRepository, ICatalogCache catalogCache)
    {
        _productRepository = productRepository;
        _catalogCache = catalogCache;
    }

    public Task<IReadOnlyList<string>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken) =>
        _catalogCache.GetOrCreateAsync(
            CacheKeys.Categories,
            async ct => (IReadOnlyList<string>)(await _productRepository.GetCategoriesAsync(ct)).ToArray(),
            TimeSpan.FromMinutes(15),
            cancellationToken)!;
}
