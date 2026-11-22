# Tellurian.Trains.Schedules.Exporters.Drawing

Writes Module Draw's drawing files at the current schema version. Reading them is in
`Tellurian.Trains.Schedules.Importers.Drawing`; the shape of the files is in `Tellurian.Trains.Schedules.Model.Layouts`.

```
dotnet add package Tellurian.Trains.Schedules.Exporters.Drawing
```

| Exporter | Writes | Since |
|---|---|---|
| `EndProfileExporter` | `.endprofile.json` | 4.0.9 |
| `CatalogueExporter` | `.catalogue.json` | 4.0.18 |
| `HallExporter` | `.hall.json` | 4.0.19 |
| `ModuleExporter` | `.module.json` | 4.0.22 |
| `HallPlanExporter` | `.hallplan.json`, with the layout as `content.layout` in its own form, as `HallPlanFile.ReadLayout` reads it | 4.0.26 |

Each exporter is an `IExporter<T>` (item and stream in, `ExportResult<T>` out) and also has a synchronous
`Write(item, options)` that returns the text. `ExportOptions` says the app's version for the envelope and whether the
write is a **new revision**: a save to a file counts up `Revision` and records who revised it, and when, in
`RevisionLog`; a draft kept in browser storage is written as it is. The item given is never changed: the result holds
what was written.

```csharp
var exporter = new EndProfileExporter();
var options = new ExportOptions(AppVersion: "0.10.0", IsRevision: true, RevisedBy: "Stefan");
await using var stream = File.Create($"B96{exporter.FileExtension}");
var result = await exporter.ExportAsync(profile, stream, options);
var saved = result.Item;   // revision counted up, log entry added
```
