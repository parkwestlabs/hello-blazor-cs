using MyApp.Core.Models;

namespace MyApp.Core.Interfaces;

public interface IWeatherService
{
    Task<WeatherForecast[]> GetActiveForecastsAsync(CancellationToken cancellationToken = default);
}
