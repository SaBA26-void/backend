namespace OnlineShop.Api.Entities;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public int StockQuantity { get; set; }

    public Product Product { get; set; } = null!;
}
