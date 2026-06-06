using FluentValidation;
using Marketplace.Products.Application;
using Marketplace.Products.Application.Implementation;
using Marketplace.Products.Application.Validators;
using Marketplace.Products.Infrastructure.Helpers;
using Marketplace.Products.Infrastructure.Implementation;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                              ?? throw new InvalidOperationException("No database connection string found.");

var services = builder.Services;
services.AddControllers();
services.AddGrpc();

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddSingleton<IPostgresConnectionFactory>(new PostgresConnectionFactory(connectionString));
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IProductRepository, ProductRepository>();
services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
//services.AddValidatorsFromAssemblyContaining<UpdateProductDtoValidator>();
//services.AddValidatorsFromAssemblyContaining<ProductFilterDtoValidator>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();