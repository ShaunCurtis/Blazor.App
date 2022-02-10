using Blazor.App.Core;
using Blazor.App.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Blazor.App.UI.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure the services container
var services = builder.Services;
{
    services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    services.AddScoped<IWeatherForecastDataBroker, WeatherForecastAPIDataBroker>();
    services.AddSingleton<WeatherForecastNotificationService>();
    services.AddScoped<WeatherForcastListViewService>();
    services.AddScoped<WeatherForcastCrudViewService>();
}

await builder.Build().RunAsync();
