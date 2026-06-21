using Marketplace.Products.Application.DTOs;

namespace Marketplace.Products.Application.Events;

public class ProductSyncEvent
{
    public Guid Id { get; init; }
    public EventAction Action { get; init; }
    public ProductMessageDto? MessageDto { get; init; }
}