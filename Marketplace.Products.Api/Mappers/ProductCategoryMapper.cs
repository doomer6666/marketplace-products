using Marketplace.Products.Api.Protos;
using Marketplace.Products.Domain;

namespace Marketplace.Products.Api.Mappers;

public static class ProductCategoryMapper
{
    public static ProductCategory ToDomain(this GrpcProductCategory grpcCategory)
    {
        return grpcCategory switch
        {
            GrpcProductCategory.Electronics => ProductCategory.ELECTRONICS,
            GrpcProductCategory.HomeAppliances => ProductCategory.HOME_APPLIANCES,
            GrpcProductCategory.ClothingAndShoes => ProductCategory.CLOTHING_AND_SHOES,
            GrpcProductCategory.HealthAndBeauty => ProductCategory.HEALTH_AND_BEAUTY,
            GrpcProductCategory.JewelryAndWatches => ProductCategory.JEWELRY_AND_WATCHES,
            GrpcProductCategory.ChildrenGoods => ProductCategory.CHILDREN_GOODS,
            GrpcProductCategory.SportsAndOutdoors => ProductCategory.SPORTS_AND_OUTDOORS,
            GrpcProductCategory.Groceries => ProductCategory.GROCERIES,

            GrpcProductCategory.UnspecifiedCategory =>
                throw new ArgumentException("Category is required and cannot be unspecified.", nameof(grpcCategory)),

            _ => throw new ArgumentOutOfRangeException(nameof(grpcCategory),
                     $"Unexpected gRPC category: {grpcCategory}")
        };
    }

    public static GrpcProductCategory ToProto(this ProductCategory domainCategory)
    {
        return domainCategory switch
        {
            ProductCategory.ELECTRONICS => GrpcProductCategory.Electronics,
            ProductCategory.HOME_APPLIANCES => GrpcProductCategory.HomeAppliances,
            ProductCategory.CLOTHING_AND_SHOES => GrpcProductCategory.ClothingAndShoes,
            ProductCategory.HEALTH_AND_BEAUTY => GrpcProductCategory.HealthAndBeauty,
            ProductCategory.JEWELRY_AND_WATCHES => GrpcProductCategory.JewelryAndWatches,
            ProductCategory.CHILDREN_GOODS => GrpcProductCategory.ChildrenGoods,
            ProductCategory.SPORTS_AND_OUTDOORS => GrpcProductCategory.SportsAndOutdoors,
            ProductCategory.GROCERIES => GrpcProductCategory.Groceries,
            _ => throw new ArgumentOutOfRangeException(nameof(domainCategory),
                     $"Unexpected domain category: {domainCategory}")
        };
    }
}