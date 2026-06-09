namespace Marketplace.Products.Application.DTOs;

public class ProductPriceChangedEvent
{
    public Guid ProductId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ChangedAt { get; set; }
}