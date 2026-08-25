using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Api.DTOs;

public class ProductVariantDto
{
    public int Id { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public int StockQuantity { get; set; }
}

public class ProductVariantInputDto
{
    [StringLength(50)]
    public string? Size { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
    public int StockQuantity { get; set; }
}
