using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Data;
using OnlineShop.Api.DTOs;
using OnlineShop.Api.Entities;
using OnlineShop.Api.Security;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedProductsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedProductsDto>> GetProducts(
        [FromQuery] int? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sort = "name_asc",
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return BadRequest("page must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest("pageSize must be between 1 and 100.");
        }

        var query = _db.Products.AsNoTracking().AsQueryable();

        if (categoryId is int filterCategoryId)
        {
            var categoryIds = await GetCategoryAndDescendantIdsAsync(filterCategoryId, cancellationToken);
            if (categoryIds.Count == 0)
            {
                return Ok(new PagedProductsDto
                {
                    Items = new List<ProductDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                });
            }

            query = query.Where(p => categoryIds.Contains(p.CategoryId));
        }

        query = ApplySort(query, sort);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapProductDto())
            .ToListAsync(cancellationToken);

        return Ok(new PagedProductsDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProduct(int id, CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(MapProductDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    [AdminAuthorize]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductDto request,
        CancellationToken cancellationToken)
    {
        var categoryExists = await _db.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            ModelState.AddModelError(nameof(request.CategoryId), "Category does not exist.");
            return ValidationProblem(ModelState);
        }

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId,
            ImageUrl = request.ImageUrl,
            Variants = request.Variants.Select(ToVariantEntity).ToList()
        };

        if (product.Variants.Count > 0)
        {
            product.StockQuantity = product.Variants.Sum(v => v.StockQuantity);
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == product.Id)
            .Select(MapProductDto())
            .FirstAsync(cancellationToken);

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, dto);
    }

    [HttpPut("{id:int}")]
    [AdminAuthorize]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        int id,
        [FromBody] UpdateProductDto request,
        CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        var categoryExists = await _db.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            ModelState.AddModelError(nameof(request.CategoryId), "Category does not exist.");
            return ValidationProblem(ModelState);
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;
        product.ImageUrl = request.ImageUrl;

        _db.ProductVariants.RemoveRange(product.Variants);
        product.Variants = request.Variants.Select(ToVariantEntity).ToList();
        product.StockQuantity = product.Variants.Count > 0
            ? product.Variants.Sum(v => v.StockQuantity)
            : request.StockQuantity;

        await _db.SaveChangesAsync(cancellationToken);

        var dto = await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == product.Id)
            .Select(MapProductDto())
            .FirstAsync(cancellationToken);

        return Ok(dto);
    }

    [HttpDelete("{id:int}")]
    [AdminAuthorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static IQueryable<Product> ApplySort(IQueryable<Product> query, string? sort) =>
        (sort ?? "name_asc").Trim().ToLowerInvariant() switch
        {
            "name_desc" => query.OrderByDescending(p => p.Name),
            "price_asc" => query.OrderBy(p => p.Price).ThenBy(p => p.Name),
            "price_desc" => query.OrderByDescending(p => p.Price).ThenBy(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

    private async Task<HashSet<int>> GetCategoryAndDescendantIdsAsync(
        int categoryId,
        CancellationToken cancellationToken)
    {
        var allCategories = await _db.Categories
            .AsNoTracking()
            .Select(c => new { c.Id, c.ParentCategoryId })
            .ToListAsync(cancellationToken);

        if (!allCategories.Any(c => c.Id == categoryId))
        {
            return new HashSet<int>();
        }

        var childrenByParent = allCategories
            .Where(c => c.ParentCategoryId is not null)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Id).ToList());

        var result = new HashSet<int> { categoryId };
        var queue = new Queue<int>();
        queue.Enqueue(categoryId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!childrenByParent.TryGetValue(current, out var children))
            {
                continue;
            }

            foreach (var childId in children)
            {
                if (result.Add(childId))
                {
                    queue.Enqueue(childId);
                }
            }
        }

        return result;
    }

    private static System.Linq.Expressions.Expression<Func<Product, ProductDto>> MapProductDto() =>
        p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.Name,
            ImageUrl = p.ImageUrl,
            Variants = p.Variants
                .OrderBy(v => v.Size)
                .ThenBy(v => v.Color)
                .Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Size = v.Size,
                    Color = v.Color,
                    StockQuantity = v.StockQuantity
                })
                .ToList()
        };

    private static ProductVariant ToVariantEntity(ProductVariantInputDto input) => new()
    {
        Size = string.IsNullOrWhiteSpace(input.Size) ? null : input.Size.Trim(),
        Color = string.IsNullOrWhiteSpace(input.Color) ? null : input.Color.Trim(),
        StockQuantity = input.StockQuantity
    };
}
