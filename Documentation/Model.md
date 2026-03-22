# Unified Domain Model

> **Last Updated:** 2025-12-30

## Completed Work

- [x] Three-layer hierarchy: Layout → Timetable → Schedule
- [x] Layout layer: `Layout`, `OperationLocation`, `StationTrack`, `TrackStretch`, `TimetableStretch`
- [x] Timetable layer: `Timetable`, `Train`, `TrainCategory`, `StationCall`, `Time`
- [x] Schedule layer: `Schedule`, `VehicleSchedule`, `LocoSchedule`, `TrainsetSchedule`, `DriverDuty`, `TrainPart`
- [x] `TrainCategory` class with `Prefix`, `Suffix`, `Name`, `Color`, `DisplayOrder`
- [x] `Sessions` class (replaces obsolete `OperatingSessions`) with factory properties
- [x] `Company` record with `Id`, `Name`, `Signature`, `CountryCode`
- [x] Train uses `int Number` + `TrainCategory Category` (not string-based identity)
- [x] Services layer with `ICompaniesService`, `ITrainCategoriesService`
- [x] JSON dataset with ~9,700+ railway operators
- [x] XPLN importer: composite identity parsing ("Gt1234" → prefix + number)
- [x] XPLN importer: auto-create `TrainCategory` from unique prefixes + background color
- [x] XPLN importer: deterministic ID generation using rowNumber
- [x] XPLN importer: three-step import (Layout → Timetable → Schedule)
- [x] EF Core compatibility: FK properties, private constructors, `[JsonIgnore]` attributes
- [x] Model.EntityFramework project with `ScheduleDbContext`
- [x] Tellurian.Utilities integration (replaced local extension methods)
- [x] All tests passing (73 tests)

---

## Remaining Work

### Model Improvements

- [ ] **Access importer alignment** - Update Access importer to use new model classes with FK properties
- [ ] **Planning** - Implement planning functionality (project created, placeholder only)

### Future Phases

- [ ] **Extract to NuGet** - Create separate `Tellurian.Trains.Model` NuGet package
- [ ] **Dispatch integration** - See "Dispatch Integration" section at end of document

---

## Model Overview

### Conceptual Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         DATA SOURCES                                    │
│                                                                         │
│  ┌─────────────────┐  ┌───────────────────┐  ┌───────────────────┐      │
│  │ Module Registry │  │  Access Database  │  │ XPLN Spreadsheet  │      │
│  │  (Layout only)  │  │    (Complete)     │  │    (Complete)     │      │
│  └────────┬────────┘  └────────┬──────────┘  └────────┬─────────┘       │
│           │                    │                      │                 │
│           ▼                    ▼                      ▼                 │
├─────────────────────────────────────────────────────────────────────────┤
│                    THREE-STEP IMPORT PROCESS                            │
│                                                                         │
│  Step 1: Import Layout (infrastructure)                                 │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │  LAYOUT                                                         │    │
│  │  ┌───────────────┐  ┌───────────────┐  ┌────────────────────┐   │    │
│  │  │ Stations +    │  │ TrackStretches│  │ TimetableStretches │   │    │
│  │  │ Tracks        │  │               │  │                    │   │    │
│  │  └───────────────┘  └───────────────┘  └────────────────────┘   │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                              │                                          │
│                              ▼ (reference)                              │
│  Step 2: Import Timetable (trains, validates against Layout)            │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │  TIMETABLE                                                      │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │    │
│  │  │  Sessions   │  │   Train     │  │   Trains    │              │    │
│  │  │             │  │ Categories  │  │   + Calls   │              │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘              │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                              │                                          │
│                              ▼ (reference)                              │
│  Step 3: Import Schedule (vehicle schedules + duties)                   │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │  SCHEDULE                                                       │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │    │
│  │  │    Loco     │  │  Trainset   │  │   Driver    │              │    │
│  │  │  Schedules  │  │  Schedules  │  │   Duties    │              │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘              │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

### Three-Layer Hierarchy

```
Layout (infrastructure)
   ↑
Timetable (trains) ─── references Layout
   ↑
Schedule (equipment + crew) ─── references Timetable
```

This separation enables:
- Importing Layout from Module Registry, then Timetable and Schedule from XPLN
- Validating Timetable data against the Layout during import
- Validating Schedule data against Trains in the Timetable
- Reusing the same Layout across different timetables
- Having multiple Schedules for the same Timetable (different crew/equipment rotations)

### Layer Contents

| Layer | Classes | Purpose |
|-------|---------|---------|
| **Layout** | `Layout`, `OperationLocation`, `StationTrack`, `TrackStretch`, `TimetableStretch` | Physical infrastructure |
| **Timetable** | `Timetable`, `Train`, `TrainCategory`, `StationCall`, `Time`, `Sessions` | Planned train services |
| **Schedule** | `Schedule`, `LocoSchedule`, `TrainsetSchedule`, `DriverDuty`, `TrainPart` | Equipment and crew assignments |

---

## Sessions

The `Sessions` class manages operating sessions (1-14). Entities reference this to specify which sessions they operate on.

**Note:** `Sessions` replaces the obsolete `OperatingSessions` class.

| Configuration | MaxSessions | Example |
|---------------|-------------|---------|
| Bi-daily | 2 | Sessions 1 and 2 alternate |
| Weekly | 7 | Sessions 1-7 map to weekdays |
| Bi-weekly | 14 | Sessions 1-7 = week 1, 8-14 = week 2 |

Factory properties: `Sessions.All`, `Sessions.Odd`, `Sessions.Even`, `Sessions.OnDemand`

---

## ID Generation Strategy

ID assignment is delegated to the import module:

| Data Source | ID Strategy |
|-------------|-------------|
| **Access Database** | Use actual database IDs |
| **Module Registry** | Use actual registry IDs |
| **XPLN Spreadsheets** | Generate deterministic IDs from rowNumber |

---

## EF Core and JSON Compatibility

The model supports both EF Core (database) and JSON serialization without requiring DTOs.

### Key Patterns

| Pattern | Purpose |
|---------|---------|
| Private parameterless constructor | EF Core instantiation |
| FK properties (`TimetableId`, `TrackId`) | Efficient queries, JSON round-trips |
| `[JsonIgnore]` on parent navigation | Breaks circular references |
| `[JsonInclude]` on internal setters | Enables JSON serialization |

### Example: Train Class

```csharp
public class Train : IEquatable<Train>
{
    private Train() { }  // EF Core

    [SetsRequiredMembers]
    public Train(int id, int number, string externalId = "") { ... }

    public required int Id { get; set; }
    public required int Number { get; set; }

    // FK property for EF Core
    public int TimetableId { get; set; }

    // Navigation property - ignored in JSON
    [JsonIgnore]
    public Timetable Timetable { get; set; }

    public IList<StationCall> Calls { get; set; }
}
```

### Extension Methods Must Set FKs

When adding entities via extension methods, both navigation and FK properties must be set:

```csharp
public static Train Add(this Timetable timetable, Train train)
{
    train.Timetable = timetable;
    train.TimetableId = timetable.Id;  // Required for JSON round-trip
    timetable.Trains.Add(train);
    return train;
}
```

---

## Dispatch Integration (Future)

> This section describes future integration with the Dispatch application. This work is lower priority now that the core model is established.

### Dispatch-Specific Extensions

The Dispatch application would add operational state on top of the planned schedule:

| Addition | Purpose |
|----------|---------|
| `TrainState` enum | Planned → Manned → Running → Completed |
| `DispatchState` enum | Requested → Accepted → Departed → Arrived |
| Observed times | Actual arrival/departure times |
| Track occupancy | Runtime capacity tracking |
| `SignalControlledPlace`, `OtherPlace` | Operational location types |

### Extension Pattern

```csharp
namespace Tellurian.Trains.Dispatch;

public record DispatchTrain : Train
{
    public TrainState State { get; set; }
    public TrainState? PreviousState { get; private set; }
}

public record StationCallState
{
    public StationCall BaseCall { get; init; }
    public StationTrack? NewTrack { get; set; }  // Track change
    public Time ObservedArrival { get; set; }
    public Time ObservedDeparture { get; set; }
}
```

### Model Comparison (Importers vs Dispatch)

| Concept | Schedule.Importers | Dispatch |
|---------|-------------------|----------|
| Operating Location | `OperationLocation` | `OperationPlace` (abstract) with subtypes |
| Station Track | `StationTrack` | Similar + MaxLength, PlatformLength |
| Track Stretch | `TrackStretch` | Similar + capacity management |
| Train | `Train` | + TrainState, observed times |
| Station Call | `StationCall` | + scheduled vs observed times |

### Integration Phases

1. **Add NuGet reference** to `Tellurian.Trains.Model`
2. **Create Dispatch extensions** of base model classes
3. **Create state extension classes** for runtime tracking
4. **Create import adapter** to convert imported data to Dispatch types
5. **Integrate with Broker** for state persistence

### Open Questions

1. Should Dispatch extend Station or use composition?
   - *Recommendation:* Extend with `DispatchStation`

2. How to handle Dispatch-specific track features?
   - *Recommendation:* Wrap `TrackStretch` with `DispatchTrackStretch`

3. Should `TrainPart` be in base model or Schedule only?
   - *Recommendation:* Keep in Schedule layer
