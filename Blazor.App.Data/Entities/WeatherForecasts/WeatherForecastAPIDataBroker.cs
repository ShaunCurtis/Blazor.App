namespace Blazor.App.Data;

public class WeatherForecastAPIDataBroker : IWeatherForecastDataBroker
{
    protected HttpClient HttpClient { get; set; }

    public WeatherForecastAPIDataBroker(HttpClient httpClient)
        => this.HttpClient = httpClient;

    public async ValueTask<List<DboWeatherForecast>> GetWeatherForecastsAsync(ListOptions options)
    {
        var response = await this.HttpClient.PostAsJsonAsync($"/api/weatherforecast/list", options);
        var list = await response.Content.ReadFromJsonAsync<List<DboWeatherForecast>>();
        list ??= new List<DboWeatherForecast>();
        return list;
    }

    public async ValueTask<int> GetWeatherForecastCountAsync()
       => await this.HttpClient.GetFromJsonAsync<int>($"/api/weatherforecast/count");

    public async ValueTask<DboWeatherForecast> GetWeatherForecastAsync(Guid id)
    {
        var response = await this.HttpClient.PostAsJsonAsync($"/api/weatherforecast/get", id);
        var result = await response.Content.ReadFromJsonAsync<DboWeatherForecast>();
        result ??= new DboWeatherForecast();
        return result;
    }

    public async ValueTask<bool> SaveWeatherForecastAsync(DboWeatherForecast record)
    {
        var response = await this.HttpClient.PostAsJsonAsync($"/api/weatherforecast/save", record);
        return await response.Content.ReadFromJsonAsync<bool>();
    }

    public async ValueTask<bool> DeleteWeatherForecastAsync(Guid id)
    {
        var response = await this.HttpClient.PostAsJsonAsync($"/api/weatherforecast/delete", id);
        return await response.Content.ReadFromJsonAsync<bool>();
    }
}

