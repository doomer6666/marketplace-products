using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Domain;
using Marketplace.Products.IntegrationTests.Fixtures;

namespace Marketplace.Products.IntegrationTests.Api;

public class ProductControllerTests : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    private const string ApiRoute = "/api/v1/products";
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;
    private readonly PostgresFixture _fixture;

    public ProductControllerTests(PostgresFixture fixture)
    {
        _fixture = fixture;

        _factory = new MarketplaceApiFactory(_fixture.ConnectionString);
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync() => await _fixture.ClearDatabase();

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var dto = new CreateProductDto(
            "API Test Phone",
            "Good phone",
            999.99m,
            0.5,
            ProductCategory.ELECTRONICS
        );

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoute, dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        responseBody.Should().NotBeNull();
        responseBody!["id"].Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidDto = new CreateProductDto(
            "Iphone",
            "Bad Phone",
            -50m,
            0.5,
            ProductCategory.ELECTRONICS
        );

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoute, invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProductById_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var fakeId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"{ApiRoute}/{fakeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}