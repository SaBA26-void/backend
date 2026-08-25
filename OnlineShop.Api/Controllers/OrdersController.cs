using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Data;
using OnlineShop.Api.DTOs;
using OnlineShop.Api.Entities;
using OnlineShop.Api.Security;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Admin: list all delivery orders, newest first.
    /// </summary>
    [HttpGet]
    [AdminAuthorize]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<OrderDto>>> GetOrders(CancellationToken cancellationToken)
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(MapOrderDto())
            .ToListAsync(cancellationToken);

        return Ok(orders);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderDto request,
        CancellationToken cancellationToken)
    {
        var order = new Order
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PersonalNumber = request.PersonalNumber.Trim(),
            Address = request.Address.Trim(),
            City = request.City.Trim(),
            Comment = string.IsNullOrWhiteSpace(request.Comment)
                ? null
                : request.Comment.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            Items = request.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                ProductName = item.ProductName.Trim(),
                Size = string.IsNullOrWhiteSpace(item.Size) ? null : item.Size.Trim(),
                Color = string.IsNullOrWhiteSpace(item.Color) ? null : item.Color.Trim(),
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = await _db.Orders
            .AsNoTracking()
            .Where(o => o.Id == order.Id)
            .Select(MapOrderDto())
            .FirstAsync(cancellationToken);

        return CreatedAtAction(nameof(GetOrder), new { id = dto.Id }, dto);
    }

    [HttpGet("{id:int}")]
    [AdminAuthorize]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrder(int id, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(MapOrderDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    private static System.Linq.Expressions.Expression<Func<Order, OrderDto>> MapOrderDto() =>
        o => new OrderDto
        {
            Id = o.Id,
            FirstName = o.FirstName,
            LastName = o.LastName,
            PersonalNumber = o.PersonalNumber,
            Address = o.Address,
            City = o.City,
            Comment = o.Comment,
            TotalAmount = o.TotalAmount,
            CreatedAtUtc = o.CreatedAtUtc,
            Items = o.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                VariantId = i.VariantId,
                ProductName = i.ProductName,
                Size = i.Size,
                Color = i.Color,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        };
}
