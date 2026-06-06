using Marketplace.Products.Domain;

namespace Marketplace.Products.Application.DTOs;

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public double? Weight { get; set; }
    public ProductCategory? Category { get; set; }
}