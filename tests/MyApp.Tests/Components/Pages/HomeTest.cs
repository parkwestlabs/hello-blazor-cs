using Bunit;
using MyApp.Web.Components.Pages;

namespace MyApp.Tests.Components.Pages;

[TestClass]
public class HomeTest
{
    [TestMethod]
    public void Home_ShouldRenderCorrectHtml()
    {
        using var ctx = new BunitContext();
        var cut = ctx.Render<Home>();

        cut.Find("h1").MarkupMatches("<h1>Hello, world!</h1>");

        var expectedText = "Welcome to your new app.";
        var mainText = cut.Find("h1").NextSibling?.TextContent.Trim()!;
        Assert.Contains(expectedText, mainText, StringComparison.Ordinal);
    }

    [TestMethod]
    public void Home_ShouldUpdateTextInRealTime_WhenUserTypes()
    {
        // 1. Arrange: 画面をレンダリング
        using var ctx = new BunitContext();
        var cut = ctx.Render<Home>();

        // 初期状態ではアラートの中身が空（"リアルタイム表示：" のみ）であることを確認
        var alertElement = cut.Find(".alert");
        Assert.AreEqual("Your Input:", alertElement.TextContent.Trim());

        // 2. Act: 入力欄（input）を見つけて、ユーザーが「Hello Blazor!」とタイピングしたイベントを発生させる
        var inputElement = cut.Find("input");
        inputElement.Input("Hello Blazor!"); // 💡 これが bUnit でタイピングを擬似再現するコマンド

        // 3. Assert: リアルタイムにアラート内の文字列が書き換わっているかを検証
        Assert.AreEqual("Your Input: Hello Blazor!", alertElement.TextContent.Trim());
    }
}
