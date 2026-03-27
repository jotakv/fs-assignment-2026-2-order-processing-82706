namespace SportsStore.Application.Common.Dtos;

public sealed class PaginationDto
{
    public int CurrentPage { get; set; }

    public int ItemsPerPage { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages =>
        ItemsPerPage == 0 ? 0 : (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
}
