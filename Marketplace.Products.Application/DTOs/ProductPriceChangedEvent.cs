namespace Marketplace.Products.Application.DTOs;

public record ProductPriceChangedEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice,
    DateTime ChangedAt
);