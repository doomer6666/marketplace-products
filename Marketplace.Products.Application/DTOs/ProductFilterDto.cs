using Marketplace.Products.Domain;

namespace Marketplace.Products.Application.DTOs;

public record ProductFilterDto
{
    public DateTime? CreatedAt { get; init; }
    public ProductCategory? Category { get; init; }
    public string? SearchTerm { get; init; }
    public decimal? MaxPrice { get; init; }
    public decimal? MinPrice { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}