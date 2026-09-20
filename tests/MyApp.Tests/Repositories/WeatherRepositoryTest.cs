using System.Net;
using NSubstitute;
using RichardSzalay.MockHttp;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using MsTestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
using MyApp.Data.Repositories;
using MyApp.Core.Models;
using MyApp.Tests.Helpers;

namespace MyApp.Tests.Repositories;

[TestClass]
public class WeatherRepositoryTests
{
    public required MsTestContext TestContext { get; set; }
    private CancellationToken Token => TestContext.CancellationToken;

    [TestMethod]
    public async Task GetForecastAsync_Returns_WeatherData()
    {
        var url = "http://localhost/v1/forecast?latitude=35.6785&longitude=139.6823&daily=temperature_2m_max";

        var dummyResponse = new OpenMeteoResponse(
            new DailyData(
                [
                    new DateOnly(2026, 9, 1),
                    new DateOnly(2026, 9, 2),
                    new DateOnly(2026, 9, 3),
                    new DateOnly(2026, 9, 4),
                    new DateOnly(2026, 9, 5)
                ],
                [29.5, 30.1, 29.0, 29.0, 29.0]
            )
        );

        HttpRequestMessage? actualRequest = null;

        using var mockHttpHandler = new MockHttpMessageHandler();

        var requestWatcher = mockHttpHandler.When(HttpMethod.Get, url)
            .RespondJson(dummyResponse, req => actualRequest = req);

        var httpClient = mockHttpHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost/");

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = NullLogger<WeatherRepository>.Instance;

        var repository = new WeatherRepository(httpClient, cache, logger);

        var duration = TimeSpan.FromMinutes(5);
        var result = await repository.FetchForecastAsync(35.6785, 139.6823, duration, Token);
        await repository.FetchForecastAsync(35.6785, 139.6823, duration, Token);

        Assert.AreEqual(1, mockHttpHandler.GetMatchCount(requestWatcher));

        Assert.IsNotNull(result);

        Assert.AreEqual(HttpMethod.Get, actualRequest?.Method);

        Assert.IsNotNull(actualRequest?.RequestUri);
        Assert.Contains("v1/forecast", actualRequest.RequestUri.ToString());

        Assert.HasCount(5, result.Daily.Time);
        Assert.AreEqual(new DateOnly(2026, 9, 1), result.Daily.Time[0]);
        Assert.AreEqual(29.5, result.Daily.Temperature2mMax[0]);
    }

    [TestMethod]
    public async Task GetForecastAsync_ThrowsHttpRequestException_WhenApiReturns500InternalServerError()
    {
        // 1. 500 Internal Server Error を返すレスポンスを準備
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        // 2. 作成した FakeHttpMessageHandler にエラーレスポンスを渡して HttpClient を生成
        using var fakeHandler = new FakeHttpMessageHandler(httpResponse);
        using var httpClient = new HttpClient(fakeHandler)
        {
            BaseAddress = new Uri("https://open-meteo.com")
        };

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = NullLogger<WeatherRepository>.Instance;

        var repository = new WeatherRepository(httpClient, cache, logger);

        // 3. 実行 & 検証
        // GetFromJsonAsync はステータスコードがエラー（4xx, 5xx）の場合、
        // 内部で EnsureSuccessStatusCode() を呼び出して HttpRequestException を発生させます。
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await repository.FetchForecastAsync(35.6785, 139.6823, TimeSpan.Zero, Token);
        });

        // 4. 追加の検証（必要に応じてステータスコードが500であることを確認）
        Assert.AreEqual(HttpStatusCode.InternalServerError, exception.StatusCode);

        // リクエストが確かに送信されたかもチェック
        Assert.IsNotNull(fakeHandler.LastRequest);
        Assert.AreEqual(HttpMethod.Get, fakeHandler.LastRequest.Method);
    }
}
