# Tellurian.Trains.Schedules.Importers.Model

Core domain model for railway timetable planning and scheduling.

## Key Types

| Type | Description |
|------|-------------|
| `Schedule` | Complete schedule with timetable, loco schedules, trainset schedules, and driver duties |
| `Timetable` | Collection of trains within a track layout |
| `Train` | Individual train with station calls and category |
| `Layout` | Physical track layout with stations and stretches |
| `Station` | Railway station with tracks |
| `StationTrack` | Track within a station |
| `StationCall` | Scheduled stop with arrival/departure times |
| `TrackStretch` | Connection between two stations |
| `LocoSchedule` / `TrainsetSchedule` | Equipment assignments |
| `DriverDuty` | Driver shift assignments |

## Installation

```
dotnet add package Tellurian.Trains.Schedules.Importers.Model
```

## Usage

```csharp
using Tellurian.Trains.Schedules.Importers.Model;

// Create a layout with stations
var layout = new Layout { Name = "MyLayout" };
var station = new Station { Name = "Central", Signature = "C" };
station.Add(new StationTrack("1") { IsMain = true });
layout.Add(station);

// Create a timetable with trains
var timetable = new Timetable("Morning", layout);
var train = new Train(101, "P101") { Category = "Passenger" };
train.Add(new StationCall(station.Tracks.First(),
    arrival: new Time(8, 0),
    departure: new Time(8, 5)));
timetable.Add(train);

// Create a complete schedule
var schedule = Schedule.Create("Schedule2024", timetable);
```

## Validation

The model includes comprehensive validation to detect scheduling conflicts and data inconsistencies.
Validation is performed after data has been successfully imported with referential integrity intact.

### Validation Options

Validation can be configured using `ValidationOptions`:

```csharp
var options = new ValidationOptions
{
    ValidateStationCalls = true,      // Check arrival/departure times
    ValidateStationTracks = true,     // Check track occupation conflicts
    ValidateStretches = true,         // Check single-track conflicts
    ValidateTrainSpeed = true,        // Check speed limits
    ValidateLocoSchedules = true,     // Check loco assignment overlaps
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
| **Loco schedules** | Detects overlapping train parts assigned to the same locomotive |

### Message Severity

Validation results are returned as `Message` objects with severity levels:

```csharp
Message.Error("...");       // Critical errors preventing import
Message.Warning("...");     // Issues that should be reviewed
Message.Information("..."); // Scheduling conflicts and warnings
```

After a successful import (no errors), there may still be information messages
indicating potential scheduling issues that are non-critical but worth reviewing.
