namespace Blazor.App.Core;

public class WeatherForcastCrudViewService
{
    private IWeatherForecastDataBroker _dataBroker;
    private WeatherForcastListViewService _listService;

    public DboWeatherForecast Record { get; private set; } = new DboWeatherForecast();

    public event EventHandler? RecordChanged;

    public WeatherForcastCrudViewService(IWeatherForecastDataBroker weatherForecastDataBroker, WeatherForcastListViewService weatherForcastListViewService)
    { 
        _dataBroker = weatherForecastDataBroker;
        _listService = weatherForcastListViewService;
    }

    public async ValueTask GetRecordAsync(Guid Id)
    {
        this.Record = await _dataBroker.GetWeatherForecastAsync(Id);
        this.NotifyRecordChanged(this, EventArgs.Empty);
    }

    public async ValueTask SaveRecordAsync(DboWeatherForecast record)
    {
        await _dataBroker.SaveWeatherForecastAsync(record);
        await this.GetRecordAsync(record.Id);
    }

    public async ValueTask DeleteRecordAsync(Guid Id)
    {
        await _dataBroker.DeleteWeatherForecastAsync(Id);
        this.Record = new DboWeatherForecast();
        this.NotifyRecordChanged(this, EventArgs.Empty);
    }

    protected void NotifyRecordChanged(object? sender, EventArgs e)
    {
        this.RecordChanged?.Invoke(sender, e);
        _listService.NotifyListChanged(this, EventArgs.Empty); 
    }
}
