using FluentValidation;
using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Domain;

namespace Marketplace.Products.Application.Implementation;

public class ProductService(
    IProductRepository productRepository,
    IProductSearchRepository searchProductRepository,
    IValidator<CreateProductDto> createValidator,
    IValidator<UpdateProductDto> updateValidator,
    IValidator<ProductFilterDto> filterValidator,
    ICacheService cacheService,
    IMessageProducer messageProducer) : IProductService
{
    public async Task<Guid> CreateProduct(CreateProductDto dto)
    {
        await createValidator.ValidateAndThrowAsync(dto);
        var id = Guid.NewGuid();

        var newProduct = new Product
                         {
                             Id = id,
                             Name = dto.Name,
                             Description = dto.Description,
                             Price = dto.Price,
                             Weight = dto.Weight,
                             Category = dto.Category
                         };
        await productRepository.Add(newProduct);
        await searchProductRepository.IndexProductAsync(newProduct);
        return id;
    }

    public async Task DeleteProductById(Guid id)
    {
        await cacheService.RemoveAsync($"product-{id}");
        await productRepository.DeleteById(id);
        await searchProductRepository.DeleteProductAsync(id);
    }

    public async Task<List<Product>> GetFilteredProductList(ProductFilterDto filterDto)
    {
        await filterValidator.ValidateAndThrowAsync(filterDto);
        return await searchProductRepository.SearchAsync(filterDto);
    }

    public async Task<Product> GetProductById(Guid id)
    {
        var cacheKey = $"product-{id}";

        var cachedProduct = await cacheService.GetAsync<Product>(cacheKey);
        if (cachedProduct is not null)
        {
            return cachedProduct;
        }

        var product = await productRepository.GetById(id);
        if (product is null)
        {
            throw new KeyNotFoundException($"Product with id {id} not found");
        }

        await cacheService.SetAsync(cacheKey, product);
        return product;
    }

    public async Task<Product> UpdateProductById(Guid id, UpdateProductDto dto)
    {
        await updateValidator.ValidateAndThrowAsync(dto);
        var existingProduct = await productRepository.GetById(id);

        if (existingProduct is null)
        {
            throw new KeyNotFoundException($"Product with id '{id}' not found");
        }

        var oldPrice = existingProduct.Price;
        await cacheService.RemoveAsync($"product_{id}");

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

        var updatedProduct = await productRepository.UpdateById(existingProduct);
        if (oldPrice != updatedProduct.Price)
        {
            var priceEvent = new ProductPriceChangedEvent
                             {
                                 ProductId = updatedProduct.Id,
                                 OldPrice = oldPrice,
                                 NewPrice = updatedProduct.Price,
                                 ChangedAt = DateTime.UtcNow
                             };

            await messageProducer.PublishMessageAsync("product-price-updates",
                updatedProduct.Id.ToString(),
                priceEvent);
        }

        await searchProductRepository.IndexProductAsync(updatedProduct);
        return updatedProduct;
    }
}