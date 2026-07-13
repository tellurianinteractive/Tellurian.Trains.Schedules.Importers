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
| `timetable` | `StationCall` | Station, track, arrival/departure times, remarks (see *Station call conventions* below) |
| `locomotive` | `TextCallNote` | Adds loco info as driver/station note on first call; sets train's operating company |
| `trainset` | `TextCallNote` | Adds trainset info as driver note on first call |
| `wheel` | `Train.Length` | Max train length in axles (meters not set) |
| `group` | `Train.Groups` | Train classification (e.g., *P_Zug* = Passenger, *G_Zug* = Freight or else actual value) |

#### Station call conventions

The import derives each call's stop flags (`IsArrival`, `IsDeparture`) from its position and times:

- The **first** call of a train is always its origin, so it is made **departure only** (`IsArrival = false`).
- The **last** call of a train is always its terminus, so it is made **arrival only** (`IsDeparture = false`).
  The origin and terminus are always stops; when either has **equal arrival and departure times** a 10-minute
  dwell is synthesised (the origin's arrival is moved 10 minutes earlier, the terminus's departure 10 minutes
  later) so the stop has a visible duration.
- An **intermediate** call whose **arrival equals its departure** is a **pass-through**: the train passes
  without stopping, so both flags are cleared and `IsStop` becomes `false`.
- Any other intermediate call (arrival earlier than departure) is a normal stop with a dwell.

The equal-times rule is *only* an import convention. Once imported, a call is a stop when `IsStop` is true
and a pass-through when `IsStop` is false; the model never re-compares arrival and departure times to decide
this. See `StationCall.IsStop` / `StationCall.IsPassthrough`, `TrainExtensions.WithOriginAndTerminusDwell`
and `TrainExtensions.WithPassthroughCalls`.

The location type adds a further rule, independent of the call flags:

- At a **`Station`** (`Station` subtype) or an **`OtherLocation`** (other values), the train stops when the call's `IsStop` is true.
- At a **`SignalControlledLocation`** (`Block` subtype), the train **never** stops — it always passes through, whatever the call flags say.

So the effective stop is `call.IsStop && call.Station is not SignalControlledLocation`.

### Vehicle Schedules (from Trains worksheet)

| Tag | Model Object | Description |
|-----|--------------|-------------|
| `locomotive` | `Vehicle`, `VehicleSchedule` | All `locomotive` rows with same *Object Id* are combined into one locomotive with a vehicle schedule that runs all sessions  |
| `trainset` | `VehicleSchedule` | All `trainset` rows with same *Object Id* are combined into one trainset with a vehicle schedule that runs all sessions  |
| `trainset` | `CargoFlowTrainPart` | When no *object ID* but a *remark* is given, a cargo flow is created |
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
