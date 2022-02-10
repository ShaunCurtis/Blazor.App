namespace Blazor.App.Core;

public class WeatherForecastNotificationService
{

    public event EventHandler<RecordSetChangedEventArgs>? RecordSetChanged;

    public event EventHandler<RecordChangedEventArgs>? RecordChanged;

    public void NotifyRecordSetChanged(object? sender, RecordSetChangedEventArgs e)
        =>  this.RecordSetChanged?.Invoke(this, e);

    public void NotifyRecordChanged(object? sender, RecordChangedEventArgs e)
        => this.RecordChanged?.Invoke(this, e);
}

