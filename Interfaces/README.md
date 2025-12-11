# Tellurian.Trains.Schedules.Importers.Interfaces

Abstraction layer defining the contract for timetable import services.

## Installation

```
dotnet add package Tellurian.Trains.Schedules.Importers.Interfaces
```

## The IImportService Interface

All importers implement this interface:

```csharp
public interface IImportService
{
    ImportResult<Schedule> ImportSchedule(string name);
}
```

## The ImportResult Type

A rich result type that captures both data and validation messages:

```csharp
// Successful import
var result = ImportResult<Schedule>.Success(schedule);
var result = ImportResult<Schedule>.Success(schedule, messages);

// Failed import
var result = ImportResult<Schedule>.Failure(Message.Error("File not found"));

// Check result
if (result.IsSuccess)
{
    var schedule = result.Item;
    // Use the schedule...
}
else
{
    foreach (var message in result.Messages)
    {
        Console.WriteLine($"{message.Severity}: {message.Text}");
    }
}
```

## Implementing a Custom Importer

```csharp
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Model;

public class MyImporter : IImportService
{
    public ImportResult<Schedule> ImportSchedule(string name)
    {
        try
        {
            // Read and parse your data source
            var layout = new Layout { Name = name };
            // ... populate layout, timetable, schedule

            var timetable = new Timetable(name, layout);
            var schedule = Schedule.Create(name, timetable);

            return ImportResult<Schedule>.Success(schedule);
        }
        catch (Exception ex)
        {
            return ImportResult<Schedule>.Failure(
                Message.Error(ex.Message));
        }
    }
}
```

## JSON Serialization

Results can be serialized to JSON:

```csharp
string json = result.Json();
```
