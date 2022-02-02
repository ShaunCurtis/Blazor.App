using MVC = Microsoft.AspNetCore.Mvc;

namespace Blazor.App.Controllers;

[ApiController]
public class WeatherForecastController : Controller
{
    protected IWeatherForecastDataBroker DataService { get; set; }

    public WeatherForecastController(IWeatherForecastDataBroker dataService)
        => this.DataService = dataService;

    [MVC.Route("/api/weatherforecast/list")]
    [HttpPost]
    public async Task<List<DboWeatherForecast>> Read([FromBody] ListOptions options)
        => await DataService.GetWeatherForecastsAsync(options);

    [MVC.Route("/api/weatherforecast/count")]
    [HttpGet]
    public async Task<int> Count()
        => await DataService.GetWeatherForecastCountAsync();

    [MVC.Route("/api/weatherforecast/get")]
    [HttpGet]
    public async Task<DboWeatherForecast> GetRec(Guid id)
        => await DataService.GetWeatherForecastAsync(id);

    [MVC.Route("/api/weatherforecast/save")]
    [HttpPost]
    public async Task<bool> Save([FromBody] DboWeatherForecast record)
        => await DataService.SaveWeatherForecastAsync(record);


    [MVC.Route("/api/weatherforecast/delete")]
    [HttpPost]
    public async Task<bool> Delete([FromBody] Guid id)
        => await DataService.DeleteWeatherForecastAsync(id);
}