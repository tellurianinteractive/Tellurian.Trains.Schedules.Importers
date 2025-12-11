# Tellurian.Trains.Schedules.Importers.Xpln

Import railway schedules from XPLN spreadsheet files (ODS format).

## About XPLN
XPLN is the defacto tool within the FREMO community to
create model railway schedules and printed media for module meetings.
It is developed based on *OpenOffice Calc*, with scripting and forms.
With this tool most aspects of model railway scheduling can be made.

Unlike databases, spreadsheet files like XPLN-files cannot guarantee consistent data.
In XPLN, a user has the option to run macros to help achieve consistency,
but any cell can be modified without automatic check of consistence.

Because it lacks the data integrity of a real database, it requires users to
follow a strict workflow to not end up with inconsistent data.

Therefore it is essential that XPLN-documents can be read and validated for formal data consistency
before they can be imported into a database.

## Installation

```
dotnet add package Tellurian.Trains.Schedules.Importers.Xpln
```

## File Format

XPLN saves schedules as **ODS** (OpenDocument Spreadsheet) files.

## Required Worksheets

The spreadsheet must contain these worksheets:

| Worksheet | Content |
|-----------|---------|
| `StationTrack` | Stations and their tracks |
| `Routes` | Track stretches between stations |
| `Trains` | Train definitions, timetables, and assignments to loco schedules, trainsets and duties|

## Usage

```csharp
using Tellurian.Trains.Schedules.Importers.Xpln;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;
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
1. **Referential integrity** - verifies that all references between objects are valid
   (stations, tracks, routes, loco schedules, etc. exist and are correctly linked).
   Errors in this phase must be fixed in the XPLN file before import can succeed.
2. **Scheduling conflicts** - checks for timing issues, track conflicts, and sequence errors.
   These are reported as warnings/information messages.

A successful import (`result.IsSuccess == true`) means the data is consistent and can be used.
However, there may still be warning messages indicating potential scheduling issues worth reviewing:

```csharp
// Check for warnings after successful import
foreach (var msg in result.Messages.Where(m => m.Severity == Severity.Information))
    Console.WriteLine(msg);
```

See the [Model README](../Model/README.md#validation) for details on which scheduling checks are performed.

Error messages include row numbers and are available in multiple languages (English, German, Danish, Norwegian, Swedish).

## What Gets Imported

- Track layout (stations, tracks, routes)
- Train timetables with station calls
- Locomotive assignments
- Trainset assignments
- Driver duties

The *wheel* and *group* tags are currently not read.
- *Wheels* denotes train length in axles. It will be added in a forthcoming release.
- *Group* is only for internal purposes in XPLN.

## The Story of Reading XPLN Files
XPLN is stored in ODS-files, an *Open Document* format.
Despite it is open, it is tricky to read.
- First, I used Excel COM-objects, which make reading dependent having Microsoft Excel installed.
Excel can open ODS-files directly. This was easiest to start with, but not a solution that can run in the cloud or be distributed.
- In the second effort, I found the **ExcelDataReader** package, that removed the dependency of Microsoft Excel.
But it required the ODS-files to be converted to .XLSX before reading.
Although there are free online converters, it forces the user to make a conversion.
- Finally, I found a 10+ year old codebase for reading ODS-files directly.
After some tweaking, I made it work. And it had much better performance than the initial Excel-solution.

## How is this Software Tested?
Not two planners use XPLN exactly the same way, validating one XPLN-file isn't enough.
The only way to test is to have a large number of XPLN-files to read and validate.
Therefore, the tests of this software use a set of XPLN files from different origins.

All of the tested XPLN-files had some kind of data integrity issue that required correction of the XPLN-file
before it could be successfully validated.
This clearly demonstrates the problems with using a spreadsheet for complex data storage.
