using BlazorHelloWorld.Models;

namespace BlazorHelloWorld.Repositories;

public interface IWeatherRepository
{
    Task<WeatherForecast[]> FetchFromApiAsync();
}
