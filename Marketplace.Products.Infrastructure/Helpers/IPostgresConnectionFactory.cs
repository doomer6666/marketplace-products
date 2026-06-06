using Npgsql;

namespace Marketplace.Products.Infrastructure.Helpers;

public interface IPostgresConnectionFactory
{
    public NpgsqlConnection GetConnection();
}