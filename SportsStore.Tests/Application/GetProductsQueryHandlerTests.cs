using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using SportsStore.Application.Abstractions.Persistence;
using SportsStore.Application.Common.Dtos;
using SportsStore.Application.Features.Products.Queries;
using SportsStore.Application.Mapping;
using SportsStore.Domain.Entities;
using SportsStore.Infrastructure.Caching;

namespace SportsStore.Tests.Application;

public class GetProductsQueryHandlerTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<StoreMappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task Uses_Repository_And_Caches_Result()
    {
        var repository = new Mock<IProductRepository>();
        repository
            .Setup(repo => repo.GetPagedAsync("Soccer", 1, 4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedProductsResult(
            [
                new Product { ProductID = 1, Name = "Ball", Category = "Soccer", Price = 19.5m },
                new Product { ProductID = 2, Name = "Flags", Category = "Soccer", Price = 34.95m }
            ], 2));

        var cache = new MemoryCatalogCache(new MemoryCache(new MemoryCacheOptions()));
        var handler = new GetProductsQueryHandler(repository.Object, cache, CreateMapper());

        PagedProductsDto first = await handler.Handle(new GetProductsQuery("Soccer", 1, 4), CancellationToken.None);
        PagedProductsDto second = await handler.Handle(new GetProductsQuery("Soccer", 1, 4), CancellationToken.None);

        Assert.Equal(2, first.Products.Count);
        Assert.Equal("Ball", first.Products[0].Name);
        Assert.Equal(2, second.Pagination.TotalItems);
        repository.Verify(repo => repo.GetPagedAsync("Soccer", 1, 4, It.IsAny<CancellationToken>()), Times.Once);
    }
}
