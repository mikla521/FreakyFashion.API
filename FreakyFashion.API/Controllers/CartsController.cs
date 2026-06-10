using FreakyFashion.API.Data;
using FreakyFashion.API.DTOs;
using FreakyFashion.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreakyFashion.API.Controllers;

/// <summary>
/// Session-based shopping cart. Pass X-Session-Id header to identify the cart.
/// </summary>
[ApiController]
[Route("api/cart")]
public class CartsController : ControllerBase
{
    private readonly ILogger<CartsController> _logger;
    private readonly AppDbContext _db;

    public CartsController(ILogger<CartsController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    private string GetSessionId() =>
        Request.Headers.TryGetValue("X-Session-Id", out var val) && val.Count > 0
            ? val[0]!
            : string.Empty;

    // GET /api/cart
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var sessionId = GetSessionId();
        if (string.IsNullOrEmpty(sessionId))
            return BadRequest(new { error = "X-Session-Id header is required." });

        var cart = await _db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId);

        if (cart is null)
            return Ok(new CartDto(0, sessionId, new List<CartItemDto>()));

        return Ok(MapToDto(cart));
    }

    // POST /api/cart/items
    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
    {
        var sessionId = GetSessionId();
        if (string.IsNullOrEmpty(sessionId))
            return BadRequest(new { error = "X-Session-Id header is required." });

        var product = await _db.Products.FindAsync(dto.ProductId);
        if (product is null) return NotFound(new { error = "Product not found." });

        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId);

        if (cart is null)
        {
            cart = new Cart { SessionId = sessionId };
            _db.Carts.Add(cart);
        }

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existing is not null)
            existing.Quantity += dto.Quantity;
        else
            cart.Items.Add(new CartItem { ProductId = dto.ProductId, Quantity = dto.Quantity });

        await _db.SaveChangesAsync();

        // Reload with product details
        await _db.Entry(cart).Collection(c => c.Items).Query()
            .Include(i => i.Product).LoadAsync();

        return Ok(MapToDto(cart));
    }

    // PUT /api/cart/items/{itemId}
    [HttpPut("items/{itemId:int}")]
    public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateCartItemDto dto)
    {
        var sessionId = GetSessionId();
        var cart = await _db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId);

        if (cart is null) return NotFound();

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return NotFound();

        if (dto.Quantity == 0)
            cart.Items.Remove(item);
        else
            item.Quantity = dto.Quantity;

        await _db.SaveChangesAsync();
        return Ok(MapToDto(cart));
    }

    // DELETE /api/cart
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var sessionId = GetSessionId();
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId);

        if (cart is null) return NoContent();

        _db.Carts.Remove(cart);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static CartDto MapToDto(Cart c) => new(
        c.Id,
        c.SessionId,
        c.Items.Select(i => new CartItemDto(
            i.Id,
            i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.Product?.Price ?? 0,
            i.Quantity)).ToList()
    );
}
