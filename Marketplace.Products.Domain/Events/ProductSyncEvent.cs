namespace Marketplace.Products.Domain.Events;

public class ProductSyncEvent
{
    public Guid Id { get; init; }
    public EventAction Action { get; init; }
    public Product? Product { get; init; }
}