using FluentAssertions;
using Marketplace.Products.Application;
using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Application.Implementation;
using Marketplace.Products.Application.Validators;
using Marketplace.Products.Domain;
using Moq;

namespace Marketplace.Products.UnitTests.Application;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        var searchRepositoryMock = new Mock<IProductSearchReader>();
        var cacheMock = new Mock<ICacheService>();
        var producerMock = new Mock<IMessageProducer>();

        _service = new ProductService(
            _repositoryMock.Object,
            searchRepositoryMock.Object,
            new CreateProductDtoValidator(),
            new UpdateProductDtoValidator(),
            new ProductFilterDtoValidator(),
            cacheMock.Object,
            producerMock.Object);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldSaveAndReturnId()
    {
        // Arrange
        var dto = new CreateProductDto(
            "Valid Name",
            "Valid Desc",
            100m,
            1.5,
            ProductCategory.ELECTRONICS
        );

        // Act
        var resultId = await _service.CreateProduct(dto);

        // Assert
        resultId.Should().NotBeEmpty();
        _repositoryMock.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_WithPartialData_ShouldUpdateOnlyProvidedFields()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var existingProduct = Product.Import(
            productId,
            "Old Name",
            "Desc",
            100m,
            1.0,
            ProductCategory.ELECTRONICS,
            DateTime.UtcNow,
            DateTime.UtcNow);

        _repositoryMock
            .Setup(repo => repo.GetById(productId))
            .ReturnsAsync(existingProduct);

        _repositoryMock
            .Setup(repo => repo.UpdateById(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        var updateDto = new UpdateProductDto { Price = 999m };

        // Act
        var result = await _service.UpdateProductById(productId, updateDto);

        // Assert
        result.Price.Should().Be(999m);
        result.Name.Should().Be("Old Name");
        _repositoryMock.Verify(repo => repo.UpdateById(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_NonExistent_ShouldThrowKeyNotFound()
    {
        // Arrange
        var updateDto = new UpdateProductDto { Name = "Test" };

        _repositoryMock
            .Setup(repo => repo.GetById(It.IsAny<Guid>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = async () => await _service.UpdateProductById(Guid.NewGuid(), updateDto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}