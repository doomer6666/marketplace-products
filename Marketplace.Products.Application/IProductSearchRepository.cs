using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Domain;

namespace Marketplace.Products.Application;

public interface IProductSearchReader
{
    public Task<List<Product>> SearchAsync(ProductFilterDto filter);
}

public interface IProductSearchWriter
{
    public Task IndexProductAsync(Product product);
    public Task DeleteProductAsync(Guid id);
}