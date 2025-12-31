# Validation System Overview

This document describes the validation system in the Tellurian.Trains.Schedules.Importers solution.

## Architecture

The validation system is located in `Model/Validations/` and consists of three files:

| File | Purpose |
|------|---------|
| `ValidationOptions.cs` | Configuration class for controlling which validations run |
| `ValidationExtensions.cs` | Core validation logic as extension methods |
| `ValidationError.cs` | Structured error type with location and time for graphical display |

## ValidationError

A structured error type that captures location and time information for graphical timetable highlighting:

```csharp
public sealed record ValidationError
{
    public required ValidationErrorType ErrorType { get; init; }
    public required Time FromTime { get; init; }      // Min departure time of conflict
    public required Time ToTime { get; init; }        // Max arrival time of conflict
    public required StationTrack FromTrack { get; init; }  // Start location
    public required StationTrack ToTrack { get; init; }    // End location
    public required IReadOnlyList<Train> Trains { get; init; }
    public required Message Message { get; init; }

    public bool IsStationConflict => FromTrack.Equals(ToTrack);
    public bool IsStretchConflict => !FromTrack.Equals(ToTrack);
}
```

### Location Semantics
- **Same track** (`FromTrack == ToTrack`): Conflict is at a station on that track
- **Different tracks** (`FromTrack != ToTrack`): Conflict spans the track stretch between them

### Error Types
| Type | Description |
|------|-------------|
| `MissingTrackReference` | Station track is referenced but not in layout |
| `StationTrackConflict` | Two trains conflict on the same station track |
| `StationCallTiming` | Station call has arrival after departure |
| `TrackStretchConflict` | Two trains conflict on a track stretch |
| `TrainTimeSequence` | Train calls are not in correct time sequence |
| `TrainSpeedTooSlow` | Train speed is too slow between calls |
| `TrainSpeedTooFast` | Train speed is too fast between calls |
| `TrainTooFewCalls` | Train must have at least two station calls |
| `VehicleScheduleOverlap` | Vehicle schedule has overlapping train parts |
| `LocomotiveCoverageGap` | Train has a gap in locomotive coverage |
| `LocomotiveCoverageOverlap` | Train has overlapping locomotive assignments |
| `VehicleDoubleBooked` | Vehicle has overlapping schedule assignments |

## ValidationOptions

Controls which validations are performed and threshold values:

```csharp
public class ValidationOptions
{
    // Toggle flags (all default to true)
    public bool ValidateStationCalls { get; set; } = true;
    public bool ValidateStationTracks { get; set; } = true;
    public bool ValidateStretches { get; set; } = true;
    public bool ValidateTrainSpeed { get; set; } = true;
    public bool ValidateTrainNumbers { get; set; } = true;      // Not implemented
    public bool ValidateVehicleSchedules { get; set; } = true;
    public bool ValidateLocomotiveCoverage { get; set; } = true;
    public bool ValidateDriverDuties { get; set; } = true;      // Not implemented

    // Threshold values
    public double MinTrainSpeedMetersPerClockMinute { get; set; } = 0.3;
    public double MaxTrainSpeedMetersPerClockMinute { get; set; } = 10;
    public int MinMinutesBetweenTrackUsage { get; set; }         // Not implemented
}
```

## Two-Phase Validation Strategy

1. **Import Phase** - Validates data integrity during XPLN/Access reading
   - Produces Error/Warning/System severity messages
   - Import fails on critical errors

2. **Post-Import Phase** - Validates scheduling conflicts and operational constraints
   - Called by user code via `GetValidationErrors(options)`
   - Produces Information severity messages
   - Does not affect import success/failure

## Validation Entry Points

### Schedule Validation (Top-Level)
```csharp
IEnumerable<Message> GetValidationErrors(this Schedule schedule, ValidationOptions options)
```
- Entry point for validating entire schedule
- Validates timetable and vehicle schedules

### Timetable Validation
```csharp
IEnumerable<Message> GetValidationErrors(this Timetable timetable, Schedule schedule, ValidationOptions options)
```
- Comprehensive timetable validation
- Calls sub-validators based on options

## Entities Validated

### 1. Station Tracks (Referential Integrity)
**Method**: `EnsureStationHasTrack(this Timetable me)`

**Validates**: All station tracks referenced in trains exist in the layout

**Error**: `"Track {track} in station {station} referred in train {train} is not in layout."`

### 2. Station Track Conflicts
**Method**: `GetValidationErrors(this StationTrack me, IEnumerable<VehicleSchedule> vehicleSchedules)`

**Validates**: No conflicting train calls on the same track

**Logic**:
- Detects when different trains occupy the same track with overlapping times
- Exception: Trains sharing the same vehicle are allowed (e.g., loco changes)

**Error**: `"Train {train1} {time1} has conflicts with train {train2} {time2}."`

### 3. Station Calls
**Method**: `GetValidationErrors(this StationCall stationCall)`

**Validates**: Arrival time ≤ Departure time at same station

**Error**: `"At {station} arrival {arrival} is after departure {departure}."`

### 4. Track Stretch Conflicts
**Method**: `GetValidationErrors(this TrackStretch me)`

**Validates**: No conflicting trains on the same stretch segment

**Logic**:
- Analyzes passings sorted by departure time
- Considers number of tracks on stretch
- Detects overlapping train movements

**Error**: `"Train {train1} between {stretch1} is conflicting with train {train2} between {stretch2}."`

### 5. Train Time Sequence
**Method**: `CheckTrainTimeSequence(this Train me)`

**Validates**:
- Train has minimum 2 station calls
- Calls are in correct time sequence

**Checks for conflicts**:
- Arrival after departure at next station
- Arrival after another arrival
- Departure before station arrival
- Departure before departure

**Errors**:
- `"Train {train} must stop at at least two stations."`
- `"Train {train} calls {call1} and {call2} are conflicting."`

### 6. Train Speed
**Method**: `CheckTrainSpeed(this Train me, ...)`

**Validates**: Train speed is within realistic bounds

**Formula**: `speed = distance / timeInMinutes`

**Checks**:
- Speed ≥ `MinTrainSpeedMetersPerClockMinute`
- Speed ≤ `MaxTrainSpeedMetersPerClockMinute`

**Errors**:
- `"Train {train} speed from {station1} {time1} to {station2} {time2} is too slow, length {distance} meters."`
- `"Train {train} speed from {station1} {time1} to {station2} {time2} is too fast, length {distance} meters."`

### 7. Vehicle Schedule Overlaps
**Method**: `ValidateOverlappingParts(this VehicleSchedule me)`

**Validates**: Vehicle schedule has no overlapping train parts

**Logic**: Compares all train part pairs in a vehicle schedule for time overlaps

**Error**: `"Vehicle schedule {id} contains overlapping {trainPart1} and {trainPart2}."`

### 8. Locomotive Coverage
**Method**: `ValidateLocomotiveCoverage(this Schedule schedule)`

**Validates**: Every train has complete locomotive coverage without gaps or overlaps

**Logic**:
1. Gets all locomotive vehicle schedules (via VehicleScheduleAssignments from Locomotives)
2. For each train, collects all train parts assigned to locomotives
3. Checks for gaps between parts (uncovered segments)
4. Checks for overlaps between parts (double-booked locomotives)

**Exception**: Locomotive changes at the same station are allowed. If one locomotive part ends and another begins at the same station (even with a time gap for the changeover), this is not reported as a coverage gap.

**Errors**:
- `"Train {0} has no locomotive assigned."` - No locomotive at all
- `"Train {0} has a locomotive coverage gap between {1} and {2}."` - Gap in coverage between different stations
- `"Train {0} has overlapping locomotive assignments: {1} and {2}."` - Overlap detected

### 9. Vehicle Double Booking
**Method**: `ValidateVehicleDoubleBooking(this Schedule schedule)`

**Validates**: No vehicle has overlapping schedule assignments (sessions)

**Logic**:
1. For each vehicle, gets all VehicleScheduleAssignments
2. Compares each pair of assignments for overlapping Sessions
3. Uses bitwise AND on session flags to detect overlap

**Error**: `"Vehicle {0} is double-booked: sessions {1} overlap with sessions {2}."`

## Not Yet Implemented

The following options exist but validation logic is not implemented:

| Option | Status |
|--------|--------|
| `ValidateTrainNumbers` | Property exists, not used |
| `ValidateDriverDuties` | Property exists, validation not implemented |
| `MinMinutesBetweenTrackUsage` | Property exists, not used |

## Severity Levels

```csharp
public enum Severity
{
    None = 0,
    Information = 1,
    Warning = 2,
    Error = 3,
    System = 4
}
```

**Note**: All post-import validation messages use `Message.Information(...)` severity.

## Resource Strings (Multi-Language)

All validation messages use resource strings in `Model/Resources/Strings.resx` with translations for:
- English (default)
- German (de)
- Danish (da)
- Norwegian (no)
- Swedish (sv)

## Usage Example

```csharp
// After importing
var result = await importer.ImportScheduleAsync(scheduleName);
if (result.IsSuccess)
{
    var options = new ValidationOptions
    {
        MaxTrainSpeedMetersPerClockMinute = 8.0,
        MinTrainSpeedMetersPerClockMinute = 0.3,
        ValidateDriverDuties = true,
        ValidateVehicleSchedules = true,
        // ... other options
    };

    var validationErrors = result.Item.GetValidationErrors(options);
    foreach (var error in validationErrors)
    {
        Console.WriteLine(error);
    }
}
```

## Validation Flow

```
Schedule.GetValidationErrors(options)
  │
  ├─► Timetable.GetValidationErrors(schedule, options)
  │     │
  │     ├─► EnsureStationHasTrack()           [always]
  │     ├─► StationCall.GetValidationErrors() [if ValidateStationCalls]
  │     ├─► StationTrack.GetValidationErrors()[if ValidateStationTracks]
  │     ├─► TrackStretch.GetValidationErrors()[if ValidateStretches]
  │     ├─► CheckTrainSpeed()                 [if ValidateTrainSpeed]
  │     └─► CheckTrainTimeSequence()          [always]
  │
  └─► VehicleSchedule.ValidateOverlappingParts() [if ValidateVehicleSchedules]
```

## Known Issues / TODOs

1. **ValidateTrainNumbers** - Option exists but no implementation
2. **ValidateDriverDuties** - Option exists but no implementation
3. **MinMinutesBetweenTrackUsage** - Parameter exists but not used
4. **Vehicle model refactoring** - VehicleSchedule validation needs updating for new Vehicle/VehicleScheduleAssignment model
5. **Test expected counts** - Some Xpln.Tests validation count expectations need updating after ValidationError refactoring
