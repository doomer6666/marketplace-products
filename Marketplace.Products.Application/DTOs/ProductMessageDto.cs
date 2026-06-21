using Marketplace.Products.Domain;

namespace Marketplace.Products.Application.DTOs;

public record ProductMessageDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    double Weight,
    ProductCategory Category,
    DateTime CreatedAt,
    DateTime UpdatedAt
);