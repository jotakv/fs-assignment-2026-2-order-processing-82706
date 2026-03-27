using AutoMapper;
using MediatR;
using SportsStore.Application.Abstractions.Caching;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Caching;
using SportsStore.Application.Common.Dtos;

namespace SportsStore.Application.Features.Products.Queries;

public sealed record GetProductByIdQuery(long ProductID) : IRequest<ProductDto?>;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogCache _catalogCache;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(
        IProductRepository productRepository,
        ICatalogCache catalogCache,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _catalogCache = catalogCache;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        ProductDto? product = await _catalogCache.GetOrCreateAsync(
            CacheKeys.Product(request.ProductID),
            async ct =>
            {
                var entity = await _productRepository.GetByIdAsync(request.ProductID, ct);
                return entity is null ? null : _mapper.Map<ProductDto>(entity);
            },
            TimeSpan.FromMinutes(10),
            cancellationToken);

        return product;
    }
}
