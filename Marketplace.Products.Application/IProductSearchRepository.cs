using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Domain;

namespace Marketplace.Products.Application;

public interface IProductSearchRepository
{
    public Task IndexProductAsync(Product product);
    public Task DeleteProductAsync(Guid id);
    public Task<List<Product>> SearchAsync(ProductFilterDto filter);
}