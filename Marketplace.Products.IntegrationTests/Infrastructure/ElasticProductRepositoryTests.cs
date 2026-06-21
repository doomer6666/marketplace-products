using FluentAssertions;
using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Domain;
using Marketplace.Products.Infrastructure.Implementation;
using Marketplace.Products.IntegrationTests.Fixtures;

namespace Marketplace.Products.IntegrationTests.Infrastructure;

public class ElasticProductRepositoryTests(ElasticFixture fixture) : IClassFixture<ElasticFixture>, IAsyncLifetime
{
    private readonly Product[] _mockProducts =
    [
        Product.Import(
            Guid.NewGuid(),
            "Apple iPhone 15 Pro Ultra",
            "Smartphone",
            1200m,
            0.2,
            ProductCategory.ELECTRONICS,
            DateTime.UtcNow,
            DateTime.UtcNow),

        Product.Import(
            Guid.NewGuid(),
            "Children Toy Car",
            "Red car",
            6m,
            2,
            ProductCategory.CHILDREN_GOODS,
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-10)),

        Product.Import(
            Guid.NewGuid(),
            "Big Children Truck",
            "Large truck",
            15m,
            5,
            ProductCategory.CHILDREN_GOODS,
            DateTime.UtcNow.AddDays(-5),
            DateTime.UtcNow.AddDays(-5))
    ];

    private readonly ElasticProductRepository _repository = new(fixture.ConnectionString);

    public async Task InitializeAsync() => await fixture.ClearDatabase();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedElasticsearchAsync()
    {
        foreach (var product in _mockProducts)
        {
            await _repository.IndexProductAsync(product);
        }

        await fixture.ForceRefresh();
    }

    [Fact]
    public async Task Index_And_Search_BySearchTerm_ShouldReturnProduct()
    {
        // Arrange
        var product = _mockProducts[0];

        // Act
        await _repository.IndexProductAsync(product);

        await fixture.ForceRefresh();

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

        var filter = new ProductFilterDto
                     {
                         SearchTerm = "Children", MaxPrice = 9, Category = ProductCategory.CHILDREN_GOODS
                     };
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
                         Category = ProductCategory.CHILDREN_GOODS, SortBy = ProductSortBy.PriceDesc
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
        await fixture.ForceRefresh();

        var filter = new ProductFilterDto { SearchTerm = "iPhone" };
        var result = await _repository.SearchAsync(filter);

        // Assert
        result.Should().BeEmpty();
    }
}