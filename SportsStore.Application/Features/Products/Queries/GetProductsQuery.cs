using AutoMapper;
using MediatR;
using SportsStore.Application.Abstractions.Caching;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Caching;
using SportsStore.Application.Common.Dtos;

namespace SportsStore.Application.Features.Products.Queries;

public sealed record GetProductsQuery(string? Category, int ProductPage = 1, int PageSize = 4) : IRequest<PagedProductsDto>;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedProductsDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogCache _catalogCache;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        ICatalogCache catalogCache,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _catalogCache = catalogCache;
        _mapper = mapper;
    }

    public Task<PagedProductsDto> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
        _catalogCache.GetOrCreateAsync(
            CacheKeys.Products(request.Category, request.ProductPage, request.PageSize),
            async ct =>
            {
                var result = await _productRepository.GetPagedAsync(
                    request.Category,
                    request.ProductPage,
                    request.PageSize,
                    ct);

                return new PagedProductsDto
                {
                    Products = _mapper.Map<IReadOnlyList<ProductDto>>(result.Products),
                    Pagination = new PaginationDto
                    {
                        CurrentPage = request.ProductPage,
                        ItemsPerPage = request.PageSize,
                        TotalItems = result.TotalItems
                    },
                    CurrentCategory = request.Category
                };
            },
            TimeSpan.FromMinutes(5),
            cancellationToken)!;
}
