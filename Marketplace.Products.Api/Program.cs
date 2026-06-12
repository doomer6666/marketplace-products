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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var postrgesConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("No database connection string found.");

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
                            ?? throw new InvalidOperationException("No redis connection string found.");

var elasticSearchConnectionString = builder.Configuration.GetConnectionString("ElasticSearch")
                                    ?? throw new InvalidOperationException("No ES connection string found.");

var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString!);

var kafkaServers = builder.Configuration.GetSection("Kafka").ToString()
                   ?? throw new InvalidOperationException("No kafka connection string found.");

var services = builder.Services;
services.AddControllers()
        .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
services.AddGrpc(options =>
{
    options.Interceptors.Add<GrpcExceptionInterceptor>();
    options.Interceptors.Add<LoggingInterceptor>();
});
services.AddEndpointsApiExplorer();

services.AddSingleton<IPostgresConnectionFactory>(new PostgresConnectionFactory(postrgesConnectionString));
services.AddSingleton<IConnectionMultiplexer>(multiplexer);
services.AddScoped<ICacheService, RedisCacheService>();
services.AddSingleton<IMessageProducer>(new KafkaProducer(kafkaServers));
services.AddSingleton<IProductSearchRepository>(new ElasticProductRepository(elasticSearchConnectionString));
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IProductRepository, ProductRepository>();

services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();

var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.RunMigrations();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGrpcService<ProductGrpcEndpoint>();

app.Run();

public partial class Program
{
}