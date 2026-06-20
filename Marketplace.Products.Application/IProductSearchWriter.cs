using Marketplace.Products.Domain;

namespace Marketplace.Products.Application;

public interface IProductSearchWriter
{
    public Task IndexProductAsync(Product product);

    public Task DeleteProductAsync(Guid id);
}