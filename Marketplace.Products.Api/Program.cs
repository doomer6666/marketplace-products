using System.Text.Json.Serialization;
using FluentValidation;
using Marketplace.Products.Api.GrpcServices;
using Marketplace.Products.Api.Interceptors;
using Marketplace.Products.Api.Middleware;
using Marketplace.Products.Application;
using Marketplace.Products.Application.Implementation;
using Marketplace.Products.Application.Validators;
using Marketplace.Products.Infrastructure.Helpers;
using Marketplace.Products.Infrastructure.Implementation;
using Marketplace.Products.Migrations.Migrations;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var postrgesConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("No database connection string found.");

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
                            ?? throw new InvalidOperationException("No redis connection string found.");

var elasticSearchConnectionString = builder.Configuration.GetConnectionString("ElasticSearch")
                                    ?? throw new InvalidOperationException("No ES connection string found.");

var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString!);

var kafkaServers = builder.Configuration.GetConnectionString("Kafka")
                   ?? throw new InvalidOperationException("No kafka connection string found.");

var services = builder.Services;
services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddControllers()
        .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
services.AddGrpc(options =>
{
    options.Interceptors.Add<GrpcExceptionInterceptor>();
    options.Interceptors.Add<LoggingInterceptor>();
});

services.AddSingleton<IPostgresConnectionFactory>(new PostgresConnectionFactory(postrgesConnectionString));
services.AddSingleton<IConnectionMultiplexer>(multiplexer);
services.AddScoped<ICacheService, RedisCacheService>();

services.AddSingleton(new ElasticProductRepository(elasticSearchConnectionString));
services.AddSingleton<IProductSearchReader>(sp => sp.GetRequiredService<ElasticProductRepository>());
services.AddSingleton<IProductSearchWriter>(sp => sp.GetRequiredService<ElasticProductRepository>());

services.AddSingleton<IMessageProducer>(_ => new KafkaProducer(kafkaServers));
services.AddHostedService<KafkaProductSyncWorker>(sp =>
{
    var searchRepository = sp.GetRequiredService<IProductSearchWriter>();
    return new KafkaProductSyncWorker(kafkaServers, searchRepository);
});
services.AddScoped<IDevToolsService, DevToolsService>();
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IProductRepository, ProductRepository>();

services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();

var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("AllowAll");

app.RunMigrations();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGrpcService<ProductGrpcEndpoint>();

app.Run();

public partial class Program
{
}