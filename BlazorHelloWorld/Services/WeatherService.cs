using BlazorHelloWorld.Models;
using BlazorHelloWorld.Repositories;

namespace BlazorHelloWorld.Services;

public class WeatherService(IWeatherRepository repository) : IWeatherService
{
    // https://api.open-meteo.com/v1/forecast?latitude=35.6895&longitude=139.6917&current_weather=true
    private const double TokyoLatitude = 35.6895;
    private const double TokyoLongitude = 139.6917;

    public async Task<WeatherForecast[]> GetActiveForecastsAsync(CancellationToken cancellationToken = default)
    {
        var response = await repository.FetchForecastAsync(TokyoLatitude, TokyoLongitude, cancellationToken);

        if (response?.Daily is null)
        {
            return [];
        }

        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        // Zipを使って安全に2つのリストをペアリング
        return [.. response.Daily.Time
            .Zip(response.Daily.Temperature2mMax).Index()
            .Select(item =>
            {
                var (date, temp) = item.Item;
                return new WeatherForecast
                {
                    Date = date,
                    TemperatureC = (int)Math.Round(temp),
                    Summary = summaries[item.Index]
                };
            })];
    }
}
