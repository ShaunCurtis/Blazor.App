namespace Blazor.App.Core;

public class WeatherForcastListViewService
{
    private IWeatherForecastDataBroker _dataBroker;

    public List<DboWeatherForecast> Records { get; private set; } = new List<DboWeatherForecast>();

    public readonly ListOptions ListOptions = new ListOptions() { PageSize = 20, StartRecord = 0 };

    public event EventHandler? ListChanged;

    public int RecordCount { get; private set; }

    public WeatherForcastListViewService(IWeatherForecastDataBroker weatherForecastDataBroker)
        => _dataBroker = weatherForecastDataBroker;

    public async ValueTask GetPageAsync()
    {
        await _dataBroker.GetWeatherForecastsAsync(ListOptions);
        this.RecordCount = await _dataBroker.GetWeatherForecastCountAsync();
    }

    public virtual async ValueTask<ItemsProviderResult<DboWeatherForecast>> GetPageAsync(ItemsProviderRequest request)
    {
        this.ListOptions.StartRecord = request.StartIndex;
        this.ListOptions.PageSize = request.Count;
        this.Records = await _dataBroker.GetWeatherForecastsAsync(ListOptions);
        var listCount = this.RecordCount = await _dataBroker.GetWeatherForecastCountAsync();
        return new ItemsProviderResult<DboWeatherForecast>(this.Records, listCount);
    }

    public void NotifyListChanged(object? sender, EventArgs e)
        => this.ListChanged?.Invoke(this, EventArgs.Empty);
}
