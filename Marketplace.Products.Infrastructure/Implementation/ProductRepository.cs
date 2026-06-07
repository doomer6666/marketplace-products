using System.Data;
using System.Text;
using Dapper;
using Marketplace.Products.Application;
using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Domain;
using Marketplace.Products.Infrastructure.Helpers;

namespace Marketplace.Products.Infrastructure.Implementation;

public class ProductRepository(IPostgresConnectionFactory postgresConnectionFactory) : IProductRepository
{
    public async Task Add(Product product)
    {
        await using var connection = postgresConnectionFactory.GetConnection();

        var sql =
            @"INSERT INTO products (id, name, description, price, weight, category)
            VALUES (@Id, @Name, @Description, @Price, @Weight, @Category)";

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

    public async Task<List<Product>> GetFilteredList(ProductFilterDto filterDto)
    {
        await using var connection = postgresConnectionFactory.GetConnection();

        var sql = new StringBuilder(
            @"SELECT id, name, description, price, weight, category, createdat FROM products WHERE 1 = 1"
        );
        var parameters = new DynamicParameters();
        if (filterDto.CreatedAt.HasValue)
        {
            sql.Append(" AND createdat::date = @CreatedAt");
            parameters.Add("CreatedAt", filterDto.CreatedAt.Value.Date, DbType.Date);
        }

        if (filterDto.Category.HasValue)
        {
            sql.Append(" AND category = @Category");
            parameters.Add("Category", (int)filterDto.Category.Value, DbType.Int32);
        }

        if (filterDto.MinPrice.HasValue)
        {
            sql.Append(" AND Price >= @MinPrice ");
            parameters.Add("MinPrice", filterDto.MinPrice.Value, DbType.Decimal);
        }

        if (filterDto.MaxPrice.HasValue)
        {
            sql.Append(" AND Price <= @MaxPrice ");
            parameters.Add("MaxPrice", filterDto.MaxPrice.Value, DbType.Decimal);
        }

        if (!string.IsNullOrWhiteSpace(filterDto.SearchTerm))
        {
            sql.Append(" AND Name ILIKE @SearchTerm ");
            parameters.Add("SearchTerm", $"%{filterDto.SearchTerm}%");
        }

        sql.Append(
            @" ORDER BY createdat DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY"
        );

        parameters.Add("Offset", (filterDto.PageNumber - 1) * filterDto.PageSize, DbType.Int32);
        parameters.Add("PageSize", filterDto.PageSize, DbType.Int32);

        var result = await connection.QueryAsync<Product>(sql.ToString(), parameters);

        return [.. result];
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
}