global using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Blazor.App.Core;
using Blazor.App.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers().PartManager.ApplicationParts.Add(new AssemblyPart(typeof(Blazor.App.Controllers.WeatherForecastController).Assembly));
builder.Services.AddSingleton<WeatherDataStore>();
builder.Services.AddSingleton<IWeatherForecastDataBroker, WeatherForecastServerDataBroker>();   

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

app.UseBlazorFrameworkFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
