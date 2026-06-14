using Marketplace.Products.Domain;

namespace Marketplace.Products.Application.DTOs;

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    double Weight,
    ProductCategory Category);