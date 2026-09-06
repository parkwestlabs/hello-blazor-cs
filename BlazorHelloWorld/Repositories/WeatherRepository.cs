using BlazorHelloWorld.Models;
using System.Security.Cryptography;

namespace BlazorHelloWorld.Repositories;

// 本物の実装（HttpClientでOpen-Meteoを叩く想定）
public class WeatherRepository() : IWeatherRepository
{
    public async Task<WeatherForecast[]> FetchFromApiAsync()
    {
        // 💡 将来的にここで HttpClient を使って Open-Meteo API を叩いたり、Dapper を呼んだりします
        // 今はアプリが正常に動くこと（画面表示）を確認するため、非同期でデータを返します
        await Task.Delay(500);

        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot" };

        return [
            .. Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = RandomNumberGenerator.GetInt32(-20, 55),
                Summary = summaries[RandomNumberGenerator.GetInt32(summaries.Length)]
            })
        ];
    }
}
