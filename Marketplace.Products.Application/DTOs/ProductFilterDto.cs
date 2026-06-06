using Marketplace.Products.Domain;

namespace Marketplace.Products.Application.DTOs;

public class ProductFilterDto
{
    public DateTime? CreatedAt { get; set; }
    public ProductCategory? Category { get; set; }
    public string? SearchTerm { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinPrice { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}