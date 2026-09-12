using System.Globalization;
using Bunit;
using BlazorHelloWorld.Components.Pages;
using BlazorHelloWorld.Models;
using BlazorHelloWorld.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BlazorHelloWorld.Tests.Components.Pages;

public class WeatherTest
{
    [Fact]
    public void WeatherComponent_ShouldRenderLoading_ThenShowMockedData()
    {
        var culture = new CultureInfo("ja-JP");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        // 1. Arrange: bUnit コンテキストの生成
        using var ctx = new BunitContext();

        // 2. NSubstitute でサービスの偽物（Mock）を生成
        var mockService = Substitute.For<IWeatherService>();

        // 偽物が呼ばれたら、3行の確定データを返すように仕込む（件数の検証を正確にするため）
        WeatherForecast[] mockResult = [
            new() { Date = new (2026, 9, 6), TemperatureC = 20, Summary = "Chilly" },
            new() { Date = new (2026, 9, 7), TemperatureC = 25, Summary = "Mild" },
            new() { Date = new (2026, 9, 8), TemperatureC = 30, Summary = "NSubstitute Showcase!" }
        ];
        mockService.GetActiveForecastsAsync().Returns(async _ =>
        {
            await Task.Delay(100);
            return mockResult;
        });

        // 3. bUnit の DI コンテナに Mock を登録
        ctx.Services.AddSingleton<IWeatherService>(mockService);

        // 4. Act: 画面をレンダリング（非同期なので最初は Loading 状態）
        var cut = ctx.Render<Weather>();

        // 初期状態（Loading...）の検証
        var pElements = cut.FindAll("p");
        Assert.Equal(2, pElements.Count);

        var loadingText = pElements[1].TextContent.Trim();
        Assert.Equal("Loading...", loadingText);
        Assert.Empty(cut.FindAll("table")); // まだテーブルは描画されていない

        // 5. Act 2: 非同期データが読み込まれて画面が更新されるのを待機
        cut.WaitForState(() => cut.FindAll("table").Count > 0, TimeSpan.FromSeconds(2));

        // 6. Assert: データ読み込み後「Loading...」が消えたことの検証
        Assert.Empty(cut.FindAll("p:contains('Loading...')"));

        // テーブルヘッダーの文字検証
        var thElement = cut.Find("th[aria-label='Temperature in Celsius']");
        Assert.Equal("Temp. (C)", thElement.TextContent);

        // 仕込んだデータが「正確に3行」表示されているか
        var rows = cut.FindAll("tbody tr");
        Assert.Equal(3, rows.Count);

        // 特定のカスタム文字列が画面に正しく出現しているか
        rows[2].MarkupMatches(@"
            <tr>
                <td>2026/09/08</td>
                <td>30</td>
                <td>85</td>
                <td>NSubstitute Showcase!</td>
            </tr>
        ");
    }
}
