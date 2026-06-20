using Marketplace.Products.Domain;
using Marketplace.Products.Domain.Events;

namespace Marketplace.Products.Application.Implementation;

public class DevToolsService(
    IProductRepository productRepository,
    IMessageProducer messageProducer) : IDevToolsService
{
    private const string TopicName = "product-sync-topic";
    private const int BatchSize = 1000;

    public async Task GenerateFakeProducts(int count)
    {
        var batches = GenerateProductsLazily(count).Chunk(BatchSize);

        foreach (var batch in batches)
        {
            await productRepository.AddMany(batch);

            var kafkaTasks = batch.Select(product =>
            messageProducer.PublishMessageAsync(
                    TopicName,
                    product.Id.ToString(),
                    new ProductSyncEvent { Id = product.Id, Action = EventAction.Create, Product = product }
                    ));

            await Task.WhenAll(kafkaTasks);
        }
    }

    private IEnumerable<Product> GenerateProductsLazily(int count)
    {
        var random = new Random();
        var categories = new[] { 0, 1, 2 };

        var words = new[] { "Phone", "TV", "Car", "Laptop", "Soap", "Table", "Chair", "Monitor" };
        var adjectives = new[] { "Super", "Ultra", "Cheap", "Premium", "Children", "Gaming", "Office" };

        for (var i = 0; i < count; i++)
        {
            var category = categories[random.Next(categories.Length)];
            var name = $"{adjectives[random.Next(adjectives.Length)]} {words[random.Next(words.Length)]} {i}";
            var price = random.Next(10, 2000) + (decimal)random.NextDouble();
            var weight = Math.Round(random.NextDouble() * 10, 2);

            yield return new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = $"Auto-generated description {i}",
                Price = price,
                Category = (ProductCategory)category,
                Weight = weight,
            };
        }
    }
}