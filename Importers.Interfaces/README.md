# Tellurian.Trains.Schedules.Importers.Interfaces

Contracts for schedule import/export services and supporting data services.

## Installation

```
dotnet add package Tellurian.Trains.Schedules.Importers.Interfaces
```

## Interfaces

### IImportService

The main interface for importing schedules from external sources:

```csharp
public interface IImportService
{
    Task<ImportResult<Schedule>> ImportScheduleAsync(string name);
}
```

### IExportService

Interface for exporting schedules to external formats:

```csharp
public interface IExportService
{
    Task<ExportResult<Schedule>> ExportScheduleAsync(Schedule schedule);
}
```

### ICompaniesService

Service for retrieving railway company data:

```csharp
public interface ICompaniesService
{
    Task<IEnumerable<Company>> GetAllCompaniesAsync();
}
```

### ITrainCategoriesService

Service for retrieving train category definitions:

```csharp
public interface ITrainCategoriesService
{
    Task<IEnumerable<TrainCategory>> GetAllTrainCategoriesAsync();
}
```

## Result Types

### ImportResult&lt;T&gt;

A rich result type that captures both data and validation messages:

```csharp
// Successful import
var result = ImportResult<Schedule>.Success(schedule);
var result = ImportResult<Schedule>.Success(schedule, messages);

// Failed import
var result = ImportResult<Schedule>.Failure(Message.Error("File not found"));

// Success only if no error messages
var result = ImportResult<Schedule>.SuccessIfNoErrorMessagesOtherwiseFailure(schedule, messages);

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

### ExportResult&lt;T&gt;

A simpler result type for export operations:

```csharp
// Successful export
var result = ExportResult<Schedule>.Success(schedule);

// Failed export
var result = ExportResult<Schedule>.Failure("Export failed", "Reason...");

if (result.IsSuccess)
{
    // Export succeeded
}
```

## Implementing a Custom Importer

```csharp
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

public class MyImporter : IImportService
{
    public async Task<ImportResult<Schedule>> ImportScheduleAsync(string name)
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

Import results can be serialized to JSON:

```csharp
string json = result.Json();

// Or write to temp file
result.Write();
```
