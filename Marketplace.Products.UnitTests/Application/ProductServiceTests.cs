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
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<IMessageProducer> _producerMock;
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IProductSearchRepository> _searchRepositoryMock;
    private readonly ProductService _service;


    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _service = new ProductService(_repositoryMock.Object,
            _searchRepositoryMock.Object,
            new CreateProductDtoValidator(),
            new UpdateProductDtoValidator(),
            new ProductFilterDtoValidator(),
            _cacheMock.Object,
            _producerMock.Object);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldSaveAndReturnId()
    {
        // Arrange
        var dto = new CreateProductDto
                  {
                      Name = "Valid Name",
                      Description = "Valid Desc",
                      Price = 100,
                      Weight = 1.5,
                      Category = ProductCategory.ELECTRONICS
                  };

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
        var existingProduct = new Product
                              {
                                  Id = productId,
                                  Name = "Old Name",
                                  Price = 100,
                                  Weight = 1.0,
                                  Category = ProductCategory.ELECTRONICS
                              };

        _repositoryMock
            .Setup(repo => repo.GetById(productId))
            .ReturnsAsync(existingProduct);

        _repositoryMock
            .Setup(repo => repo.UpdateById(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        var updateDto = new UpdateProductDto { Price = 999 };

        // Act
        var result = await _service.UpdateProductById(productId, updateDto);

        // Assert
        result.Price.Should().Be(999);
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