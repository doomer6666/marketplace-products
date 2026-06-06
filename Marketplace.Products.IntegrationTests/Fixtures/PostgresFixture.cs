using Dapper;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Marketplace.Products.IntegrationTests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public string ConnectionString => _container.GetConnectionString();

    public PostgresFixture()
    {
        _container = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("testdb")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await CreateSchema();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    private async Task CreateSchema()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        var sql = @"
            CREATE TABLE IF NOT EXISTS products (
                id UUID PRIMARY KEY,
                name TEXT NOT NULL,
                description TEXT NOT NULL,
                price NUMERIC NOT NULL,
                weight DOUBLE PRECISION NOT NULL,
                category INTEGER NOT NULL,
                createdat TIMESTAMP NOT NULL DEFAULT timezone('utc', now()),
                updatedat TIMESTAMP NOT NULL DEFAULT timezone('utc', now())
            )";

        await connection.ExecuteAsync(sql);
    }

    public async Task ClearDatabase()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("TRUNCATE TABLE products");
    }
}