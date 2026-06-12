namespace Marketplace.Products.Domain.Events;

public class ProductSyncEvent
{
    public Guid Id { get; set; }
    public EventAction Action { get; init; }
    public Product? Product { get; init; }
}

public enum EventAction
{
    Create,
    Update,
    Delete
}