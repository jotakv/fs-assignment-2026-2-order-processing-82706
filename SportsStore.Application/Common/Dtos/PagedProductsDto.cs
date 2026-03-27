namespace SportsStore.Application.Common.Dtos;

public sealed class PagedProductsDto
{
    public IReadOnlyList<ProductDto> Products { get; set; } = [];

    public PaginationDto Pagination { get; set; } = new();

    public string? CurrentCategory { get; set; }
}
