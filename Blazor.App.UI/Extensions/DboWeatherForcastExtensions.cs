namespace Blazor.App.UI;

    public static class DboWeatherForcastExtensions
    {
        public static int TemperatureF(this DboWeatherForecast forecast)
            => 32 + (int)(forecast.TemperatureC / 0.5556);
    }

