# Survey: what the Layouts layer needs for Module Draw to work with it alone

Written 2026-11-20. Read-only: nothing was changed in either repository. A small throw-away console project was built outside the repositories to check what source-generated JSON can do (section 4.4); nothing else was built or run.

Paths are relative to `C:\Utveckling\Github\Tellurian.Trains\`, shortened as in `CrossAppReview.md`:

| Prefix | Repository |
|---|---|
| `S\` | `Tellurian.Trains.Schedules\` (the contract is `S\Model.Layouts\`) |
| `D\` | `Tellurian.Trains.Drawing\` (Module Draw); `D\Model\` is short for `D\Tellurian.Trains.Drawing.Model\`, and likewise `D\Geometry\`, `D\Editing\`, `D\App\` |

Module Draw is described as its **working copy**: branch `feature/schema-v2`, 219 changed files on top of release 0.9.0. Two facts about that working copy matter throughout:

- **Schema version 2 is unreleased.** The released app (0.9.0, `HEAD`) writes version 1 for every file kind; the working copy writes version 2 (`D\Model\Files\DrawingFileFormat.cs:15-23`). Every change of shape proposed here can therefore still go into version 2, with one migration from version 1.
- **The licence clash is gone.** The working copy is GPL-3.0-only (`D\Directory.Build.props:13`); `HEAD` is MIT. Code can move between the repositories.

Anything inferred and not checked in code is marked *(unverified)*.

## 0. Summary

1. **Module Draw's model is already half on the contract.** Its `Module` derives from the contract's `Module` and its location and tracks are the contract's types. What is still its own is the geometry (track elements, exits, sides, end profiles, halls), the hall plan, the catalogue, the file envelope with its migrations, and the 2D geometry library everything rests on.
2. **About 7,000 lines move, in three blocks**: `D\Geometry` (pure 2D mathematics, about 1,500 lines), `D\Model` without its rendering helpers (about 5,000 lines), and the file code (envelope, migration 1→2, schema generation). What stays in Module Draw is `Editing`, `Components`, `App` and three rendering helpers of `Model`.
3. **The contract's own statement must be turned round.** `Layout` says its module list exists "so that no reader needs geometry" (`S\Model.Layouts\Layout.cs:7-8`). After decision 1 of section 0 the layout *has* the geometry, and stretches are measured from it.
4. **Recommended shape: a hall plan is a `Layout` plus where it stands in a hall.** The layout holds the modules with their drawings, their order (lines), the joints, the locations and the stretches; it travels into the plan file and the database. The hall plan adds the hall, the anchors, the public areas, the walking paths and the acknowledgements. Module Draw's own type named `Layout` (a connected group of lines) goes, and with it the name clash.
5. **Three things the model lacks entirely**: a speed limit on the module, a track graph that crosses module joints, and any code that computes a module's place in a hall plan (planned for Module Draw's phase 9, not built: `D\docs\ImplementationPlan.md:118`). All three are best built once, here.
6. **Reference-by-id can be written with attributes alone.** Checked by building: a `[JsonIgnore]` on the object and an id property beside it whose getter reads the object's id and whose setter keeps what was read (`get => ControlledBy?.Id ?? field; set => field = value;`). No run-time added properties, no pending table, no reflection; zero trim or AOT warnings.
7. **The one thing attributes can't express is "leave out a value equal to the initial value".** Two ways through are given in 4.4; it is a decision for the owner.
8. **Identity needs two rules, not one.** `int` ids local to a layout stay as decided. What exists on its own outside a layout (a module drawing, an end profile, a hall, a hall plan, a standard) keeps the GUID it has in every file since version 1. The Registry's ids are external references.

## 1. What Module Draw has

"Data" is written to files. "Authoring state" is written to files too, but says how the drawing was made, not what the module is. "Editor/UI" is never in a drawing file.

### 1.1 Geometry primitives (`D\Geometry\`)

Pure mathematics, no domain knowledge, about 1,500 lines. Units: millimetres and degrees, counter-clockwise from the positive X axis (`D\Geometry\Tolerance.cs`, `Angle.cs`).

| Type | What it is |
|---|---|
| `Point(X, Y)`, `Vector`, `Angle(Degrees)`, `Pose(Position, Heading)` | Value types (`Point.cs`, `Angle.cs`, `Pose.cs`) |
| `Segment` → `LineSegment`, `ArcSegment` | A piece of a path with `Length`, `PoseAt(distance)`, `Between`, `Offset`, `ClosestDistance`; an arc has `Center`, `Radius`, `SweepDegrees` (`Segment.cs`, `ArcSegment.cs:20-23`) |
| `SegmentPath` | Connected segments: `Length`, `PoseAt`, `Between(from, to)`, `Project(point)`, `Reversed`, `Transformed`, `Offset`, `Tessellate` (`SegmentPath.cs:30,44,59,80`) |
| `Transform`, `Join` | Rigid placement. `Join.PlacementFor(target, localExit)` puts one module's exit against another's; `Join.Gap` is the closure gap of a loop (`Join.cs`) |
| `Mirror`, `ShapeComparison`, `PointMatching`, `PolygonOffset`, `AreaMeasure`, `BoundingBox`, `SpatialIndex`, `Polyline`, `PathBuilder` | Comparison of end profiles, areas, clearances |

All of it is needed by the model that moves (routes of track elements, outlines, fit evaluation), so it moves with it. `SpatialIndex` and `PolygonOffset` are used for hall-plan clearance, which is validation and moves too.

### 1.2 Paths as stored (`D\Model\`)

- `PathDrawing { Start, Steps, IsClosed }` with `PathStep` → `LineStep(End)`, `ArcStep(Center, Sweep)`, discriminator `type` with values `line`, `arc` (`PathDrawing.cs:10-14`, `PathStep.cs:7-9`). Data. Used for every outline, side, ground line and flex track.
- `Circle(Center, Diameter)` (`Circle.cs`), `DocumentReference { Title, Url, Revision }` (`DocumentReference.cs`). Data.
- `ExtensibleRecord`: the base of every stored record, keeping unknown JSON properties (`ExtensibleRecord.cs:11-19`). The same job as the contract's `LayoutPart`.
- `Dimension`, `DimensionKind`: not part of any stored type *(inferred: not referenced from `EndProfile`, `ModuleDrawing`, `Hall` or `HallPlan`)*. Editor/UI.
- `PathCorners`: a path as corners, for corner-by-corner editing (`PathCorners.cs`). Editor, though `Obstacle.TryGetRectangle` uses it (`Halls\Obstacle.cs:40`).

### 1.3 End profiles (`D\Model\EndProfiles\`)

| Type | Properties (mm unless stated) | Kind |
|---|---|---|
| `EndProfile` (`EndProfile.cs:12-64`) | `Id` (GUID), `Designation`, `ShortName`, `Scale` (short name, a string), `Standards` (references by GUID and name), `Outline` (closed path), `Plate`, `GroundLine` (open path), `Tracks`, `ScrewHoles`, `Dowels`, `ScrewHolePattern`, `Source`, `ExternalReferences`, `Revision`, `RevisionLog` | Data |
| `PlateShape` (`PlateShape.cs`) | `Start`, `Segments`, `Bottom`: the plate as entered; outline and ground line are generated from it and still stored | Data |
| `PlateSegment` (`PlateSegment.cs:12-24`) | `Kind` (Line, Arc, Fillet), `End`, `Center`, `Radius`, `Side` | Data |
| `EndProfileTrack` (`EndProfileTrack.cs:11-14`) | `Offset` (lateral, positive right, seen from outside), `Gauge`, `RailSystem`, `Label` | Data |
| `ProfileOrientation` | Normal, Mirrored: how the plate is fitted at an exit | Data (on the exit) |
| `FitTolerances` (`FitTolerances.cs:12-21`) | `Joint` 0.5, `Landscape` 2.0, `MinimumBoltedScrewHoles` 3 | Data (in hall plan settings) |
| `JointFit`, `FitDifferences`, `JointFitResult`, `JoinMethod` | The result of comparing two ends | Derived, never stored (except `JoinMethod` as an override on a joint) |
| `JointFitEvaluator`, `EndProfileComparison`, `EndProfileAnalysis`, `EndProfileProperties` | The rules | Logic |
| `EndProfileTemplate`, `EndProfileTemplateParameters` | Makes a plain profile from a few numbers | Logic |
| `TrackCrossSection` | How a rail and sleeper are drawn in end view (`TrackCrossSection.cs`) | Rendering constants |

Coordinates: end view seen from outside the module, X to the right, Y up, origin on the centre line at rail top (`EndProfile.cs:8-9`).

### 1.4 Modules (`D\Model\Modules\`)

`Module : Shared.Module` (`Module.cs:24`). From the contract: name, prefix, number, FREMO mark, configuration and package label, owners, standard, operation location, tracks. Added by Module Draw:

| Property | Note |
|---|---|
| `Id` (GUID, required, written first) | `Module.cs:27-28` |
| `StandardId` (GUID in Module Draw's catalogue) | `:34`; beside the contract's `Standard` by short name, nothing keeps the two in step |
| `AddOns` (references by GUID and name) | `:37` |
| `Theme` (a string) | `:39`; the contract's `Theme` is an enum |
| `RepresentsFromYear`, `RepresentsUptoYear`, `NumberOfThroughTracks` | `:40-44` |
| `HasNormalGauge`, `HasNarrowGauge`, `IsDuckunder`, `IsTurntable`, `IsEndOfLine`, `IsEndProfileSwitcher`, `IsSignalModule`, `IsStandAlone` | `:46-57` |
| `FunctionalState`, `LandscapeState` | `:60,63`; the Registry's numeric codes, as `int?` |
| `Note`, `Revision`, `RevisionLog`, `ExternalReferences` | `:65-74`; `Revision` is documented as incremented on every save and never is (`CrossAppReview.md` section 5) |
| `Drawing` (required, written last) | `:78` |

**There is no speed limit on the module.** A search for "speed" in `D\Model` and `D\Editing` finds nothing; it is listed as a candidate property in `D\docs\OperationLocationsTasks.md:88` (task D6, not done). The contract has one on a track (`S\Model.Layouts\StationTrack.cs:93`) and a speed on a stretch (`TrackStretch.cs:89`).

`ModuleDrawing` (`ModuleDrawing.cs:12-63`), in module coordinates: origin at the first exit's end-plate centre, X into the module.

| Member | What it is | Kind |
|---|---|---|
| `Exits` → `ModuleExit { Label, Direction, EndProfile (reference), Orientation, RailTopOffset, Pose, IsPositionProvisional }` (`ModuleExit.cs:14-36`) | The pose is the end-plate centre, heading out. `RailTopOffset` is the height against the standard's rail top, 0 except on ramps. `IsPositionProvisional` is authoring state | Data |
| `MainPath` | The centre line from the first exit to the second (`:17`) | Data |
| `Sides` → `ModuleSide { FromExit, ToExit (exit labels), Mode (Auto, Custom), Path }` | The outline between end plates (`ModuleSide.cs:10-15`); `Mode` is authoring state | Data |
| `Sections` → `Section { Label, Outline }`; `InternalJoints` → `InternalJoint { FirstSection, SecondSection, Start, End, Bends, EndProfile }` | Baseboard parts and the joints between them | Data |
| `TrackElements` | See 1.5 | Data |
| `TracksWithEnteredLength`, `TracksWithEnteredStub` (lists of track ids) | Which track values the user typed, so they aren't measured again (`:35,41`) | Authoring state |
| `OperatorSpaces`, `PublicSides`, `DuckUnderAreas` (with `SideAttachment { SideIndex, From, To }`) | Zones beside the module | Data |
| `Labels` → `ModuleLabel { Kind, Text, Pose, Length }` | Texts and markers on the drawing | Data (drawing annotation) |
| `RegistryMeasures` | The Registry's numbers, kept for comparison | Data |
| `AutoDraw` → `AutoDrawRecord { GeneratorVersion, Parameters }` | Present while the drawing is as generated | Authoring state |
| `Kind` (from the number of exits) | `[JsonIgnore]` | Derived |

Exit side and reference: `ExitDirection` Undefined, Left, Right, Up, Down (`ExitDirection.cs:7-23`), the same five values as the contract's `ExitSide` (`S\Model.Layouts\ExitSide.cs:8-24`). `ExitPlace(Side, Number)` names an exit by its side and its position among the exits of that side (`ExitPlace.cs:11,24,32`).

Logic in the same folder: `ModuleGeometry` (end plates, track positions at an exit, the assembled outline: `ModuleGeometry.cs:19-42`), `ModuleSections` (cuts sections from the outline), `ExitDirections` (infers Up and Down), `ModuleValidation` and `ModuleIdentification` (name, prefix, number). Rendering helpers: `ModuleNameLabel` (where the name is written on the module) and `ProfileShading` (how an end profile is shaded in plan view).

Measures: `ModuleMeasures { Length, Straight, Radius, Angle, Width, TrackLength, NumberOfSections, RampHeightDifference, RampGradient }` in millimetres, degrees and per mille (`D\Model\Measures\ModuleMeasures.cs:12-38`), derived by `ModuleMeasurement.Derive` from the main path only (`ModuleMeasurement.cs:14-35`). They are written to the module file for receivers and derived again on load. `Radius` is null when the main path has curves of different radii (`:28`).

Auto-draw (`D\Model\AutoDraw\`, about 1,300 lines): draws a module from the Registry's numbers. It is "UI-independent, so the Registry can run it on its server" (`ModuleAutoDraw.cs`). Logic, with `AutoDrawParameters` stored in the file.

### 1.5 Track geometry (`D\Model\Tracks\`)

`TrackElement` (abstract), discriminator `type` in lower case (`TrackElement.cs:11-18`). Common: `Id` (GUID, `:21`), `StationTrackId` (the `int` id of a track of the module's list, `:27`), `IsNarrowGauge`, `IsGenerated` (authoring state). Each element gives `Routes()` (centre lines as `SegmentPath`) and `ConnectionPoints()` (poses heading out).

| Element | Stored | Routes |
|---|---|---|
| `StraightTrack` | `Start` (pose), `Length` | 1 |
| `CurvedTrack` | `Start`, `Radius`, `Angle` (degrees, positive left) | 1 |
| `FlexTrack` | `Path` (lines and arcs) | 1 |
| `Turnout` | `Start` (the points), `Kind` (Left, Right, Wye, ThreeWay, CurvedLeft, CurvedRight), `Length`, `Radius`, `Angle`, `InnerRadius`, `InnerAngle` | 2 or 3 |
| `Crossing` | `Center`, `Length`, `Angle` | 2 |
| `DoubleSlip` | `Center`, `Length`, `Angle` (slip radius derived) | 4 |
| `BufferStop` | `Pose` | 0 |

`TrackNetwork` (**internal**, `TrackNetwork.cs:14`): one `Line(Element, Path, TrackId)` per route; ends are joined when within 0.01 mm (`:17`); a route of a shared element belongs to the track that continues from it (`:88-100`). It answers `LinesOfTrack`, `LinesEndingAt`, `ReachableFrom` and `IsAtExit`. It stops at the module's edge: nothing joins the tracks of two modules.

`StationTrackMeasurement` (`StationTrackMeasurement.cs`): the usable length of a track between fouling points, sampled every 5 mm and refined to 0.01 mm (`:28,31`), and whether it is a stub. It writes metres, rounded to the millimetre, into the contract's `StationTrack.Length` (`:74,86`). The fouling distance comes from the standard, else from the track distance of the standard's default double-track end profile (`:40-48`).

`StationTrackList` gives ids as the lowest free number per module (`StationTrackList.cs:40-47`). `StationTrackValidation` has seven codes of its own (`StationTrackValidation.cs:7-29`).

State: only the through tracks that auto-draw makes exist in practice. Drawing turnouts and sidings is the rest of phase 7, not built (`D\docs\OperationLocationsTasks.md:14,19`). The element types and the network are written and tested.

### 1.6 Operation locations and tracks

The contract's types, used directly. Module Draw writes the tracks on the module (`module.tracks`) and nothing in `operationLocation.tracks`; the centre marker (task D3) and the viewing side (tasks B3–B5) are not built (`OperationLocationsTasks.md:61-63,85`). Module Draw validates the location with the contract's `OperationLocationRules` and the tracks with its own codes (`:40`).

### 1.7 Halls (`D\Model\Halls\`)

Hall coordinates: millimetres, X right, Y up.

| Type | Properties |
|---|---|
| `Hall` (`Hall.cs:13-35`) | `Id` (GUID), `Name`, `Address`, `Note`, `Outline` (closed path; wall *i* is step *i*), `MinimumAisleWidth`, `Obstacles`, `Doors`, `NoGoZones`, `Revision`, `RevisionLog`, `ExternalReferences`. Derived: `FloorArea`, `EffectiveFloorArea` |
| `Door` (`Door.cs:14-29`) | `Id` (GUID), `Kind` (Door, EmergencyExit), `Position` (pose on the wall, heading in), `Width`, `KeepFreeDepth` (1000), `Label`, `IsModuleEntrance` |
| `Obstacle` (`Obstacle.cs:8-14`) | `Id` (GUID), `Kind` (Pillar, Furniture), `Area` (closed path), `Label` |
| `NoGoZone` (`NoGoZone.cs:6-11`) | `Id` (GUID), `Area`, `Reason` |

The hall editor is code complete (`D\docs\ImplementationPlan.md:243`).

### 1.8 Hall plans (`D\Model\HallPlans\`)

Data types and file format exist; **no code computes anything from them**. The engine that turns lines into poses is phase 9 (`ImplementationPlan.md:118`); there is no hall-plan editor.

| Type | Properties |
|---|---|
| `HallPlan` (`HallPlan.cs:17-64`) | `Id` (GUID), `Name`, `Note`, `Hall` (embedded copy), `Scale` (string), `PrimaryStandard`, `AcceptedStandards`, `PreferredAddOns` (references), `Settings`, `Layouts`, `Joints`, `PublicAreas`, `WalkingPaths`, `Acknowledgements`, `Modules` (embedded copies), `EndProfiles`, `Standards`, `Overrides`, `Revision`, `RevisionLog`, `ExternalReferences` |
| `Layout` (`Layout.cs:11-27`) | `Id` (GUID), `Name`, `AnchorModuleId`, `AnchorPose` (hall coordinates), `IsAnchorLocked`, `AnchorDimensions`, `Lines`. A connected group of modules; a plan can have several |
| `Line` (`Line.cs:11-21`) | `Id` (GUID), `Name`, `Entries`, `StartAt`, `EndAt` (exit references; a branch starts at one, a loop or rejoining branch ends at one) |
| `LineEntry` (`LineEntry.cs:16-30`) | `ModuleId` (GUID), `IsTurned`, `ContinuesFrom` (`ExitPlace`, for three or more exits), `LegAdjustment` (mm), `Note` |
| `ExitReference(ModuleId, Side, Number)` | `ExitReference.cs:13` |
| `Joint` (`Joint.cs:13-22`) | `First`, `Second` (exit references), `JoinMethodOverride`, `NumberOfClamps` (2), `Note`. Only the planner's choices; the fit is evaluated |
| `HallPlanSettings` (`HallPlanSettings.cs:11-32`) | `Fit`, `JointGap` 2 mm, `JointAngle` 0.5°, `Height` 1 mm, `MinimumClearance`, `IsClearanceChecked`, `WalkingPathWidth` 600, `RotationStep` 15° (the last is UI) |
| `AnchorDimensions`, `AnchorReferencePoint`, `HallElementReference` | How the people setting up find the anchor: a point on the anchor module, two walls or obstacles, an angle to a wall |
| `PublicArea`, `WalkingPath` | Areas and paths in hall coordinates, each with a GUID |
| `Acknowledgement` (`Acknowledgement.cs:10-22`) | `Id`, `FindingCode`, `ModuleIds`, `Fingerprint`, `Note`, `Time`: a warning the planner accepted |

Module poses are not stored: they follow from the anchor pose and the order of the lines (`HallPlan.cs:10-11`). A module is placed at most once per plan (`:76`).

### 1.9 Catalogue (`D\Model\Catalogue\`)

| Type | Properties | Identity |
|---|---|---|
| `DrawingCatalogue` (`DrawingCatalogue.cs:15-24`) | `Version`, `Scales`, `Standards`, `AddOns`, `EndProfiles`, `Overrides`, `ExternalReferences` | None |
| `Scale` (`Scale.cs:6-7`) | `ShortName`, `Denominator` | Short name |
| `ModuleStandard` (`ModuleStandard.cs:15-69`) | `Id`, `ShortName`, `Scale` (string), `TrackSystem`, `RailSystem`, `NormalGauge`, `NarrowGauge`, `Couplings`, `Electricity`, `Wheelset`, `RailTopHeight` (1300), `MinimumClearance`, `OperatorSpaceDepth` (800), `TrackColor`, `FoulingDistance`, `MinimumReverseCurveRadius`, `DefaultCurveRadius`, `DefaultSingleTrackEndProfileId`, `DefaultDoubleTrackEndProfileId`, `Source`, `ExternalReferences`, `Countries` (ISO codes) | GUID |
| `StandardAddOn` (`StandardAddOn.cs:8-12`) | `Id`, `ShortName`, `BaseStandardId`, `Description`, `Source` | GUID |
| `EndProfileCompatibilityOverride` (`EndProfileCompatibilityOverride.cs:11-14`) | `FirstEndProfileId`, `SecondEndProfileId`, `Fit`, `Note` | The unordered pair |
| `CatalogueReference(Id, Name)` (`CatalogueReference.cs:7`) | How a drawing refers to a standard, add-on or end profile: GUID and name | |
| `RailSystem` | TwoRail, ThreeRail | |

A built-in catalogue is an embedded file, version `0.3-provisional`: scales H0 and N; standards FREMO/H0-RE, MMM-2R, MMM-3R, FREMO/N-RE; add-ons MMM-Summer, MMM-Modern; end profiles with drawings (`BuiltInCatalogue.cs:17`, `BuiltInCatalogue.catalogue.json:7`). The user's own entries are kept apart and layered over it (`DrawingCatalogue.Merge`, `:106-119`); `ForCountry` and `ForScale` filter what is offered. `ModuleStandard.ToShared()` maps to the contract's standard (`ModuleStandard.cs:79`). `EndProfileImport` matches imported profiles by id, else by designation and scale (`EndProfileImport.cs:49`). `CatalogueValidation` has twelve codes.

### 1.10 Files (`D\Model\Files\`)

- **Envelope**: `{ format: "tellurian-drawing", kind, schemaVersion, appVersion, content }` (`DrawingFile.cs:7-21`, `DrawingFileFormat.cs:9`). Kinds: EndProfile, Module, Hall, HallPlan, Catalogue (`DrawingFileKind.cs`). The version is per kind; all five are at 2 (`DrawingFileFormat.cs:15-23`). File names end `.endprofile.json`, `.module.json`, `.hall.json`, `.hallplan.json`, `.catalogue.json`; the older `.td…` extensions are still read (`:33-52`).
- **Content**: an end profile, hall, hall plan or catalogue is the content itself. A module file's content is `ModuleFileContent { Module, EndProfiles, Measures }` (`ModuleFileContent.cs:13-21`).
- **Migrations**: `DrawingFileMigration(Kind, FromVersion, Migrate)` on the JSON tree (`DrawingFileMigration.cs:9`); one chain, `Upgrade(kind, content, from, to, migrations)` (`DrawingFileMigrations.cs:26-35`). The only step is 1→2 (`SchemaVersion2.cs`, 373 lines). `UpgradeDraft` runs it again on every version 2 file, because version 2 changed shape while it was made (`:16-17`).
- **Reading**: checks format and kind, refuses a newer version, migrates, normalises the `kind` of locations with the contract's `OperationLocationJson.Normalise`, then deserialises (`DrawingFileSerializer.cs:80-112`).
- **Serialisation**: source-generated, `DrawingJsonContext` (`DrawingJsonContext.cs:17-39`): camelCase, nulls left out, enum values as strings, three converters for `Point`, `Angle` and `Pose` written flat. The contract's module, location and track are generated through it already, since `Module` derives from the contract's.
- **Around the file**: `DrawingIdentity` reads kind, id, name and revision without the content (`DrawingIdentity.cs:11`); `DrawingRevisionLog` records who saved and when (`DrawingRevisionLog.cs:17-27`); `ExternalReferences` names the Registry's entry and its keys `moduleId`, `endProfileId`, `standardId` (`ExternalReferences.cs:14-26`); `DrawingImport` reads many files from a ZIP or a folder; `DrawingFileSchema` generates the JSON Schemas in `D\schemas\` from the serialiser's metadata.

### 1.11 Settings and storage (`D\App\`)

All of this is the app's own and stays there.

- `UserSettings { UserName, CountryId (the Registry's id), Scale, ApiKey }`, kept in the browser, never in a drawing (`D\App\Settings\UserSettings.cs`).
- `SettingsFile`: an envelope of its own, `format: "tellurian-drawing-settings"`, `version: 1` (`SettingsFile.cs`).
- `Country { Id, EnglishName, DomainSuffix }` with an embedded `countries.json` (`Country.cs`, `countries.json`): a fifth copy of the country list.
- `Draft`, `RecentFile`, `EditorState`: workbench storage in IndexedDB, with a source-generated context of its own (`D\App\Storage\WorkbenchJsonContext.cs`).

## 2. What the Layouts layer needs

Conventions used below: **lengths of drawn geometry are millimetres, angles degrees; operational lengths are metres** (see 6.4 for why both). "Replaces" names the Module Draw type that goes.

### 2.1 Projects

| Project (in Schedules) | Holds | Replaces |
|---|---|---|
| `Model.Geometry` (new; no dependencies) | `Point`, `Vector`, `Angle`, `Pose`, `Segment`s, `SegmentPath`, `Transform`, `Join` and the comparison and area helpers, as they are | `D\Geometry` |
| `Model.Layouts` (references `Model.Geometry`) | Everything in 2.2–2.9, the queries of section 3, the files of section 4 | `D\Model` without `ModuleNameLabel`, `ProfileShading`, `TrackCrossSection`, `Dimension`, `PathCorners` |

The README's "It refers to nothing but the base library" (`S\Model.Layouts\README.md`) becomes "nothing outside this repository". A separate geometry project keeps the mathematics free of the model; putting it inside `Model.Layouts` would also do, and saves a package. Module Draw's `Editing` and `Components` use the geometry heavily and take it from the package.

`ExtensibleRecord` is replaced by `LayoutPart`. Module Draw's stored types are immutable records edited with `with`; the contract's are mutable classes copied with `Copy()` and `With()` (decision 12). **The moved types become mutable classes**, as `Module` already did; this is the largest mechanical change in Module Draw's editor code.

### 2.2 Identity

| Thing | Identity | Unique within | Stable |
|---|---|---|---|
| Module drawing, end profile, hall, hall plan, standard, add-on | `Guid Id`, as in every file since version 1 | Everywhere | Across saves; a copy gets a new one and loses its external references (as today, `EndProfile.cs:88`) |
| Scale | Short name, with the denominator where two share a name (contract equality, `S\Model.Layouts\Scale.cs:28`) | The catalogue | |
| Module placed in a layout | `LayoutModule.Id` (`int`) | The layout | Given once when the module is placed, kept in the hall plan file, never regenerated at save |
| Operation location in a layout | `OperationLocation.Id` (`int`) | The layout | Given when its module is placed; 0 in a module file |
| Track | `StationTrack.Id` (`int`) | The module in a module file; the layout in a layout (as the contract says today, `S\Model.Layouts\StationTrack.cs:41-45`) | See 6.1 for the renumbering |
| Track stretch, line | `int` | The layout | A stretch keeps its id while it joins the same two locations |
| Track element | `int` (new; a GUID today) | The module | So that a route can be named as module, element, route number |
| Exit | Side and number (`ExitPlace`), as decided in `OperationLocations.md` 4.5 | The module | Holds while the exits of a side keep their order; see 6.5 |
| Section, side | Label; index | The drawing | As today |
| Door, obstacle, no-go zone, public area, walking path, acknowledgement | `int` (new; GUIDs today) | The hall, or the hall plan | They are referred to only from inside the same document (`HallElementReference.ObstacleId`) |
| What another system calls a thing | `ExternalReferences`, one entry per system | | Typed keys, see 2.9 |

The rule to add to the README: **a document that exists on its own has a GUID; everything inside a document has an `int` that is unique within it.** It keeps decision 4 (`int` ids local to a layout) and explains the GUIDs the contract already has (`Layout.SourceId`, `LayoutModule.SourceId`).

### 2.3 End profiles

Add to `Model.Layouts` (namespace `…Model.Layouts.EndProfiles`), as mutable classes deriving from `LayoutPart`:

- `EndProfile`: as 1.3, with `Scale` as the contract's `Scale` (by short name) in place of a string, and `Standards` as references (2.9). Units: millimetres; the end-view coordinate system is stated on the type.
- `PlateShape`, `PlateSegment`, `PlateSegmentKind`, `FilletSide`, `EndProfileTrack`, `RailSystem`, `ProfileOrientation`, `Circle`, `DocumentReference`.
- `FitTolerances`, `JointFit`, `FitDifferences`, `JointFitResult`, `JoinMethod`, and the logic: `JointFitEvaluator`, `EndProfileComparison`, `EndProfileAnalysis`, `EndProfileTemplate`.

Stays in Module Draw: `TrackCrossSection` (how a rail is drawn).

`PlateShape` is read through a constructor with parameters that validates and throws (`PlateShape.cs`, the `[JsonConstructor]`). That is fine for source generation, but it means a bad plate fails the whole file; keep the behaviour, and report it as a file error with the property path.

### 2.4 Modules

**`Module` (existing, changes).** Module Draw's subclass goes; its properties move onto the contract's class:

| Add to `Module` | Type and unit | Note |
|---|---|---|
| `Id` | `Guid?` | The module drawing's identity; null for a module the planner made without a drawing. Written first. Replaces `LayoutModule.SourceId` (6.6) |
| `SpeedLimit` | `int?`, km/h of the prototype | **New.** Set by the owner, never worked out. Null: the module sets no limit. The same unit as `TrackStretch.Speed` and `StationTrack.SpeedLimit` |
| `AddOns` | List of references (2.9) | |
| `Theme` | See 6.7 | A string in Module Draw, an enum in the contract |
| `RepresentsFromYear`, `RepresentsUptoYear`, `NumberOfThroughTracks` | `int?` | |
| `HasNormalGauge` (initially true), `HasNarrowGauge`, `IsDuckunder`, `IsTurntable`, `IsEndOfLine`, `IsEndProfileSwitcher`, `IsSignalModule`, `IsStandAlone` | `bool` | |
| `FunctionalState`, `LandscapeState` | `int?` today | The Registry's codes. Either enums with explicit numbers taken from the Registry, or left as they are and named as Registry codes; see question 9 |
| `Note` | `string?` | |
| `Revision`, `RevisionLog` | `int`, list of `Revision { Name, Date }` | Incrementing belongs in the file code (4.3), so every app does it the same way |
| `ExternalReferences` | As on the location | Today on `LayoutModule` (`S\Model.Layouts\LayoutModule.cs:25`); it belongs to the module, which has it wherever it is |
| `Drawing` | `ModuleDrawing?` | Null for a module without a drawing. Written last |

`Standard` stays as it is, a `ModuleStandard` by short name, and becomes the only reference to the standard: `StandardId` goes, because the standard itself now carries its GUID (2.9).

**`ModuleDrawing` and its parts (new)**, as 1.4: `ModuleExit`, `ModuleSide`, `SideMode`, `Section`, `InternalJoint`, `OperatorSpace`, `PublicSide`, `DuckUnderArea`, `SideAttachment`, `ModuleLabel`, `LabelKind`, `RegistryMeasures`, `AutoDrawRecord`, `AutoDrawParameters`, `PathDrawing`, `PathStep`. Changes on the way in:

- `ModuleExit.Direction` is the contract's `ExitSide`; `ExitDirection` goes (6.5).
- `ModuleKind` stays derived.
- `ModuleSide.FromExit` and `ToExit`, and `InternalJoint.FirstSection` and `SecondSection`, refer by label. Labels are otherwise "text for the reader only" (`D\schemas\README.md`), so a renamed exit breaks its sides. Refer by `ExitPlace` and by section number instead, while version 2 is still open.
- `TracksWithEnteredLength` and `TracksWithEnteredStub` are two lists of ids beside the tracks. With the track a contract type they can be two flags on the track, `IsLengthEntered` and `IsStubEntered`, the same pattern as `TrackStretch.IsDistanceEntered` and `IsTimeEntered` (`S\Model.Layouts\TrackStretch.cs:83,98`). Module Draw moved them off the track only because the track was not its own to extend.

**Logic that moves with it**: `ModuleGeometry`, `ModuleSections`, `ExitDirections`, `ModuleValidation`, `ModuleMeasurement` and `ModuleMeasures`, `MeasureComparison`, and auto-draw (`ModuleAutoDraw`, `AutoDrawGenerator`, `ModuleRedraw`, `ModuleTemplate`), which the Registry is meant to call without the editor.

**`LayoutModule` (existing, changes).** Keeps `Id` and `Module`. Loses `SourceId` and `ExternalReferences` to the module (6.6). Gains nothing: where the module stands in a line is the line's (2.7).

### 2.5 Track geometry

Add (namespace `…Model.Layouts.Tracks`): `TrackElement` and its seven kinds, `TurnoutKind`, as 1.5, with `Id` as `int`. Units: millimetres and degrees, module coordinates.

- The discriminator stays `type` with the lower-case values, because version 1 files have them. It is the one place where a discriminator isn't `kind` with PascalCase values; changing it is possible in the 1→2 migration and would make the suite uniform (question 8).
- `IsGenerated` stays as authoring state.
- `TrackNetwork` becomes **public** and grows into the layout-wide track graph of section 3.
- `StationTrackMeasurement` (usable length, stub), `StationTrackList` (free id, free designation, the generated list) move as they are.
- `StationTrackValidation` merges with the contract's track rules (section 6.8).

### 2.6 Operation locations and tracks

Already the contract's. Changes:

- `StationTrack`: add `IsLengthEntered`, `IsStubEntered` (2.4) and `SourceId` (`int?`, the track's id in the module drawing it was copied from; 6.1).
- `OperationLocation.Centre` (`S\Model.Layouts\OperationLocation.cs:116`) stays metres along the main track from the left end. Section 3 needs it as a point on the track graph; say on the property that the main track is the module's main path.
- A module file must not write what is decided for a layout. Today it does (`isManned`, `shiftCount`, `instructions`, `timings`, `id: 0`; `CrossAppReview.md` section 3). With default values left out (4.4) they disappear from a module file without a second type.

### 2.7 The layout: lines, joints and stretches

**`Layout` (existing, changes).** Add what makes the physical layout complete:

| Add to `Layout` | What it is |
|---|---|
| `Lines` | `Line { Id (int), Name, Entries, StartAt, EndAt }`. The first line of a connected group is its main line; a branch has `StartAt` |
| `Line.Entries` | `LineEntry { ModuleId (int, the LayoutModule), IsTurned, ContinuesFrom (ExitPlace), LegAdjustment (mm), Note }` |
| `Joints` | `Joint { First, Second (ExitReference: LayoutModule id, side, number), JoinMethodOverride, NumberOfClamps, Note }` |
| `EndProfiles` | Every end profile the modules use, each once |
| `Standards` | The standards of the layout and of its modules, each once, with their full properties (rail-top height, fouling distance, clearance) |
| `Overrides` | The compatibility overrides that apply to its end profiles |
| `FitTolerances` | From `HallPlanSettings.Fit`; it decides whether two tracks join |

`Modules`, `OperationLocations`, `TrackStretches`, `SourceId`, `SourceRevision` and `PreferredStandard` are there already. The summary of `Layout` (`Layout.cs:7-8`) and of `HallPlanFile` (`HallPlanFile.cs:7-9`), which say no reader needs geometry, are rewritten.

**`TrackStretch` (existing, changes).** See section 3.4: `Distance` and `IsDistanceEntered` stay; add `IsSpeedEntered`. `StartSide`, `EndSide`, `TracksCount` and `ModuleIds` are filled from the lines.

**Replaces**: `D\Model\HallPlans\Line`, `LineEntry`, `ExitReference`, `Joint`, and the module, end-profile, standard and override lists of `HallPlan`.

### 2.8 Halls and hall plans

Add (namespace `…Model.Layouts.Halls`): `Hall`, `Door`, `DoorKind`, `Obstacle`, `ObstacleKind`, `NoGoZone`, as 1.7, ids as `int` inside the hall. Units: millimetres, hall coordinates.

**`HallPlan` (new): a layout and where it stands in a hall.**

| Property | What it is |
|---|---|
| `Id` (GUID), `Name`, `Note`, `Revision`, `RevisionLog`, `ExternalReferences` | The document. `Layout.SourceId` and `SourceRevision` are these |
| `Hall` | An embedded copy |
| `Layout` | The contract's `Layout` of 2.7: the product that the planner takes |
| `Anchors` | `Anchor { LineId (a main line), ModuleId, Pose (mm and degrees, hall coordinates), IsLocked, Dimensions }`, one per connected group |
| `AcceptedStandards`, `PreferredAddOns` | References; the primary standard is `Layout.PreferredStandard` |
| `Settings` | `JointGap`, `JointAngle`, `Height`, `MinimumClearance`, `IsClearanceChecked`, `WalkingPathWidth`. `RotationStep` is an editor setting and stays in Module Draw |
| `PublicAreas`, `WalkingPaths`, `Acknowledgements` | As 1.8, ids as `int` |

`AnchorDimensions`, `AnchorReferencePoint`, `ReferencePointKind`, `HallElementReference`, `HallElementKind` and `PlateSide` move as they are.

**Module Draw's `HallPlans.Layout` goes.** What it holds splits: the lines are the layout's, the anchor is the hall plan's. A connected group is then not a stored thing: it is a main line and the branches that start from it. This removes the second meaning of "layout" inside the suite, and the schema-name collision noted in `CrossAppReview.md` section 3.

**The engine that is not built yet goes here, not in Module Draw**: the pose of every module from the anchors and the lines (`Join.PlacementFor`), the closure gap of loops and rejoining lines (`Join.Gap`), the joint fits, and the clearances. Module Draw's phases 9 and 10 then become editor and rendering only.

Why not make the hall plan *be* the layout: the plan file and the database would carry a hall, aisles and acknowledgements that neither the planner nor Dispatch uses. Why not keep the hall plan's own lists and derive a layout from them, as the layout block was meant to: that is the mapping that decision 14 rules out, and the place where identities were lost (`CrossAppReview.md` finding 3).

### 2.9 Catalogue and references

See section 5 for the catalogue. Two small types serve the whole model:

- **`Reference { Id (Guid), Name }`**: how a module refers to an end profile or an add-on, and a hall plan to a standard, when the full entry isn't embedded. Replaces `CatalogueReference`. *(A better name is wanted; "reference" alone is vague.)*
- **`ExternalReference`**: the typed entry of another system. `CrossAppReview.md` recommendation 4 already asks for it; Module Draw has the names ready: section `externalReferences`, entry `moduleRegistry`, keys `moduleId`, `endProfileId`, `standardId` (`D\Model\Files\ExternalReferences.cs:14-26`), and the Registry may add `stationId`. Move those constants into the contract and give the Registry's entry typed accessors, while still keeping the whole entry as JSON so that what the Registry adds survives.

### 2.10 Validation

One pattern for all of it: a code per broken rule and the thing it concerns, as `LocationProblem` and `LocationProblemCode` do today (`S\Model.Layouts\LocationProblem.cs`, `LocationProblemCode.cs`). To move in, each with its codes:

| Area | From | Note |
|---|---|---|
| Module identification | `ModuleValidation`, `ModuleProblemCode` | Number 1–9999, a number needs a prefix, else a name |
| Tracks against the drawing | `StationTrackValidation` | Merged with the contract's track rules (6.8) |
| Catalogue entries | `CatalogueValidation`, `CatalogueProblemCode` | |
| Drawing integrity | Exceptions in `ModuleGeometry.Outline` (`ModuleGeometry.cs:53-54,73`) | A side that doesn't meet its end plate throws today; it should be a reported problem |
| Joints | `JointFitEvaluator` | A result, not a problem list; a misfit becomes a problem at the layout level |
| Layout (new) | — | A module placed twice (`HallPlan.cs:79-80`); an exit reference that names no exit; tracks that don't meet across a joint; a stretch with no track path between its ends; a closure gap over the tolerance; a height step over the tolerance; a module on a stretch without a speed limit, where the layout expects one |
| Hall (new, Module Draw's phase 10) | — | Clearances, doors kept free, walking paths |

The contract has no translations (`CrossAppReview.md` recommendation 15); every code added here makes that companion resources package more worth having.

### 2.11 What stays in Module Draw

- `Editing`: commands, undo, tools, snapping, selection, scenes, sheets, viewport, number input.
- `Components`: every view and editor component, and `SvgPath`.
- `App`: shell, pages, workbench storage, drafts, recent files, user settings, the settings file.
- From `Model`: `ModuleNameLabel`, `ProfileShading`, `TrackCrossSection`, `Dimension`, `DimensionKind`, `PathCorners` (rendering and editing helpers).
- Its own `Country` and `countries.json` go; it uses the contract's `Country`, and `UserSettings.CountryId` becomes a country code.

## 3. Querying the physical layout

### 3.1 What the geometry gives, and what it doesn't

| Measure | Available from | Unit | Gap |
|---|---|---|---|
| Length along a track | Every route is a `SegmentPath` with `Length`, `PoseAt`, `Between` | mm, reported in metres | Only within one module today |
| Curve radius | `ArcSegment.Radius` on every curved route; a turnout's diverging radius | mm | Nothing reports the smallest radius of a route; `ModuleMeasures.Radius` covers the main path only and is null for mixed radii |
| Gradient | `ModuleExit.RailTopOffset` at each exit; `ModuleMeasures.RampGradient` from the first exit to the second, per mille | mm and ‰ | Height is known only at exits, so a gradient is the average between two exits of one module. `LineEntry.LegAdjustment` raises or lowers a whole module and must be added. There is no height along a track |
| Speed limit | — | km/h | Missing on the module (2.4). On a track it exists (`StationTrack.SpeedLimit`) |
| Number of tracks on the line | `EndProfile.Tracks` at each exit (`ModuleGeometry.TrackPositions`, `ModuleGeometry.cs:31-34`) | count | |
| Usable track length, stub | `StationTrackMeasurement` | m | Needs the standard's fouling distance |
| Position in the hall | Anchor and lines | mm, hall coordinates | Engine not built (2.8). **Not needed for distances**: those follow from the order of the modules alone |

### 3.2 The track graph

Build one graph for the layout, as the public successor of `TrackNetwork`:

- **Edge**: one route of one track element of one placed module (`LayoutModule` id, element id, route number), with its `SegmentPath`, its length, its smallest radius and the `StationTrack` it is part of.
- **Node**: a track end. Ends inside a module are joined where they are within the joining tolerance, as today (0.01 mm, `TrackNetwork.cs:17`).
- **Across a joint**: the hall plan says which two exits meet (the line order, `IsTurned`, `ContinuesFrom`, `StartAt`, `EndAt`). At each exit the end profile gives the lateral offset of each track, as built (`Oriented(exit.Orientation)`). Two exits are seen from outside their own modules, so a track at offset *x* on one meets the track at −*x* on the other, within `FitTolerances.Joint`. A track element that ends on the exit's joint face (`TrackNetwork.IsAtExit`, `:65-75`) at that offset is the node that crosses.
- **A location's centre** is a point on an edge: `Centre` metres along the module's main path from the left end, or its middle when null (`S\Model.Layouts\OperationLocation.cs:112-116`). For a double-track module the main path runs between the tracks, so the centre is projected onto each line track.

Two things must hold for the graph to be whole, and both are validation (2.10): every exit track of the end profile has a track element ending there, and the tracks of two joined exits pair up. Where a module has no drawing, or a drawing without track elements, its part of the graph is missing; the query then says so and does not guess.

A simpler fallback is worth keeping for exactly that case: **along the main paths**. Every drawn module has a main path whose length is the module's length (`ModuleMeasurement.cs:25`), also when it has no track elements. The sum of main-path lengths from centre to centre is what `OperationLocations.md` 4.5 describes, needs no graph, and is what today's hand-entered distances approximate.

### 3.3 Finding the route from one location to the next

A stretch joins two neighbouring locations. Its route is found in two steps:

1. **Which modules.** Walk the lines from the first location's module to the next module that is a location, through `IsTurned` and `ContinuesFrom`, and across `StartAt` and `EndAt` for branches. This gives the plain-line modules in order (`TrackStretch.ModuleIds`), the side each location is left from (`StartSide`, `EndSide`) and the number of tracks (from the end profiles at the joints). It needs no geometry, and is the part the contract already models.
2. **Which track.** A shortest-path search (Dijkstra) in the track graph, restricted to the modules of step 1 and the two end modules, from the centre of the first location to the centre of the second.

The result is a **route**: an ordered list of pieces, each with the module, the track, the length in metres, the smallest radius in millimetres, the module's speed limit and the gradient where both exits of the module give a height. From it follow the distance (the sum), the smallest radius, the lowest speed limit, the steepest gradient, and a running time worked out piece by piece.

### 3.4 Several paths: shortest or average

| | Shortest | Average |
|---|---|---|
| Definition | One well-defined number: the shortest way along the track from centre to centre | Needs a rule for which paths count. All simple paths through a station throat is not usable; the workable rule is one path per line track of the stretch |
| Double track on a curve | Takes the inner track | The mean of inner and outer, which is the length of the main path |
| When a siding or a crossover is added | Unchanged, unless it is a shortcut | Changes if the new track counts as a path |
| To explain to a planner | "The shortest way along the track" | "The mean of the tracks of the line" |
| Size of the difference | With 46 mm between two H0 tracks, a 90° curve differs by 72 mm between the tracks, so 36 mm either way from the mean. On a stretch of 10 to 30 m that is 0.1 to 0.4 %, far below the rounding of a running time to whole minutes | |
| Cost | One search | One search per line track |

**Recommendation: the shortest path as the stretch's distance, with the per-track distances kept in the route result.** The number is stable and easy to explain, the difference from the average is below what a timetable can show, and a caller that wants the average (or Dispatch, which wants each line track for itself) gets the per-track figures from the same query. If the owner prefers the average, define it as the mean over the line tracks of the stretch, never over all paths.

### 3.5 The measured distance and the planner's override

Already in the contract: `TrackStretch.Distance` in metres and `IsDistanceEntered`, which keeps the planner's figure when the layout is updated (`S\Model.Layouts\TrackStretch.cs:77,83`; `ModelHarmonisation.md` decision 10).

- **The measured distance is derived, not stored**: an extension member, `stretch.MeasuredDistance`, null when the layout lacks the geometry for it. It follows the house rule for derived members and can never be out of step with the drawings.
- **`Distance` is what the timetable uses.** While `IsDistanceEntered` is false it is set from the measured distance whenever the layout changes; when true it is the planner's. Where both exist and differ, the planner shows the difference, as decision 10 asks, without needing the newer hall plan beside it.
- **A stretch not made of modules** (`ModuleIds` empty), or one with a module that has no drawing, has no measured distance; `Distance` is then always the entered one. This is today's plan entered from scratch, unchanged.
- **Speed.** Add `IsSpeedEntered` beside the two flags that exist. While false, `Speed` follows the modules of the route; how one figure is taken from several limits is question 3. The running time (`TrackStretchTiming.RunningMinutes`, `:160-168`) is today distance over one speed; with a route it can be summed piece by piece, each at its module's limit.

## 4. Files

### 4.1 The file kinds today

| File | Envelope | Version: released / working copy | Migrations | Identity in the file |
|---|---|---|---|---|
| Module | `tellurian-drawing`, `kind: Module`, `content: { module, endProfiles, measures }` | 1 / 2 | 1→2 | `module.id` (GUID), `module.revision` |
| End profile | `kind: EndProfile` | 1 / 2 | 1→2 | `id`, `revision` |
| Hall | `kind: Hall` | 1 / 2 | 1→2 (enum values only) | `id`, `revision` |
| Hall plan | `kind: HallPlan` | 1 / 2 | 1→2 | `id`, `revision` |
| Catalogue | `kind: Catalogue` | 1 / 2 | 1→2 | None; a `version` text |
| Plan | `tellurian-plan`, no kind, `plan: { … }` | 4 | 0→1 … 3→4 (`S\Model\PlanFiles\PlanFileMigrations.cs:12`) | `plan.id` (usually 0) |
| Layout block | None | None | None | `sourceId`, `sourceRevision` |
| Settings (Module Draw) | `tellurian-drawing-settings`, `version: 1` | | None | App-only; not part of this |

The two envelopes are the same idea written twice. Where they differ:

| | Plan file | Drawing file |
|---|---|---|
| Content property | `plan` (`S\Model\PlanFiles\PlanFile.cs:32`) | `content` |
| Kind | None | `kind`, five values |
| Version | One | One per kind |
| No envelope | Read as version 0 (`:112`) | Refused; version must be 1 or more (`DrawingFileSerializer.cs:135`) |
| Unknown envelope properties | Not kept | Not kept (`DrawingFile.cs:7-21`) |
| Errors | `PlanFileError`: NotAPlan, NewerVersion, MigrationMissing | `DrawingFileError`: InvalidJson, NotADrawingFile, WrongKind, NewerSchemaVersion, InvalidContent |
| Migration step | `(FromVersion, Migrate)` | `(Kind, FromVersion, Migrate)` |
| Chain | `Upgrade(content, from, to, migrations)` | `Upgrade(kind, content, from, to, migrations)` |
| Draft handling | None | `UpgradeDraft` re-runs 1→2 on every version 2 file |

### 4.2 One mechanism

Put it in `Model.Layouts`, the lowest project both need (namespace `…Model.Files`), and let `PlanFile` and the drawing files be thin users of it:

- **A description of a file kind**: the format text, the kind text (none for a plan), the name of the content property, the current schema version, the lowest version it reads, and its migrations.
- **A migration step**: `(FromVersion, Migrate)` on the JSON tree, belonging to one file kind. Written once and never changed, as the plan's are.
- **Opening**: parse; check the format; read kind and version; refuse another kind, a newer version or a missing step with one error type that has the union of today's codes (InvalidJson, NotThisFormat, WrongKind, NewerVersion, MigrationMissing, InvalidContent); run the chain; hand the content to the typed reader.
- **Writing**: the envelope first, by hand with a `Utf8JsonWriter` as `PlanFile.Write` does (`PlanFile.cs:50-59`), then the content through the source-generated metadata. No generic envelope type is then needed in the serialiser context.
- **A header read** without the content (kind, version, id, name, revision), for file lists: today's `DrawingIdentity` and `PlanFile.SchemaVersionOf`.
- **Revision and revision log** recorded here at save, for every kind, so that `revision` is at last incremented in one place.

The file names on disk need not change. Whether the plan file should also get `kind: "Plan"` and `content`, so that every file of the suite has literally the same envelope, is question 5; the reader can accept both forms either way, since that is envelope reading and not a content migration.

**Where each kind's code goes**:

| Kind | Reader and writer | Migrations |
|---|---|---|
| Module, end profile, hall, hall plan, catalogue | `Model.Layouts` | The existing 1→2 step moves here as it is (`D\Model\Files\SchemaVersion2.cs`), then is extended with the changes of section 2 while version 2 is unreleased |
| Layout on its own | `Model.Layouts` | Becomes the hall plan's `content.layout`; a layout block alone stays readable as the open format for other tools (`HallPlanFile.cs:98`) and gets a version through the hall plan's |
| Plan | `Model` (`PlanFile`), calling the shared mechanism | 0→4 as they are; 4→5 for what section 6 changes in the layout part |

`UpgradeDraft` is removed when version 2 is released; files written by development builds were never meant to last (`D\docs\OperationLocationsTasks.md:30`).

**Schemas.** Module Draw generates JSON Schemas from the serialiser's metadata and fails a test when they drift (`D\schemas\README.md`, last section). The generator and the `schemas\` folder move with the file code. Give every property its unit in the schema; the single line "Lengths in millimetres" is false for the contract's metres (`CrossAppReview.md` section 3).

### 4.3 What is embedded where

- **A module file** embeds the end profiles its exits use. Keep.
- **A hall plan file** embeds the hall and, in its layout, the modules, end profiles, standards and overrides. Keep; it stays self-contained.
- **A plan file** embeds the layout, and so the modules with their drawings. On *Värnamo Moduldagar* the layout content is about 310 kB without logos (`ModelHarmonisation.md` section 10). A module drawing is a few kilobytes *(unverified: judged from the examples in `D\schemas\examples`, not measured)*, so a hundred modules add a few hundred kilobytes. Acceptable, and it is what makes the layout queryable in the planner and in Dispatch.
- **The database** takes the drawings as JSON columns in the contract's own form, as the modules, the standard and the lock key already are (`ModelHarmonisation.md` section 10, step 7). Geometry is read whole and queried in memory; it does not need tables.

### 4.4 Source-generated serialisation

What the owner asks for: a `JsonSerializerContext` with `[JsonSerializable]`, hierarchies by `[JsonPolymorphic]` and `[JsonDerivedType]`, no reflection. Module Draw's context is the model to copy (`D\Model\Files\DrawingJsonContext.cs:17-39`). What the Schedules side does today that source generation can't do as written:

| Today | Where | Problem |
|---|---|---|
| Properties removed by name at run time | `LayoutSerialization.NotWritten` (`S\Model.Layouts\LayoutSerialization.cs:22-33`), `PlanReferences.NotWritten` (`S\Model\Schedules\PlanReferences.cs:26-45`) | Finds members through `AttributeProvider is MemberInfo` (`LayoutSerialization.cs:92`) |
| Properties that exist only in the file, added at run time | `FileProperties` and `CreateJsonPropertyInfo` (`LayoutSerialization.cs:36-46,100`; `PlanReferences.cs:48-56,71`) | Creating property metadata at run time is the part marked unsafe for trimming and AOT; the values wait in a `ConditionalWeakTable` (`:19`) |
| A property written on a condition | `module.tracks` only when the module is no location (`LayoutSerialization.cs:94-96`) | |
| Every property without a getter or setter removed | `PlanReferences.cs:67` | |
| Default values left out | `PlanJson.LeaveDefaultValuesOut` (`S\Model\Schedules\PlanJson.cs:70-92`) | Compares with a newly created object |
| Empty id lists left out | `PlanJson.cs:52-57` | |
| `DefaultJsonTypeInfoResolver`, non-generic `JsonStringEnumConverter` | `PlanJson.cs:31-32`, `LayoutSerialization.cs:53-54` | Reflection |

**Checked by building.** A throw-away console project, .NET 11, with `IsAotCompatible` and `JsonSerializerIsReflectionEnabledByDefault=false`, a polymorphic hierarchy with `kind`, extension data, private `[JsonConstructor]`s and references by id. It built with **0 warnings** and read back what it wrote. What it shows:

1. **Reference by id, with attributes only.** The object carries `[JsonIgnore]`; beside it is the id, whose getter gives the object's id when there is an object and otherwise what was read:

   ```csharp
   [JsonIgnore] public Station? ControlledBy { get; set; }
   public int? ControlledById { get => ControlledBy?.Id ?? field; set => field = value; }
   ```

   After reading, `RestoreReferences` looks the ids up, exactly as now, but reads them from the properties and not from a side table. Before writing nothing needs synchronising, so most of `PlanReferences.SynchroniseIds` (`PlanReferences.cs:86-117`) goes too. This is the pattern the model already has for `TrackStretch.StartId` with a `[JsonIgnore]` on `Start` (`S\Model.Layouts\TrackStretch.cs:54-58`) and for `StationCall.TrackId`; it is extended to the references that lack an id: `ControlledById`, `CargoServedFromId`, `LockKey.HeldAtId`, `DispatchStretch.FromId`, `ToId` and `StretchIds`, `TimetableStretch.StretchIds`, `Module.OperationLocationId`, and in the plan `Origin.LocationId`, `Destination.LocationId`, `ScheduledTrainPart.FromTrackId` and `ToTrackId`, `DriverDuty.PartIds`. The JSON names stay as they are in schema version 4, so **no migration is needed for it**. The ids are real foreign keys, which the database mapping wants anyway (`ModelHarmonisation.md` section 6).
2. **An id property need not be public.** `[JsonInclude]` on an `internal` property worked, as long as the context is in the same assembly as the type. Public is simpler and serves EF Core; internal keeps the public surface smaller. Either way each assembly generates its own types: `Model.Layouts` has its context, `Model` has one for the plan's own types, and the two are combined (`JsonTypeInfoResolver.Combine`), the layout's first. *(Unverified: the combination across the two assemblies was not built, only the single-assembly case.)*
3. **A property written on a condition** is a second property under the same JSON name: the module's own track list is written as `tracks` only when the module is no location (the getter returns null otherwise), while `Tracks` itself is `[JsonIgnore]` and returns the location's list when there is one. Built and checked: after reading and restoring, the module's `Tracks` *is* the location's list.
4. **What is not written** becomes `[JsonIgnore]` on the property, where most of the contract has it already (`TrackStretch.cs:110-115`, `StationTrack.cs:109-114`).
5. **Get-only properties are written by default** by the generator (the spike wrote one). Set `IgnoreReadOnlyProperties = true` in `[JsonSourceGenerationOptions]`; it replaces the rule at `PlanReferences.cs:67`. Derived members written as extension members, the house rule, never reach JSON at all.
6. **A private `[JsonConstructor]`** works with the generator on .NET 11. The model has about twenty (`Plan`, `Train`, `StationCall`, `TrackStretch`, `Company`, `DispatchStretch` …).
7. **`kind` in any position** is read with `AllowOutOfOrderMetadataProperties = true` in the generation options; unknown properties land in `ExtensionData`. Both checked.
8. **Enum values as strings**: `UseStringEnumConverter = true`, as Module Draw has. Converters with a parameterless constructor are listed in the options (`Point`, `Angle`, `Pose`; the plan's `CountryByIdConverter` and time converters).
9. **`DriverDuty.parts`**, the parts a duty owns because no schedule does (`PlanReferences.OwnedParts`, `:220-225`), becomes a property computed the same way. It is the one file property with logic in it.

**Default values: the one decision.** "Leave out a value that equals what a new object has" (schema version 4) can't be said with an attribute: `JsonIgnoreCondition.WhenWritingDefault` compares with `default(T)`, and several properties start as something else (`IsScheduled`, `HasPassengerExchange`, `HasCargoExchange`, `IsElectrified` are true; `TracksCount` is 1). Two ways:

| | A. Attributes only | B. One modifier on the generated resolver |
|---|---|---|
| How | `DefaultIgnoreCondition = WhenWritingDefault` for the context, and `[JsonIgnore(Condition = JsonIgnoreCondition.Never)]` on each property whose initial value isn't `default(T)`, so it is always written | `Context.Default.WithAddedModifier(…)`: for each generated type, create one object and set `ShouldSerialize` on each property to compare with its value. No property is added or looked up by reflection |
| Checked | The attributes are standard; not built here | Built: no warnings, output as today's |
| Files | A few bytes larger: those flags are always written, and an empty list is written as `[]` | Byte for byte as schema version 4 |
| Reading older files | Unchanged: a missing property keeps its initial value | Unchanged |
| Rule "enforced by attributes" | Met in full | The metadata is generated; one function still shapes it at start-up |
| Risk | A new property with a non-default initial value and no attribute would be dropped from files when it happens to equal `default(T)`. A test can catch it: every such property must carry the attribute | A modifier that sets `ShouldSerialize` replaces the null check, so it must test for null itself (the spike wrote `"tracks": null` until it did) |

Recommendation: **A**, because it is what the owner asked for and the cost is small, with the test named under "Risk". If the size of the empty lists matters, B does only that.

## 5. Catalogue

### 5.1 The model here

Add to `Model.Layouts` (namespace `…Model.Layouts.Catalogue`):

- **`Catalogue`** `{ Version, Scales, Standards, AddOns, EndProfiles, Overrides, ExternalReferences }`, with the lookups, `Merge` (the user's entries over the served ones), `ForCountry` and `ForScale`. Replaces `DrawingCatalogue`.
- **`ModuleStandard` (existing, extended)**: today `{ Id (int?, the Registry's), ShortName, Scale }` (`S\Model.Layouts\ModuleStandard.cs:10-16`). It takes all of Module Draw's properties (1.9) and becomes the one standard type. Where a module or a layout names a standard, only `Id`, `ShortName` and `Scale` are written; the catalogue, and a layout's `Standards`, hold the full entry. Units: millimetres, stated per property.
- **`Scale` (existing)**: already has `ShortName`, `Denominator`, `Gauge`, `Remark` (`Scale.cs:13-22`), which covers Module Draw's. The planner's own scale catalogue (`S\Model\Layouts\Scale.cs`) gives way to it, as `ModelHarmonisation.md` already says.
- **`StandardAddOn`**, **`EndProfileCompatibilityOverride`**, **`EndProfile`** (2.3), **`RailSystem`**.
- `ToShared()` disappears with the second type.

### 5.2 Whose ids

The catalogue's entries are referred to from files that are passed around, opened offline and kept for years; the user can also make entries of their own that the Registry has never seen (`D\docs\Specification.md`, CA-2).

| Option | For | Against |
|---|---|---|
| **Own GUIDs, the Registry's ids as external references** | Already so in every Module Draw file since version 1. Works offline and for the user's own entries. A file names the same standard whichever Registry served it | The Registry must store a GUID per standard, add-on and end profile, and keep it when an entry is uploaded |
| The Registry's `int` ids | One id fewer | An entry made offline has none. The ids mean something in one database only. Every existing file would need mapping |
| Short names only | Readable; what the contract's `ModuleStandard` uses as its equality | A standard can be renamed; two user-made profiles can share a designation |

**Recommendation: own GUIDs.** A reference is the GUID with the name beside it for the reader and as a fallback match (`Reference`, 2.9); the Registry's id sits in `externalReferences.moduleRegistry`, where Module Draw's migration already puts it (`standardId`, `endProfileId`). Scales stay keyed by short name, with the denominator.

This collides with the contract as built: `ModuleStandard.Id` and `Scale.Id` are the Registry's `int?` (`ModuleStandard.cs:10`, `Scale.cs:10`). `ModuleStandard.Id` becomes the GUID, and the Registry's number moves to the external reference. A plan can hold the number only where a hall plan carried it (`S\Model\Layouts\HallPlanImport.cs:62`), and no hall plan writer exists, so in practice nothing stored is affected; the plan's 4→5 migration moves it if it is there. `IdentitySettings.ScaleId` is a table id too (`CrossAppReview.md` finding 7) and becomes the scale's short name in the same step.

### 5.3 What the Registry's API must serve

Assumed in place, per section 0. All answers in the suite's envelope and the contract's JSON, so that the same reader serves a file and an API call:

- **The catalogue**: `kind: Catalogue`, with scales (short name, denominator, gauge), standards with every property of 1.9, add-ons, end profiles **with their drawings** (plate, outline, ground line, tracks with offset, gauge and rail system, screw holes, dowels, screw-hole pattern, source), and compatibility overrides. Each entry with its GUID and the Registry's id in `externalReferences.moduleRegistry`.
- **Filters**: by scale and by country, matching `ForScale` and `ForCountry`.
- **A version** of the catalogue's content and an ETag, so the app can ask whether anything changed and keep its copy for offline use.
- **Upload of an end profile**: `kind: EndProfile`. The Registry keeps the GUID, answers with the stored entry including its own reference, and refuses with the differences when its copy is newer (as Module Draw task H2 expects for modules).
- **No `ReferenceHandler.Preserve`**, a versioned route, CORS for the browser apps, and the key in a header (`CrossAppReview.md` recommendation 11).
- **Fouling distance, rail-top height and minimum clearance per standard**: the first is needed to measure a track's usable length, the others for joints and clearances. If the Registry doesn't have them, they are the first entries in the list of gaps.

**The built-in catalogue** stays as the seed for a first start without a network: an embedded catalogue file, moved here with the catalogue code. The served catalogue replaces it when fetched, and the user's own entries are layered over either. Its version says `provisional`: the end profiles are entered by hand and not checked against the published drawings (`BuiltInCatalogue.cs:11-15`).

## 6. Gaps and conflicts

### 6.1 Track ids: per module in a drawing, per layout in a layout

The contract says both (`S\Model.Layouts\StationTrack.cs:41-45`); Module Draw numbers from 1 in every module (`StationTrackList.cs:40-47`); a call refers to a track by its id alone. Nothing says how one becomes the other, and the id must not change from one save of a hall plan to the next, or every call loses its track.

**Recommendation.** When a module is placed in a layout, its copy's tracks get ids that are free in the layout, once, and the copy's track elements are renumbered with them. Each track keeps `SourceId`, its id in the module drawing. The hall plan file stores the placed copy, so the ids survive every save. When the embedded module is updated from a newer drawing, tracks are matched by `SourceId`, then by designation; a new track gets a new id. `LayoutComparison.CompareTracks` (`LayoutComparison.cs:269-270`) then matches by id first when both layouts come from the same hall plan, which is `CrossAppReview.md` recommendation 4.

The alternative, an id made by a formula from the placed module's id and the track's own, needs no stored mapping but builds meaning into a key and breaks at a module with more tracks than the factor allows.

### 6.2 `module.tracks` and `location.tracks`

Module Draw writes the tracks on the module and an empty list on the location. The contract's layout writes them on the location and leaves `module.tracks` out for a module that is a location (`LayoutSerialization.cs:94-96`), and after reading makes the two the same list (`:146-151`). A module copied as it is from a module file into a layout loses its tracks (`CrossAppReview.md` section 3).

**Recommendation: one rule for every file. The tracks are written where they are owned: on the location when the module is one, else on the module.** In memory `Module.Tracks` is always the list, the location's own when there is a location (4.4, point 3). The plan file keeps its shape. The module file changes, while version 2 is open: `module.operationLocation.tracks` in place of `module.tracks` for a module that is a location. Track elements refer by id, so they don't care which list holds the track.

The other way round, the module always owning the tracks, would suit Module Draw, but the planner's layouts have locations without modules, and every plan file would change.

A second difference of the same kind remains: in a module file the location is nested in the module; in a layout it is in `operationLocations` and the module has `operationLocationId`. Two properties, each written in its own case, handle it with attributes (4.4, point 3); in a layout a location is never nested.

### 6.3 Location ids

Module Draw writes `id: 0` for every location. In a layout the id is given when the module is placed and stored with the placed copy, like the track ids. A module file keeps 0, which default-value omission then leaves out.

### 6.4 Units

The contract: metres, as `double`, "No platform is a platform length of 0" (`S\Model.Layouts\README.md`). Module Draw: millimetres and degrees for everything drawn, metres for the three contract values (`length`, `platformLength`, `centre`).

Converting the drawings to metres would change every number in every existing file, and the tolerances (0.01 mm joins, 0.5 mm joint fit) read badly in metres. **Recommendation: two units, each with its domain, and say so in the README**: what is drawn (points, poses, radii, widths, heights, gauges, tolerances) is millimetres and degrees; what is operated (track length, platform length, centre, distance) is metres; speed is km/h of the prototype; gradient is per mille. Every property says its unit in its summary and in the schema, and every query of section 3 returns operational units. Whether units should instead be carried by a type is question 6.

### 6.5 `ExitDirection` and `ExitSide`

The same five values in two enums (`D\Model\Modules\ExitDirection.cs:7-23`, `S\Model.Layouts\ExitSide.cs:8-24`). `ExitSide` has explicit numbers; `ExitDirection` has none but the same order. **`ExitSide` holds; `ExitDirection` goes.** The JSON values are the same names, so no file changes.

The exit reference by side and number is a decision already made (`OperationLocations.md` 4.5), so that a module can be redrawn without a plan breaking. Its weakness, noted in `CrossAppReview.md` section 5, is that reordering the exits of one side re-targets every reference. It moves here as it is; a check that the exits of a side keep their order when an embedded module is updated belongs in the update, not in the reference.

### 6.6 Module identity: GUID, `int`, `SourceId`

Three ids for one module: Module Draw's GUID on its subclass, the contract's `LayoutModule.Id` (`int`) and `LayoutModule.SourceId` (GUID, which nothing writes). With the GUID on the contract's `Module` (2.4), `LayoutModule.SourceId` says the same thing twice and goes; `LayoutModule.Id` stays as the id in the layout. Both `SourceId` and `ExternalReferences` on `LayoutModule` are removals, which the README forbids ("never renamed, removed", last line); since no file carries them, do it in the 0.4.0 that `CrossAppReview.md` recommendation 1 already calls for.

### 6.7 Theme

`Module.Theme` is a string in Module Draw (`Module.cs:39`); the contract's `Theme` is an enum without numbers (`S\Model.Layouts\Theme.cs:6-16`), used for the layout. The Registry has an enum and free text in three places (`CrossAppReview.md` section 4). They are not obviously the same concept: the layout's theme chooses American or European practice for the timetable; a module's theme is what it depicts. Keep the module's as text until the Registry's meaning is settled, and name it apart (`ThemeText`, or drop it) so two properties called `Theme` don't have two types.

### 6.8 Two sets of track rules

Module Draw checks tracks with its own seven codes and deliberately not with `OperationLocationRules` (`OperationLocationsTasks.md:40`). Three overlap (no designation, designation not unique, no tracks); four need the drawing (unknown track, element without track, track without elements, scheduled track reaches no exit). With the drawing in the contract there is one rule set: the contract's rules for the list, extended with the four that need geometry, run only when the module has a drawing.

### 6.9 Revision

`revision` is documented as incremented on every save and never is (`CrossAppReview.md` section 5); `Layout.SourceRevision` and the planner's "is this file older?" warning rest on it. Fixed by 4.2: the file code increments it and records the log, for every kind.

### 6.10 The contract's `Layout` carries the planner's settings

A `Layout` has `Settings` (graphic timetable, time and speed, validation, integration), `Countries`, `Companies`, `Regions`, timetable and dispatch stretches, topology positions (`S\Model.Layouts\Layout.cs:33-57`). In a hall plan saved by Module Draw they are empty or default. With defaults left out they don't appear, but Module Draw must **keep** them when it opens a layout that has been through the planner and saves it again; mutable objects read and written whole do that without extra code. What happens when both apps changed the same value is the question section 0 leaves open; nothing here settles it.

### 6.11 Equality by text

`OperationLocation` equals by signature, `StationTrack` by designation and location, `ModuleStandard` by short name (`OperationLocation.cs:194-200`, `StationTrack.cs:120`, `ModuleStandard.cs:19`). Module Draw's records compare by value and its editor relies on that to tell whether an edit changed anything (`StationTrackMeasurement.cs:76-77` works round it for tracks already). As mutable classes the moved types compare by reference unless given an identity. Give each its id as equality (GUID for documents, `int` inside), and let the editor compare by serialised content where it needs "is anything different", as `ModuleFileContent.Same` does for end profiles (`ModuleFileContent.cs:89-90`).

### 6.12 Version numbers

Module Draw compiles against a `Model.Layouts` 0.3.5 that is older than the source with the same number (`CrossAppReview.md` finding 2). Every step of section 7 that changes the contract takes a new version, pushed to the local feed before Module Draw is touched.

### 6.13 One package or several

Today `Model.Layouts` is small and "changes far more seldom than the planner" (`Model.Layouts.csproj:7`). After this it holds geometry, drawings, the catalogue, files, validation and auto-draw: about three times its size, and changing as often as Module Draw does. Dispatch and the Registry will take all of it to get a location and a track. That follows from decision 1 and is acceptable; trimming removes what an app doesn't call. Splitting into several packages can wait until someone is hurt by it.

## 7. Proposed order of work

Each step leaves both apps building and their tests passing. "S" is Schedules, "D" is Module Draw. A step that changes the contract ends with a new `Model.Layouts` version on the local feed.

| # | Step | S | D |
|---|---|---|---|
| 1 | **Source-generated JSON for what exists.** A context for the layout types and one for the plan's; ids as properties (4.4, point 1); `[JsonIgnore]` in place of the not-written lists; the default-value decision. `LayoutSerialization` and `PlanReferences` keep only `RestoreReferences`. The fingerprint tests of *Värnamo Moduldagar* guard it; plan files stay at version 4 | Yes | — |
| 2 | **The shared envelope and migration chain** (4.2), with `PlanFile` moved onto it. No file changes | Yes | — |
| 3 | **Version 0.4.0**: `IsSignal` gone (already in source), explicit enum numbers, `ExternalReference` typed, the constants for the Registry's entry. Module Draw updates and drops `isSignal` from schemas and examples | Yes | Yes |
| 4 | **Geometry moves.** `D\Geometry` becomes `Model.Geometry` with its tests; Module Draw references the package and deletes its project. Namespaces change, nothing else | Yes | Yes |
| 5 | **Drawing files are read and written here.** The envelope, kinds, the 1→2 migration, `DrawingIdentity`, the revision log (now incrementing `revision`), import from ZIP and folder, schema generation. To do this before the types move, the content types move in the same step in their present shape (records become classes): end profiles first, then catalogue, then hall, each a sub-step with Module Draw switching over. **Packaged as decided 2026-11-21** (`CrossAppReview.md` decisions 18 to 20): reading in `Importers.Drawing` with the migrations, writing in `Exporters.Drawing` with the schemas, the shape in `Model.Layouts`; sub-step 0 adds the contracts (`IImporter<T>`, `Exporters.Interfaces`, `Exporters.Services`). **Done 2026-11-21:** end profiles (4.0.11), catalogue (4.0.18), hall (4.0.19, namespace `…Model.Layouts.Halls`; obstacles, doors and no-go zones with `int` ids, migrated from GUIDs, `DrawingMigrations.HallToVersion2` giving hall plans the new obstacle ids) | Yes | Yes |
| 6 | **The module.** **6a done 2026-11-21 (4.0.22):** the types and logic moved in their present shape (namespaces `…Model.Layouts.Modules`, `.Tracks`, `.Measures`, `.AutoDraw`; drawing, exits, sides, sections, joints and track elements as classes), Module Draw's additions on the contract's `Module` with `Drawing` never null and written only when not empty, `ExitSide` for `ExitDirection`, the module's theme dropped (owner's decision), `ModuleImporter` and `ModuleExporter` with the module's 1→2 step. **6b done 2026-11-22 (4.0.23):** the format changes of 2.4–2.6 while version 2 is open: a side refers to its exits by `ExitPlace` (`ModuleDrawing.ChangeExits` keeps sides at their exits when directions are inferred), track elements have `int` ids numbered from 1 (`ModuleDrawing.NumberTrackElements`), `StationTrack.IsLengthEntered` and `IsStubEntered` in place of the drawing's two lists, `Module.SpeedLimit` added, `Module.StandardId` gone (the standard named in a module carries its id, `NameOnly` keeps it), `LayoutModule.SourceId` gone; the module's 1→2 step migrates each. **Still open:** internal joints refer to sections by label, `StationTrack.SourceId` (6.1), `LayoutModule.ExternalReferences` (6.6), and `StationTrackValidation` merging with the contract's rules (6.8) | Yes | Yes |
| 7 | **Catalogue identity.** One `ModuleStandard`, its `Id` the GUID, `ToShared()` gone; the planner's scale catalogue gives way; plan schema 5 moves a Registry id and `ScaleId` | Yes | Yes |
| 8 | **The layout gets lines and joints; the hall plan is a layout in a hall** (2.7, 2.8). The hall plan file's `content.layout` is now written, by the same code that reads it. Ids given at placement and kept (6.1, 6.3). Module Draw's `HallPlans` folder goes | Yes | Yes |
| 9 | **The engine**: stretches from the lines (which modules, sides, track count), then the track graph, routes and measured distance (section 3), then poses, closure gaps and joint fits. Each is new code with tests on the schema examples; nothing existing depends on it yet | Yes | — |
| 10 | **The planner uses it**: `MeasuredDistance` shown beside the entered one, speed from the modules, running time by route. `HallPlanImport` reads a real hall plan | Yes | — |
| 11 | **Schema version 2 is released** in Module Draw, `UpgradeDraft` removed, `Model.Layouts` and `Model.Geometry` published to nuget.org (its CI can't build otherwise) | Yes | Yes |
| 12 | **Module Draw builds its hall-plan editor** (its phase 9) on the model and engine that are now here | — | Yes |

Steps 1 and 2 are worth doing first whatever else is decided: they remove the warnings the build already gives and are needed by every later step. Steps 4 to 8 are best done without a release of Module Draw in between, since each changes the unreleased version 2 format.

## 8. Questions for the owner

1. **Hall plan and layout.** Is a hall plan a `Layout` plus where it stands in a hall (recommended, 2.8), so that the planner's plan carries the modules, drawings and lines but not the hall? Or is the hall plan itself the layout, hall included?
2. **Shortest or average** (3.4). Recommended: shortest, with the per-track distances available. If average: over the line tracks of the stretch?
3. **One speed for a stretch from several module limits.** The lowest limit among the modules of the route, or a running time summed module by module with `Speed` only shown? And do the two end modules, the locations themselves, count?
4. **Default values in JSON** (4.4): attributes only, with slightly larger files (A), or one modifier on the generated metadata, with files exactly as today (B)?
5. **One envelope on disk.** Should the plan file get `kind: "Plan"` and `content` at its next schema version, so every file of the suite has the same envelope, with the old form still read?
6. **Units.** Two documented units, millimetres for what is drawn and metres for what is operated (recommended, 6.4), or a length type that carries its unit?
7. **Catalogue ids.** Own GUIDs with the Registry's ids as external references (recommended, 5.2)? It asks the Registry to store a GUID per standard, add-on and end profile.
8. **The `type` discriminator** of track elements and path steps, in lower case, beside `kind` in PascalCase everywhere else: leave it, or make it uniform in the 1→2 migration while version 2 is unreleased?
9. **`FunctionalState` and `LandscapeState`**: enums in the model with the Registry's numbers, or left as the Registry's codes?
10. **Height along the track.** Today a height is known only at a module's exits, so a gradient is an average per module. Is that enough, or should a ramp be able to say where it starts and ends?
11. **Names.** `Reference` for an id-and-name reference to a catalogue entry, `Anchor` for where a connected group stands in the hall, `Catalogue` for the catalogue type: acceptable, or are there Registry names to follow?
