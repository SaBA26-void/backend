namespace OnlineShop.Api.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Color { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public Order Order { get; set; } = null!;
}
