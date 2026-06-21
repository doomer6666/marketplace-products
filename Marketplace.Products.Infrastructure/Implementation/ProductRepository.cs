using Dapper;
using Marketplace.Products.Application;
using Marketplace.Products.Domain;
using Marketplace.Products.Infrastructure.Helpers;

namespace Marketplace.Products.Infrastructure.Implementation;

public class ProductRepository(IPostgresConnectionFactory postgresConnectionFactory) : IProductRepository
{
    public async Task Add(Product product)
    {
        await using var connection = postgresConnectionFactory.GetConnection();

        var sql =
            @"INSERT INTO products (id, name, description, price, weight, category, createdat, updatedat)
            VALUES (@Id, @Name, @Description, @Price, @Weight, @Category, @CreatedAt, @UpdatedAt)";

        await connection.ExecuteAsync(
            sql,
            product
        );
    }

    public async Task DeleteById(Guid id)
    {
        await using var connection = postgresConnectionFactory.GetConnection();

        var sql = "DELETE FROM products WHERE id = @id";

        await connection.ExecuteAsync(sql, new { id });
    }

    public async Task<Product?> GetById(Guid id)
    {
        await using var connection = postgresConnectionFactory.GetConnection();

        var sql = @"SELECT id, name, description, price, weight, category, createdat, updatedat
                    FROM products WHERE id = @id";

        var res = await connection.QueryFirstOrDefaultAsync<Product>(sql, new { id });
        return res;
    }

    public async Task<Product> UpdateById(Product product)
    {
        await using var connection = postgresConnectionFactory.GetConnection();

        var sql =
            @"UPDATE products
                  SET name = @Name,
                      price = @Price,
                      weight = @Weight,
                      category = @Category,
                      description = @Description
                  WHERE id = @Id
                  RETURNING *";

        var result = await connection.QueryFirstOrDefaultAsync<Product>(sql, product);
        if (result is null)
        {
            throw new KeyNotFoundException($"Product with id '{product.Id}' not found");
        }

        return result;
    }

    public async Task AddMany(IEnumerable<Product> products)
    {
        await using var connection = postgresConnectionFactory.GetConnection();

        var sql = @"INSERT INTO products (id, name, description, price, weight, category, createdat, updatedat)
            VALUES (@Id, @Name, @Description, @Price, @Weight, @Category, @CreatedAt, @UpdatedAt)";
        await connection.ExecuteAsync(sql, products);
    }
}