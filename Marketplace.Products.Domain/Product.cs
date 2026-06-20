namespace Marketplace.Products.Domain;

public class Product
{
    private Product()
    {
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public double Weight { get; private set; }
    public decimal Price { get; private set; }
    public ProductCategory Category { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static Product Create(
        string name,
        string description,
        decimal price,
        double weight,
        ProductCategory category)
    {
        var now = DateTime.UtcNow;

        return new Product
               {
                   Id = Guid.NewGuid(),
                   Name = name,
                   Description = description,
                   Price = price,
                   Weight = weight,
                   Category = category,
                   CreatedAt = now,
                   UpdatedAt = now
               };
    }

    public void Update(
        string name,
        string description,
        decimal price,
        double weight,
        ProductCategory category)
    {
        Name = name;
        Description = description;
        Price = price;
        Weight = weight;
        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Product Import(
        Guid id,
        string name,
        string description,
        decimal price,
        double weight,
        ProductCategory category,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new Product
               {
                   Id = id,
                   Name = name,
                   Description = description,
                   Price = price,
                   Weight = weight,
                   Category = category,
                   CreatedAt = createdAt,
                   UpdatedAt = updatedAt
               };
    }
}