using MyApp.Core.Models;

namespace MyApp.Web.Components.Pages;

public partial class Weather
{
    private WeatherForecast[]? Forecasts { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Forecasts = await WeatherService.GetActiveForecastsAsync();
    }
}
