namespace OnlineShop.Api.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    public Category Category { get; set; } = null!;
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
