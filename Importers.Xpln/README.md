# Tellurian.Trains.Schedules.Importers.Xpln

Import railway schedules from XPLN spreadsheet files (ODS format) into a validated, strongly-typed object model.

## Features

- **Native ODS reading** - Reads XPLN files directly without conversion or external dependencies
- **Complete data import** - Extracts layout, trains, schedules, vehicles, and driver duties
- **Comprehensive validation** - Two-phase validation catches data integrity issues and scheduling conflicts
- **Multi-language messages** - Error and warning messages in English, German, Danish, Norwegian, and Swedish
- **Reference data integration** - Automatic lookup of operating companies and train categories from bundled data
- **Cell styling support** - Extracts background colors from traindef rows for train category coloring
- **Flexible time parsing** - Handles multiple time formats (HH:mm, HH:mm:ss, decimal day fractions)

## Installation

```
dotnet add package Tellurian.Trains.Schedules.Importers.Xpln
```

## About XPLN

XPLN is the de facto tool within the FREMO community for creating model railway schedules and printed media for module meetings. It is developed based on *OpenOffice Calc*, with scripting and forms.

Unlike databases, spreadsheet files cannot guarantee consistent data. In XPLN, users can run macros to help achieve consistency, but any cell can be modified without automatic validation. This makes proper validation essential before using XPLN data.

## What Gets Imported

### Layout (from StationTrack worksheet)

| Type | Model Object | Description |
|------|--------------|-------------|
| `station` | `Station`, `SignalControlledLocation`, or `OtherLocation` | Name, signature, shadow/depot flag. The subtype determines the class (see below). |
| `track` | `StationTrack` | Track number, subtype (Main, Side, Siding, Depot, Goods), display order, usage notes |

#### Station SubTypes

The XPLN SubType field determines which `OperationLocation` subclass is created:

| SubType | Model Class | Description |
|---------|-------------|-------------|
| `Station` | `Station` | An operation location with a dispatcher function (either manned or delegated to locomotive driver) |
| `Block` | `SignalControlledLocation` | An unmanned location controlled by another station or automatic |
| Other values | `OtherLocation` | An unmanned location without signal control |

### Routes (from Routes worksheet)

| Model Object | Description |
|--------------|-------------|
| `TrackStretch` | Start/end stations, distance, number of tracks, speed limit, travel time |
| `TimetableStretch` | Named route groupings for timetable display |
| `DispatchStretch` | Stretch between adjacent stations that carry out train dispatch between them |

### Trains (from Trains worksheet)

| Tag | Model Object | Description |
|-----|--------------|-------------|
| `traindef` | `Train` | Number, category (extracted from *Name* and color from row background color) |
| `timetable` | `StationCall` | Station, track, arrival/departure times, remarks |
| `locomotive` | `TextCallNote` | Adds loco info as driver/station note on first call; sets train's operating company |
| `trainset` | `TextCallNote` | Adds trainset info as driver note on first call |
| `wheel` | `Train.Length` | Max train length in axles (meters not set) |
| `group` | `Train.Groups` | Train classification (e.g., *P_Zug* = Passenger, *G_Zug* = Freight or else actual value) |

### Vehicle Schedules (from Trains worksheet)

| Tag | Model Object | Description |
|-----|--------------|-------------|
| `locomotive` | `Vehicle`, `VehicleSchedule` | All `locomotive` rows with same *Object Id* are combined into one locomotive with a vehicle schedule that runs all sessions  |
| `trainset` | `VehicleSchedule` | All `trainset` rows with same *Object Id* are combined into one trainset with a vehicle schedule that runs all sessions  |
| `trainset` | `WagonGroup` | When no *object ID* but a *remark* is given, a wagon group is created |
| `trainset` | **Ignored** | When object ID and a remark are empty cells |


### Driver Duties (from Trains worksheet)

| Tag | Model Object | Description |
|-----|--------------|-------------|
| `job` | `DriverDuty` | All train parts with same *Object Id* are combined into one duty |

## Required Worksheets

The spreadsheet must contain these worksheets:

| Worksheet | Content |
|-----------|---------|
| `StationTrack` | Stations and their tracks |
| `Routes` | Track stretches between stations |
| `Trains` | Train definitions, timetables, and assignments |

## Usage

```csharp
using Tellurian.Trains.Schedules.Importers.Xpln;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;
using Tellurian.Trains.Schedules.Importers.Services;
using Microsoft.Extensions.Logging;

// Create services
var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
var companiesService = new CompaniesFromJsonService();
var categoriesService = new TrainCategoriesFromCsvService();

// Import from ODS file
var file = new FileInfo("schedule.ods");
using var importer = new XplnDataImporter(
    file,
    new OdsDataSetProvider(loggerFactory.CreateLogger<OdsDataSetProvider>()),
    companiesService,
    categoriesService,
    loggerFactory.CreateLogger<XplnDataImporter>());

var result = await importer.ImportScheduleAsync("MySchedule");

if (result.IsSuccess)
{
    var schedule = result.Item;

    // Access the imported data
    Console.WriteLine($"Locations: {schedule.Timetable.Layout.OperationLocations.Count}");
    Console.WriteLine($"Trains: {schedule.Timetable.Trains.Count}");
    Console.WriteLine($"Vehicles: {schedule.Vehicles.Count}");
    Console.WriteLine($"Driver duties: {schedule.DriverDuties.Count}");
}

// Check and display messages
if (result.Messages.HasStoppingErrors())
{
    Console.WriteLine("Import failed with errors:");
}
foreach (var message in result.Messages)
{
    Console.WriteLine(message);  // Includes severity prefix
}
```

## Validation

### Import Validation

The importer validates **referential integrity** during import, ensuring all references are valid
(stations, tracks, routes, loco schedules, etc. exist and are correctly linked).
Errors in this phase must be fixed in the XPLN file before import can succeed.

Import messages are returned in `result.Messages` with severity levels:
- **Error** - Blocks import (e.g., missing station, invalid track reference)
- **Warning** - Issues that should be reviewed
- **Information** - Progress and informational messages

```csharp
// Check if import has errors
if (result.Messages.HasStoppingErrors())
{
    // Handle errors...
}

// Display all messages (each includes severity prefix)
foreach (var message in result.Messages)
    Console.WriteLine(message);
```

### Import Message Features

- Row numbers indicate exact location of issues in the XPLN file
- Available in multiple languages (English, German, Danish, Norwegian, Swedish)

### Schedule Validation

After a successful import, you can run additional **schedule validation** to detect
timing conflicts and scheduling issues using the `ValidationError` type:

```csharp
if (result.IsSuccess)
{
    var options = new ValidationSettings
    {
        ValidateStationTracks = true,      // Track occupation conflicts
        ValidateStretches = true,          // Single-track conflicts
        ValidateVehicleSchedules = true,   // Vehicle schedule overlaps
        ValidateLocomotiveCoverage = true  // Locomotive coverage gaps
    };

    var errors = result.Item.GetValidationErrors(options);
    foreach (var error in errors)
    {
        Console.WriteLine($"{error.ErrorType}: {error.Message}");
        // error.FromTrack, error.ToTrack - conflict location
        // error.FromTime, error.ToTime - conflict time span
        // error.Trains - trains involved
    }
}
```

See the [Model README](../Model/README.md#validation) for details on `ValidationError` types and options.

## How ODS Reading Works

XPLN files are stored in ODS format (OpenDocument Spreadsheet). The importer:

1. Opens the ODS file as a ZIP archive
2. Parses the `content.xml` file using XML namespaces
3. Extracts cell values and background colors from automatic styles
4. Handles repeated rows and columns efficiently
5. Converts data to a DataSet for processing

This approach has no external dependencies beyond the standard .NET libraries.

## Testing

The importer has been tested with XPLN files from different planners and events.
All tested files had some form of data integrity issue that required correction,
demonstrating the value of automated validation for spreadsheet-based scheduling.
