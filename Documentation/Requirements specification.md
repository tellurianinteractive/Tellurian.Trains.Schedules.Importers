# Timetable Planning System — Requirements Specification

> **Status:** Draft
> **Last Updated:** 2026-03-20

This is the main specification document for the Timetable Planning System,
a single application for planning model railway operations based on schedules.

## 1. Objectives

### 1.1 Purpose

The system enables timetable planners to create, edit, validate, and publish
complete operating schedules for model railway sessions — primarily for FREMO
module meetings, but also for fixed club layouts and home layouts.

### 1.2 Design Principles


| Principle                       | Description                                                                                                    |
| --------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| **Prototype-oriented**          | Model operations as close to real railway practice as possible, while acknowledging model-specific constraints |
| **Separation of concerns**      | Domain logic and validation are independent of the GUI; the same model supports local and online use           |
| **Local-first, online-capable** | Primary mode is local (offline) planning; collaborative online planning is an alternative mode                 |
| **Incremental data entry**      | Data can be entered from scratch, imported from previous plans, or fetched from external services              |
| **Multi-language**              | UI and validation messages in English, German, Danish, Norwegian, Swedish (at minimum)                         |
| **Reports separated from editing** | Printable content is treated as *reports*, hosted in a dedicated print shell distinct from the interactive editor; the same content may be offered both as an editor and as a report |
| **Settings on the layout**      | All configurable settings live on the layout (`Layout.Settings`), grouped by purpose, and are persisted and re-applied with it |

### 1.3 Scope Boundaries

**In scope:**

- Layout definition (stations, tracks, stretches)
- Train scheduling with time calculations
- Vehicle (locomotive and trainset) scheduling
- Driver duty planning
- Wagon group management
- Validation of conflicts and consistency
- Graphical timetable display
- Printed output (train cards, station books, driver duty sheets)
- Import of reusable data (layouts, companies, train categories)
- Sessions / operating day patterns

**Out of scope (separate systems already exist):**

- Real-time dispatch and train clearance during operation (separate dispatch system; consumes data exported from this module)
- Module registry and meeting management (separate system handles modules, meetings, participants)

**Deferred (lower priority):**

- Internal track connections with entry/exit points and routing within stations
- Shunting task planning at stations

---

## 2. Domain Concepts

### 2.1 Glossary


| Term                       | German              | Description                                                                                                      |
| ---------------------------- | --------------------- | ------------------------------------------------------------------------------------------------------------------ |
| Layout                     | Gleisplan           | The physical infrastructure: stations, track stretches, and their connections                                    |
| Operation Location         | Betriebsstelle      | A point on the layout where operational events occur (abstract base)                                             |
| Station                    | Bahnhof             | A manned operation location with one or more tracks                                                              |
| Shadow Station             | Schattenbahnhof     | A hidden yard at the end of a line, representing the outside world                                               |
| Signal-Controlled Location | Blockstelle         | An unmanned location controlled by signals                                                                       |
| Station Track              | Gleis               | A track within a station where trains stop or pass                                                               |
| Track Stretch              | Strecke             | A section of track connecting two operation locations                                                            |
| Timetable Stretch          | Buchfahrplanstrecke | A named sequence of track stretches forming a logical line (used for graphical timetable display)                |
| Dispatch Stretch           | Zugleitstrecke      | A stretch between two dispatched stations                                                                        |
| Timetable                  | Fahrplan            | A collection of trains operating on a specific layout                                                            |
| Train                      | Zug                 | A scheduled service with an ordered sequence of station calls                                                    |
| Train Category             | Zuggattung          | Classification of a train (passenger, freight, etc.) with display properties                                     |
| Station Call               | Halt                | A scheduled stop or passage at a station track, with arrival and departure times                                 |
| Company                    | Bahngesellschaft    | A railway company operating trains                                                                               |
| Schedule                   | Umlaufplan          | The complete operational plan: timetable + vehicle assignments + driver duties                                   |
| Vehicle Schedule           | Fahrzeugumlauf      | A sequence of train parts forming a circulation; assignable to locomotives, wagons, wagon groups, or cargo flows |
| Driver Duty                | Schicht             | A continuous assignment of a driver to a sequence of train parts                                                 |
| Train Part                 | Zugteil             | A segment of a train's journey used for assigning vehicles or drivers                                            |
| Wagon Group                | Wagengruppe         | A group of wagons within a train, tracked between origin and destination                                         |
| Cargo Flow                 | Güterverkehr       | A flow of cargo to specific destinations, scheduled like a vehicle but assigned to a cargo flow object           |
| Sessions                   | Verkehrstage        | Operating day patterns controlling which sessions a train, duty, or vehicle runs                                 |
| On-Demand Train            | Bedarfszug          | A train that only runs when needed                                                                               |
| Fast Clock                 | Modelluhr           | An accelerated clock used during operation; the ratio of model time to real time                                 |
| Graphical Timetable        | Bildfahrplan        | Time-distance diagram showing train movements along a timetable stretch                                          |

### 2.2 Model Railway–Specific Concepts

These concepts distinguish model railway planning from prototype railway planning:


| Concept                                   | Description                                                                                                                                                                                                 |
| ------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Fast clock**                            | Operations use an accelerated clock. All scheduled times are in fast-clock time. Real durations for physical actions (loco runaround, telephone dispatch) must be converted to fast-clock time.             |
| **Speed mapping**                         | Model trains cannot be scaled accurately by prototype speed alone. A multi-point mapping translates scale speeds (km/h) to real model speeds (m/s). See §4.3.                                              |
| **Sessions**                              | A model railway meeting may run multiple operating sessions (1–14). Trains, vehicles, and duties can be active in specific sessions only.                                                                  |
| **Shadow stations**                       | Hidden staging yards that represent off-layout destinations. Trains originate from and terminate at shadow stations.                                                                                        |
| **Module ownership**                      | In FREMO meetings, stations are owned by individual members who bring their modules.                                                                                                                        |
| **Station-specific real-time operations** | Some actions take a fixed real-world time regardless of clock speed: locomotive runaround, manual dispatch telephone calls, coupling/uncoupling. These must be converted to fast-clock time for scheduling. |

### 2.3 Three-Layer Model Hierarchy

```
Layout (physical infrastructure)
   ↑
Timetable (train services) — references Layout
   ↑
Schedule (equipment + crew) — references Timetable
```

This separation enables:

- Reusing the same layout across different timetables
- Having multiple schedules for the same timetable (different crew/equipment rotations)
- Validating each layer against the layer below it
- Importing layers independently from different sources

---

## 3. Functional Requirements

This section describes what the user can do with the system.
Detailed GUI interactions will be expanded progressively in the subsections below.

### 3.1 User Interface Overview

The user interface shall be web-based. A minimal viewport for data entry and editing
shall be defined. The interface uses a tab-based organisation with the following tabs:

1. **Settings** — name of layout, timing parameters, and other default values (see §3.2)
2. **Layout Operational Places** — each expandable to show tracks; data entered manually or imported from external file/web API (see §3.3)
3. **Track and Timetable Stretches** — connections between operation locations and timetable stretches as sequences of track stretches (see §3.4)
4. **Train Categories** — list of train category entries that can be added, edited, deleted (if not referenced), and imported from file/web API (see §3.5)
5. **Trains** — list of trains, each expandable to show times, each time expandable to show notes; editable at all levels; groupable by category (see §3.6)
6. **Graphical Timetable** — one tab per timetable stretch; trains can be added, removed, and edited in the graph (see §3.7)
7. **Vehicle Schedule Editor** — build vehicle schedules and assign to locomotives, wagons, wagon groups, and cargo flows (see §3.8)
8. **Vehicle Owners** — who brings what of the needed rolling stock (see §3.9)
9. **Reports** — preview and print all printable content; reports are selected here and kept separate from the editing tabs (see §3.12)

### 3.2 Settings

All configurable settings are stored on the layout as `Layout.Settings` and are
persisted with it, so they are re-applied whenever the layout is reopened. Settings
are organised into groups — each a separate type — surfaced as sub-sections of the
**Settings** tab:

| Group               | Purpose                                                      | Detailed in  |
| ------------------- | ------------------------------------------------------------ | ------------ |
| General             | Session/day model and operating time window                  | this section |
| Graphical Timetable | View and print preferences for the graphical timetable       | §3.7.3       |
| Time & Speed        | Fast clock, speed mapping, default station operational times | §4.3         |
| Validation          | Which validations run and their thresholds                   | §3.11.3      |
| Integration         | API keys for external services (e.g. ModuleRegistry)         | §3.3         |

**General settings:**

| Setting    | Description                                                        | Default      |
| ---------- | ----------------------------------------------------------------- | ------------ |
| Use days   | Present operating days instead of sessions                        | Use sessions |
| Start day  | Weekday of the first session when using days                      | Monday       |
| Start time | Fast-time start hour of operation                                 | 06:00        |
| End time   | Fast-time end hour of operation                                   | 20:00        |
| Break time | Optional fast-time break hour that splits the graphical timetable | None         |

Start, end, and break time define the operating time window used by the graphical
timetable (see §3.7).

The **user-interface language** is a user-level preference, chosen via the language selector
in the top bar and persisted in the browser (localStorage) — it is not stored per layout.

### 3.3 Layout Operational Places

*To be detailed — entry and editing of operation locations and their tracks.*

#### FR-3.3.1 Data Import

- **From the [ModuleRegistry](https://moduleregistry.azurewebsites.net)**: if the modules have been submitted to the meeting layout, it shall be possible to import the operation locations using its web API. This API requires an API key, stored in settings.

### 3.4 Track and Timetable Stretches

To create track stretches, the user enters a sequence of operational places using a
dropdown. The application then automatically creates track stretch entries with default
values and builds a timetable stretch.

The user can continue with an additional sequence of operational places; the application
reuses already-created track stretches or creates new ones as necessary.

The application shall enforce that all stretches within a timetable stretch run in the
same direction, meaning trains can traverse these stretches without changing direction.

### 3.5 Train Categories

*To be detailed — entry, editing, deletion, and import of train categories.*

### 3.6 Trains

*To be detailed — entry and editing of trains, station calls, and call-level notes.*

### 3.7 Graphical Timetable

The system shall display a graphical timetable for each timetable stretch:

- One axis: time (fast-clock hours), defined by start and end time in settings (see §3.2)
- Other axis: operation locations positioned by distance, with a minimum distance defined by the text that needs to be presented on the graph in between
- Train movements shown as lines connecting arrival/departure points and times at a track

#### FR-3.7.1 Axis Orientation

The user shall be able to switch the graphical timetable between two orientations:

| Orientation | Time Axis                | Station/Distance Axis   | Default print orientation |
| ----------- | ------------------------ | ----------------------- | ------------------------- |
| Horizontal  | Horizontal (left→right) | Vertical (top→bottom)   | Landscape                 |
| Vertical    | Vertical (top→bottom)   | Horizontal (left→right) | Portrait                  |

Both orientations display the same data; the user chooses based on preference or
the shape of the timetable stretch (many stations vs. long time span).

#### FR-3.7.2 Visual Styling

- Trains are drawn as solid lines coloured by train category
- Lines are drawn between stations from departure time at A to arrival time at B
- Lines are drawn at a station from arrival to departure time
- When a train stops, this is indicated by a vertical line at arrival and departure times
- Train number labels along lines between stations, formatted as {company} {category} {number} {sessions/days}

#### FR-3.7.3 Display Settings

The graphical timetable shall support the following configurable display parameters:


| Setting                | Description                                                |
| ---------------------- | ---------------------------------------------------------- |
| Orientation            | Horizontal or Vertical time axis                           |
| Show Arrival Minutes   | Toggle display of arrival minute labels at station calls   |
| Show Departure Minutes | Toggle display of departure minute labels at station calls |
| Minute Spacing         | Pixels per minute on the time axis                         |
| Station Spacing        | Minimum pixel spacing between stations on the distance axis|
| Track Spacing          | Spacing in pixels between individual tracks at a station   |
| Show train category    | Also shows train category prefix before train number       |
| Show company           | Also shows train company signature before train number     |
| Hide sessions/days     | Hide sessions/days before train number                     |

The display parameters are stored on the layout as `Layout.Settings.GraphicTimetable`
and re-applied when the layout is reopened. They are user preferences that apply to
both on-screen viewing and printing. Start, end, and break time come from the General
settings (see §3.2).

#### FR-3.7.4 Interaction

##### Change timing of a train or part of train
- Click to select a train: arrival and departure times become small draggable squares
- When dragging an arrival or departure time, only the dragged time and all later times are affected
- To move a whole train in time, drag the first arrival time
- Click outside draggable squares to stop editing times

##### Train context menu
Right-click on a train to show a context menu: edit, duplicate, remove, select category.

### 3.8 Vehicle Schedule Editor

*To be detailed — building vehicle schedules and assigning them to locomotives, wagons, wagon groups, and cargo flows.*

### 3.9 Vehicle Owners

*To be detailed — managing who brings what rolling stock for the session.*

---

### 3.10 Automatic Time Calculation

The system shall calculate travel times between station calls using:

- The speed mapping (see §4.3) to convert scale speed to real model speed
- The track stretch distance
- The fast clock speed to convert real time to scheduled time
- Station-specific operational times (see §4.3)

When a user changes one time, subsequent times should shift accordingly,
with an option to lock individual times.

---

### 3.11 Validation

The system shall validate the schedule at two levels:

#### FR-3.11.1 Data Integrity Validation (during data entry/import)

- All referenced station tracks exist in the layout
- Train has at least two station calls
- Arrival time ≤ departure time at each call

#### FR-3.11.2 Scheduling Conflict Detection (on demand)

- **Station track conflicts** — two trains on the same track with overlapping times
  (exception: trains sharing a vehicle schedule, e.g. loco change)
- **Track stretch conflicts** — trains conflicting on single-track stretches
- **Train time sequence** — calls not in chronological order
- **Train speed** — speed between calls outside min/max thresholds
- **Vehicle schedule overlaps** — a vehicle schedule with overlapping train parts
- **Locomotive coverage** — gaps or overlaps in locomotive assignment across a train
  (exception: loco change at same station)
- **Vehicle double booking** — same vehicle assigned to overlapping sessions

#### FR-3.11.3 Validation Configuration

All validations shall be individually toggleable. Speed thresholds and timing
parameters shall be configurable. These settings are stored on the layout as
`Layout.Settings.Validation` (see §3.2).

#### FR-3.11.4 Validation Output

Validation errors shall include:

- Severity level (Information, Warning, Error, System)
- Localized message text
- Location (station track) and time range for graphical highlighting
- Involved trains

---

### 3.12 Output and Printing

The system shall generate printable reports in various page formats
(A3 landscape, A4 portrait/landscape, A5, pocket cards).
All reports shall support filtering by operator, station, duty number, etc.

Printable content is organised as **reports**, separate from the editing UI. All
reports are reached from the **Reports** tab, where the user selects a report,
previews it, and prints it. Each report declares its own page format and
**orientation (portrait or landscape)**, so a single planning session can produce
reports of mixed orientations. Content that is also editable (e.g. the Graphical
Timetable) is offered both as an editor and as a report, reusing the same rendering.

#### FR-3.12.0 Report Shell

Reports are hosted in a print-specific shell without editing chrome (title bar, tabs).
On screen a report shows non-printing controls (parameters, print button) that are
excluded from the printed output. Each report sets its own page size and orientation.

#### FR-3.12.1 Driver & Station Reports


| Report                 | Description                                                                            | Format |
| ------------------------ | ---------------------------------------------------------------------------------------- | -------- |
| Driver Duties Booklet  | Multi-page booklet with duty front pages, duty parts, and instructions                 | A4     |
| Station Duties Booklet | All trains at each station with arrival/departure details and staff instructions       | A4     |
| Station Instructions   | Station-specific operational and shunting instructions                                 | A5     |
| Station Train Order    | Train order tables per station with times, tracks, destinations, and dispatch contacts | A4L    |

#### FR-3.12.2 Vehicle Schedule Reports


| Report                       | Description                                                           | Format |
| ------------------------------ | ----------------------------------------------------------------------- | -------- |
| Locomotive Schedule Cards    | Individual loco assignment cards, optionally including shunting locos | Card   |
| Trainset Schedule Cards      | Trainset (passenger/freight) duty assignment cards                    | Card   |
| Wagon/Turnus Cards           | Four cards per page showing specific wagon assignments                | Card   |
| Graphic Locomotive Schedules | Graphical time-based view of locomotive assignments across turnus     | A3L    |

#### FR-3.12.3 Timetable Displays


| Report              | Description                                                        | Format |
| --------------------- | -------------------------------------------------------------------- | -------- |
| Graphical Timetable | Time-distance diagram per timetable stretch, filterable by days    | A3L    |
| Train Compositions  | Detailed car assignments per train with class, number, and routing | A4L    |

#### FR-3.12.4 Operational Lists


| Report                   | Description                                                          | Format |
| -------------------------- | ---------------------------------------------------------------------- | -------- |
| Train Departure Labels   | Physical track labels with train number, operator, time, destination | Label  |
| Block Destinations       | Signal block routing information for dispatching                     | A4     |
| Adjacent Dispatch Places | Contact list of neighbouring dispatch locations with phone numbers   | A4     |

#### FR-3.12.5 Vehicle Start & Inventory


| Report              | Description                                                                                                     | Format |
| --------------------- | ----------------------------------------------------------------------------------------------------------------- | -------- |
| Vehicle Start Infos | Where vehicles must be at session start — multiple views: overview, per owner, per station, with DCC addresses | A4     |

#### FR-3.12.6 Planning Analysis


| Report                 | Description                                                                          | Format |
| ------------------------ | -------------------------------------------------------------------------------------- | -------- |
| Trains Time Allocation | Bar chart showing driver duty demand per time slot, colour-coded for capacity status | A4L    |

---

### 3.13 Collaboration (Future Enhancement)

#### FR-3.13.1 Local Planning Mode

The primary mode: a single planner works locally, data stored on disk or in browser storage.

#### FR-3.13.2 Online Collaborative Mode

An alternative mode where multiple planners work on the same schedule simultaneously.

- **Conflict resolution**: Last-change-wins — changes are propagated fast so conflicts are rare in practice.
- **Division of work**: Planners typically divide by scope (train category, stretch) or time, so the app does not need to enforce locking or conflict resolution beyond fast propagation.
- **Hosting model**: To be specified separately.

---

## 4. Data Model

This section describes the data entities, their properties, and how they relate to each other.

### 4.1 Layout

The layout carries all configurable settings as `Layout.Settings`, grouped by purpose
(see §3.2).

#### DM-4.1.1 Operation Locations

The system shall support defining operation locations with:


| Property  | Description                                          | Required |
| ----------- | ------------------------------------------------------ | ---------- |
| Name      | Full station name                                    | Yes      |
| Signature | Short code (e.g. "Hb")                               | Yes      |
| Type      | Station, Signal-Controlled, Other                    | Yes      |
| Owner     | Module owner (for FREMO)                             | No       |
| Is Shadow | Hidden yard at line end                              | No       |
| Regions   | Regions/countries represented (shadow stations only) | No       |
| Timings   | Per-station operational time overrides (`OperationLocation.Timings`); each value optional, inheriting the layout default when unset (see §4.3.3) | No       |

Subtypes:

- **Station** — manned location; may be a shadow station. Shadow stations represent
  the outside world and are configured with the regions and countries they serve
  (used for cargo flow routing, see DM-4.2.5)
- **Signal-Controlled Location** — unmanned, controlled by signals
- **Other Location** — any other operational point

#### DM-4.1.2 Station Tracks

Each operation location shall have one or more tracks:


| Property      | Description                                |
| --------------- | -------------------------------------------- |
| Number        | Track designation (e.g. "1", "2a")         |
| Display Order | Order in UI and printed output             |
| Is Main       | Whether this is a main through-track       |
| Is Scheduled  | Whether trains are scheduled on this track |
| Length        | Usable track length (meters, model scale)  |
| Usage         | Free-text usage description                |

#### DM-4.1.3 Track Stretches

The system shall define physical connections between operation locations:


| Property     | Description                                                    |
| -------------- | ---------------------------------------------------------------- |
| Start / End  | The two connected operation locations                          |
| Distance     | Length in meters (model scale)                                 |
| Tracks Count | Number of parallel tracks (1 = single track, 2 = double track) |
| Speed        | Maximum permitted scale speed (km/h)                           |
| Time         | Calculated travel time based on speed mapping                  |

#### DM-4.1.4 Timetable Stretches

The system shall support grouping track stretches into named lines:


| Property    | Description                         |
| ------------- | ------------------------------------- |
| Number      | Line number                         |
| Description | Line name                           |
| Stretches   | Ordered sequence of track stretches |

Timetable stretches are the unit for graphical timetable display.

#### DM-4.1.5 Dispatch Stretches

The system shall define dispatch territories between manned stations.

#### DM-4.1.6 Companies

The system shall maintain railway companies operating on the layout:


| Property      | Description                 | Remark                                        |
| -------------- | :---------------------------- | ----------------------------------------------- |
| Name          | Company name                | Can be both real and fictive companies        |
| Signature     | Short code                  | For real companies, use the UIC-assigned code |
| Language Code | Two-letter ISO country code | Used to translate output                      |

---

### 4.2 Timetable and Trains

#### DM-4.2.1 Train Definition

The system shall support creating trains with:


| Property  | Description                        | Required           |
| ----------- | ------------------------------------ | -------------------- |
| Number    | Train number                       | Yes                |
| Category  | Train category (see DM-4.2.2)      | Yes                |
| Company   | Operating company                  | Yes                |
| Sessions  | Which sessions this train runs     | Yes (default: all) |
| Max Speed | Train's maximum scale speed (km/h) | No                 |

#### DM-4.2.2 Train Categories

The system shall support configurable train categories:


| Property                  | Description                                                                       |
| :-------------------------- | :---------------------------------------------------------------------------------- |
| Prefix / Suffix           | For train identity formatting (e.g. "IC", "G")                                    |
| Is Passenger / Is Freight | Classification, can be none, one of or both                                       |
| Color                     | Display color in graphical timetable and otherwise where color makes sense        |
| Display Name              | Name of category                                                                  |
| Default Speed             | Default scale speed for this category (km/h); used when no per-train speed is set |

#### DM-4.2.3 Station Calls

Each train shall have an ordered sequence of station calls:


| Property  | Description                      |
| ----------- | ---------------------------------- |
| Track     | The station track used           |
| Arrival   | Arrival time (fast-clock time)   |
| Departure | Departure time (fast-clock time) |
| Notes     | Call-specific notes              |

- The first arrival time on a train is the last expected show-up time for the train driver
- The last departure time on a train is the expected ready time for the train driver

#### DM-4.2.4 Wagon Groups

The system shall track wagon groups within trains:


| Property               | Description                                      |
| ------------------------ | -------------------------------------------------- |
| Position In Train      | Order within the consist                         |
| From / To Station Call | Where the wagon group joins and leaves the train |
| Remark                 | Description of the wagon group                   |

**Direction-dependent ordering**: Wagon position is direction-dependent.
A consist ordered 1-2-3-4 becomes 4-3-2-1 when the train reverses direction.
The system must track and display the correct order based on current direction of travel.

#### DM-4.2.5 Cargo Flows

The system shall support cargo flow scheduling, which is distinct from wagon/vehicle scheduling.
A cargo flow describes the movement of cargo to specific destinations, assigned to a
cargo flow object rather than to a locomotive or wagon group.

Each cargo flow has one or several destinations, which can be:


| Destination Type       | Description                                              |
| ------------------------ | ---------------------------------------------------------- |
| Local                  | An operation location within the layout                  |
| External               | Sent to the corresponding shadow yard                    |
| To All Destinations    | Cargo goes to all destinations on the flow               |
| And Local Destinations | Including local stops                                    |
| And Regions            | Including specific regions attached to a shadow yard     |
| And Abroad             | Including countries/regions represented by a shadow yard |

Shadow yards represent parts of the outside world — specific regions and/or countries.
Cargo destined externally is routed to the shadow yard that represents the relevant region.
Region-to-shadow-yard mapping is configured per layout (see DM-4.1.1).

A cargo flow is scheduled using the common vehicle schedule mechanism (see DM-4.4.3) —
the same sequence-of-train-parts pattern used for locomotives and wagon groups,
but assigned to a cargo flow object instead.

#### DM-4.2.6 Sessions

The system shall support 1–14 operating sessions with:

- Predefined patterns: All, Odd, Even, Thirds, On-Demand
- Day-of-week mapping (Monday–Sunday) for weekly patterns
- Operations for combining and testing overlap
- Trains, vehicle schedule assignments, and driver duties can each specify their active sessions
- Display as actual session numbers or localised day names

---

### 4.3 Speed Mapping and Time Calculation

#### DM-4.3.1 Multi-Point Speed Mapping

The system shall map scale speeds (km/h) to real model speeds (m/s) using a
three-point curve:


| Speed Class    | Example Scale Speed | Example Real Speed | Purpose          |
| ---------------- | --------------------: | -------------------: | ------------------ |
| Slow           |             60 km/h |           0.15 m/s | Yard movements   |
| Normal         |            100 km/h |           0.25 m/s | Regular services |
| High (express) |            200 km/h |           0.35 m/s | Fast trains      |

The three points are configurable per timetable. Intermediate values are
interpolated linearly (piecewise) between the defined points.

The effective speed for a train on a stretch is:

```
effectiveScaleSpeed = min(train.MaxSpeed ?? train.Category.DefaultSpeed, stretch.MaxSpeed)
effectiveRealSpeed = interpolate(effectiveScaleSpeed, speedMapping)
```

A train's speed defaults to its category's default speed if not set explicitly.

**Variable speed limits on a stretch** are deliberately not modelled in scheduling.
While some stretches have speed changes along their length (e.g. curves, junctions),
modelling these in the planner would add complexity without practical benefit.
In real model railway operation, actual running times are dominated by factors
outside the planner's control: driver skill, module joint conditions, rolling stock
derailments requiring intervention, and track shortcuts. An estimated average speed
per stretch is sufficient for scheduling; actual speed compliance is an operational
concern during the running session.

#### DM-4.3.2 Fast Clock

The system shall use an expected fast clock speed (integer multiplier, e.g. 5×)
to convert between real time and scheduled (model) time:

```
scheduledMinutes = realSeconds / 60 × expectedFastClockSpeed
```

All times in station calls and timetables are in fast-clock time.

#### DM-4.3.3 Station Operational Times

The following real-world durations are configurable. The layout-wide defaults are
stored as `Layout.Settings.TimeAndSpeed.StationTimings`; each station may override
any individual value via `OperationLocation.Timings`. Overrides are per field — an
unset (null) value inherits the layout default — so imported stations can carry only
the timing values that differ.


| Parameter                | Description                                        | Default | Unit               |
| -------------------------- | ---------------------------------------------------- | --------- | -------------------- |
| Minimum stop duration    | Minimum time a train stops at a manned station     | 3       | fast-clock minutes |
| Loco runaround duration  | Real time for a locomotive to run around its train | 5       | real minutes       |
| Train clearance duration | Real time for telephone dispatch clearance         | 1       | real minutes       |

Real-time durations are converted to fast-clock time for scheduling:

```
fastClockMinutes = realMinutes × fastClockSpeed
```

Stations override these defaults to reflect their specific infrastructure
(e.g., a large station with a long runaround track takes longer).

---

### 4.4 Schedule and Vehicles

#### DM-4.4.1 Schedule

A schedule is the top-level planning artifact combining:

- A timetable (trains and their station calls)
- Vehicle schedules (locomotive and trainset assignments)
- Driver duties
- Vehicle inventory

#### DM-4.4.2 Vehicles

The system shall maintain a vehicle inventory:


| Property           | Description                                             |
| -------------------- | --------------------------------------------------------- |
| Number             | Vehicle number                                          |
| Type               | Locomotive or Wagonset                                  |
| Class              | Vehicle class/series                                    |
| Number of Units    | For multiple units                                      |
| Is Double-Directed | Can run in both directions without runaround or turning |
| Company            | Owning company                                          |
| DCC-address        | For motorised vehicles only                             |

#### DM-4.4.3 Vehicle Schedules

A vehicle schedule defines a sequence of train parts forming a circulation pattern.
The same schedule structure is used for all assignable objects.

**Schedule patterns** (examples where a, b, c are stations):


| Pattern               | Description                       |
| ----------------------- | ----------------------------------- |
| a-b b-a               | Simple return working             |
| a-b b-c c-b b-a       | Extended trip with return         |
| a-b / b-a             | Split across sessions (see below) |
| a-b-c / b-c-a / c-a-b | Three-session rotation            |

**Multi-session rotation**: A schedule like a-b can be paired with b-a across sessions.
Vehicle 1 runs a-b in session 1 and b-a in session 2, while vehicle 2 does the reverse.
This extends to three or more sessions for longer rotations.

**Assignment targets**: A vehicle schedule can be assigned to:


| Target       | Description                                   |
| -------------- | ----------------------------------------------- |
| Locomotive   | Motive power assignment                       |
| Single Wagon | Individual wagon circulation                  |
| Wagon Group  | Group of wagons moving together               |
| Cargo Flow   | Cargo movement to destinations (see DM-4.2.5) |

Each assignment specifies which sessions it applies to.
Cargo flow assignments can additionally restrict the maximum number of wagons to bring.

#### DM-4.4.4 Driver Duties

The system shall support creating driver duties:


| Property | Description                        |
| ---------- | ------------------------------------ |
| Identity | Duty number/name                   |
| Company  | Operating company                  |
| Sessions | Which sessions this duty is active |
| Parts    | Sequence of train parts            |
| Notes    | Duty-specific notes                |

---

### 4.5 Note Structure and Localized Text

#### DM-4.5.1 Data-Driven Note Texts

The system shall generate note texts automatically from schedule data rather than
storing pre-composed text. Each note is assembled at display time from structured
data and localized resource strings, so the same schedule produces correct output
in any supported language without re-editing.

#### DM-4.5.2 Note Types

The following note types shall be generated from station call data:


| Note Type                      | Generated From                          | Example Output                                |
| -------------------------------- | ----------------------------------------- | ----------------------------------------------- |
| Loco connect/disconnect        | Vehicle schedule + station call         | "Connect loco DB 218 042"                     |
| Loco exchange                  | Two vehicle schedules at same call      | "Replace loco; Use loco DB 101 003"           |
| Loco turn/reverse/circulate    | Vehicle properties + station call       | "Turn loco", "Reverse loco", "Circulate loco" |
| Loco driver sorts wagons       | Wagon group data at arrival             | "Loco driver sorts wagons at arrival"         |
| Wagon group connect/disconnect | Wagon group assignment                  | "Connect wagons to [destinations]"            |
| Block origin                   | Wagon block routing at arrival          | "Connect wagons from [stations]"              |
| Block destinations             | Wagon block routing at departure        | "Brings wagons to [destinations]"             |
| Block arrival                  | Wagon block disconnect at arrival       | Disconnect details with transfer/routing info |
| Scheduled wagons               | Wagon turnus assignment                 | "Turnus 5 (max 4 wagons)"                     |
| Passenger pickup               | Passenger service at departure          | "Pick up passengers"                          |
| Passenger interchange          | Passenger transfer at station           | "Passenger interchange"                       |
| Train continuation             | Train number change                     | "Continues as IC 2045"                        |
| Train meets                    | Trains meeting from opposite directions | "Meets G 4012 at 14:23"                       |
| Train overtaking               | Train passing another in same direction | "Overtakes G 4012"                            |
| Manual note                    | User-entered per language               | Free text, stored per language code           |

#### DM-4.5.3 Note Formatting

Notes shall be rendered as structured markup with semantic CSS classes,
enabling consistent styling across screen display and printed output:

- **Days prefix** — when a note applies only to certain sessions/days
- **Localized action text** — from language resource files (e.g. "Connect loco" / "Lok ankuppeln")
- **Value** — the specific vehicle, train, or destination
- **Remark** — optional additional information

#### DM-4.5.4 Destination Rendering in Notes

Wagon group, cargo flow, and block destination notes display destinations with
visual type indicators and detailed routing information:


| Destination Type     | Visual Treatment                                  |
| ---------------------- | --------------------------------------------------- |
| Country              | Flag icon (international traffic)                 |
| Region               | Background color with auto-contrasting text color |
| Cargo destination    | Plain text                                        |
| Transfer destination | Transfer origin/destination names shown           |

Block destinations carry additional detail: whether traffic goes to all destinations,
includes local destinations, extends beyond (and-beyond), or involves international routing.
Wagon order in train and max wagon counts may also be displayed.

The auto-contrasting text color (black or white) is calculated from the background
color luminance to ensure readability.

#### DM-4.5.5 Manual Notes with Multi-Language Support

Users shall be able to add manual free-text notes to any station call.
A manual note has a default text (in the planner's language of choice) and
optional translations as sub-items, each keyed by language code (e.g. "de", "sv").
When generating reports, the system uses the translation matching the requested
language; if no translation exists, the default text is used.

---

## 5. Integration Requirements

This section describes how data is exchanged with external systems and previous plans.

### 5.1 Import from Previous Plans

The system shall support importing reusable data from saved plans:

- Layout (stations, tracks, stretches)
- Train categories
- Companies
- Vehicle definitions

### 5.2 External Service Import

The system shall support fetching reference data via web API:

- Train categories from a shared service
- Company data (existing JSON dataset of ~9,700 railway operators)
- Potentially: module/station data from a module registry

### 5.3 XPLN Import (Legacy)

The system shall support importing complete schedules from XPLN spreadsheets
(ODS/XLSX format) as described in the existing Importers.Xpln project.

### 5.4 Export

The system shall export schedules in JSON format for:

- Backup and archival
- Transfer to other users
- Input to other systems (dispatch, etc.)

---

## 6. Non-Functional Requirements

### 6.1 Usability

- **NFR-6.1.1**: Multi-language UI — at minimum English, German, Danish, Norwegian, Swedish.
- **NFR-6.1.2**: Validation messages localized in the same languages.
- **NFR-6.1.3**: The application shall work on large screens (desktop/laptop);
  mobile support is not required.
- **NFR-6.1.4**: Keyboard navigation for time entry fields.

### 6.2 Data Integrity

- **NFR-6.2.1**: Auto-save to prevent data loss.
- **NFR-6.2.2**: Prevent orphan references when deleting entities.
- **NFR-6.2.3**: Undo/redo support for editing operations.

### 6.3 Performance

- **NFR-6.3.1**: Graphical timetable with 100+ trains shall render in < 2 seconds.
- **NFR-6.3.2**: Validation of a complete schedule shall complete in < 1 second.

### 6.4 Portability

- **NFR-6.4.1**: The application shall run offline (no server required for local mode).
- **NFR-6.4.2**: Data shall be storable locally (file system or browser storage).
- **NFR-6.4.3**: Cross-platform: Windows, macOS, Linux (via browser).

---

## 7. Technical Requirements

This section describes what the application needs to support technically
and outlines implementation approaches.

### 7.1 Application Architecture

- **TR-7.1.1**: Domain logic (model, validation, time calculation) shall be in
  a separate .NET class library, independent of any GUI framework.
- **TR-7.1.2**: The GUI shall be implemented in Blazor — WebAssembly for local mode,
  potentially Blazor Server or Blazor Web App for collaborative mode.
- **TR-7.1.3**: The application shall be a Progressive Web App (PWA),
  installable and fully functional offline.

### 7.2 Technology Stack


| Component           | Technology                   | Notes                                                                      |
| --------------------- | ------------------------------ | ---------------------------------------------------------------------------- |
| Language            | C# / .NET                    | Current codebase targets .NET 10.0                                         |
| GUI framework       | Blazor                       | WebAssembly for local, Server for collaborative                            |
| Domain model        | Plain C# classes             | No framework dependencies in the model layer                               |
| Persistence         | JSON + Entity Framework Core | JSON for file-based local storage; EF Core for database-backed scenarios   |
| Localization        | .resx resource files         | Existing pattern in Model/ for validation messages; extend to UI and notes |
| Graphical timetable | SVG rendering                | Generated in Blazor components; scalable for print and screen              |
| Report output       | HTML/CSS → browser print    | Semantic markup with print stylesheets; browser handles PDF generation     |

### 7.3 Persistence Strategy

- **Local mode**: Schedules stored as JSON files on disk or in browser IndexedDB/localStorage.
  Auto-save writes to local storage on each significant edit.
- **Collaborative mode**: Server-side persistence (database via EF Core).
  Changes propagated to other clients via SignalR or similar real-time channel.
- **Serialization**: The domain model shall support round-trip JSON serialization
  (System.Text.Json) for file exchange, backup, and local storage.

### 7.4 Localization Architecture

- All user-visible strings (UI labels, validation messages, note action texts)
  shall come from .resx resource files, one per supported language.
- Note texts are assembled from resource strings + data at render time (see §4.5),
  so no translated text is stored in the schedule data.
- The company's language code (see DM-4.1.6) determines the output language
  for company-specific reports (e.g. driver duty sheets printed in the
  operating company's language).
- Manual notes use a default-text-plus-translations model (see DM-4.5.5),
  independent of the resource file system.

### 7.5 Graphical Rendering

- **Graphical timetable**: Rendered as SVG within a Blazor component.
  Coordinate calculations are in the domain layer; SVG element generation
  is in the GUI layer. Axis orientation (FR-3.7.1) is a coordinate transform,
  not a separate rendering path.
- **Graphic locomotive schedules**: Same SVG approach as the graphical timetable,
  showing vehicle assignments on a time axis.
- **Destination colors**: Region background colors and auto-contrasting text colors
  (see DM-4.5.4) calculated from hex color values using YIQ luminance.

### 7.6 Report and Print Architecture

- Reports are rendered as HTML with semantic CSS classes and print-specific stylesheets.
- Each report type is a Blazor component that can be displayed on screen
  and printed via the browser's print function (or saved as PDF).
- Page formats (A3L, A4, A5, Card, Label) are controlled by CSS @page rules
  and component-level layout.
- Note markup (see DM-4.5.3) uses the same CSS classes in both screen and print contexts.

### 7.7 Collaborative Mode Technology

- Real-time synchronization via SignalR (or equivalent WebSocket-based channel).
- Last-change-wins conflict resolution at the entity level (see FR-3.13.2).
- Fast propagation: changes broadcast to all connected clients immediately on save.
- Authentication and hosting model to be specified separately.

---

## 8. Deferred Features

These features are recognized as valuable but deferred to later phases:


| Feature                    | Source          | Notes                                                    |
| ---------------------------- | ----------------- | ---------------------------------------------------------- |
| Internal track connections | FTrain FR-2.2.6 | Entry/exit points, routing within stations               |
| Shunting task planning     | FTrain FR-2.8   | Detailed shunting operations at stations                 |
| Wheel/axle count tracking  | FTrain FR-2.7.4 | Weight calculations                                      |
| Multi-window support       | FTrain FR-2.12  | Multiple specialized editor windows                      |
| Consist diagrams           | FTrain FR-2.7.6 | Visual train composition display                         |
| Path-finding algorithm     | FTrain FR-2.6.6 | Automatic route calculation (may already be implemented) |

---

## 9. Relationship to Existing Code


| Existing Artifact         | Relationship to This Spec                                                    |
| --------------------------- | ------------------------------------------------------------------------------ |
| `Model/` (Schedules repo) | Authoritative implementation of the domain model (§2, §4)                   |
| `Model/Validations/`      | Implements §3.11                                                             |
| `Importers.Xpln/`         | Implements §5.3 (XPLN import)                                               |
| `Importers.Access/`       | Legacy Access import (experimental)                                          |
| `Importers.Services/`     | Shared import services; provides company data for §5.2                      |
| Timetables App            | Earlier UI attempt; source for §4.3 speed/time model and UI patterns        |
| Timetable Planning App    | Prototype with full report implementations; source for §4.5, §3.7, §3.12   |
| FTrain analysis           | Reference for functional requirements; see §8 for deferred items            |

---

## Appendix A: Design Decisions

Resolved questions captured here for reference.

1. **Wagon group vs single wagon**: The planner decides at planning time whether to use
   a wagon group or individual wagon assignment. Single wagon assignments are used for
   specific vehicles like dining cars or luggage cars.
2. **Cargo flow max wagon count**: A planning-time advisory set by the planner, not a
   hard validation rule. There may also be a max wagon/axle count for the train as a whole,
   but enforcement is operational — the train driver respects the limit when attaching wagons.
3. **Variable speed limits on stretches**: Not modelled in scheduling (see DM-4.3.1).
   Actual running times are dominated by operational factors outside the planner's control.
4. **Collaborative conflict resolution**: Last-change-wins with fast propagation (see FR-3.13.2).
   Planners divide work by scope in practice.
5. **Shadow yard region mapping**: Configured per layout, not per timetable (see DM-4.1.1).
6. **Vehicle schedule model**: Unified — one schedule structure assignable to locomotives,
   single wagons, wagon groups, or cargo flows (see DM-4.4.3).
