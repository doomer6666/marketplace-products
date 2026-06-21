using FluentAssertions;
using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Infrastructure.Implementation;
using Marketplace.Products.IntegrationTests.Fixtures;

namespace Marketplace.Products.IntegrationTests.Infrastructure;

public class RedisCacheServiceTests(RedisFixture fixture) : IClassFixture<RedisFixture>, IAsyncLifetime
{
    private readonly RedisCacheService _cacheService = new(fixture.Connection);


    public async Task InitializeAsync() => await fixture.ClearDatabase();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SetAsync_And_GetAsync_ShouldReturnCachedData()
    {
        // Arrange
        const string key = "test_key";
        var expectedData = new ProductFilterDto { SearchTerm = "iPhone" };

        // Act
        await _cacheService.SetAsync(key, expectedData, TimeSpan.FromMinutes(1));
        var retrievedData = await _cacheService.GetAsync<ProductFilterDto>(key);

        // Assert
        retrievedData.Should().NotBeNull();
        retrievedData!.SearchTerm.Should().Be("iPhone");
    }
}