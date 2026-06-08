using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Marketplace.Products.Api.Protos;
using Marketplace.Products.IntegrationTests.Fixtures;

namespace Marketplace.Products.IntegrationTests.Api;

public class ProductGrpcEndpointTests : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    private readonly ProductServiceGrpc.ProductServiceGrpcClient _client;
    private readonly MarketplaceApiFactory _factory;
    private readonly PostgresFixture _fixture;

    public ProductGrpcEndpointTests(PostgresFixture fixture)
    {
        _fixture = fixture;

        _factory = new MarketplaceApiFactory(_fixture.ConnectionString);
        var httpClient = _factory.CreateClient();

        var channel =
            GrpcChannel.ForAddress(httpClient.BaseAddress!, new GrpcChannelOptions { HttpClient = httpClient });
        _client = new ProductServiceGrpc.ProductServiceGrpcClient(channel);
    }

    public async Task InitializeAsync() => await _fixture.ClearDatabase();

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Create_And_Get_ShouldWorkCorrectly()
    {
        // Arrange
        var createRequest = new CreateProductRequest
                            {
                                Name = "gRPC Phone",
                                Description = "Description",
                                Price = 99.99m,
                                Weight = 0.5,
                                Category = GrpcProductCategory.Electronics
                            };

        // Act
        var createResponse = await _client.CreateProductAsync(createRequest);

        createResponse.Should().NotBeNull();
        createResponse.Id.Should().NotBeNullOrWhiteSpace();

        var getRequest = new GetProductByIdRequest { Id = createResponse.Id };
        var getResponse = await _client.GetProductByIdAsync(getRequest);

        // Assert
        getResponse.Should().NotBeNull();
        getResponse.Product.Should().NotBeNull();
        getResponse.Product.Name.Should().Be("gRPC Phone");

        decimal actualPrice = getResponse.Product.Price;
        actualPrice.Should().Be(99.99m);
    }

    [Fact]
    public async Task GetProductById_NonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        var fakeId = Guid.NewGuid().ToString();
        var request = new GetProductByIdRequest { Id = fakeId };

        // Act
        var act = async () => await _client.GetProductByIdAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }
}