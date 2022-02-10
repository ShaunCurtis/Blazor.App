namespace Blazor.App.Core;

public record DboWeatherForecast
{
    public Guid WeatherForecastId { get; init; } = Guid.Empty;

    public DateTime Date { get; init; }

    public int TemperatureC { get; init; }

    public string? Summary { get; init; }
}
