using System.Net;
using System.Text.Json;
using System.Net.Http.Json;
using MyApp.Data.Repositories;
using MyApp.Core.Models;
using MyApp.Tests.Helpers;

namespace MyApp.Tests.Repositories;

[TestClass]
public class WeatherRepositoryTests
{
    [TestMethod]
    public async Task GetForecastAsync_Returns_WeatherData()
    {
        var expectedResponse = new OpenMeteoResponse
        {
            Daily = new DailyData
            {
                Time = [
                    new DateOnly(2026, 9, 1),
                    new DateOnly(2026, 9, 2),
                    new DateOnly(2026, 9, 3),
                    new DateOnly(2026, 9, 4),
                    new DateOnly(2026, 9, 5)
                ],
                Temperature2mMax = [29.5, 30.1, 29.0, 29.0, 29.0],
            }
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedResponse, options: options)
        };

        using var testHandler = new FakeHttpMessageHandler(httpResponse);
        using var httpClient = new HttpClient(testHandler)
        {
            BaseAddress = new Uri("https://open-meteo.com")
        };
        var repository = new WeatherRepository(httpClient);

        var result = await repository.FetchForecastAsync(35.6785, 139.6823, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.IsNotNull(testHandler.LastRequest);

        Assert.AreEqual(HttpMethod.Get, testHandler.LastRequest.Method);
        Assert.IsNotNull(testHandler.LastRequest.RequestUri);
        Assert.Contains("v1/forecast", testHandler.LastRequest.RequestUri.ToString(), StringComparison.Ordinal);

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
        var repository = new WeatherRepository(httpClient);

        // 3. 実行 & 検証
        // GetFromJsonAsync はステータスコードがエラー（4xx, 5xx）の場合、
        // 内部で EnsureSuccessStatusCode() を呼び出して HttpRequestException を発生させます。
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await repository.FetchForecastAsync(35.6785, 139.6823, CancellationToken.None);
        });

        // 4. 追加の検証（必要に応じてステータスコードが500であることを確認）
        Assert.AreEqual(HttpStatusCode.InternalServerError, exception.StatusCode);

        // リクエストが確かに送信されたかもチェック
        Assert.IsNotNull(fakeHandler.LastRequest);
        Assert.AreEqual(HttpMethod.Get, fakeHandler.LastRequest.Method);
    }
}
