using FluentAssertions;
using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Domain;
using Marketplace.Products.Infrastructure.Implementation;
using Marketplace.Products.IntegrationTests.Fixtures;

namespace Marketplace.Products.IntegrationTests.Infrastructure;

public class ElasticProductRepositoryTests : IClassFixture<ElasticFixture>, IAsyncLifetime
{
    private readonly ElasticFixture _fixture;
    private readonly ElasticProductRepository _repository;

    public ElasticProductRepositoryTests(ElasticFixture fixture)
    {
        _fixture = fixture;
        _repository = new ElasticProductRepository(_fixture.ConnectionString);
    }

    public async Task InitializeAsync() => await _fixture.ClearDatabase();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Index_And_Search_BySearchTerm_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product
                      {
                          Id = Guid.NewGuid(),
                          Name = "Apple iPhone 15 Pro Ultra Extreme Supreme",
                          Description = "Smartphone",
                          Price = 1200,
                          Weight = 0.2,
                          Category = ProductCategory.ELECTRONICS,
                          CreatedAt = DateTime.UtcNow
                      };

        // Act
        await _repository.IndexProductAsync(product);

        await _fixture.ForceRefresh();

        var filter = new ProductFilterDto { SearchTerm = "iPhone", PageNumber = 1, PageSize = 10 };
        var result = await _repository.SearchAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(product.Id);
    }
}