using Bunit;
using BlazorHelloWorld.Components.Pages;

namespace BlazorHelloWorld.Tests.Components.Pages;

public class WeatherTest
{
    [Fact]
    public void WeatherComponent_ShouldRenderLoadingThenShowData()
    {
        using var ctx = new BunitContext();
        var cut = ctx.Render<Weather>();

        Assert.NotNull(cut.Find("p:contains('Loading...')"));
        Assert.Empty(cut.FindAll("table"));

        cut.WaitForState(() => cut.FindAll("table").Count > 0, TimeSpan.FromSeconds(2));

        Assert.Empty(cut.FindAll("p:contains('Loading...')"));
        Assert.Equal(5, cut.FindAll("tbody tr").Count);

        var thElement = cut.Find("th[aria-label='Temperature in Celsius']");
        Assert.Equal("Temp. (C)", thElement.TextContent);
    }
}
