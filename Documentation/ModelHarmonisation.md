# Harmonising Model and Model.Layouts: analysis of the effect

Written 2026-11-19. Status: analysis. Nothing is built or changed. The decisions that are still open are in section 9.

`Model.Layouts` (package `Tellurian.Trains.Schedules.Model.Layouts`, version 0.1.0) is the first attempt to take the layout part out of the planner's model, as the shared contract of `OperationLocations.md` (section 4.2, decision 23). The planner's own model, `Model`, is not yet moved onto it. This document says where the two clash, and what the move does to the plan file (JSON) and to the database.

Read for the analysis:

- `Model.Layouts` and `Model.Layouts.Tests`, as they are in the working copy.
- `Model` (version 3.12.0), `PlanJson` and `Model.Databases`.
- `Documentation/OperationLocations.md`.
- The Drawing app's documents (`Tellurian.Trains.Drawing/docs`), its `schemas` and its use of the shared package on branch `feature/schema-v2`.
- The Module Registry's handling of packages (`Module.PackageLabel`, `ModulePackage`).
- Two plan files: *Värnamo Moduldagar.json* (a plan made in the app, saved 2026-10-29) and an XPLN import (Kolding 2022) written with today's `PlanJson`.

## 1. Summary

- **The two models can't be referenced together as they stand.** Both declare `Layout`, `OperationLocation`, `StationTrack` and `TrackStretch` in the namespace `Tellurian.Trains.Schedules.Model.Layouts`.
- **The plan file should be redesigned in the same step.** What it writes as `$ref` today is `ReferenceHandler.Preserve`. That is the cause of most of the repetition in the file, and it stands in the way of the migrations on the JSON tree that `OperationLocations.md` 4.2 prescribes.
- **Recommended for the file**: hierarchy where one thing owns another, a stable id for every other reference, a parent never written on its child, and default values left out.
- **The largest effect in code** is in the user interface: the shared types are immutable records, and the planner edits its locations in place.
- **The contract has gaps** that must be filled before it is published: an id on a track and on a stretch, and a module list in the layout block (also needed for packages).
- **The database is the small part.** It is export only and created without migrations, so it is mapped again after the model.

## 2. How a plan is written today

Measured on *Värnamo Moduldagar.json*: 31 locations, 128 tracks, 275 trains, 1,579 calls, 81 vehicles, 67 schedules, 179 driver duties. The file is 3.2 MB indented and 1.78 MB without white space.

47 % of that is 27 company logos as base64 (838 kB). The shares below are of the rest, about 0.94 MB.

| Finding | Count | Share |
|---|---|---|
| `$id` on every object and every list | 6,594 | about 9 % |
| `$ref` | 6,179 | about 10 % |
| Lists wrapped as `{ "$id", "$values" }` | 2,951 | counted in the two above |
| Foreign keys written beside the `$ref` to the same object | 23 property names | about 11 % |
| Values that are null, false, 0 or empty | | about 33 % |

The XPLN import shows the same pattern (2,123 `$id` and 1,733 `$ref` in 294 kB).

**Every reference is stored twice**, as an id and as a `$ref`:

```json
{ "$id": "224", "Id": 3, "TrackId": 25, "Track": { "$ref": "72" }, "TrainId": 2, "Train": { "$ref": "222" }, "Sequence": 1, "Arrival": "09:53:00", "Departure": "10:03:00", "IsArrival": false, "IsDeparture": true, "TurnLoco": false, "Runaround": 0, "PushWagons": false, "Notes": { "$id": "225", "$values": [] } }
```

**Parents are stored on their children.** `Train` on each of the 1,579 calls, `Timetable` on each train, `Layout` on each location and stretch, `Station` on each track, `Plan` on each duty and vehicle, `Schedule` on each part. The hierarchy already says all of it.

**An object is stored where the writer first meets it**, not where it belongs:

- 4 of the 31 locations are written inside another location's `CargoServedFrom` or `LockKey.HeldAt`, with their 26 tracks. The list of locations holds a `$ref` to them.
- All 67 schedules are written inside a vehicle's `ScheduleAssignments`. `Plan.Schedules` is a list of 67 `$ref`s.
- Train parts are written in three places: 343 under vehicles, 68 under driver duties and 11 under schedules.

Which object ends up where depends on the order of the lists. `PlanJson` already works against this for calls, categories, companies and regions, each with a rule of its own (`WriteCallsOnlyWithTheirTrain`, `WriteCatalogueEntriesOnlyInTheirCatalogue`).

**The `$id` numbers are given again at every save.** Nothing outside the file can refer to them, and two saves of the same plan can't be compared.

**All 422 train parts have `Id` 0.** Their identity exists only as a `$ref`. They need real ids before anything can refer to them by id.

**Names and enums**: property names are PascalCase and enums are numbers (`"StartLabels": 2`). The shared contract writes camelCase names, and enum values by their name in the code (decision 5).

**No version number.** A plan doesn't say which format it has (`OperationLocations.md` 4.2).

## 3. References: recommendation

Hierarchy for ownership, and a stable id for everything else, resolved after reading. The plan already does this for train categories, companies and regions; the shared contract and the Drawing app's files do it throughout.

| | What | In the file |
|---|---|---|
| Owned | location → tracks; train → calls → notes; schedule → parts; vehicle → units; duty → notes | Nested |
| Referred to | call → track; part → from and to call; duty → parts; vehicle → schedules; stretch → locations; dispatch and timetable stretch → track stretches; controlled by; cargo served from; key held at; track route → locations | The id of the other |
| Parent of a child | train of a call, location of a track, plan of a duty | Not written |

**Why not `ReferenceHandler.Preserve`.** A `$ref` must come after its `$id` in the file. Two locations in the same list that refer to each other can then only be written by nesting one inside the other, which is the displacement in section 2. It can't be turned off within Preserve.

**The literal `$ref` form.** `{ "$ref": "track/25" }` can be written by a converter of our own, if the form is wanted. It costs more bytes than `"track": 25` and gives nothing more, so the plain id is recommended.

**Default values are left out.** For this the defaults must first be the same in both models. Today `IsScheduled`, `HasPassengerExchange` and `HasCargoExchange` are true by default in `Model` and false in `Model.Layouts`, so a left-out value would mean two things.

**Logos** are half the file. Whether they stay inline is a question of its own; only one of the 27 is a duplicate, so it isn't repetition.

A sketch of the same plan in the recommended form, to show the principle and not the final names:

```json
{
  "format": "tellurian-plan",
  "schemaVersion": 1,
  "layout": {
    "locations": [
      { "id": 5, "name": "Tellby", "signature": "Tby", "countryCode": "SE", "kind": "Station",
        "tracks": [ { "id": 25, "designation": "1", "isMain": true, "isScheduled": true, "length": 3.2 } ] }
    ],
    "stretches": [ { "id": 2, "start": 5, "end": 9, "distance": 18, "tracksCount": 2 } ],
    "dispatchStretches": [ { "id": 7, "from": 5, "to": 12, "stretches": [ 2, 3 ] } ],
    "locationPlans": [ { "location": 5, "isManned": true, "controlledBy": 12, "cargoServedFrom": 9 } ]
  },
  "timetable": {
    "trains": [
      { "id": 2, "number": 130, "category": 1,
        "calls": [ { "id": 3, "track": 25, "arrival": "09:53", "departure": "10:03", "isDeparture": true } ] }
    ]
  },
  "schedules": [ { "id": 9, "parts": [ { "id": 41, "from": 3, "to": 8 } ] } ],
  "vehicles": [ { "id": 14, "schedules": [ 9 ] } ],
  "driverDuties": [ { "id": 11, "parts": [ 41 ] } ]
}
```

The planner's decisions about a location (`locationPlans` here) are drawn inside the layout, after decision 3; whether they are a list of their own or properties of the location is still open.

## 4. Where Model and Model.Layouts clash

| Subject | Model today | Model.Layouts 0.1.0 | Effect |
|---|---|---|---|
| Type names | `Layout`, `OperationLocation`, `StationTrack`, `TrackStretch` in namespace `…Model.Layouts` | The same names in the same namespace | A project that references both can't name the types. The old ones must be removed or renamed first |
| Kind of location | Four classes; `$type` in JSON | A `Kind` property | About 150 tests of type (`is Station`, `OfType<Station>`, the named subclasses) in Model and Planning.Components. `ChangeOperationLocationType` disappears |
| Rules of a kind | Setters and getters that override: a shadow station always exchanges, an `OtherLocation` never has cargo | Checks in `OperationLocationRules` | The migration must write the values the getters gave, or old plans break the rules when read |
| Mutability | Classes with setters, edited in place with `@bind` | Records with `init` | The largest effect on the user interface: 226 references to `OperationLocation` in Planning.Components |
| Equality | Signature for a location; number and location for a track; start and end for a stretch | Value equality of a record | Route logic, sets and dictionaries that compare locations must compare ids. A record with a list in it compares the list by reference |
| Id of a location | `int`, local to the plan | `string`, on `LayoutOperationLocation` only | One type of id must be chosen |
| Id of a track | `int`; every call refers to it | None. The Drawing app has added a `Guid` of its own | The contract needs an id on a track. Without it a changed designation breaks every call |
| Id of a stretch | `int`; dispatch and timetable stretches refer to it | None | The same gap |
| Name of a track | `Number` | `Designation` | Renamed, with a migration (decision 22) |
| Country | `CountryId` | `CountryCode` | Looked up in the migration; `Country` already has the code |
| Route of a track | `PreviousLocationId`, `NextLocationId`, `AppliesInBothDirections`, chosen by the planner | The same two ids worked out from the hall plan, and `Direction` | Not the same thing: one is a choice, the other a fact. An import needs a rule for which holds |
| Stretch | Speed, time, electrified | The side it leaves each location from | Speed, time and electrification stay with the planner, beside the shared stretch |
| The planner's decisions | On the location: instructions, controlled by, cargo served from, lock key, timings, staffing, hide flags, start labels, regions, passenger interchange, split dispatch | Not there, by design | They need a home of their own, which refers to the location |
| New in the contract | None of them | `IsTerminus`, `IsHarbour`, `IsKeyRequired`, `Centre`, `SpeedLimit`, `TrainsCanCross` on every kind | Additions for the planner; the first three come from the Registry |
| Leftovers | `IsSignal`, `PhoneNumber` as `int`, platform length 1 as "has a platform" | Not there | Removed in the migration |
| Unknown JSON properties | Dropped | Kept (`ExtensionData`) | The plan file should keep them too, if other systems are to add to it |
| JSON names | PascalCase, enums as numbers | camelCase, enum values by their name in the code | The plan file follows the contract (decision 5) |

## 5. The plan file: what the move takes

1. **A version number in the file**, in an envelope like the Drawing app's (`format`, `schemaVersion`, `appVersion`). A plan without one is version 0.
2. **Migration from version 0 to 1 on the JSON tree.** It is the hardest one: it must first resolve Preserve's `$id` and `$ref` into the form of section 3, and only then rename and move. *Värnamo Moduldagar.json* is the test case for it, since it has the displaced locations, schedules and parts of section 2. It is kept as `Model.Tests/TestData/Plan.Varnamo-Moduldagar.json`, and `VarnamoPlanCompatibilityTests` holds what must still be true of it after every change of the format.
3. **Ids for what has none**: train parts, and tracks and stretches where the contract lacks them.
4. **The values that getters gave are written out**: cargo and passenger exchange at shadow stations and at each kind.
5. **The legacy code leaves the model**: `AcceptLegacyNames`, `AcceptLegacyTrainCategoryFlags`, `CountryByIdConverter`, `IdListConverter`, `EnsurePlatforms`, and the reconciling in `Plan.OnDeserialized`. Their work is done once, by the migration.
6. **Browser storage** goes through the same chain when a plan is loaded.
7. **A plan from a newer version** is refused with a clear message.

The size of a plan falls to roughly half, logos not counted, from the figures in section 2: about 19 % for `$id`, `$ref` and list wrappers, about 11 % for doubled foreign keys and most of the 33 % that are default values. The ids that replace the `$ref`s take some of it back.

## 6. Database

- The mapping changes from a discriminator column (`LocationType`) to a `Kind` column, and the planner's decisions become a table or an owned type of their own.
- There are no migrations (`EnsureCreated`) and no real import (`DatabaseImportService` is 15 lines), and the export clears the stretches before it saves. Nothing stored has to be converted.
- With references by id, relations are mapped with explicit foreign keys in place of navigation properties.
- `IndustrialArea` has no discriminator value in `ScheduleDbContext`. Whether the export fails for a plan with industrial areas was not tried; *Värnamo Moduldagar* has ten.
- Recommended: map the database again when the model has settled, not in step with it.
- Done at step 7 (section 10, progress).

## 7. Packages

A package is the box that one or several modules are transported in (`OperationLocations.md`, decision 25). The label is optional. When a module has one, the package with all its modules is submitted to a meeting; otherwise the module itself is.

| System | Today |
|---|---|
| Module Registry | `Module.PackageLabel` (10 characters). `ModulePackage` is not stored: it is worked out when modules are registered for a meeting, as a package (same label), the variants of a module (same name, each with a `ConfigurationLabel`) or a single module |
| Drawing | `Module.PackageLabel`, in the model and in the module file. The Registry's label holds for a registered module |
| Model.Layouts | Nothing. A location has a `ModuleId`; plain-line modules disappear into the distance of a stretch |
| Planner | Nothing |

What it takes:

- **A module list in the layout block** (`Layout.Modules`): id, name, owners, external references and the optional `packageLabel` of every module in the hall plan, also the plain-line ones. A location refers to its module by id, as it already does.
- **The package is derived, not stored**: the modules with the same owner and the same label. The owner is part of it, since two participants can both have a "Box 1". A package has no properties but its label, so it needs no entry of its own.
- **The planner's own model needs nothing.** Packages concern what is submitted to a meeting and what is in the hall, not the timetable. If the planner is to show them, it reads them from the layout block.
- It is an addition to the contract and doesn't disturb the Drawing app's schema version 2.

## 8. Other effects

- **Packages on NuGet**: `Model` gets a dependency on `Model.Layouts` and a new major version. Dispatch, on 2.1.0, isn't affected until it is upgraded.
- **Drawing app**: its branch `feature/schema-v2` references `Model.Layouts` 0.1.0 from the local feed, and its `StationTrack` extends the shared one with a `Guid` id. An id on the shared track and stretch is an addition, but should be settled together with the Drawing app's own, before 0.1.0 is published and schema version 2 is released.
- **Licence**: the packages of this repository are GPL-3.0-only and the Drawing app's are MIT. It is open in the Drawing app's task list and stands in the way of publishing.
- **CI**: `Model.Layouts.Tests` is not in the test job, and the package is not in the publish job. The Drawing app's CI can't build until the package is on nuget.org.
- **XPLN and Access import**: they build the model in code and are changed with it, without any change of what they import.

## 9. Decisions

| # | Question | Recommendation or decision |
|---|---|---|
| 1 | References in the plan file: plain ids, or the literal `$ref` form? | **Decided 2026-11-19**: plain ids (section 3) |
| 2 | The shared types are immutable records and the planner edits in place. Does the planner's location hold the shared record as a value that is replaced, or do the contract's types get setters? | **Decided 2026-11-19**: the planner's location holds the shared record as a value that is replaced, and everything else refers by id. The contract's types get no setters |
| 3 | Where are the planner's decisions about a location written? | **Decided 2026-11-19**: they are part of the layout model, `Model.Layouts`, since the other apps use them and a database is created from the model. This replaces the analysis's "not there, by design" (section 4) and `OperationLocations.md` 4.7. As in the Module Registry, what survives a layout (`Module`, `OperationLocation`, `StationTrack`) is kept apart from what is decided for one layout (`LayoutModule`, `LayoutOperationLocation`, `LayoutStationTrack`): the layout type contains the surviving one and has the decisions beside it. The decisions are preconditions of a timetable, so they can be entered in Module Draw, in the hall plan, and changed in the planner. Built in 0.2.0 for manned, controlled by, instructions and track usage, with their rules; the rest follow. Still open: what happens when the same decision has been changed in both the hall plan and the plan, and what becomes of those that depend on timetable types (`StartLabels`, `Timings`) |
| 4 | An id on track and stretch in the contract, and its type: `int`, `string` or `Guid`? | **Decided 2026-11-19**: yes, and the type is `int`, so that it is compatible with the other apps. The Drawing app's `Guid` on a track gives way to it. The id of a location and of a module in the layout is `int` too, as every id in the Module Registry. Built in `Model.Layouts` 0.2.0 |
| 5 | Names and enums, in JSON and in a database | **Decided 2026-11-19.** In JSON (files and API), property names are camelCase, as is common practice, and enum values are written by their name in the code, in PascalCase (`"kind": "SignalControlled"`). In a database, tables, views and columns are PascalCase, as in the Module Registry. The whole plan file follows this in the migration to version 1: one break instead of two, and no file with two casings. The Drawing app's files are to be checked for how they write enum values |
| 6 | A module list in the layout block, with `packageLabel` and owners (section 7)? | **Decided 2026-11-19**: yes. `Model.Layouts` follows the structure of the Module Registry's database, in two levels: what a thing is (`Module`, `OperationLocation`, `StationTrack`) and what it is in one layout (`LayoutModule`, `LayoutOperationLocation`, `LayoutStationTrack`, `TrackStretch` with its modules in order). The name of a module is worked out in the contract from prefix, number and name, as far as they are given; it is a FREMO name only when an owning person is a FREMO member with a prefix. The owners are one text on the module, `ModuleOwners`, the names separated by commas, and not a structure; the Module Registry sets or overwrites it, with the prefix and the FREMO mark, when a drawing is uploaded. This replaces the owners list of `OperationLocations.md`, decision 18. Built in 0.2.0; more properties are to come |
| 7 | Should the planner do anything with packages beyond showing them? | No |
| 8 | Do the company logos stay inline in the plan? | Open; they are half the file |
| 9 | Is the hall-plan import built before or after the planner moves onto the shared model? | **Decided 2026-11-19**: after. The planner's layout becomes the shared `Layout`, so the import reads the layout block and merges it by id, with no mapping layer to throw away. Entering a layout from scratch stays: the planner then makes the layout modules itself, without a source |
| 10 | The distance of a stretch that comes from a hall plan: fixed, or can the planner change it? | **Decided 2026-11-19**: the hall plan's distance (centre to centre) is the default, and the planner can override it per stretch. A reload keeps the override and shows where it differs from the hall plan. A stretch entered by hand is edited as today |
| 11 | The kind of a location: a property of one type, or a hierarchy of types? | **Decided 2026-11-19**: a hierarchy, closed. `OperationLocation` in `Model.Layouts` becomes a `closed` record (C# 15) with `Station`, `SignalControlledLocation`, `IndustrialArea` and `OtherLocation` beneath it, each with only the properties its kind can have. In JSON and in a database the kind is still a value: `"kind": "Station"` is the type discriminator, and EF Core maps the hierarchy to one table with a `Kind` column, so the file and the Registry's table look as with a property. A switch over the kinds is checked to be exhaustive. This replaces the `Kind` property of decision 23 and `OperationLocationRules` as the place where a kind's properties are limited; the rules between properties stay as checks. Dispatch already has the hierarchy. A closed type needs `net11.0`, so the solution moved to .NET 11 the same day; the Drawing app, the Registry and Dispatch follow when they take `Model.Layouts` 0.3 |
| 12 | The shared types: immutable records or mutable classes? | **Decided 2026-11-19**: mutable classes, closed where they are a hierarchy. This replaces decision 2: the planner's own classes moved into `Model.Layouts` and are edited in place, as before. Module Draw, which derived its module from the shared record and used `with`, changes with it when it takes 0.3 |
| 13 | Where are the decisions of a layout and the planner's own data? | **Decided 2026-11-19**: on the location itself, one level, as properties of the shared classes: manned, controlled by, instructions, and the track's usage and lockable, and also the planner's own data (timings, start labels, staff posts, lock key, regions, cargo served from, hide flags, split dispatch, passenger interchange), with their small types moved into `Model.Layouts` without dependencies. The other apps ignore what they don't use, and a module file leaves the decisions out. `LayoutOperationLocation` and `LayoutStationTrack` are gone; `Module` and `LayoutModule` stay, for the module list, the packages and plain line. This replaces decision 26 for locations: reloading a hall plan copies what a module is onto the existing location and keeps what is decided for it |
| 14 | Where do the model, the formats and the serialisation of the suite belong? | **Decided 2026-11-20**: in Tellurian.Trains.Schedules, in three layers: Layouts (`Model.Layouts`, what Module Draw makes), Timetables (`Model`, what the planner adds) and Dispatching (what Dispatch adds; not here yet). Module Draw, the Timetable Planner and Tellurian Dispatch use these models and add only app-specific functionality and user interface; the Module Registry keeps its own model and is extended afterwards from the gaps. JSON is read and written with source-generated serialisation, required by attributes, for AOT. Reading and writing in every format is done here: the importers, which are extendable, and exporters to be added with interfaces of their own and implementations for JSON, databases and possibly others. All serialisation and deserialisation code is here, so that files, databases and API calls use the same code. Background and consequences: `CrossAppReview.md`, section 0 |

### The structure as built in `Model.Layouts` 0.2.0

Where the rows above name types differently, this holds. Sections 3 and 4 describe 0.1.0 and the sketch of the plan file is older than this structure.

- **`Module`** is what survives a layout: name, prefix, number, labels, the owners as one text, its optional `OperationLocation` and its `StationTrack`s. It is the same in a module file and in a layout.
- **`LayoutModule`** is a module in one layout: an id, the `Module` as it was when placed, and a `LayoutOperationLocation` when the module is an operation location.
- **`LayoutOperationLocation`** holds only what is decided for the layout: an id, manned, controlled by, instructions. It has no copy of the location and no tracks.
- **Tracks exist in one place**, on the module, each with an `int` id and what holds in the long term (the Module Registry enters usage and lockable).
- **`LayoutStationTrack`** holds only what a layout decides differently for a track, by the id of the track: a usage that replaces the track's own or is added to it, and whether it is lockable. A track with nothing decided has no entry. The planner enters these once the hall plan is imported.
- **`ModuleStandard` and `Scale`** are referred to by short name, with the Module Registry's id beside it. A module has its standard (`Standard`) and a layout its preferred standard (`PreferredStandard`); the scale is that of the standard. The planner's own `Scale` type has the same name and gives way to this one when `Model` moves onto the package.
- **`Layout`** has modules and stretches. The locations of a layout are its modules that have a `LayoutOperationLocation`.
- **What is worked out** (designation, display name, has a platform) is an extension member in the file of the type it extends, never a stored property.
- **Names** follow the Module Registry's code, and no type has the kind of type in its name; the common base is `LayoutPart`.

Still to be decided: the previous and next location of a track, which 0.1.0 stored per track in the layout. `LayoutStationTrack` is the place for it if it has to be stored.

## 10. Suggested order

1. Settle decisions 1 to 6, and with them the contract: ids, defaults, the module list. Publish `Model.Layouts`.
2. The envelope with a version number and the chain of migrations, at first with no step in it: a plan without a version is read as today. *Värnamo Moduldagar.json* is kept as the test plan of version 0.
3. References by id in the model and the file, with the migration from version 0 that resolves `$id` and `$ref`. The relations can be moved one at a time in the model, first those of section 2 that are displaced (schedules, parts, controlled by, cargo served from, key held at), but the file changes form once, when all of them are done.
4. The location: kind as a property, the rules as checks, the planner's decisions apart, the old types removed. This is where the two models stop clashing.
5. Track and stretch onto the shared types.
6. The hall-plan import (decisions 9 and 10).
7. The database mapping.

### Progress

- **Step 2 built 2026-11-19.** `Model/PlanFiles`: `PlanFile` writes the envelope (`format`, `schemaVersion`, `appVersion`, `plan`) and reads a plan of any version up to the current one through the chain in `PlanFileMigrations`, which has no step yet; the current schema version is 0, the content being written as before. A plan without an envelope is read as version 0, and a newer version is refused (`PlanFileException`). Browser storage, file open and save, and the JSON import and export services all go through it. Tests in `Model.Tests/PlanFileTests`.
- **Step 3 built 2026-11-19: schema version 1.** The plan file is a tree with references by id, as in section 3; `ReferenceHandler.Preserve` is gone. Property names are camelCase and enum values are written by name (decision 5). The model keeps its object references for now: the shape of the file is given by `PlanReferences` (a modifier of `PlanJson`), which writes the id where the model keeps one beside the object (`trackId`, `fromId`, `startId`, `categoryId` …) and adds a property that exists only in the file where it doesn't (`cargoServedFromId`, `controlledById`, `heldAtId`, `stretchIds`, `locationId`, `fromTrackId`, `partIds`). After reading, the references are resolved and the parents set; before writing, the ids are brought in line with the objects.
  - **Train parts** get an id of their own; a duplicate id of a track, a call or a part is given a new number before writing. A vehicle schedule owns its parts. A driver duty refers to its parts by id, in order (`partIds`), and owns only those that belong to no schedule (`parts`), the first duty that has one writing it: 68 of the 279 parts in *Värnamo Moduldagar*.
  - **The migration from version 0** (`PlanFiles/SchemaVersion1`) works on the JSON tree from a frozen description of version 0, with the enum names copied: it resolves `$id`, `$ref` and `$values`, puts every displaced object back with its owner, numbers the train parts, completes the catalogues of companies, categories and regions with what is referred to and gives entries that share an id new ones, and takes over the legacy names and flags. The legacy code has left the model: `AcceptLegacyNames`, `AcceptLegacyTrainCategoryFlags`, `WriteCallsOnlyWithTheirTrain`, `WriteCatalogueEntriesOnlyInTheirCatalogue`, `IdListConverter`, and the reading of a whole country by `CountryByIdConverter`. Still in the model: `EnsurePlatforms` and the reconciling in `Plan.OnDeserialized`, which an import needs too.
  - **Verified** against the plan read the old way (`Model.Tests/PlanVersion0Reader`, kept until step 4): the fingerprint of *Värnamo Moduldagar*, every reference included, is the same migrated, written as version 1 and read again (`SchemaVersion1MigrationTests`). Without the logos, its content falls from 942 kB to 573 kB (61 %), before default values are left out.
  - **Not yet:** default values left out (section 3; the defaults of the two models must agree first). Done in schema version 4, below.
- **Step 4 built 2026-11-19, with decisions 11 to 13: `Model.Layouts` 0.3 and schema version 2.** The planner's layout classes are the shared model now: `Model` references `Model.Layouts`, which holds `Layout`, the closed `OperationLocation` with `Station`, `SignalControlledLocation`, `IndustrialArea` and `OtherLocation`, `StationTrack`, `TrackStretch`, `TimetableStretch`, `DispatchStretch`, `TopologyPosition`, `Company`, `Region`, `LockKey`, `StaffPost`, `Scale`, `Country`, the layout settings and `StartLabelPrinting`, in their old namespaces, beside `Module`, `LayoutModule` and `ModuleStandard`. The two models no longer clash. What depends on the planner stayed in `Model` as extension members, so most call sites are unchanged: the calls on a track (`track.Calls`, an index kept by the timetable), the passings and the localised description of a stretch, and the planner's names `track.Number` and `location.CountryId` over the contract's `Designation` and `CountryCode`.
  - **The kind is the type**, and in JSON the discriminator `"kind"`; `Kind` is worked out from the type, with a switch the compiler checks to be complete. The rules of a kind are the type and its overriding getters; `OperationLocationRules` checks what is left: names, country code, centre, tracks, and who controls a location and its instructions.
  - **Parents are never written** in the contract's own JSON either: a location's layout, a track's location and a stretch's layout are `[JsonIgnore]`, and a stretch is written with `startId` and `endId`.
  - **The plan file is schema version 2** (`PlanFiles/SchemaVersion2`): `$type` becomes `kind`, written first, a track's `number` becomes `designation`, and a location's `countryId` becomes `countryCode`, with the codes copied from the catalogue as it was. `PlanVersion0Reader` reads version 0 under its old names, so the migrations are still verified against it: *Värnamo Moduldagar* through versions 1 and 2 has the same fingerprint as read the old way.
  - **Database**: mapped to `Designation` and `CountryCode`, the call to its track from the call's side, `IndustrialArea` given its discriminator value, and what is new in the contract ignored until the database is mapped again (step 7).
  - **Not yet:** the leftovers of section 4 (`IsSignal`, `PhoneNumber` as a number; the platform length of 1 went in schema version 3), the rules as overriding getters (still in the subclasses), and a module's tracks being the same list as its location's (step 6, when a hall plan is loaded).
- **Module Draw on `Model.Layouts` 0.3.5, 2026-11-19.** Module Draw was on .NET 11 already. It uses the shared classes directly, as the user prefers, and what it needed was added to the contract rather than as glue code in the app: `Copy()` and `With(change)` for its immutable editor and undo (its `Module` derives from the shared one and is a class now; every `with` became `With`), `OperationLocation.Create(kind)`, `Conforming` and `ClearedBy` for the closed hierarchy, public parameterless constructors, `IList` track lists, `AlwaysHasCargoExchange`, `TrainsCanCrossAt(location, tracks)` for tracks kept on the module, `ExtensionData` on the small nested types, and `OperationLocationJson.Normalise` for earlier files. The contract's own JSON writes no reference to another location (`cargoServedFrom`, `controlledBy`, a lock key's `heldAt`) nor a station's region objects: the plan file writes them as ids, and a module file has none. Module Draw's schemas were regenerated (its `schemas/CHANGELOG.md`); its 1115 tests and the planner's pass. Open: a module's tracks are on the module in a module file and on the location in a plan, which the hall-plan import must join.
- **Step 6 started 2026-11-19: comparing a layout with a newer hall plan**, in `Model.Layouts` (`LayoutComparison`, `LayoutUpdate`). Timetable planning should start when the hall plan is settled, but a hall plan can still change afterwards; as long as the order of the locations holds, that changes only distances and times. The comparison matches locations by external reference, then by id within the same hall plan, then by signature, and sorts each difference by what it does to the timetable:
  - **Automatic**: a stretch's distance and number of tracks, what a location and a track are, a track added. Applied at once: a stretch takes the hall plan's distance unless the planner entered its own (`IsDistanceEntered`), and then a running time from distance and speed unless the planner entered the time (`IsTimeEntered`, set by the stretch editor when a value is changed). The hall plan's identity, revision and modules are taken; a location module has its location's tracks as one list.
  - **Confirm**: a track removed, a kind changed, a location removed from between two neighbours (the planner's remove-and-merge), a location inserted on a stretch (split, with pass-through calls for the trains running it). Applied by the planner, since they touch calls and trains.
  - **Manual**: another order, a location dropped or added off the stretches, a stretch added or removed. Reported; the order doesn't hold.
  - **The planner side of the Confirm changes, built 2026-11-20** (`Model/Layouts/LayoutChangeRules`): `PreviewChange` says what a change would do without doing it, `ApplyChange` does exactly that, as `DeletionRules` does. A track removed: its calls are moved with `StationCall.MoveTo` to the track the location would give each train now (blocked where the location has no other track), and a vehicle fetched from it or put on it is fetched or put on the arrival or departure track. A kind changed: `ChangeOperationLocationType`, then what the location is; the preview names the trains that stop there and won't any more. A location removed: the planner's own delete (`DeletionRules.TryDelete`), and the stretch that joins its neighbours takes the newer distance. A location inserted: added as the newer layout has it, with ids of its own and nothing decided for it; the stretch is split with the newer distances (an entered time is shared by distance, else each half gets its running time); timetable and dispatch stretches take both halves in their order; each train running the stretch gets a pass-through call there (`Train.AddAfter`), at the time its share of the distance gives.
  - **Reading the layout block, built 2026-11-20** (`Model.Layouts/HallPlanFile`, `LayoutSerialization`): `HallPlanFile.ReadLayout(json)` reads `content.layout` of a Module Draw hall plan file, or a layout block on its own, and refuses another kind of file or one without the block. How a layout is written is defined once, on `Layout` through an extension block: `Layout.JsonSerializerOptions`, the modifier `Layout.ModifyJsonTypeInfo` (which the plan file's options use too, so `PlanReferences` keeps only what belongs to plans) and `layout.RestoreReferences()`.
  - **The planner side of the import, built 2026-11-20** (`Model/Layouts/HallPlanImport`): `PlanFactory.CreatePlan(hallPlan, language)` makes a new plan with the usual defaults, named after the hall plan, taking its locations, tracks, stretches (each given a running time from distance and speed), modules, identity and scale; the default country is the one most locations are in, else the language's, and every country a location is in joins the catalogue (`EnsureCountries` now counts locations). `plan.UpdateFromHallPlan(hallPlan)` compares, applies the Automatic changes and returns a `HallPlanUpdate` with what was applied, what is `ToConfirm` (for `PreviewChange`/`ApplyChange`) and what is `Manual`.
  - **Schema version 3: platforms given once, by the migration.** The planner used to give every track of a passenger location without platforms a platform of 1 m on every read and every reconcile, which would have falsified a hall plan's platforms and made every track of it look changed. `PlanFiles/SchemaVersion3` does it once for the older files; reading and `Plan.Reconcile` no longer do. `EnsurePlatforms` stays for what has no platforms: the app's XPLN import calls it (`ScheduleImportService`), and so does the location form. `PlanVersion0Reader` gives platforms after reading, as reading did then, so the fingerprint of *Värnamo Moduldagar* through versions 1–3 still matches.
  - **Layout importers, built 2026-11-20**: `ILayoutImporter` (`Importers.Interfaces`) reads a `Layout` made elsewhere: the resource key of its name (a label of the app, added with the importer), its file extensions, a quick `CanImport(fileName, content)` and `ImportAsync`. Importers are registered with `services.AddLayoutImporter<T>()` (compiled in, as WebAssembly with trimming can't load assemblies at run time); the first one whose extension fits and which recognises the file reads it. The Module Draw importer (`Importers.Services/HallPlanLayoutImporter`, recognising by `HallPlanFile.IsHallPlan`) is the first; a layout block on its own is the open format for other tools. On the Import tab a layout becomes a new plan, or updates the open plan when that was made from the same source (`plan.IsMadeFrom(layout)`, by `SourceId`), with a message of what was applied and what is left to confirm.
  - **The list of changes to confirm, built 2026-11-20** (`Planning.Components/Imports/LayoutChangesList`, `Services/LayoutRevisionState`): on the Import tab, after a newer revision is loaded. Each change to confirm has a **Review** dialog with what it does to the trains (calls moved to another track, trains no longer stopping, trains given a pass-through call; a removed location shows the planner's own deletion confirmation) and **Apply**. The changes that break the order are listed with the advice to make them by hand. After each change applied, the plan is compared with the newer layout again (`plan.UpdateFromHallPlan`), which works because the rules only copy from the newer layout; the list is kept for the open plan while moving between tabs, not saved: opening the same file again lists what is left. Help for the Import tab in all five languages; E2E test `LayoutChangesUiTests`.
  - **Timetable stretches are the planner's (user's decision 2026-11-20):** a plan made from a hall plan has none, and none are made for it. Where they begin and end is a planning choice: they may overlap, and a long line may be split, for instance where it crosses a country border.
  - **Update or new plan, built 2026-11-20**: a file that is a revision of the open plan's source (`plan.IsMadeFrom`) is read first and the planner asked, with both revision numbers and a warning when the file's is older: update the open plan, make a new plan from it, or cancel. Nothing is changed before the choice. `ScheduleImportService` has `ReadLayoutAsync`, then `NewPlan` or `Update`.
- **Correction 2026-11-20, after `CrossAppReview.md`:** the planner's side of the hall-plan import is built, but **Module Draw does not write the layout block yet** (no hall-plan editor, no code using `Layout` or `HallPlanFile`), so a Module Draw hall plan is refused until it does. The import works today only for a layout block written in the contract's form, as the tests do. Module Draw also builds against an older build of `Model.Layouts` 0.3.5 than the source here.
- **Leftovers of section 4, 2026-11-20: schema version 4** (`PlanFiles/SchemaVersion4`).
  - **`IsSignal` is gone**, from the model and, by the migration, from the plan, so it isn't kept among the unknown properties.
  - **`PhoneNumber` stays a number** (user's decision 2026-11-20): it is the short extension of the meeting's phones, shown on the dispatch lists and the location sheet.
  - **Default values are left out.** The two models are one now, so their defaults agree. A value is left out where it is what a new object of its type has, taken from an object created as reading creates it, not `default(T)`: a track's `isScheduled` is left out when true and written when false. A type read through a constructor with parameters is left as it is. Changing the initial value of a property now changes what a plan without it means, and so takes a migration. Without the logos, *Värnamo Moduldagar* falls from 573 kB to 310 kB, a third of the 942 kB of version 0; its fingerprint is the same written and read again (`SchemaVersion4MigrationTests`).
  - **Still open:** the rules of a kind as no-op overriding getters in the subclasses, where decision 11 says each kind should have only the properties it can have.
- **Step 7 built 2026-11-20: the database mapping.** A plan goes to the database and back unchanged: *Värnamo Moduldagar* exported to SQLite and read again has the same fingerprint, and is written to a plan file character for character as before (`Model.Databases.Tests/PlanRoundTripTests`); so is a plan made from a hall plan, with its modules, standard, lock key, staffing, external references and unknown properties.
  - **One plan per database** (user's decision 2026-11-20). A plan's ids are local to it and are the keys, so the export creates the database anew (`EnsureDeleted`, `EnsureCreated`) rather than removing a plan of the same name. Before saving it calls the new `plan.PrepareForWriting()`, which writing a plan file calls too (it was the body of `Plan.OnSerializing`): catalogues rebuilt, ids given and references by id brought in line. The plan, timetable and layout keep their ids as given, often 0. The export no longer empties the caller's track and timetable stretches.
  - **Train parts are one kind of id.** The scheduled parts and the cargo flows share a table, so `PlanReferences` now gives a train part an id no other part of either kind has; a cargo flow is the one renumbered, since the scheduled parts are referred to by id. The fingerprint knows a cargo flow by its calls, as it does a scheduled part.
  - **The database holds any plan a file can.** The unique indexes on signatures, track designations and stretch ends are plain indexes now: *Värnamo Moduldagar* has two tracks "12" at Nlu. Instead, a repeated track designation is a validation conflict (user's request 2026-11-20): `TrackDesignationNotUnique`, layout scope, always run like the lock-key rule, using the shared model's `OperationLocationRules.Validate(tracks)`; message in all five languages.
  - **Mapping.** The discriminator column is `Kind`, with the values of `OperationLocationKind` (as the plan file's `kind`). Every list keeps its order through a shadow `ListPosition` column, numbered in `SaveChanges` and restored by the reader (`ListPositions`); the four lists that refer to what they hold (a station's regions, a timetable or dispatch stretch's stretches, a duty's parts) have named join tables with the position. What was left out at step 4 is mapped: a station's staffing as a JSON column; the hall plan's modules and standard and a location's lock key as JSON in the contract's own form (`Layout.JsonSerializerOptions`), resolved after reading by `layout.RestoreReferences()` — EF won't let a type owned by a location refer to another location, so the lock key can't be an owned type; unknown properties and external references as JSON text; a stretch's module ids as a primitive collection. A train's length is three columns (it lost the wagon count and the decimals of the metres as a piped string).
  - **Reading**: `DatabaseImportService` is an `IImportService` now, and `context.ReadPlanAsync(name)` loads each part of the plan with a query of its own into one context, puts the lists in order, stops tracking, restores the layout's references and reconciles the plan as an importer's.
  - **Finished the same day:** the unknown properties inside JSON-mapped types (the station timings in the settings, a staff post) are kept too, and `ScheduleDbContext` has one mapping per entity (`Model.Databases/Mappings/<Entity>Mapping.cs`, an `extension(ModelBuilder)` block each, called in alphabetical order); the EF model before and after the split was compared and is identical. Release notes: NuGet 4.0.0 and app 1.5.0, versions not yet bumped.
- **Completing the model, step 1 built 2026-11-20: source-generated JSON** (`LayoutsLayerSurvey.md` section 7, step 1; `CrossAppReview.md` section 0). Reading and writing a layout and a plan uses no reflection: `LayoutJsonContext` (in `Model.Layouts`) and `PlanJsonContext` (in `Model`), combined by `PlanJson.CreateOptions()` with the layout's first; `Layout.JsonSerializerOptions` and `PlanFile` use them. Both projects build with `IsAotCompatible` and no trimming or AOT warnings.
  - **The shape is said by attributes.** A parent, and an object written by its id, is `[JsonIgnore]`; beside it an internal `[JsonInclude]` id property gives the object's id, or holds the id read until the reference is restored (`ControlledById`, `CargoServedFromId`, `HeldAtId`, `FromId`/`ToId`/`StretchIds`, `Module.OperationLocationId`, `Origin`/`Destination.LocationId`, `FromTrackId`/`ToTrackId`, `DriverDuty.PartIds` and `parts`). Setting the object clears the pending id, so a stale id is never written. `LayoutSerialization` and `PlanReferences` keep only the restoring; the run-time property modifiers, the side table and `Layout.ModifyJsonTypeInfo` are gone.
  - **Default values by attributes** (decision 6): a value is left out when it is the default of its type; 60 properties whose initial value is something else, and `Train`'s required members, carry `[JsonIgnore(Condition = Never)]`. `JsonDefaultValueTests` sets each such property to its type's default, writes and reads it, and fails by name if one is lost.
  - **A module is written by where it is**: placed in a layout (`LayoutModule.Module`), it refers to its location by `operationLocationId`; on its own, as in a module file, its location is nested with the tracks, which are written where they are owned (`LayoutsLayerSurvey.md` 6.2).
  - **A country is written as its id everywhere** (`CountryByIdConverter` on `Country`), in a layout block as in a plan.
  - **Verified**: *Värnamo Moduldagar* written before and after, compared value by value: nothing changed or lost; the extra keys are only the always-written defaults of decision 6 (empty lists, empty texts, flags that start true), and the keys left out are values equal to their type's default (an empty `length`, an arrival of 00:00). The indented file grows by about 7 %. All migration and fingerprint tests pass; the database keeps its columns, its foreign keys now being the id properties read through their getters. The E2E test `Each_location_gets_a_sheet…` fails on the committed code as well (the report never finishes); not caused by this step.
  - **Not yet:** the contract's version is still 0.3.5 although public members changed (`Layout.ModifyJsonTypeInfo` removed); it is bumped with step 3. Next: step 2, one envelope and migration mechanism for every file of the suite.
