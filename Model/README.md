# Tellurian.Trains.Schedules.Model

Core domain model for railway timetable planning and scheduling.

## Installation

```
dotnet add package Tellurian.Trains.Schedules.Model
```

## Key Types

### Layout (Physical Infrastructure)

| Type | Description |
|------|-------------|
| `Layout` | Physical track layout with stations, companies, and stretches |
| `OperationLocation` | Manned station, junction, signal controlled location or other location |
| `StationTrack` | Track within an operation location |
| `TrackStretch` | Physical connection between two stations with distance and number of parallel tracks |
| `TimetableStretch` | A named sequence of track stretches for timetable display |
| `Company` | Railway company operating trains, vehicles and/or duties |

### Timetable (Train Operations)

| Type | Description |
|------|-------------|
| `Timetable` | Trains within a track layout |
| `Train` | Train with calls at operation locations, category, and optional wagon groups |
| `TrainCategory` | Train type with prefix, suffix, color, and passenger/freight flags |
| `StationCall` | Scheduled stop at an operation locattion at a specific track with arrival/departure times |
| `WagonGroup` | A group of (usually) freight wagons within a train, that runs part of or whole train, and are often ordered within the train |
| `Sessions` | Representing which sessions/days a train runs |

### Schedule (Resource Assignments)

| Type | Description |
|------|-------------|
| `Schedule` | Holds vehicles, vehicle schedules, and driver duties |
| `Vehicle` | Locomotive or trainset with type, number, and company |
| `VehicleSchedule` | A schedule containing train parts for locomotives and trainsets (not wagon groups) |
| `VehicleScheduleAssignment` | Links a vehicle to a vehicle schedule for specific sessions (usually all session) |
| `TrainPart` | A portion of a train between two station calls (used in vehicle schedules), default is a train part for the whole train |
| `DriverDuty` | Driver shift assignments to train parts |

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

var errors = schedule.GetValidationErrors(options);
```

### ValidationError

Validation returns `ValidationError` objects containing location and time information for highlighting conflicts in a graphical timetable:

```csharp
foreach (var error in errors)
{
    Console.WriteLine(error.Message);           // Localized message
    Console.WriteLine(error.ErrorType);         // Error category
    Console.WriteLine(error.Trains);            // Trains involved
    Console.WriteLine(error.FromTrack);         // Conflict start location
    Console.WriteLine(error.ToTrack);           // Conflict end location
    Console.WriteLine(error.FromTime.HHMM());   // Conflict start time
    Console.WriteLine(error.ToTime.HHMM());     // Conflict end time
}

// Check conflict type
if (error.IsStationConflict)  // FromTrack == ToTrack
    // Conflict is at a single station track
if (error.IsStretchConflict)  // FromTrack != ToTrack
    // Conflict spans a track stretch between stations
```

### Validation Error Types

| ErrorType | Description |
|-----------|-------------|
| `MissingTrackReference` | Station track is referenced but not in layout |
| `StationCallTiming` | Station call has arrival after departure |
| `StationTrackConflict` | Two trains conflict on the same station track |
| `TrackStretchConflict` | Two trains conflict on a track stretch |
| `TrainTimeSequence` | Train calls are not in correct time sequence |
| `TrainSpeedTooSlow` | Train speed is too slow between calls |
| `TrainSpeedTooFast` | Train speed is too fast between calls |
| `TrainTooFewCalls` | Train must have at least two station calls |
| `VehicleScheduleOverlap` | Vehicle schedule has overlapping train parts |
| `LocomotiveCoverageGap` | Train has a gap in locomotive coverage |
| `LocomotiveCoverageOverlap` | Train has overlapping locomotive assignments |
| `VehicleDoubleBooked` | Vehicle has overlapping schedule assignments |

### Validation Checks by Option

| Option | Checks Performed |
|--------|------------------|
| `ValidateStationCalls` | `StationCallTiming` |
| `ValidateStationTracks` | `StationTrackConflict` |
| `ValidateStretches` | `TrackStretchConflict` |
| `ValidateTrainSpeed` | `TrainSpeedTooSlow`, `TrainSpeedTooFast` |
| `ValidateVehicleSchedules` | `VehicleScheduleOverlap`, `VehicleDoubleBooked` |
| `ValidateLocomotiveCoverage` | `LocomotiveCoverageGap`, `LocomotiveCoverageOverlap` |

Note: `MissingTrackReference`, `TrainTimeSequence`, and `TrainTooFewCalls` are always checked.

## Localization

Validation messages are available in:
- English (default)
- German (de)
- Danish (da)
- Norwegian (nb)
- Swedish (sv)
