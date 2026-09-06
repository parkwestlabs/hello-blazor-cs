using BlazorHelloWorld.Models;
using BlazorHelloWorld.Services;
using BlazorHelloWorld.Repositories;
using NSubstitute;

namespace BlazorHelloWorld.Tests.Services;

public class WeatherServiceTest
{
    [Fact]
    public async Task GetActiveForecastsAsync_ShouldFilterOutEmptySummaries()
    {
        var mockRepository = Substitute.For<IWeatherRepository>();

        var dummyFromApi = new WeatherForecast[]
        {
            new() { Date = new(2026, 9, 6), TemperatureC = 20, Summary = "Chilly" },
            new() { Date = new(2026, 9, 7), TemperatureC = 25, Summary = "" } // 💡 除去されるべきゴミデータ
        };
        mockRepository.FetchFromApiAsync().Returns(Task.FromResult(dummyFromApi));

        // 1. Arrange: テスト対象のサービスをインスタンス化
        var service = new WeatherService(mockRepository);

        // 2. Act: メソッドを非同期で実行
        var result = await service.GetActiveForecastsAsync();

        // 3. Assert: 取得したデータの検証
        Assert.NotNull(result);
        Assert.Single(result); // 2件中1件が除外されて、1件になっているはず！
        Assert.Equal("Chilly", result[0].Summary);
    }
}
