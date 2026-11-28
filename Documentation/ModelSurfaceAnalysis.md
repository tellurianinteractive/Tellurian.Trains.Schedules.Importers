# What the shared model needs to expose: the planner and the Module Registry

Written 2026-11-28 as a read-only analysis; nothing was changed. It asks what `Model.Layouts` (with the drawing types moved from Module Draw) has to expose to meet two requirements, and what can go back to being Module Draw's own business. It was checked against the source of this repository, Module Draw (`Tellurian.Trains.Drawing`, packages 4.4.4) and the Module Registry (`Modules Registry App`, packages 4.4.0) as they are in their working copies.

Paths are shortened as in `CrossAppReview.md`: `S\` this repository, `D\` Module Draw, `R\` the Module Registry (`R\SourceCode\` unless said otherwise).

## 1. The requirements

1. **Timetable Planner.** It takes a layout drawn in Module Draw. Its only new feature is to work out the **track stretches**: the locations at each end, the length, the number of tracks, the average speed, and the other properties a `TrackStretch` has today. Everything else it does with a layout already works without drawings.
2. **Module Registry.** It **displays Module Draw drawings** (modules, end profiles, halls), optionally **with measures**, and **reads and writes the properties a drawing shares** with the Registry object it stands for: module, exit, station, station track, end profile, hall/locality.

Other layout and drawing matters can be Module Draw's alone.

## 2. Summary

- **Today there is one package, `Model.Layouts`, and it carries everything.** It holds the operating layout (locations, tracks, stretches), the planner's settings and catalogues, and the whole drawing model: module drawings, track elements, end profiles, halls, hall plans, catalogues, auto-draw and file formats. All of it is public, and every app gets all of it.
- **The planner uses almost none of the drawing model, but stores all of it.** No planner project names a type from `AutoDraw`, `Catalogues`, `EndProfiles`, `Halls`, `Measures`, `Modules`, `Tracks` or `Model.Geometry`. Even so, a plan made from a hall plan writes every module's full `ModuleDrawing`, the end profiles, standards, overrides, lines, joints and fit tolerances into the **plan file and the SQLite database** (`S\Model.Layouts\Layout.cs:75-105`, `Module.cs:229-236`, `S\Model.Databases\Mappings\LayoutMapping.cs:43-50`). The planner registers the `Point`, `Angle` and `Pose` converters only so that it can write drawings it never reads (`S\Model\Schedules\PlanJson.cs:44-46`).
- **The planner needs the drawings only while a hall plan is imported.** Stretches are worked out by `layout.UpdateTrackStretchesFromLines()` (`LineWalker`, `TrackGraph`). After that, two editors still read the drawings: `TrackStretchesEditor` (measured distance and speed beside the entered ones) and `TimetableStretchesEditor` (suggested timetable stretches from the lines). Both can be served by stored results. Timetable stretches are the planner's alone: made by hand, or with a suggestion feature that only the planner has, so neither they nor the suggesting belong in the shared packages.
- **The Registry already displays drawings** (module, end profile, hall) with Module Draw's components package. It reads and writes the files with `Importers.Drawing` and `Exporters.Drawing`, and serves the catalogue. What it lacks:
  - a **module view with measures**: no component draws them, though `ModuleMeasurement` exists;
  - any **copying of module, station and track properties** between the drawing and its own rows. Today only the Registry id and the owners go into the file, and only an end profile's width and height come back out of it.
- **Recommendation: split the package in two along the product chain, and give the editor's algorithms back to Module Draw.**
  - `Model.Layouts`, slimmed, becomes the **operating layout**: what the planner and Dispatch use.
  - A new `Model.Drawings` holds the **drawn layout**: what Module Draw makes and the Registry shows. The work of turning a hall plan into stretches lives here, and the planner calls it only when it imports.
  - Auto-draw and the editing helpers go back to Module Draw.

  Details in section 5.

## 3. The planner: what it needs

### 3.1 What it uses today

From the planner survey: `Model` is the only planner project that references `Model.Layouts`; the others get it through `Model`.

| Part of `Model.Layouts` | Used by the planner |
|---|---|
| Root: `Layout`, `OperationLocation` and its kinds, `StationTrack`, `TrackStretch`, `TimetableStretch`, `DispatchStretch`, `TopologyPosition`, `Company`, `Region`, `Country`, `Scale`, `LockKey`, `StaffPost` | Throughout: this is the planner's own model, moved here in step 4 of `ModelHarmonisation.md` |
| `Settings`, `Timetables` (`StartLabelPrinting`) | Heavily. This is planner data |
| `Files` (`FileEnvelope`, `FileKind`, `FileMigration`, `FileFormatException`) | The plan file. The drawing file kinds and `ModuleFileContent` are not used |
| `HallPlanFile.IsHallPlan`, `ReadLayout` | The hall-plan importer (`S\Importers.Services\HallPlanLayoutImporter.cs:20,30`) |
| `LayoutComparison`, `LayoutUpdate`, `LayoutChange` | Updating from a newer revision (`S\Model\Layouts\HallPlanImport.cs`, `LayoutChangeRules.cs`, `LayoutChangesList.razor`) |
| `Lines`: `UpdateTrackStretchesFromLines`, `StretchesFromLines`, `SuggestedTimetableStretches`, `TakeLinesFrom` | Import, and the two stretch editors |
| `Layout.Modules`, `Lines`, `Joints`, `EndProfiles`, `Standards`, `Overrides`, `FitTolerances` | **Only copied and persisted.** No planner code reads a member of `LayoutModule` or `Module` |
| `ModuleStandard` | Only `.Scale` (`HallPlanImport.cs:67`) |
| `AutoDraw`, `Catalogues`, `Drawings`, `EndProfiles`, `Halls`, `Measures`, `Modules`, `Tracks`, `Model.Geometry` | **None**, apart from the three JSON converters |

### 3.2 What working out a stretch takes

`LineWalker` and `TrackGraph` (`S\Model.Layouts\Lines\`) need:

| Result on `TrackStretch` | Worked out from |
|---|---|
| `StartId`, `EndId`, `ModuleIds`, `StartSide`, `EndSide` | The lines (`Line`: module ids, entry and exit `ExitPlace`, `StartAt`/`EndAt`). The modules' exits (`ModuleDrawing.Exits`, `ExitSide`). Which modules are locations (`Module.OperationLocation`) |
| `TracksCount` | The fewest tracks anywhere between the two locations: over every module of the stretch, and over the end profiles at **both** sides of every joint passed, since a module with a double-track exit can be joined to a single-track one. In code, `LineWalker.CountTracks` is called for the exit on each side of each joint (`LineWalker.cs:61-62`), with the count looked up from `ModuleExit.EndProfile` in `Layout.EndProfiles`, and the stretch takes the minimum (`:93`). The modules' own track counts (`Module.NumberOfThroughTracks`, or the drawn track elements) are not counted yet; only the end profiles are |
| `Distance` | The main paths: `ModuleDrawing.MainPath.ToPath().Length` of each plain-line module, plus centre to exit in the two location modules (`OperationLocation.Centre`). Failing that, the shortest route through `TrackNetwork(drawing)` from `TrackElements`, joined across joints by `EndProfileTrack.Offset` within `FitTolerances.Joint` |
| `Speed` | `Module.SpeedLimit`, weighted by each module's length along the stretch |
| `Time` | `TrackStretchTiming.RunningMinutes`, from distance, speed and `TimeAndSpeedSettings`. This is planner logic, not drawing logic |

Halls, anchors, placements and hall coordinates are **not** used (`TrackGraph.cs:11-12`).

### 3.3 What the planner needs to keep after the import

| Needed | Why | Where it should live |
|---|---|---|
| Locations, tracks (with measured `Length` and `IsStub`), stretches | Its model | `Model.Layouts` |
| On each stretch: `TracksCount`, the fewest tracks over the modules and the end profiles on both sides of every joint (3.2) | Its model | `Model.Layouts`, worked out at import |
| On each stretch: the **measured** distance and speed, beside the values in use | `TrackStretchesEditor` shows them beside the entered values. Today it works them out again from the drawings on every render (`StretchesFromLines`) | Two stored results on `TrackStretch` (e.g. `MeasuredDistance`, `MeasuredSpeed`), set at import, `null` for a stretch entered by hand. The present extension member `stretch.MeasuredDistance` goes |
| The order of the locations along each line | The planner's suggestion of timetable stretches. Timetable stretches are made by hand or suggested; only the planner needs either (owner, 2026-11-28) | The suggesting moves from `Model.Layouts` (`LineStretches.SuggestedTimetableStretches`) to the planner's `Model`. It needs only the line order: a light `Line` on the layout (id, name and the ordered track-stretch ids), worked out at import. `TimetableStretch` itself is Timetables data (5.1) |
| `SourceId`, `SourceRevision` | "Is this a revision of my source?" | `Model.Layouts` |
| Scale (and, if wanted, the standard's short name) | Settings and the identity of the plan | `Model.Layouts`: a `Scale` and a standard by name only (`standard.NameOnly` exists) |
| A module list: id, source `Guid`, title, owners, package label, location id | Packages shown, decision 7 of `ModelHarmonisation.md`. Updating matches modules by source | A summary `LayoutModule` **without** `Module.Drawing`, tracks of plain line, add-ons, revision log, functional state and the like |
| `ExternalReferences` | Matching locations and modules on update | `Model.Layouts` |
| A location's `HasTurntable` and `HasSignals` | **The planner uses them** (owner, 2026-11-28). Only an operation location needs them; the module features (`IsDuckunder`, `IsTurntable`, `IsSignalModule`, `IsEndOfLine`) are not needed by the planner | On the location in `Model.Layouts`, per kind (owner, 2026-11-28): a `Station` can have a built-in turntable (`HasTurntable`, as today) and signals, at least those controlling entry and exit (`HasSignals`, stored); an `IndustrialArea` may have signals (`HasSignals`, stored); a `SignalControlledLocation` has signals by design, so `HasSignals` is always true there and not stored. Following decision 11, each kind has only what it can have; a switch over the kinds (`location.HasSignals`) reads it for any location. An `OtherLocation` never has signals or a turntable (owner, 2026-11-28). The plan has no drawing, so what is worked out from the drawing (4.2.1) is worked out at import and stored as the result; what the user entered in Module Draw is copied. A planner without a hall plan enters them by hand |

The planner does **not** need to keep: `ModuleDrawing`, `TrackElement`, `EndProfile`, full `ModuleStandard`, `EndProfileCompatibilityOverride`, `FitTolerances`, `Joint`, the full `Line` with `ExitPlace`, `Hall`, `HallPlan`, measures, catalogues, the drawing converters. All of these are needed **during** the import and can be read by an importer that the planner app references (`Importers.Drawing`), without `Model` or the plan file knowing them.

**Side effect:** plans made from hall plans get much smaller, and the database loses seven JSON columns.

## 4. The Module Registry: what it needs

### 4.1 What it has today

From the Registry survey:

- **References**: `Importers.Drawing` and `Exporters.Drawing` 4.4.0 (`Services\Services.csproj:15-16`), and with them `Model.Layouts` and `Model.Geometry`. It also references Module Draw's `Tellurian.Trains.Drawing.Components` 0.10.0 (`App\App.csproj:24`), which itself pins Schedules 4.4.4 through `Drawing.Model`: **two versions of the same packages in one app**.
- **Storage**: drawings are stored as uploaded JSON in `Document`. `Module.ModuleDrawingId` holds the `.module.json`, `ModuleEndProfile.DrawingId` the `.endprofile.json`, and `Locality.HallDrawingId` the `.hall.json`.
- **Display** (static SSR):
  - `ModuleView` on the module page and a full-width drawing page;
  - `EndProfileView`, plus a sheet **with measures** built in the Registry itself (`App\Extensions\EndProfileDrawingSheet.cs`);
  - `HallView`.
- **Written into the file**: the Registry module id and the owners (`ModuleDrawings.WithRegistryData`); a hall's locality name, address and id.
- **Read from the file**: an end profile's width and height (`EndProfileAnalysis`), and its revision.
- **API**: `GET /api/catalogue` (scales, standards, end profiles with drawings, uuid-v5 ids from the Registry's ids) and `POST /api/endprofiles/{id}/drawing`.

### 4.2 The shared properties

What a drawing and a Registry row both hold, and which way they should flow. The file is the source of truth for geometry. The Registry owns ownership and registration (Module Draw `docs\OperationLocationsTasks.md` H5, `ModelHarmonisation.md` decision 6).

| Registry | Shared model | Direction | Note |
|---|---|---|---|
| `Module.FullName`, `ConfigurationLabel`, `PackageLabel`, `Note`, `RepresentsFromYear`/`UptoYear` | `Module.Name`, `ConfigurationLabel`, `PackageLabel`, `Note`, `RepresentsFromYear`/`UptoYear` | Both | |
| `FremoNumber`, owner's prefix | `Prefix`, `Number`, `HasFremoDesignation` | Registry → file | The two disagree on the format (`CrossAppReview.md` 3) |
| Owners | `ModuleOwners` | Registry → file | Built |
| `StandardId`, `ScaleId` | `Module.Standard` (a `Guid` from `CatalogueIds`) | Both, by id | |
| `SpeedLimit` | `Module.SpeedLimit` | Both | **The planner's speed comes from here** |
| `NumberOfThroughTracks` | `Module.NumberOfThroughTracks` | Drawing → Registry when drawn | Also derivable from the end profiles |
| `HasNormalGauge`, `HasNarrowGauge` | Replaced by the gauges of the end-profile tracks (`EndProfileTrack.Gauges`, in mm, named by the standard's `Gauges`), as in Module Draw, where an end profile states both scale and gauge. `Module.HasNormalGauge`/`HasNarrowGauge` leave the shared model | Drawing → Registry, derived | **Decided 2026-11-28.** A module's gauges are read from its drawing (its exits' end profiles and its track elements) and need not be stored in the Registry at all |
| `Is2R`, `Is3R` | `EndProfileTrack.RailSystem` (`TwoRail`, `ThreeRail`) on the exits' end profiles; `ModuleStandard.RailSystem` | Drawing → Registry | **Decided 2026-11-28.** They stay in the Registry, which sets them from every module drawing uploaded to it (owner, 2026-11-28): `Is2R` when any exit's end-profile track is `TwoRail`, `Is3R` when any is `ThreeRail`. `RailSystem` is not nullable and defaults to `TwoRail`, so a drawing always gives an answer; a three-rail module is only right when its end profiles say so |
| `IsEndProfileSwitcher`, `IsStandAlone`, `FunctionalState`, `LandscapeState` | The same names on `Module` | Registry → file | Registry facts that Module Draw only shows |
| `IsDuckunder`, `IsTurntable`, `IsSignalModule`, `IsEndOfLine`; `Station.HasTurntable`; signals at a location | `Module` flags, `Station.HasTurntable`, the drawing | Drawing → Registry | **Decided 2026-11-28:** set from the drawing, either entered by the user in Module Draw or worked out from the track paths (4.2.1) |
| `Radius`, `Angle`, `Straight`, `Length`, `Width`, `NumberOfSections` | `ModuleMeasurement.Derive(drawing, endProfiles)` → `ModuleMeasures` | Drawing → Registry | Not done today. `ModuleFileContent.Measures` is already written for receivers |
| `ModuleExit.Label`, `EndProfileId`, `Direction` (East/West) | `ModuleExit.Label`, `EndProfile`, `Direction` (`ExitSide`) | Drawing → Registry | Convert the direction by name, never by number (`CrossAppReview.md` recommendation 11) |
| `Station.FullName`, `Signature`, `IsShadow`, `IsTerminus`, `IsHarbour`, `IsKeyRequired`, `HasTurntable`, `LockKeyName`; `IsJunction`, `IsIndustrialArea` | `OperationLocation.Name`, `Signature`, kind (`Station`/`IsShadow`, `SignalControlledLocation`, `IndustrialArea`, `OtherLocation`), `IsTerminus`, `IsHarbour`, `IsKeyRequired`, `HasTurntable`, `LockKey` | Both | The kind against flags is the hard part. `IsManned` is decided per meeting, not per module |
| `StationTrack.Designation`, `DisplayOrder`, `DirectionId`, `MaxTrainLength`, `PlatformLength`, `SpeedLimit`, `IsSiding`, `IsLockable`, `UsageNote` | `StationTrack.Designation`, `DisplayOrder`, `Direction`, `Length` (measured, `StationTrackMeasurement`), `PlatformLength`, `SpeedLimit`, `IsMain`, `IsStub`, `IsLockable`, `Usage` | Drawing → Registry for lengths and stub; both for the rest | Enum numbers differ (`CrossAppReview.md` 1.10) |
| `ModuleEndProfile.WidthMillimeter`, height | `EndProfileAnalysis` | Drawing → Registry | Built |
| `Locality` name and address | `Hall` external reference | Registry → file | Built |

### 4.2.1 Features set from the drawing (owner, 2026-11-28)

These go from the drawing to the Registry, like the measures. Some are entered by the user in Module Draw, others are worked out from the drawing. What the drawing can give today:

| Registry | Worked out from the drawing | Entered by the user in Module Draw | Today |
|---|---|---|---|
| `Module.IsDuckunder` | Yes: the drawing has duck-under areas (`ModuleDrawing.DuckUnderAreas`, MD-19) | The areas themselves are marked by the user | Derivable now. `Module.IsDuckunder` is then a derived value, not stored |
| `Module.IsEndOfLine` | Yes: one exit (`ModuleDrawing.Kind == EndOfLine`) | — | Derivable now |
| `Module.IsTurntable` (the module is a turntable) | Only with a turntable track element, which `Tracks/` lacks (straight, curved, flex, turnout, double slip, crossing, buffer stop) | Until then, a flag on the module (`Module.IsTurntable` exists) | Entered |
| `Station.HasTurntable` | Yes, once there is a turntable track element: a location whose module has one | Until then, the flag on the location (exists) | Entered |
| `Module.IsSignalModule` | Only with signals on the track paths: a signal placed at a point along a track element, with its direction. Nothing in the drawing models signals today | Until then, a flag on the module (exists) | Entered |
| Signals at a location (`HasSignals`) | As above: a location whose module has a signal. Always true for a `SignalControlledLocation`, by design | Until then, a flag on a `Station` or `IndustrialArea` | **Exists nowhere yet**, neither in the model nor in the Registry. The Registry has `Module.SignalFeature` (No, Optional, Fixed) beside `IsSignalModule` |

So the model has to give:
- derived members for what follows from the drawing (`drawing.HasDuckUnder`, `drawing.IsEndOfLine`, later `HasTurntable` and `HasSignals`), so the Registry and Module Draw work them out the same way;
- the flags the user enters, until the drawing can say it;
- if signals and turntables are to be worked out, two drawing additions: a turntable track element and a signal on a track path.

Once a flag can be worked out, it stops being entered, and a stored value that disagrees is reported, not kept.

**Only an operation location's `HasTurntable` and `HasSignals` are used by the planner** (owner, 2026-11-28). They belong to `Model.Layouts` as stored values on the location. `hallPlan.ToLayout()` fills them in, worked out or copied, so that the planner gets them without the drawing (3.3). The module features (`IsDuckunder`, `IsTurntable`, `IsSignalModule`, `IsEndOfLine`) are shared by Module Draw and the Registry only, and stay with the module in `Model.Drawings`. In `Model.Drawings` the values are derived from the drawing where it can tell them, so the Registry, Module Draw and the import all use the one rule.

### 4.3 What the Registry needs from the model

- **To read and write the files**: the three file kinds (module, end profile, hall) and the catalogue, through `Importers.Drawing` and `Exporters.Drawing`. It already does this.
- **To display them**: the drawn types in full, read-only:
  - `ModuleDrawing` with exits, sides, sections, track elements, labels and areas;
  - `EndProfile` with `PlateShape`;
  - `Hall`;
  - `PathDrawing` and the geometry under them.

  The rendering itself is Module Draw's components package. That is UI and rightly stays in Module Draw's repository.
- **To show measures**: `ModuleMeasurement`/`ModuleMeasures` for a module and `EndProfileAnalysis` for an end profile; both are in the model already. What is missing is a **dimension layer in `ModuleView`**, which is Module Draw's to build. `DimensionLine` exists but only the editors use it. The Registry's own end-profile sheet would move there too, so that the two apps don't draw dimensions twice.
- **To copy shared properties**: `Module`, `OperationLocation` and its kinds, `StationTrack`, `ModuleExit`, `ModuleStandard` by id, and `StationTrackMeasurement` for track lengths, which needs the standard's fouling distance and the end profiles.
  - The mapping code to the Registry's entities belongs in the Registry, which keeps its own model (`CrossAppReview.md` section 0).
  - **Which** properties are shared should be stated once, as a contract. **Decided 2026-11-28: a typed contract in `Model.Drawings`**, in its root namespace (no sub-namespace). It is a type holding exactly the shared properties, e.g. `ModuleRegistryData`, with `ModuleRegistryData.From(moduleFile)` to read and derive them from a drawing and `moduleFile.WithRegistryData(data)` to write the Registry's values back. Both apps use it. The Registry maps its entities to it, Module Draw compares with it, and it replaces Module Draw task F3's list in `schemas\README.md`.

The Registry does **not** need: auto-draw (the Registry will not auto-draw; question 3), `Lines`, `TrackGraph`, hall plans, `LayoutComparison`, the planner's settings, or the timetable decisions on a location (`Timings`, `StaffPosts`, `StartLabels`, `ShiftCount`, `HideMeets` …).

## 5. Proposed shape

```
Model.Geometry         unchanged: pure 2D geometry (SpatialIndex could go to Module Draw, see below)
Model.Files            the envelope and migration mechanism (FileEnvelope, FileKind, FileMigration, FileError), today in Model.Layouts
    ↓
Model.Layouts          the operating layout — what the planner and Dispatch take
    ↓                      Layout (locations, tracks, track stretches with measured results, module summaries, light lines,
    ↓                      SourceId/Revision, scale, standard by name), OperationLocation hierarchy, StationTrack,
    ↓                      TrackStretch, ExitSide, Country, Region, ExternalReferences, LayoutComparison/LayoutUpdate
    ↓                      No geometry. No dependency on Model.Geometry.
Model.Drawings  (new)  the drawn layout — what Module Draw makes and the Registry shows
                           Module (with ModuleDrawing), ModuleExit, sides, sections, TrackElement, TrackNetwork,
                           EndProfile, PlateShape, ModuleStandard (full), Catalogue, Hall, HallPlan (= hall + anchors
                           + modules + lines + joints + end profiles + standards + overrides + fit tolerances),
                           Measures, StationTrackMeasurement, validation that a reader needs, drawing file kinds,
                           DrawingJsonContext; LineWalker/TrackGraph and one entry point:
                           hallPlan.ToLayout() → Layout with stretches worked out
Importers.Drawing      reads the drawing files; the planner's hall-plan importer becomes hallPlan.ToLayout() here
Exporters.Drawing      unchanged

Back to Module Draw    AutoDraw (generator, SideBuilder, ModuleRedraw, ModuleTemplate), SpatialIndex, and other
                       editor helpers (decided 2026-11-28: the Registry does not auto-draw)
```

What this changes:

1. **The hall plan carries the drawing; the layout carries the result.** `Layout.Modules`, `Lines`, `Joints`, `EndProfiles`, `Standards`, `Overrides` and `FitTolerances` move to `HallPlan`. This reverses part of decision 5 in `CrossAppReview.md` ("the layout holds the modules with their drawings … and travels into the plan file and the database"). The open question there ("whether the plan file carries only the layout … or the hall plan too") is answered: **only the layout**, with measured results (owner's decision 2026-11-28).
2. **The stretches are worked out by the drawing package and applied by the planner.**
   - `hallPlan.ToLayout()` (in `Model.Drawings`) gives a `Layout` whose stretches carry the measured distance, speed, track count, sides and module ids.
   - The planner's present rules then apply unchanged: `IsDistanceEntered`, `IsSpeedEntered`, `IsTimeEntered`, `RunningMinutes`, `LayoutComparison`.

   This meets the requirement that the planner works out the stretches: it is the planner's import that calls it. `Model` and the plan file never see a drawing.
3. **`Module` splits in two.** The summary that a layout keeps stays in `Model.Layouts`, without the module features (`IsDuckunder`, `IsTurntable`, `IsSignalModule`, `IsEndOfLine`), which the planner doesn't need; the module with its drawing, add-ons, revision log and Registry states goes to `Model.Drawings`.
   - Today the one class decides by an `internal IsPlaced` flag how it is written, and nests or refers to its location (`Module.cs:44-127`). That trick goes with the split.
4. **`Model.Geometry` is no longer a dependency of the planner's model** (`Model` → `Model.Layouts` only). Only the planner's importer brings it in.
5. **What the Registry references stays about the same** (`Importers.Drawing` and `Exporters.Drawing`, now with `Model.Drawings`). It gets a smaller public surface: no auto-draw, no planner settings.

### 5.1 Planner data inside `Model.Layouts` (a second finding)

The cleanup asked for also touches the opposite direction: `Model.Layouts` exposes planner-only data to Module Draw and the Registry.

- `LayoutSettings` (general, graphic timetable, time and speed, validation, identity, integration with the Registry API key: `CrossAppReview.md` 1.9);
- `TimetableStretch` (made by hand or suggested, only in the planner; owner, 2026-11-28), `DispatchStretch`, `TopologyPosition`, `Company`;
- `StartLabelPrinting`;
- on a location: `Timings`, `StaffPosts`, `ShiftCount`, `HideMeets`, `HidePassings`, `SplitDispatchByDirection`, `IsPassengerInterchange`, `Instructions`, `PhoneNumber`;
- `StationTrack.PreviousLocationId`/`NextLocationId`/`AppliesInBothDirections`.

Module Draw writes them all, with default values, into every hall plan's layout block.

**Decision 13 of `ModelHarmonisation.md` put them there deliberately**: one level, "the other apps ignore what they don't use". Moving them to `Model` (the Timetables layer) would make `Model.Layouts` what Module Draw and the Registry share. It costs a plan-file migration and goes against that decision.

**Decided 2026-11-28: they leave `Model.Layouts` for `Model`** (question 2). This narrows decision 13 of `ModelHarmonisation.md`: the planner's settings and its own data are no longer on the shared layout classes. `Model.Layouts` keeps what Module Draw, the Registry and the planner share; what only the planner uses is Timetables data. Which of the location's properties count as the planner's own (for instance `Instructions` and `IsManned`, which Module Draw can enter in a hall plan) is settled when the step is designed.

## 6. Questions for the owner

1. ~~Does the plan keep only the result?~~ **Decided 2026-11-28: the plan keeps only the result.** It holds a `Layout` with measured stretches, light lines and module summaries. There are no drawings in the plan file or the database. Geometry is queried from the hall plan, through `Model.Drawings`, by an app that has it.
2. ~~Should the planner's decisions and settings leave `Model.Layouts`?~~ **Decided 2026-11-28: yes**, for `Model` (section 5.1).
3. ~~Will the Registry auto-draw modules?~~ **Decided 2026-11-28: no.** Auto-draw goes back to Module Draw. Later, Module Draw is to download a module's data from the Registry and auto-draw it there, where adjustments are easily made, for instance choosing or correcting the end profiles. That takes a Registry endpoint that serves a module's data (measures, exits with end profiles, tracks, location), in the shape of the shared model; it is a later step. Module Draw's spec MD-20, which has the Registry auto-draw, is to be changed to this.
4. ~~Typed contract or documented list?~~ **Decided 2026-11-28: a typed contract in `Model.Drawings`, in its root namespace** (section 4.3).
5. ~~Gauges~~ **Decided 2026-11-28.** `HasNormalGauge`/`HasNarrowGauge` go, in the shared `Module` and in the Registry. Gauges are read from the drawing, from the end profiles, which state both scale and gauge, and need not be stored in the Registry. `Is2R`/`Is3R` stay in the Registry and are set from every module drawing uploaded to it, from the rail system of its end-profile tracks.
6. **Signals and turntables in the drawing: deferred** (owner, 2026-11-28). Maybe as future features; the basics must work first. Until then `IsTurntable`, `IsSignalModule`, `HasTurntable` and `HasSignals` are entered by the user. Kept for later: should Module Draw get a turntable track element and signals placed on track paths, so that `IsTurntable`, `HasTurntable`, `IsSignalModule` and `HasSignals` are worked out rather than entered (4.2.1)? `HasSignals` is a new property of an operation location (owner, 2026-11-28); how does it relate to the Registry's `SignalFeature` (No, Optional, Fixed)?
7. ~~The Registry has two versions of the Schedules packages~~ **Decided 2026-11-28.** The decisions of this document give the Schedules packages a new major version, and the apps that depend on them adapt to it. Every app uses the latest version of the NuGet packages, so that everything stays in step; the Registry then gets the same version directly and through Module Draw's components.

## 7. Order of work

1. Store the measured distance and speed on `TrackStretch` and light lines on `Layout`, and switch the two planner editors to them. Move the suggesting of timetable stretches into the planner's `Model`, working on the light lines. The planner then no longer needs drawings after the import.
2. Add `hallPlan.ToLayout()`, then move `Modules`, `Lines`, `Joints`, `EndProfiles`, `Standards`, `Overrides` and `FitTolerances` from `Layout` to `HallPlan`. Add a plan-file migration that drops them, and remove their database columns.
3. Move the planner's settings and its own data from `Model.Layouts` to `Model` (5.1), with a plan-file migration.
4. Split out `Model.Files`, then `Model.Drawings`, and split `Module`, removing `HasNormalGauge`/`HasNarrowGauge` (gauges and rail systems are read from the end profiles). This is a breaking change for Module Draw and the Registry, so it gets a new major version, released once with steps 1–4. Remove the drawing converters from `PlanJson`.
5. Module Draw, the Registry and the planner move to that version; every app uses the latest version of the packages.
6. Module Draw takes back auto-draw and its editor helpers, and adds the dimension layer to `ModuleView`. Later: auto-draw from a module's data downloaded from the Registry.
7. The Registry copies the shared properties of section 4.2 on upload and download: on upload it sets `Is2R`/`Is3R` and reads the gauges from the drawing, then drops its `HasNormalGauge`/`HasNarrowGauge` columns. It also shows the module with measures.
