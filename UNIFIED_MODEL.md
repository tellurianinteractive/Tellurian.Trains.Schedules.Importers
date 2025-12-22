# Unified Domain Model Analysis and Plan

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

### Phase 1: Refactor Existing Model In Place

Refactor `Tellurian.Trains.Schedules.Importers.Model` to align with the three-layer unified model design:

1. **Verify three-layer structure exists**:
   - `Layout` → Infrastructure (Stations, TrackStretches, TimetableStretches)
   - `Timetable` → Trains (references Layout, contains Trains)
   - `Schedule` → Equipment + Crew (references Timetable, contains LocoSchedules, TrainsetSchedules, DriverDuties)

2. **Verify Layout contains only infrastructure**:
   - `Station`, `StationTrack`, `TrackStretch`, `TimetableStretch`
   - No train-related collections

3. **Verify Timetable references Layout and contains only trains**:
   - `Layout` reference (required for validation)
   - `Trains` collection
   - No vehicle schedules or driver duties

4. **Verify Schedule references Timetable and contains assignments**:
   - `Timetable` reference (required for validation)
   - `LocoSchedules`, `TrainsetSchedules`, `DriverDuties` collections

5. **Add OperatingSessions class** (if not exists):
   - Class with `Id` and `IList<int>` of session numbers (1-14)
   - Static factory properties: `OnDemand`, `AllSessions`, `OddSessions`, `EvenSessions`
   - Extension methods for `OperatingDaysResourceKey` (localization support)

6. **Add TrainCategory class** with:
   - `Prefix`, `Suffix` for identity display
   - `ResourceName` for localization (e.g., "Passenger", "Freight")
   - `Color` for timetable graphs

### Phase 2: Update Importers and Validate

Update Access and XPLN importers to work with three-layer model:

1. **Update XPLN importer**:
   - Parse composite train identity (e.g., "Gt1234") into category + number
   - Create `TrainCategory` for each unique prefix/suffix
   - Generate deterministic IDs for all entities
   - Implement three-step import: Layout → Timetable → Schedule

2. **Update Access importer**:
   - Use actual database IDs
   - Map existing database structure to three-layer model

3. **Run all tests** and fix any failures:
   - `dotnet test Model.Tests`
   - `dotnet test Interfaces.Tests`
   - `dotnet test Xpln.Tests`

4. **Iterate** until all tests pass

### Phase 3: Extract Model to Separate Solution

Once tests pass and importers work correctly:

1. **Create new solution** `Tellurian.Trains.Model`:
   - Move model classes from `Tellurian.Trains.Schedules.Importers.Model`
   - Move corresponding tests to `Tellurian.Trains.Model.Tests`

2. **Create NuGet package** `Tellurian.Trains.Model`:
   - Configure package metadata
   - Publish to NuGet.org

3. **Update Schedule.Importers**:
   - Remove local model project
   - Add NuGet reference to `Tellurian.Trains.Model`
   - Verify all tests still pass

### Phase 4: Refactor Dispatch Model (Future)

1. Add NuGet reference to `Tellurian.Trains.Model`
2. Create Dispatch-specific extensions of base model:
   - `DispatchStation : Station` (adds IsManned, Dispatcher, PreferredLanguage)
   - `SignalControlledPlace` (Dispatch-specific location type)
   - `OtherPlace` (Dispatch-specific location type)
3. Create state extension classes:
   - `DispatchTrain` extending or wrapping `Train`
   - `StationCallState` wrapping `StationCall` with observed times
4. Keep Dispatch-specific classes: `TrainSection`, `DispatchStretch`

### Phase 5: Create Import-to-Dispatch Adapter (Future)

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

### Phase 6: Integrate with Broker (Future)

1. Broker accepts imported data through adapters (all three layers)
2. State provider persists only state deltas (TrainState, DispatchState, observed times)
3. On reload, base data is re-imported and state is reapplied

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

## 7. Conclusion

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

The migration can be done incrementally:
1. Verify existing three-layer structure in `Tellurian.Trains.Model`
2. Refactor Schedule.Importers to produce all three layers
3. Refactor Dispatch to consume them and add its state layer
4. Future consumers (publishing, simulation) follow the same pattern
