namespace Blazor.App.Data;

public class WeatherForecastServerDataBroker : IWeatherForecastDataBroker
{
    private readonly WeatherDataStore _weatherDataStore;

    public WeatherForecastServerDataBroker(WeatherDataStore weatherDataStore)
        => _weatherDataStore = weatherDataStore;

    public async ValueTask<List<DboWeatherForecast>> GetWeatherForecastsAsync(ListOptions options)
        => await _weatherDataStore.GetWeatherForecastsAsync(options);

    public async ValueTask<int> GetWeatherForecastCountAsync()
        => await _weatherDataStore.GetWeatherForecastCountAsync();

    public async ValueTask<DboWeatherForecast> GetWeatherForecastAsync(Guid Id)
        => await _weatherDataStore.GetWeatherForecastAsync(Id);

    public async ValueTask<bool> SaveWeatherForecastAsync(DboWeatherForecast record)
        => await _weatherDataStore.SaveWeatherForecastAsync(record);

    public async ValueTask<bool> DeleteWeatherForecastAsync(Guid Id)
        => await _weatherDataStore.DeleteWeatherForecastAsync(Id);
}

