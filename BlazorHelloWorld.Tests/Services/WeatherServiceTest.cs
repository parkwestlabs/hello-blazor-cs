using BlazorHelloWorld.Models;
using BlazorHelloWorld.Services;
using BlazorHelloWorld.Repositories;
using NSubstitute;

namespace BlazorHelloWorld.Tests.Services;

public class WeatherServiceTest
{
    [Fact]
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

        mockRepository.FetchForecastAsync(Arg.Any<double>(), Arg.Any<double>())
            .Returns(dummyResponse);

        // 1. Arrange: テスト対象のサービスをインスタンス化
        var service = new WeatherService(mockRepository);

        // 2. Act: メソッドを非同期で実行
        var result = await service.GetActiveForecastsAsync();

        // 3. Assert: 取得したデータの検証
        Assert.NotNull(result);
        Assert.Equal(5, result.Length);
        Assert.Equal("Freezing", result[0].Summary);
        Assert.Equal(30, result[0].TemperatureC);
    }

    [Fact]
    public async Task GetActiveForecastsAsync_WhenRepositoryReturnsNull_ReturnsEmptyArray()
    {
        // Arrange
        var mockRepository = Substitute.For<IWeatherRepository>();

        mockRepository
            .FetchForecastAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns((OpenMeteoResponse?)null);

        var service = new WeatherService(mockRepository);

        // Act
        var result = await service.GetActiveForecastsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveForecastsAsync_WhenDailyIsNull_ReturnsEmptyArray()
    {
        // Arrange
        var mockRepository = Substitute.For<IWeatherRepository>();

        // Daily プロパティを null にしたオブジェクトを作成
        var responseWithNullDaily = new OpenMeteoResponse { Daily = null! };

        mockRepository.FetchForecastAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns(responseWithNullDaily);

        var service = new WeatherService(mockRepository);

        // Act
        var result = await service.GetActiveForecastsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result); // if (response?.Daily is null) 内の return []; を通過
    }
}
