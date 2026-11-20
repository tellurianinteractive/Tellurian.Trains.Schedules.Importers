# Operation locations: one model for four systems

Written 2026-11-18 in the Module Registry repository and moved here the same day, so that the work on the shared model can be done here. Status: analysis and design direction. Nothing is built. What is decided and what is still open is listed in section 2 and section 9.

Updated 2026-11-19 with the structure of the shared model as built in `Model.Layouts` 0.2.0: decisions 26 to 33, and sections 4.2, 4.3, 4.6 and 4.7. What the planner's own move onto it takes is in `ModelHarmonisation.md`.

Operation locations (stations, halts, junctions, block posts and their tracks) are defined in four systems. This document compares them, sets out a common design, and says what each system needs, with most detail for the Module Registry.

| | System | Repository | Model read | Role today |
|---|---|---|---|---|
| a | Module Registry | `Modules Registry App` | `SourceCode/Data` (`Station`, `StationTrack`, `LayoutStation`, `Module`, `ModuleExit`) | The owner's data about modules and stations, and that they come to a meeting |
| b | Schedules (the planner) | this one | `Model/Layouts`, model version 3.12.0 | How the planner uses the locations in a timetable |
| c | Drawing (Module Draw) | `Tellurian.Trains.Drawing` | `Tellurian.Trains.Drawing.Model`, release 0.8.0 | The geometry of modules and how they are joined in a hall |
| d | Dispatch | `Tellurian.Trains.Dispatch` | `Tellurian.Trains.Dispatch/Layout` and `/Trains` | The running state of trains between locations |

## 1. Summary

**Today**

- The Schedules model is the richest and the only one with a type hierarchy. Its concepts and names are the right vocabulary for all four systems.
- The Registry holds most of what the planner needs about stations and tracks, but only one kind of location, and nothing about how locations are connected.
- Drawing has no operation location at all, only a station-name label and a designation on each track element.
- Dispatch is built on the Schedules model, but on package version 2.1.0, and its conversion drops or hard-codes several fields.
- Three mismatches would give wrong data silently: swapped numbers for exit direction, the unit of a stretch's distance, and the API key header (section 3.5).
- The Registry's API key is the user's `ObjectId`, which is also the secret in the password link. This is a security fault that should be corrected on its own, first (section 6.7).

**Direction**

- The **module file** of the Drawing app is the source of truth for what a module and its operation location are. The Registry keeps the file, a copy of its properties in columns, and everything that relates the module to people and meetings.
- An **operation location is part of the module file**, with a list of tracks, so that a drawing is enough on its own for users without the Registry.
- A **hall plan** is the layout. The planner loads a hall plan, from a file or from the Registry, and gets locations, tracks and track stretches from it.
- **One shared contract** for module, location, track and stretch is used in the module file, the hall plan, the Registry's API and the planner's import.
- **The contract has two levels**, as the Registry's database: what survives meetings (the module with its location and tracks) and what is decided for one layout (the layout module with its decisions). The decisions can be entered when the hall is planned and changed when the timetable is.

## 2. Decisions

| # | Decision | State |
|---|---|---|
| 1 | The module file is the source of truth for module and operation-location data; the Drawing app can enter all of it | Direction set; the four conditions in 4.1 are open |
| 2 | Standards and end profiles are kept in the Registry; other sources are possible for users without it | Decided |
| 3 | Drawings with metadata are generated for the modules and stations already in the Registry | Decided; details in 6.5 |
| 4 | Left and right replace east and west, with up and down when needed; the viewing side is the non-public side, else the operation side, else the user's choice, the same for all variants | Decided; details in 4.3 |
| 5 | A hall plan is an ordered list of modules, each with a flip | Decided, to be built in the Drawing app |
| 6 | The planner loads a hall plan, not individual modules | Decided |
| 7 | Distance between locations is from centre to centre; each location module has a centre, by default its middle | Decided |
| 8 | A station is one module, split in sections | Fact |
| 9 | ~~The user draws each variant; the main variant is the largest~~ Replaced by 20 | Withdrawn 2026-11-19 |
| 10 | The Registry maps a few properties of a file and reads no geometry | Decided |
| 11 | The Registry fetches the file schemas from the Drawing app | Decided |
| 12 | The API key follows best practice | Proposal in 6.7 |
| 13 | The model is shared: the layout part of the Schedules model is cleaned up and becomes a package that all systems use | Decided; what must be fixed is in 4.2 |
| 14 | A signature is unique within its country of operation; the owner's country is used where that isn't set | Decided; details in 6.10 |
| 15 | The Schedules model is the starting point for operation locations; the Registry adapts | Decided; the adaptations are in 6.9 |
| 16 | Dispatch adapts to the same model, but is dealt with later | Decided |
| 17 | What belongs to another system is kept in the file's `externalReferences`, with one entry per system (`moduleRegistry`), which the Drawing app doesn't change | Decided; details in 4.3 |
| 18 | The owners of a module are saved with the drawing, as names and prefix only, without contact details; for a registered module the Registry writes them | Decided; the form is changed by 29: one text with the names, and the prefix on the module |
| 19 | Track direction is best assigned in the Drawing app, where the user sees the module's orientation; the Registry's form can assign it if it shows the same orientation | Decided; details in 4.3 |
| 20 | **Variants exist only in the Module Registry** (2026-11-19). A drawing is one complete module, and optionally an operation location. A module that can be set up in several ways is several drawings, each with its own file and GUID; the Registry alone knows that they belong together: they are `Module` rows of the same station, told apart by `ConfigurationLabel`. No other system has the concept. (`PackageLabel` is something else: the box the modules are transported in, decision 25.) Where this document says "per variant" (4.3, 4.4, 6.4), read "per module file"; the Registry's entry in a file's external references has one `moduleId`, not a list per variant | Decided; built in the Drawing app |
| 21 | **Every module has a track list**, with at least one track: a through track or a track ending. The operation location is an optional block beside it (`module.operationLocation`), with the location's own facts. A module without the block is plain line | Decided 2026-11-19; built in the Drawing app's model and file format |
| 22 | **Names in the contract** (2026-11-19): a track's name is `designation` (the planner renames `Number` with a plan migration when it moves onto the core); the country of operation is an ISO 3166-1 alpha-2 code, `countryCode`, not the id of a table; lengths are metres | Decided |
| 23 | **The shared model is the package `Tellurian.Trains.Schedules.Model.Layouts`**, project `Model.Layouts` in this repository, with no dependencies and a version of its own (2026-11-19). First version 0.1.0: `OperationLocation` with `OperationLocationKind`, `StationTrack` with `TrackDirection`, the rules of the kinds as checks (`OperationLocationRules`), and a first shape of the layout block (`Layout`, `TrackStretch`, `ExitSide`, and two types that 0.2.0 replaces with `LayoutModule`, `LayoutOperationLocation` and `LayoutStationTrack`). The Drawing app's module file uses the location and the track. The planner's own move onto it is not started | Built; not published to nuget.org. Version 0.2.0 has the structure of decisions 26 to 32 and breaks 0.1.0 |
| 24 | **In a hall plan an exit is referred to by side and number** (Left 1, Right 2), and a line entry has a turned flag and, for three or more exits, the exit the line continues from (2026-11-19) | Decided; built in the Drawing app's model and file format |
| 25 | **A package is what modules are transported in, usually a box** (2026-11-19). All modules in a box come as one package, and a participant submits packages to a meeting. `PackageLabel` names the box; it has nothing to do with variants. The label is saved in the module file (`module.packageLabel`) and can be entered in the Drawing app. When a file is uploaded, the Registry takes the file's label only if it has none itself; otherwise the Registry's label holds and is written into the file. Hall planning in the Drawing app is to be package aware | Decided; the property and its entry are built in the Drawing app |
| 26 | **The shared model has two levels, as the Module Registry's database** (2026-11-19). What survives meetings and hall plans: `Module`, with its optional `OperationLocation` and its `StationTrack`s. What is decided for one layout, that is one hall plan: `LayoutModule`, with its `LayoutOperationLocation` and `LayoutStationTrack`s. A `LayoutModule` contains the module as it was when placed and has the decisions beside it; when the module is updated from its source, the contained module is replaced as a whole and the decisions stay | Decided; built in 0.2.0 |
| 27 | **What is decided for a layout is part of the shared model**, not of the planner alone (2026-11-19): manned, controlled by, instructions, and what differs for a track. The other apps use it and a database is created from the model. The decisions are the preconditions of a timetable: they can be entered in Module Draw, in the hall plan, and changed in the planner. This replaces "facts only" in 4.2 and most of 4.7 | Decided; built in 0.2.0 for those named, the rest follow |
| 28 | **Tracks exist in one place, on the module**, with what holds in the long term; the Registry enters usage and lockable (2026-11-19). A lockable track can have the name of its key (`KeyName`). A layout says only what it decides differently for a track (`LayoutStationTrack`, by the id of the track): a usage that replaces the track's own or is added to it, and whether it can be locked. The planner enters these once the hall plan is imported | Decided; built in 0.2.0 |
| 29 | **The name and the owners of a module** (2026-11-19). A module has a name, a prefix and a number. Its designation is the prefix and the number as far as they are given (`SF 0012`, and `12` without a prefix); its display name is the standard and its scale, without a word said twice (`FREMO/H0-RE SF 0012 Tellby`, as Module Draw titles an end profile), followed by the designation and the name of its operation location (`SF 0012 Tellby`), with a number and no prefix its name followed by the number (`Curve 12`), or without a number its name followed by the configuration label. The designation is a FREMO name only when an owning person is a FREMO member with a FREMO prefix (`HasFremoDesignation`). The owners are one text, the names separated by commas (`ModuleOwners`), not a structure. The Registry sets or overwrites the owners, the prefix and the FREMO mark when a drawing is uploaded. The rule is in the shared model, so all systems name a module alike | Decided; built in 0.2.0 |
| 30 | **Ids in a layout are `int`**, as every id in the Registry: module, location, track and stretch (2026-11-19). They are unique within the layout for their kind, given once and never renumbered, and everything else refers by them. What identifies a thing outside the layout is in its external references | Decided; built in 0.2.0 |
| 31 | **Casing** (2026-11-19). In JSON, files and API, property names are camelCase and enum values are written by their name in the code, in PascalCase (`"kind": "SignalControlled"`). In a database, tables, views and columns are PascalCase, as in the Registry | Decided |
| 33 | **Standard and scale** (2026-11-19). A module has a standard (`Standard`), and a layout, that is a hall plan, a preferred standard (`PreferredStandard`). The scale is not stored beside it: it is that of the standard. A standard and a scale are referred to by their short name (`FREMO H0-RE`, `H0`), with the Registry's id stored beside it when it comes from there. The Registry defines them, and the other apps are later to load them from it | Decided; built in 0.2.0 |
| 32 | **Naming of types** (2026-11-19). Types are named as in the Registry's code (`Module`, `LayoutModule`, `StationTrack`, `LayoutStationTrack`, `TrackStretch`), except where the concept is extended: `Station` is `OperationLocation`, so `LayoutStation` is `LayoutOperationLocation`. No type has the kind of type in its name; the common base is `LayoutPart`. What is worked out from the stored properties is an extension member beside the type, never a stored property | Decided; built in 0.2.0 |

## 3. The four models today

### 3.1 Location types

| Concept | Registry | Schedules | Drawing | Dispatch |
|---|---|---|---|---|
| Base type | `Station` (only type) | `OperationLocation` (abstract) | none | `OperationPlace` (abstract) |
| Manned or driver-operated station | `Station` | `Station` | none | `Station` |
| Block post, junction, crossing place | `Station.IsJunction` flag; `Module.IsSignalModule` without a station | `SignalControlledLocation` (+ `TrainsCanCross`) | `Module.IsSignalModule`, `ModuleKind.Junction` (3+ exits) | `SignalControlledPlace` (+ `IsJunction`) |
| Unstaffed halt | none | `OtherLocation` | none | `OtherPlace` (+ `IsJunction`) |
| Freight-only area | `Station.HasCargoCustomers` with no platforms | `IndustrialArea` | none | missing |
| Shadow station | `Station.IsShadow` | `Station.IsShadow` | none | none |
| Station outside the layout | `ExternalStation` (cargo flows only) | `Region` on a shadow station | none | none |

### 3.2 Location properties

"Planner" marks data that is decided per timetable and stays in the plan.

| Property | Registry | Schedules | Dispatch | Note |
|---|---|---|---|---|
| Id | `Station.Id`, and `LayoutStation.Id` per meeting | `Id`, local to the plan | `Id`, from Schedules or generated | No shared id |
| Name | `FullName` (50), `LayoutStation.OtherName` | `Name` (`_` replaced by space) | `Name` | |
| Signature | `Signature` (5), `OtherSignature` (6) | `Signature`, the equality key | `Signature` | |
| Country | `Region.Country`, `LayoutStation.OtherCountry` | `CountryId` (ids 1–14 match the Registry) | `Station.PreferredLanguage` | |
| Regions represented | `LayoutStation.Regions`, `Station.RegionId` | `Station.Regions` | none | Maps directly |
| Owner | `LayoutParticipant.Person`, `ModuleOwnership` | `Owner` (text), `PhoneNumber` (int) | none | |
| Manned | `LayoutStation.IsManned` | `Station.IsManned` | `Station.IsManned` | |
| Staffing | `LayoutStation.Difficulty` | `StaffPosts`, `ShiftCount` | none | Different ideas; both worth keeping |
| Shadow | `IsShadow` | `IsShadow` | none | |
| Turntable | `HasTurntable` | `HasTurntable` | none | |
| Terminus, harbour | `IsTerminus`, `IsHarbour` | none | none | Registry only |
| Junction | `IsJunction` (entered) | `IsJunction` (derived from the stretches) | `IsJunction` (entered) | |
| Trains can cross | none | `SignalControlledLocation.TrainsCanCross` | `CanHostMeeting` = more than one track | Missing in Registry |
| Direction change possible | none | `IsChangingTrainDirectionPossible` | none | Missing in Registry |
| Passenger exchange | derived: shadow, or any track with a platform | `HasPassengerExchange` | none | Derivable |
| Cargo exchange | `HasCargoCustomers`, `StationCustomers` | `HasCargoExchange` | none | Maps directly |
| Key for switches | `IsKeyRequired` | `LockKey` (name, held at station) | none | Registry says *that*, planner says *where* |
| Owner's instructions | `OperationInstructionsMarkdown`, `PdfInstruction` | deliberately not held | none | See 3.5 |
| Meeting instructions | none | `Instructions` | none | Planner |
| Hide meets / passings | `HideMeetingTrains`, `HidePassingTrains` | `HideMeets`, `HidePassings` | none | Maps directly |
| Start labels | `PrintTrainStartLabels`, `PrintOnlyFirstTrainStartLabel` | `StartLabels` (None, First, All) | none | Maps directly |
| Controlled by | none | `ControlledBy` (station) | `ControlledByDispatcherId` | Planner |
| Cargo served from, timings, passenger interchange, split dispatch | none | yes | none | Planner |
| Switchgear, boosters | `TypeOfSwitchgearId` (unused), `BoosterTypes` | none | none | Registry only |
| The module of the location | `PrimaryModule` (`Modules` holds its variants) | none | `Module`, `ModuleVariant.RegistryModuleId` | A station is one module |

### 3.3 Station tracks

| Property | Registry `StationTrack` | Schedules `StationTrack` | Dispatch `StationTrack` | Drawing `TrackElement` |
|---|---|---|---|---|
| Number | `Designation` (5) | `Number` | `Number` | `Designation` (per element, not per track) |
| Order | `DisplayOrder` | `DisplayOrder` | `DisplayOrder` | none |
| Scheduled | `DirectionId != NotScheduled` (`IsScheduled` is obsolete) | `IsScheduled` | only scheduled tracks are loaded | none |
| Direction | `DirectionId`: EastWest, WestEast, Bidirectional | `PreviousLocationId`, `NextLocationId`, `AppliesInBothDirections` | none on station tracks | none |
| Main track | `IsSiding` (inverted) | `IsMain` | `IsMainTrack` | `IsMainTrack` |
| Stub | `IsThroughTrack` (inverted, to be confirmed) | `IsStub` | none | `BufferStop` element |
| Length | `MaxTrainLength` (m, double) | `Length` (m, double) | `MaxLength` (int) | `TrackLength` (mm, derived) |
| Platform | `PlatformLength` (m, nullable) | `PlatformLength` (m, 0 = none) | `PlatformLength` (int) | label `PlatformEdge` with `Length` (mm) |
| Speed limit | `SpeedLimit` (km/h) | none | none | none |
| Usage | `UsageNote` (20) | `Usage` | none | none |
| Lockable | none | `IsLockable` | none | none |

### 3.4 Lines between locations

| Concept | Registry | Schedules | Drawing | Dispatch |
|---|---|---|---|---|
| Stretch between two locations | none | `TrackStretch` (distance in m, `TracksCount`, speed, time, electrified) | `Line` with a `LineEntry` per module | `TrackStretch` (length in m, one `Track` per line track with direction) |
| Dispatch stretch | none | `DispatchStretch` | none | `DispatchStretch` (with live occupancy) |
| Timetable stretch | none | `TimetableStretch` | none | none |
| Position for a schematic | none | `TopologyPosition` (x, y) | computed poses | none |
| Module ends | `ModuleExit` (label, direction, end profile) | none | `ModuleExit` (label, direction, end profile, pose) | none |

### 3.5 Incompatibilities

**Location kind.** The Schedules model is the starting point, and the Registry adapts to it (decided 2026-11-18); every adaptation is listed in 6.9. Today the Registry has one type with flags, Schedules four kinds, Dispatch three. A Registry station can't say that it is a block post, a halt or an industrial area, and a signal module usually has no station row, so block posts and plain junctions don't exist as locations in the Registry. "Junction" is entered in the Registry and in Dispatch but derived from the stretches in Schedules; the derived one is the reliable one. Dispatch throws `UnreachableException` for an `IndustrialArea`.

**Identity.** Schedules ids are local to a plan and equality is by signature, case-insensitive. Dispatch equality is by id. Drawing uses GUIDs and keeps `RegistryModuleId` per variant. Nothing in Schedules can hold an id from outside, so an import can't be refreshed.

The Registry has two ids, with different reach (settled 2026-11-18):

| Id | Reach | Used in |
|---|---|---|
| `Station.Id` | Global: the location, wherever it is used | Drawings, the shared contract, the planner's external reference |
| `Module.Id` | Global: one variant of a module | Drawings (one per variant), hall plans |
| `LayoutStation.Id` | One layout: the location's part in one meeting | Only the Registry's answer for a layout, as the key of the meeting data. Never in a drawing |

- **Drawings use the module id and the station id**, nothing else from the Registry. They are kept in the file's external references (4.3), apart from the drawing's own properties. A station has one id and each of its variants has its own module id, so every variant's operation-location block carries the same station id.
- **The ids are written by the Registry.** A file made outside it has none until it is first saved there, and is identified by its GUIDs until then. An id found in an uploaded file is accepted only together with the GUID check for copied files (6.3).
- **In a hall plan** the module id of each placed module is what lets the Registry find the module's registration, and through it the `LayoutStation`.

**Signature.** 5 characters on `Station`, 6 on `LayoutStation.OtherSignature` and `ExternalStation`, no limit in Schedules. Schedules requires signatures to be unique in a layout; the Registry doesn't check uniqueness at all.

The requirement will be: **a signature is unique within its country of operation** (decided 2026-11-18). The Registry has no country of operation for a station today. Until it has, and afterwards for every station where it isn't set, the **owner's country** is used.

**Track direction.** Three conventions: a compass direction of the module (Registry), the neighbouring location (Schedules), forward or backward along a stretch (Dispatch). The first can be converted to the second only when the neighbours are known.

**Numbers and units.**

- Exit direction: Registry `Eastwards = 1, Westwards = 2`; Drawing `West = 1, East = 2`. Mapping by number swaps the two. Map by name only.
- Stretch distance: Schedules has metres. Dispatch's conversion, written against model 2.1.0, multiplies by 1000 as if it were kilometres.
- Lengths: metres as `double` in the Registry and Schedules, truncated to `int` in Dispatch, millimetres in Drawing. No platform is `null` in the Registry and `0` in Schedules.
- Main track and stub: `IsSiding` and `IsThroughTrack` are assumed to be the inverses of `IsMain` and `IsStub`. A terminus track can be a main track and a stub, so the meaning of `IsThroughTrack` must be confirmed.

**Two kinds of instructions.** The Registry holds the owner's general instructions. Schedules holds the planner's instructions for one meeting, and states that the owner's don't belong there. They must stay two things.

**API.** The Registry reads the key from `X-API-Key` or the `apiKey` query parameter; the planner's `ModuleRegistryUploadService` sends `Authorization: Bearer`. The public API is GET only. Its JSON uses `ReferenceHandler.Preserve`; the Schedules model uses a `$type` discriminator. The planner and the Drawing app are Blazor WebAssembly and call from the browser, and the Registry has no CORS policy for `/api`.

**Dispatch's conversion** also sets every station as manned, never sets the junction property, and doesn't copy the platform length.

## 4. The design

### 4.1 Source of truth

The module file is the source for what a module and its operation location *are*. The Registry is the source for how they *relate* to people and meetings.

| Truth in the file | Truth in the Registry |
|---|---|
| The module: measures, exits, sections, standard, theme, years, flags, states, note | Ids, and the link between a file's variants and module rows |
| The location: name, signature, kind, viewing side, centre, and its tracks with all their properties | Who owns it, and who may see and change it |
| The drawing | Customers, cargo and waybills |
| | Everything about a meeting: registrations, and `LayoutStation` (manned, name at the meeting, regions, start labels) |
| | The catalogue: standards and end profiles |

For users without the Registry, the file and the hall plan are all there is, and what the right-hand column holds is entered in the planner or not available (customers).

Four conditions, **not yet confirmed**:

1. **The boundary above.** Ownership stays with the Registry even though a file has an owners list: otherwise a file could give a module to someone else. Customers stay because entering cargo is not for a drawing app.
2. **The Registry's forms stay, as editors of the file.** Many members will never draw. Changing a note, or whether a track is scheduled, must not require the Drawing app. The forms change the properties in the stored file. The Registry's columns remain as a copy made from the file in the same transaction, so lists, searches, statistics and the API work unchanged.
3. **A check against lost changes, and a history.** The worst case is an older copy overwriting a newer one. Each stored version has a mark, an upload is accepted only when it carries the current mark, and several earlier versions are kept (6.3).
4. **Generated content is recognisable.** An assumed end profile or an inferred exit is marked as such and left out of the fit check until the owner has confirmed it (6.5).

### 4.2 The shared model

The model is shared, not copied: the layout part of the Schedules model is cleaned up and becomes a package of its own, which the planner, the Registry, the Drawing app and Dispatch all use (decided 2026-11-18: the Schedules model is fixed where it stands in the way of sharing). It is called the shared contract in this document, for example `Tellurian.Trains.Schedules.Model.Layouts`, owned by the Schedules repository.

**What it holds**

It has two levels, as the Registry's database (decision 26).

What survives meetings and hall plans:

- **Module**: name, prefix, number, configuration label, package label, the owners as one text, and how it is named (decision 29). It has its standard, its operation location, when it is one, and its tracks.
- **Module standard** and **scale**: a short name and the Registry's id; a standard has its scale (decision 33).
- **Operation location**, with the four kinds and only what is true of the location wherever it is set up.
- **Station track**, as in 4.3, with an id.

What is decided for one layout:

- **Layout module**: a module in the layout, with an id. It contains the module as it was when placed.
- **Layout operation location**: manned, controlled by, instructions (decision 27). It has no copy of the location and no tracks.
- **Layout station track**: what the layout decides differently for a track of the module (decision 28).
- **Track stretch**: the two locations, the distance, the number of tracks, which side of each location it leaves from, and the plain-line modules on it in order.
- **Layout**: modules and stretches together (4.6), and the preferred standard. Its locations are the modules that have a layout operation location.
- **External reference** on layout module and layout operation location: the source and its id, so that an import can be refreshed. For a registered location it is the Registry's station id, which is global (3.5), taken from the file's external references (4.3); without the Registry it is the module GUID. A track is referred to by its id (decision 30).
- `Scale` and `Country`, whose ids already match the Registry, and `Region`.
- Lengths in one stated unit.

The same types are used in four places:

1. a module file: the first level only,
2. the layout block of a hall plan file: both levels,
3. the answers of the Registry's API,
4. the planner, as the base of its own model.

**Why the Schedules model can't be shared as it is**

Its vocabulary is right. Its construction is made for one app:

| # | Today | Why it stands in the way |
|---|---|---|
| 1 | One assembly for layouts, timetables, notes, duties, schedules, settings and validation, each visible to the others | A user of the locations gets everything. The layout types depend on the rest: a track holds its `Calls`, a station has `StartLabels` from the timetable part, a location has `Timings` from the settings |
| 2 | Dependencies on `Microsoft.AspNetCore.Components` (used by `Region`), `Tellurian.Localization` and localised texts in constructors | A data model that needs a user-interface framework can't be a contract |
| 3 | Facts and the planner's decisions are properties of the same class: `Instructions`, `ControlledBy`, `CargoServedFrom`, `LockKey`, `Timings`, `StaffPosts`, the hide flags | The other systems can't fill half of a location, and can't tell which half |
| 4 | The rules of a kind are setters that ignore what they are given (`OtherLocation.HasCargoExchange`) | A value read from a file disappears silently; nobody can say that a file breaks a rule |
| 5 | The kind is the class | Changing kind means replacing the object and repointing everything to it (`ChangeOperationLocationType`). A kind that is a property can be changed and checked |
| 6 | A live object graph: back references to the layout, the track to its location and calls, `ControlledBy` as an object, regions resolved after reading | It needs special handling to be written and read, and can't be sent as it is |
| 7 | Equality by signature for a location, by number and location for a track; ids local to a plan | Two locations with the same signature in different countries are one (6.10); nothing refers outside the plan |
| 8 | Leftovers: `IsSignal` (marked for removal), `PhoneNumber` as a number, a platform length of 1 as "has a platform", timings where 10 or more means seconds | Small, but each is a trap for the next reader |

**What the cleaning consists of**

1. **A layout core without dependencies.** Location, track, stretch, region, scale and country in an assembly that refers to nothing but the base library.
2. **What survives apart from what is decided for a layout.** The decisions about a location move to the layout level of the shared model (decisions 26 and 27), beside the module and not on the location. Calls refer to tracks; tracks don't hold calls.
3. **Kind as a property, rules as validation.** The rules of 6.9 become one set of checks that every system runs, with a message when a rule is broken.
4. **Identity.** A location is identified by an id and its external reference; signature and country are data that must be unique (6.10), not the identity.
5. **The leftovers** removed or corrected.

**Plans saved by earlier versions**

The planner is released and has users with saved plans, and the format of a plan changes with the cleaning. Reading old plans adds work, but it can be kept completely apart from the model, and should be.

Today it isn't apart. A plan has no version number, and what is needed to read older plans is spread through the model: legacy property names and old flags in `PlanJson`, converters that accept an earlier shape (`CountryByIdConverter`), reconciling steps in `Plan` and `Layout`, and repairs such as `EnsurePlatforms`. None of that may come into the shared core.

The way to keep it apart:

1. **A version number in the plan file.** A plan without one is version 0.
2. **Migrations work on the JSON, before the model reads it.** Each is a small function from one version to the next, changing the JSON tree. They run in a chain: a version 0 plan goes through all of them.
3. **The model reads only the current format.** It has no legacy names, no tolerant converters and no repairs.
4. **A place of their own**: one folder or project, one file per step, each with a test that reads a saved plan of that version and compares the result. The Drawing app does this for its files (`DrawingFileMigrations`).
5. **The compatibility code that is in the model today moves there**, step by step, as each part of the model is cleaned.

Notes:

- A migration is written once and never changed. Old steps cost nothing to keep.
- A plan saved by a newer version than the app is refused with a clear message.
- Plans are also kept in the browser's storage. They go through the same chain when loaded and are saved in the current format.
- Reference data that changes meaning (an id that moves, a catalogue that is merged) is a migration too, not a rule in the model.
- A migration can't ask the user. Where the old format lacks what the new one needs, the step chooses a stated default and the plan's validation reports it.

**What it costs, and the order**

- Every step of the cleaning that changes the plan format comes with its migration and a test plan of the version before.
- It can be done without stopping the other work: the core is defined first, as the target, in its own assembly. The Registry and the Drawing app use it from the start. The planner moves its own classes onto it step by step: first the types moved with no change of behaviour, then the decisions separated, then the rules.
- Dispatch is on model 2.1.0 and moves when it is taken up (decision 16).

What this avoids is a contract beside the model with mapping code between them, which would have to follow every change in either, for as long as the systems live.

The core must be settled before the Drawing app adds it to its file schemas, since those are public and kept stable.

### 4.3 The operation location in the module file

A module file gets an operation-location block **per variant**. The viewing side is not in the block: it belongs to the module and is the same for all variants.

- **The location**: name, signature, country of operation, kind, centre, and its own facts: trains can cross, direction change possible, turntable, terminus, harbour, cargo exchange, passenger exchange, key required.
- **Its tracks, as a list of their own**: id, designation, display order, scheduled, lockable and optionally the name of the key, main track, stub, length, platform length, direction (left to right, right to left, both), speed limit, usage. The list belongs to the module, so a plain-line module has one too (decision 21).

The track elements of the drawing refer to a track of the list. The Drawing app can then work out length and platform length from the geometry and write them in the list, so no reader has to interpret geometry.

The Registry's ids of the station and the modules are not in the block; they are in the external references, below.

A module without the block is plain line. A signal module or a junction module can be given a location of kind signal controlled, which makes block posts and junctions exist as locations.

**Left and right.** East and west are not geography: a module is turned any way in a hall, and both the Registry and Drawing already define them by a viewer ("stand at the operation place with the module to the north; west is on your left"). Left and right say what is meant. (The compass letters, also north and south, came from some American modules.)

An exit is **Left** or **Right**, and when needed **Up** or **Down**: Up is the side away from the viewer, Down the side towards the viewer. Up and Down are for an exit whose track leaves across the left–right direction, such as a branch at a right angle. They are inferred from the angle of the track at the exit once Left and Right are set, and can be changed.

Left and right need a **viewing side**, the side of the module where the viewer stands. It also decides from which side the end profiles are seen. It is one for the module, the same for all its variants. Decided 2026-11-18, first match wins:

| | The module has | Viewing side |
|---|---|---|
| a | A public side on one side only | The non-public side |
| b | An operator space on one side only | The operation side |
| c | Neither, or both on both sides | The user decides |

- Until the user has decided in case c, the side that has the first exit on the left is used. That is what auto-draw does today: the first exit is labelled `W`, and the operator space of a station is put on the right-hand side seen from it. Existing data keeps its meaning: West becomes Left, East becomes Right.
- For a plain straight or curve, left and right only name the exits and nothing in operations depends on them, so case c seldom needs the user.
- When a public side and an operator space are on the same side, a wins: the viewer stands on the non-public side.
- The viewing side is **stored once derived**. Otherwise adding a public side to a drawing later would swap left and right for every track.

**Track length at a turnout.** A track ends where a train on the other track can still pass freely, the fouling point, not at the points (decided 2026-11-18). The Drawing app works the length out from the geometry and the clearance of the standard.

**Where track direction is assigned** (decided 2026-11-18). A track's direction, left to right, right to left or both, only means something together with how the user sees the module. The Drawing app is therefore the natural place to assign it: the user has the module in front of them, with the viewing side, and sees which way each track runs.

It can be assigned in the Registry too, since the forms stay (4.1), but then the form must show the same orientation as the drawing, or left and right will be entered the wrong way round without anyone noticing:

- The track form shows the drawing of the module, from the viewing side, when there is one. The Registry has the display components for this (6.5).
- Direction is offered in terms of the exits, by their names ("from exit A towards exit B"), not only as left and right.
- For a module with a generated drawing only, the same picture is shown; it is drawn from the viewing side.
- If the viewing side is changed, in either place, every track direction of the location is turned with it, in the same save. The user is told.

**External references** (decided 2026-11-18). What belongs to another system is kept apart from the drawing's own properties, in a section of its own that the Drawing app doesn't change:

```json
"externalReferences": {
  "moduleRegistry": {
    "stationId": 1234,
    "variants": [
      { "variantId": "<the variant's GUID>", "moduleId": 5678 }
    ],
    "version": { "mark": "...", "savedAt": "2026-11-18T10:15:00Z" }
  }
}
```

- **One entry per system.** `moduleRegistry` is the Registry's. Another source of identities gets an entry of its own beside it, and the Drawing app can have one too.
- **Each system owns its entry**: it alone writes it and decides what is in it. To the Drawing app an entry is JSON that it reads into memory and writes back unchanged. The file schema only says that the section is an object.
- **The Registry's entry** holds the station id, the module id of each variant (keyed by the variant's GUID, since ids are per variant, 3.5), and the Registry's mark of the stored version (6.3).
- **The same section in every kind of file.** An end profile and a standard have the Registry's id of that profile or standard there.
- **It replaces the Registry ids spread over the Drawing model today**: `ModuleVariant.RegistryModuleId`, `EndProfile.RegistryId`, `ModuleStandard.RegistryId`, `Owner.RegistryPersonId` and `RegistryGroupId`. They are moved by a migration of the file format. The owners themselves stay in the drawing (below); their person and group ids are not carried over, since the Registry never reads them from a file (6.2).
- **It narrows what the Drawing app must keep.** For the Registry's needs it is enough that this one section survives a save, not every unknown property everywhere.

Where "not touched" needs care:

- **A copy must lose the section.** When the Drawing app makes a new module from a copy of another, it gives it a new GUID and must remove the external references, or the copy points to the original's module and station. This is the one case where the app changes the section. The Registry's check for copied files (6.3) stays, for files copied outside the app.
- **Variants come and go in the Drawing app.** A new variant has no entry until the file is saved in the Registry; a removed variant leaves an entry behind, which is how the Registry sees that it was removed.
- **Embedded modules in a hall plan carry the section with them**, so that the Registry can find each module's registration. When the Drawing app builds the layout block it copies each location's reference into the block, without interpreting it.
- **Still not trusted on its own.** The section is text in a file like everything else (3.5).

**Owners** (decided 2026-11-18; the form changed 2026-11-19, decision 29). The owners of a module are saved with the drawing as **one text, the names separated by commas** (`ModuleOwners`). The prefix is a property of the module, beside its number, with a mark for whether the two are a FREMO name. **Nothing else**: no contact details, no shares and no ids. The one planning a hall needs to know whom to ask about a module, and the hall plan carries the modules with it. Where the points below say "list" or "entry", read the text.

- **For a registered module the Registry writes the owners**, at every save and every download (6.3), from its own data. The Registry never reads ownership from a file (4.1), so a file can show who owns a module but can't change it.
- **Without the Registry** the owners are entered in the Drawing app.
- **The owners are the drawing's own properties**, not external references: they mean the same whatever system wrote them. The Registry's ids of persons and groups are not needed in the file, since the Registry finds the owners from the module id.
- **A hall plan holds copies.** An owner changed after a module was placed is not in the plan until the module is updated there.
- **No contact details in the file.** Files are passed on and embedded in hall plans. A name and a prefix are what members already see of each other. Within the Registry the planner reaches the owner through the module.
- **A group** that owns a module is written with its name, and its short name as prefix.
- **In the Drawing model** an owner has a name, a prefix, a share and the Registry's ids today. The share and the ids go; the Registry keeps the shares.
- The module's visibility in the Registry applies to its file (6.11).

**Centre.** Each location has a centre, stored as a distance along the main path from the left exit. It defaults to the middle, half the length of the main path, and the owner moves it where the station building or the platform is elsewhere. It is per variant.

### 4.4 Variants

- A station is one module, split in sections. A variant is the same module set up with other sections: some left out, or one used instead of another.
- Only the owner knows which variants exist, so the user draws each one. No system works out a variant by itself.
- The recommendation to users: **the main variant is the largest one**. The station's customers belong to it. Other variants are shorter, without customers.
- Each variant has its own measures, its own operation-location block and so its own tracks and centre. A shorter variant states exactly the tracks it has.
- In the Registry each variant is a `Module` row of the same station with its own `ConfigurationLabel`. The main variant is the station's `PrimaryModule`. Each variant has a module file of its own (decision 20). `PackageLabel` doesn't tie variants together: it names the box a module is transported in (decision 25).

### 4.5 The hall plan

A hall plan is the layout. Each line of it is a **list of modules in order, each with a flag for being turned 180°**, and the variant used. Nothing inside a module drawing is referred to from the plan, so a module can be redrawn or replaced without the plan breaking.

With left and right this gives the topology:

- Not turned: the module's left exit joins the previous module, its right exit the next.
- Turned: the other way round.

A module with **three or more exits** is not covered by a flip: the plan must say which exit the line continues from and at which exit a branch starts. The exit is named by side and number (Left 1, Right 2, Up 1), not by a free-text label in the drawing.

**Distance** is from centre to centre: from the centre of the first location to its exit, plus the lengths of the modules between, plus from the exit of the second location to its centre. For a branch exit the distance from the centre is measured along the branch track. An end-of-line module has one exit. A flip doesn't change a centre, only which neighbour each distance belongs to.

### 4.6 The layout block, and the planner's import

A hall plan embeds its modules as drawings, and a module's length is not stated in it. So that no reader needs geometry, the Drawing app writes a derived **layout block** in the hall plan file, in the shared contract:

- every module of the plan as a layout module, also the plain-line ones, each with its module, operation location and tracks,
- what is decided for the layout: per operation location and per track (decisions 27 and 28),
- the track stretches: neighbours, distance, number of tracks (from the end profiles), the side of each location, and the plain-line modules on the stretch in order,
- the plan's GUID and revision.

The stretches are worked out and written for receivers, like the `measures` block of a module file. What is decided for the layout is not worked out: it is entered, in the hall plan or in the planner, and must be kept by both. Only this block needs to be a stable public contract, not the whole hall plan format. The Drawing app works out branches and junctions, which it knows and a reader doesn't.

The route of each track, its previous and next location, was stored per track in 0.1.0 and is not in 0.2.0. With one stretch on each side of a location it follows from the sides of the stretches. At a junction with two stretches on the same side it doesn't, and is then to be stored with the layout station track (question 16).

**The planner loads a hall plan, not individual modules**, and reads only the layout block. The source is a file, or the Registry's API when the layout's hall plan is stored there (6.6); the importer is the same. The Registry adds what it alone knows: the meeting data of each location, and customers.

The planner keeps the ids and the external references, so a hall plan can be loaded again without losing what was decided in the planner: the contained module of each layout module is replaced, and the decisions beside it stay. A correction made in the plan is not sent back to the source; at the next load the planner shows such differences and doesn't silently take either.

Since a decision can be changed both in the hall plan and in the planner, the two can differ at the next load. How that is settled is open (question 14).

### 4.7 What is decided for a layout, and what stays in the plan

Manned, controlled-by, the instructions for a meeting, and the usage and locking of a track in the layout are decided for one layout. They are in the shared model, at the layout level, and never in a module file (decision 27).

Timings, cargo-served-from, the lock key's holder, staff posts, passenger interchange, the name and signature at a meeting, the hide flags and the start labels are decided for a layout too. They are not yet in the shared model. Those that are preconditions of a timetable follow the same pattern; those that depend on the timetable's own types (timings, start labels) are to be looked at one by one (question 15).

## 5. Changes in the Drawing app, the planner and Dispatch

**Drawing app**

- The external references section (4.3), kept unchanged when a file is saved and removed when a module is copied; the Registry ids in today's model moved into it. This comes first, since other systems will write to the files.
- Preferably also: keep every unknown JSON property when a file is saved. Today anything the model doesn't know is dropped, silently, also by older app versions.
- A track list per variant; track elements refer to it; lengths worked out from the geometry.
- The operation-location block, with entry of all its properties, and of all module properties.
- Left and right: `ExitDirection` West/East becomes Left/Right, with Up and Down added. The enum is written by name, so the file format needs a migration. The viewing side and the centre.
- The full task list is in the Drawing repository: `docs/OperationLocationsTasks.md`.
- The hall plan as an ordered list with a flip; the layout block; a check that signatures are unique in a plan.
- The shared model 0.2.0 (decisions 26 to 32): an entry of the hall plan is a layout module; ids are `int`, also on a track, in place of the app's own GUID; the owners are one text and the prefix a property of the module; enum values are written in PascalCase.
- Entry of what is decided for a layout, in the hall plan: manned, controlled by, instructions.
- Opening from and saving to the Registry with the API key. Today the app is files first.
- The schemas published as static files (6.8). The specification says today that they are not deployed with the app.
- Assumed end profiles and inferred exits marked, and left out of the fit check.

**Planner (Schedules)**

- The layout core taken out and cleaned, as in 4.2. This is the largest change in the planner.
- Import of the layout block from a hall plan file and from the Registry.
- Entry of what a layout decides differently for a track, once a hall plan is imported: usage, replacing or added to the track's own, and lockable (decision 28).
- The external references of the layout module and the layout operation location, and the ids, kept so that a hall plan can be loaded again.
- `StationTrack.SpeedLimit`; owner as name and reference; the phone number as text (an `int` loses `+` and leading zeros).
- The `IsSignal` TODO resolved; it overlaps `SignalControlledLocation`.
- The difficulty of a station, and a hint that a key is required, which the Registry knows (6.9).
- Optionally: tracks with direction on a `TrackStretch`, not only a count, which Dispatch needs and the hall plan can supply.
- The API key sent as decided in 6.7.

**Dispatch**

Dispatch has to adapt to the same model, but is dealt with later (decided 2026-11-18). What is known now:

- Upgrade from model 2.1.0.
- Handle `IndustrialArea`; map manned, platform length and the junction property; correct the distance unit.
- It keeps loading from a plan made in the planner.

## 6. The Module Registry

### 6.1 What it stores

- **The module file**, in the document store, with earlier versions and a mark per version. One file per module row; a variant is a module row and has its own file (decision 20).
- **Columns, as a copy** of the file's properties, written in the same transaction as the file. Everything else in the Registry reads the columns, as today.
- **Its own data**: ownership, visibility, customers, meetings and the catalogue (4.1).
- **Hall plans**, with the Registry `Layout` (6.6).

### 6.2 The Registry maps a few properties

The Registry reads no geometry. To it a file is a document with a few properties it reads and writes in the JSON tree; everything else is stored untouched.

| Reads and copies to columns | Writes |
|---|---|
| Kind, schema version, module GUID, revision, and its own entry in the external references | Name, number, prefix, the FREMO mark and the owners as one text, from its own data (decision 29) |
| Variants: GUID, name, default | Its entry in the external references (4.3): station id, module id per variant, its mark |
| The module's properties and the derived measures per variant | What a user changes in the Registry's forms (4.1) |
| Exits: label, side, end profile reference | |
| The operation-location block with its tracks | |

- **No Drawing library is needed** for this. It is needed only for generating drawings (6.5) and for showing them on pages.
- **Measures are taken as stated in the file.** The owner can type any value into the module form today, so this is no worse.
- **Person and group ids in a file are ignored.** Ownership is always written from the Registry.
- **Properties from a newer app version survive**, since the Registry changes the JSON tree and doesn't write the file through a model.
- The mapped properties are the Registry's contract with the Drawing app and should be listed as such in the schema documentation, so that they aren't moved by accident.
- Content from files that is shown on pages (labels, notes, document addresses) is encoded; addresses must be `https`; string lengths are limited.

### 6.3 Saving a file

A file reaches the Registry by upload on a page, by the Drawing app's save, or from the Registry's own forms. The same checks apply.

1. **Validate**: size (the examples are 10 to 20 kB; 500 kB is generous), the fetched schema (6.8), and the mapped properties with their types.
2. **Which module.** The upload is made on a module, and the file names a Registry module per variant, in the external references.
   - A variant without an id: match by configuration label, or create the module row.
   - An id of another module than the one uploaded to, or of one the user may not change: refuse.
   - Registry variants that the file doesn't have: kept. A variant removed from a file can't remove a module row that old meetings refer to; the row is marked unavailable.
   - **Copied files**: a new module started from a copy keeps the GUID and the ids, and would overwrite the wrong module. The Registry stores the file's GUID with the module rows and refuses a GUID that belongs to another module.
   - Permission must hold for every variant row.
3. **Lost changes.** The file must carry the mark of the stored version. If it doesn't, someone has changed the module since the file was fetched, and the differences are shown.
4. **The Registry's own relations.** A signature that another station in the same country already has (6.10). Removing a track that a customer is placed on, changing a signature while the station is registered for a meeting, or a new length for a module placed in a hall plan: checked before the file is accepted. For modules in a layout: warn the organiser, or freeze after the module registration closing date (open).
5. **Store**, in one transaction through the change tracker: the file with a new mark, the earlier version kept, the columns of every variant row, the exits and the station's tracks.

Notes:

- **The mark is the Registry's own**, with the time and a hash of the content, kept in its entry in the external references (4.3). The file's `Revision` is not touched: it is a counter that the Drawing app increments, and hall plans embed modules by revision, so a second writer would make two different files with the same number.
- **The Registry's data is written into the file at every save and again at every download**: name, owners, the Registry ids. A stored file is then complete on its own, and what a user gets is current.
- **Not one step.** An upload that needs the user (differences, a variant to match) is kept as a pending document between the steps, since a static SSR page has no circuit to hold it.
- **Geometry in the forms.** While a drawing is as generated, changing length, radius, angle or width in the form draws it again. After a hand edit they are read-only in the form.
- The old DWG, SKP and PDF documents stay beside the file.

### 6.4 Tracks and variants

- The station's tracks are copied from the operation-location block of the **main variant**.
- A warning, not an error, when another variant is longer than the main one.
- A designation found only in a shorter variant is shown to the user.
- When a shorter variant is brought to a meeting, the planner gets that variant's tracks and no customers. For a variant without a file the Registry can only give all tracks, with a note that the variant is shorter.
- `StationTrack.Designation` is at most 5 characters; the file has no limit. A turnout belongs to two tracks.

### 6.5 Generating files for existing modules

Drawings with metadata are generated from the Registry's data for the modules and stations that are already there.

- **What can be generated.** Simple straight and curved modules, fully. A station gets an outline and through tracks only; its track work must be drawn by hand. Its **tracks as data** are generated completely from `StationTrack`, so the planner is served from the first day also for stations nobody has drawn.
- **Missing end profiles.** A flat default profile, chosen from the module's width and standard; where there is no width, the standard's default. It is marked as assumed and left out of the fit check until confirmed or replaced, since hall planning would otherwise report fits and misfits that aren't real.
- **Missing exits.** 71 % of the modules have no exits in the Registry, and 172 of 281 stations have none. Auto-draw infers them and reports what it assumed; the notes are kept in the file and shown to the owner.
- **Doubtful data.** Some values are implausible (widths of 30 mm, angles of 360°). Auto-draw has rules for these.
- **The catalogue first.** Standards and end profile drawings must be in the Registry before modules are generated.
- **All in one run.** Generating only on request would leave two kinds of modules for years: those where the columns are the truth and those where the file is. It is safe because the columns stay.
- **Stations with more than one module row.** 14 of 281. They are presumably variants, but must be checked in the data first; any that are really separate modules are merged into one module with sections.
- This is where the Registry needs the Drawing library: auto-draw is made to be used without a user interface.

### 6.6 Hall plans and the layout

- A hall plan is stored with the Registry `Layout`. In its simple form (4.5) it could also be edited in the Registry.
- Its modules are the layout's registered modules, each traced to its `LayoutStation`. Every module in the plan must carry its module id, in its external references (4.3).
- The Registry gives the planner the layout block with its own data added.

### 6.7 The API

**Endpoints**, GET, in the shared contract and not in the old `ReferenceHandler.Preserve` format:

- `GET /api/layouts/{layoutId}/operationlocations`: the locations of a layout with their tracks, from the columns, with the meeting data of each (`LayoutStation`: name and signature at the meeting, country, regions, manned, hide meets and passings, start labels, difficulty), the owner's name and a link to the owner's instructions. It needs no files and no schema change, and meets the planner's requirement FR-3.3.1. The layout id is already in `GET /api/meetings`.
- `GET /api/layouts/{layoutId}/layout`: the layout block of the stored hall plan, with the same additions. When no hall plan is stored it has the locations and no stretches.
- The catalogue: standards and end profiles with their drawings.

Writing (a module file, a hall plan, a plan from the planner) needs POST, which the public API doesn't have by convention. A decision when the Drawing app's save to the Registry is built.

**The API key.** Today the key is the user's `ObjectId`, which is also the secret in the password link (`users/confirm?email=...&objectid=...`). Whoever has a user's key and e-mail address can set that user's password. And a password change replaces the `ObjectId`, so the user's clients stop working. Proposal:

1. **A key of its own**: random, at least 256 bits, created by the user on the profile page, shown once, stored only as a hash, replaceable and removable. A recognisable prefix makes leaked keys easy to find. **This is a security correction on its own and comes first.**
2. **Sent as `Authorization: Bearer <key>`** (RFC 6750), which every HTTP client and OpenAPI tool supports and the planner already sends. A failure answers `401` with `WWW-Authenticate: Bearer`.
3. **`X-API-Key` stays** for the existing consumers.
4. **The `apiKey` query parameter is phased out.** A key in an address ends up in logs, also ours, in browser history and in proxies. Until the consumers have moved, it is removed from what is logged.
5. **The key identifies the user**: each endpoint returns only what that user may see, and use is logged per user.
6. **CORS** on `/api` for the known origins of the planner and the Drawing app, allowing the `Authorization` header and no credentials.
7. **An OpenAPI document** with the Bearer scheme, so clients can be generated.

A key in a browser app can be read by the user of that browser, which is fine: it is the user's own key. Sign-in with OpenID Connect would be the next level and isn't worth its cost here.

### 6.8 The catalogue and the schemas

**End profiles and standards** are kept in the Registry and imported into the Drawing app.

- Two users who each draw "B96" get two GUIDs for one designation. The Registry is the single source.
- An end profile in a module file is matched by Registry id, then GUID, then designation and scale. For one the Registry doesn't have: refuse the module, or accept the profile as private to it (open).
- The copies of end profiles embedded in a module file are left as they are. Bringing them up to date is done in the Drawing app.
- Uploading an end profile is for administrators or those responsible for a standard: a change affects the fit of every module that uses it. `ModuleEndProfile` has only scale, designation and a PDF today; it needs the drawing and the GUID.
- Add-ons to standards don't exist in the Registry and pass through untouched.

**Schemas.** The Drawing app publishes its JSON Schemas as static files, and the Registry fetches them and validates against them. Nothing is written to the Registry from outside.

- **Where**: a fixed address, set in the Registry's configuration, with kind and major version in the name as today (`module.v1.json`).
- **When**: when a file arrives with a major version the Registry has no schema for; and for a known version a conditional request (ETag) at most once a day, so additions are picked up.
- **Stored** in the document store with kind, version, hash and time; an older copy is kept when a new one replaces it.
- **Unreachable**: the stored copy is used.
- Only the configured host, over https, with a size limit and a short timeout; never an address from an uploaded file; the validator loads no `$ref` from the network.
- Stored files are not validated again when a schema changes.
- A new optional property needs nothing: the envelope has only the major version, and the schemas allow unknown properties. A new major version stops a save only if it moves a mapped property; such a file is kept as waiting, and processed when the Registry has been released.

### 6.9 Adapting the Registry to the Schedules model

The model of the planner (`Tellurian.Trains.Schedules`, `Model/Layouts`) is the starting point for operation locations (decided 2026-11-18). The shared contract (4.2) is taken from it, and the Registry adapts to it. This section lists every adaptation.

**Principles**

- The Registry takes over the **concepts and rules** of the Schedules model: the four kinds, what each kind can and can't have, and the meaning of each property.
- **New columns get the Schedules names.** Existing columns keep their names for now and are mapped in one place, since every rename is a manual change of table and code that breaks the running app until both are deployed. Renaming can follow with EF migrations (stage 4 of the Registry's `Migration.md`).
- The table stays `Station` although it will hold all four kinds. Schedules does the same: its track is a `StationTrack` with a `StationId` at every kind of location. In texts shown to users it is "operation location".

#### The four kinds

`Station` gets a `Kind` column with the values of `OperationLocationKind`: Station = 0, SignalControlled = 1, IndustrialArea = 2, Other = 3. Every existing row is 0, a station, which is what they are today.

The rules of each kind are built into the Schedules classes. The Registry must apply the same rules in its forms and when a file is saved, or it will hold data the planner silently ignores:

| | Station | Signal controlled | Industrial area | Other (halt) |
|---|---|---|---|---|
| Trains stop | When the call says so | Never | When the call says so | When the call says so |
| Passenger exchange | Chosen; always at a shadow station | Never | Never | Chosen |
| Cargo exchange | Chosen; always at a shadow station | Never | Always | Never |
| Direction change possible | Chosen | Chosen | Never | Never |
| Trains can cross | From its tracks (more than one scheduled) | Chosen (`TrainsCanCross`) | No | No |
| Can be manned | Yes | No | No | No |
| Shadow, turntable, regions, start labels, passenger interchange | Yes | No | No | No |
| Can be controlled from a manned station | When unmanned | Yes | Yes | No |
| Instructions for the meeting | Yes | No | Yes | No |

What follows in the Registry:

- **Customers** (`StationCustomer`) only where cargo is exchanged: a station or an industrial area.
- **`LayoutStation.IsManned`**, difficulty, regions and the start-label choices only for a station.
- **A signal module or junction module** gets a location of kind signal controlled. A block post is a signal controlled location that is neither a junction nor a place where trains can cross; the planner derives that, so the Registry stores only `TrainsCanCross`.
- **Changing the kind** of an existing location must clear what the new kind can't have. The planner does this in `ChangeOperationLocationType`; the Registry needs the same, and must refuse when customers or meeting registrations would be lost.
- **Existing rows are not reclassified automatically.** A station with customers and no platform may be an industrial area, but only the owner knows. The owners are told about the new kinds.

#### Properties of the location

| Schedules | Registry today | Adaptation |
|---|---|---|
| `Id` | `Station.Id` | None. The station id is global and is given to the planner as the external reference; the planner's own id stays local to the plan. `LayoutStation.Id` is only the key of the meeting data (3.5) |
| `Name` | `FullName` (50) | Mapped. The planner replaces `_` by a space; the Registry should not allow `_` in a name |
| `Signature` | `Signature` (5) | Mapped. Uniqueness within the country (6.10) |
| `CountryId` | none on the station | **New**: country of operation (6.10). The ids are the same in both systems |
| Kind (the class) | none | **New**: `Kind` |
| `IsShadow` | `IsShadow` | None |
| `HasTurntable` | `HasTurntable` | None |
| `HasPassengerExchange` | Derived: shadow, or any track with a platform | **New column**, set from the derivation for existing rows. The planner has both the property and the platforms, and they can differ: a location with a platform that no longer takes passengers |
| `HasCargoExchange` | `HasCargoCustomers` | Mapped; same meaning |
| `IsChangingTrainDirectionPossible` | none | **New** |
| `TrainsCanCross` | none | **New**, for signal controlled locations |
| `Owner`, `PhoneNumber` | The person or group that owns the module | Mapped from ownership. The phone number is personal data and is given only if the owner has agreed to it |
| `Regions` | `LayoutStation.Regions`, per meeting | Mapped. The planner's region has a name, a country and a background colour; the Registry's has all three (`BackColor`) |
| `IsManned` | `LayoutStation.IsManned`, per meeting | Mapped |
| `HideMeets`, `HidePassings` | `LayoutStation.HideMeetingTrains`, `HidePassingTrains` | Mapped. The Registry's rule that hiding passings also hides meets doesn't exist in the planner; it should be the same in both |
| `StartLabels` (None, First, All) | `PrintTrainStartLabels`, `PrintOnlyFirstTrainStartLabel` | Mapped: neither is None, both is First, the first alone is All |
| `StaffPosts`, `ShiftCount` | `LayoutStation.Difficulty` | Not the same thing. Staffing is the planner's. Difficulty stays in the Registry and is offered to the contract as an addition |
| `LockKey` (name, held at) | `IsKeyRequired` | The planner decides the key. The Registry's flag is passed as a hint that a key is needed; an addition to the contract |
| `Instructions` | none | The planner's, for one meeting. The owner's instructions (`OperationInstructionsMarkdown`, PDF) are passed separately, as a link |
| `ControlledBy`, `CargoServedFrom`, `Timings`, `IsPassengerInterchange`, `SplitDispatchByDirection` | none | The planner's. Not in the Registry |
| `IsSignal` | none | Not taken over. It is marked for removal in the planner |
| Junction (derived from the stretches) | `IsJunction` (entered) | The derived one wins. The Registry's flag is no longer asked for, and removed when hall plans are stored |

Registry properties that the Schedules model doesn't have:

| Registry | What happens |
|---|---|
| `IsTerminus` | The planner sees it from the stretches. Kept in the Registry for searching; not in the contract |
| `IsHarbour` | Used for cargo flows in the Registry. Kept; not in the contract |
| `TypeOfSwitchgearId` (unused), `BoosterTypes` | Kept as they are; not in the contract |
| `RegionId` on the station | Kept. To check whether its country can serve as country of operation for existing rows |

#### Properties of a track

| Schedules | Registry today | Adaptation |
|---|---|---|
| `Number` | `Designation` (5) | Mapped |
| `DisplayOrder` | `DisplayOrder` | None |
| `IsScheduled` | `DirectionId` is not NotScheduled | Derived, as today. The obsolete `IsScheduled` column is removed |
| `IsMain` | `IsSiding`, inverted | Mapped |
| `IsStub` | `IsThroughTrack`, inverted | Mapped, when the meaning is confirmed (question 3) |
| `Length` (m) | `MaxTrainLength` (m) | Mapped, when confirmed to be the same measure (question 4) |
| `PlatformLength` (m, 0 = none) | `PlatformLength` (m, null = none) | Mapped: null becomes 0 |
| `IsLockable` | none | **New** |
| `KeyName` | none | **New**: the name of the key of a lockable track, optional |
| `Usage` | `UsageNote` (20) | Mapped. The length limit is the Registry's only. It is the usage in the long term; a layout can replace it or add to it (decision 28) |
| `PreviousLocationId`, `NextLocationId`, `AppliesInBothDirections` | `DirectionId`: east to west, west to east, both | Can't be stored in the Registry: they name the neighbours in one layout. The Registry stores left to right, right to left or both (4.3), and the layout block turns that into previous and next location (4.6) |
| none | `SpeedLimit` | Kept. Proposed as an addition to the Schedules model |

#### Stretches

The Schedules `TrackStretch` (two locations, distance, number of tracks, speed, time, electrified) has no counterpart in the Registry and gets none as a table. Stretches come from the hall plan's layout block (4.6). Speed, time and electrification are the planner's.

To be checked: this section and 3.4 were written from the Registry's `Data` classes. Its database project has tables for the same things (`TrackStretch`, `TimetableStretch`, `ModuleOnTimetableTrackStretch`, `LayoutStationTrack`, and instruction columns on `LayoutStation`). Whether they are mapped and used was not looked at; if they are, "none" in the tables of section 3 is wrong for them.

#### Summary of the changes to tables

| Table | New columns | Removed | Changed meaning |
|---|---|---|---|
| `Station` | `Kind`, `CountryId` (of operation), `HasPassengerExchange`, `IsChangingTrainDirectionPossible`, `TrainsCanCross`; and from 4.3 the viewing side and the centre | `IsJunction`, later | Holds all four kinds |
| `StationTrack` | `IsLockable`, `KeyName` | `IsScheduled` (obsolete) | `DirectionId` means left and right |
| `LayoutStation` | none | none | Manned, difficulty, regions and start labels only for kind station |

Each row is one small step of table, code and deployment, in this order: `Kind` first, with all rows as stations, since nothing else changes until someone chooses another kind.

#### Additions asked of the Schedules model

So that nothing the Registry knows is lost on the way: a speed limit on a track, the difficulty of a station, a hint that a key is required, and the external reference. They are listed in section 5.

### 6.10 Other changes to the Registry's model

The changes to `Station` and `StationTrack` that come from the Schedules model are in 6.9. The others:

- `ModuleExitDirection` Eastwards/Westwards becomes Right/Left, and `StationTrackDirection` EastWest/WestEast becomes LeftToRight/RightToLeft. The stored numbers stay, so no data changes. The texts need new resource keys, since enum texts are looked up by name.
- `Module`: the file's GUID and its file document, one per module row (decision 20). `PackageLabel` stays what it is, the box the module is transported in, and is copied to and from the file as in decision 25.
- `ModuleEndProfile`: GUID and drawing.
- `Document`: the kind as a column (module, end profile, hall plan, schema), the mark, and earlier versions. `FileExtension` is 5 characters, fixed length, in the model and `NVARCHAR(6)` in the table; to be aligned.
- `Layout`: the hall plan.
- One maximum length for all signatures.

**Signatures are unique within the country of operation.**

- **The country**: the station's country of operation; where it isn't set, the owner's country. The country of operation is a property of the location in the file (4.3) and a column in the Registry.
- **Which owner.** A module can have several owners, in different countries, and can be owned by a group. Proposal: the first owner, as for the FREMO designation; for a group, the group's country. To confirm.
- **Compared without regard to case**, as in the planner.
- **Variants** of a station share its signature and are not duplicates.
- **Checked when a file or a form is saved** (6.3, step 4), in the service. It can't be a unique index in the database, since the country comes from the owner when it isn't set.
- **Existing duplicates.** The data must be searched for them before the rule is enforced. Start with a warning to the owners concerned, and refuse only new duplicates.
- **A change of owner can create a duplicate** without anyone touching the station, when the country comes from the owner. The same check is made when ownership changes; setting the country of operation on the station avoids it.
- **Users without the Registry** have nothing to check against. The rule can only be upheld where stations are registered.
- **External stations** have signatures of their own (6 characters). Whether they share the name space of a country is to be decided.

**A layout can still have the same signature twice**: an international meeting has stations from several countries. The planner compares locations by signature alone today, so either its key becomes country and signature, or the organiser gives one of them another signature for the meeting (`OtherSignature`), which the Registry then requires before the layout is given to the planner. The first is the lasting solution; the second works at once.

### 6.11 Pitfalls

| Pitfall | Handling |
|---|---|
| The truth is in a JSON document, for decades | The file format and its migrations are maintained as long as the Registry lives; mapped properties keep their place or are migrated for every stored file |
| Two editors of one file: the Registry's forms and the Drawing app | The shared contract and the fetched schema keep them together |
| A fault in an editor damages the truth | Earlier versions are kept |
| Names in the file: owners, and those who saved it (`RevisionLog`) | The module's visibility applies to the file; the revision log is removed when the file is given to someone else than the owner |
| A file with very many elements is slow to show | Limits on size and number of elements |
| Storage of files and versions in the database | Small (10 to 20 kB per file), but the number of versions is limited |
| Cost | Schema fetching is a small request at most once a day; everything stays on the free tiers |

## 7. Other pitfalls

- **Three places to edit tracks**: the Registry, the Drawing app and the planner. The file is the truth, but each user interface is built and kept alike. The Drawing app's can be small: a table of tracks beside the drawing.
- **Embedded modules in a hall plan are copies.** A track changed afterwards isn't in the plan until the module is updated there and the plan saved again. The layout block carries the plan's revision so the planner can tell what it has loaded.
- **Old files and old app versions.** A module file without an operation-location block is valid and counts as plain line. An older Drawing app drops what it doesn't know when it saves, also the external references, so the app version that keeps them must be out before the Registry starts writing to files.
- **A layout planned from drawings alone has no cargo customers.** A limit to state, not a fault.

## 8. Order of work

1. **Registry: an API key of its own** (6.7, item 1). Independent of everything else.
2. **Registry: the operation locations of a layout from today's data** (6.7, first endpoint), with Bearer and CORS. Independent of the files.
3. **The shared model**: the layout core defined in its own assembly (4.2). The planner's own move onto it follows in steps and doesn't hold up the others.
4. **Drawing app**: the external references section; the track list and the operation-location block; entry of all module and location data; left and right; viewing side and centre.
5. **Drawing app and planner**: the hall plan as a list with a flip; the layout block; the planner's import. From here users without the Registry are served.
6. **Registry: the catalogue**: standards and end profile drawings; fetched schemas.
7. **Registry: module files**: storing with versions and the mark; columns copied from the file; forms that change the file. Modules with one variant first, then several.
8. **Registry: files generated for all existing modules.**
9. **Drawing app: open from and save to the Registry.**
10. **Registry: hall plans** with the layout, and the layout endpoint.
11. **Dispatch**: adapted to the current model. Later, and not planned here.

Steps 7 and 8 are the large ones in the Registry and should each be divided further before they are started.

## 9. Open questions

1. Are the four conditions in 4.1 accepted, in particular that the Registry's forms stay and change the file?
2. (Answered 2026-11-18: the viewing side is the non-public side, else the operation side, else the user's choice, and is the same for all variants; 4.3.)
3. Is `StationTrack.IsThroughTrack` "open at both ends" (then stub is its inverse) or "main through track"? It decides how tracks are generated.
4. Are `MaxTrainLength` (Registry) and `Length` (Schedules) the same measure, both in model metres?
5. (Answered 2026-11-19: what survives is kept apart from what is decided for a layout, and both are in the shared model, decisions 26 and 27. The change of the plan format is in `ModelHarmonisation.md`.)
6. Should a layout be able to have locations without a participant's module, such as a block post set up by the organiser?
7. Who uses the `apiKey` query parameter today? It decides how fast it can be removed.
8. (Answered 2026-11-19: each module row has its own file, decision 20. A package is the box modules are transported in, not the variants of a module, decision 25.)
9. An end profile in a module file that the Registry doesn't have: refuse, or private to the module?
10. A new file for a module that is in a layout: warn the organiser, or freeze after the module registration closing date?
11. How many earlier versions of a file are kept?
12. One maximum length for signatures: 5 or 6?
13. Signatures: which owner gives the country when there are several, or a group? Do external stations share a country's signatures? Does the planner's key become country and signature (6.10)?
14. A decision for a layout that has been changed both in the hall plan and in the planner: which holds at the next load? A proposal is that the planner keeps the decisions as they were loaded, takes what only the hall plan has changed, keeps what only the planner has changed, and asks about the rest.
15. Which of the remaining decisions for a layout go into the shared model (4.7), and what becomes of those that depend on the timetable's types?
16. The previous and next location of a track at a junction with two stretches on the same side: stored with the layout station track, or worked out in another way?
17. `Module.ConfigurationLabel` is in the shared model because the display name uses it, while decision 20 says that only the Registry knows variants. Does the label stay, as a plain text that the other systems don't interpret?
18. Can a layout also decide the name of the key of a track, or only whether it is lockable? And how does a key name per track relate to the planner's lock key of a location, with a name and the station where it is held?
