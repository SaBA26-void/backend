using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Api.DTOs;

public class CreateOrderItemDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    public int? VariantId { get; set; }

    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Size { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public class CreateOrderDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    [RegularExpression(
        @"^[\p{L}][\p{L}\s\-']*$",
        ErrorMessage = "First name must contain letters only (no numbers).")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    [RegularExpression(
        @"^[\p{L}][\p{L}\s\-']*$",
        ErrorMessage = "Last name must contain letters only (no numbers).")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "Personal number must be exactly 11 digits.")]
    public string PersonalNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string City { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Comment { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Order must contain at least one item.")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Color { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PersonalNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
