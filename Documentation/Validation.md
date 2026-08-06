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
| `TrainRouteNotConnected` | Two calls the train runs one after the other have no track stretch between them |
| `VehicleScheduleOverlap` | Vehicle schedule has overlapping train parts |
| `LocomotiveCoverageGap` | Train has a gap in locomotive coverage — **no longer produced**, superseded by `TrainMissingTraction` |
| `LocomotiveCoverageOverlap` | Train has overlapping locomotive assignments |
| `VehicleDoubleBooked` | Vehicle has overlapping schedule assignments |
| `ScheduleNotContiguous` | A schedule's parts are not geographically contiguous |
| `ScheduleHasNoVehicle` | A schedule that runs regular sessions has no vehicle assigned |
| `TrainMissingTraction` | A stretch of a train's run has no traction unit on some sessions it runs |
| `VehicleNotClosed` | A traction unit's circulation does not close over the period |
| `VehicleIdentityDuplicated` | Two vehicles share an identity — external id, or operator and number — on a common session |
| `LockKeyIgnored` | An operation location carries a lock key the manning on one side or the other has left meaningless |

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
    public bool ValidateRouteContinuity { get; set; } = true;
    public bool ValidateTrainNumbers { get; set; } = true;
    public bool ValidateSchedules { get; set; } = true;
    public bool ValidateLocomotiveCoverage { get; set; } = true;
    public bool ValidateDriverDuties { get; set; } = true;

    // Threshold values
    public double MinTrainSpeedMetersPerClockMinute { get; set; } = 0.3;
    public double MaxTrainSpeedMetersPerClockMinute { get; set; } = 10;
    public int MinMinutesBetweenTrackUsage { get; set; }         // Fast-clock minutes; 0 = overlap only
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
**Method**: `GetValidationErrors(this StationTrack me, IEnumerable<Schedule> vehicleSchedules, bool extendOccupancyByVehicleStay, int minMinutesBetweenTrackUsage)`

**Validates**: A station track must be free between two occupants for at least
`MinMinutesBetweenTrackUsage` fast-clock minutes. At the default of 0 this is plain double booking —
at most one train on the track at a time — and one train arriving exactly as another leaves is a
handover, not a conflict. Above 0 the same test also requires that much free time in between; exactly
the required number of minutes is enough, one minute less is a conflict.

**Occupancy**: Each call's whole window, optionally extended by a traction unit's stay between two
trains (`ExtendTrackOccupancyByVehicleStay`), not the call's own arrival and departure alone.

**Exception**: Calls sharing the same vehicle are allowed (e.g. loco changes), as are trains that
never run on a common session.

**Error**: `"Train {train1} {span1} overlaps in time with train {train2} {span2}."` where the two are
on the track together, and `"Train {train1} {span1} is followed by train {train2} {span2} after only
{free} minutes; at least {required} minutes are required."` where the required gap is what is missing.

#### L3 — Track stretch conflicts ✅
**Method**: `GetValidationErrors(this TrackStretch me)`

**Validates**: Trains simultaneously on a stretch ≤ track count, **both directions counted together**

**Logic**: Passings sorted by departure; an *i* vs *i + TracksCount* overlap test (direction-agnostic).
A train's passings are its pairs of calls in run order (`Train.CallsInRunOrder`), so a leg is not missed
on a train whose calls were added in another order than it runs them.

**Error**: `"Train {train1} between {stretch1} is conflicting with train {train2} between {stretch2}."`

#### L1 — Meet needs ≥2 tracks 🟡
No dedicated diagnostic; emergent from L2 (single-track meet → same-track conflict) and L3.

#### L4 — Lock key consistency ✅
**Method**: `ValidateLockKeys(this Plan plan)`

**Validates**: A lock key is in force only where the location still needs one — it exchanges cargo and
is not a manned station — and the station holding it is still manned. Manning is edited on both sides
long after a key is set, and either change can leave the key meaningless.

**Kept, not deleted**: an ignored key stays on its location, because the manning change may well be
undone and throwing the key away would make that a retyping job. Everything derived from a key reads
`OperationLocation.EffectiveLockKey`, so an ignored key produces no notes; `OperationLocation.LockKeyFault`
says which change did it.

**Always run**, like the other checks for a model that contradicts itself: a key nobody can fetch is a
fault in the layout whatever else is being validated, not a planning preference to switch off.

**Errors** (one per location, `ValidationScope.Layout`, placeless and timeless):
- `"Operation location {0} is manned, so the lock key held at {1} is ignored."`
- `"Operation location {0} exchanges no cargo, so the lock key held at {1} is ignored."`
- `"Station {1} is not manned, so the lock key it holds for {0} cannot be fetched and is ignored."`

### Timetable scope (T) — train rules

#### T1 + T2 — Train time sequence ✅
**Method**: `CheckTrainTimeSequence(this Train me)`

**Validates**: Train has at least two station calls (T1); calls are ascending and arrival ≤ departure (T2)

**Successive calls** are the pairs of `Train.CallsInRunOrder` — the calls ordered by `SortTime`, which is
the order the train runs them and the order the Trains tab lists them. `Train.Calls` is in insertion
order, which is not run order (a call added last can be timed first), so pairing calls in that order
would report conflicts between calls the train does not run one after the other. What remains a conflict
after ordering is a train whose times contradict themselves — above all one that reaches the next location
before it has left the previous one.

**Errors**:
- `"Train {train} must stop at at least two stations."`
- `"Train {train} calls {call1} and {call2} are conflicting."`

#### T2 — Station call timing ✅
**Method**: `GetValidationErrors(this StationCall stationCall)`

**Validates**: Arrival ≤ departure at the call

**Error**: `"At {station} arrival {arrival} is after departure {departure}."`

#### T3 — Train speed ✅
**Method**: `CheckTrainSpeed(this Train me, ...)`

**Validates**: Speed between consecutive calls within `Min`/`MaxTrainSpeedMetersPerClockMinute`. Every leg
in run order is checked, the one into the terminus included; a leg whose locations have no track stretch is
skipped here and reported by T5 instead.

**Errors**:
- `"Train {train} speed from {station1} {time1} to {station2} {time2} is too slow, length {distance} meters."`
- `"Train {train} speed from {station1} {time1} to {station2} {time2} is too fast, length {distance} meters."`

#### T4 — Duplicate train-number sessions ✅
**Method**: `ValidateTrainNumbers(this Timetable me)`

**Validates**: Trains equal on Company+Category+Number run on disjoint sessions. Trains are grouped by (company, category, number) and every pair whose sessions overlap is flagged. Gated by `ValidateTrainNumbers`.

**Error**: `"Trains {train1} and {train2} have the same number but run on overlapping sessions {sessions}."` (`DuplicateTrainNumber`)

#### T5 — Route continuity ✅
**Method**: `CheckRouteContinuity(this Train me)`

**Validates**: Every leg the train runs — each pair of calls in run order — is a track stretch of the
layout. A train travels a stretch by departing its start and arriving at its end, so it must call at both
ends of every stretch on its way; two successive calls with no stretch between them are a route that jumps
a location, which no train can run. Gated by `ValidateRouteContinuity`.

**Not a gap**: two successive calls at the same operating location (a train changing track there) travel no
stretch. Connectivity is judged in either direction, a stretch being bidirectional.

**Error**: `"Train {train} runs from {location1} {time1} to {location2} {time2}, but the layout has no track
stretch between these locations."` (`TrainRouteNotConnected`)

This rule is why a call in the middle of a route may not be deleted, only one at either end: removing an
intermediate call would leave the route jumping the location it stood for. The deletion rule
(`DeletionRules.MayDelete(StationCall)`) enforces that up front, so the planner cannot create the fault by
deleting; T5 reports it in a plan that already has it — from a hand-edited file, a re-pointed call, or a
stretch removed from the layout under a train that used it.

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

#### S3 + S5 — Circulation closure (per vehicle) ✅
**Method**: `ValidateVehicleClosure(this Plan plan)`

**Validates**: Over the operating period a **vehicle's** movements must **balance at every station** — it departs each station as often as it arrives — so the layout's vehicle distribution repeats and the working can run again. This is flow conservation: movements are counted **per session worked** (`+1` where the unit departs, `−1` where it arrives), so running a leg on more sessions than its return is caught. A unit that works both a forward and a return leg closes even when the legs run on **different sessions** and even when they are **split across several schedules** — the rotation case (forward on some sessions, return on others; position carries over between sessions). Applies to **traction units, wagonsets and cargo-only units**, each of which turns on its own working. **Exempt**: cargo flows (freight directed by waybills, not a turning vehicle) and units whose only working is on demand. Gated by `ValidateSchedules`.

**Error**: `"Vehicle {vehicle} does not return to its start {start}; it ends at {end}."` (`VehicleNotClosed`) — `start` is the station it departs more often than it returns to, `end` the station it is left at.

This single per-unit rule replaces the earlier per-schedule S3 and per-session-combination S5 checks, which required each schedule (or each session combination) to return to its start **in isolation** and so wrongly flagged rotation schemes that close only across sessions or across several schedules.

On-demand trains are marked at import (`XplnDataImporter.MarkSingleTrainWorkingsOnDemand`): a train that is the sole train of a single-train vehicle schedule and the sole train of a duty gets the on-demand session flag.

#### S4 — Per-session traction ✅
**Method**: `ValidateTractionCoverage(this Plan plan)`

**Validates**: Two things.

1. A schedule (turnus) that runs regular (non on-demand) sessions must have **at least one vehicle assigned**; an orphan working with no vehicle at all is reported.
2. **Every leg a train runs** must be hauled by a traction unit on **every session the train runs it**. Traction may come from any schedule that works the train, so a wagonset turnus alongside a loco turnus is fine — coverage is judged per train, not per schedule.

**Exempt**: cargo flows (hauled across several trains, not a self-contained working) and on-demand trains. Gated by `ValidateSchedules`.

**Logic**: For each train, the legs are the pairs of calls in **run order** (`CallsInRunOrder`) — insertion order would pair up calls the train does not run one after the other. Each traction assignment whose schedule has a part spanning a leg contributes its `Sessions` to that leg; the leg is short of traction on the sessions the train runs but no assignment covers. Consecutive legs missing the same sessions are coalesced into one span, so a train with no traction at all gives one error over its whole run rather than one per leg.

Coverage is judged **leg by leg, not by whether the train appears in some turnus at all**. Shortening a turnus part (A→C down to A→B) leaves B→C unworked while the train still has a part in the turnus; the earlier per-train check passed such a plan silently.

**Errors**:
- `"Vehicle schedule {number} has no vehicle assigned."` (`ScheduleHasNoVehicle`)
- `"Train {0} has no traction unit between {1} and {2} on sessions {3}."` (`TrainMissingTraction`)

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

#### P4 — Overlapping locomotive assignments ✅
**Method**: `ValidateLocomotiveCoverage(this Plan plan)`

**Validates**: No train is assigned two traction units over the same stretch of its run

**Logic**:
1. Gathers the schedules traction is booked on, each with the sessions it is booked for: the union over the schedule's traction assignments. Grouped by schedule, so a schedule two locomotives share counts once — a double-headed working is one claim on the train, not two
2. For each train, collects the parts on those schedules (matched by train `Id`, so runs sharing a category and number are not merged), each with the sessions it is hauled on: the booking narrowed to the sessions the train runs
3. Checks for pairs of parts that overlap **both in time and in sessions**

**Error**: `"Train {0} has overlapping locomotive assignments: {1} and {2}."`, where each of `{1}` and `{2}` is a part's `TractionWorkingSpanText` — the locomotives working it and the times of its working span, e.g. `MZ 5 Fullerup 14:51->Skovborg 15:05`. The train is `{0}` already, and two parts of one train can read alike to the minute, so the locomotive is what tells them apart. The traction is resolved through the part's own `Schedule`, not through `TractionUnits`: a part is equal to any part over the same two calls, so two schedules covering one leg each would otherwise both name both locomotives.

**Attributed to two schedules.** The error carries `Schedules = [part1.Schedule, part2.Schedule]`, so `Involves(Schedule)` marks exactly the two schedules holding the offending parts. Without them it fell back to matching by train and marked every schedule holding any part of that train — flagging a locomotive that works the train on a leg nothing doubles.

**A rotation is not a conflict.** Two workings that share no session — one locomotive takes the train on the odd sessions, another on the even — are never at the meeting on the same day, so the train is never hauled twice over and nothing is reported. An on-demand train runs on no numbered session and so has nothing to narrow the bookings by; theirs then stand on their own.

**Where the doubling is confined to some sessions, the message names them** (`TrainHasLocomotiveCoverageOverlapOnSessions`, five languages). Doubled on every session the train runs — the ordinary case, where both bookings are for every session — there is no subset to point at and the plain string is used instead.

**Coverage gaps are not checked here.** S4 judges the same thing per leg and per session, correctly allows a traction change at a station, and reads the calls in run order; the time-based gap check this rule used to carry reported each gap a second time and missed the gap entirely on a train whose calls were added in another order than it runs them. `ValidationErrorType.LocomotiveCoverageGap` is no longer produced.

#### P5 — Duplicate vehicle identity ✅
**Method**: `ValidateVehicleIdentities(this Plan plan)`

**Validates**: A `VehicleIdentity` names one physical vehicle, so on any one session it may belong to only one of the plan's vehicles

**Identity** (`ScheduledObject.Identity`): the `ExternalId` where the vehicle carries one — the identifier it was imported under, unique in the system it came from — otherwise the operating company and the number, the number alone with no company. The two kinds never match each other, and `CreateVehicle` gives a vehicle made in the planner no external id, so operator and number always identify those.

**Logic**:
1. Groups the vehicles by identity alone, so the rule spans every `ScheduledObjectType` (a wagonset and a locomotive may not share one either). Cargo flows are left out — their identifier stands for a group of wagons, not a vehicle (`HasVehicleIdentity`)
2. Compares each vehicle's `ClaimedSessions` — its assignments' sessions, or *every* session when it is assigned nowhere, since an unused vehicle still holds its identity in the pool
3. Reports each duplicate **once**, against the first earlier vehicle of its identity whose sessions it shares. Pairwise reporting would bury the rest of the conflict list: a group of *n* vehicles under one identity gives *n(n−1)/2* pairs but only *n−1* vehicles to fix

**Two vehicles may reuse an identity** when the sessions they work are strictly disjoint — they are then never both at the meeting.

**Errors**:
- `"Vehicles {0} and {1} share operator and number {2} on sessions {3}."`
- `"Two vehicles share the external id {0} on sessions {1}."` (both would otherwise be named by the same designation)

**Imported plans are unaffected.** Every XPLN vehicle carries its own identifier, so the importer test files report exactly the conflicts they did before. Judging identity on operator and number instead would report hundreds of false duplicates, since the number is the trailing digits of the identifier — `DB-Post1`, `NPB E1` and `DSB G 01` all become number 1.

**Editor guard**: `Plan.VehicleClaiming(identity, sessions, excluding)` answers the same question before the edit is made, so the Schedules tab's add- and edit-vehicle dialogs refuse a taken identity rather than letting one be created. Older plans keep theirs and are reported here.

#### P2 — Every part scheduled / no cross-schedule session overlap 🟡
Every train part must belong to a schedule, and no part may be in two schedules with overlapping sessions. Partly served by P3 and P4; the full check is not implemented.

## Not Yet Implemented

The following options exist but validation logic is not implemented:

| Option | Status |
|--------|--------|
| `ValidateTrainNumbers` | Property exists, not used |
| `ValidateDriverDuties` | Property exists, validation not implemented |

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
  │     ├─► CheckRouteContinuity()            [ValidateRouteContinuity] T5
  │     ├─► StationTrack.GetValidationErrors()[ValidateStationTracks]  L2
  │     ├─► StationCall.GetValidationErrors() [ValidateStationCalls]   T2
  │     ├─► TrackStretch.GetValidationErrors()[ValidateStretches]      L3
  │     ├─► CheckTrainSpeed()                 [ValidateTrainSpeed]     T3
  │     └─► Timetable.ValidateTrainNumbers()  [ValidateTrainNumbers]   T4
  │
  ├─► Schedule.ValidateOverlappingParts()     [ValidateSchedules]      S1
  ├─► Schedule.ValidateContiguity()           [ValidateSchedules]      S2
  ├─► Plan.ValidateTractionCoverage()         [ValidateSchedules]      S4
  ├─► Plan.ValidateVehicleClosure()           [ValidateSchedules]      S3+S5
  ├─► Plan.ValidateVehicleDoubleBooking()     [ValidateSchedules]      P3
  └─► Plan.ValidateLocomotiveCoverage()       [ValidateLocomotiveCoverage]  P4 (overlaps only)
```

## Known Issues / TODOs

Rules are catalogued by scope (**L**ayout, **T**imetable, **S**chedule, **P**lan) in the
Requirements Specification §3.11. Fully implemented: L2, L3, T1, T2, T3, T4, T5, S1, S2, S3,
S4, S5, P1, P3, P4. Partial: L1 (emergent from L2/L3), P2. Note L3
counts trains on a stretch **direction-agnostically** (one train per track, both directions
together) — the existing capacity check is correct as-is, not a direction bug.

Closure (S3+S5) is judged **per vehicle** (traction units, wagonsets and cargo-only units) by
flow conservation over the whole period, not per schedule or per session combination — so
rotation schemes that close only across sessions or across several schedules are correctly
allowed (see the S3+S5 section above). Cargo flows are exempt.

1. **P2 (partial)** - every part scheduled; the same part not in two overlapping-session schedules is not fully checked
2. **Vehicle model refactoring** - VehicleSchedule validation needs updating for new Vehicle/VehicleScheduleAssignment model
3. **Test expected counts** - Some Xpln.Tests validation count expectations need updating after ValidationError refactoring
