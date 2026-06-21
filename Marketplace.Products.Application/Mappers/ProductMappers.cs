using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Domain;

namespace Marketplace.Products.Application.Mappers;

public static class ProductMappers
{
    public static ProductMessageDto ToMessageDto(this Product product)
    {
        return new ProductMessageDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Weight,
            product.Category,
            product.CreatedAt,
            product.UpdatedAt
        );
    }

    public static Product ToProduct(this ProductMessageDto dto)
    {
        return Product.Import(
            dto.Id,
            dto.Name,
            dto.Description,
            dto.Price,
            dto.Weight,
            dto.Category,
            dto.CreatedAt,
            dto.UpdatedAt);
    }
}