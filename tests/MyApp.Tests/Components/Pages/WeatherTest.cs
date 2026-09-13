using System.Globalization;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using MyApp.Web.Components.Pages;
using MyApp.Core.Models;
using MyApp.Core.Interfaces;

namespace MyApp.Tests.Components.Pages;

[TestClass]
public class WeatherTest : BunitContext
{
    [TestMethod]
    public async Task WeatherComponent_ShouldRenderLoading_ThenShowMockedData()
    {
        var culture = new CultureInfo("ja-JP");
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        var mockService = Substitute.For<IWeatherService>();

        WeatherForecast[] mockResult = [
            new() { Date = new (2026, 9, 6), TemperatureC = 20, Summary = "Chilly" },
            new() { Date = new (2026, 9, 7), TemperatureC = 25, Summary = "Mild" },
            new() { Date = new (2026, 9, 8), TemperatureC = 30, Summary = "NSubstitute Showcase!" }
        ];

        var tcs = new TaskCompletionSource<WeatherForecast[]>();

        mockService.GetActiveForecastsAsync(Arg.Any<CancellationToken>())
            .Returns(_ => tcs.Task);

        // bUnit の DI コンテナに Mock を登録
        Services.AddSingleton(mockService);

        // 画面をレンダリング（非同期なので最初は Loading 状態）
        var cut = Render<Weather>();

        // 初期状態（Loading...）の検証
        var loadingElm = cut.Find("p > em");
        Assert.AreEqual("Loading...", loadingElm.TextContent);
        Assert.IsEmpty(cut.FindAll("table")); // まだテーブルは描画されていない

        // データを流し込んで非同期処理を完了させる
        tcs.SetResult(mockResult);

        // MTP/bUnit環境でUIがデータ表示に更新されるのを待つ
        await cut.WaitForStateAsync(() => cut.Find("table") != null);

        // データ読み込み後「Loading...」が消えたことの検証
        Assert.IsEmpty(cut.FindAll("p:contains('Loading...')"));

        // テーブルヘッダーの文字検証
        var thElement = cut.Find("th[aria-label='Temperature in Celsius']");
        Assert.AreEqual("Temp. (C)", thElement.TextContent);

        // 仕込んだデータが「正確に3行」表示されているか
        var rows = cut.FindAll("tbody tr");
        Assert.HasCount(3, rows);

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
