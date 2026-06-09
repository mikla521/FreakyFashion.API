namespace FreakyFashion.API.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Image { get; set; } = string.Empty;
    public string UrlSlug { get; set; } = string.Empty;

    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
