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

    private readonly Product[] _mockProducts =
    [
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Apple iPhone 15 Pro Ultra",
            Description = "Smartphone",
            Price = 1200m,
            Weight = 0.2,
            Category = ProductCategory.ELECTRONICS,
            CreatedAt = DateTime.UtcNow
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Children Toy Car",
            Description = "Red car",
            Price = 6m,
            Weight = 2,
            Category = ProductCategory.CHILDREN_GOODS,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Big Children Truck",
            Description = "Large truck",
            Price = 15m,
            Weight = 5,
            Category = ProductCategory.CHILDREN_GOODS,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        }
    ];

    public ElasticProductRepositoryTests(ElasticFixture fixture)
    {
        _fixture = fixture;
        _repository = new ElasticProductRepository(_fixture.ConnectionString);
    }

    public async Task InitializeAsync() => await _fixture.ClearDatabase();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedElasticsearchAsync()
    {
        foreach (var product in _mockProducts)
        {
            await _repository.IndexProductAsync(product);
        }
        await _fixture.ForceRefresh();
    }

    [Fact]
    public async Task Index_And_Search_BySearchTerm_ShouldReturnProduct()
    {
        // Arrange
        var product = _mockProducts[0];

        // Act
        await _repository.IndexProductAsync(product);

        await _fixture.ForceRefresh();

        var filter = new ProductFilterDto { SearchTerm = "iPhone", PageNumber = 1, PageSize = 10 };
        var result = await _repository.SearchAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task Index_And_Search_ByFilters_ShouldReturnProduct()
    {
        // Act
        await SeedElasticsearchAsync();

        var filter = new ProductFilterDto { SearchTerm = "Children", MaxPrice = 9, Category = ProductCategory.CHILDREN_GOODS };
        var result = await _repository.SearchAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(_mockProducts[1].Id);
    }

    [Fact]
    public async Task Search_SortByPriceDesc_ShouldReturnMostExpensiveFirst()
    {
        await SeedElasticsearchAsync();

        var filter = new ProductFilterDto
        {
            Category = ProductCategory.CHILDREN_GOODS,
            SortBy = ProductSortBy.PriceDesc
        };
        var result = await _repository.SearchAsync(filter);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(_mockProducts[2].Id);
        result[1].Id.Should().Be(_mockProducts[1].Id);
    }

    [Fact]
    public async Task Delete_ShouldRemoveProductFromIndex()
    {
        await SeedElasticsearchAsync();
        var idToDelete = _mockProducts[0].Id;

        // Act
        await _repository.DeleteProductAsync(idToDelete);
        await _fixture.ForceRefresh();

        var filter = new ProductFilterDto { SearchTerm = "iPhone" };
        var result = await _repository.SearchAsync(filter);

        // Assert
        result.Should().BeEmpty();
    }
}