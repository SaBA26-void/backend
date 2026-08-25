namespace OnlineShop.Api.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<CategoryDto> Children { get; set; } = new();
}
