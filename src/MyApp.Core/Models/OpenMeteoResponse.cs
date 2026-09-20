using System.Text.Json.Serialization;

namespace MyApp.Core.Models;

public record OpenMeteoResponse(DailyData Daily);

/// <summary>
/// Open-Meteo から返される日ごとの気象データ
/// </summary>
/// <param name="Time"></param>
/// <param name="Temperature2mMax">
/// JsonNamingPolicy.SnakeCaseLower が temperature2_m_max にしてしまう防止策が必要
/// </param>
public record DailyData(
    IReadOnlyList<DateOnly> Time,
    [property: JsonPropertyName("temperature_2m_max")]
    IReadOnlyList<double> Temperature2mMax
);
