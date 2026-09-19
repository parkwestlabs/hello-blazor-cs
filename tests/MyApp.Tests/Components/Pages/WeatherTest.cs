using System.Globalization;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using NSubstitute;
using MyApp.Web.Components.Pages;
using MyApp.Core.Models;
using MyApp.Core.Interfaces;

namespace MyApp.Tests.Components.Pages;

[TestClass]
public class WeatherTest
{
    [TestMethod]
    public async Task WeatherComponent_ShouldRenderLoading_ThenShowMockedData()
    {
        using var ctx = new BunitContext();

        var culture = new CultureInfo("ja-JP");
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        var mockService = Substitute.For<IWeatherService>();

        List<WeatherForecast> mockResult = [
            new() { Date = new (2026, 9, 6), TemperatureC = 20, Summary = "Chilly" },
            new() { Date = new (2026, 9, 7), TemperatureC = 25, Summary = "Mild" },
            new() { Date = new (2026, 9, 8), TemperatureC = 30, Summary = "NSubstitute Showcase!" }
        ];

        var tcs = new TaskCompletionSource<List<WeatherForecast>>();

        mockService.GetActiveForecastsAsync(Arg.Any<CancellationToken>())
            .Returns(tcs.Task);

        // FluentUI が内部で必要とする共通サービスを一括登録
        ctx.Services.AddFluentUIComponents();

        // bUnit の DI コンテナに Mock を登録
        ctx.Services.AddSingleton(mockService);

        // FluentUIからのJS呼び出しはすべて問題ないものとしてスルーする
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // 画面をレンダリング（非同期なので最初は Loading 状態）
        var cut = ctx.Render<Weather>();

        // テストの出力（Console）に、現在の画面のHTMLをそのまま吐き出させます
        Console.WriteLine(cut.Markup);

        // グリッド内に FluentProgressRing (ぐるぐる) が存在していることを確認
        var progressRing = cut.Find("fluent-progress-ring");
        Assert.IsNotNull(progressRing);

        // データを流し込んで非同期処理を完了させる
        tcs.SetResult(mockResult);

        // loading 属性の監視をやめて、「ぐるぐる」が画面から消滅するのを待つ
        await cut.WaitForStateAsync(() => cut.FindAll("fluent-progress-ring").Count == 0);

        // データ読み込み後「Loading...」が消えたことの検証
        Assert.IsEmpty(cut.FindAll("fluent-progress-ring"));

        // 仕込んだデータが「正確に3行」表示されているか
        var rows = cut.FindAll("tbody tr[role='row']");
        Assert.HasCount(3, rows);

        // 特定のカスタム文字列が画面に正しく出現しているか
        // 3行目（インデックス2）の行内のセル（gridcell）を取得
        var cells = rows[2].QuerySelectorAll("td[role='gridcell']");

        Assert.AreEqual("2026/09/08", cells[0].TextContent);
        Assert.AreEqual("30", cells[1].TextContent);
        Assert.AreEqual("85", cells[2].TextContent);
        Assert.AreEqual("NSubstitute Showcase!", cells[3].TextContent);
    }
}
