using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MyApp.Core.Models;
using MyApp.Core.Interfaces;

namespace MyApp.Data.Repositories;

public class WeatherRepository(HttpClient httpClient, IMemoryCache cache, ILogger<WeatherRepository> logger) : IWeatherRepository
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        RespectRequiredConstructorParameters = true,
    };

    public async Task<OpenMeteoResponse?> FetchForecastAsync(
        double latitude,
        double longitude,
        TimeSpan cacheDuration = default,   // default = TimeSpan.Zero (no cache)
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("cacheDuration {CacheDuration}", cacheDuration);

        if (cacheDuration == TimeSpan.Zero)
        {
            return await FetchForecastFromApiAsync(latitude, longitude, cancellationToken);
        }

        logger.LogInformation("Fetching with cache for ({Latitude}, {Longitude})", latitude, longitude);
        string cacheKey = $"weather-{latitude}-{longitude}";

        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = cacheDuration;

            return await FetchForecastFromApiAsync(latitude, longitude, cancellationToken);
        });
    }

    private async Task<OpenMeteoResponse?> FetchForecastFromApiAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("Requesting API...");

        var requestUri = $"v1/forecast?latitude={latitude}&longitude={longitude}&daily=temperature_2m_max";

        return await httpClient.GetFromJsonAsync<OpenMeteoResponse>(
            requestUri, _jsonOptions, cancellationToken
        );
    }
}
