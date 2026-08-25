using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Data;
using OnlineShop.Api.DTOs;
using OnlineShop.Api.Entities;
using OnlineShop.Api.Security;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.ParentCategoryId })
            .ToListAsync(cancellationToken);

        var lookup = categories.ToDictionary(
            c => c.Id,
            c => new CategoryDto { Id = c.Id, Name = c.Name });

        var roots = new List<CategoryDto>();

        foreach (var category in categories)
        {
            var dto = lookup[category.Id];

            if (category.ParentCategoryId is int parentId && lookup.TryGetValue(parentId, out var parent))
            {
                parent.Children.Add(dto);
            }
            else
            {
                roots.Add(dto);
            }
        }

        return Ok(roots);
    }

    [HttpPost]
    [AdminAuthorize]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        [FromBody] CreateCategoryDto request,
        CancellationToken cancellationToken)
    {
        if (request.ParentCategoryId is int parentId)
        {
            var parentExists = await _db.Categories.AnyAsync(c => c.Id == parentId, cancellationToken);
            if (!parentExists)
            {
                ModelState.AddModelError(nameof(request.ParentCategoryId), "Parent category does not exist.");
                return ValidationProblem(ModelState);
            }
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            ParentCategoryId = request.ParentCategoryId
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new CategoryDto { Id = category.Id, Name = category.Name };
        return CreatedAtAction(nameof(GetCategories), dto);
    }

    [HttpPut("{id:int}")]
    [AdminAuthorize]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(
        int id,
        [FromBody] UpdateCategoryDto request,
        CancellationToken cancellationToken)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        if (request.ParentCategoryId == id)
        {
            ModelState.AddModelError(nameof(request.ParentCategoryId), "A category cannot be its own parent.");
            return ValidationProblem(ModelState);
        }

        if (request.ParentCategoryId is int parentId)
        {
            var parentExists = await _db.Categories.AnyAsync(c => c.Id == parentId, cancellationToken);
            if (!parentExists)
            {
                ModelState.AddModelError(nameof(request.ParentCategoryId), "Parent category does not exist.");
                return ValidationProblem(ModelState);
            }
        }

        category.Name = request.Name.Trim();
        category.ParentCategoryId = request.ParentCategoryId;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new CategoryDto { Id = category.Id, Name = category.Name });
    }

    [HttpDelete("{id:int}")]
    [AdminAuthorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        var hasChildren = await _db.Categories.AnyAsync(c => c.ParentCategoryId == id, cancellationToken);
        if (hasChildren)
        {
            return BadRequest("Remove or reassign subcategories before deleting this category.");
        }

        var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id, cancellationToken);
        if (hasProducts)
        {
            return BadRequest("Move or delete products in this category before deleting it.");
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
