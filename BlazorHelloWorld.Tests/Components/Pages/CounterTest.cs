using Bunit;
using BlazorHelloWorld.Components.Pages;

namespace BlazorHelloWorld.Tests.Components.Pages;

public class CounterTest
{
    [Fact]
    public void Counter_ShouldIncrement_WhenButtonClicked()
    {
        using var ctx = new BunitContext();
        var cut = ctx.Render<Counter>();    // cut: Component Under Test

        cut.Find("p[role='status']").MarkupMatches("<p role=\"status\">Current count: 0</p>");

        cut.Find("button").Click();

        cut.Find("p[role='status']").MarkupMatches("<p role=\"status\">Current count: 1</p>");
    }

    [Fact]
    public void Counter_ShouldIncrementToFive_WhenButtonClickedFiveTimes()
    {
        // 1. Arrange: Counterコンポーネントをレンダリング
        using var ctx = new BunitContext();
        var cut = ctx.Render<Counter>();

        // 2. Act: for文を使ってボタンを5回連続でクリック
        var buttonElement = cut.Find("button");
        for (int i = 0; i < 5; i++)
        {
            buttonElement.Click();
        }

        // 3. Assert: カウントが「5」になっていることを検証
        cut.Find("p[role='status']").MarkupMatches("<p role=\"status\">Current count: 5</p>");
    }
}
