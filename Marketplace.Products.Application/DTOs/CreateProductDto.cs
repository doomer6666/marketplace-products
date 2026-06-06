using Marketplace.Products.Domain;

namespace Marketplace.Products.Application.DTOs;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Weight { get; set; }
    public decimal Price { get; set; }
    public ProductCategory Category { get; set; }
}