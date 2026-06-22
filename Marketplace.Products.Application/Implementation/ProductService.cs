using FluentValidation;
using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Application.Events;
using Marketplace.Products.Application.Mappers;
using Marketplace.Products.Domain;

namespace Marketplace.Products.Application.Implementation;

public class ProductService(
    IProductRepository productRepository,
    IProductSearchReader searchReader,
    IValidator<CreateProductDto> createValidator,
    IValidator<UpdateProductDto> updateValidator,
    IValidator<ProductFilterDto> filterValidator,
    ICacheService cacheService,
    IMessageProducer messageProducer) : IProductService
{
    private const string TopicName = "product-sync-topic";

    public async Task<Guid> CreateProduct(CreateProductDto dto)
    {
        await createValidator.ValidateAndThrowAsync(dto);
        var newProduct = Product.Create(
            dto.Name,
            dto.Description,
            dto.Price,
            dto.Weight,
            dto.Category
        );
        await productRepository.Add(newProduct);
        await messageProducer.PublishMessageAsync(TopicName,
            newProduct.Id.ToString(),
            new ProductSyncEvent
            {
                Id = newProduct.Id, Action = EventAction.Create, MessageDto = newProduct.ToMessageDto()
            });
        return newProduct.Id;
    }

    public async Task DeleteProductById(Guid id)
    {
        await cacheService.RemoveAsync($"product-{id}");
        await productRepository.DeleteById(id);
        await messageProducer.PublishMessageAsync(TopicName,
            id.ToString(),
            new ProductSyncEvent { Id = id, Action = EventAction.Delete });
    }

    public async Task<List<Product>> GetFilteredProductList(ProductFilterDto filterDto)
    {
        await filterValidator.ValidateAndThrowAsync(filterDto);
        return await searchReader.SearchAsync(filterDto);
    }

    public async Task<Product> GetProductById(Guid id)
    {
        var cacheKey = $"product-{id}";

        var cachedProduct = await cacheService.GetAsync<ProductMessageDto>(cacheKey);
        if (cachedProduct is not null)
        {
            return cachedProduct.ToProduct();
        }

        var product = await productRepository.GetById(id);
        if (product is null)
        {
            throw new KeyNotFoundException($"Product with id {id} not found");
        }

        await cacheService.SetAsync(cacheKey, product.ToMessageDto());
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

        await cacheService.RemoveAsync($"product-{id}");

        existingProduct.Update(
            dto.Name ?? existingProduct.Name,
            dto.Description ?? existingProduct.Description,
            dto.Price ?? existingProduct.Price,
            dto.Weight ?? existingProduct.Weight,
            dto.Category ?? existingProduct.Category
        );


        var updatedProduct = await productRepository.UpdateById(existingProduct);

        await messageProducer.PublishMessageAsync(TopicName,
            updatedProduct.Id.ToString(),
            new ProductSyncEvent { Id = id, Action = EventAction.Update, MessageDto = existingProduct.ToMessageDto() });
        return updatedProduct;
    }
}