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
| `OperationLocation` | Abstract base class for locations where trains can stop or pass. The type governs whether a train may stop: at a `Station` or `OtherLocation` it stops per the call's `IsStop`; at a `SignalControlledLocation` it never stops |
| `Station` | A manned operation location (dispatcher present). A train stops per the call's `IsStop` — to meet, be overtaken, or exchange passengers or cargo |
| `SignalControlledLocation` | An unmanned, signal-controlled location (block post or junction). A train **never** stops here; it always passes through regardless of the call's `IsStop` |
| `OtherLocation` | An unmanned location without signal control, for example a halt. A train **may** stop per the call's `IsStop` (passenger exchange only, no cargo) |
| `StationTrack` | Track within an operation location |
| `TrackStretch` | Physical connection between two operation locations with distance and track count. i.e. single or double track |
| `TimetableStretch` | A named sequence of track stretches for timetable display |
| `DispatchStretch` | A stretch between two adjacent stations where trains are dispatcheds |
| `Company` | Railway company operating trains, vehicles and/or duties |

### Timetable (Train Operations)

| Type | Description |
|------|-------------|
| `Timetable` | Holds a collection of trains running on the layout |
| `Train` | Train with calls at operation locations, category, and optional cargo flows |
| `TrainCategory` | Train type with prefix, suffix, color, and passenger/freight flags |
| `StationCall` | A train's call at an operation location track with arrival/departure times. `IsStop` (it arrives and/or departs) versus `IsPassthrough` (neither, the train passes without stopping) is the single source of truth — times are never compared to decide it. The location can override it: a train never stops at a `SignalControlledLocation`, so the effective stop is `IsStop && Station is not SignalControlledLocation` |
| `CargoFlowTrainPart` | Freight wagons a train couples at one call and uncouples at a later one, over a segment of its route, referencing a reusable `CargoFlowOptions` description |
| `Sessions` | Representing which sessions/days a train runs |

### Schedule (Resource Assignments)

| Type | Description |
|------|-------------|
| `Schedule` | Holds vehicles, vehicle schedules, and driver duties |
| `Vehicle` | Locomotive or trainset with type, number, and company |
| `VehicleSchedule` | A schedule containing train parts for locomotives and trainsets |
| `VehicleScheduleAssignment` | Links a vehicle to a vehicle schedule for specific sessions (usually all session) |
| `TrainPart` | A portion of a train between two station calls (used in vehicle schedules), default is a train part for the whole train |
| `DriverDuty` | Driver shift assignments to train parts |

`Remark` The concept with **VehicleSchedule**s containing **TrainPart**s is to make it possible to change
locomotives and wagonsets at a station between the train's first and last station call.

## Usage

```csharp
using Tellurian.Trains.Schedules.Model;

// Create a layout with stations
var layout = new Layout { Name = "MyLayout" };
var station = new Station(1, "Central", "C");
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

Validation can be configured using `ValidationSettings`:

```csharp
var options = new ValidationSettings
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
