using FluentAssertions;
using Marketplace.Products.Application;
using Marketplace.Products.Domain;
using Marketplace.Products.Infrastructure.Helpers;
using Marketplace.Products.Infrastructure.Implementation;
using Marketplace.Products.IntegrationTests.Fixtures;

namespace Marketplace.Products.IntegrationTests.Infrastructure;

public class ProductRepositoryTests : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private readonly IProductRepository _repository;

    public ProductRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        var connectionFactory = new PostgresConnectionFactory(_fixture.ConnectionString);
        _repository = new ProductRepository(connectionFactory);
    }

    public async Task InitializeAsync() => await _fixture.ClearDatabase();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_And_GetById_ShouldReturnCorrectProduct()
    {
        var newProduct = new Product
                         {
                             Id = Guid.NewGuid(),
                             Name = "Test Smartphone",
                             Description = "Very good phone",
                             Price = 999.99m,
                             Weight = 0.2,
                             Category = ProductCategory.ELECTRONICS
                         };

        // Act
        await _repository.Add(newProduct);
        var retrievedProduct = await _repository.GetById(newProduct.Id);

        // Assert
        retrievedProduct.Should().NotBeNull();
        retrievedProduct!.Name.Should().Be("Test Smartphone");
        retrievedProduct.Price.Should().Be(999.99m);
        retrievedProduct.Category.Should().Be(ProductCategory.ELECTRONICS);

        retrievedProduct.CreatedAt.Should().BeAfter(DateTime.MinValue);
        retrievedProduct.UpdatedAt.Should().BeAfter(DateTime.MinValue);
    }

    [Fact]
    public async Task Update_ShouldUpdateProduct()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = new Product
                      {
                          Id = id,
                          Name = "Old Name",
                          Description = "Old Description",
                          Price = 100,
                          Weight = 1.5,
                          Category = ProductCategory.ELECTRONICS
                      };
        await _repository.Add(product);

        product.Name = "New Name";
        product.Price = 250;
        product.Category = ProductCategory.CHILDREN_GOODS;

        // Act
        await _repository.UpdateById(product);

        // Assert
        var fromDb = await _repository.GetById(id);

        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("New Name");
        fromDb.Description.Should().Be("Old Description");
        fromDb.Price.Should().Be(250);
        fromDb.Category.Should().Be(ProductCategory.CHILDREN_GOODS);
    }

    [Fact]
    public async Task Update_ShouldUpdateProduct_And_FireTrigger()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = new Product
                      {
                          Id = id,
                          Name = "Old Name",
                          Description = "Old Description",
                          Price = 100,
                          Weight = 1.5,
                          Category = ProductCategory.ELECTRONICS
                      };
        await _repository.Add(product);

        var createdProduct = await _repository.GetById(id);
        var originalUpdatedAt = createdProduct!.UpdatedAt;

        await Task.Delay(1000);

        product.Name = "New Name";

        // Act
        await _repository.UpdateById(product);

        // Assert
        var fromDb = await _repository.GetById(id);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("New Name");

        fromDb.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        fromDb.UpdatedAt.Should().BeAfter(fromDb.CreatedAt);
    }

    [Fact]
    public async Task Update_NonExistentProduct_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var fakeProduct = new Product
                          {
                              Id = Guid.NewGuid(),
                              Name = "Ghost",
                              Description = "Ghost",
                              Price = 100,
                              Weight = 2.5,
                              Category = ProductCategory.ELECTRONICS
                          };

        // Act
        var act = async () => await _repository.UpdateById(fakeProduct);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteById_ShouldDeleteProduct()
    {
        // Arrange
        var id = Guid.NewGuid();
        await _repository.Add(new Product
                              {
                                  Id = id,
                                  Name = "To Delete",
                                  Description = "D",
                                  Price = 100,
                                  Weight = 1,
                                  Category = ProductCategory.ELECTRONICS
                              });

        // Act
        await _repository.DeleteById(id);

        // Assert
        var afterDelete = await _repository.GetById(id);
        afterDelete.Should().BeNull();
    }

    [Fact]
    public async Task DeleteById_NonExistentProduct_ShouldNotThrow()
    {
        var id = Guid.NewGuid();
        var act = async () => await _repository.DeleteById(id);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetById_NonExistentProduct_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetById(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Add_DuplicateId_ShouldThrowException()
    {
        // Arrange
        var product = new Product
                      {
                          Id = Guid.NewGuid(),
                          Name = "Unique Phone",
                          Price = 500,
                          Weight = 1,
                          Category = ProductCategory.ELECTRONICS
                      };
        await _repository.Add(product);

        // Act
        var act = async () => await _repository.Add(product);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}