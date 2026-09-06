using BlazorHelloWorld.Models;
using BlazorHelloWorld.Repositories;

namespace BlazorHelloWorld.Services;

public class WeatherService(IWeatherRepository repository) : IWeatherService
{
    public async Task<WeatherForecast[]> GetActiveForecastsAsync()
    {
        var rawData = await repository.FetchFromApiAsync();

        // 例：Summaryが空のデータを除外する、などのロジックをここで挟む
        var processedData = rawData.Where(f => !string.IsNullOrEmpty(f.Summary)).ToArray();

        return processedData;
    }
}
