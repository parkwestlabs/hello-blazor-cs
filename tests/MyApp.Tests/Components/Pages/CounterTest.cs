using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using MyApp.Web.Components.Pages;

namespace MyApp.Tests.Components.Pages;

[TestClass]
public class CounterTest
{
    [TestMethod]
    public void Counter_ShouldIncrement_WhenButtonClicked()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddFluentUIComponents();

        var cut = ctx.Render<Counter>();    // cut: Component Under Test
        Assert.IsNotNull(cut);

        var status = cut.Find("p[role='status']");
        Assert.AreEqual("Current count: 0", status.TextContent.Trim());

        cut.Find("fluent-button[type='button']").Click();

        Assert.AreEqual("Current count: 1", status.TextContent.Trim());
    }

    [TestMethod]
    public void Counter_ShouldIncrementToFive_WhenButtonClickedFiveTimes()
    {
        // 1. Arrange: Counterコンポーネントをレンダリング
        using var ctx = new BunitContext();
        ctx.Services.AddFluentUIComponents();

        var cut = ctx.Render<Counter>();
        Assert.IsNotNull(cut);

        // 2. Act: for文を使ってボタンを5回連続でクリック
        var buttonElement = cut.Find("fluent-button[type='button']");

        for (int i = 0; i < 5; i++)
        {
            buttonElement.Click();
        }

        // 3. Assert: カウントが「5」になっていることを検証
        var status = cut.Find("p[role='status']");
        Assert.AreEqual("Current count: 5", status.TextContent.Trim());
    }
}
