using Marketplace.Products.Domain;

namespace Marketplace.Products.Application;

public interface IProductRepository
{
    public Task<Product?> GetById(Guid id);

    public Task Add(Product product);

    public Task<Product> UpdateById(Product product);

    public Task DeleteById(Guid id);

    public Task AddMany(IEnumerable<Product> products);
}