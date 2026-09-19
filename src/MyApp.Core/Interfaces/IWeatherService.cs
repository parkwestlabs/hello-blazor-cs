using MyApp.Core.Models;

namespace MyApp.Core.Interfaces;

public interface IWeatherService
{
    Task<List<WeatherForecast>> GetActiveForecastsAsync(CancellationToken cancellationToken = default);
}
