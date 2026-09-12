using BlazorHelloWorld.Models;

namespace BlazorHelloWorld.Services;

public interface IWeatherService
{
    Task<WeatherForecast[]> GetActiveForecastsAsync(CancellationToken cancellationToken = default);
}
