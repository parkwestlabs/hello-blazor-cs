using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using MyApp.Web.Components.Pages;

namespace MyApp.Tests.Components.Pages;

[TestClass]
public class HomeTest
{
    [TestMethod]
    public void Home_ShouldRenderCorrectHtml()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddFluentUIComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<Home>();

        cut.Find("h1").MarkupMatches("<h1>Hello, world!</h1>");

        var expectedText = "Welcome to your new app.";
        var mainText = cut.Find("h1").NextSibling?.TextContent.Trim()!;
        Assert.Contains(expectedText, mainText, StringComparison.Ordinal);
    }

    [TestMethod]
    public void Home_ShouldUpdateTextInRealTime_WhenUserTypes()
    {
        // 画面をレンダリング
        using var ctx = new BunitContext();
        ctx.Services.AddFluentUIComponents();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<Home>();

        // 初期状態ではmessageの中身が空であることを確認
        var message = cut.Find(".fluent-messagebar-message");
        Assert.AreEqual("Your Input:", message.TextContent);

        // ユーザーが「Hello Blazor!」とタイピングしたイベントを発生させる
        var inputElement = cut.Find("fluent-text-field");
        inputElement.Input("Hello Blazor!"); // 💡 これが bUnit でタイピングを擬似再現するコマンド

        // リアルタイムにアラート内の文字列が書き換わっているかを検証
        Assert.AreEqual("Your Input:Hello Blazor!", message.TextContent);
    }
}
