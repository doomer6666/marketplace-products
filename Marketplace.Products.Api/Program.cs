using System.Text.Json.Serialization;
using FluentValidation;
using Marketplace.Products.Application;
using Marketplace.Products.Application.Implementation;
using Marketplace.Products.Application.Validators;
using Marketplace.Products.Infrastructure.Helpers;
using Marketplace.Products.Infrastructure.Implementation;
using Marketplace.Products.Migrations.Migrations;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("No database connection string found.");

var services = builder.Services;
services.AddControllers()
        .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
services.AddGrpc();
services.AddEndpointsApiExplorer();

services.AddSingleton<IPostgresConnectionFactory>(new PostgresConnectionFactory(connectionString));
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IProductRepository, ProductRepository>();

services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
services.AddFluentValidationAutoValidation();

var app = builder.Build();
app.RunMigrations();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();

public partial class Program
{
}