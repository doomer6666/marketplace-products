using Marketplace.Products.Domain;

namespace Marketplace.Products.Application.DTOs;

public record UpdateProductDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public double? Weight { get; init; }
    public ProductCategory? Category { get; init; }
}