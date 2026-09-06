using System.Security.Cryptography;
using BlazorHelloWorld.Models;

namespace BlazorHelloWorld.Components.Pages;

public partial class Weather
{
    private WeatherForecast[]? Forecasts { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // Simulate asynchronous loading to demonstrate streaming rendering
        await Task.Delay(500);

        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        Forecasts = [
            .. Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = RandomNumberGenerator.GetInt32(-20, 55),
                Summary = summaries[RandomNumberGenerator.GetInt32(summaries.Length)]
            })
        ];
    }
}
