using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Domain;

namespace Marketplace.Products.Application;

public interface IProductService
{
    public Task<Guid> CreateProduct(CreateProductDto productDto);

    public Task<List<Product>> GetFilteredProductList(ProductFilterDto filterDto);

    public Task<Product> GetProductById(Guid id);

    public Task<Product> UpdateProductById(Guid id, UpdateProductDto updateProductByIdDto);

    public Task DeleteProductById(Guid id);
}