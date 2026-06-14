using Grpc.Core;
using Marketplace.Products.Api.Mappers;
using Marketplace.Products.Api.Protos;
using Marketplace.Products.Application;
using Marketplace.Products.Application.DTOs;

namespace Marketplace.Products.Api.GrpcServices;

public class ProductGrpcEndpoint(IProductService productService)
    : ProductServiceGrpc.ProductServiceGrpcBase
{
    public override async Task<CreateProductResponse> CreateProduct(CreateProductRequest request,
                                                                    ServerCallContext context)
    {
        var dto = new CreateProductDto(
            request.Name,
            request.Description,
            request.Price,
            request.Weight,
            request.Category.ToDomain()
        );

        var id = await productService.CreateProduct(dto);
        return new CreateProductResponse { Id = id.ToString() };
    }

    public override async Task<GetProductByIdResponse> GetProductById(GetProductByIdRequest request,
                                                                      ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var validGuid))
        {
            throw new RpcException(new Status(StatusCode.NotFound,
                $"Product with id {request.Id} not found"));
        }

        var product = await productService.GetProductById(validGuid);

        return new GetProductByIdResponse { Product = product.ToProductDto() };
    }

    public override async Task<UpdateProductByIdResponse> UpdateProductById(
        UpdateProductByIdRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var validGuid))
        {
            throw new RpcException(new Status(StatusCode.NotFound,
                $"Product with id {request.Id} not found"));
        }

        var dto = new UpdateProductDto
                  {
                      Name = request.HasName ? request.Name : null,
                      Description = request.HasDescription ? request.Description : null,
                      Price = request.Price != null ? (decimal)request.Price : null,
                      Weight = request.HasWeight ? request.Weight : null,
                      Category = request.HasCategory ? request.Category.ToDomain() : null
                  };

        var updatedProduct = await productService.UpdateProductById(validGuid, dto);
        return new UpdateProductByIdResponse { Product = updatedProduct.ToProductDto() };
    }

    public override async Task<DeleteProductByIdResponse> DeleteProductById(
        DeleteProductByIdRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var validGuid))
        {
            throw new RpcException(new Status(StatusCode.NotFound,
                $"Product with id {request.Id} not found"));
        }

        await productService.DeleteProductById(validGuid);

        return new DeleteProductByIdResponse();
    }

    public override async Task<GetProductListResponse> GetProductList(GetProductListRequest request,
                                                                      ServerCallContext context)
    {
        var dto = new ProductFilterDto
                  {
                      CreatedAt = request.CreatedAt != null ? request.CreatedAt.ToDateTime() : null,
                      Category = request.HasCategory ? request.Category.ToDomain() : null,
                      SearchTerm = request.HasSearchTerm ? request.SearchTerm : null,
                      MaxPrice = request.MaxPrice != null ? (decimal)request.MaxPrice : null,
                      PageNumber = request.HasPageNumber ? request.PageNumber : 1,
                      PageSize = request.HasPageSize ? request.PageSize : 20
                  };

        var productList = await productService.GetFilteredProductList(dto);

        var result = productList.Select(p => p.ToProductDto()).ToList();
        return new GetProductListResponse { Products = { result } };
    }
}