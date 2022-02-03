using Blazor.App.Core;
using Blazor.App.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Blazor.App.UI.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IWeatherForecastDataBroker, WeatherForecastAPIDataBroker>();
builder.Services.AddScoped<WeatherForcastListViewService>();
builder.Services.AddScoped<WeatherForcastCrudViewService>();

await builder.Build().RunAsync();
