# Unified Domain Model Analysis and Plan

---

## Implementation Status

> **Last Updated:** 2025-12-24

### ✅ Completed

| Item | Description |
|------|-------------|
| **Three-Layer Structure** | Layout → Timetable → Schedule hierarchy fully implemented |
| **Layout Layer** | `Layout`, `Station`, `StationTrack`, `TrackStretch`, `TimetableStretch` |
| **Timetable Layer** | `Timetable`, `Train`, `TrainCategory`, `StationCall`, `Time` |
| **Schedule Layer** | `Schedule`, `VehicleSchedule`, `LocoSchedule`, `TrainsetSchedule`, `DriverDuty`, `TrainPart` |
| **TrainCategory Class** | New class with `Prefix`, `Suffix`, `ResourceName`, `Color`, `DisplayOrder` |
| **OperatingSessions Class** | Session management with factory properties (`OnDemand`, `AllSessions`, etc.) |
| **Train Refactored** | Changed from string-based to `int Number` + `TrainCategory Category` |
| **OperatingCompany Refactored** | Proper record with `Id`, `Name`, `Signature`, `CountryCode` |
| **Services Layer** | New `Services` project with `IOperatingCompaniesService`, `ITrainCategoriesService` |
| **Operating Companies Data** | JSON dataset with ~9,700+ railway operators |
| **All Tests Passing** | 54 tests pass across Model.Tests, Interfaces.Tests, Xpln.Tests, Services.Tests |

### 🔄 In Progress / Remaining

| Phase | Item | Description |
|-------|------|-------------|
| **Phase 2** | ✅ XPLN composite identity parsing | Parse "Gt1234" → prefix="Gt", number=1234 |
| **Phase 2** | ✅ TrainCategory creation from XPLN | Auto-create categories from unique prefixes + background color |
| **Phase 2** | ✅ Deterministic ID generation | Using rowNumber for reproducible IDs |
| **Phase 2** | ✅ Three-step import orchestration | Explicit Layout → Timetable → Schedule in `XplnDataImporter` |
| **Phase 2** | ~~Operating sessions mapping~~ | N/A - XPLN has no session columns; schedules are single-session |
| **Phase 2** | ⬜ Access importer alignment | Update Access importer to use new model classes |

### 📋 Future Phases

| Phase | Description | Status |
|-------|-------------|--------|
| **Phase 3** | Extract model to separate `Tellurian.Trains.Model` NuGet package | Not started |
| **Phase 4** | Dispatch integration with state extensions | Not started |
| **Phase 5** | Import-to-Dispatch adapter | Not started |
| **Phase 6** | Broker integration | Not started |

---

## Executive Summary

This document analyzes the domain models in **Schedule.Importers/Model** and **Dispatch/Trains+Layout** folders with the goal of creating a unified base model that can be shared between both projects.

The base model represents the **planned schedule** - the result of the schedule planning process. This includes the layout (stations, tracks, stretches), train categories, and the complete timetable with all scheduled train movements. This planned data is **static** and **immutable** during operations.

Systems that consume the planned schedule add their own concerns:
- **Dispatch** adds operational state (TrainState, DispatchState) and observed times
- **Timetable publishing** might add formatting and presentation data
- **Simulation** might add vehicle physics and timing variations

By separating the planned schedule from runtime concerns, we can use the Schedule.Importers functionality to import base data into any consumer while each consumer maintains its own state layer.

---

## Conceptual Model

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
│  │  Physical infrastructure - can come from different source       │    │
│  │  ┌───────────┐  ┌───────────────┐  ┌────────────────────┐       │    │
│  │  │ Stations  │  │ TrackStretches│  │ TimetableStretches │       │    │
│  │  │ + Tracks  │  │               │  │                    │       │    │
│  │  └───────────┘  └───────────────┘  └────────────────────┘       │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                              │                                          │
│                              ▼ (reference)                              │
│  Step 2: Import Timetable (trains, validates against Layout)            │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │  TIMETABLE                                                      │    │
│  │  Train services - references Layout for validation              │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │    │
│  │  │ Operating   │  │   Train     │  │   Trains    │              │    │
│  │  │ Sessions    │  │ Categories  │  │   + Calls   │              │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘              │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                              │                                          │
│                              ▼ (reference)                              │
│  Step 3: Import Schedule (vehicle schedules + duties)                   │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │  SCHEDULE                                                       │    │
│  │  Equipment and crew assignments - references Timetable          │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │    │
│  │  │    Loco     │  │  Trainset   │  │   Driver    │              │    │
│  │  │  Schedules  │  │  Schedules  │  │   Duties    │              │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘              │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                              │                                          │
├──────────────────────────────┼──────────────────────────────────────────┤
│                              ▼                                          │
│                     CONSUMING SYSTEMS                                   │
│                                                                         │
│  ┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────┐  │
│  │      DISPATCH       │  │  TIMETABLE PUBLISH  │  │   SIMULATION    │  │
│  │                     │  │                     │  │                 │  │
│  │ + TrainState        │  │ + Formatting        │  │ + Physics       │  │
│  │ + DispatchState     │  │ + Page layout       │  │ + Delays        │  │
│  │ + Observed times    │  │ + Station sheets    │  │ + Random events │  │
│  │ + Track occupancy   │  │                     │  │                 │  │
│  └─────────────────────┘  └─────────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
```

### Three-Layer Model Structure

The model uses a three-layer hierarchy where each layer references the one above:

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

### What belongs in Layout (Step 1 - Infrastructure)

| Included | Reason |
|----------|--------|
| Stations with tracks | Physical locations where trains can stop |
| Track stretches | Physical connections between stations |
| Timetable stretches | Logical route groupings for display/organization |

**Sources:** Access Database, Module Registry, XPLN

### What belongs in Timetable (Step 2 - Trains)

| Included | Reason |
|----------|--------|
| Reference to Layout | Enables validation of station/track references |
| Operating sessions | Session patterns for trains |
| Train categories | Classification with prefix/suffix/color |
| Trains with station calls | Planned train services and schedules |

**Sources:** Access Database, XPLN (Module Registry does not provide timetable data)

### What belongs in Schedule (Step 3 - Equipment + Crew)

| Included | Reason |
|----------|--------|
| Reference to Timetable | Enables validation of train references |
| Loco schedules | Locomotive rotation assignments to trains |
| Trainset schedules | Trainset rotation assignments to trains |
| Driver duties | Crew assignments to trains |

**Sources:** Access Database, XPLN

### What belongs in Consuming Systems

| System | Additions | Reason |
|--------|-----------|--------|
| **Dispatch** | TrainState, DispatchState, observed times, track changes | Runtime operational state |
| **Dispatch** | SignalControlledPlace, OtherPlace | Operational place types beyond stations |
| **Dispatch** | TrainSection, track occupancy | Movement tracking |
| **Publishing** | Page breaks, column widths, fonts | Presentation concerns |
| **Simulation** | Actual running times, delays, conflicts | Simulated execution |

---

## 1. Current Model Comparison

### 1.1 Layout-Related Classes

| Concept | Schedule.Importers | Dispatch | Notes |
|---------|-------------------|----------|-------|
| **Layout container** | `Layout` (record) | - | Contains operating locations, track stretches, timetable stretches |
| **Operating Location** | `Station` (record) | `OperationPlace` (abstract) | Proposed: use `OperatingLocation` as base type in unified model |
| **Station** | `Station` (record) | `Station` (extends `OperationPlace`) | Both have Name, Signature, Tracks. Dispatch has IsManned, Dispatcher, PreferredLanguage |
| **Station Track** | `StationTrack` (record) | `StationTrack` (record) | Similar: Number, IsMain. Dispatch has MaxLength, PlatformLength. Importers has Length, IsScheduled, Usage, Calls |
| **Track Stretch** | `TrackStretch` (class) | `TrackStretch` (class) | Similar concept. Importers has Start/End Station, Distance, TracksCount, Speed, Time. Dispatch has Start/End OperationPlace, Tracks list, capacity management |
| **Timetable Stretch** | `TimetableStretch` (record) | `DispatchStretch` (class) | Logical grouping of track stretches. Dispatch adds direction handling |
| **Signal Controlled Place** | - | `SignalControlledPlace` (record) | Dispatch-specific: block signals, passing loops |
| **Other Place** | - | `OtherPlace` (record) | Dispatch-specific: halts, unsignalled junctions |

### 1.2 Train-Related Classes

| Concept | Schedule.Importers | Dispatch | Notes |
|---------|-------------------|----------|-------|
| **Train Category** | - (string property) | - (via Identity.Prefix) | Currently implicit. Proposed: explicit `TrainCategory` class with Prefix/Suffix/Color |
| **Train** | `Train` (class) | `Train` (record) | Proposed: `Number` as integer (1-99999), `Category` as reference to `TrainCategory` |
| **Train Call** | `StationCall` (record) | `TrainStationCall` (record) | Proposed: rename to `TrainCall`. Both: Track, Arrival/Departure times, IsArrival/IsDeparture. Dispatch adds Scheduled/Observed times, SequenceNumber |
| **Time** | `Time` (struct) | `CallTime` (struct) | Importers: single TimeSpan. Dispatch: separate ArrivalTime/DepartureTime |
| **Train Part** | `TrainPart` (record) | - | Part of a train journey (from/to calls) |
| **Train Section** | - | `TrainSection` (class) | Dispatch-specific: runtime section with state management |
| **Train State** | - | `TrainState` (enum) | Dispatch-specific: Planned, Manned, Running, etc. |
| **Company** | - | `Company` (record) | Dispatch-specific |
| **Identity** | - | `Identity` (record) | Dispatch: Prefix + Number |

### 1.3 Schedule/Timetable Classes

| Concept | Schedule.Importers | Dispatch | Notes |
|---------|-------------------|----------|-------|
| **Timetable** | `Timetable` (record) | - | Container for trains and layout |
| **Schedule** | `Schedule` (record) | - | Container for timetable + vehicle schedules + duties |
| **Vehicle Schedule** | `VehicleSchedule`, `LocoSchedule`, `TrainsetSchedule` | - | Schedule.Importers only |
| **Driver Duty** | `DriverDuty` (class) | - | Schedule.Importers only |

### 1.4 State/Runtime Classes (Dispatch Only)

| Class | Purpose |
|-------|---------|
| `TrainState` (enum) | Train lifecycle: Planned → Manned → Running → Completed |
| `DispatchState` (enum) | Section dispatch: None → Requested → Accepted → Departed → Arrived |
| `TrainSection` | Runtime object for train movement between stations |
| `TrackStretchOccupancy` | Track capacity tracking |
| `DispatchStretchDirection` | Direction-aware stretch traversal |

---

## 2. Key Differences Analysis

### 2.1 ID Strategy

**Schedule.Importers:**
- Most classes have `int Id { get; init; }` with database-generated option
- Some use private backing fields with public readonly properties
- No auto-generation logic

**Dispatch:**
- Uses `int Id { get; set { field = value.OrNextId; } }` pattern
- Auto-generates IDs when set to 0

**Recommendation:** The Dispatch pattern is more flexible. For deterministic IDs from imports, the importer can compute and set explicit IDs, while runtime-created objects get auto-generated IDs.

### 2.2 Operating Location Hierarchy

**Schedule.Importers:** Single `Station` class

**Dispatch:** Inheritance hierarchy:
```
OperationPlace (abstract)
├── Station
├── SignalControlledPlace
└── OtherPlace
```

**Recommendation:** Use `OperatingLocation` as the base type in the unified model instead of `Station`. This is a more accurate term that encompasses all types of locations where trains can stop or pass through.

**Unified Model:**
```
OperatingLocation (base record in Tellurian.Trains.Model)
├── Common properties: Id, Name, Signature, Tracks
└── Each consuming application defines its own subtypes
```

**Dispatch subtypes (example):**
```
OperatingLocation (from base model)
    ↓ (application-specific extensions)
├── Station (manned station with dispatcher)
├── SignalControlledPlace (block signals, passing loops)
└── OtherPlace (halts, unsignalled junctions)
```

This approach allows:
- The base model to remain generic and reusable
- Each consuming application to define subtypes optimal for its needs
- Importers to create `OperatingLocation` instances without knowing about application-specific subtypes

### 2.3 Time Representation

**Schedule.Importers:** `Time` struct wrapping `TimeSpan`, with rich parsing from ODS/Excel formats

**Dispatch:** `CallTime` struct with separate `ArrivalTime` and `DepartureTime` TimeSpans

**Recommendation:** Both representations are valid. The unified model should use a `Time` struct similar to Schedule.Importers, with `CallTime` being a Dispatch-specific composite for scheduled vs. observed times.

### 2.4 Train Identity and Category

**Train Number:** An integer value in the range 1-99999.

**Current implementations:**

| Source | Train Identity | Category Handling |
|--------|---------------|-------------------|
| **Access Database** | Separate `TrainCategory` object referenced from `Train` | Already properly separated |
| **XPLN Spreadsheet** | Composite string like "Gt1234" combining prefix and number | Needs parsing to split |
| **Dispatch** | `Train.Identity` (Prefix + Number) | Implicit category via prefix |

**XPLN Import Strategy:**

The XPLN spreadsheet uses a composite train identity (e.g., "Gt1234", "P42", "Sn101") that must be parsed:

1. **Parse composite identity** → Extract prefix/suffix and integer number
   - "Gt1234" → prefix="Gt", number=1234
   - "P42" → prefix="P", number=42
   - "101Sn" → number=101, suffix="Sn" (suffix case)

2. **Create TrainCategory for each unique prefix/suffix** found during import
   - First occurrence of "Gt" creates a TrainCategory with Prefix="Gt"
   - Subsequent "Gt" trains reference the same TrainCategory

3. **Extract color from traindef row** (future enhancement)
   - XPLN colors the 'traindef' row based on train type
   - This color can be extracted and stored in TrainCategory.Color
   - Requires ODS cell styling extraction (not yet implemented)

**Recommendation:** Introduce explicit `TrainCategory` class that:
- Defines Prefix/Suffix for train identity display
- Specifies ResourceName (e.g., "Passenger", "Freight") for localization
- Provides Color for timetable graphs and schematic displays
- Is part of Timetable (train categories are timetable-specific)
- Is required for all trains
- Train.Number is an integer, not a string

---

## 3. Proposed Unified Model Architecture

### 3.1 Layer Separation

```
┌─────────────────────────────────────────────────────────────────┐
│                    IMPORT LAYER                                 │
│                    Schedule.Importers                           │
│                                                                 │
│  Three-step import process:                                     │
│                                                                 │
│  Step 1: Import Layout (from Access, Module Registry, or XPLN)  │
│          → Produces Layout with Stations, TrackStretches,       │
│            TimetableStretches                                   │
│                                                                 │
│  Step 2: Import Timetable (from Access or XPLN)                 │
│          → Requires Layout reference for validation             │
│          → Produces Timetable with Trains, Categories,          │
│            OperatingSessions                                    │
│                                                                 │
│  Step 3: Import Schedule (from Access or XPLN)                  │
│          → Requires Timetable reference for validation          │
│          → Produces Schedule with LocoSchedules,                │
│            TrainsetSchedules, DriverDuties                      │
├─────────────────────────────────────────────────────────────────┤
│                    PLANNED DATA (Base Model)                    │
│                    Tellurian.Trains.Model                       │
│                                                                 │
│  Layout (infrastructure):                                       │
│  - Station, StationTrack                                        │
│  - TrackStretch, TimetableStretch                               │
│                                                                 │
│  Timetable (trains, references Layout):                         │
│  - OperatingSessions (session patterns)                         │
│  - TrainCategory, Train, StationCall, Time                      │
│                                                                 │
│  Schedule (equipment + crew, references Timetable):             │
│  - LocoSchedule, TrainsetSchedule                               │
│  - DriverDuty, TrainPart                                        │
├─────────────────────────────────────────────────────────────────┤
│                    CONSUMER LAYER (Example: Dispatch)           │
│                    Tellurian.Trains.Dispatch                    │
│                                                                 │
│  Adds runtime/operational state on top of planned data:         │
│  - TrainState, DispatchState (runtime enums)                    │
│  - TrainSection (movement tracking with state)                  │
│  - Observed times, track changes                                │
│  - SignalControlledPlace, OtherPlace (operational places)       │
│  - Track occupancy, capacity management                         │
└─────────────────────────────────────────────────────────────────┘
```

**Key principles:**

1. **Three-layer hierarchy:** Layout → Timetable → Schedule, each referencing its parent
2. **Different sources:** Layout can come from Module Registry while Timetable/Schedule come from XPLN
3. **Validation dependency:** Each layer validates against its parent (trains against layout, schedules against trains)
4. **Immutability:** All three layers are static during operations
5. **Deterministic:** Same import always produces identical model (via source-appropriate IDs)
6. **Multiple schedules:** A single Timetable can have multiple Schedules (different rotations)

### 3.2 Unified Base Model Classes

#### Layout.cs (Infrastructure)
```csharp
namespace Tellurian.Trains.Model;

/// <summary>
/// Represents the physical railway infrastructure.
/// Imported in Step 1, can come from Access, Module Registry, or XPLN.
/// A single Layout can be referenced by multiple Timetables.
/// </summary>
public sealed record Layout
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;

    public ICollection<Station> Stations { get; init; } = [];
    public ICollection<TrackStretch> TrackStretches { get; init; } = [];
    public ICollection<TimetableStretch> TimetableStretches { get; init; } = [];

    public override string ToString() => Name;
}
```

#### Timetable.cs (Trains)
```csharp
namespace Tellurian.Trains.Model;

/// <summary>
/// Represents the train services for a Layout.
/// Imported in Step 2, requires a Layout reference for validation.
/// Contains trains and their station calls.
/// </summary>
public sealed record Timetable
{
    public Layout Layout { get; init; }
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;

    public ICollection<Train> Trains { get; }

    public Timetable(string name, Layout layout)
    {
        Name = name;
        Layout = layout;
        Trains = [];
    }

    public override string ToString() => Name;
}
```

#### Schedule.cs (Equipment + Crew)
```csharp
namespace Tellurian.Trains.Model;

/// <summary>
/// Represents vehicle schedules and driver duties for a Timetable.
/// Imported in Step 3, requires a Timetable reference for validation.
/// Contains locomotive/trainset rotations and crew assignments.
/// </summary>
public record Schedule
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Timetable Timetable { get; init; }

    public ICollection<LocoSchedule> LocoSchedules { get; }
    public ICollection<TrainsetSchedule> TrainsetSchedules { get; }
    public ICollection<DriverDuty> DriverDuties { get; }

    public static Schedule Create(string name, Timetable timetable) =>
        new(name, timetable);

    private Schedule(string name, Timetable timetable)
    {
        Name = name;
        Timetable = timetable;
        LocoSchedules = [];
        TrainsetSchedules = [];
        DriverDuties = [];
    }

    public override string ToString() => Name;
}
```

#### TrainCategory.cs (Unified)
```csharp
namespace Tellurian.Trains.Model;

/// <summary>
/// Defines a category of trains with shared characteristics for identification and visualization.
/// Examples: "Passenger Express", "Regional", "Freight", "Service/Shunting"
/// </summary>
public record TrainCategory
{
    public int Id { get; init; }

    /// <summary>
    /// Prefix shown before train number (e.g., "P" for passenger, "G" for goods)
    /// Used in train identity display.
    /// </summary>
    public string Prefix { get; init; } = string.Empty;

    /// <summary>
    /// Optional suffix shown after train number.
    /// </summary>
    public string Suffix { get; init; } = string.Empty;

    /// <summary>
    /// Type of train (e.g., "Passenger", "Freight", "HighSpeed") used for translations.
    /// This is a resource key that can be localized.
    /// </summary>
    public required string ResourceName { get; init; }

    /// <summary>
    /// Color used when drawing timetable graphs and schematic train lines.
    /// Format: CSS color string (e.g., "#FF0000", "red", "rgb(255,0,0)")
    /// </summary>
    public string Color { get; init; } = "#000000";

    /// <summary>
    /// Display order for sorting categories in UI.
    /// </summary>
    public int DisplayOrder { get; init; }

    public override string ToString() => $"{Prefix} {Suffix}".Trim();
}
```

### 3.3 Operating Days / Sessions

Operating days/sessions belong to the **Timetable**, not the Layout. They are applied to **individual entities** (trains, loco schedules, trainset schedules, duties) within the timetable.

**Session concept:** A session is a logical operating period, independent of actual clock time. Trains can span midnight and still belong to the same session:
- Train 1: runs in session 4 (Thursday), starts 05:00, ends 07:00
- Train 2: runs in session 4 (Thursday), starts 23:00, ends 01:00 (Friday clock time, but still session 4)

Trains can start anytime from 00:00 to 23:59 within their session and may "spill over" into the next calendar day while remaining part of the original session.

#### OperatingSessions Class

We use a simple class-based approach with a list of session numbers (1-14). This is simpler and more readable than a bit-field approach.

| Advantage | Description |
|-----------|-------------|
| **Simplicity** | Easy to understand and debug - just a list of integers |
| **Readability** | `[1, 2, 3, 4, 5]` is immediately clear vs. bit manipulation |
| **Reusable patterns** | Static factory properties for common patterns (Daily, Odd, Even) |
| **Localization** | `OperatingDaysResourceKey` generates keys for translation |
| **Entity with Id** | Can be referenced and persisted as a first-class entity |

**Note:** `MaxSessions` is defined on the **Layout**, not the Timetable. Since Timetable references Layout, the actual sessions can be deduced from `timetable.Layout.MaxSessions`.

**Session configurations:**

| Configuration | MaxSessions | Example |
|---------------|-------------|---------|
| Bi-daily | 2 | Sessions 1 and 2 alternate |
| Weekly | 7 | Sessions 1-7 map to weekdays |
| Bi-weekly | 14 | Sessions 1-7 = week 1, 8-14 = week 2 |

**Bi-daily loco rotation example:**
```
Layout.MaxSessions = 2

Train 101: OperatingSessions = [1, 2]  → runs all sessions
LocoSchedule L1: OperatingSessions = [1]  → runs train 101 on odd sessions
LocoSchedule L2: OperatingSessions = [2]  → runs train 101 on even sessions
```

**Weekly with weekday mapping:**
```
Layout.MaxSessions = 7
Timetable.FirstOperatingWeekday = Monday

Session 1 = Monday, Session 2 = Tuesday, ..., Session 7 = Sunday

Pattern "Daily"    = [1, 2, 3, 4, 5, 6, 7]
Pattern "Weekdays" = [1, 2, 3, 4, 5]
Pattern "Weekend"  = [6, 7]

Train 101: OperatingSessions = [1, 2, 3, 4, 5] → runs Monday to Friday
Train 102: OperatingSessions = [1, 2, 3, 4, 5, 6, 7] → runs all 7 days
```

**Bi-weekly rotation:**
```
Layout.MaxSessions = 14

Pattern "Week1"   = [1, 2, 3, 4, 5, 6, 7]
Pattern "Week2"   = [8, 9, 10, 11, 12, 13, 14]
Pattern "AllDays" = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]

Train 101: OperatingSessions = [1, 2, 3, 4, 5, 6, 7] → runs only first week
Train 102: OperatingSessions = AllSessions → runs both weeks
LocoSchedule L1: [1, 2, 3] → sessions 1-3 only
LocoSchedule L2: [4, 5, 6, 7] → sessions 4-7 only
```

**On-demand trains:** Empty session list `[]` means "on demand" - train exists but is not scheduled for any session.

#### OperatingSessions.cs
```csharp
namespace Tellurian.Trains.Model;

/// <summary>
/// Represents a collection of operating sessions (1-14).
/// Entities reference this to specify which sessions they operate on.
/// </summary>
public class OperatingSessions(int id, IEnumerable<int> sessionNumbers)
{
    public int Id { get; init; } = id;
    private readonly IList<int> _sessions = [.. sessionNumbers.Where(s => s is >= 1 and <= 14).Distinct()];

    /// <summary>
    /// Gets the collection of session identifiers.
    /// Session numbers are in the range 1 to 14.
    /// </summary>
    public IEnumerable<int> Sessions => _sessions;
}

public static class OperatingSessionsExtensions
{
    extension(OperatingSessions operatingSessions)
    {
        /// <summary>On-demand (no scheduled sessions).</summary>
        public static OperatingSessions OnDemand => new(0, []);

        /// <summary>All 14 sessions.</summary>
        public static OperatingSessions AllSessions => new(1, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]);

        /// <summary>Odd sessions (1, 3, 5, 7, 9, 11, 13).</summary>
        public static OperatingSessions OddSessions => new(2, [1, 3, 5, 7, 9, 11, 13]);

        /// <summary>Even sessions (2, 4, 6, 8, 10, 12, 14).</summary>
        public static OperatingSessions EvenSessions => new(3, [2, 4, 6, 8, 10, 12, 14]);

        /// <summary>
        /// Returns a resource key for operating days description.
        /// </summary>
        /// <remarks>
        /// Examples: "Daily", "MondayToFriday", "Monday_Wednesday_Friday", "OnDemand".
        /// </remarks>
        public string OperatingDaysResourceKey =>
            operatingSessions.Sessions.Count() switch
            {
                0 => "OnDemand",
                14 => "Daily",
                _ when operatingSessions.Sessions.IsConsecutiveSessions() =>
                    $"{operatingSessions.Sessions.Min().DayName}To{operatingSessions.Sessions.Max().DayName}",
                _ => $"{string.Join("_", operatingSessions.Sessions.Select(s => s.DayName))}"
            };
    }

    extension(IEnumerable<int> sessionNumbers)
    {
        internal bool IsConsecutiveSessions()
        {
            var ordered = sessionNumbers.OrderBy(s => s).ToArray();
            if (ordered.Length == 0) return false;
            for (var i = 1; i < ordered.Length; i++)
            {
                if (ordered[i] != ordered[i - 1] + 1) return false;
            }
            return true;
        }
    }

    extension(int sessionNumber)
    {
        internal string DayName =>
            sessionNumber switch
            {
                1 => "Monday", 2 => "Tuesday", 3 => "Wednesday", 4 => "Thursday",
                5 => "Friday", 6 => "Saturday", 7 => "Sunday",
                8 => "Monday", 9 => "Tuesday", 10 => "Wednesday", 11 => "Thursday",
                12 => "Friday", 13 => "Saturday", 14 => "Sunday",
                _ => ""
            };
    }
}
```

**Note:** Entities (Train, VehicleSchedule, DriverDuty) reference an `OperatingSessions` instance to specify which sessions they run. Common patterns are defined as static factory properties with predefined IDs (0=OnDemand, 1=AllSessions, 2=OddSessions, 3=EvenSessions).

#### Train.cs (with operating sessions)
```csharp
namespace Tellurian.Trains.Model;

public record Train
{
    public int Id { get; init; }
    public int Number { get; init; }
    public required TrainCategory Category { get; init; }

    /// <summary>
    /// The operating sessions this train runs on.
    /// Reference to an OperatingSessions instance (use OnDemand for unscheduled trains).
    /// </summary>
    public required OperatingSessions OperatingSessions { get; init; }

    public string? ExternalId { get; init; }
    public string? OperatorName { get; init; }
    public string? OperatorSignature { get; init; }
    public int? MaxLength { get; init; }
    public IList<TrainCall> Calls { get; init; } = [];

    public string Identity => $"{Category.Prefix} {Number}{Category.Suffix}".Trim();
    public override string ToString() => Identity;
}
```

#### VehicleSchedule.cs
```csharp
namespace Tellurian.Trains.Model;

/// <summary>
/// Base class for locomotive and trainset rotations.
/// Part of Schedule (Step 3).
/// </summary>
public abstract record VehicleSchedule
{
    public int Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public ICollection<TrainPart> Parts { get; }

    protected VehicleSchedule(string number)
    {
        Number = number;
        Parts = [];
    }

    public override string ToString() => Number;
}

public sealed record LocoSchedule : VehicleSchedule
{
    public LocoSchedule(string number) : base(number) { }
}

public sealed record TrainsetSchedule : VehicleSchedule
{
    public TrainsetSchedule(string number) : base(number) { }
}
```

#### DriverDuty.cs
```csharp
namespace Tellurian.Trains.Model;

/// <summary>
/// Represents a driver's work assignment across train parts.
/// Part of Schedule (Step 3). References back to its parent Schedule.
/// </summary>
public class DriverDuty
{
    public int Id { get; }
    public string Identity { get; }
    public ICollection<TrainPart> Parts { get; }
    public ICollection<Note> Notes { get; }
    public Schedule Schedule { get; internal set; }

    public DriverDuty(string identity)
    {
        Identity = identity;
        Parts = [];
        Notes = [];
    }

    public override string ToString() => Identity;
}
```

**Example: Three-layer model creation**

```csharp
// Step 1: Create Layout (infrastructure)
var layout = new Layout { Name = "My Railway" };
layout.Stations.Add(new Station { Name = "Central", Signature = "C" });
layout.Stations.Add(new Station { Name = "North", Signature = "N" });
layout.TrackStretches.Add(new TrackStretch(layout.Station("C"), layout.Station("N"), 10.5, 1));

// Step 2: Create Timetable (trains) - references Layout
var timetable = new Timetable("Summer 2024", layout);
var passengerCategory = new TrainCategory { Prefix = "P", ResourceName = "Passenger" };
var train101 = new Train { Number = 101, Category = passengerCategory };
train101.Calls.Add(new StationCall { ... });
timetable.Add(train101);

// Step 3: Create Schedule (equipment + crew) - references Timetable
var schedule = Schedule.Create("Rotation A", timetable);
var locoL1 = new LocoSchedule("L1");
locoL1.Parts.Add(new TrainPart { Train = train101, From = ..., To = ... });
schedule.AddLocoSchedule(locoL1);

var duty1 = new DriverDuty("D1");
duty1.Add(new TrainPart { Train = train101, From = ..., To = ... });
schedule.AddDriverDuty(duty1);
```

**Navigation between layers:**

```csharp
// From Schedule, navigate up the hierarchy
var schedule = ...;
var timetable = schedule.Timetable;          // Parent timetable
var layout = schedule.Timetable.Layout;      // Grandparent layout
var trains = schedule.Timetable.Trains;      // Sibling trains
var stations = schedule.Timetable.Layout.Stations;  // Stations in layout

// Find which trains a loco covers
var locoSchedule = schedule.LocoSchedules.First();
var coveredTrains = locoSchedule.Parts.Select(p => p.Train).Distinct();

// Find which duties cover a specific train
var train = timetable.Trains.First();
var dutiesForTrain = schedule.DriverDuties
    .Where(d => d.Parts.Any(p => p.Train == train));
```

This three-layer structure allows:
- Multiple timetables for the same layout
- Multiple schedules for the same timetable (different rotations)
- Clear separation between infrastructure, operations, and assignments

#### Station.cs
```csharp
namespace Tellurian.Trains.Model;

/// <summary>
/// A station where trains can stop. Part of Layout.
/// </summary>
public record Station
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Signature { get; init; } = string.Empty;
    public bool IsShadow { get; init; }
    public Layout Layout { get; internal set; } = default!;
    public IList<StationTrack> Tracks { get; init; } = [];

    public override string ToString() => Name;
}
```

**Example: Dispatch-specific extension**
```csharp
namespace Tellurian.Trains.Dispatch;

/// <summary>
/// A manned station with dispatcher - Dispatch-specific extension.
/// </summary>
public record DispatchStation : Station
{
    public bool IsManned { get; init; } = true;
    public string? Dispatcher { get; init; }
    public string? PreferredLanguage { get; init; }
}
```

#### StationTrack.cs
```csharp
namespace Tellurian.Trains.Model;

/// <summary>
/// A track at a station where trains can stop.
/// </summary>
public sealed record StationTrack
{
    public int Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public bool IsMain { get; init; }
    public bool IsScheduled { get; init; } = true;
    public double Length { get; init; }
    public string Usage { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}
```

#### TrackStretch.cs
```csharp
namespace Tellurian.Trains.Model;

/// <summary>
/// A track connection between two stations.
/// </summary>
public class TrackStretch
{
    public int Id { get; init; }
    public Station Start { get; init; } = default!;
    public Station End { get; init; } = default!;
    public double Distance { get; init; }
    public int TracksCount { get; init; } = 1;
    public int Speed { get; init; }
    public int Time { get; init; }

    public TrackStretch(Station start, Station end, double distance, int tracksCount)
    {
        Start = start;
        End = end;
        Distance = distance;
        TracksCount = tracksCount;
    }
}
```

#### StationCall.cs
```csharp
namespace Tellurian.Trains.Model;

/// <summary>
/// A scheduled stop or pass-through at a station.
/// </summary>
public sealed record StationCall
{
    public int Id { get; init; }
    public StationTrack Track { get; init; } = default!;
    public int SequenceNumber { get; init; }
    public Time Arrival { get; init; }
    public Time Departure { get; init; }
    public bool IsArrival { get; init; } = true;
    public bool IsDeparture { get; init; } = true;
    public ICollection<Note> Notes { get; init; } = [];
}
```

### 3.3 Dispatch State Extension Pattern

Two approaches for extending the base model with state:

#### Option A: Subclassing (Recommended for Trains)
```csharp
namespace Tellurian.Trains.Dispatch.Trains;

public record DispatchTrain : Train
{
    public TrainState State { get; set; }
    public TrainState? PreviousState { get; private set; }

    public DispatchTrain(Train baseTrain) : base(baseTrain) { }

    // State management methods...
}
```

#### Option B: Composition/Wrapper (Recommended for Calls)
```csharp
namespace Tellurian.Trains.Dispatch.Trains;

public record TrainStationCallState
{
    public StationCall BaseCall { get; init; }          // Reference to base model
    public StationTrack? NewTrack { get; set; }         // Track change
    public CallTime Observed { get; set; }              // Actual times
}
```

### 3.4 ID Generation Strategy

**Principle:** ID assignment is delegated to the import module, not the base model. Different data sources handle IDs differently:

| Data Source | ID Availability | ID Strategy |
|-------------|-----------------|-------------|
| **Access Database** | IDs present | Use actual database IDs as-is |
| **Module Registry** | IDs present | Use actual registry IDs as-is (layout data only) |
| **XPLN Spreadsheets** | No IDs | Generate deterministic IDs from natural keys |

This delegation ensures that:
- Sources with actual IDs preserve referential integrity with the original system
- Sources without IDs get consistent, reproducible IDs across imports
- The base model remains agnostic to ID origin

#### Sources with Actual IDs (Access, Module Registry)

The **Access** importer reads IDs directly from the database tables. These IDs should be used as-is to maintain consistency with the Access database.

The **Module Registry** is an online database that provides layout data (stations, track stretches) with actual IDs. When importing layout data from the Module Registry, use those IDs directly. Note: The Module Registry provides layout/infrastructure data only, not timetable data.

```csharp
// Access importer - use database IDs directly
station.Id = reader.GetInt32("StationId");
trackStretch.Id = reader.GetInt32("StretchId");

// Module Registry importer - use registry IDs directly
station.Id = registryStation.Id;
trackStretch.Id = registryStretch.Id;
```

#### Sources without IDs (XPLN)

For XPLN spreadsheets where no IDs exist in the data, generate deterministic IDs using a hash-based approach:

```csharp
public static class IdGenerator
{
    /// <summary>
    /// Generates a deterministic ID from a natural key.
    /// Same input always produces the same ID.
    /// </summary>
    public static int FromKey(params string[] keyParts)
    {
        var combined = string.Join("|", keyParts);
        return combined.GetDeterministicHashCode();
    }

    // Extension for deterministic hashing (consistent across runs)
    private static int GetDeterministicHashCode(this string str)
    {
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;
            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }
            return hash1 + (hash2 * 1566083941);
        }
    }
}

// XPLN importer - generate deterministic IDs from natural keys:

// Step 1: Layout IDs (infrastructure)
var layoutId = IdGenerator.FromKey(layoutName);
var locationId = IdGenerator.FromKey(layoutName, locationSignature);
var locationTrackId = IdGenerator.FromKey(layoutName, locationSignature, trackNumber);
var trackStretchId = IdGenerator.FromKey(layoutName, startLocationSignature, endLocationSignature);

// Step 2: Timetable IDs (operational plan - uses timetableName as scope)
var timetableId = IdGenerator.FromKey(layoutName, timetableName);
var categoryId = IdGenerator.FromKey(timetableName, categoryName);  // e.g., "Summer2024", "Pt"
var operatingSessionsId = IdGenerator.FromKey(timetableName, string.Join(",", sessionNumbers));  // e.g., "1,2,3,4,5"
var trainId = IdGenerator.FromKey(timetableName, trainNumber.ToString());
var trainCallId = IdGenerator.FromKey(timetableName, trainNumber.ToString(), sequenceNumber.ToString());
var dutyId = IdGenerator.FromKey(timetableName, dutyIdentity);
var locoScheduleId = IdGenerator.FromKey(timetableName, "loco", scheduleNumber);
```

#### Mixed-Source Imports

A consuming system might combine data from multiple sources. For example:
- Layout data from Module Registry (with registry IDs)
- Timetable data from XPLN (with generated IDs)

In such cases, each importer maintains responsibility for its own ID assignment. The consuming system must handle potential ID conflicts if combining data from multiple sources into a single datastore.

---

## 4. Migration Plan

The migration follows a safe, incremental approach: refactor in place first, validate with existing tests, then extract to a separate package.

### Phase 1: Refactor Existing Model In Place ✅ COMPLETED

Refactor `Tellurian.Trains.Schedules.Importers.Model` to align with the three-layer unified model design:

1. ✅ **Verify three-layer structure exists**:
   - `Layout` → Infrastructure (Stations, TrackStretches, TimetableStretches)
   - `Timetable` → Trains (references Layout, contains Trains)
   - `Schedule` → Equipment + Crew (references Timetable, contains LocoSchedules, TrainsetSchedules, DriverDuties)

2. ✅ **Verify Layout contains only infrastructure**:
   - `Station`, `StationTrack`, `TrackStretch`, `TimetableStretch`
   - No train-related collections

3. ✅ **Verify Timetable references Layout and contains only trains**:
   - `Layout` reference (required for validation)
   - `Trains` collection
   - No vehicle schedules or driver duties

4. ✅ **Verify Schedule references Timetable and contains assignments**:
   - `Timetable` reference (required for validation)
   - `LocoSchedules`, `TrainsetSchedules`, `DriverDuties` collections

5. ✅ **Add OperatingSessions class**:
   - Class with `Id` and `IList<int>` of session numbers (1-14)
   - Static factory properties: `OnDemand`, `AllSessions`, `OddSessions`, `EvenSessions`
   - Extension methods for `OperatingDaysResourceKey` (localization support)

6. ✅ **Add TrainCategory class** with:
   - `Prefix`, `Suffix` for identity display
   - `ResourceName` for localization (e.g., "Passenger", "Freight")
   - `Color` for timetable graphs

### Phase 2: Update Importers and Validate 🔄 IN PROGRESS

Update Access and XPLN importers to work with three-layer model:

1. **Update XPLN importer**:
   - ✅ Parse composite train identity (e.g., "Gt1234") into category + number
   - ✅ Create `TrainCategory` for each unique prefix/suffix (+ background color extraction)
   - ✅ Generate deterministic IDs using rowNumber
   - ✅ Implement three-step import: Layout → Timetable → Schedule
   - N/A ~~Map XPLN session columns to `OperatingSessions`~~ - XPLN has no session columns; schedules are single-session

2. **Update Access importer**:
   - ⬜ Use actual database IDs
   - ⬜ Map existing database structure to three-layer model
   - ⬜ Align with new `TrainCategory` and `OperatingSessions` classes

3. ✅ **Run all tests** and fix any failures:
   - `dotnet test Model.Tests` ✅
   - `dotnet test Interfaces.Tests` ✅
   - `dotnet test Xpln.Tests` ✅
   - `dotnet test Services.Tests` ✅ (new)

4. **Iterate** until all importer updates are complete

### Phase 3: Extract Model to Separate Solution 📋 NOT STARTED

Once tests pass and importers work correctly:

1. ⬜ **Create new solution** `Tellurian.Trains.Model`:
   - Move model classes from `Tellurian.Trains.Schedules.Importers.Model`
   - Move corresponding tests to `Tellurian.Trains.Model.Tests`

2. ⬜ **Create NuGet package** `Tellurian.Trains.Model`:
   - Configure package metadata
   - Publish to NuGet.org

3. ⬜ **Update Schedule.Importers**:
   - Remove local model project
   - Add NuGet reference to `Tellurian.Trains.Model`
   - Verify all tests still pass

### Phase 4: Refactor Dispatch Model 📋 NOT STARTED

1. ⬜ Add NuGet reference to `Tellurian.Trains.Model`
2. ⬜ Create Dispatch-specific extensions of base model:
   - `DispatchStation : Station` (adds IsManned, Dispatcher, PreferredLanguage)
   - `SignalControlledPlace` (Dispatch-specific location type)
   - `OtherPlace` (Dispatch-specific location type)
3. ⬜ Create state extension classes:
   - `DispatchTrain` extending or wrapping `Train`
   - `StationCallState` wrapping `StationCall` with observed times
4. ⬜ Keep Dispatch-specific classes: `TrainSection`, `DispatchStretch`

### Phase 5: Create Import-to-Dispatch Adapter 📋 NOT STARTED

```csharp
namespace Tellurian.Trains.Dispatch.Import;

public class ScheduleImportAdapter
{
    public DispatchLayout FromLayout(Layout layout)
    {
        // Convert Station -> DispatchStation with default dispatcher settings
        // Convert TrackStretch -> Dispatch TrackStretch with capacity tracking
        // Convert TimetableStretch -> DispatchStretch with direction handling
    }

    public IEnumerable<DispatchTrain> FromTimetable(Timetable timetable)
    {
        // Convert Train -> DispatchTrain with initial state
        // Convert StationCall -> StationCallState with scheduled times
    }

    public void ApplySchedule(Schedule schedule, IEnumerable<DispatchTrain> trains)
    {
        // Associate loco schedules with trains
        // Associate driver duties with trains
    }
}
```

### Phase 6: Integrate with Broker 📋 NOT STARTED

1. ⬜ Broker accepts imported data through adapters (all three layers)
2. ⬜ State provider persists only state deltas (TrainState, DispatchState, observed times)
3. ⬜ On reload, base data is re-imported and state is reapplied

---

## 5. Benefits of Unified Model

1. **Three-Layer Hierarchy:** Clear separation between infrastructure (Layout), operations (Timetable), and assignments (Schedule)
2. **Planned Schedule as Contract:** The three layers represent the complete output of schedule planning, serving as a stable contract between planners and operators
3. **Single Source of Truth:** Layout, timetable, and schedule data defined once, consumed by multiple systems
4. **Flexible Composition:** Multiple timetables per layout, multiple schedules per timetable
5. **Deterministic IDs:** Same import always produces same IDs, enabling state correlation across sessions
6. **Clear Separation of Concerns:**
   - Planned data (static): what *should* happen
   - Runtime state (dynamic): what *is* happening / *did* happen
7. **Reusable Import Logic:** Schedule.Importers can feed Dispatch, publishing, simulation, or any future consumer
8. **Simplified State Persistence:** Only runtime deltas need saving; planned data can always be re-imported
9. **Type Safety:** Strong typing prevents accidental mixing of planned and runtime data
10. **Independent Evolution:** Planning tools and consuming systems can evolve independently as long as they respect the three-layer contract

---

## 6. Open Questions

1. **Should Dispatch extend Station or use composition?**
   - Recommendation: Extend `Station` with `DispatchStation` adding IsManned, Dispatcher, PreferredLanguage.

2. **How to handle bi-directional references (e.g., Station.Layout)?**
   - Recommendation: Use object references where needed; the three-layer structure naturally supports navigation up the hierarchy.

3. **Should `TrainPart` be in base model or Schedule only?**
   - Recommendation: Keep in Schedule layer. It links trains to vehicle schedules and duties.

4. **How to handle Dispatch-specific track features (direction, occupancy)?**
   - Recommendation: Dispatch wraps base `TrackStretch` with `DispatchTrackStretch` adding runtime features.

---

## 7. EF Core and JSON Serialization Compatibility Analysis

> **Last Updated:** 2025-12-29

This section analyzes what adaptations are required to make the Model suitable for:
1. **Entity Framework Core** (EF Core) for database read/save operations
2. **JSON serialization** (System.Text.Json) for API/storage purposes

### 7.1 Executive Summary

**Key Finding: NOT Mutually Exclusive**

EF Core and JSON serialization requirements are **compatible** and can coexist in the same model with minimal modifications. The model can be adapted to support both without requiring DTOs or separate mapping layers.

| Aspect | Compatibility | Notes |
|--------|---------------|-------|
| Core design patterns | ✅ Compatible | Both work with modern C# features |
| Nullable reference types | ✅ Same semantics | Both use nullability for required/optional |
| Constructors | ✅ Compatible | Private parameterless constructor works for both |
| Required properties | ✅ Compatible | C# `required` modifier works for both |
| Circular references | ⚠️ Config required | EF natural, JSON needs `ReferenceHandler` |
| Foreign keys | ⚠️ Additive change | EF prefers explicit FK properties |

---

### 7.2 Current Model Patterns Analysis

The current model uses several patterns that affect EF Core and JSON compatibility:

#### **Pattern: Primary Constructors with Required Properties**
```csharp
// Current: Train.cs
[method: SetsRequiredMembers]
public class Train(int id, TrainCategory category, int number, string externalId = "")
{
    public required int Id { get; init; } = id;
    public required TrainCategory Category { get; init; } = category;
}
```

| Framework | Compatibility | Notes |
|-----------|---------------|-------|
| EF Core | ⚠️ Works | EF Core 7+ can bind constructor params to properties |
| JSON | ✅ Works | `[JsonConstructor]` or primary constructor supported |

#### **Pattern: Parent Reference with `= default!`**
```csharp
// Current: OperationLocation.cs
public Layout Layout { get; internal set; } = default!;

// Current: TrainPart.cs
#pragma warning disable CS8618
public VehicleSchedule Schedule { get; internal set; }
```

| Framework | Compatibility | Notes |
|-----------|---------------|-------|
| EF Core | ⚠️ Works | EF sets after construction; requires configuration |
| JSON | ⚠️ Works | Needs `[JsonInclude]` for internal setters |

#### **Pattern: Private Backing Field with Public Property**
```csharp
// Current: StationCall.cs
public Train Train { get => _train; init => _train = value; }
private Train _train = default!;
internal void SetTrain(Train train) => _train = train;
```

| Framework | Compatibility | Notes |
|-----------|---------------|-------|
| EF Core | ⚠️ Config | Use `.HasField("_train")` in model builder |
| JSON | ⚠️ Works | Property with init setter is serializable |

#### **Pattern: Navigation-Only Relationships (No FK Properties)**
```csharp
// Current: Train.cs - no TimetableId property
public Timetable? Timetable { get; internal set; }

// Current: StationCall.cs - no TrackId property
public StationTrack Track { get; init; }
```

| Framework | Compatibility | Notes |
|-----------|---------------|-------|
| EF Core | ⚠️ Shadow FK | EF creates shadow FK; explicit preferred |
| JSON | ✅ Works | Navigations serialize fine |

---

### 7.3 Entity Framework Core Requirements

#### **7.3.1 Constructor Requirements**

EF Core can instantiate entities via:
1. **Parameterless constructor** (preferred) - public, protected, or private
2. **Parameterized constructor** - if params match property names exactly

**Current Issue:** Primary constructors with `required` properties work but are complex.

**Recommendation:** Add private parameterless constructor for EF Core:
```csharp
public class Train : IEquatable<Train>
{
    // EF Core constructor (private)
    private Train() { }

    // Application constructor
    [method: SetsRequiredMembers]
    public Train(int id, TrainCategory category, int number, string externalId = "")
    {
        Id = id;
        Category = category;
        Number = number;
        ExternalId = externalId;
    }

    public required int Id { get; init; }
    // ... rest unchanged
}
```

#### **7.3.2 Foreign Key Properties**

EF Core **strongly prefers** explicit FK properties alongside navigation properties:
- Enables more efficient queries
- Required for serialization round-trips
- Makes relationships explicit in code

**Current Issue:** Model uses navigation-only relationships.

**Recommendation:** Add FK properties for key relationships:
```csharp
public class Train
{
    public int? TimetableId { get; set; }          // FK property (added)
    public Timetable? Timetable { get; set; }      // Navigation property
}

public sealed record StationCall
{
    public int TrackId { get; init; }              // FK property (added)
    public StationTrack Track { get; init; }       // Navigation property

    public int TrainId { get; init; }              // FK property (added)
    public Train Train { get; init; }              // Navigation property
}

public sealed record TrainPart
{
    public int ScheduleId { get; init; }           // FK property (added)
    public VehicleSchedule Schedule { get; set; }  // Navigation property

    public int FromId { get; init; }               // FK property (added)
    public StationCall From { get; init; }         // Navigation property

    public int ToId { get; init; }                 // FK property (added)
    public StationCall To { get; init; }           // Navigation property
}
```

#### **7.3.3 Navigation Property Setters**

EF Core requires writable navigation properties (can be private/internal):
```csharp
// ✅ Good - has setter
public Timetable? Timetable { get; internal set; }

// ❌ Bad - init-only doesn't work for EF change tracking
public Timetable? Timetable { get; init; }
```

**Current Issue:** Some properties use `init` that EF Core can't update after construction.

**Recommendation:** Change `init` to `set` (or `internal set`) for FK/navigation properties:
```csharp
public StationTrack Track { get; set; }  // Changed from init
```

#### **7.3.4 Record Types Consideration**

| Type | EF Core Suitability | Notes |
|------|---------------------|-------|
| `class` | ✅ Preferred | Reference equality, mutable state |
| `record` | ⚠️ Works | Value equality can confuse change tracking |
| `record struct` | ❌ Poor | Not recommended for entities |

**Current State:** Mix of `class` and `record` types.

**Recommendation:** Consider converting core entities to `class`:
- `Train` - already a class ✅
- `StationCall` - currently a record, consider `class`
- `OperationLocation` - currently a record, consider `class`
- `TrainPart` - currently a record, consider `class`

Records are fine for value objects like `Time`, `Company`, `TrainCategory`.

#### **7.3.5 Required EF Core Model Configuration**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configure backing fields where needed
    modelBuilder.Entity<StationCall>()
        .Property(e => e.Train)
        .HasField("_train");

    // Configure internal setters
    modelBuilder.Entity<OperationLocation>()
        .Property(e => e.Layout)
        .UsePropertyAccessMode(PropertyAccessMode.Field);

    // Configure relationships with FKs
    modelBuilder.Entity<Train>()
        .HasOne(t => t.Timetable)
        .WithMany(tt => tt.Trains)
        .HasForeignKey(t => t.TimetableId);

    // Configure inheritance for VehicleSchedule
    modelBuilder.Entity<VehicleSchedule>()
        .HasDiscriminator<string>("ScheduleType")
        .HasValue<LocoSchedule>("Loco")
        .HasValue<TrainsetSchedule>("Trainset");
}
```

---

### 7.4 JSON Serialization Requirements (System.Text.Json)

#### **7.4.1 Constructor Support**

System.Text.Json supports:
1. **Parameterless constructor** (default)
2. **Single parameterized constructor** (auto-detected for classes)
3. **`[JsonConstructor]` attribute** for multiple constructors
4. **Records** with primary constructors (excellent support)

**Current State:** Primary constructors work well.

**Recommendation:** Add `[JsonConstructor]` where multiple constructors exist:
```csharp
public class Train
{
    private Train() { }  // EF Core - not used by JSON

    [JsonConstructor]
    public Train(int id, TrainCategory category, int number, string externalId = "")
    { }
}
```

#### **7.4.2 Circular Reference Handling**

**Problem:** Bidirectional relationships cause infinite serialization loops:
```
Train → Timetable → Trains → Train → ...
StationCall → Train → Calls → StationCall → ...
```

**Solution Options:**

| Option | Pros | Cons |
|--------|------|------|
| `ReferenceHandler.Preserve` | Full round-trip fidelity | Adds `$id`/`$ref` metadata |
| `ReferenceHandler.IgnoreCycles` | Clean JSON, no metadata | Nulls circular refs (data loss) |
| `[JsonIgnore]` on parent refs | Clean JSON, explicit control | Manual maintenance |

**Recommendation:** Use `[JsonIgnore]` on parent navigation properties:
```csharp
public sealed record StationCall
{
    [JsonIgnore]
    public Train Train { get; init; }   // Parent - ignore to break cycle

    public int TrainId { get; init; }   // FK - include for relationships
}

public class Train
{
    [JsonIgnore]
    public Timetable? Timetable { get; set; }  // Parent - ignore

    public int? TimetableId { get; set; }       // FK - include
}

public sealed record TrainPart
{
    [JsonIgnore]
    public VehicleSchedule Schedule { get; set; }  // Parent - ignore

    public int ScheduleId { get; init; }           // FK - include
}
```

This pattern:
- Serializes FK values for relationship reconstruction
- Avoids circular reference issues
- Keeps JSON payload clean
- Parent references can be restored during deserialization

#### **7.4.3 Internal Setter Support**

Properties with `internal set` need `[JsonInclude]` for serialization:
```csharp
public sealed record OperationLocation
{
    [JsonInclude]
    public Layout Layout { get; internal set; } = default!;
}
```

**Alternatively**, change to private set with JsonSerializer options:
```csharp
var options = new JsonSerializerOptions
{
    IncludeFields = false,
    PropertyNameCaseInsensitive = true
};
```

#### **7.4.4 Required Properties**

Both C# `required` and `[JsonRequired]` work:
```csharp
public required int Id { get; init; }                    // ✅ Works
[JsonRequired] public int Number { get; init; }           // ✅ Also works
```

In .NET 9+, set `RespectRequiredConstructorParameters = true` for constructor params.

#### **7.4.5 Polymorphism (Inheritance)**

For `VehicleSchedule` → `LocoSchedule`/`TrainsetSchedule`:
```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(LocoSchedule), "loco")]
[JsonDerivedType(typeof(TrainsetSchedule), "trainset")]
public abstract record VehicleSchedule
{
    // ...
}
```

---

### 7.5 Nullable Reference Types Impact

C# nullable reference types work consistently for both frameworks:

| Declaration | EF Core | JSON | Database |
|-------------|---------|------|----------|
| `string Name` | Non-nullable column | Required in JSON | NOT NULL |
| `string? Name` | Nullable column | Optional in JSON | NULL |
| `Train Train` | Required relationship | Required in JSON | FK NOT NULL |
| `Train? Train` | Optional relationship | Optional in JSON | FK NULL |

**The model's current nullable annotations are correct and work for both.**

---

### 7.6 Comparison: DTOs vs Direct Model Usage

#### **Option A: Direct Model Usage (Recommended)**

Modify the Model classes to work with both EF Core and JSON:

| Pros | Cons |
|------|------|
| No mapping code | Model has framework-specific attributes |
| Single source of truth | Slightly more complex model |
| Less maintenance | EF Core and JSON concerns mixed |
| Better performance | |

**Required Changes:**
1. Add FK properties alongside navigation properties
2. Add private parameterless constructors
3. Add `[JsonIgnore]` on parent navigation properties
4. Add `[JsonInclude]` on internal-set properties
5. Consider converting some records to classes

#### **Option B: Separate DTOs**

Create separate DTO classes for JSON serialization:

| Pros | Cons |
|------|------|
| Clean separation of concerns | Mapping overhead |
| Model stays framework-agnostic | Duplicate type definitions |
| Different shapes for different uses | More maintenance |
| | Potential sync issues |

**Not recommended** for this model because:
- The domain model IS the data contract
- Mapping adds complexity without clear benefit
- The modifications needed are minimal

---

### 7.7 Recommended Model Modifications

#### **7.7.1 Example: Modified Train Class**

```csharp
public class Train : IEquatable<Train>
{
    // Private constructor for EF Core
    private Train()
    {
        Calls = new List<StationCall>();
        Groups = new List<string>();
    }

    // Application constructor
    [SetsRequiredMembers]
    [JsonConstructor]
    public Train(int id, TrainCategory category, int number, string externalId = "")
    {
        Id = id;
        Category = category;
        Number = number;
        ExternalId = externalId;
        Calls = new List<StationCall>();
        Groups = new List<string>();
    }

    public required int Id { get; init; }
    public required int Number { get; init; }
    public string ExternalId { get; init; } = "";
    public string? Remark { get; init; }
    public TrainLenght Length { get; set; }
    public Company Company { get; set; } = Company.None;
    public required TrainCategory Category { get; init; }
    public required Sessions Sessions { get; set; } = Sessions.All;
    public IList<string> Groups { get; init; }

    // FK property for EF Core
    public int? TimetableId { get; set; }

    // Navigation property - ignored in JSON to prevent cycles
    [JsonIgnore]
    public Timetable? Timetable { get; set; }

    // Child collection - included in JSON
    public IList<StationCall> Calls { get; }

    // ... rest unchanged
}
```

#### **7.7.2 Example: Modified StationCall Class**

```csharp
public class StationCall : IEquatable<StationCall>, IComparable<StationCall>
{
    // Private constructor for EF Core
    private StationCall()
    {
        Notes = new List<Note>();
    }

    // Application constructor
    [JsonConstructor]
    public StationCall(int id, int trackId, Time arrival, Time departure, string? remark = null)
    {
        Id = id;
        TrackId = trackId;
        Arrival = arrival;
        Departure = departure;
        Notes = new List<Note>();
        // ... remark handling
    }

    public int Id { get; init; }

    // FK properties
    public int TrackId { get; init; }
    public int TrainId { get; set; }

    // Navigation properties - parent ignored for JSON
    [JsonIgnore]
    public StationTrack Track { get; set; } = null!;

    [JsonIgnore]
    public Train Train { get; set; } = null!;

    // Computed property - not persisted
    [JsonIgnore]
    [NotMapped]
    public OperationLocation Station => Track.Station;

    public Time Arrival { get; init; }
    public Time Departure { get; init; }
    public bool IsArrival { get; set; }
    public bool IsDeparture { get; set; }
    public ICollection<Note> Notes { get; }

    // ... rest unchanged
}
```

---

### 7.8 Summary: Compatibility Matrix

| Current Pattern | EF Core Change | JSON Change |
|-----------------|----------------|-------------|
| Primary constructor | Add private parameterless ctor | Add `[JsonConstructor]` if needed |
| `init` properties | Change to `set` for FKs/navs | ✅ No change |
| `= default!` | Configure backing field access | Add `[JsonInclude]` |
| `internal set` | ✅ No change | Add `[JsonInclude]` |
| Navigation-only | Add FK properties | Add FK properties |
| Bidirectional refs | ✅ Natural | Add `[JsonIgnore]` on parents |
| Records | Consider `class` for entities | ✅ No change |
| Nullable types | ✅ No change | ✅ No change |
| Required modifier | ✅ No change | ✅ No change |
| Abstract base (`VehicleSchedule`) | Configure discriminator | Add `[JsonPolymorphic]` |

---

### 7.9 Conclusion

**The Model can support both EF Core and JSON serialization without DTOs or mapping layers.**

Key modifications:
1. **Add FK properties** - Essential for both EF Core efficiency and JSON relationship preservation
2. **Add private parameterless constructors** - Required for EF Core instantiation
3. **Use `[JsonIgnore]` on parent references** - Breaks circular reference chains
4. **Use `[JsonInclude]` for internal setters** - Enables JSON serialization of internal properties
5. **Consider `class` over `record`** - For entities that EF Core tracks

These changes are **additive** and **non-breaking** for existing import functionality.

---

### 7.10 Impact on XplnImporter and AccessRepository

This section analyzes how the proposed model modifications affect the existing importers.

#### **7.10.1 Current Import Patterns**

Both importers follow a similar pattern:

| Pattern | XplnImporter | AccessRepository |
|---------|--------------|------------------|
| Constructor calls | Parameterized constructors | Parameterized constructors |
| Relationship setup | Extension methods (`timetable.Add(train)`) | Extension methods |
| Property setting | Object initializers | Object initializers |
| FK values | Not used (navigations only) | Not used (navigations only) |

**Example: Current Train Creation (XplnDataImporter.cs:509-512)**
```csharp
static Train CreateTrain(int rowNumber, string[] fields, TrainCategory category)
{
    return new(rowNumber, category, fields[Object].NumberOrZero, fields[Object])
    { Remark = fields[Remark] };
}
```

**Example: Current Relationship Setup (TimetableExtensions.cs:35-45)**
```csharp
public static Train Add(this Timetable timetable, Train train)
{
    train.Timetable = timetable;  // Sets navigation property
    timetable.Trains.Add(train);
    return train;
}
```

#### **7.10.2 Impact of Each Proposed Change**

##### **Adding FK Properties (`TimetableId`, `TrackId`, etc.)**

**Impact: MINIMAL - Optional use only**

The FK properties would be:
- **Optional during import** - Navigation properties handle relationships
- **Auto-populated by EF Core** when saving (if using EF Core)
- **Useful for JSON round-trips** - Can serialize/deserialize relationships

Current code continues to work unchanged:
```csharp
timetable.Add(train);  // Sets train.Timetable = timetable (FK not required)
```

##### **Adding Private Parameterless Constructors**

**Impact: NONE**

```csharp
public class Train
{
    private Train() { }  // EF Core only - importers never call this

    // Importers continue using this constructor:
    public Train(int id, TrainCategory category, int number, string externalId = "")
    { ... }
}
```

Importers don't need to change - they use the existing public constructors.

##### **Changing `init` to `set` for Navigation Properties**

**Impact: NONE or POSITIVE**

Current patterns already work because relationships are established via extension methods.
Changing `init` to `set` actually **helps** because some properties need modification after
initial creation (e.g., `Train.Company` is set later during locomotive processing in XplnImporter).

##### **Adding `[JsonIgnore]` / `[JsonInclude]` Attributes**

**Impact: NONE**

These attributes only affect JSON serialization. Import code doesn't use reflection or care about attributes.

##### **Converting Records to Classes**

**Impact: MINIMAL - Test adjustments may be needed**

| Entity | Change | Import Impact |
|--------|--------|---------------|
| `StationCall` | record → class | None - same constructor |
| `OperationLocation` | record → class | None - same constructor |
| `TrainPart` | record → class | None - same constructor |

**Potential test impact:** If tests rely on value equality for records, they may need adjustment.
However, the model already implements `IEquatable<T>` on most types, so impact should be minimal.

#### **7.10.3 Required Changes Summary**

##### **XplnImporter**

| File | Required Changes |
|------|------------------|
| `XplnDataImporter.cs` | **None** |
| All extension files | **None** |

##### **AccessRepository**

| File | Required Changes |
|------|------------------|
| `AccessRepository.cs` | **None** for import |
| `Stations.cs` | **None** |
| `Trains.cs` | **None** |
| All other files | **None** |

#### **7.10.4 Required: Populate FK Properties for JSON Serialization**

**Important:** If the model will be serialized to JSON (with `[JsonIgnore]` on navigation properties
to break circular references), then FK properties **MUST** be set during import.

**Why this is required:**

When serializing to JSON with `[JsonIgnore]` on parent navigation properties:
- Navigation properties are excluded from JSON output
- Only FK properties (e.g., `TimetableId`) are serialized
- If FKs are not set, they default to `0` or `null`
- Deserialization cannot reconstruct relationships without FK values

**Example problem without FK assignment:**
```json
{
  "id": 1,
  "number": 101,
  "timetableId": 0,    // ❌ NOT SET - relationship lost!
  "calls": [...]
}
```

**Required extension method updates:**

```csharp
// Timetable.cs - REQUIRED change
public static Train Add(this Timetable timetable, Train train)
{
    train.Timetable = timetable;
    train.TimetableId = timetable.Id;  // ✅ REQUIRED for JSON round-trip
    timetable.Trains.Add(train);
    return train;
}

// Train.cs - REQUIRED change
public static StationCall Add(this Train train, StationCall call)
{
    call.SetTrain(train);
    call.TrainId = train.Id;           // ✅ REQUIRED for JSON round-trip
    train.Calls.Add(call);
    return call;
}
```

**Complete list of extension methods requiring FK assignment:**

| Extension Method | File | FK Property to Set |
|------------------|------|-------------------|
| `Layout.Add(OperationLocation)` | Layout.cs | `station.LayoutId = layout.Id` |
| `OperationLocation.Add(StationTrack)` | OperationLocation.cs | `track.StationId = station.Id` |
| `Timetable.Add(Train)` | Timetable.cs | `train.TimetableId = timetable.Id` |
| `Train.Add(StationCall)` | Train.cs | `call.TrainId = train.Id` |
| `StationCall` constructor | StationCall.cs | `TrackId = track.Id` (in constructor) |
| `VehicleSchedule.Add(TrainPart)` | VehicleSchedule.cs | `part.ScheduleId = schedule.Id` |
| `DriverDuty.Add(TrainPart)` | DriverDuty.cs | `part.DutyId = duty.Id` |
| `Schedule.AddLocoSchedule()` | Schedule.cs | `loco.ScheduleId = schedule.Id` |
| `Schedule.AddTrainsetSchedule()` | Schedule.cs | `trainset.ScheduleId = schedule.Id` |
| `Schedule.AddDriverDuty()` | Schedule.cs | `duty.ScheduleId = schedule.Id` |
| `TrainPart` constructor | TrainPart.cs | `FromId = from.Id`, `ToId = to.Id` |

**Note:** These changes apply to both XplnImporter and AccessRepository since both use the
same extension methods in the Model project.

#### **7.10.5 Conclusion: Importer Compatibility**

| Change Category | Importer Impact | Action Required |
|-----------------|-----------------|-----------------|
| Add FK properties | Low | Update extension methods |
| Private parameterless ctor | None | None |
| `init` → `set` | Positive | None |
| JSON attributes | None | None |
| Records → Classes | Minimal | Test review |
| **FK assignment in extensions** | **Required** | **Update all Add() methods** |

**Summary:**
- The model modifications themselves are backward-compatible
- **However**, if JSON serialization is required, the extension methods in the Model project
  **must be updated** to set FK properties alongside navigation properties
- This is a **one-time change** in the Model project that benefits all importers
- Without this change, JSON serialization would lose relationship information

---

## 8. Conclusion

> **Current Status:** Phase 1 is complete. Phase 2 (importer updates) is in progress. The model classes are implemented and all tests pass.

The unified model uses a **three-layer hierarchy** that establishes clear boundaries:

| Layer | Content | Purpose |
|-------|---------|---------|
| **Layout** | Stations, TrackStretches, TimetableStretches | Physical infrastructure |
| **Timetable** | Trains, StationCalls, TrainCategories | Planned train services |
| **Schedule** | LocoSchedules, TrainsetSchedules, DriverDuties | Equipment and crew assignments |

Each layer references its parent:
```
Layout
   ↑
Timetable ─── references Layout
   ↑
Schedule ─── references Timetable
```

This separation enables:

- **Schedule.Importers** produces all three layers with deterministic IDs
- **Dispatch** (and other consumers) layer their operational state on top
- **Flexible composition:** Multiple timetables per layout, multiple schedules per timetable
- **State persistence** stores only runtime deltas; planned data can always be re-imported
- **Multiple consumers** can use the same planned data for different purposes

The migration is being done incrementally:
1. ✅ Phase 1: Three-layer structure implemented in `Tellurian.Trains.Model`
2. 🔄 Phase 2: Update XPLN and Access importers to use new model classes
3. 📋 Phase 3: Extract model to separate NuGet package
4. 📋 Phase 4-6: Dispatch integration, adapters, and broker integration

**Next Steps:**
- Phase 2 XPLN importer updates are complete
- Remaining: Update Access importer to use new model classes (can be deferred)
- Ready for Phase 3: Extract model to separate NuGet package
