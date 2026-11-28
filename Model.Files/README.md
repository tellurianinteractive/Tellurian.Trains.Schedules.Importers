# Tellurian.Trains.Schedules.Model.Files

The envelope every Tellurian Trains file is written in, and the chain of migrations that brings an older file up to date.

```json
{ "format": "tellurian-plan", "schemaVersion": 7, "appVersion": "1.8.0", "plan": { ... } }
```

- `FileKind` says what a kind of file is: its format, its kind, the property that holds the content, the schema version written and the oldest one read, and the migration steps between them.
- `FileEnvelope` reads and writes the envelope, and runs the migrations (`FileMigration`, one step per version).
- `FileFormatException` carries what went wrong as a `FileError`.

Plan files (`Tellurian.Trains.Schedules.Model`) and drawing files (`Tellurian.Trains.Schedules.Model.Drawings`) both use it.
