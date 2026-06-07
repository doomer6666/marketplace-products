using Marketplace.Products.Infrastructure.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Products.IntegrationTests.Api;

public class MarketplaceApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPostgresConnectionFactory));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<IPostgresConnectionFactory>(new PostgresConnectionFactory(connectionString));
        });
    }
}