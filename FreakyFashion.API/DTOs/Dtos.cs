using System.ComponentModel.DataAnnotations;

namespace FreakyFashion.API.DTOs;

// ── Product DTOs ──────────────────────────────────────────────────────────────

public record ProductDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string Image,
    string UrlSlug
);

public record ProductInCategoryDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string Image,
    string UrlSlug
);

public record CreateProductDto
{
    [Required, MinLength(1)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Description { get; init; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; init; }

    public string Image { get; init; } = string.Empty;

    public List<int> Categories { get; init; } = new();
}

// ── Category DTOs ─────────────────────────────────────────────────────────────

public record CategoryDto(
    int Id,
    string Name,
    string Image,
    string UrlSlug,
    List<ProductInCategoryDto> Products
);

public record CreateCategoryDto
{
    [Required, MinLength(1)]
    public string Name { get; init; } = string.Empty;

    public string Image { get; init; } = string.Empty;
}

public record CreatedCategoryDto(int Id, string Name, string Image, string UrlSlug);

// ── Auth DTOs ─────────────────────────────────────────────────────────────────

public record LoginDto
{
    [Required] public string Username { get; init; } = string.Empty;
    [Required] public string Password { get; init; } = string.Empty;
}

public record TokenDto(string AccessToken, string TokenType, int ExpiresIn);

// ── Cart DTOs ─────────────────────────────────────────────────────────────────

public record CartDto(int Id, string SessionId, List<CartItemDto> Items);

public record CartItemDto(int Id, int ProductId, string ProductName, decimal Price, int Quantity);

public record AddToCartDto
{
    [Required] public int ProductId { get; init; }
    [Range(1, 100)] public int Quantity { get; init; } = 1;
}

public record UpdateCartItemDto
{
    [Range(0, 100)] public int Quantity { get; init; }
}

// ── Pagination ────────────────────────────────────────────────────────────────

public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount);
