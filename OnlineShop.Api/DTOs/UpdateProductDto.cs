using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Api.DTOs;

public class UpdateProductDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
    public int StockQuantity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CategoryId is required.")]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(500)]
    [Url]
    public string ImageUrl { get; set; } = string.Empty;

    public List<ProductVariantInputDto> Variants { get; set; } = new();
}
