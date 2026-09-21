using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.Options;

public class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    [Required]
    public required Uri BaseUrl { get; set; }
    public int TimeoutSeconds { get; set; } = 100; // HttpClientのデフォルトは100秒
}
