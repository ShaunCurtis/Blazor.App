namespace Blazor.App.Core;

public class WeatherForcastCrudViewService
{
    private IWeatherForecastDataBroker _dataBroker;
    private WeatherForecastNotificationService _notificationService;

    public DboWeatherForecast Record { get; private set; } = new DboWeatherForecast();

    public WeatherForcastCrudViewService(IWeatherForecastDataBroker weatherForecastDataBroker, WeatherForecastNotificationService weatherForecastNotificationService)
    { 
        _dataBroker = weatherForecastDataBroker;
        _notificationService = weatherForecastNotificationService;
    }

    public async ValueTask GetRecordAsync(Guid Id)
    {
        this.Record = await _dataBroker.GetWeatherForecastAsync(Id);
        this.NotifyRecordChanged();
    }

    public async ValueTask SaveRecordAsync(DboWeatherForecast record)
    {
        await _dataBroker.SaveWeatherForecastAsync(record);
        await this.GetRecordAsync(record.WeatherForecastId);
        this.NotifyRecordSetChanged();
    }

    public async ValueTask DeleteRecordAsync(Guid Id)
    {
        await _dataBroker.DeleteWeatherForecastAsync(Id);
        this.Record = new DboWeatherForecast();
        this.NotifyRecordChanged();
        this.NotifyRecordSetChanged();
    }

    protected void NotifyRecordChanged()
        =>  _notificationService.NotifyRecordChanged(this, RecordChangedEventArgs.Create(Record.WeatherForecastId));

    protected void NotifyRecordSetChanged(bool recordOnly = false)
        =>  _notificationService.NotifyRecordSetChanged(this, RecordSetChangedEventArgs.Create<DboWeatherForecast>());
}
