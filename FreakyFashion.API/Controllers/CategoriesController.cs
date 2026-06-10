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
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ILogger<CategoriesController> _logger;
    private readonly AppDbContext _db;

    public CategoriesController(ILogger<CategoriesController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    // GET /api/categories[?slug=xxx]
    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] string? slug)
    {
        _logger.LogInformation("Fetching categories.");

        var query = _db.Categories.Include(c => c.Products).AsQueryable();

        if (slug is not null)
            query = query.Where(c => c.UrlSlug == slug);

        var result = await query.Select(c => MapToDto(c)).ToListAsync();

        if (slug is not null)
            return Ok(result); // always 200, empty list if not found

        return Ok(result);
    }

    // GET /api/categories/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _db.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null) return NotFound();
        return Ok(MapToDto(category));
    }

    // POST /api/categories
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Image = dto.Image,
            UrlSlug = SlugHelper.GenerateSlug(dto.Name)
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id },
            new CreatedCategoryDto(category.Id, category.Name, category.Image, category.UrlSlug));
    }

    // PATCH /api/categories/{id}
    [Authorize]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchCategory(int id, [FromBody] JsonPatchDocument<Category> patchDoc)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        patchDoc.ApplyTo(category, ModelState);
        if (!ModelState.IsValid) return BadRequest(ModelState);

        category.UrlSlug = SlugHelper.GenerateSlug(category.Name);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/categories/{id}
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/categories/{categoryId}/products/{productId}
    [Authorize]
    [HttpDelete("{categoryId:int}/products/{productId:int}")]
    public async Task<IActionResult> RemoveProductFromCategory(int categoryId, int productId)
    {
        var category = await _db.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category is null) return NotFound();

        var product = category.Products.FirstOrDefault(p => p.Id == productId);
        if (product is null) return NotFound();

        category.Products.Remove(product);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static CategoryDto MapToDto(Category c) => new(
        c.Id,
        c.Name,
        c.Image,
        c.UrlSlug,
        c.Products.Select(p => new ProductInCategoryDto(
            p.Id, p.Name, p.Description, p.Price, p.Image, p.UrlSlug)).ToList()
    );
}
