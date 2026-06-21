using FluentAssertions;
using FluentValidation;
using Marketplace.Products.Application;
using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Application.Events;
using Marketplace.Products.Application.Implementation;
using Marketplace.Products.Domain;
using Moq;

namespace Marketplace.Products.UnitTests.Application;

public class ProductServiceTests
{
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<IValidator<CreateProductDto>> _createValidatorMock = new();

    private readonly Mock<IValidator<ProductFilterDto>> _filterValidatorMock = new();
    private readonly Mock<IMessageProducer> _producerMock = new();
    private readonly Mock<IProductRepository> _repositoryMock = new();
    private readonly Mock<IProductSearchReader> _searchRepositoryMock = new();

    private readonly ProductService _service;
    private readonly Mock<IValidator<UpdateProductDto>> _updateValidatorMock = new();

    public ProductServiceTests()
    {
        _service = new ProductService(
            _repositoryMock.Object,
            _searchRepositoryMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _filterValidatorMock.Object,
            _cacheMock.Object,
            _producerMock.Object);
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

    [Fact]
    public async Task GetFilteredProductList_ShouldCallSearchRepository()
    {
        // Arrange
        var filter = new ProductFilterDto { SearchTerm = "Test" };
        var expectedList = new List<Product> { Product.Create("Test", "D", 10, 1, ProductCategory.ELECTRONICS) };

        _searchRepositoryMock
            .Setup(repo => repo.SearchAsync(filter))
            .ReturnsAsync(expectedList);

        // Act
        var result = await _service.GetFilteredProductList(filter);

        // Assert
        result.Should().BeEquivalentTo(expectedList);
        _searchRepositoryMock.Verify(repo => repo.SearchAsync(filter), Times.Once);
        _filterValidatorMock.Verify(v =>
                v.ValidateAsync(It.IsAny<ValidationContext<ProductFilterDto>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteProduct_ShouldRemoveFromCacheAndRepository()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        await _service.DeleteProductById(id);

        // Assert
        _cacheMock.Verify(cache => cache.RemoveAsync($"product-{id}"), Times.Once);
        _repositoryMock.Verify(repo => repo.DeleteById(id), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldUpdateAndSendToKafka()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingProduct = Product.Import(id,
            "Old",
            "Desc",
            10m,
            1,
            ProductCategory.ELECTRONICS,
            DateTime.UtcNow,
            DateTime.UtcNow);
        var updateDto = new UpdateProductDto { Name = "New Name" };

        _repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(existingProduct);
        _repositoryMock.Setup(r => r.UpdateById(It.IsAny<Product>())).ReturnsAsync(existingProduct);

        // Act
        var result = await _service.UpdateProductById(id, updateDto);

        // Assert
        result.Name.Should().Be("New Name");
        _cacheMock.Verify(c => c.RemoveAsync($"product-{id}"), Times.Once);
        _repositoryMock.Verify(r => r.UpdateById(It.IsAny<Product>()), Times.Once);

        _producerMock.Verify(p => p.PublishMessageAsync(
                It.IsAny<string>(),
                id.ToString(),
                It.Is<ProductSyncEvent>(e => e.Action == EventAction.Update)),
            Times.Once);
    }
}