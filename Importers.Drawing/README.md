# Tellurian.Trains.Schedules.Importers.Drawing

Reads Module Draw's drawing files, of any schema version up to the current one. Writing them is in
`Tellurian.Trains.Schedules.Exporters.Drawing`; the shape of the files (the types and their source-generated JSON
metadata) is in `Tellurian.Trains.Schedules.Model.Layouts`.

```
dotnet add package Tellurian.Trains.Schedules.Importers.Drawing
```

| Importer | Reads | Since |
|---|---|---|
| `EndProfileImporter` | `.endprofile.json` (and `.tdendprofile`) | 4.0.9 |

Modules, catalogues, halls and hall plans follow as their types move from Module Draw.

Each importer is an `IImporter<T>` (stream in, `ImportResult<T>` out) and also has a synchronous `Import(string json)`
for text already in memory, such as a draft in browser storage. An importer never throws because of what a file
contains: a failure's message has a `Code` (`FileErrorCode`: `InvalidJson`, `NotThisFormat`, `WrongKind`,
`NewerVersion`, `MigrationMissing`, `InvalidContent`) for the app to translate.

```csharp
var result = new EndProfileImporter().Import(json);
if (result.IsSuccess) Use(result.Item);
else Show(result.Messages);
```

`DrawingFileReader.ReadHeader(json, out var message)` reads what a file is (kind, schema version, app version) without
its content. `DrawingMigrations` holds the steps between schema versions.
