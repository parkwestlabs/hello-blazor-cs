using MyApp.Core.Models;

namespace MyApp.Core.Interfaces;

public interface IWeatherRepository
{
    Task<OpenMeteoResponse?> FetchForecastAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default
    );
}
