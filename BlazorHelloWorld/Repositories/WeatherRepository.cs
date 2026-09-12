using System.Text.Json;
using BlazorHelloWorld.Models;

namespace BlazorHelloWorld.Repositories;

public partial class WeatherRepository(HttpClient httpClient) : IWeatherRepository
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public async Task<OpenMeteoResponse?> FetchForecastAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default
    )
    {
        var requestUri = $"v1/forecast?latitude={latitude}&longitude={longitude}&daily=temperature_2m_max";

        return await httpClient.GetFromJsonAsync<OpenMeteoResponse>(
            requestUri, _jsonOptions, cancellationToken
        );
    }
}
