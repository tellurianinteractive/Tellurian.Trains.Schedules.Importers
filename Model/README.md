# Tellurian.Trains.Schedules.Model

Core domain model for railway timetable planning and scheduling.

## Installation

```
dotnet add package Tellurian.Trains.Schedules.Model
```

## Key Types

### Schedule & Timetable

| Type | Description |
|------|-------------|
| `Schedule` | Complete schedule with timetable, vehicles, vehicle schedules, and driver duties |
| `Timetable` | Collection of trains within a track layout |
| `Train` | Individual train with station calls and category |
| `TrainPart` | A portion of a train between two station calls (used in vehicle schedules) |
| `TrainCategory` | Train type with prefix, suffix, color, and passenger/freight flags |
| `Sessions` | Bit flags representing which sessions/days a train runs |

### Layout & Infrastructure

| Type | Description |
|------|-------------|
| `Layout` | Physical track layout with stations, companies, and stretches |
| `OperationLocation` | Railway station or stop with tracks (formerly Station) |
| `StationTrack` | Track within a station |
| `StationCall` | Scheduled stop with arrival/departure times |
| `TrackStretch` | Physical connection between two stations with distance and track count |
| `TimetableStretch` | A named sequence of track stretches for timetable display |
| `Company` | Railway company operating vehicles |

### Vehicles & Assignments

| Type | Description |
|------|-------------|
| `Vehicle` | Locomotive or trainset with type, number, and company |
| `VehicleSchedule` | A schedule containing train parts that a vehicle runs |
| `VehicleScheduleAssignment` | Links a vehicle to a vehicle schedule for specific sessions |
| `DriverDuty` | Driver shift assignments |

## Usage

```csharp
using Tellurian.Trains.Schedules.Model;

// Create a layout with stations
var layout = new Layout { Name = "MyLayout" };
var station = new OperationLocation(1, "Central", "C");
station.Add(new StationTrack("1") { IsMain = true });
layout.Add(station);

// Create a timetable with trains
var timetable = new Timetable("Morning", layout);
var train = new Train(101, "P101") { Category = passengerCategory };
train.Add(new StationCall(station.Tracks.First(),
    arrival: new Time(8, 0),
    departure: new Time(8, 5)));
timetable.Add(train);

// Create a complete schedule
var schedule = Schedule.Create("Schedule2024", timetable);

// Add vehicles and assignments
var loco = new Vehicle(1, VehicleType.Locomotive, 1234);
schedule.AddVehicle(loco);

var vehicleSchedule = new VehicleSchedule(1);
schedule.AddVehicleSchedule(vehicleSchedule);
vehicleSchedule.Add(train.AsTrainPart(0, 1)); // First to last call

var assignment = new VehicleScheduleAssignment(1, loco, vehicleSchedule);
loco.ScheduleAssignments.Add(assignment);
```

## Validation

The model includes comprehensive validation to detect scheduling conflicts and data inconsistencies.
Validation is performed after data has been successfully imported with referential integrity intact.

For detailed validation documentation, see [VALIDATION.md](../VALIDATION.md).

### Validation Options

Validation can be configured using `ValidationOptions`:

```csharp
var options = new ValidationOptions
{
    ValidateStationCalls = true,       // Check arrival/departure times
    ValidateStationTracks = true,      // Check track occupation conflicts
    ValidateStretches = true,          // Check single-track conflicts
    ValidateTrainSpeed = true,         // Check speed limits
    ValidateVehicleSchedules = true,   // Check vehicle schedule overlaps
    ValidateLocomotiveCoverage = true, // Check locomotive assignments cover entire train
    MinTrainSpeedMetersPerClockMinute = 0.3,
    MaxTrainSpeedMetersPerClockMinute = 10
};

var messages = schedule.GetValidationErrors(options);
```

### Validation Checks

| Check | Description |
|-------|-------------|
| **Station calls** | Arrival time must be before or equal to departure time |
| **Station tracks** | Detects conflicts where different trains occupy the same track at overlapping times |
| **Track stretches** | Detects conflicts on single-track sections where trains would collide |
| **Train time sequence** | Ensures calls within a train are in chronological order |
| **Train speed** | Warns if train speed between stations is too slow or too fast |
| **Vehicle schedules** | Detects overlapping train parts assigned to the same vehicle schedule |
| **Locomotive coverage** | Ensures every train has complete locomotive coverage without gaps |
| **Vehicle double booking** | Detects vehicles assigned to overlapping schedules in same sessions |

### Message Severity

Validation results are returned as `Message` objects with severity levels:

```csharp
Message.Error("...");       // Critical errors preventing import
Message.Warning("...");     // Issues that should be reviewed
Message.Information("..."); // Scheduling conflicts and warnings
```

After a successful import (no errors), there may still be information messages
indicating potential scheduling issues that are non-critical but worth reviewing.

## Localization

Validation messages are available in:
- English (default)
- German (de)
- Danish (da)
- Norwegian (nb)
- Swedish (sv)
