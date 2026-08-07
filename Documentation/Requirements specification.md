# Timetable Planning System — Requirements Specification

> **Status:** Draft
> **Last Updated:** 2026-07-14

This is the main specification document for the Timetable Planning System,
a single application for planning model railway operations based on schedules.

> **Implementation status annotations.** As of 2026-07-14 this document is
> annotated with the state of the implementation in this solution. Each functional
> (§3) and data-model (§4) subsection carries a **Status** line, and the
> [Implementation Status Overview](#implementation-status-overview) below summarises
> coverage. Legend:
>
> | Marker | Meaning |
> | ------ | ------- |
> | ✅ Implemented | Built and matching the spec (minor naming differences noted inline) |
> | 🟡 Partial | Partly built; what is missing is noted |
> | ❌ Missing | Not yet implemented |
>
> These markers describe *what exists today*, not a change to the requirement. A ❌
> item is still a requirement, just not yet built.

## Implementation Status Overview

Snapshot of coverage by area (see each section for detail).

### Domain model (§4)

| Requirement | Status | Note |
| ----------- | ------ | ---- |
| Operation locations + subtypes (§4.1.1) | ✅ | owner on every location; a shadow station's regions are chosen from the layout's region catalogue (each region has a name, a country and a palette colour); a location where cargo is exchanged with nobody on duty can name the manned station holding the key to its switches |
| Station tracks (§4.1.2) | ✅ | |
| Track / Timetable / Dispatch stretches (§4.1.3–5) | ✅ | |
| Companies (§4.1.6) | ✅ | each company names the country it operates in, chosen from the layout's country catalogue |
| Country catalogue (§4.1.7) | ✅ | the layout saves the set of countries it uses; companies, regions and the default country all reference it |
| Train (§4.2.1) | ✅ | maximum speed, train length (axles/wagons/metres) and train continuation |
| Train categories (§4.2.2) | ✅ | default speed and start number; categories are a catalogue on the timetable, seeded with Passenger and Freight |
| Station calls, wagon groups, sessions (§4.2.3–4, 4.2.6) | ✅ | station calls distinguish a stop from a pass-through; a wagonset's individual wagons are listed on the wagonset itself (§4.2.4); sessions are a catalogue on the timetable |
| Cargo flows (§4.2.5) | 🟢 | reusable cargo-flow descriptions catalogue plus per-train occurrences; the **Cargo flow** tab (descriptions + per-train editor); destination note generated (report rendering pending) |
| Speed mapping / fast clock / station timings (§4.3) | ✅ | effective-speed calculation wired (train speed capped by stretch speed, mapped to a real model speed) |
| Schedule top level (§4.4.1) | ✅ | terminology: what this document calls a *Schedule* (the whole operating plan) is called a *plan* in the data model; what it calls a *Vehicle Schedule* is called simply a *schedule* |
| Vehicles inventory (§4.4.2) | ✅ | includes a DCC address |
| Vehicle schedules / driver duties (§4.4.3–4) | 🟡 | a vehicle schedule is a reusable, type-agnostic sequence of train parts; interactive and automatic turnus building and session-aware vehicle assignment (§3.8); wagon-group assignment editor not wired |
| Note generation system (§4.5.1–4) | 🟡 | notes are assembled from schedule data and localised texts, in plain text and styled markup; about half the note types are built (see §4.5.2), the rest are not |
| Manual note translations (§4.5.5) | 🟡 | a single language code, not a translation collection |

### Functional / UI (§3)

| Requirement | Status | Note |
| ----------- | ------ | ---- |
| Settings tab (§3.2) | ✅ | all 5 groups + language selector |
| Layout Operational Places (§3.3) | 🟡 | locations + tracks + manned/shadow editor and the lock key built; ModuleRegistry import (FR-3.3.1) missing |
| Track/Dispatch/Timetable Stretches (§3.4) | ✅ | three sub-sections; direction warnings; auto dispatch + route builder |
| Train Categories (§3.5) | ✅ | list + add/edit/delete; delete blocked when referenced by a train; start number and exclude-from-automatic-scheduling editable |
| Trains (§3.6) | ✅ | Trains tab: inline-edit rows + expandable calls / wagon-groups sub-tables; add/clone/move trains; calls listed in travel order, editing a departure shifts the times after it and an arrival the times before it; pass-through set via arrival/departure checkboxes; conflict highlighting |
| Graphical Timetable (§3.7) | 🟡 | renders + display settings + orientation + stretch/half selection + conflict highlighting; interaction (drag, context menu) is still not built |
| Vehicle Schedule Editor (§3.8) | 🟡 | Schedules tab with a turn chart: interactive and automatic turnus building, session-aware vehicle assignment, vehicle editing with wagon rakes; wagon-group assignment editor not wired |
| Vehicle Owners (§3.9) | ❌ | stub page |
| Automatic time calculation UI (§3.10) | 🟡 | editing a call time shifts the times on one side of it — after a departure, before an arrival — keeping run and dwell times; locking individual times and recomputing from the travel-time calculation while editing not built |
| Validation (§3.11) | 🟡 | rules organised by scope (Layout/Timetable/Schedule/Plan); L2–L4, T1–T5, S1–S5, P1/P3–P5 done; closure (S3+S5) judged per traction unit by flow conservation; vehicle identity (P5) is the imported external id or else operator + number, refused at entry and reported for older plans; P2 partial; L1 emergent. GUI feedback: toolbar indicator + list, conflict highlighting on the graphical timetable and the Trains and Schedules tabs, click-to-locate |
| Reports (§3.12) | 🟡 | shell + page formats present; 2 reports built — Turnus Cards and a paginated tabular Timetable report |

### Integration (§5)

| Requirement | Status | Note |
| ----------- | ------ | ---- |
| Import from previous plans / JSON (§5.1) | ✅ | |
| External service import — categories, companies (§5.2) | 🟡 | categories + ~9,700 companies done; ModuleRegistry not built |
| XPLN import (§5.3) | ✅ | ODS/XLSX |
| JSON export (§5.4) | ✅ | SQLite produced externally by an online service (§5.5, future), not in the app |

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
| **Settings on the layout**      | All configurable settings live on the layout, grouped by purpose, and are persisted and re-applied with it |

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
| Driver Duty                | Dienst              | A continuous assignment of a driver to a sequence of train parts                                                 |
| Train Part                 | Zugteil             | A segment of a train's journey used for assigning vehicles or drivers                                            |
| Wagon Group                | Wagengruppe         | A group of wagons within a train, tracked between origin and destination                                         |
| Cargo Flow                 | Güterverkehr       | A flow of cargo to specific destinations, scheduled like a vehicle but assigned to a cargo flow object           |
| Sessions                   | Verkehrstage        | Operating day patterns controlling which sessions a train, duty, or vehicle runs                                 |
| On-Demand Train            | Bedarfszug          | A train that only runs when needed                                                                               |
| Fast Clock                 | Schnelle Uhr        | An accelerated clock used during operation; the ratio of model time to real time                                 |
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
shall be defined. The interface uses a tab-based organisation. **The tab order reflects the
recommended order of data entry** when building a layout from scratch:

1. **Settings** — name of layout, timing parameters, and other default values (see §3.2)
2. **Countries** — the catalogue of countries used by the layout; companies and regions reference an entry here (see §3.2.2)
3. **Regions** — domestic regions and foreign countries for cargo-flow routing
4. **Layout Operational Places** — each expandable to show tracks; data entered manually or imported from external file/web API (see §3.3)
5. **Track and Timetable Stretches** — connections between operation locations and timetable stretches as sequences of track stretches (see §3.4)
6. **Companies** — railway companies that operate trains; each belongs to a country
7. **Train Categories** — list of train category entries that can be added, edited, deleted (if not referenced), and imported from file/web API (see §3.5)
8. **Trains** — list of trains, each expandable to show times, each time expandable to show notes; editable at all levels; groupable by category (see §3.6)
9. **Graphical Timetable** — one tab per timetable stretch; trains can be added, removed, and edited in the graph (see §3.7)
10. **Vehicle Schedule Editor** — build vehicle schedules and assign to locomotives, wagons, wagon groups, and cargo flows (see §3.8)
11. **Vehicle Owners** — who brings what of the needed rolling stock (see §3.9)
12. **Reports** — preview and print all printable content; reports are selected here and kept separate from the editing tabs (see §3.12)

A **Home** tab (start page) and an **Import** tab sit outside this sequence.

#### 3.1.1 Creating a new layout

From the **Home** tab the user can create a new, empty layout/timetable/plan from scratch
(the alternative to importing). All translatable values are generated in the current
GUI language. The new layout is seeded with these defaults:

| Area | Default |
| ---- | ------- |
| Layout name | "New layout" (localised) |
| Theme · Scale | European · H0 (1:87) |
| Default country | derived from the GUI language (sv→Sweden, da→Denmark, de→Germany, nb→Norway, en→United Kingdom); also added to the layout's country catalogue |
| Operating window | Start 06:00, End 18:00 |
| Graphical timetable | Horizontal; minute spacing 3, kilometre spacing 2, station spacing 100, track spacing 10 |
| Time & Speed | Clock speed 5; minimum station stop 2; speed points, loco runaround (5) and clearance (1) at their standard values |
| Validation | all checks on, standard thresholds; Integration empty |
| Countries | the default country only — the user adds more on the Countries tab |
| Regions | the standard regions for the default country (localised) |
| Operation locations · stretches · companies | none (added or imported by the user) |
| Train categories | Passenger (prefix `P`) and Freight (prefix `G`), localised, no company |

Creating a new layout replaces any plan currently loaded (confirmed first when one exists).

### 3.2 Settings

> **Status:** ✅ Implemented (Settings tab). All five groups
> (General, Graphical Timetable, Time & Speed, Validation, Import & Export) and the
> top-bar language selector (saved in the browser) are present.

All configurable settings are stored on the layout and are
persisted with it, so they are re-applied whenever the layout is reopened. Settings
are organised into groups by purpose, surfaced as sub-sections of the
**Settings** tab:

| Group               | Purpose                                                      | Detailed in  |
| ------------------- | ------------------------------------------------------------ | ------------ |
| General             | Session/day model and operating time window                  | this section |
| Graphical Timetable | View and print preferences for the graphical timetable       | §3.7.3       |
| Time & Speed        | Fast clock, speed mapping, distance factor, default station operational times | §4.3         |
| Validation          | Which validations run and their thresholds                   | §3.11.3      |
| Import & Export     | Module Registry API URL and key (importing operation locations; sending the plan for conversion) | §3.3, §5.5 |

**General settings:**

| Setting    | Description                                                        | Default      |
| ---------- | ----------------------------------------------------------------- | ------------ |
| Use days   | Present operating days instead of sessions                        | Use sessions |
| Start day  | Weekday of the first session when using days                      | Monday       |
| Start time | Fast-time start hour of operation                                 | 06:00        |
| End time   | Fast-time end hour of operation                                   | 18:00        |
| Break time | Optional fast-time hour that splits the graphical timetable into two halves (start–break and break–end), for printing across pages and for the on-screen first/last-half view (see §3.7.5) | None |

Start, end, and break time define the operating time window used by the graphical
timetable (see §3.7).

The **user-interface language** is a user-level preference, chosen via the language selector
in the top bar and persisted in the browser — it is not stored per layout.

#### 3.2.2 Countries

> **Status:** ✅ Built (Countries tab).

Each layout keeps a curated catalogue of countries, saved with the layout
so a plan is self-contained. A company, a region and the layout's default country all
reference an entry here.

- The catalogue starts with the layout's **default country** only (added when the layout is
  created); the user adds more on the Countries tab, choosing from **all predefined countries**
  (so foreign countries needed by regions or international trains can be added).
- A country **cannot be removed** while it is the default or is referenced by any company or region.
- The default country is set on the **Settings** tab; it is always kept in the catalogue.

The predefined catalogue (stable ids, never reused) is the reference data described in §4; the
Countries tab selects a subset of it into the layout.

### 3.3 Layout Operational Places

> **Status:** 🟡 Partial (Operation Locations tab). Entry and editing
> of operation locations (stations, signal-controlled and other locations), their tracks, and
> the manned/shadow flags are built. The ModuleRegistry import (FR-3.3.1) is **not** built (the
> API-key field exists in Settings).

The user adds, edits and deletes operation locations and their station tracks, and marks each
station as manned and/or a shadow station — these flags drive automatic dispatch-stretch
generation (§3.4.2). Where cargo is exchanged but nobody is on duty, the user also names the manned
station holding the key that unlocks the switches there, and what that key is called (§4.1.1).

#### FR-3.3.1 Data Import

- **From the [ModuleRegistry](https://moduleregistry.azurewebsites.net)**: if the modules have been submitted to the meeting layout, it shall be possible to import the operation locations using its web API. This API requires an API key, stored in settings.

### 3.4 Track, Dispatch and Timetable Stretches

> **Status:** ✅ Built (Stretches tab). The tab has three sub-sections —
> Track, Dispatch and Timetable stretches — with the active one remembered as a user
> preference. Direction consistency is checked and timetable-route contiguity is enforced.

#### FR-3.4.1 Track stretches

A **track stretch** connects two adjacent operation locations and carries its distance, track
count, speed and running time. The user adds, edits and deletes track stretches; the start and end
locations are chosen from the layout's operation locations, so direction is explicit.

All track stretches are expected to run in the same direction so trains traverse them without
unintended reversals. The editor surfaces any inconsistency (a reversed pair or a directed cycle in
the start-to-end direction graph) as a **warning** that lists the offending stretches, but does not block
saving. A track stretch used by a timetable stretch cannot be deleted.

#### FR-3.4.2 Dispatch stretches

A **dispatch stretch** runs between two dispatch endpoints — a manned or a shadow station — passing
through any unmanned locations between them. They are generated automatically from the track
stretches, recording the ordered track stretches they comprise,
and presented read-only with a **Regenerate** action. Whether a station is manned or a shadow station
is set on the Operation locations tab (§3.3).

#### FR-3.4.3 Timetable stretches

A **timetable stretch** is an ordered, contiguous sequence of track stretches forming a logical line.
The user gives it a number and optional description and builds its route by appending track stretches
one at a time; only stretches that continue from the current end are offered, keeping the route
connected. Each timetable stretch is selectable on the Graphical timetable tab (§3.7.5) and becomes
one graph.

### 3.5 Train Categories

> **Status:** ✅ Built (Train Categories tab).

The train categories are a catalogue saved on the timetable. The user adds, edits
and deletes categories (name, prefix, suffix, passenger/freight, colour and an optional operating
company). A category **cannot be deleted** while any train references it.

A new timetable is seeded with two standard categories, named in the layout's default language:
**Passenger** (prefix `P`) and **Freight** (prefix `G`), with no operating company.

*Still to come: import of categories from file/web API.*

### 3.6 Trains

> **Status:** ✅ Built (Trains tab). A table with inline-editable rows — identity,
> company, number, sessions, category, maximum speed, train length (axles/wagons/metres)
> and continuation (filtered to same-category trains starting after this one ends). Each
> row expands to a fully editable **Calls** or **Wagon groups** sub-table. Trains can be
> added, cloned and time-shifted. Station calls carry arrival/departure checkboxes that
> set stop vs pass-through (see DM-4.2.3). Edits are saved immediately, and deletion is
> blocked when other data depends on the train. Rows with scheduling conflicts are
> highlighted (§3.11).
>
> The **Calls** sub-table always lists the calls in the order the train travels them, from the
> first to the last. Editing a departure shifts the times after it and editing an arrival the
> times before it (see §3.10), so the run follows the change and the order is kept. Times that do not ascend, and a route that jumps
> a location instead of following the track stretches, are reported as conflicts on the train
> (rules T2 and T5 in §3.11). Only the **first or last** call may be deleted, which shortens the
> route at that end; a call in between is an operating location on the way, which the train
> cannot skip.
>
> *Still to come: call-level note editing (the note-generation system, §4.5, is not built).*

### 3.7 Graphical Timetable

> **Status:** 🟡 Partial (Graphical Timetable tab). SVG rendering, both orientations
> (FR-3.7.1), visual styling (FR-3.7.2), all display settings (FR-3.7.3) and the
> stretch/time-window selection (FR-3.7.5) are built, and trains with scheduling
> conflicts are highlighted (§3.11). Interaction (FR-3.7.4: drag times, context menu)
> is **not** built yet — no train selection, time dragging or context menu.

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

The display parameters are stored on the layout with the other Graphical Timetable settings
and re-applied when the layout is reopened. They are user preferences that apply to
both on-screen viewing and printing. Start, end, and break time come from the General
settings (see §3.2).

#### FR-3.7.4 Interaction

> **Status:** ❌ Missing. No train selection, time dragging, or context menu yet.

##### Change timing of a train or part of train
- Click to select a train: arrival and departure times become small draggable squares
- When dragging a departure, the dragged time and all later times are affected; when dragging an
  arrival, the dragged time and all earlier times (as in the Trains tab, see §3.10)
- To move a whole train in time, drag the first arrival time
- Click outside draggable squares to stop editing times

##### Train context menu
Right-click on a train to show a context menu: edit, duplicate, remove, select category.

#### FR-3.7.5 Stretch and Time-Window Selection

> **Status:** ✅ Built (Graphical Timetable tab).

The user shall be able to choose what part of the timetable is drawn:

- **Stretches** — one or more timetable stretches are selected; each selected stretch
  is drawn as its own graph. The selection is remembered with the layout.
- **Time window** — when a **break time** is set (see §3.2), a *Show* selector offers
  the **whole graph**, the **first half** (start–break) or the **last half** (break–end).
  Selecting a half limits the time axis to that part of the day, so a long day fits on
  smaller screens, especially with a vertical time axis. The selector is hidden when no
  break time is set. Train lines and minute labels are clipped to the visible window.

The chosen half is a **user-level preference** persisted in the browser,
not stored in the plan document, so it is retained across restarts and is independent of
which layout is open.

### 3.8 Vehicle Schedule Editor

> **Status:** 🟡 Partial (Schedules tab). The vehicle-schedule (turnus) editor is built
> as a Gantt-style **turn chart** — rows are schedules on a shared time axis, one block
> per train part. It supports **manual** building (new schedule, add/remove/edit parts,
> including partial from/to-call selection) and **automatic** building (greedy contiguity
> chaining, skipping categories flagged as excluded from automatic scheduling). Vehicles
> are created and assigned **session-aware** (offering only the sessions a vehicle is
> still free), and wagonsets can be given an individual **wagon rake** (§4.2.4).
> A part already in a schedule can be **reshaped** by changing its from- or to-stop; the
> neighbouring part that joins it is adapted to the new joint where its own train calls
> there, so shortening one part turns the return working round at the new place by itself.
> An edit the neighbour cannot follow is applied all the same and reported as a conflict.
> A train can also be worked into the **middle** of a working: each joint between two parts
> shows where the vehicle stands and for how long, and offers the trains it could make in
> that time. A leg that does not bring the vehicle back to where the working goes on is
> added all the same and reported as a conflict until the leg back is added, so an
> out-and-back trip is worked into a layover a leg at a time.
> Conflicting schedules are highlighted (§3.11). **Not yet wired:** assigning a schedule
> to a wagon group via an editor.
>
> *Naming: this is the glossary's Vehicle Schedule; this document's top-level Schedule is
> the whole operating plan (see DM-4.4.1).*

### 3.9 Vehicle Owners

> **Status:** ❌ Missing. The Vehicle Owners tab is a stub.

*To be detailed — managing who brings what rolling stock for the session.*

Beyond in-app entry, vehicle owners may **submit the rolling stock they will bring online** (planned
as a Module Registry feature). Those submissions are folded into the SQLite database distributed to
the on-premise dispatch applications, so the inventory is complete without the planner re-keying it
(see §5.5).

---

### 3.10 Automatic Time Calculation

> **Status:** 🟡 Partial. Editing a call time in the Trains tab now shifts the times on
> one side of it, in the direction the edited time belongs to. A **departure** carries
> the rest of the run with it: the times after the call move by the same number of
> minutes, and the train stands here for as long as the change. An **arrival** works
> backwards and carries the run leading up to the call: the times before it move by the
> same number of minutes, and the stand at the call absorbs the change. Either way the
> calls that move keep the run and dwell times they already have, and the calls on the
> other side stay where they are. At the train's origin there is nothing before the
> call, so setting its arrival only changes the driver's preparation time; at the
> terminus there is nothing after it, so setting its departure only changes the
> finishing time. The change is refused as a whole when it would take the train outside
> the plan's operating window. **Not built:** locking individual times, and recomputing times
> from the §4.3.1 travel-time calculation while editing — that calculation exists in
> the model and is applied on demand by **update all timings** on the graphical
> timetable, but a time typed by hand is shifted as typed, not recomputed.

The system shall calculate travel times between station calls using:

- The speed mapping (see §4.3) to convert scale speed to real model speed
- The track stretch distance
- The fast clock speed to convert real time to scheduled time
- Station-specific operational times (see §4.3)

When a user changes one time, subsequent times should shift accordingly,
with an option to lock individual times.

---

### 3.11 Validation

> **Status:** 🟡 Partial. Fully implemented: the Layout occupancy rules **L2–L3**, the
> Timetable train rules **T1–T5**, the Schedule rules **S1–S5**, and the Plan
> consistency rules **P1, P3–P5**, all with FR-3.11.6 output (severity, localised
> message, location + time range, involved trains). Closure (**S3 + S5**) is judged **per
> traction unit** by flow conservation over the operating period, so rotation schemes that
> return across sessions or across several schedules are correctly allowed. Missing or
> partial: **L1** (emergent only) and part-coverage (**P2**). A configured
> minimum-minutes-between-track-usage threshold exists but has **no backing validation
> yet**. See `Documentation/Validation.md`.
>
> **GUI feedback.** The conflicts are surfaced in the app: a briefly debounced recompute
> feeds a toolbar icon with a count badge and a severity-ordered list. The list is the
> source of truth, and offending objects are highlighted inline on the **graphical
> timetable**, the **Trains** tab and the **Schedules** turn chart. List items are
> click-to-locate (they navigate to the owning tab). Severity is derived per error type
> (genuine conflicts = Warning; speed and traction-coverage = Information). See
> `Documentation/Validation feedback plan.md`.

Validation rules are organised by the **model scope** they apply to, bottom-up through
the model hierarchy — **Layout, Timetable, Schedule, Plan**. Each scope owns its own
validation logic, into which the rules below are to be moved, improved, or newly implemented.

Rules are numbered by scope: **L** = Layout, **T** = Timetable, **S** = Schedule,
**P** = Plan. Two enforcement modes cut across the scopes:

- **Consistency (always enforced):** rule **P1** — data consistency is upheld
  continuously and cannot be switched off.
- **Publish-blocking (detected on demand):** every other rule *may* be violated while
  editing but must be resolved before the plan is **published**. These are computed on
  demand and surfaced as validation errors, not blocked at entry time.

In each table the **Status** column records the current implementation; a ❌ or 🟡 rule
is still a requirement, just not yet fully built.

#### FR-3.11.1 Layout scope — infrastructure occupancy (L)

How trains occupy the physical layout. A **meet** is two trains present at the same
operation location, or on the same track stretch, at overlapping times.

| Rule | Requirement | Status | Notes / gap |
| ---- | ----------- | ------ | ----------- |
| **L1** | Trains may only **meet** where there are **at least two tracks** — at an operation location or on a track stretch. A single-track location or stretch cannot host a meet. | 🟡 Emergent | Not asserted directly; follows from L2 (a meet at a single-track station forces both trains onto one track → conflict) and L3 (stretch capacity). No dedicated "a meet needs ≥2 tracks" diagnostic. |
| **L2** | **At most one train may occupy a station track** at any time; two trains meeting must therefore stand on different tracks. | ✅ | Overlapping calls on the same track by different trains are flagged. Exception: calls sharing the same vehicle (e.g. a loco change) are allowed. |
| **L3** | The number of trains **simultaneously on a track stretch** may not exceed the **number of tracks**, counting **both directions together**. Double track permits two concurrent trains (one each way, or two the same way). | ✅ | Simultaneous passings on a stretch are compared against its track count, counting both directions together. |
| **L4** | A **lock key** is in force only where the location still needs one — it exchanges cargo and has nobody on duty — and the station holding it is still manned. A key either change has left meaningless is **kept but ignored**, and reported. | ✅ | Manning is edited on both sides long after a key is set. An ignored key produces no notes and is kept, so undoing the manning change brings it back; the conflict says which change did it. Always enforced. |

> Layout **structural** validity — consistent track-stretch directions and contiguous
> timetable-stretch routes — is validated on the Stretches tab (see §3.4), not here.

#### FR-3.11.2 Timetable scope — train rules (T)

A timetable is the collection of trains on the layout; these rules validate the trains
individually and as a set.

| Rule | Requirement | Status | Notes / gap |
| ---- | ----------- | ------ | ----------- |
| **T1** | A train has **at least two station calls**. | ✅ | Checked. |
| **T2** | A train's call times must be **ascending**, and at each operation location **arrival must not be after departure**. | ✅ | Checked. The calls are read in the order the train runs them, which is the order of their times, so what is reported is a train whose times contradict themselves — above all one that reaches the next location before it has left the previous one. |
| **T3** | A train's **speed** between consecutive calls stays within the configured min/max thresholds. | ✅ | Checked against the configured min/max speed thresholds. |
| **T4** | When trains are equal on **Company + Category + Number**, each instance must run on **different, non-overlapping sessions**. | ✅ | Trains equal on company, category and number are flagged when any pair has overlapping sessions. Can be toggled off. |
| **T5** | A train's route must be **continuous**: every leg it runs, from one call to the next, must be a **track stretch of the layout**. A train travels a stretch by departing its start and arriving at its end, so it calls at both ends of every stretch on its way. | ✅ | Two successive calls with no stretch between them are flagged as a route that jumps a location. Two successive calls at the same location (a change of track) travel no stretch and are not flagged. This is also why only the first or last call of a train may be deleted: removing one in between would leave the route jumping the location it stood for. Can be toggled off. |

#### FR-3.11.3 Schedule scope — vehicle schedule / turnus (S)

Each vehicle schedule (turnus) is a sequence of train parts forming a circulation.

| Rule | Requirement | Status | Notes / gap |
| ---- | ----------- | ------ | ----------- |
| **S1** | A schedule's train **parts do not overlap in time** (one vehicle cannot be in two places at once). | ✅ | Checked. |
| **S2** | A **following part must start from the station where the previous part ends** (geographic contiguity). | ✅ | Each part, in working order, must start where the previous ended; applies to all vehicle types. **Skipped when the schedule's parts overlap in time** (S1 reports that instead). Entry already enforces contiguity; the check catches schedules assembled unconditionally, e.g. XPLN import. Can be toggled off. |
| **S3** | **Circulation closure.** Over the operating period a **traction unit** must return to where it began, so the layout's vehicle distribution repeats and the working can run again. **Exemption:** a unit whose trains are all **on demand** need not close (the sequence rule S2 still applies). | ✅ | Judged **per traction unit** by flow conservation: over every session worked, the unit must depart each station as often as it arrives there. A unit that works both a forward and a return leg closes even when the legs run on **different sessions** and even when they are **split across several schedules** (the rotation case). Wagons and cargo flows are not required to close. On-demand trains are marked at import. Can be toggled off. Implemented together with S5. |
| **S4** | A **traction unit is assigned for all of a train's sessions** (may be different units for different sessions/days), and a schedule that runs regular sessions has a vehicle assigned. | ✅ | Per-train, session-aware: every train must be hauled by a traction unit on every session it runs, provided through any schedule that works it (a wagonset has its own turnus with no traction, hauled by the loco's turnus). An orphan schedule with no vehicle is reported. On-demand trains and cargo flows are exempt. Complements P4's per-train, time-based coverage. Can be toggled off. |
| **S5** | When a traction unit works **different parts on different sessions/days**, its working across those sessions must still form a valid, closing circulation. | ✅ | Folded into the S3 per-unit closure check: because closure is judged over all the unit's parts across all sessions worked, a unit that runs different legs on different sessions closes as long as its movements balance overall. Can be toggled off. |

#### FR-3.11.4 Plan scope — cross-object consistency (P)

The plan is the aggregate root; these rules span the timetable, schedules and vehicles.

| Rule | Requirement | Status | Notes / gap |
| ---- | ----------- | ------ | ----------- |
| **P1** | **Referential integrity (always enforced).** No dangling references; an object that others depend on cannot be deleted; every station track referenced by a train exists in the layout. | ✅ | Deletion rules plus a check that every station track a train uses exists in the layout. |
| **P2** | **Every train part belongs to a schedule.** A part may appear in several schedules only for **non-overlapping sessions**; no part may be in two schedules whose sessions overlap. | 🟡 Partial | Partly served by P4 (no traction → gap) and P3 (session overlap). Not checked: that *every* part is scheduled, and the *same part* across overlapping-session schedules. |
| **P3** | **No vehicle is double-booked** — the same vehicle is not assigned to schedules that overlap in **both** sessions **and** clock time (a vehicle cannot be in two places at once). | ✅ | Requires both session overlap **and** clock-time overlap, so a vehicle working a morning then an afternoon turn on the same day is not flagged. |
| **P4** | **Traction coverage** — each train's run is covered by traction schedules **without gaps or overlaps** (a loco change at the same station is allowed). | ✅ | Checked. Complements the per-session view in S4. |
| **P5** | **A vehicle has an identity that names one vehicle.** It is the external id the vehicle was imported under where it has one, and otherwise its operator and number — the number alone with no operator. On any one session an identity may belong to only **one** vehicle, whatever kind of vehicle it is, so a wagonset and a locomotive may not share it either. Two vehicles may reuse an identity only for **non-overlapping sessions**. | ✅ | Adding or editing a vehicle refuses an identity another vehicle already holds, so it cannot be created; older plans keep theirs and every duplicate is listed once among the conflicts. An imported plan raises no new conflicts, since its external ids are already unique. Wagon groups are exempt — their identifier stands for a group of wagons, not a vehicle. |

#### FR-3.11.5 Validation Configuration

All publish-blocking validations shall be **individually toggleable**. Speed thresholds
and timing parameters shall be configurable. These settings are stored on the layout with
the other Validation settings (see §3.2). The rules that catch a plan contradicting itself —
consistency (P1) and lock keys (L4) — are always enforced and are not toggleable.

#### FR-3.11.6 Validation Output

Validation errors shall include:

- Severity level (Information, Warning, Error, System)
- Localised message text
- Location (station track) and time range for graphical highlighting
- Involved trains

---

### 3.12 Output and Printing

> **Status:** 🟡 Partial. The report shell (FR-3.12.0) and the A4L / A4P / Card page
> formats exist, and a **Reports** menu selects them. Two reports are built:
> **Wagon/Turnus Cards** (one card per vehicle × session combination) and a paginated
> **tabular Timetable report** (A4L, one table per stretch and direction; estimate-based
> pagination, verified in Chrome and Edge). A5 / A3L / Label formats are not yet created.
> See per-report markers below.

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

> **Status:** ❌ None of these four reports built yet.

#### FR-3.12.2 Vehicle Schedule Reports


| Report                       | Description                                                           | Format |
| ------------------------------ | ----------------------------------------------------------------------- | -------- |
| Locomotive Schedule Cards    | Individual loco assignment cards, optionally including shunting locos | Card   |
| Trainset Schedule Cards      | Trainset (passenger/freight) duty assignment cards                    | Card   |
| Wagon/Turnus Cards           | Twelve cards per sheet showing specific wagon assignments             | Card   |
| Graphic Locomotive Schedules | Graphical time-based view of locomotive assignments across turnus     | A3L    |

> **Status:** 🟡 Only **Wagon/Turnus Cards** built. Locomotive Schedule Cards,
> Trainset Schedule Cards and Graphic Locomotive Schedules ❌.

#### FR-3.12.3 Timetable Displays


| Report              | Description                                                        | Format |
| --------------------- | -------------------------------------------------------------------- | -------- |
| Graphical Timetable | Time-distance diagram per timetable stretch, filterable by days    | A3L    |
| Train Compositions  | Detailed car assignments per train with class, number, and routing | A4L    |

> **Status:** 🟡 A **tabular** timetable report is built (A4L, per stretch and direction) —
> a Buchfahrplan-style table of times per station, distinct from the A3L stringline. The
> **A3L graphical** Timetable report and **Train Compositions** are not built. (The
> graphical timetable still exists as an interactive editor, §3.7, but not yet as an A3L
> print report.)

#### FR-3.12.4 Operational Lists


| Report                   | Description                                                          | Format |
| -------------------------- | ---------------------------------------------------------------------- | -------- |
| Train Departure Labels   | Physical track labels with train number, operator, time, destination | Label  |
| Cargo Flow Destinations  | Displays how freight trains should be built and the order of destinations | A4     |
| Adjacent Dispatch Places | Contact list of neighbouring dispatch locations with phone numbers   | A4     |

> **Status:** ❌ None of these three reports built yet.

#### FR-3.12.5 Vehicle Start & Inventory


| Report              | Description                                                                                                     | Format |
| --------------------- | ----------------------------------------------------------------------------------------------------------------- | -------- |
| Vehicle Start Infos | Where vehicles must be at session start — multiple views: overview, per owner, per station, with DCC addresses | A4     |

> **Status:** ❌ Report not built. Its DCC-address dependency exists (§4.4.2).

#### FR-3.12.6 Planning Analysis


| Report                 | Description                                                                          | Format |
| ------------------------ | -------------------------------------------------------------------------------------- | -------- |
| Trains Time Allocation | Bar chart showing driver duty demand per time slot, colour-coded for capacity status | A4L    |

> **Status:** ❌ Not built.

---

### 3.13 Collaboration (Future Enhancement)

> **Status:** ❌ Not started. The app runs in local mode only (FR-3.13.1 storage
> in the browser); online collaborative mode (FR-3.13.2) is not built.

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

The layout carries all configurable settings, grouped by purpose
(see §3.2).

#### DM-4.1.1 Operation Locations

> **Status:** ✅ Implemented. Name, signature, subtypes (Station, Signal-Controlled and
> Other Location), the shadow flag and per-station timings are present, and an owner and
> meeting-specific instructions are on every operation location. The region catalogue is owned by the layout; a station's
> regions reference a subset of it. A region carries a single name (in the layout's default
> language), the country it belongs to (defaulting to the layout's default country) and a
> background colour chosen from a fixed palette (see DM-4.5.4). The standard regions are
> named in the layout's default language. A location that exchanges cargo with nobody on duty can
> name the manned station holding the key to its switches, and what that key is called. Operation
> locations, their tracks and region assignments are edited on the **Operation Locations** tab.



The system shall support defining operation locations with:


| Property  | Description                                          | Required |
| ----------- | ------------------------------------------------------ | ---------- |
| Name      | Full station name                                    | Yes      |
| Signature | Short code (e.g. "Hb")                               | Yes      |
| Type      | Station, Signal-Controlled, Other                    | Yes      |
| Owner     | Module owner (for FREMO)                             | No       |
| Instructions | Markdown text for how this location is worked at this meeting — which tracks are used for what, how the shunting is arranged. General instructions on operating the location come from its owner. Available at a station or an industrial area, where passengers and/or cargo are exchanged; never at a signal-controlled or other location | No |
| Is Shadow | Hidden yard at line end                              | No       |
| Regions   | Regions/countries represented (shadow stations only) | No       |
| Lock key  | The manned station holding the key that unlocks the switches here, and optionally what the key is called. Available only where cargo is exchanged and nobody is on duty — an unmanned station or an industrial area. Drives the lock key notes (see DM-4.5.2). A key the manning on either side has left meaningless is kept but ignored, and reported as a conflict (see L4 in §3.11.1) | No       |
| Timings   | Per-station operational time overrides; each value optional, inheriting the layout default when unset (see §4.3.3) | No       |

Subtypes:

- **Station** — manned location; may be a shadow station. Shadow stations represent
  the outside world and are configured with the regions and countries they serve
  (used for cargo flow routing, see DM-4.2.5)
- **Signal-Controlled Location** — unmanned, controlled by signals
- **Other Location** — any other operational point

#### DM-4.1.2 Station Tracks

> **Status:** ✅ Implemented — all properties present.

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

> **Status:** ✅ Implemented.

The system shall define physical connections between operation locations:


| Property     | Description                                                    |
| -------------- | ---------------------------------------------------------------- |
| Start / End  | The two connected operation locations                          |
| Distance     | Length in meters (model scale)                                 |
| Tracks Count | Number of parallel tracks (1 = single track, 2 = double track) |
| Speed        | Maximum permitted scale speed (km/h)                           |
| Time         | Calculated travel time based on speed mapping                  |

Reports and graphs convert this stored metre distance into a displayed kilometre
figure using the distance factor (see §4.3.4).

#### DM-4.1.4 Timetable Stretches

> **Status:** ✅ Implemented.

The system shall support grouping track stretches into named lines:


| Property    | Description                         |
| ------------- | ------------------------------------- |
| Number      | Line number                         |
| Description | Line name                           |
| Stretches   | Ordered sequence of track stretches |

Timetable stretches are the unit for graphical timetable display.

#### DM-4.1.5 Dispatch Stretches

> **Status:** ✅ Implemented.

The system shall define dispatch territories between dispatch endpoints — manned or shadow stations.
Besides its from/to endpoints, a dispatch stretch records the ordered, contiguous track stretches it
comprises and exposes the unmanned locations it passes through. They are generated by following track
stretches from each endpoint, passing through unmanned locations.

#### DM-4.1.6 Companies

> **Status:** ✅ Implemented. A company references its country (an entry in the layout's
> country catalogue). A company operating in several countries is added once per country of
> operation; in a selection drop-down it shows its name followed by the country, e.g.
> "DB Cargo Germany".

The system shall maintain railway companies operating on the layout:


| Property   | Description    | Remark                                                        |
| ---------- | :------------- | ------------------------------------------------------------- |
| Name       | Company name   | Can be both real and fictive companies                        |
| Signature  | Short code     | For real companies, use the UIC-assigned code; unique per layout |
| Country    | Country        | References a country in the layout's country catalogue; drives report language |

#### DM-4.1.7 Country catalogue

> **Status:** ✅ Implemented.

Each layout saves the set of countries it uses, a curated subset of the predefined country
catalogue (stable ids 1–14 from the Module Registry plus a 101+ overflow range; ids are never
reused). Companies, regions and the default country all reference an entry by its id. A lookup
resolves an id (preferring the saved catalogue, falling back to the predefined list), and
referenced countries are kept present.

---

### 4.2 Timetable and Trains

#### DM-4.2.1 Train Definition

> **Status:** ✅ Implemented. Number, category, company, sessions and maximum speed are
> present. The model also has train continuation (continues-as / continues-from) and train
> length.

The system shall support creating trains with:


| Property  | Description                        | Required           |
| ----------- | ------------------------------------ | -------------------- |
| Number    | Train number                       | Yes                |
| Category  | Train category (see DM-4.2.2)      | Yes                |
| Company   | Operating company                  | Yes                |
| Sessions  | Which sessions this train runs     | Yes (default: all) |
| Max Speed | Train's maximum scale speed (km/h) | No                 |

#### DM-4.2.2 Train Categories

> **Status:** ✅ Implemented. Prefix, suffix, passenger/freight flags, display name, colour
> and default speed are present, plus start number (the first train number offered for the
> category — seeded 1 for Passenger, 5000 for Freight) and exclude-from-automatic-scheduling
> (skipped by the automatic turnus builder, §3.8). The catalogue is held on the timetable,
> seeded with the Passenger/Freight pair.

The system shall support configurable train categories:


| Property                  | Description                                                                       |
| :-------------------------- | :---------------------------------------------------------------------------------- |
| Prefix / Suffix           | For train identity formatting (e.g. "IC", "G")                                    |
| Is Passenger / Is Freight | Classification, can be none, one of or both                                       |
| Color                     | Display color in graphical timetable and otherwise where color makes sense        |
| Display Name              | Name of category                                                                  |
| Default Speed             | Default scale speed for this category (km/h); used when no per-train speed is set |
| Start Number              | First train number offered when adding a train of this category                   |
| Exclude from auto-scheduling | When set, the automatic turnus builder never seeds or chains this category (manual add still allowed) |

#### DM-4.2.3 Station Calls

> **Status:** ✅ Implemented. A call models **stop vs pass-through** explicitly (it is a
> stop when it has an arrival or a departure, otherwise a pass-through). Equal
> arrival/departure times is only an XPLN **import convention**, not the stop test. A
> signal-controlled location (block) is always a pass-through regardless of the flags.

Each train shall have an ordered sequence of station calls:


| Property  | Description                      |
| ----------- | ---------------------------------- |
| Track     | The station track used           |
| Arrival   | Arrival time (fast-clock time); cleared to mark a departure-only or pass-through call |
| Departure | Departure time (fast-clock time); cleared to mark an arrival-only or pass-through call |
| Is Stop   | Whether the train stops here (it has an arrival or a departure); otherwise it passes through |
| Notes     | Call-specific notes              |

- The first arrival time on a train is the last expected show-up time for the train driver
- The last departure time on a train is the expected ready time for the train driver
- On the Trains tab the arrival/departure checkboxes set stop vs pass-through per call

#### DM-4.2.4 Wagon Groups

> **Status:** ✅ Implemented. The individual wagons of a wagonset are held as an ordered
> rake on the wagonset (present only for wagonsets), running the whole schedule the wagonset
> is assigned to. It is edited in the vehicle editor (number of wagons + a re-sequenceable
> wagon list) and persisted with the plan. This is distinct from the per-part wagon group on
> a train part. Freight wagons a train couples and later uncouples over a segment are
> modelled as a **cargo flow** (DM-4.2.5) instead.

**Direction-dependent ordering:** Wagon position is direction-dependent. A consist ordered
1-2-3-4 becomes 4-3-2-1 when the train reverses direction. A tested helper splits the
schedule into legs and flips the order at every travel-direction change. It has **no report
consumer yet** — the Turnus Card does not apply direction changes — and will be wired into
whichever report needs direction-aware wagon order once one is specified.

#### DM-4.2.5 Cargo Flows

> **Status:** 🟢 Implemented. A cargo flow's routing is a reusable description (name;
> destinations with the and-regions / and-beyond / and-local-destinations options and max
> wagons/axles; origins; to-all-destinations) held in a catalogue on the timetable, routing
> to the shadow-shunting-yard regions on a station (DM-4.1.1). Each occurrence references a
> description and records its from-call/to-call, position and per-occurrence shunting/couple
> flags. Edited on the **Cargo flow** tab (Cargo destinations + Cargo trains sub-tabs);
> deletion is guarded. The destination note is generated from the occurrence; rendering it
> in the printed reports (§4.5.4) is pending. The XPLN importer creates a cargo-flow
> scheduled object directly; aligning it with this catalogue model is pending.

The system shall support cargo flow scheduling, which is distinct from wagon/vehicle scheduling.
A cargo flow describes the movement of cargo to specific destinations, assigned to a
cargo flow object rather than to a locomotive or wagon group.

Each cargo flow has one or several destinations, which can be:


| Destination Type       | Description                                              |
| ------------------------ | ---------------------------------------------------------- |
| Local                  | An operation location within the layout                  |
| External               | Sent to the corresponding shadow shunting yard                    |
| To All Destinations    | Cargo goes to all destinations on the flow                        |
| And Local Destinations | Including local stops                                             |
| And Regions            | Including specific regions attached to a shadow shunting yard     |
| And Abroad             | Including countries/regions represented by a shadow shunting yard |

Shadow shunting yards represent parts of the outside world — specific regions and/or countries.
Cargo destined externally is routed to the shadow shunting yard that represents the relevant region.
Region-to-shadow-shunting-yard mapping is configured per layout (see DM-4.1.1).

A cargo flow is scheduled using the common vehicle schedule mechanism (see DM-4.4.3) —
the same sequence-of-train-parts pattern used for locomotives and wagon groups,
but assigned to a cargo flow object instead.

#### DM-4.2.6 Sessions

> **Status:** ✅ Implemented. Bit patterns 1–14, predefined patterns, day mapping,
> and/or/overlap, and number-or-day-name display all present.

The system shall support 1–14 operating sessions with:

- Predefined patterns: All, Odd, Even, Thirds, On-Demand
- Day-of-week mapping (Monday–Sunday) for weekly patterns
- Operations for combining and testing overlap
- Trains, vehicle schedule assignments, and driver duties can each specify their active sessions
- Display as actual session numbers or localised day names

---

### 4.3 Speed Mapping and Time Calculation

#### DM-4.3.1 Multi-Point Speed Mapping

> **Status:** ✅ Implemented. The three configurable points, piecewise-linear
> interpolation (clamped at the ends) and the effective-speed and travel-time calculation
> are wired and unit-tested. *Note:* applying these to recalculate call times in the UI is
> the separate §3.10 work, still ❌.

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
effectiveScaleSpeed = min(train max speed, or the category default; stretch max speed)
effectiveRealSpeed  = interpolate the effective scale speed on the speed mapping
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

> **Status:** ✅ Implemented (default 5×).

The system shall use an expected fast clock speed (integer multiplier, e.g. 5×)
to convert between real time and scheduled (model) time:

```
scheduledMinutes = realSeconds / 60 × expectedFastClockSpeed
```

All times in station calls and timetables are in fast-clock time.

#### DM-4.3.3 Station Operational Times

> **Status:** ✅ Implemented. The per-field, unset-inherits-default design is in place.

The following real-world durations are configurable. The layout-wide defaults are
stored on the layout; each station may override any individual value. Overrides are per
field — an unset value inherits the layout default — so imported stations can carry only
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

The per-field, unset-inherits-default design means defaults are resolved at the point of
use, not copied onto each station. Imports must therefore **not** copy the layout defaults
onto each station: a station with no explicit value stays unset, so it continues to track
the layout default if that default is later changed.

When a future import refreshes layout operational locations into an existing plan (a merge,
rather than the current full rebuild), it must **preserve any timing overrides already set**
on a matching location — only locations new to the plan get their timings from the import.
This keeps user-entered per-station overrides from being overwritten on re-import.

#### DM-4.3.4 Distance Display Factor

> **Status:** ✅ Implemented (default 1×).

A track stretch's distance is stored in metres (model scale). The system shall apply a
configurable distance factor to convert that stored metre value into the kilometre
figures shown in timetable reports and the graphical timetable's station markers:

```
displayedKilometres = round(storedMetres × distanceFactor)
```

Kilometre figures are always whole numbers, so the scaled value is rounded to the
nearest kilometre (halves rounded upwards). For a stretch that branches off another
line, the offset from the junction is added before scaling, so both lines show the
same kilometre at the junction station.

The factor defaults to 1, so reports show the same number as before this setting
existed. Raising the factor lets a layout present a larger, more prototype-like
kilometre count for the same physical stretch length; it has no effect on travel-time
calculations, which use the stored metre value directly.

---

### 4.4 Schedule and Vehicles

#### DM-4.4.1 Schedule

> **Status:** ✅ Implemented. **Naming difference:** in the data model this document's
> top-level *Schedule* (Umlaufplan) is called the *plan*, and a *Vehicle Schedule*
> (DM-4.4.3) is called simply a *schedule*.

A schedule is the top-level planning artifact combining:

- A timetable (trains and their station calls)
- Vehicle schedules (locomotive and trainset assignments)
- Driver duties
- Vehicle inventory

#### DM-4.4.2 Vehicles

> **Status:** ✅ Implemented (a vehicle is one of: Locomotive, Trainset, Wagonset, Cargo).
> The DCC address (optional; motorised vehicles only) is present, feeding the Vehicle Start
> Infos report (FR-3.12.5).

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

> **Status:** 🟡 Partial. A schedule is a reusable, type-agnostic sequence of train parts
> assigned to a vehicle (so one schedule can be reused across locomotives, wagons and
> cargo). The object-type-specific data lives in four optional, combinable option slots on
> each train part (traction, non-traction, cargo-flow and cargo-only). The **editor**
> (§3.8) builds schedules manually or automatically and assigns vehicles session-aware.
> Assignment to **wagon groups via an editor is not wired**.
>
> Model capabilities for the editor: an unguarded append (for XPLN import) vs a
> guarded append (enforcing contiguity, time overlap and shared sessions); a schedule's
> effective sessions; the unique session/day combinations a vehicle works (one turnus card
> each, §3.12.2); operating-period coverage/complement of a session pattern; and an
> automatic schedule builder.

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

> **Status:** ✅ Implemented.

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

> **Status:** 🟡 Partial. Notes are assembled from schedule data and localised texts as described
> below, in plain text and in styled markup, and a manual free-text note can be written by hand.
> Built so far are the loco and wagon connect/disconnect notes, loco exchange, moves to and from a
> parking track, reinforcement, cargo destinations, whether the train stops or exchanges anything,
> the lock key notes, and the meeting and overtaking notes. Still missing are turning and running a
> loco round, the loco driver sorting wagons, block origins and arrivals, scheduled wagons, the two
> passenger notes and the train continuation note.

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
| Lock key collected             | A later stop at a location whose key is held here | "Pick up key A1 for unlocking Bruket."        |
| Lock key handed back           | An earlier stop at a location whose key is held here | "Leave key A1 from Bruket."                   |
| Train meets                    | Trains meeting from opposite directions | "Crosses G 4012 14:23-14:28"                  |
| Train overtaking               | Train passing another that stands still | "Overtakes G 4012 14:23-14:28", "Is overtaken by G 4012 14:25" |
| Manual note                    | User-entered per language               | Free text, stored per language code           |

#### DM-4.5.3 Note Formatting

Notes shall be rendered as structured markup with semantic styling,
enabling consistent presentation across screen display and printed output:

- **Days prefix** — when a note applies only to certain sessions/days
- **Localized action text** — from the language resources (e.g. "Connect loco" / "Lok ankuppeln")
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

> **Status:** 🟡 Partial. A manual note carries its text and a single language code.
> The default-text-plus-translations collection (multiple translations per note) is
> not yet modelled.

Users shall be able to add manual free-text notes to any station call.
A manual note has a default text (in the planner's language of choice) and
optional translations as sub-items, each keyed by language code (e.g. "de", "sv").
When generating reports, the system uses the translation matching the requested
language; if no translation exists, the default text is used.

---

## 5. Integration Requirements

This section describes how data is exchanged with external systems and previous plans.

### 5.1 Import from Previous Plans

> **Status:** ✅ Implemented via a JSON round-trip (Import tab).

The system shall support importing reusable data from saved plans:

- Layout (stations, tracks, stretches)
- Train categories
- Companies
- Vehicle definitions

### 5.2 External Service Import

> **Status:** 🟡 Partial. Train categories (from a CSV file) and the ~9,700-company
> dataset (from a JSON file) are loaded. Module/station data from the ModuleRegistry API
> (FR-3.3.1) is **not** implemented.

The system shall support fetching reference data via web API:

- Train categories from a shared service
- Company data (existing JSON dataset of ~9,700 railway operators)
- Potentially: module/station data from a module registry

### 5.3 XPLN Import (Legacy)

> **Status:** ✅ Implemented (ODS + XLSX spreadsheets).

The system shall support importing complete schedules from XPLN spreadsheets
(ODS/XLSX format) as described by the existing XPLN importer.

### 5.4 Export

> **Status:** ✅ JSON export implemented. The "SQLite" option in the export dialog is a
> disabled placeholder; SQLite is produced by an external online service, not by this
> application (see §5.5).

The system shall export schedules in JSON to two destinations, chosen in the export dialog:

- **Save to disk** — downloads a `.json` file for backup, archival or transfer to other users.
- **Send to Module Registry** — sends the plan JSON to the Module Registry API (URL and key from
  the **Import & Export** settings), where it is converted and distributed as SQLite (§5.5).

Both show a progress indicator while the plan is serialised (a large graph) and sent. The export
dialog also lists SQLite as a disabled, "via Module Registry" placeholder.

### 5.5 SQLite distribution (online conversion service)

> **Status:** ❌ Future feature, hosted **outside** this application — planned as part of the
> [Module Registry](https://moduleregistry.azurewebsites.net).

On-premise applications used at module meetings (train dispatch, station displays, etc.) consume a
**SQLite database** rather than JSON. Producing SQLite in the browser would require the database
engine to run inside the browser (a heavy build); instead the conversion is delegated to an online
service:

1. The planner exports the plan as JSON (§5.4) and it is uploaded to the service.
2. The service builds a SQLite database from the JSON using the **server-side** database model
   (which already targets SQLite), and offers it for download as a database file.
3. The service may **enrich** the database with data collected online — in particular
   **vehicle-owner submissions** (§3.9): owners register the rolling stock they will bring, which is
   added to the downloadable database so the on-premise apps have the full inventory.

The downloaded database is one-way (downstream) output; it is **not** re-imported into the planner,
which continues to import only JSON and XPLN (§5.1–5.3). This keeps the in-browser planner light
and reuses the tested server-side database mapping. The in-app "SQLite" export option remains a
placeholder until this service exists.

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
5. **Shadow shunting yard region mapping**: Configured per layout, not per timetable (see DM-4.1.1).
6. **Vehicle schedule model**: Unified — one schedule structure assignable to locomotives,
   single wagons, wagon groups, or cargo flows (see DM-4.4.3).
