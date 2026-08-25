using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Api.DTOs;

public class CreateCategoryDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Null for a top-level category; set to create a subcategory.
    /// </summary>
    public int? ParentCategoryId { get; set; }
}

public class UpdateCategoryDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    public int? ParentCategoryId { get; set; }
}
