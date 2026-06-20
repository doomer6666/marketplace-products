using Marketplace.Products.Domain;

namespace Marketplace.Products.Infrastructure.Implementation;

public class ProductDocument
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