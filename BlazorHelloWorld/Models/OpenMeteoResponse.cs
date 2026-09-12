using System.Text.Json.Serialization;

namespace BlazorHelloWorld.Models;

public class OpenMeteoResponse
{
    public required DailyData Daily { get; init; }
}

public class DailyData
{
    public required IReadOnlyList<DateOnly> Time { get; init; }
    /// <remarks>
    /// JsonNamingPolicy.SnakeCaseLower が temperature2_m_max にしてしまう防止策が必要
    /// </remarks>
    [JsonPropertyName("temperature_2m_max")]
    public required IReadOnlyList<double> Temperature2mMax { get; init; }
}
