namespace Blazor.App.Test;

public class DataBrokerTests
{
    [Theory]
    [InlineData(1000, 0, 100)]
    [InlineData(10, 20, 10)]
    [InlineData(1000, 50, 50)]
    public async void DoWeGet100Records(int pageSize, int startRecord, int expectedCount)
    {
        //Define
        WeatherDataStore dataStore = new WeatherDataStore();
        IWeatherForecastDataBroker? _dataBroker = new WeatherForecastServerDataBroker(dataStore) as IWeatherForecastDataBroker;
        IWeatherForecastDataBroker? dataBroker = _dataBroker!;
        var listOptions = new ListOptions { PageSize = pageSize, StartRecord = startRecord };

        //Test
        var list = await dataBroker.GetWeatherForecastsAsync(listOptions);

        //Assert
        Assert.Equal(expectedCount, list.Count);
    }

    [Fact]
    public async void CanWeAddARecord()
    {
        //Define
        WeatherDataStore dataStore = new WeatherDataStore();
        IWeatherForecastDataBroker? _dataBroker = new WeatherForecastServerDataBroker(dataStore) as IWeatherForecastDataBroker;
        IWeatherForecastDataBroker? dataBroker = _dataBroker!;

        var listOptions = new ListOptions { PageSize = 1000, StartRecord = 0 };
        var id = Guid.NewGuid();
        var newRecord = new DboWeatherForecast() { Date = DateTime.Now, Id = id, Summary = "Testing", TemperatureC = 20 };

        //Test
        var result = await dataBroker.SaveWeatherForecastAsync(newRecord);
        var list = await dataBroker.GetWeatherForecastsAsync(listOptions);
        var returnRecord = await dataBroker.GetWeatherForecastAsync(id);

        //Assert
        Assert.Equal(101, list.Count);
        Assert.Equal(newRecord, returnRecord);
        Assert.False(result);
    }

    [Fact]
    public async void CanWeDeleteARecord()
    {
        //Define
        WeatherDataStore dataStore = new WeatherDataStore();
        IWeatherForecastDataBroker? _dataBroker = new WeatherForecastServerDataBroker(dataStore) as IWeatherForecastDataBroker;
        IWeatherForecastDataBroker? dataBroker = _dataBroker!;

        var listOptions = new ListOptions { PageSize = 1000, StartRecord = 0 };

        var list = await dataBroker.GetWeatherForecastsAsync(listOptions);

        var record = list[10];
        var id = record.Id;

        //Test
        var result = await dataBroker.DeleteWeatherForecastAsync(id);
        var newList = await dataBroker.GetWeatherForecastsAsync(listOptions);

        //Assert
        Assert.Equal(99, newList.Count);
        Assert.True(result);
    }

}