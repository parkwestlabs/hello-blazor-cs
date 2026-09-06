using Bunit;
using BlazorHelloWorld.Components.Pages;

namespace BlazorHelloWorld.Tests.Components.Pages;

public class HomeTest
{
    [Fact]
    public void Home_ShouldRenderCorrectHtml()
    {
        using var ctx = new BunitContext();
        var cut = ctx.Render<Home>();

        cut.Find("h1").MarkupMatches("<h1>Hello, world!</h1>");

        var expectedText = "Welcome to your new app.";
        var mainText = cut.Find("h1").NextSibling?.TextContent.Trim();
        Assert.Contains(expectedText, mainText, StringComparison.Ordinal);
    }
}
