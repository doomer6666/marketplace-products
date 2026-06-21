using StackExchange.Redis;
using Testcontainers.Redis;

namespace Marketplace.Products.IntegrationTests.Fixtures;

public class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _container = new RedisBuilder("redis:7-alpine").Build();

    public ConnectionMultiplexer Connection { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Connection = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString() + ",allowAdmin=true");
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public async Task ClearDatabase()
    {
        var server = Connection.GetServer(Connection.GetEndPoints().First());
        await server.FlushDatabaseAsync();
    }
}