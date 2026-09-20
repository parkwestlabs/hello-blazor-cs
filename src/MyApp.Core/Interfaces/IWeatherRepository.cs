using MyApp.Core.Models;

namespace MyApp.Core.Interfaces;

public interface IWeatherRepository
{
    Task<OpenMeteoResponse?> FetchForecastAsync(
        double latitude,
        double longitude,
        TimeSpan cacheDuration = default,   // default = TimeSpan.Zero (no cache)
        CancellationToken cancellationToken = default
    );
}
