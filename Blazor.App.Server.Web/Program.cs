global using Blazor.App.Core;
global using Blazor.App.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
{
    services.AddRazorPages();
    services.AddServerSideBlazor();
    services.AddSingleton<WeatherDataStore>();
    services.AddSingleton<IWeatherForecastDataBroker, WeatherForecastServerDataBroker>();
    services.AddSingleton<WeatherForecastNotificationService>();
    services.AddScoped<WeatherForcastListViewService>();
    services.AddScoped<WeatherForcastCrudViewService>();
}

var app = builder.Build();

// Http Request Middleware Pipeline
{
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseRouting();

    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");
}

app.Run();
