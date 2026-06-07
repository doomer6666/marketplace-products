using Google.Protobuf.WellKnownTypes;
using Marketplace.Products.Api.Protos;
using Marketplace.Products.Domain;

namespace Marketplace.Products.Api.Mappers;

public static class ProductDtoMapper
{
    public static ProductDto ToProductDto(this Product product)
    {
        return new ProductDto
               {
                   Id = product.Id.ToString(),
                   Name = product.Name,
                   Description = product.Description,
                   Price = product.Price,
                   Weight = product.Weight,
                   Category = product.Category.ToProto(),
                   CreatedAt = Timestamp.FromDateTime(
                       DateTime.SpecifyKind(product.CreatedAt, DateTimeKind.Utc)),
                   UpdatedAt = Timestamp.FromDateTime(
                       DateTime.SpecifyKind(product.UpdatedAt, DateTimeKind.Utc))
               };
    }
}