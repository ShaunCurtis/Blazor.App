namespace Blazor.App.Core;

public interface IWeatherForecastDataBroker
{
    public ValueTask<List<DboWeatherForecast>> GetWeatherForecastsAsync(ListOptions options);

    public ValueTask<int> GetWeatherForecastCountAsync();

    public ValueTask<DboWeatherForecast> GetWeatherForecastAsync(Guid Id);

    public ValueTask<bool> SaveWeatherForecastAsync(DboWeatherForecast record);

    public ValueTask<bool> DeleteWeatherForecastAsync(Guid Id);

}

