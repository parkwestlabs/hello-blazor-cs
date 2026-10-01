using MyApp.Core.Interfaces;
using MyApp.Core.Models;
using MyApp.Core.Services;
using NSubstitute;

namespace MyApp.Tests.Services;

[TestClass]
public class ProductServiceTests
{
    private IProductRepository _repository = null!;
    private ProductService _service = null!;

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = Substitute.For<IProductRepository>();
        _service = new ProductService(_repository);
    }

    #region GetAllProductsAsync Tests

    [TestMethod]
    public async Task GetAllProductsAsync_ReturnsProductList_WhenProductsExist()
    {
        // Arrange
        var expectedProducts = new List<Product>
        {
            new() { Id = 1, Name = "Product A" },
            new() { Id = 2, Name = "Product B" }
        };

        _repository.GetAllAsync(TestContext.CancellationToken)
            .Returns(Task.FromResult(expectedProducts));

        // Act
        var result = await _service.GetAllProductsAsync(TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.AreSequenceEqual(expectedProducts, result);
        await _repository.Received(1).GetAllAsync(TestContext.CancellationToken);
    }

    #endregion

    #region GetProductByIdAsync Tests

    [TestMethod]
    public async Task GetProductByIdAsync_ReturnsProduct_WhenProductExists()
    {
        // Arrange
        var targetId = 1;
        var expectedProduct = new Product { Id = targetId, Name = "Product A" };

        _repository.GetByIdAsync(targetId, TestContext.CancellationToken).Returns(Task.FromResult<Product?>(expectedProduct));

        // Act
        var result = await _service.GetProductByIdAsync(targetId, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(targetId, result.Id);
        await _repository.Received(1).GetByIdAsync(targetId, TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task GetProductByIdAsync_ReturnsNull_WhenProductDoesNotExist()
    {
        // Arrange
        var targetId = 999;

        _repository.GetByIdAsync(targetId, TestContext.CancellationToken).Returns(Task.FromResult<Product?>(null));

        // Act
        var result = await _service.GetProductByIdAsync(targetId, TestContext.CancellationToken);

        // Assert
        Assert.IsNull(result);
        await _repository.Received(1).GetByIdAsync(targetId, TestContext.CancellationToken);
    }

    #endregion

    #region SaveProductAsync Tests

    [TestMethod]
    public async Task SaveProductAsync_ThrowsArgumentNullException_WhenProductIsNull()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await _service.SaveProductAsync(null!, TestContext.CancellationToken)
        );

        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task SaveProductAsync_CallsAddAsync_WhenProductIdIsZero()
    {
        // Arrange
        var newProduct = new Product { Id = 0, Name = "New Product" };

        // Act
        await _service.SaveProductAsync(newProduct, TestContext.CancellationToken);

        // Assert
        await _repository.Received(1).AddAsync(newProduct, TestContext.CancellationToken);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task SaveProductAsync_CallsUpdateAsync_WhenProductIdIsNotZero()
    {
        // Arrange
        var existingProduct = new Product { Id = 10, Name = "Existing Product" };

        // Act
        await _service.SaveProductAsync(existingProduct, TestContext.CancellationToken);

        // Assert
        await _repository.Received(1).UpdateAsync(existingProduct, TestContext.CancellationToken);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteProductAsync Tests

    [TestMethod]
    public async Task DeleteProductAsync_CallsDeleteAsync_WithCorrectIdAndToken()
    {
        // Arrange
        var targetId = 5;

        // Act
        await _service.DeleteProductAsync(targetId, TestContext.CancellationToken);

        // Assert
        await _repository.Received(1).DeleteAsync(targetId, TestContext.CancellationToken);
    }

    #endregion
}
