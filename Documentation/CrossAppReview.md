# Cross-app review: data exchange between the planner, the Module Registry, Module Draw and Dispatch

Written 2026-11-20 as a read-only review of the four repositories; nothing was changed, built or run in them. Kept here, in the repository that holds the shared model, so that the owner and the agents of every app can reach it. Section 0 was added afterwards with the owner's decisions on where things belong and what comes next; sections 1–6 are the review as written.

## 0. Direction (owner's decisions, 2026-11-20)

### The product chain

Each app makes a product that the next one takes as its input:

| Step | App | Works with | Its product |
|---|---|---|---|
| 1 | **Module Draw** | Drawings, with all the metadata needed for **end profiles, modules, operation locations, halls and hall plans**. A hall plan is the final layout: all its modules in the order they appear in it | The layout |
| 2 | **Timetable Planner** | The layout, to which it adds or changes some data on the layout's items; above all it adds **trains with calls, vehicle schedules, the vehicles that work them, and driver duties** | The final operation of the layout: the plan |
| 3 | **Tellurian Dispatch** | The plan, against a **database** in which the state transitions of dispatched trains are stored. A dispatcher can also **cancel trains, or create additional trains**, on request | The operation as it happens |

### The model has three layers, all in this repository

- **Layouts**: what Module Draw makes, and the planner adds to. Today `Model.Layouts`.
- **Timetables**: what the planner adds: trains and calls, vehicle schedules, vehicles, driver duties. Today `Model`.
- **Dispatching**: what Dispatch adds: the state of each dispatched train, its transitions, cancelled and additional trains. Not in this repository yet; today Dispatch has a model of its own.

Tellurian.Trains.Schedules is complete when Module Draw, the Timetable Planner and Tellurian Dispatch can each work with these three layers alone. **The apps add only app-specific functionality and user interface**: no app keeps its own types for a shared concept, and no app maps the shared model into its own. When an app needs something the model lacks, the model is extended here (decision 14 in `ModelHarmonisation.md`).

**The priority now is to harmonise this**: to look at Module Draw and Dispatch and decide what Tellurian.Trains.Schedules needs in order to be complete.

### The Module Registry

The Module Registry keeps its own model. When the three layers here are finished, the gaps between them and the Registry are to be found, so that the Registry can be extended with what it is missing.

The owner plans to extend the Registry's API so that **Module Draw's catalogue can be fetched from the Registry**, and so that **end profiles can be uploaded**, with their drawings included when the catalogue is fetched. The model here is to be harmonised **assuming this is in place**: the catalogue (module standards, scales, end profiles with drawings) is something the model here must be able to represent, read and write.

### Formats and serialisation

- **Reading and writing data, in every format, belongs here.** Importers already exist and are extendable (`IImportService`, `ILayoutImporter`). **Exporters** are to be added beside them, with interfaces of their own and implementations for JSON, databases and possibly others. Today `IExportService` sits in `Importers.Interfaces`, the JSON export in `Importers.Services` and the database export in `Model.Databases`.
- **All serialisation and deserialisation code is here**, so that the same code serves files, databases and API calls, in every app and service.
- **JSON import and export use source-generated serialisation, required by attributes**, so that they work with AOT compilation and trimming: a `JsonSerializerContext` with `[JsonSerializable]` for the types read and written, `[JsonPolymorphic]` and `[JsonDerivedType]` for the hierarchies, and no reflection-based resolver. Today the plan file (`PlanJson`, `PlanReferences`) and the layout JSON (`LayoutSerialization`) use `DefaultJsonTypeInfoResolver` with modifiers that add properties at run time; the build already warns about it (IL2026 and IL3050 in `LayoutSerialization.cs` and `HallPlanFile.cs`). That is to be replaced. Module Draw already serialises this way (`D\Tellurian.Trains.Drawing.Model\Files\DrawingJsonContext.cs`).

### Decided, 2026-11-20

1. **Full geometry in the Layouts layer.** Track elements, end-profile shapes, hall outlines and the placement of modules in a hall plan become model types here. Module Draw keeps only its editor and its rendering, and the Registry's catalogue serves the same types. **Why:** so that every app has the physical layout and can ask questions of it: distances, curve radii, gradients, speed limits and the like. The geometry is therefore not only something to draw: it must be queryable, with explicit units, and give such measures along a route, for instance from one location to the next across the modules between them.
   - **Speed is set by the module owner**, per module. It is data on the module, never deduced from curve radii or anything else.
   - **A distance is always measured along the actual track.** Where several paths lead between two operation locations, the distance is either the shortest or the average of them (which of the two is still to be decided). A planner can enter a distance by hand that overrides the measured one: where module data is missing, or where the stretch between two operation locations isn't made of modules at all, as in plans entered without a hall plan today.
2. **All of Module Draw's files are read and written here**: module, end profile, hall, hall plan and catalogue, with one envelope and one migration mechanism for every file of the suite, plan files included. Module Draw only calls it.
3. **The dispatch state goes in the plan's database.** One database per operated plan holds the layout, the timetable and the dispatch state together; Dispatch adds its tables to the database the planner exports.
4. **An additional train is a Timetables `Train`**, with its calls, marked as created during dispatch; a cancellation is dispatch state on an existing train. The planner can read them back.
5. **A hall plan is a `Layout` plus where it stands in a hall.** The layout holds the modules with their drawings, the lines, joints, locations and stretches, and travels into the plan file and the database; the hall plan adds the hall, the anchors, public areas, walking paths and acknowledgements (`LayoutsLayerSurvey.md` 2.8). Module Draw's own type named `Layout` goes.
6. **JSON default values are handled with attributes only**: `WhenWritingDefault`, and `[JsonIgnore(Condition = Never)]` on each property whose initial value isn't `default(T)`, with a test that every such property has it. Files grow a little (`LayoutsLayerSurvey.md` 4.4, option A).
7. **The Dispatch server creates the operating database** from a plan file (`PlanFile`, then `DatabaseExportService`). The planned conversion service in the Module Registry is not needed for this.
8. **A plan can be corrected during a meeting.** A corrected plan replaces the plan in the operating database and the operation is carried over (`ReplacePlanAsync`); trains and calls created in dispatch get ids from a reserved range (from 1,000,000) so the planner's ids never collide with them (`DispatchingLayerSurvey.md` 4.3, 4.5).

9. **Running times are worked out per module.** The time over a stretch is the sum over the modules of its route, each module's length at that module's speed limit; a module without a speed limit is run at the train's speed. A running time therefore belongs to a train on a stretch, not to the stretch alone. Where the modules between two operation locations are missing, the planner's scenario of today is kept: the stretch's distance and speed are entered by the planner (`IsDistanceEntered`, and a new `IsSpeedEntered`), and the running time is distance over that speed, limited by the train's speed as today.

### Decided, 2026-11-21

10. **Aggregate roots own trees; every other reference is an id.** An object holds objects only downwards, to the children it owns; no child refers back to its parent (`StationTrack.Station`, `OperationLocation.Layout`, `StationCall.Train`, `Train.Timetable` go). Every other reference, also inside an aggregate, is an id property, looked up through the root. A root that exists on its own (module, end profile, standard, hall, hall plan, layout, plan) has a `Guid`; an entity inside a root has an `int`, unique within that root. A module placed in a layout is a copy that keeps its source's `Guid`, not a reference to it. The ids that exist beside today's object references (`ControlledById`, `StartId`, `HeldAtId`, `FromId`, `CompanyId`, `TrackId` …) stay; the object properties go, with `RestoreReferences`. Drawn in the page *Layout Model Aggregates*.
11. **Scale: `Id` is an `int`, `Denominator` a `decimal`** (1:43.5, 1:22.5 and 1:76.2 exist). The planner's ids 1–6 and 101 stay. **The gauge leaves `Scale`**: H0 and H0m are one scale with two gauges.
12. **A catalogue has no id.** It is a set of standards and end profiles carried together, in a JSON file or from the Module Registry's Web API; identity is on each item, its `Guid`. Importing merges with the current catalogue: an item with a new `Guid` is added, one with a known `Guid` replaces the existing. The Registry stores the `Guid` of each standard and end profile; its own ids are external references. This settles "who owns the catalogue's ids".
13. **Gauges are millimetres on the tracks, named in the standard, and a track can have several.** End-profile tracks, track elements and station tracks have `Gauges`, a list of widths in mm (`double`, like every drawn value), more than one for a dual-gauge track. The standard lists the gauges it allows, each with a short name, the width and the prototype gauge (H0 16.5 mm 1435, H0m 12 mm 1000, H0e 9 mm 750), in place of `NormalGauge` and `NarrowGauge`. A module's gauges are worked out from its tracks, not stored (`HasNormalGauge`, `HasNarrowGauge` and `TrackElement.IsNarrowGauge` go). `RailSystem` stays beside the gauges.
14. **Trains stay in the plan** (`Plan.Trains`); a train is not a root of its own.
15. **Lines and joints belong to the hall plan**, since only there are modules joined into lines.
16. **End plates are parts of the module drawing, and exits sit on them.** A plate is a straight stretch of the outline with an `int` id, a pose, its physical width and its own shape in end view: by default its exit's end-profile plate, or cut lower, or of a non-standard height; the screw holes come from the end profiles of its exits. An exit refers to its plate by id and has an offset along it; its pose follows from the plate. A plate can carry several exits (a 546 mm plate taking a FREMO single-track and an MMM double-track module side by side), and exits on one plate whose profiles overlap can't both be joined, worked out from the drawing. Sides run between plates and refer to them by id, no longer to exits by label. **The outline can be drawn first**, with plates marked on it and exits placed on them, as a module is built; **or the exits first**, with plates and sides generated around them, as auto-draw does. Joint fit compares the exits' end profiles; a difference in plate height is reported as a step, not a misfit. Written into Module Draw's `docs/ModuleDrawing.md` §1, §4 and §5 and its specification (MD-5, MD-22).
17. **The exits' directions are read from how the module drawing lies.** The drawing lies the way the module is seen: the viewer stands below it, on the viewing side. The user decides how it lies (a chosen track horizontal, turned 180°, or a curve laid symmetrically with its plates leaning ±half the angle), and turning **rotates the stored geometry**, so every reader sees it the same way. An exit whose heading is within 15° of straight up is Up, within 15° of straight down Down, otherwise Left or Right by where it faces; directions are never entered, and a branch at 30° or 60° from the left end of a station is Left. **Exits on a side are numbered by position**: Left and Right from top to bottom, Up and Down from left to right. Turning 180° swaps Left and Right, the viewing side and the tracks' directions together, in one step. A change of an exit's side or number in a module used in a hall plan is reported when it is updated. This replaces Module Draw's inference by angle (task B2) and its rules for deriving the viewing side (B3 to B5). Written into `docs/ModuleDrawing.md` §2, §2.1, §4.1 and §4.4.
18. **Reading and writing are separate packages.** Reading extends the importers: `Importers.Interfaces` gets `IImporter<T>` (resource key, file extensions, `CanImport`, `ImportAsync` to an `ImportResult<T>`), of which `ILayoutImporter` is the case for a `Layout`; Module Draw's files are read by a new package `Importers.Drawing`, which holds the migrations. Writing is a new family on the same pattern: `Exporters.Interfaces` (`IExporter<T>`, `ExportOptions`, `ExportResult<T>` with `Message`s like `ImportResult<T>`, and `IExportService` moved here), `Exporters.Drawing` for Module Draw's files and the JSON Schemas, and `Exporters.Services` for the plan's JSON. The JSON shape (content types, source-generated contexts, converters, file-kind descriptors and the envelope) stays in `Model.Layouts`, since both directions need it and source generation must sit with the types. Importers never throw for a bad file; `Message` gets a `Code` an app can translate. An exporter counts up the revision and adds the log entry only when asked to (`ExportOptions.IsRevision`), so drafts in browser storage don't. Design: Module Draw session plan of 2026-11-21, written into `LayoutsLayerSurvey.md` §7 step 5.
19. **The plan file stays in `Model` for now.** Moving `PlanFile` and its migrations into Importers and its writing into Exporters is a later step of its own; only its JSON export becomes `PlanJsonExporter`.
20. **Content types that move from Module Draw become mutable classes** like the rest of the shared model (`LayoutPart`, `Copy()` and `With()`, extension data, no back-references per decision 10). Module Draw's editor changes from `with` to `With()`. **Values without identity stay immutable records** (decided 2026-11-21): paths, circles, the plate shape, references, revision entries, gauges, compatibility overrides and the catalogue itself, which is a set.
21. **One `ModuleStandard` in the shared model** (survey step 7, done 2026-11-21 with the catalogue): its `Id` is the catalogue's `Guid` (the Module Registry's id is an external reference); it has Module Draw's properties, all optional beyond id, short name and scale, with their defaults applied where they are used; where a module or layout only names a standard, `standard.NameOnly` gives its short name and scale. `ToShared()` is gone. `Scale.Denominator` is a `decimal` and the scale has no gauge; a standard lists its `Gauges` (width, name, prototype width); an end-profile track has `Gauges`. No plan file holds a standard with the Registry's `int` id (no hall plan has been written with a layout block yet), so the plan file needs no migration for it.

Still open, to be settled as the survey goes:

- **The layout's modules and stretches with lines in the hall plan.** Proposed: the layout keeps the placed modules (what the layout consists of, with their tracks and locations), the hall plan's lines and joints refer to them by id, and stretches are computed from the hall plan and written into the layout as results (locations, modules in route order, measured distance, track count, each module's length and speed limit). References then run one way, hall plan to layout. ~~Open with it: whether the plan file carries only the layout or the hall plan too.~~ **Decided 2026-11-28: the plan keeps only the result**: the layout with measured stretches, light lines and module summaries, and no drawings in the plan file or the database. The modules with their drawings, lines, joints, end profiles, standards, overrides and fit tolerances move from `Layout` to `HallPlan`; this narrows decision 5. Analysis: `ModelSurfaceAnalysis.md`.
- ~~Dual gauge in joint fit~~: **done 2026-11-21** as proposed for the track comparison: two tracks at a joint fit when they share a gauge within the fit tolerance. Reporting a gauge that ends at a joint is still to come with the hall plan.
- **Replacing a catalogue item with an older one.** Proposed: replace when the imported revision is the same or higher, else ask.
- **Names of the layers in code.** Today `Model.Layouts` and `Model`; whether `Model` becomes `Model.Timetables` beside a new `Model.Dispatching` (a breaking rename, so in 4.0.0 if at all).
- **The same layout data changed in two places.** The planner changes some data on the layout's items, and Module Draw can issue a newer hall plan; what wins when both changed the same thing (`ModelHarmonisation.md`, decision 3, still open).
### The surveys, 2026-11-20

Two read-only surveys work out what the layers need, each with an order of work and questions for the owner:

- `LayoutsLayerSurvey.md`: what the Layouts layer needs for Module Draw to work with it alone (geometry, end profiles, modules, halls, hall plans, catalogue, files, source-generated JSON, querying the physical layout).
- `DispatchingLayerSurvey.md`: what a new Dispatching layer contains, and what Layouts, Timetables and the database add for Tellurian Dispatch (operation, operating sessions, train runs and sections, the log, the state machines, the dispatch tables).

Both put **source-generated JSON** and **one shared envelope and migration mechanism** first: they are needed by every later step and remove the warnings the build already gives.

### How this bears on the recommendations in section 6

- 2 (the layout block written by the contract) and 16 (one reference mechanism for layouts and plans) follow directly, and are written source-generated.
- 3 and 14: Module Draw writes its files through code from here, not through its own serialiser context, and gives up its own types for shared concepts.
- 11: the Registry is extended after the model here is finished, from a list of gaps.
- 13: Dispatch takes the Dispatching layer from here and works on the plan's own database; its own model and conversion layer go.
- 3 and 14 go further after decision 1: Module Draw's drawing model (tracks, end profiles, halls) moves here, not only its metadata.

Paths are relative to `C:\Utveckling\Github\Tellurian.Trains\`, shortened as:

| Prefix | Repository |
|---|---|
| `S\` | `Tellurian.Trains.Schedules\` (planner; the contract is `S\Model.Layouts\`) |
| `R\` | `Modules Registry App\` |
| `D\` | `Tellurian.Trains.Drawing\` (Module Draw) |
| `X\` | `Tellurian.Trains.Dispatch\` |

How it was checked: I read the contract and the planner's exchange code myself. The three other repositories were read by separate reviewers; I re-checked their headline claims against the code (marked "re-checked" below). Everything else from them is cited with path and line but was not opened a second time. Anything inferred is marked *(unverified)*.

Two of the four repositories have large uncommitted working copies: the whole of `S\Model.Layouts` is uncommitted, and Module Draw has 219 changed files on top of release 0.9.0. The review describes the working copies.

## 1. Summary

Ranked by what they do to data exchange.

1. **The only exchange the contract was built for does not work yet: Module Draw writes no layout block.** The planner reads `content.layout` of a hall plan (`S\Model.Layouts\HallPlanFile.cs:40,96`), but no code in Module Draw uses `Layout`, `LayoutModule`, `TrackStretch` or `HallPlanFile` (grep, re-checked), and there is no hall-plan editor in the app. Every Module Draw hall plan is refused with "The file has no layout block". `ModelHarmonisation.md` §10 reads as if the chain were complete; only the reading half is.
2. **"0.3.5" is two different contracts.** The package Module Draw compiles against (local feed and NuGet cache, built 2026-11-19 18:57) has no `HallPlanFile`, `LayoutComparison`, `RestoreReferences` or `IsDistanceEntered`, and still has `IsSignal` (string search of the DLL, re-checked). The source, also 0.3.5 (`S\Model.Layouts\Model.Layouts.csproj:8`), has the former and has removed the latter. A removal without a version change breaks the contract's own rule (`S\Model.Layouts\README.md`, last line), and Module Draw's schemas and examples still carry `isSignal`.
3. **Nothing in Module Draw can supply the identities the comparison relies on.** Locations are matched by external reference, then by `int` id within the same source, then by signature (`S\Model.Layouts\LayoutComparison.cs:207-231`). Module Draw has GUID module ids, writes location id 0, numbers tracks from 1 per module, never increments `revision`, and keeps the Registry reference on the module, not the location. In practice every match falls through to the signature.
4. **The Module Registry exchanges nothing the other apps need.** Its public API has three GET endpoints (cargo types, languages, meetings); none returns modules, stations, tracks, standards, scales or regions. It has no CORS policy, accepts no JSON drawing or hall plan, and does not reference the contract (`R\SourceCode\Api\PublicApi.cs:40-45`, re-checked). The planner nevertheless POSTs a whole plan to its base URL (`S\Planning.Components\Services\ModuleRegistryUploadService.cs:34-39`); no endpoint receives it.
5. **Dispatch is a full parallel model on planner packages 2.1.0, with a hand-written mapper that no longer compiles against the planner.** Once the names are fixed it gives distances 1000 times too large, throws on an industrial area, drops dispatch stretches that end at a non-station, and discards the planner's train and call ids (`X\Tellurian.Trains.Dispatch.Data\Timetable\TimetableConversionExtensions.cs:38-47,81-85,121-133`, re-checked).
6. **The same `Layout` type is written in two JSON dialects, and neither the layout block nor the database has a version.** The plan file leaves default values out (`S\Model\Schedules\PlanJson.cs:70-92`); the layout block writes them. The plan file has `schemaVersion` 4; the layout block read by `HallPlanFile.ReadLayout` has none, and the SQLite database is created with `EnsureCreated` and carries no version at all (`S\Model.Databases\DatabaseExportService.cs:36-37`).
7. **The contract breaks its own country and catalogue rules.** The README says countries are ISO codes, "never the id of a table", yet `Region.CountryId`, `Company.CountryId`, `IdentitySettings.DefaultCountryId` and `IdentitySettings.ScaleId` are table ids, and the country catalogue with the Registry's ids is copied into the contract by hand (`S\Model.Layouts\Country.cs:36-57`).
8. **A layout read and written with the contract alone loses data.** A station's regions are resolved only by the planner, through an `internal` member opened with `InternalsVisibleTo` (`S\Model.Layouts\Station.cs:57-64`, `Model.Layouts.csproj:19`, `S\Model\Layouts\Layout.cs:166-178`). Any other app that reads a layout and writes it back writes no `regionIds`.
9. **The Registry API key is stored in the plan, and the plan is what gets sent around.** `IntegrationSettings.ModuleRegistryApiKey` is part of `Layout.Settings` in the contract (`S\Model.Layouts\Settings\IntegrationSettings.cs:13`), so it is written to the plan file and the database (`S\Model.Databases\Mappings\LayoutMapping.cs:79`). That key is the user's `ObjectId`, the same value as the secret in the password link (`R\SourceCode\Services\Extensions\UserExtensions.cs:16,28`).
10. **Enum numbers disagree where the Registry stores them as `int`.** Track direction 0 is "not scheduled" in the Registry and "both" in the contract; theme 0 is "undefined" in the Registry and `American` in the contract (which gives its values no numbers, `S\Model.Layouts\Theme.cs:6-16`). The planner's database stores enums as numbers too.

## 2. Exchange map

What exists in code today. "Planned" means documented but with no code.

| From | To | Carrier | Format and version | State |
|---|---|---|---|---|
| Contract `Model.Layouts` | Planner | Project reference (`S\Model\Model.csproj:22`) | Source, labelled 0.3.5 | Works |
| Contract | Module Draw | NuGet package from the local feed `C:\NuGets` (`D\Tellurian.Trains.Drawing.Model\Tellurian.Trains.Drawing.Model.csproj:9`) | 0.3.5 as built 2026-11-19; older than the source | Works, on a stale build; not on nuget.org |
| Contract | Registry, Dispatch | none | — | No reference in any csproj |
| Module Draw | Planner | Hall plan file, `content.layout` | Envelope `format: tellurian-drawing`, `kind: HallPlan`, `schemaVersion: 2`; the block itself has no version and is in no schema | **Reader only.** `S\Model.Layouts\HallPlanFile.cs`, `S\Importers.Services\HallPlanLayoutImporter.cs`; no writer |
| Module Draw | itself | Module, end profile, hall, hall plan, catalogue files | Same envelope, all at schema version 2, migrations in `D\Tellurian.Trains.Drawing.Model\Files\DrawingFileMigrations.cs` | Works; v2 unreleased |
| Planner | itself | Plan file, browser storage | `format: tellurian-plan`, `schemaVersion: 4`, migrations 1–4 (`S\Model\PlanFiles\PlanFile.cs:24-27`) | Works; newer versions refused |
| Planner | itself | SQLite via EF Core | PascalCase tables, discriminator `Kind`; **no version**; one plan per database | Works (`S\Model.Databases`) |
| Planner | Registry | HTTP POST of the plan JSON, `Authorization: Bearer` | Plan file as above | Client only; the Registry has no POST endpoint and reads the key from `X-API-Key` or `?apiKey=` (`R\SourceCode\Api\ApiKeyFilter.cs:25-42`) |
| Registry | Dispatch | SQLite converted from the plan | — | Planned (`S\Documentation\Requirements specification.md` §5.5) |
| Registry | anyone | `GET /api/cargoes`, `/api/languages/all/supported`, `/api/meetings/{countryId?}` | camelCase, `ReferenceHandler.Preserve` (`$id`/`$values`), no version in the route (`R\SourceCode\Api\PublicApiJson.cs:22-29`) | Works; not used by the other three apps |
| Registry | Module Draw | Catalogue of standards and end profiles; module files | — | Planned (`R\TODO.txt:25-28`; Module Draw tasks H1–H5) |
| Planner | Dispatch | Plan JSON read with `JsonImportService` 2.1.0 (`X\Tellurian.Trains.Dispatch.Server\Services\DataProviderServiceExtensions.cs:128-139`) | Plan format of 2.1.0, before the envelope | Works only for plans in the 2.1.0 format *(unverified: a version 4 plan file was not tried, but 2.1.0 predates the envelope and the id references)* |
| Dispatch | its clients | REST `/api/v1` and server-sent events | Strings only: signatures, display names, "HH:mm" | Works; carries no planner ids |
| Fast clock app | Dispatch | `GET /api/clocks/{name}/time` | `Tellurian.Trains.MeetingApp.Contracts` 3.8.0 | Works |

## 3. Essential differences, per concept

### Operation location

| | Contract / planner | Registry | Module Draw | Dispatch |
|---|---|---|---|---|
| Type | Closed hierarchy of four, discriminator `kind` (`S\Model.Layouts\OperationLocation.cs:25-30`) | One `Station` with flags `IsJunction`, `IsShadow`; `Module.IsSignalModule` (`R\SourceCode\Data\Station.cs:17-32`) | The contract types, inside a module file | Own `OperationPlace` with three kinds, no industrial area (`X\Tellurian.Trains.Dispatch\Layout\OperationPlace.cs:12-38`) |
| Identity | `int` id local to the layout; **equality by signature**, case-insensitive (`OperationLocation.cs:194-200`) | Database id, global | Location id written as 0; module GUID | Planner id; equality by id |
| Name | `Name` | `FullName` (50) | `Name` | `Name` |
| Signature | No limit | 5 characters; 6 on `ExternalStation` | No limit | — |
| Country | `CountryCode` (ISO) | Through the region; `Country.IsoCode` is nullable, `DomainSuffix` is the required one | Contract's | None; a `PreferredLanguage` with no source |
| Regions | List, by id | One `RegionId` | — | — |
| Manned, hide meets, start labels | On the location | On `LayoutStation`, per meeting; two booleans for the one `StartLabels` enum | Written with default values in every module file | `IsManned` hard-coded `true` |

What is lost or converted:

- **Registry to contract.** The kind has to be guessed from flags. SQL already has `IsIndustrialArea`, `IsManned` and `LockKeyName` on `Station` (`R\SourceCode\Database\dbo\Tables\Stations\Station.sql:27,32,34`) but the entity class does not, so EF neither reads nor writes them.
- **Contract equality.** Two locations with the same signature in different countries are one object in every dictionary and `Equals` call, although the design says a signature is unique per country (`OperationLocations.md` decision 14). The hash code is taken from a mutable property.
- **Module Draw.** A module file is meant to leave layout decisions out (`S\Model.Layouts\README.md`); because the whole contract type is serialised, it writes `isManned`, `shiftCount`, `instructions`, `timings` and `id: 0` (`D\schemas\examples\station-3-sections.module.json:33-60`).
- **Dispatch.** `IndustrialArea` throws `UnreachableException` (`TimetableConversionExtensions.cs:46`, re-checked); `TrainsCanCross` is replaced by "more than one track"; junction is never set.
- **Unknown kind.** `OperationLocationJson.Normalise` turns any `kind` it cannot parse into `Station` (`S\Model.Layouts\OperationLocationJson.cs:49-52`), so a kind added later is read by an older app as a station, silently.

### Station track

| | Contract | Registry | Module Draw | Dispatch |
|---|---|---|---|---|
| Name | `Designation` | `Designation` (5) | Contract's | `Number` |
| Id | `int`, unique in the layout | Database id | `int`, lowest free number **per module** (`D\Tellurian.Trains.Drawing.Model\Tracks\StationTrackList.cs:40-47`, re-checked) | None; record equality over all members |
| Length | `Length`, metres, `double` | `MaxTrainLength`, metres | Millimetres in the drawing, converted for the track list (`Tracks\StationTrackMeasurement.cs:33,74,86`) | `int?`, truncated; 0 becomes null |
| Platform | `double`, 0 = none | `double?`, null = none | Contract's | Dropped |
| Scheduled | `IsScheduled` | Folded into the direction (`0 = NotScheduled`); the old `IsScheduled` column is obsolete but still `NOT NULL` | Contract's | Unscheduled tracks are filtered out |
| Direction | `TrackDirection` Both=0, LeftToRight=1, RightToLeft=2, plus `PreviousLocationId`/`NextLocationId` | `StationTrackDirection` NotScheduled=0, EastWest=1, WestEast=2, Bidirectional=10 (`R\SourceCode\Data\StationTrackDirection.cs`, re-checked) | Contract's | None on a station track |
| Main, stub | `IsMain`, `IsStub` | `IsSiding` (inverse), `IsThroughTrack` (meaning not confirmed) | Contract's | `IsMainTrack` only |
| Usage, lockable | `Usage`, `IsLockable`, `KeyName` | `UsageNote` (20 in EF, 50 in SQL); `IsLockable` in SQL only | Contract's | Dropped |

- **Where the tracks are written differs.** Module Draw writes `module.tracks` and an empty `operationLocation.tracks`; the contract's layout options omit `module.tracks` for a location module and replace the module's list with the location's on reading (`S\Model.Layouts\LayoutSerialization.cs:94-96,146-151`). A module copied as-is from a module file into a layout block would lose its tracks.
- **The track id is not trusted as identity even inside the contract.** `CompareTracks` matches on id and designation, else on designation alone (`LayoutComparison.cs:269-270`). A track renamed in the hall plan is reported as one removed and one added, and removing a track is a change the planner must confirm because it moves calls.
- **An unknown track in Dispatch** silently becomes the place's first track (`TimetableConversionExtensions.cs:155-156`).

### Track stretch

| | Contract | Module Draw | Dispatch |
|---|---|---|---|
| Ends | `startId`, `endId`, `StartSide`/`EndSide` | Nothing computes them | Start and end by planner id |
| Distance | Metres, `double`, centre to centre | Not computed; the default centre exists only in the UI (`D\...\ModuleLocationPane.razor:108`) | `(int)(Distance * 1000)`, written when the planner had kilometres |
| Tracks | `TracksCount` | Not computed | A `Track` per line track with direction, all invented as double-directed |
| Id | `int` | — | Discarded and regenerated |

- The Registry has no stretch in code. Its SQL has an unused timetabling schema, including a `TrackStretch` with `DistanceMeters`, with no entity (`R\SourceCode\Database\dbo\Tables\Timetabling\`).
- Dispatch needs direction per track; the contract has only a count. That information cannot cross today.
- `RestoreReferences` leaves `Start` or `End` null when an id is not found, behind a `!` (`LayoutSerialization.cs:135-136`), and takes the first of two objects with the same id (`:111-112`). A damaged file is not reported; it fails later.

### Module, standard and scale

- **Module id.** Contract `LayoutModule.Id` is an `int` given once; Module Draw's module id is a GUID and its `Module` subclasses the shared one to add it (`D\Tellurian.Trains.Drawing.Model\Modules\Module.cs:24-28`, re-checked); the Registry's is a database id. `LayoutModule.SourceId` (GUID) is the bridge, but nothing writes it.
- **Standard.** Three shapes: contract `ModuleStandard` (short name, Registry id, scale); Module Draw's own record with a GUID id, the scale as a string and values in millimetres, mapped with `ToShared()` (`D\...\Catalogue\ModuleStandard.cs:10-19,79`); the Registry's table. A Module Draw module carries both `standardId` (GUID) and `standard.shortName`, with nothing keeping them in step.
- **Scale.** The planner keeps its own catalogue with ids 1–6 and 101 (`S\Model\Layouts\Scale.cs:16-25`) and stores `Settings.Identity.ScaleId`. A layout from a hall plan also has `PreferredStandard.Scale`. The import copies the scale only when it has an id (`S\Model\Layouts\HallPlanImport.cs:62`); a hall plan made without the Registry leaves the planner on its default, H0, whatever the hall plan says. Whether the planner's scale ids equal the Registry's is *(unverified)*.
- **Units.** Module geometry is millimetres in the Registry and in Module Draw, track lengths metres in both, all `double` with no unit in the name. Module Draw's generated schemas say "Lengths in millimetres" for every file, which is false for the contract's `length`, `platformLength` and `centre` (`D\...\Files\DrawingFileSchema.cs:63`).
- **FREMO name.** The contract writes prefix, space and four digits (`S\Model.Layouts\Module.cs:77-80`); the Registry writes signature and three digits without a space (`R\SourceCode\Data\Module.cs:259,263`). `HasFremoDesignation` is never set by Module Draw.
- **Exit side.** Contract `ExitSide` and Module Draw `ExitDirection` are the same five values in two enums (`D\...\Modules\ExitDirection.cs:7-23`); the Registry has `Eastwards = 1, Westwards = 2`.

### Company

Contract `Company`: id, name, signature, `CountryId` (table id), logo inline (`S\Model.Layouts\Company.cs`). Equality by signature. Dispatch has `Company(Name, Signature)` with no id, and fills in `"Unknown"`/`"??"` when the train has none. The Registry and Module Draw have no company. `Company` does not derive from `LayoutPart`, so unknown properties on it are dropped.

### Country

Four representations: ISO code on a location; table id on region, company and settings; a hard-coded catalogue of 18 countries in the contract "matching the Module Registry"; a database table in the Registry whose ISO code is nullable; and a fifth, Module Draw's own `Country` type with `UserSettings.CountryId` as a Registry id (`D\Tellurian.Trains.Drawing.App\Settings\UserSettings.cs:12`).

- The planner works in ids internally through an extension that converts on every access (`S\Model\Layouts\OperationLocation.cs:18-21`). A location in a country outside the catalogue (Spain, say) keeps its code but is treated as having no country by `MostCommonCountryId` and `EnsureCountries`.
- A country the Registry adds as id 15 does not exist for the other apps until the contract is released again.

### Region

Contract: `Name`, `BackgroundColor`, `CountryId?`. Registry: `EnglishName`, `LocalName`, `ForeColor`, `BackColor`, required `CountryId` (`R\SourceCode\Data\Region.cs:16-24`). One name against two, different property names, no fore colour. A station has several regions in the contract and one in the Registry.

### Person, owner, participant

No identity crosses, by design: the contract has `Module.ModuleOwners` as one text and `OperationLocation.Owner` as a text. The planner has its own `Participant` (id, name) for vehicle owners (`S\Model\Schedules\Participant.cs:38-43`), unrelated to the Registry's `Person`. Fine for now; it means a meeting's participants are typed in twice.

### Meeting and layout

"Layout" means three things: the contract's locations and stretches; the Registry's registration container per meeting (`R\SourceCode\Data\Layout.cs:14-28`); Module Draw's geometric arrangement in a hall plan (`D\...\HallPlans\Layout.cs:9-11`). The contract has no meeting. Nothing links a plan to a Registry meeting or layout id, so the planned conversion service would not know which meeting a posted plan belongs to. Module Draw's schema generator adds definitions by bare type name; the contract's `Layout` would collide with its own (`DrawingFileSchema.cs:96-98`).

### Time

- Planner: `Time`, a `TimeSpan` where a day of 1 or more means past midnight. JSON is the .NET "c" format, e.g. `1.00:15:00` (`S\Model\Timetables\Time.cs:160-161`); the database stores ticks (`S\Model.Databases\Mappings\Conversions.cs:13-15`).
- Dispatch: `TimeSpan` with no day or session; output formats `time.Hours`, so the day is dropped (`X\...\Server\Mapping\DataMappingExtensions.cs:110-111`). The fast clock delivers "HH:MM".
- Registry: `DateTime` without zone for meetings; an `OperatingDay` table with seven booleans, against the planner's session bits relative to `StartDay`.

Both planner forms are readable only by .NET code that knows them. A dispatch app reading the SQLite file has to know that a time is ticks and that sessions are bit positions.

### Train and call

Planner: `Train` with id, number, category, company, sessions; `StationCall` with id, track, arrival, departure. Dispatch: `Train` identified by category prefix and number, generated id; calls ordered by departure time rather than the planner's run order; all trains loaded regardless of session (`X\...\Data\Timetable\TimetableBrokerDataProvider.cs:164,186,210-212`). Planner ids are discarded, so Dispatch's saved state (CSV keyed by generated ids from one process-wide counter) cannot be tied back to a plan, and depends on load order. Two planner trains with the same prefix and number collapse into one *(unverified, inferred from the code)*.

## 4. Redundancy

**The same concept as separate types**

| Concept | Where |
|---|---|
| Operation location and its kinds | Contract; `R\SourceCode\Data\Station.cs`; `X\Tellurian.Trains.Dispatch\Layout\OperationPlace.cs`, `SignalControlledPlace.cs`, `OtherPlace.cs`, `Trains\Station.cs` |
| Station track | Contract; `R\SourceCode\Data\StationTrack.cs`; `X\...\Trains\StationTrack.cs` |
| Track and dispatch stretch | Contract; `X\...\Layout\TrackStretch.cs`, `DispatchStretch.cs` (which recomputes the path the contract already carries in `Stretches`) |
| Module standard, scale | Contract; `D\...\Catalogue\ModuleStandard.cs`, `Scale.cs`; `R\SourceCode\Data\ModuleStandard.cs`, `Scale.cs`; the planner's scale catalogue `S\Model\Layouts\Scale.cs` |
| Country | Contract `Country.cs`; Registry table; `D\...App\Settings\Country.cs` and its embedded list `Countries.cs:7-8` |
| Exit side | `S\Model.Layouts\ExitSide.cs`; `D\...\Modules\ExitDirection.cs`; `R\SourceCode\Data\ModuleExitDirection.cs` |
| Theme | Contract enum; Registry enum plus free text in three places (`Module.Theme`, `Layout.Theme`, `ModuleStandard.MainTheme` beside `MainThemeId`); `string?` in Module Draw (`Modules\Module.cs:39`) |
| Timetable, train, call | Planner `Model`; Dispatch `Trains\`; an unused SQL schema in the Registry (`Database\dbo\Tables\Timetabling\`) |

**Serialisation code**

- `S\Model.Layouts\LayoutSerialization.cs:19-106` and `S\Model\Schedules\PlanReferences.cs:21-80` are the same mechanism written twice: a pending-values table, a `FileProperty` record, `Single`/`List` helpers, a not-written list. They also differ slightly: the plan version removes every property without a getter or setter, the layout version does not.
- Three sets of serialiser options for contract types: `Layout.JsonSerializerOptions`, `PlanJson.CreateOptions`, and Module Draw's source-generated context (`D\...\Files\DrawingJsonContext.cs:17-29`).
- Two envelope readers of the same shape (`format`, `kind`, `schemaVersion`, `appVersion`): `PlanFile` and Module Draw's `DrawingFile`; `HallPlanFile` reads the second by string constants.
- Dispatch test data in a third JSON format that nothing reads (`X\Tellurian.Trains.Dispatch.Data.Tests\TestData\junction-layout.json`, `junction-trains.json`).

**Validation rules**

- Module Draw validates tracks with its own codes (`D\...\Tracks\StationTrackValidation.cs:7-29`) and deliberately not with `OperationLocationRules`; it does use the contract's rules for the location.
- Module number and prefix rules exist only in Module Draw (`Modules\ModuleValidation.cs:11-34`); the contract formats them but does not check them.

**Resource strings**

- The contract has no resources, so each app translates its enums and `LocationProblemCode` itself: Module Draw in `AppTexts.resx:443-445,478-481,501-511`.
- Country names are in `S\Planning.App.Translations\Resources\Labels*.resx` and in `R\SourceCode\Resources\Strings*.resx` (key `CzechRepublic` found in both).
- Track-direction and kind words are in the Registry's `Strings.resx` (`EastWest`, `Bidirectional`, `Junction`, `ShadowStation`).

## 5. Anti-patterns

### Contract (`S\Model.Layouts`)

- `Model.Layouts.csproj:8` — a version number reused for changed content, including a removed property.
- `Model.Layouts.csproj:19`, `Station.cs:57-64` — `InternalsVisibleTo` for the planner; `RegionIds` reads one field and writes another. Regions survive only in the planner.
- `Region.cs:18`, `Company.cs:35`, `Settings\IdentitySettings.cs` (`ScaleId`, `DefaultCountryId`) — table ids where the README forbids them.
- `Country.cs:17,36-57` — reference data owned by the Registry compiled into the contract; also a record in namespace `...Model`, not `...Model.Layouts`.
- `Settings\IntegrationSettings.cs:10-13` — a secret and a service URL as layout data.
- `OperationLocation.cs:194-200`, `StationTrack.cs:120-126`, `Company.cs:49-55` — equality and hash code from mutable text; `Layout.cs:78-84` — equality by an id that is usually 0.
- `OperationLocationJson.cs:49-52` — unknown kind becomes `Station`.
- `LayoutSerialization.cs:111-112,135-136` — duplicate ids and dangling references accepted without a message.
- `LayoutSerialization.cs:48-55` — reflection-based resolver; Module Draw forbids reflection serialisation, so it may not be able to use the contract's own options *(unverified)*.
- `LayoutComparison.cs:229-231` — external references compared by raw JSON text of a whole entry; a reordered or extended entry (the Registry's version mark, say) stops matching.
- `LayoutModule.cs:25`, `OperationLocation.cs:147` — external references as untyped `Dictionary<string, JsonElement>`; the one key every app must agree on, `moduleRegistry`, is defined nowhere in the contract.
- `Company.cs`, `DispatchStretch.cs`, `TimetableStretch.cs`, `TopologyPosition.cs` — not `LayoutPart`, so unknown JSON properties are dropped, against the README's "unknown properties are kept".
- `HallPlanFile.cs:28-52` — reads a block with no version check; the contract can read the block but has no method to write the file.
- `Theme.cs:6-16` — enum values without numbers, stored as numbers by the planner's database.
- `StationTrack.cs:14` — `DefaultPlatformLength = 1`, a planner convenience, as a public constant of the contract.
- Open by the owner's own note, listed for completeness: setters that ignore their value in the kind subclasses (`SignalControlledLocation.cs:22-25`, `IndustrialArea.cs:16-22`, `OtherLocation.cs:16-22`).

### Planner

- `S\Model\Schedules\PlanJson.cs:70-92` — defaults left out, taken from a new object. A changed initial value in the contract silently changes what every saved plan means, and the contract's version is separate from the plan's schema version.
- `S\Planning.Components\Services\ModuleRegistryUploadService.cs:9-10,34-39` — posts to an endpoint that does not exist, with a header the Registry does not read.
- `S\Model\Layouts\OperationLocation.cs:18-21` — id/code conversion on every access; unknown codes read as null.
- `S\Model\Layouts\HallPlanImport.cs:62` — the scale is taken only when it has an id.
- `S\Model\Model.csproj:13`, `S\Model\Layouts\Region.cs:1,18` — the model still depends on `Microsoft.AspNetCore.Components`; any consumer of the `Model` package (Dispatch) gets a UI framework.
- `S\Model.Databases\DatabaseExportService.cs:36-37` — no schema version in a database meant for distribution; times as ticks, enums as numbers.
- `S\Documentation\ModelHarmonisation.md` §10 — says "the Module Draw importer … reads" without saying nothing writes it.

### Module Registry

- `R\SourceCode\Services\Implementations\UserService.cs:47,75-90`, `Api\ApiKeyFilter.cs:34-42` — the API key is the user's `ObjectId`, accepted in the query string, replaced on password reset. Already known (`OperationLocations.md` 6.7); still so.
- `R\SourceCode\Api\PublicApiJson.cs:26` — `ReferenceHandler.Preserve` on flat DTOs.
- `R\SourceCode\Services\Implementations\MeetingService.cs:21-26` — API values are for display: country as a localised name, status as a localised enum name, scale as a denominator.
- `R\SourceCode\Data\Module.cs:47-49,59-62`, `ModuleExit.cs:13`, `StationTrack.cs:17` — enum values held in `int` properties.
- `R\SourceCode\Data\StationTrack.cs:15-16` — obsolete `IsScheduled` kept.
- `R\SourceCode\Database\dbo\Tables\Stations\Station.sql:27,32,34`, `StationTrack.sql:8,12` — SQL and entity classes out of step.
- `R\SourceCode\Data\Country.cs:15-21` — two two-letter codes; the API filters on `DomainSuffix`, not the ISO code.
- `R\SourceCode\Model\Model.csproj` — an empty net7.0 project.

### Module Draw

- `D\Tellurian.Trains.Drawing.Model\HallPlans\HallPlan.cs:17,59`, `Modules\Module.cs:68` — `revision` documented as "incremented on every save"; no code changes it (re-checked). `Layout.SourceRevision`, and the planner's "is the file older?" warning, depend on it.
- `D\...\Files\DrawingFileMigrations.cs:16-17` — `UpgradeDraft` re-runs the v1→v2 step on every v2 file.
- `D\...\Catalogue\ModuleStandard.cs:79` — `ToShared()` is the mapping code the owner's rule excludes.
- `D\...\Modules\ExitDirection.cs:7-23` — duplicates `ExitSide`.
- `D\...\Modules\ExitPlace.cs:24,32` — an exit addressed by side and position in a list; reordering exits re-targets hall-plan references.
- `D\...\Tracks\TrackElement.cs:11-18` — discriminator `type` with lower-case values beside `kind` with PascalCase values.
- `D\...\Files\DrawingFile.cs:7-21` — the envelope drops unknown properties.
- `D\...\Files\DrawingFileSchema.cs:63` — one unit statement for the whole schema, false for contract properties.
- `D\...\Files\SchemaVersion2.cs:79,139-146,171-180` — the v1→v2 migration drops non-default variants, owner shares and `measures`.
- `D\docs\OperationLocationsTasks.md:25` — still describes 0.1.0 types.

### Dispatch

- `X\Tellurian.Trains.Dispatch.Data\Timetable\TimetableConversionExtensions.cs:4-16` — nine type aliases to tell its types from the planner's same-named ones.
- `:41` `IsManned = true`; `:43` `?? 0` for the controlling station; `:84` `* 1000`; `:61,84` truncation to `int`; `:126-127` `"Unknown"`/`"??"`; `:46` exception on a fourth kind (all re-checked).
- `:73-77,150-158,188-192` — stretches, calls and dispatch stretches dropped without a message.
- `X\Tellurian.Trains.Dispatch\Utilities\UtilityExtensions.cs:6,10` — one static id counter for all types.
- `X\Tellurian.Trains.Dispatch\Layout\DispatchStretchExtensions.cs:40-41` — `Single()` assumes consecutive calls are the two ends of one dispatch stretch; a call at a halt in between would throw *(unverified, not run)*.
- `X\Tellurian.Trains.Dispatch.Server\appsettings.json:19-20` — absolute paths into the planner repository's test data.
- `X\...\Server\README.md:243-262` — a database source documented, not implemented.

## 6. Recommendations

In order. "Contract" means a change in `S\Model.Layouts`.

1. **Stop reusing version numbers; publish what Module Draw must build against.** Bump the contract (0.4.0, since `IsSignal` went) and push it to the local feed; Module Draw updates and drops `isSignal` from schemas and examples. Repos: Schedules, Drawing. Contract: version only.
2. **Put the writing of the layout block in the contract.** Add `HallPlanFile.WriteLayout` (or a method that puts the block into a hall-plan JSON tree) beside `ReadLayout`, with a `layoutVersion` in the block that the reader checks. Then both ends are one piece of code and Module Draw needs no serialiser options of its own for it. Check first that the options work without reflection, or give the contract a source-generated context. Repos: Schedules. Contract: yes.
3. **Build the block in Module Draw (tasks G6–G8) with stable identities.** Needed in Module Draw: an `int` per placed module kept in the hall plan (not regenerated at save), track ids renumbered to be unique in the layout or the contract's rule relaxed to "unique within the location", the location's external reference written on the location, `SourceId` on each layout module, and `revision` actually incremented on save. Repos: Drawing. Contract: decide and state the track-id scope.
4. **Make the comparison use the ids it is given.** Match a track by id first when both layouts have the same source; compare external references by a typed key (system and id), not by raw text. Add a small `ExternalReference` type and name the `moduleRegistry` key in the contract. Repos: Schedules. Contract: yes.
5. **Move region resolution into `RestoreReferences` and remove `InternalsVisibleTo`.** Repos: Schedules. Contract: yes.
6. **Take the API key and URL out of the layout.** Keep them in app settings (browser storage), never in a plan file or database; migrate existing plans to drop the property. Repos: Schedules (plan schema version 5). Contract: yes, a removal, so a version bump. Separately, the Registry gets a key of its own (already proposed in `OperationLocations.md` 6.7).
7. **Decide the country and scale references once.** Either ISO codes everywhere (`Region.countryCode`, `Company.countryCode`, `defaultCountryCode`) and scale by short name, or state that ids are allowed and why. The first matches the README. Repos: Schedules (plan migration, database mapping), Drawing (`UserSettings.CountryId`). Contract: yes.
8. **Give every enum in the contract explicit numbers, and report an unknown `kind` instead of reading it as a station.** Repos: Schedules. Contract: yes.
9. **Version the database.** A one-row table or `PRAGMA user_version` with the plan schema version, checked on reading. Repos: Schedules. Contract: no.
10. **Make everything in a layout keep unknown properties.** `Company`, `DispatchStretch`, `TimetableStretch` and `TopologyPosition` derive from `LayoutPart`. Repos: Schedules. Contract: yes, additive.
11. **Registry: map the columns that already exist in SQL, then serve the contract.** Add `IsIndustrialArea`, `IsManned`, `LockKeyName`, `IsLockable` to the entities; add a `Kind` column as planned; reference the contract and add the two endpoints of `OperationLocations.md` 6.7 under a versioned route without `ReferenceHandler.Preserve`, with CORS for the two browser apps. Convert track direction and exit direction by name, never by number. Repos: Registry. Contract: no, unless a property is missing, and then extend it.
12. **Decide what the planner sends to the Registry before keeping the upload button.** Today it posts to nothing. If the conversion service is still wanted, define the endpoint in the Registry and which meeting or layout the plan belongs to (an external reference on the layout); otherwise hide the button. Repos: Schedules, Registry. Contract: possibly an external reference on `Layout`.
13. **Dispatch: replace the parallel layout model with the contract when it is taken up.** At least: reference `Model.Layouts` and the current `Model`, use `OperationLocation` and `StationTrack` directly, keep planner train and call ids, read distance as metres, handle all four kinds. What Dispatch needs and the contract lacks (direction per line track) is added to `TrackStretch`, not invented at import. The planner's `Model` should lose its `Microsoft.AspNetCore.Components` dependency first. Repos: Dispatch, Schedules. Contract: yes, per-track direction on a stretch.
14. **Module Draw: remove the duplicates of contract types.** `ExitDirection` gives way to `ExitSide`; its `ModuleStandard` and `Scale` either become the contract's (extended with what Module Draw needs) or are clearly a catalogue entry that refers to the contract's by short name, without `ToShared()`. State units per property in the schemas. Repos: Drawing. Contract: yes, where properties are missing.
15. **One place for the translations of contract enums and problem codes.** A small resources package beside the contract, or resource keys published by it, so that four apps do not each translate `OperationLocationKind`, `TrackDirection` and `LocationProblemCode`. Repos: Schedules, then the others. Contract: a companion package, to keep the contract free of dependencies.
16. **Fold `PlanReferences` and `LayoutSerialization` into one mechanism**, and correct `ModelHarmonisation.md` §10 and `OperationLocationsTasks.md:25` to say what is and is not built. Repos: Schedules, Drawing. Contract: internal only.

Areas that are fine: the plan file's envelope, version check and migration chain; Module Draw's envelope and migration chain apart from `UpgradeDraft`; the Registry's API DTOs being separate from its entities; unknown-property preservation on `LayoutPart` types.
