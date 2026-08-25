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

    /// <summary>
    /// Deletes a category, all nested subcategories, and all products in that tree.
    /// </summary>
    [HttpDelete("{id:int}")]
    [AdminAuthorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        var allCategories = await _db.Categories
            .Select(c => new { c.Id, c.ParentCategoryId })
            .ToListAsync(cancellationToken);

        if (!allCategories.Any(c => c.Id == id))
        {
            return NotFound();
        }

        var childrenByParent = allCategories
            .Where(c => c.ParentCategoryId is not null)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Id).ToList());

        var categoryIdsToDelete = new HashSet<int> { id };
        var queue = new Queue<int>();
        queue.Enqueue(id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!childrenByParent.TryGetValue(current, out var children))
            {
                continue;
            }

            foreach (var childId in children)
            {
                if (categoryIdsToDelete.Add(childId))
                {
                    queue.Enqueue(childId);
                }
            }
        }

        var products = await _db.Products
            .Where(p => categoryIdsToDelete.Contains(p.CategoryId))
            .ToListAsync(cancellationToken);

        if (products.Count > 0)
        {
            _db.Products.RemoveRange(products);
        }

        // Delete deepest categories first to satisfy the self-referencing Restrict FK.
        var depth = new Dictionary<int, int>();
        foreach (var categoryId in categoryIdsToDelete)
        {
            depth[categoryId] = GetDepth(categoryId, allCategories.ToDictionary(c => c.Id, c => c.ParentCategoryId));
        }

        var categoriesToDelete = await _db.Categories
            .Where(c => categoryIdsToDelete.Contains(c.Id))
            .ToListAsync(cancellationToken);

        foreach (var category in categoriesToDelete.OrderByDescending(c => depth[c.Id]))
        {
            _db.Categories.Remove(category);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static int GetDepth(int categoryId, Dictionary<int, int?> parentById)
    {
        var depth = 0;
        var current = categoryId;
        var guard = 0;

        while (parentById.TryGetValue(current, out var parentId) && parentId is int parent && guard < 64)
        {
            depth++;
            current = parent;
            guard++;
        }

        return depth;
    }
}
