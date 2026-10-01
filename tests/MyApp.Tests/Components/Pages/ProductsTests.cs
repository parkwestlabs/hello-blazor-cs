using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using NSubstitute;
using MyApp.Core.Interfaces;
using MyApp.Core.Models;
using MyApp.Web.Components.Pages;

namespace MyApp.Tests.Components.Pages;

[TestClass]
public class ProductsPageTests
{
    private static (BunitContext Ctx, IProductService ProductService, IDialogService DialogService) CreateTestContext()
    {
        var ctx = new BunitContext();

        var productServiceMock = Substitute.For<IProductService>();
        var dialogServiceMock = Substitute.For<IDialogService>();

        // DIコンテナのセットアップ
        ctx.Services.AddSingleton(productServiceMock);
        ctx.Services.AddSingleton(dialogServiceMock);
        ctx.Services.AddFluentUIComponents();

        // JSInteropのモック（Looseモード）
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        return (ctx, productServiceMock, dialogServiceMock);
    }

    [TestMethod]
    public void OnInitializedAsync_ShouldLoadAndDisplayProducts()
    {
        // Arrange
        var testCtx = CreateTestContext();
        using var ctx = testCtx.Ctx;
        var productService = testCtx.ProductService;

        var products = new List<Product>
        {
            new() { Id = 1, Name = "商品A", Price = 1000 },
            new() { Id = 2, Name = "商品B", Price = 2000 }
        };

        productService
            .GetAllProductsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(products));

        // Act
        var cut = ctx.Render<Products>();

        // Assert
        Assert.Contains("商品A", cut.Markup);
        Assert.Contains("商品B", cut.Markup);

        _ = productService.Received(1).GetAllProductsAsync(Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task SaveProduct_WhenNewProduct_ShouldCallSaveAndResetForm()
    {
        // Arrange
        var testCtx = CreateTestContext();
        using var ctx = testCtx.Ctx;
        var productService = testCtx.ProductService;

        productService
            .GetAllProductsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Product>()));

        // Tips:
        // シンプルな値は Arg.Is（またはそのまま値を指定）
        // 複雑なオブジェクトは Arg.Do で外出しして Assert
        Product? actualProduct = null;

        // Arg.Do：SaveProductAsyncが呼ばれたら、引数をキャプチャして、ローカル変数に退避させる
        await productService.SaveProductAsync(
            Arg.Do<Product>(p => actualProduct = p),
            Arg.Any<CancellationToken>()
        );

        var cut = ctx.Render<Products>();

        // Act
        // フォーム入力
        var nameInput = cut.Find("#product-name");
        var priceInput = cut.Find("#product-price");

        await nameInput.ChangeAsync("新商品");
        await priceInput.ChangeAsync(1500);

        var form = cut.Find("form");
        await form.SubmitAsync();

        // Assert
        // 合計で1回呼び出されたことの検証
        await productService.Received(1).SaveProductAsync(
            Arg.Any<Product>(),
            Arg.Any<CancellationToken>()
        );

        // 引数のオブジェクトの検証
        Assert.IsNotNull(actualProduct, "SaveProductAsync が呼び出されていません。");
        Assert.AreEqual("新商品", actualProduct.Name);
        Assert.AreEqual(1500, actualProduct.Price);

        _ = productService.Received(2).GetAllProductsAsync(Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task EditProduct_ShouldPopulateForm_AndCancelShouldReset()
    {
        // Arrange
        var testCtx = CreateTestContext();
        using var ctx = testCtx.Ctx;
        var productService = testCtx.ProductService;

        var existingProduct = new Product { Id = 1, Name = "既存商品", Price = 3000 };

        productService
            .GetAllProductsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Product> { existingProduct }));

        var cut = ctx.Render<Products>();

        // Act 1: 編集ボタンクリック
        var button = cut.Find("fluent-button[title='編集']");
        await button.ClickAsync();

        // Assert 1
        Assert.Contains("<h5>編集</h5>", cut.Markup);

        // Act 2: キャンセルボタンクリック
        var cancelButton = cut.FindAll("fluent-button")
                        .FirstOrDefault(b => b.InnerHtml.Trim() == "キャンセル");
        Assert.IsNotNull(cancelButton);
        await cancelButton.ClickAsync();

        // Assert 2
        Assert.Contains("<h5>新規追加</h5>", cut.Markup);
    }
}
