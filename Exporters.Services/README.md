# Tellurian.Trains.Schedules.Exporters.Services

Writes plans as plan files, the JSON the planner opens and Tellurian Dispatch reads (`PlanFile`, with its envelope and
schema version).

```
dotnet add package Tellurian.Trains.Schedules.Exporters.Services
```

## PlanJsonExporter

An `IExporter<Plan>` that writes to a stream:

```csharp
var exporter = new PlanJsonExporter();
await using var stream = File.Create("plan.json");
var result = await exporter.ExportAsync(plan, stream, ExportOptions.Default);
```

## JsonExportService

An `IExportService` that writes a whole plan to a file given when it is made:

```csharp
var service = new JsonExportService(new FileInfo("plan.json"));
var result = await service.ExportScheduleAsync(plan);
if (result.IsFailure) Console.WriteLine(string.Join(Environment.NewLine, result.Messages));
```

Reading plan files is in `Tellurian.Trains.Schedules.Importers.Services` (`JsonImportService`).
