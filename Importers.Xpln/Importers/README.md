# TimetablePlanning.Importers.Xpln

Import railway schedules from XPLN spreadsheet files (ODS/XLSX format).

XPLN is a timetable planning tool used in the model railway community, particularly for FREMO modular layouts.

## Installation

```
dotnet add package TimetablePlanning.Importers.Xpln
```

## File Format

XPLN exports schedules as **ODS** (OpenDocument Spreadsheet) files.

## Required Worksheets

The spreadsheet must contain these worksheets:

| Worksheet | Content |
|-----------|---------|
| `StationTrack` | Stations and their tracks |
| `Routes` | Track stretches between stations |
| `Trains` | Train definitions, timetables, and assignments |

## Usage

```csharp
using TimetablePlanning.Importers.Xpln;
using TimetablePlanning.Importers.Xpln.DataSetProviders;
using Microsoft.Extensions.Logging;

// Create logger (or use ILoggerFactory)
var logger = LoggerFactory.Create(b => b.AddConsole())
    .CreateLogger<XplnDataImporter>();

// Import from ODS file
var file = new FileInfo("schedule.ods");
using var importer = new XplnDataImporter(
    file,
    new OdsDataSetProvider(),
    logger);

var result = importer.ImportSchedule("MySchedule");

if (result.IsSuccess)
{
    var schedule = result.Item;

    // Access the data
    Console.WriteLine($"Stations: {schedule.Timetable.Layout.Stations.Count}");
    Console.WriteLine($"Trains: {schedule.Timetable.Trains.Count}");
    Console.WriteLine($"Loco schedules: {schedule.LocoSchedules.Count}");
    Console.WriteLine($"Driver duties: {schedule.DriverDuties.Count}");
}
else
{
    foreach (var msg in result.Messages.Where(m => m.Severity >= Severity.Error))
        Console.WriteLine(msg);
}
```

## Validation

The importer performs two-phase validation:
1. **Referential integrity** - stations, tracks, and routes exist
2. **Scheduling conflicts** - timing and sequence errors

Error messages include row numbers and are available in multiple languages (English, German, Danish, Norwegian, Swedish).

## What Gets Imported

- Track layout (stations, tracks, routes)
- Train timetables with station calls
- Locomotive assignments
- Trainset assignments
- Driver duties
