using BlazorHelloWorld.Models;
using BlazorHelloWorld.Services;
using BlazorHelloWorld.Repositories;
using NSubstitute;

namespace BlazorHelloWorld.Tests.Services;

[TestClass]
public class WeatherServiceTest
{
    [TestMethod]
    public async Task GetActiveForecastsAsync_WhenResponseIsValid_ReturnsForecasts()
    {
        var mockRepository = Substitute.For<IWeatherRepository>();

        var dummyResponse = new OpenMeteoResponse
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

        mockRepository.FetchForecastAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns(dummyResponse);

        // 1. Arrange: テスト対象のサービスをインスタンス化
        var service = new WeatherService(mockRepository);

        // 2. Act: メソッドを非同期で実行
        var result = await service.GetActiveForecastsAsync(CancellationToken.None);

        // 3. Assert: 取得したデータの検証
        Assert.IsNotNull(result);
        Assert.HasCount(5, result);
        Assert.AreEqual("Freezing", result[0].Summary);
        Assert.AreEqual(30, result[0].TemperatureC);
    }

    [TestMethod]
    public async Task GetActiveForecastsAsync_WhenRepositoryReturnsNull_ReturnsIsEmptyArray()
    {
        // Arrange
        var mockRepository = Substitute.For<IWeatherRepository>();

        mockRepository
            .FetchForecastAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns((OpenMeteoResponse?)null);

        var service = new WeatherService(mockRepository);

        // Act
        var result = await service.GetActiveForecastsAsync(Arg.Any<CancellationToken>());

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task GetActiveForecastsAsync_WhenDailyIsNull_ReturnsIsEmptyArray()
    {
        // Arrange
        var mockRepository = Substitute.For<IWeatherRepository>();

        // Daily プロパティを null にしたオブジェクトを作成
        var responseWithNullDaily = new OpenMeteoResponse { Daily = null! };

        mockRepository.FetchForecastAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns(responseWithNullDaily);

        var service = new WeatherService(mockRepository);

        // Act
        var result = await service.GetActiveForecastsAsync(Arg.Any<CancellationToken>());

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result); // if (response?.Daily is null) 内の return []; を通過
    }
}
