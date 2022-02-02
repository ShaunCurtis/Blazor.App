
## Solution and Projects

Create a Blazor Server solution.

- Solution Name: *Blazor.App*
- Blazor Project Name: *Blazor.App.Server.Web*

Create four more projects:

- *Blazor.App.Core* - Class Library template.  Remove all files and add an empty `Program.cs`.
- *Blazor.App.Data* - Class Library template.  Remove all files and add an empty `Program.cs`.
- *Blazor.App.UI* - Razor Class Library template.  Remove all files except `_Imports.razor` and add an empty `Program.cs`.
- *Blazor.App.Test* - XUnit Test template.

Add the following project dependancies:

- *Blazor.App.Core* - none.
- *Blazor.App.Data* -> *Blazor.App.Core*.
- *Blazor.App.UI* -> *Blazor.App.Core*.
- *Blazor.App.Server.Web* -> *Blazor.App.Core*, *Blazor.App.UI*, *Blazor.App.Data*.
- *Blazor.App.Test* -> *Blazor.App.Core*, *Blazor.App.UI*, *Blazor.App.Data*.

Add global usings to each project `Program` for the above dependancies.

```csharp
//.Data/Program
global using Blazor.App.Core;
namespace Blazor.App.Data;
```

```csharp
//.UI/Program
global using Blazor.App.Core;
namespace Blazor.App.Data;
```

```csharp
//.Server.Web/_Imports
@using Blazor.App.Core;
```

```csharp
//.Server.Web/Program
global using Blazor.App.Core;
global using Blazor.App.Data;
global using Blazor.App.UI;
namespace Blazor.App.Data;
```

```csharp
//.Server.Web/_Imports
@using Blazor.App.Core;
@using Blazor.App.Data;
@using Blazor.App.UI;
```

At this point we have our initial project structure with three projects representing the three main Clean Design domains and two consummer projects.  Clean design dependancies principles are applied through project dependancies.

## Sorting our Data Pipeline

First step our data classes.  

1. Move `WeatherForecast.cs` to *.Core/Entities/WeatherForecasts*.  We use it throughout the application so it must live in **Core**.
2. Rename it `DboWeatherForecast`.  It's out data store object so name it as such.
3. Make it a record.  It should be a value object and immutable.  We'll look at how to deal with editing later. 
3. Add a Guid `Id`.  We need an Id for CRUD operations.  I like to use Guids.
4. Remove the `TemperatureF`.  Calculations and derived properties don't belong in data transfer objects.  They are presentation layer stuff.  We'll look at how to add it back in further on.

This is our new `DboWeather`:

```csharp
namespace Blazor.App.Core;
public record DboWeatherForecast
{
    public Guid Id { get; init; } = Guid.Empty;
    public DateTime Date { get; init; }
    public int TemperatureC { get; init; }
    public string? Summary { get; init; }
}
```

## Test Data Store

Our data store contains large data sets so we need to page our list data.  To do this we pass a `ListOptions` object wghwnever we request a data set.  This is a **Core Domain** object

```csharp
// .Core/Data/ListOptions.cs
namespace Blazor.App.Core;
public class ListOptions
{
    public int StartRecord { get; set; }
    public int PageSize { get; set; } = 10;
}
```

### WeatherDataStore

First some internal methods w've lifted and adapted from `WeatherForecastService` to build a data set.

```csharp
    private List<DboWeatherForecast> GetForecastList()
    {
        return Enumerable.Range(1, 100).Select(index => new DboWeatherForecast
        {
            Id = Guid.NewGuid(),
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToList();
    }

    private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
}
```

Our class declaration.  This class will run as a singleton service.  It has an internal list of weather forecast records that are generated on instantiation.

```
namespace Blazor.App.Data;

public class WeatherDataStore
{
    private List<DboWeatherForecast> _weatherForecasts = new List<DboWeatherForecast>();

    public WeatherDataStore()
        => _weatherForecasts = this.GetForecastList();
//......
}
```

Next our get the dataset method

1. Declared async.  We're emulating a real database which will be async so we start that way.
2. Takes a `ListOptions` argument to define the chunk of the data set the UI will display.
3. By default ordwrs the list by `Date`.
4. Uses `Skip` anbd `Take` to get the correct chunk.
5. Returns a copy of the recordset, not a recordset that references the internally held records.  This is where using records comes in, `item with { }` returns a new copy of item.

```csharp
public ValueTask<List<DboWeatherForecast>> GetWeatherForecastsAsync(ListOptions options)
{
    var baseList = _weatherForecasts
        .OrderBy(item => item.Date)
        .Skip(options.StartRecord)
        .Take(options.PageSize)
        .ToList();

    var returnList = new List<DboWeatherForecast>();

    foreach (var item in baseList)
        returnList.Add(item with { });

    return ValueTask.FromResult(returnList);
}

public ValueTask<int> GetWeatherForecastCountAsync()
    => ValueTask.FromResult(_weatherForecasts.Count);
```

And then finally the CRUD methods.  We return a copy of the record for the get, and we delete if necessary and add for save.

```csharp
public ValueTask<DboWeatherForecast> GetWeatherForecastAsync(Guid Id)
{
    var record = _weatherForecasts.FirstOrDefault(item => item.Id == Id);
    return ValueTask.FromResult(record is null
        ? new DboWeatherForecast()
        : record with { }
    );
}

public async ValueTask<bool> SaveWeatherForecastAsync(DboWeatherForecast record)
{
    var isOverwrite = await this.DeleteWeatherForecastAsync(record.Id);
    _weatherForecasts.Add(record);
    return isOverwrite;
}

public ValueTask<bool> DeleteWeatherForecastAsync(Guid Id)
{
    var record = _weatherForecasts.FirstOrDefault(item => item.Id == Id);
    if (record is not null)
        _weatherForecasts.Remove(record);

    return ValueTask.FromResult(record is not null);
}
```

## Data Brokers

We now need to sort the interface between our Data and Core domains/projects.  We do this through an interface.  The Core project defines the interface it will use to get data, the  Data domain provides implementations of that interface.  The comsummer project defines which implementation is available by defining an interface to class mapped service in it's Dependancy Injection
container.

### IWeatherForecastDataBroker

First the interface which defines our data access methods.

```csharp
// .Core/Entities/WeatherForecasts
namespace Blazor.App.Core;
public interface IWeatherForecastDataBroker
{
    public ValueTask<List<DboWeatherForecast>> GetWeatherForecastsAsync(ListOptions options);
    public ValueTask<int> GetWeatherForecastCountAsync();
    public ValueTask<DboWeatherForecast> GetWeatherForecastAsync(Guid Id);
    public ValueTask<bool> SaveWeatherForecastAsync(DboWeatherForecast record);
    public ValueTask<bool> DeleteWeatherForecastAsync(Guid Id);
}
```

### WeatherForecastServerDataBroker

And our initial implementation.  This is the one used by the Server application (and the API controller project at a later stage).  It just calls the methods in the data store.  If this were a Entity Framework data broker it would contains the EF interface code.

```csharp
// .Data/Entities/WeatherForecasts
namespace Blazor.App.Data;

public class WeatherForecastServerDataBroker
{
    private readonly WeatherDataStore _weatherDataStore;

    public WeatherForecastServerDataBroker(WeatherDataStore weatherDataStore)
        => _weatherDataStore = weatherDataStore;

    public async ValueTask<List<DboWeatherForecast>> GetWeatherForecastsAsync(ListOptions options)
        => await _weatherDataStore.GetWeatherForecastsAsync(options);

    public async ValueTask<int> GetWeatherForecastCountAsync()
        => await _weatherDataStore.GetWeatherForecastCountAsync();

    public async ValueTask<DboWeatherForecast> GetWeatherForecastAsync(Guid Id)
        => await _weatherDataStore.GetWeatherForecastAsync(Id);

    public async ValueTask<bool> SaveWeatherForecastAsync(DboWeatherForecast record)
        => await _weatherDataStore.SaveWeatherForecastAsync(record);

    public async ValueTask<bool> DeleteWeatherForecastAsync(Guid Id)
        => await _weatherDataStore.DeleteWeatherForecastAsync(Id);
}
```

## Testing

At this point we can set up some tests in the test project.

Update `Program` in the Test project.

```csharp
global using Blazor.App.Core;
global using Blazor.App.Data;
global using System;
global using Xunit;
```

Here are the three tests I set up.

1. Testing getting a list based on various ListOption settings.

```csharp
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
```

2. Test adding a record.

```csharp
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
```

3. Test adding a record.

```csharp
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
```

## View Services

I break my View Services into 

1. *ListViewServices* which are used by lists - in our case `FetchData` will use the `WeatherForecastListService` for it's data.
2. *CrudViewServices* which provide Create/Read/Update/Delete services for a single record.

### WeatherForcastListViewService

First our `WeatherForcastListViewService`.

1. Injects the `IWeatherForecastDataBroker` configured service for data access.
2. `Records` is the current display record set.
3. There are two `GetRecordsAsync` methods.  One for normal paging components, the other specifically for the `Virtualize` component.
4. The `ListChanged` event is triggered whenever `Records` changes.

```csharp
namespace Blazor.App.Core;

public class WeatherForcastListViewService
{
    private IWeatherForecastDataBroker _dataBroker;

    public List<DboWeatherForecast> Records { get; private set; } = new List<DboWeatherForecast>();
    public readonly ListOptions ListOptions = new ListOptions() { PageSize = 20, StartRecord = 0 };
    public event EventHandler? ListChanged;
    public event EventHandler? PageChanged;
    public int RecordCount { get; private set; }

    public WeatherForcastListViewService(IWeatherForecastDataBroker weatherForecastDataBroker)
        => _dataBroker = weatherForecastDataBroker;

    public async ValueTask GetRecordsAsync()
    {
        await _dataBroker.GetWeatherForecastsAsync(ListOptions);
        this.RecordCount = await _dataBroker.GetWeatherForecastCountAsync();
        this.NotifyListChanged(this, EventArgs.Empty);
    }

    public virtual async ValueTask<ItemsProviderResult<DboWeatherForecast>> GetRecordsAsync(ItemsProviderRequest request)
    {
        this.ListOptions.StartRecord = request.StartIndex;
        this.ListOptions.PageSize = request.Count;
        this.Records = await _dataBroker.GetWeatherForecastsAsync(ListOptions);
        var listCount = this.RecordCount = await _dataBroker.GetWeatherForecastCountAsync();
        return new ItemsProviderResult<DboWeatherForecast>(this.Records, listCount);
    }

    public void NotifyListChanged(object? sender, EventArgs e)
        => NotifyListChanged(sender, e);
}
```
 
### WeatherForcastListViewService

First our `WeatherForcastListViewService`.

1. Injects the `IWeatherForecastDataBroker` configured service for data access.
2. `Records` is the current display record set.
3. There are two `GetRecordsAsync` methods.  One for normal paging components, the other specifically for the `Virtualize` component.
4. There is a `ListChanged` event which is raised when GetListAsync is called.  This can be triggered externally by calling `NotifyListChanged`.
5. There is a `PageChanged` event which is raised when a paging event happens.  It's raised whenever `GetPageAsync` is called.

```csharp
namespace Blazor.App.Core;

public class WeatherForcastListViewService
{
    private IWeatherForecastDataBroker _dataBroker;
    public List<DboWeatherForecast> Records { get; private set; } = new List<DboWeatherForecast>();
    public readonly ListOptions ListOptions = new ListOptions() { PageSize = 20, StartRecord = 0 };
    public event EventHandler? ListChanged;
    public event EventHandler? PageChanged;
    public int RecordCount { get; private set; }

    public WeatherForcastListViewService(IWeatherForecastDataBroker weatherForecastDataBroker)
        => _dataBroker = weatherForecastDataBroker;

    public async ValueTask GetListAsync()
    {
        this.Records = await _dataBroker.GetWeatherForecastsAsync(ListOptions);
        this.RecordCount = await _dataBroker.GetWeatherForecastCountAsync();
        this.NotifyListChanged(this, EventArgs.Empty);
    }

    public async ValueTask GetPageAsync()
    {
        await _dataBroker.GetWeatherForecastsAsync(ListOptions);
        this.RecordCount = await _dataBroker.GetWeatherForecastCountAsync();
        this.NotifyPageChanged(this, EventArgs.Empty);
    }

    public virtual async ValueTask<ItemsProviderResult<DboWeatherForecast>> GetPageAsync(ItemsProviderRequest request)
    {
        this.ListOptions.StartRecord = request.StartIndex;
        this.ListOptions.PageSize = request.Count;
        this.Records = await _dataBroker.GetWeatherForecastsAsync(ListOptions);
        var listCount = this.RecordCount = await _dataBroker.GetWeatherForecastCountAsync();
        this.NotifyPageChanged(this, EventArgs.Empty);
        return new ItemsProviderResult<DboWeatherForecast>(this.Records, listCount);
    }

    public async void NotifyListChanged(object? sender, EventArgs e)
        => await GetListAsync();

    protected void NotifyPageChanged(object? sender, EventArgs e)
        => PageChanged?.Invoke(sender, e);
}
```

### WeatherForcastCrudViewService

```csharp
namespace Blazor.App.Core;

public class WeatherForcastCrudViewService
{
    private IWeatherForecastDataBroker _dataBroker;
    private WeatherForcastListViewService _listService;
    public DboWeatherForecast Record { get; private set; } = new DboWeatherForecast();
    public event EventHandler? RecordChanged;

    public WeatherForcastCrudViewService(IWeatherForecastDataBroker weatherForecastDataBroker, WeatherForcastListViewService weatherForcastListViewService)
    { 
        _dataBroker = weatherForecastDataBroker;
        _listService = weatherForcastListViewService;
    }

    public async ValueTask GetRecordAsync(Guid Id)
    {
        this.Record = await _dataBroker.GetWeatherForecastAsync(Id);
        this.NotifyRecordChanged(this, EventArgs.Empty);
    }

    public async ValueTask SaveRecordAsync(DboWeatherForecast record)
    {
        await _dataBroker.SaveWeatherForecastAsync(record);
        await this.GetRecordAsync(record.Id);
    }

    public async ValueTask DeleteRecordAsync(Guid Id)
    {
        await _dataBroker.DeleteWeatherForecastAsync(Id);
        this.Record = new DboWeatherForecast();
        this.NotifyRecordChanged(this, EventArgs.Empty);
    }

    public void NotifyRecordChanged(object? sender, EventArgs e)
    {
        this.RecordChanged?.Invoke(sender, e);
        _listService.NotifyListChanged(this, EventArgs.Empty); 
    }
}
```
