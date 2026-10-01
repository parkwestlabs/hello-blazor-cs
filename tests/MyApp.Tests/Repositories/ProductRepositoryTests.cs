using Microsoft.EntityFrameworkCore;
using MyApp.Core.Models;
using MyApp.Data.Config;
using MyApp.Data.Repositories;

namespace MyApp.Tests.Repositories;

[TestClass]
public class ProductRepositoryTests
{
    private IDbContextFactory<AppDbContext> _contextFactory = null!;

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        // テストケースごとに独立したインメモリDBを使用するため、毎回新しいGUIDで名前空間を区切る
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _contextFactory = new TestDbContextFactory(options);
    }

    [TestMethod]
    public async Task AddAsync_正常に追加できること()
    {
        // Arrange
        var repository = new ProductRepository(_contextFactory);
        var product = new Product { Id = 1, Name = "ノートパソコン" };

        // Act
        await repository.AddAsync(product, TestContext.CancellationToken);

        // Assert
        var result = await repository.GetByIdAsync(1, TestContext.CancellationToken);
        Assert.IsNotNull(result);
        Assert.AreEqual("ノートパソコン", result.Name);
    }

    [TestMethod]
    public async Task GetAllAsync_ID昇順で一覧が取得できること()
    {
        // Arrange
        var repository = new ProductRepository(_contextFactory);
        await repository.AddAsync(new Product { Id = 2, Name = "商品B" }, TestContext.CancellationToken);
        await repository.AddAsync(new Product { Id = 1, Name = "商品A" }, TestContext.CancellationToken);

        // Act
        var products = await repository.GetAllAsync(TestContext.CancellationToken);

        // Assert
        Assert.HasCount(2, products);
        Assert.AreEqual(1, products[0].Id);
        Assert.AreEqual(2, products[1].Id);
    }

    [TestMethod]
    public async Task UpdateAsync_正常に更新できること()
    {
        // Arrange
        var repository = new ProductRepository(_contextFactory);
        await repository.AddAsync(new Product { Id = 1, Name = "変更前" }, TestContext.CancellationToken);

        // Act
        var targetProduct = await repository.GetByIdAsync(1, TestContext.CancellationToken);
        Assert.IsNotNull(targetProduct);

        targetProduct.Name = "変更後";
        await repository.UpdateAsync(targetProduct, TestContext.CancellationToken);

        // Assert
        var updatedProduct = await repository.GetByIdAsync(1, TestContext.CancellationToken);
        Assert.IsNotNull(updatedProduct);
        Assert.AreEqual("変更後", updatedProduct.Name);
    }

    [TestMethod]
    public async Task DeleteAsync_正常に削除できること()
    {
        // Arrange
        var repository = new ProductRepository(_contextFactory);
        await repository.AddAsync(new Product { Id = 1, Name = "削除用" }, TestContext.CancellationToken);

        // Act
        await repository.DeleteAsync(1, TestContext.CancellationToken);

        // Assert
        var result = await repository.GetByIdAsync(1, TestContext.CancellationToken);
        Assert.IsNull(result);
    }

    // テスト用の簡易 IDbContextFactory 実装
    private sealed class TestDbContextFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDbContext());
    }
}
