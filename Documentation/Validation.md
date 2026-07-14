# Validation System Overview

This document describes the validation system in the Tellurian.Trains.Schedules.Importers solution.

## Architecture

The validation system is located in `Model/Validations/` and consists of three files:

| File | Purpose |
|------|---------|
| `Settings/ValidationSettings.cs` | Configuration class for controlling which validations run |
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

## ValidationSettings

Controls which validations are performed and threshold values:

```csharp
public sealed class ValidationSettings
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
IEnumerable<Message> GetValidationErrors(this Schedule schedule, ValidationSettings options)
```
- Entry point for validating entire schedule
- Validates timetable and vehicle schedules

### Timetable Validation
```csharp
IEnumerable<Message> GetValidationErrors(this Timetable timetable, Schedule schedule, ValidationSettings options)
```
- Comprehensive timetable validation
- Calls sub-validators based on options

## Rules by scope

Validation rules are catalogued by **model scope** in the Requirements Specification
§3.11 — Layout (**L**), Timetable (**T**), Schedule (**S**), Plan (**P**). The intended
implementation gives each scope its own validation extension methods
(`extension(Layout)`, `extension(Timetable)`, `extension(Schedule)`, `extension(Plan)`);
the methods below are grouped by the scope they belong to, tagged with the rule id and
status (✅ done · 🟡 partial · ❌ missing).

### Layout scope (L) — infrastructure occupancy

#### L2 — Station track conflicts ✅
**Method**: `GetValidationErrors(this StationTrack me, IEnumerable<Schedule> vehicleSchedules)`

**Validates**: At most one train may occupy a station track at a time (no overlapping calls by different trains)

**Exception**: Calls sharing the same vehicle are allowed (e.g. loco changes)

**Error**: `"Train {train1} {time1} has conflicts with train {train2} {time2}."`

#### L3 — Track stretch conflicts ✅
**Method**: `GetValidationErrors(this TrackStretch me)`

**Validates**: Trains simultaneously on a stretch ≤ track count, **both directions counted together**

**Logic**: Passings sorted by departure; an *i* vs *i + TracksCount* overlap test (direction-agnostic)

**Error**: `"Train {train1} between {stretch1} is conflicting with train {train2} between {stretch2}."`

#### L1 — Meet needs ≥2 tracks 🟡
No dedicated diagnostic; emergent from L2 (single-track meet → same-track conflict) and L3.

### Timetable scope (T) — train rules

#### T1 + T2 — Train time sequence ✅
**Method**: `CheckTrainTimeSequence(this Train me)`

**Validates**: Train has at least two station calls (T1); calls are ascending and arrival ≤ departure (T2)

**Errors**:
- `"Train {train} must stop at at least two stations."`
- `"Train {train} calls {call1} and {call2} are conflicting."`

#### T2 — Station call timing ✅
**Method**: `GetValidationErrors(this StationCall stationCall)`

**Validates**: Arrival ≤ departure at the call

**Error**: `"At {station} arrival {arrival} is after departure {departure}."`

#### T3 — Train speed ✅
**Method**: `CheckTrainSpeed(this Train me, ...)`

**Validates**: Speed between consecutive calls within `Min`/`MaxTrainSpeedMetersPerClockMinute`

**Errors**:
- `"Train {train} speed from {station1} {time1} to {station2} {time2} is too slow, length {distance} meters."`
- `"Train {train} speed from {station1} {time1} to {station2} {time2} is too fast, length {distance} meters."`

#### T4 — Duplicate train-number sessions ✅
**Method**: `ValidateTrainNumbers(this Timetable me)`

**Validates**: Trains equal on Company+Category+Number run on disjoint sessions. Trains are grouped by (company, category, number) and every pair whose sessions overlap is flagged. Gated by `ValidateTrainNumbers`.

**Error**: `"Trains {train1} and {train2} have the same number but run on overlapping sessions {sessions}."` (`DuplicateTrainNumber`)

### Schedule scope (S) — vehicle schedule / turnus

#### S1 — Overlapping parts ✅
**Method**: `ValidateOverlappingParts(this Schedule me)`

**Validates**: A schedule's train parts do not overlap in time (one vehicle, one place)

**Error**: `"Vehicle schedule {id} contains overlapping {trainPart1} and {trainPart2}."`

#### S2 — Part contiguity ✅
**Method**: `ValidateContiguity(this Schedule me)`

**Validates**: Each part, in working (departure) order, starts from the operation location where the previous part ended. `Append` enforces this at entry time; this check covers schedules assembled unconditionally with `Add` (e.g. reconstructed from XPLN import). Applies to all vehicle types (locomotives, trainsets, wagons, cargo). Gated by `ValidateSchedules`.

**Skipped when the schedule's parts overlap in time** (`HasOverlappingParts`): such a schedule is not one vehicle's working — typically two vehicles an import merged under one identifier — so S1 already reports the overlap and ordering the parts for a contiguity test would only cascade misleading gaps.

**Error**: `"Vehicle schedule {number}: {trainPart} does not continue from where the previous part ended at {location}."` (`ScheduleNotContiguous`)

#### S3 — Circulation closure ✅
**Method**: `ValidateScheduleClosure(this Plan plan)`

**Validates**: A vehicle schedule that runs the whole operating period (`EffectiveSessions.CoversAllWithin(useDays, maxSessions)` from `Layout.Settings.General`) and whose vehicle is a **traction unit** must start and end at the **same station**. Scoped to traction (a loco/trainset must return to be reused; wagons/cargo flows are repositioned by other workings — rule S3's "starting traction unit"). **Exempt**: schedules whose trains are all on demand. A subset-session schedule is not checked here — it may be closed by a complementary schedule on the remaining sessions (the multi-session split/part-count case is deferred). Gated by `ValidateSchedules`.

**Error**: `"Vehicle schedule {number} runs every session but does not return to its start {start}; it ends at {end}."` (`ScheduleNotClosed`)

On-demand trains are marked at import (`XplnDataImporter.MarkSingleTrainWorkingsOnDemand`): a train that is the sole train of a single-train vehicle schedule and the sole train of a duty gets the on-demand session flag.

#### S4 — Per-session traction 🟡
A traction unit for all of the schedule's sessions. Partly served by P4; not checked per session across the schedule.

#### S5 — Valid session combinations ✅
**Method**: `ValidateSessionCombinationClosure(this Plan plan)`

**Validates**: For a **traction** vehicle with **more than one** `SessionCombination` (it works different parts on different sessions/days), each combination must return the vehicle to its start station (each conforming to S3). A single-combination vehicle is already covered by S3, so the two do not double-report. On-demand combinations are exempt. Gated by `ValidateDriverDuties`.

**Error**: `"Vehicle {number} on sessions {sessions} does not return to its start {start}; it ends at {end}."` (`SessionCombinationNotClosed`)

### Plan scope (P) — cross-object consistency

#### P1 — Referential integrity ✅ (always enforced)
**Methods**: `EnsureStationHasTrack(this Timetable me)`; `DeletionRules` `MayDelete` / `TryDelete`

**Validates**: Station tracks referenced in trains exist in the layout; a referenced object cannot be deleted

**Error**: `"Track {track} in station {station} referred in train {train} is not in layout."`

#### P3 — Vehicle double booking ✅
**Method**: `ValidateVehicleDoubleBooking(this Plan plan)`

**Validates**: No vehicle is assigned to schedules with overlapping sessions

**Logic**: For each vehicle, compares assignment pairs for overlapping `Sessions` (bitwise AND on session flags)

**Error**: `"Vehicle {0} is double-booked: sessions {1} overlap with sessions {2}."`

#### P4 — Locomotive / traction coverage ✅
**Method**: `ValidateLocomotiveCoverage(this Plan plan)`

**Validates**: Every train's run is covered by traction schedules without gaps or overlaps

**Logic**:
1. Gathers traction schedules (Locomotive/Trainset assignments)
2. For each train, collects the parts assigned to traction
3. Checks for gaps (uncovered segments) and overlaps (double-booked traction)

**Exception**: A loco change at the same station is allowed — one part ending and another beginning at the same station (even with a changeover gap) is not a coverage gap.

**Errors**:
- `"Train {0} has no locomotive assigned."` — no traction at all
- `"Train {0} has a locomotive coverage gap between {1} and {2}."` — gap between different stations
- `"Train {0} has overlapping locomotive assignments: {1} and {2}."` — overlap detected

#### P2 — Every part scheduled / no cross-schedule session overlap 🟡
Every train part must belong to a schedule, and no part may be in two schedules with overlapping sessions. Partly served by P3 and P4; the full check is not implemented.

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
    var options = new ValidationSettings
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
Plan.GetValidationErrors(options)
  │
  ├─► Timetable.GetValidationErrors(plan, options)
  │     │
  │     ├─► EnsureStationHasTrack()           [always]        P1
  │     ├─► CheckTrainTimeSequence()          [always]        T1, T2
  │     ├─► StationTrack.GetValidationErrors()[ValidateStationTracks]  L2
  │     ├─► StationCall.GetValidationErrors() [ValidateStationCalls]   T2
  │     ├─► TrackStretch.GetValidationErrors()[ValidateStretches]      L3
  │     ├─► CheckTrainSpeed()                 [ValidateTrainSpeed]     T3
  │     └─► Timetable.ValidateTrainNumbers()  [ValidateTrainNumbers]   T4
  │
  ├─► Schedule.ValidateOverlappingParts()     [ValidateSchedules]      S1
  ├─► Schedule.ValidateContiguity()           [ValidateSchedules]      S2
  ├─► Plan.ValidateScheduleClosure()          [ValidateSchedules]      S3
  ├─► Plan.ValidateVehicleDoubleBooking()     [ValidateSchedules]      P3
  ├─► Plan.ValidateSessionCombinationClosure()[ValidateDriverDuties]   S5
  └─► Plan.ValidateLocomotiveCoverage()       [ValidateLocomotiveCoverage]  P4
```

## Known Issues / TODOs

Rules are catalogued by scope (**L**ayout, **T**imetable, **S**chedule, **P**lan) in the
Requirements Specification §3.11. Fully implemented: L2, L3, T1, T2, T3, T4, S1, S2, S3,
S5, P1, P3, P4. Partial: L1 (emergent from L2/L3), S3 multi-session split, S4, P2. Note L3
counts trains on a stretch **direction-agnostically** (one train per track, both directions
together) — the existing capacity check is correct as-is, not a direction bug.

1. **S3 — multi-session split / part-count** - a subset-session schedule that doesn't close is left to its complementary schedule; the "part count = sessions needed to return" split is not yet validated
2. **S4 / P2 (partial)** - per-session traction coverage; every part scheduled; same part not in overlapping-session schedules
3. **MinMinutesBetweenTrackUsage** - Parameter exists but not used
4. **Vehicle model refactoring** - VehicleSchedule validation needs updating for new Vehicle/VehicleScheduleAssignment model
5. **Test expected counts** - Some Xpln.Tests validation count expectations need updating after ValidationError refactoring
