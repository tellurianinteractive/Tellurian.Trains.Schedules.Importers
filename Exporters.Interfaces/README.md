# Tellurian.Trains.Schedules.Exporters.Interfaces

Contracts for writing the Tellurian Trains models, the counterpart of `Tellurian.Trains.Schedules.Importers.Interfaces`.
Reading and writing are separate packages, so an app takes only the side it needs.

```
dotnet add package Tellurian.Trains.Schedules.Exporters.Interfaces
```

## IExporter&lt;T&gt;

Writes one kind of item to a stream in one format:

```csharp
public interface IExporter<T>
{
    string ResourceKey { get; }      // the app's label for the format, in every language of the app
    string FileExtension { get; }    // ".json", ".module.json"
    string MediaType { get; }        // "application/json"
    Task<ExportResult<T>> ExportAsync(T item, Stream destination, ExportOptions options, CancellationToken cancellationToken = default);
}
```

`ExportOptions` gives the app's version for the file's envelope, whether the write is a new **revision** (a save to a
file: the document's revision is counted up and an entry added to its log) or not (a draft in browser storage), who
revised it and when, and whether JSON is indented. An exporter doesn't throw because of what it writes: a failure is a
failed `ExportResult<T>`.

## IExportService

Exports a whole plan to a destination given when the service is made, e.g. a JSON file
(`Tellurian.Trains.Schedules.Exporters.Services`) or a database (`Tellurian.Trains.Schedules.Model.Databases`):

```csharp
public interface IExportService
{
    Task<ExportResult<Plan>> ExportScheduleAsync(Plan schedule);
}
```

## ExportResult&lt;T&gt;

The same shape as `ImportResult<T>`: `Item` (what was written), `Messages` (`Message[]`, each with a severity and an
optional `Code` for a translated text), `IsSuccess` and `IsFailure`.

```csharp
var result = ExportResult<Plan>.Success(plan);
var failed = ExportResult<Plan>.Failure(Message.Error("The disk is full.") with { Code = "WriteFailed" });
```
