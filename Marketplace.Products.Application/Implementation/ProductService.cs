using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Domain;

namespace Marketplace.Products.Application.Implementation;

public class ProductService(IProductRepository repository) : IProductService
{
    public async Task<Guid> CreateProduct(CreateProductDto dto)
    {
        var id = Guid.NewGuid();

        var newProduct = new Product
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Weight = dto.Weight,
            Category = dto.Category,
        };
        await repository.Add(newProduct);

        return id;
    }

    public async Task DeleteProductById(Guid id) => await repository.DeleteById(id);

    public async Task<List<Product>> GetFilteredProductList(ProductFilterDto filterDto) => await repository.GetFilteredList(filterDto);

    public async Task<Product> GetProductById(Guid id)
    {
        var product = await repository.GetById(id);
        if (product is null)
        {
            throw new KeyNotFoundException($"Product with id {id} not found");
        }

        return product;
    }

    public async Task<Product> UpdateProductById(Guid id, UpdateProductDto dto)
    {
        var existingProduct = await repository.GetById(id);

        if (existingProduct is null)
        {
            throw new KeyNotFoundException($"Product with id '{id}' not found");
        }
        existingProduct.Name = dto.Name ?? existingProduct.Name;
        existingProduct.Description = dto.Description ?? existingProduct.Description;

        if (dto.Price.HasValue)
        {
            existingProduct.Price = (decimal)dto.Price;
        }

        if (dto.Weight.HasValue)
        {
            existingProduct.Weight = (double)dto.Weight;
        }
        if (dto.Category.HasValue)
        {
            existingProduct.Category = (ProductCategory)dto.Category;
        }
        return await repository.UpdateById(existingProduct);
    }
}