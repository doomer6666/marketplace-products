namespace Marketplace.Products.Domain;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Weight { get; set; }
    public decimal Price { get; set; }
    public ProductCategory Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}