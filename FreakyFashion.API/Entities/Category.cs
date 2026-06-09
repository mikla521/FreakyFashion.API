namespace FreakyFashion.API.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string UrlSlug { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
