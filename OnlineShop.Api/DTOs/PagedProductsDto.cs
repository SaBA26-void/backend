namespace OnlineShop.Api.DTOs;

public class PagedProductsDto
{
    public List<ProductDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
