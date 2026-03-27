using AutoMapper;
using MediatR;
using SportsStore.Application.Abstractions.Caching;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Caching;
using SportsStore.Application.Common.Dtos;
using SportsStore.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace SportsStore.Application.Features.Products.Commands;

public sealed class CreateProductCommand : IRequest<ProductDto>
{
    [Required(ErrorMessage = "Please enter a product name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a description")]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Please enter a positive price")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Please specify a category")]
    public string Category { get; set; } = string.Empty;
}

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogCache _catalogCache;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICatalogCache catalogCache,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _catalogCache = catalogCache;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        Product product = _mapper.Map<Product>(request);
        Product created = await _productRepository.AddAsync(product, cancellationToken);
        await _catalogCache.RemoveByPrefixAsync(CacheKeys.CatalogPrefix, cancellationToken);
        return _mapper.Map<ProductDto>(created);
    }
}
