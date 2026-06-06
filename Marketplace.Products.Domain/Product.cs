namespace Marketplace.Products.Domain;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Weight { get; set; }
    public decimal Price { get; set; }
    public ProductCategory Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum ProductCategory
{
    ELECTRONICS,
    HOME_APPLIANCES,
    CLOTHING_AND_SHOES,
    HEALTH_AND_BEAUTY,
    JEWELRY_AND_WATCHES,
    CHILDREN_GOODS,
    SPORTS_AND_OUTDOORS,
    GROCERIES,
}