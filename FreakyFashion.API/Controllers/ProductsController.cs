using FreakyFashion.API.Data;
using FreakyFashion.API.DTOs;
using FreakyFashion.API.Entities;
using FreakyFashion.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreakyFashion.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly AppDbContext _db;

    public ProductsController(ILogger<ProductsController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    // GET /api/products[?page=1&pageSize=10]
    // GET /api/products?slug=xxx
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Fetching products.");

        if (slug is not null)
        {
            var bySlug = await _db.Products
                .Where(p => p.UrlSlug == slug)
                .Select(p => MapToDto(p))
                .ToListAsync();
            return Ok(bySlug);
        }

        var query = _db.Products.AsQueryable();
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return Ok(new PagedResult<ProductDto>(items, page, pageSize, total));
    }

    // GET /api/products/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        _logger.LogInformation("Fetching product with id {id}.", id);

        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();
        return Ok(MapToDto(product));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        // ─────────────────────────────────────────────
        // 1. Grundvalidering (business rules)
        // ─────────────────────────────────────────────

        if (dto.Categories == null || dto.Categories.Count == 0)
        {
            ModelState.AddModelError("Categories", "At least one category must be selected.");
        }

        // ─────────────────────────────────────────────
        // 2. Hämta kategorier
        // ─────────────────────────────────────────────

        List<Category> categories = new();

        if (dto.Categories != null && dto.Categories.Count > 0)
        {
            categories = await _db.Categories
                .Where(c => dto.Categories.Contains(c.Id))
                .ToListAsync();

            var missingIds = dto.Categories
                .Except(categories.Select(c => c.Id))
                .ToList();

            if (missingIds.Any())
            {
                foreach (var id in missingIds)
                {
                    ModelState.AddModelError(
                        "Categories",
                        $"Category with id {id} does not exist."
                    );
                }
            }
        }

        // ─────────────────────────────────────────────
        // 3. Stoppa direkt om fel finns
        // ─────────────────────────────────────────────

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        // ─────────────────────────────────────────────
        // 4. Skapa produkt
        // ─────────────────────────────────────────────

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Image = dto.Image,
            UrlSlug = SlugHelper.GenerateSlug(dto.Name)
        };

        foreach (var cat in categories)
        {
            product.Categories.Add(cat);
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            MapToDto(product)
        );
    }

    // POST /api/products
    //[Authorize]
    //[HttpPost]
    //public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    //{
    //    var product = new Product
    //    {
    //        Name = dto.Name,
    //        Description = dto.Description,
    //        Price = dto.Price,
    //        Image = dto.Image,
    //        UrlSlug = SlugHelper.GenerateSlug(dto.Name)
    //    };

    //    if (dto.Categories.Count > 0)
    //    {
    //        var categories = await _db.Categories
    //            .Where(c => dto.Categories.Contains(c.Id))
    //            .ToListAsync();
    //        foreach (var cat in categories)
    //            product.Categories.Add(cat);
    //    }


    //    _db.Products.Add(product);
    //    await _db.SaveChangesAsync();

    //    return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, MapToDto(product));
    //}

    // PATCH /api/products/{id}
    [Authorize]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchProduct(int id, [FromBody] JsonPatchDocument<Product> patchDoc)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        var invalidOps = patchDoc.Operations
        .Where(o => o.path != null &&
                    o.path.TrimStart('/').Equals("id", StringComparison.OrdinalIgnoreCase))
        .ToList();

        if (invalidOps.Any())
        {
            foreach (var op in invalidOps)
            {
                ModelState.AddModelError(
                    "Product",
                    "Product id can't be changed"
                );
            }

            return ValidationProblem(ModelState);
        }

        patchDoc.ApplyTo(product, ModelState);
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Regenerate slug if name was patched
        product.UrlSlug = SlugHelper.GenerateSlug(product.Name);

        await _db.SaveChangesAsync();
        return Ok(MapToDto(product));
    }

    // DELETE /api/products/{id}
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ProductDto MapToDto(Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Image, p.UrlSlug);
}
