global using Blazor.App.Core;
global using Blazor.App.Data;
global using Blazor.App.UI;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherDataStore>();
builder.Services.AddSingleton<IWeatherForecastDataBroker, WeatherForecastServerDataBroker>();
builder.Services.AddScoped<WeatherForcastListViewService>();
builder.Services.AddScoped<WeatherForcastCrudViewService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.Run();
