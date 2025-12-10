# TimetablePlanning.Importers.Access

Import and export railway schedules from Microsoft Access databases via ODBC.

> **Note:** This package is experimental. Layout import/export works; train import is incomplete.

## Installation

```
dotnet add package TimetablePlanning.Importers.Access
```

## Requirements

- Microsoft Access Database Engine (ODBC driver)
- Windows platform

## Usage

### Import a Layout

```csharp
using TimetablePlanning.Importers.Access;
using Microsoft.Extensions.Logging;

var logger = LoggerFactory.Create(b => b.AddConsole())
    .CreateLogger<AccessRepository>();

var dbFile = new FileInfo("timetable.accdb");
var repository = new AccessRepository(dbFile, logger);

var result = repository.ImportSchedule("LayoutName");

if (result.IsSuccess)
{
    var schedule = result.Item;
    var layout = schedule.Timetable.Layout;

    Console.WriteLine($"Stations: {layout.Stations.Count}");
    Console.WriteLine($"Track stretches: {layout.TrackStretches.Count}");
}
```

### Save a Layout

```csharp
var layout = new Layout { Name = "NewLayout" };
// ... populate stations, tracks, stretches

var saveResult = repository.Save(layout);
if (saveResult.IsFailure)
{
    Console.WriteLine("Save failed: " + saveResult.Messages.First());
}
```

### Save a Timetable

```csharp
var timetable = new Timetable("Summer2024", layout);
// ... add trains

var saveResult = repository.Save(timetable);
```

## Database Schema

The Access database must contain these tables:
- `Layout` - Track layout definitions
- `Station` - Station records
- `StationTrack` - Track records per station
- `TrackStretch` - Connections between stations
- `TimetableStretch` - Route definitions
- `Train` - Train definitions
- `TrainStationCall` - Station calls per train
