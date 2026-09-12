using BlazorHelloWorld.Models;

namespace BlazorHelloWorld.Repositories;

public interface IWeatherRepository
{
    Task<OpenMeteoResponse?> FetchForecastAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default
    );
}
