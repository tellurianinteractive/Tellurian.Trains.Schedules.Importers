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

## Implementing a layout importer

A layout made by another tool is taken into the planner through `ILayoutImporter`. It reads a `Layout` only; the
planner makes a new plan of it (`PlanFactory.CreatePlan(layout, language)`), or, when the open plan was made from the
same source (the same `Layout.SourceId`), takes it in as a newer revision (`plan.UpdateFromHallPlan(layout)`): distances
and times at once, other changes for the planner to confirm.

```csharp
public sealed class MyLayoutImporter : ILayoutImporter
{
    // A label of the app, added in every language of the app together with the importer.
    public string ResourceKey => "MyLayoutTool";
    public IReadOnlyList<string> FileExtensions { get; } = [".mylayout"];

    // Quick, and never throws: several formats may share an extension.
    public bool CanImport(string fileName, ReadOnlySpan<byte> content) => content.StartsWith("MYLAYOUT"u8);

    public async Task<ImportResult<Layout>> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        var layout = new Layout { Name = "...", SourceId = /* the same for every revision */, SourceRevision = 1 };
        // ... locations with their tracks, stretches between them; set each location's Layout, each track's Station.
        return ImportResult<Layout>.Success(layout);
    }
}
```

Register it in the app, beside the built-in Module Draw importer, and add its `ResourceKey` to the app's labels in
every language; the Import tab then accepts its files:

```csharp
services.AddLayoutImporter<MyLayoutImporter>();
```

For a revision to be compared with the layout already in a plan, keep `SourceId` the same from one revision to the
next, raise `SourceRevision`, and keep the ids (or external references) of locations that stay. Instead of writing an
importer, a tool can also write the layout block of a hall plan, a `Layout` in the JSON of
`Layout.JsonSerializerOptions`, on its own in a `.json` file: the built-in importer reads it.

## JSON Serialization

Import results can be serialized to JSON:

```csharp
string json = result.Json();

// Or write to temp file
result.Write();
```
