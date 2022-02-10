namespace Blazor.App.Data;

public class WeatherDataStore
{
    private List<DboWeatherForecast> _weatherForecasts = new List<DboWeatherForecast>();

    public WeatherDataStore()
        =>  _weatherForecasts = this.GetForecastList();

    public ValueTask<List<DboWeatherForecast>> GetWeatherForecastsAsync(ListOptions options)
    {
        var baseList = _weatherForecasts
            .OrderBy(item => item.Date)
            .Skip(options.StartRecord)
            .Take(options.PageSize)
            .ToList();

        var returnList = new List<DboWeatherForecast>();

        foreach (var item in baseList)
            returnList.Add(item with { });

        return ValueTask.FromResult(returnList);
    }

    public ValueTask<int> GetWeatherForecastCountAsync()
        => ValueTask.FromResult(_weatherForecasts.Count);

    public ValueTask<DboWeatherForecast> GetWeatherForecastAsync(Guid Id)
    {
        var record = _weatherForecasts.FirstOrDefault(item => item.WeatherForecastId == Id);
        return ValueTask.FromResult(record is null
            ? new DboWeatherForecast()
            : record with { }
        );
    }

    public async ValueTask<bool> SaveWeatherForecastAsync(DboWeatherForecast record)
    {
        var isOverwrite = await this.DeleteWeatherForecastAsync(record.WeatherForecastId);
        _weatherForecasts.Add(record);
        return isOverwrite;
    }

    public ValueTask<bool> DeleteWeatherForecastAsync(Guid Id)
    {
        var record = _weatherForecasts.FirstOrDefault(item => item.WeatherForecastId == Id);
        if (record is not null)
            _weatherForecasts.Remove(record);

        return ValueTask.FromResult(record is not null);
    }

    private List<DboWeatherForecast> GetForecastList()
    {
        return Enumerable.Range(1, 100).Select(index => new DboWeatherForecast
        {
            WeatherForecastId = Guid.NewGuid(),
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToList();
    }

    private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
}
