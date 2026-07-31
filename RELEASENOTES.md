# Release Notes

## Version 3.1.0

### New Features

- **Route continuity validation (rule T5).** `CheckRouteContinuity(this Train)` checks that every
  leg a train runs — each pair of calls in run order — is a `TrackStretch` of the layout. A train
  travels a stretch by departing its start and arriving at its end, so it calls at both ends of every
  stretch on its way; two successive calls with no stretch between them are a route that jumps a
  location. Reported as the new `ValidationErrorType.TrainRouteNotConnected`, at `Warning` severity
  and `ValidationScope.Train`. Two successive calls at the same operating location travel no stretch
  and are not reported. Gated by the new `ValidationSettings.ValidateRouteContinuity` (default on),
  and run from both `Plan.GetTimetableValidationErrors` and `Train.GetValidationErrors`.

  This is the rule behind `DeletionRules.MayDelete(StationCall)` allowing only a train's first or
  last call to be deleted.

- **`Train.CallsInRunOrder`** returns a train's calls in the order it runs them, ordered by
  `StationCall.SortTime`. `Train.Calls` is in insertion order, which is not run order — a call added
  last can be timed first — so anything reasoning about the route reads it through this property.

- **`Schedule.EditPart(part, from, to)`** changes the span of a part already in a schedule and adapts
  the neighbouring part it meets, so a working can be reshaped without truncating it. The part keeps
  its train and its identity — only `TrainPart.From` and `TrainPart.To` change — so the driver duties
  referencing it follow the change. Where the edited part meets a neighbour today and that
  neighbour's own train calls at the new joint, the neighbour is adapted to it (extended as readily
  as shortened); the adaptation is one step only and never reaches past the neighbour. A joint that
  is already broken keeps its gap rather than being rewritten, and anything the edit leaves
  inconsistent is applied as asked and reported by the schedule validations (S1, S2).
  **`Schedule.PlanPartEdit`** returns the same `PartEdit` without changing anything, so an editor can
  show what an edit would do — and what it would leave behind (`LeavesGapBefore`, `OverlapsNext`,
  `IsConsistent`) — before it is applied.

- **`Layout.IsConnected(from, to)`** answers whether a track stretch joins two operating locations,
  in either direction. Unlike `Layout.TrackStretch(from, to)` it tolerates a layout holding more than
  one stretch between the same pair.

- **`Plan.SetDeparture(call, time)` and `Plan.SetArrival(call, time)`** set one call time and push the
  times on one side of it by the same number of minutes, so that part of the run follows the change with
  its run and dwell times kept. The two are mirrors: a departure works forwards, the direction the train
  runs, moving every later time and leaving the call's own arrival where it is; an arrival works
  backwards, moving every earlier time and leaving the call's own departure where it is. Either way the
  stand at the edited call absorbs the change and the times on the other side are untouched. At the
  train's origin nothing precedes the call, so setting its arrival only changes the driver's preparation
  time; at the terminus nothing follows it, so setting its departure only changes the finishing time.
  All-or-nothing: nothing is written and `null` is returned when the result would leave the plan's
  operating window (the same rule `Plan.Move` follows). A time that leaves the train inconsistent — a
  departure set before its own arrival — is applied as asked and reported by the validation rules.

### Fixes

- **A plan is serializable in every shape it can be edited into.** Derived properties were written to
  the plan document along with the stored state, and several of them throw on a half-finished plan: a
  train left with fewer than two calls (`Train.AsTrainPart`, `DriverStartTime`, `DriverEndTime`,
  `Layout`) or a timetable stretch whose route has been emptied (`TimetableStretch.Stations`). One such
  read failed the whole serialize, which in Planning.App silently ended persistence — the planner
  worked on and lost everything from that point when the browser was reopened. Every derived property
  in the plan graph now carries `[JsonIgnore]`, so only stored state is written (`TrackStretch.Passings`,
  `StationCall.OperationLocation/IsStop/IsPassthrough/SortTime`, `TrainPart.Train/Departure/Arrival`,
  the note rendering forms, and the cargo-flow and vehicle display names among them). Nothing is lost:
  none of them has a setter, so none was ever read back. Saved documents get noticeably smaller.

  **Breaking:** `Train.Layout` is now `Layout?` and is `null` when the train has no calls, rather than
  throwing; `Train.DriverStartTime` and `DriverEndTime` throw `InvalidOperationException` instead of
  `NullReferenceException` for a train with no calls; `TimetableStretch.Stations` is empty for a
  stretch with no route instead of throwing.

- **A station call is written once, in its train.** `StationTrack.Calls` is an index into `Train.Calls`,
  rebuilt from it by `Timetable.RebuildStationCalls()`, but it was written to the plan document as well
  — and written *first*, since the layout precedes the trains. Half the plan therefore hung below the
  tracks: a track's calls, their trains, those trains' categories and cargo flows, and back through each
  call's track again, which is what drove the nesting deep enough to need `MaxDepth = 256`. `PlanJson`
  now omits it when writing, and `Timetable` implements `IJsonOnDeserialized` so the index is rebuilt
  after every read rather than only where a caller remembered to. Documents get about 40 % smaller. The
  property is still *read*, so a plan written by an earlier version — where those objects took their
  `$id` under a track — still loads; `Model.Tests/TestData/Plan.0.3.2.json` keeps that a tested promise.

- **Traction coverage (rule S4) is judged leg by leg instead of per train.** The check asked only
  whether a train had *some* part in a traction unit's turnus, so a train left half-worked passed
  silently — shorten a part from A→C to A→B (and the next from C→A to B→A) and B→C had no vehicle
  yet nothing was reported. Coverage is now computed for every leg the train runs, on every session
  it runs it, from any schedule that works it; consecutive unworked legs are reported as one span, so
  a train with no traction at all still gives a single error. Legs between two calls at the same
  operating location — a train changing track there — travel no stretch and need no traction.
  `ValidationErrorType.TrainMissingTraction` now carries the span rather than the whole train, and its
  message reads *"Train {0} has no traction unit between {1} and {2} on sessions {3}."*
  `ValidationError.TrainMissingTraction` takes the two calls: **breaking** for anyone constructing one.

- **`Plan.ValidateLocomotiveCoverage` (rule P4) no longer checks coverage gaps**, only overlapping
  locomotive assignments. Its gap check read `Train.Calls` in insertion order, so on a hand-edited
  train it missed the very gap it was for, and where it did fire it reported what S4 now reports.
  `ValidationErrorType.LocomotiveCoverageGap` is kept but is no longer produced. Coverage gaps are
  reported whenever `ValidateSchedules` is on, no longer only when `ValidateLocomotiveCoverage` is.

- **The train speed check (rule T3) now covers a train's last leg.** Its loop stopped one leg short,
  so the run into the terminus was never checked and a two-call train was not checked at all. Plans
  with slow or fast final legs will report findings that were previously silent.

- **The call time-sequence check (rule T2) and the speed check (rule T3) now compare calls in run
  order** instead of insertion order. On a hand-edited train the two differ, and pairing calls the
  train does not run one after the other reported conflicts that were not conflicts.

- **`TrackStretch.Passings` reads a train's calls in run order** for the same reason, so a stretch
  capacity conflict (rule L3) is not missed on a train whose calls were added in another order than
  it runs them. Imported plans are unaffected: there the two orders coincide.

- **`Train.DriverStartTime` and `Train.DriverEndTime` take the train's first and last call in run
  order.** They read `Calls[0]` and `Calls[^1]`, so on a hand-edited train the driver's service window
  — and with it `Plan.FitsWithinOperatingWindow`, which decides whether a train may be created, moved
  or cloned — was measured between two calls in the middle of the run.

- **The planning and graph helpers pair a train's calls in run order.** `TimetableStretch.InferDirection`
  could put a train in the opposite direction's column, `GraphicalTrainSegment`'s indices are now
  positions in `Train.CallsInRunOrder` (so overtake splitting and the extrapolated sort key follow the
  route), `Plan.UpdateTimings` recomputes along the legs the train runs instead of failing on a pair
  with no track stretch between it, and `Plan.CloneMany` measures its interval from the train's own
  departure.

- **Automatic schedule building reads a train's origin in run order.** `BuildSchedulesAutomatically`
  and `ContinuationsFor` took the train's start location and departure from its first *added* call, so
  a hand-edited train was chained from the wrong end — usually not chained at all, since it appeared
  to start where it does not.

- **`Train.AsTrainPart(fromCallIndex, toCallIndex)` and `Schedule.JoinCallIndexFor(train)` now index a
  train's calls in run order** instead of insertion order. The two are positions in the same list —
  the join index is fed straight back into `AsTrainPart` — so on a hand-edited train the picker built
  a part between the wrong two calls, and `Train.AsTrainPart` (the whole train) could end at a call
  the train does not run last. `Plan.CandidateTrainsFor` orders its candidates by the same run-order
  call. Imported plans are unaffected: there the two orders coincide.

- **Displayed kilometres are whole numbers.** `TimetableStretch.DisplayedDistanceToStation` and
  `TimetableStretch.StartKilometer` round the stored metre distance scaled by
  `TimeAndSpeedSettings.DistanceFactor` to the nearest kilometre (halves upwards) instead of
  returning a fractional figure. A diverging stretch adds its junction offset in metres before
  scaling, so the branch and the line it leaves round the junction station to the same kilometre.
  `DistanceToStation` still returns the raw, unscaled metre distance.

## Version 3.0.0

This is a major release. It is source-breaking for consumers of the 2.x packages
because the domain model namespaces have changed; the type names are unchanged, so
upgrading is largely a matter of updating `using` directives.

### Breaking Changes

- **Model types moved into nested namespaces.** To keep the growing domain model
  organised, types are now grouped under sub-namespaces instead of living in the
  single `Tellurian.Trains.Schedules.Model` namespace:
  - `Tellurian.Trains.Schedules.Model.Layouts` — `Layout`, `Station`, `TrackStretch`, `DispatchStretch`, `Theme`, `Scale`, `Region`, etc.
  - `Tellurian.Trains.Schedules.Model.Timetables` — `Timetable`, `Train`, `StationCall`, `TrainCategory`, etc.
  - `Tellurian.Trains.Schedules.Model.Schedules` — `Schedule`, `Plan`, `VehicleSchedule`, `TrainPart`, `ScheduledObject`, etc.
  - `Tellurian.Trains.Schedules.Model.Notes` — call notes (see below).
  - `Tellurian.Trains.Schedules.Model.Settings` — `LayoutSettings` and related settings.

  Consuming code needs to add the relevant `using` directives; the type names
  themselves are unchanged.

- **`StationCall` stop detection.** Whether a call is a stop is now expressed solely
  by `StationCall.IsStop` (with the inverse `IsPassthrough`); arrival and departure
  times are no longer compared to infer it. A train never stops at a
  `SignalControlledLocation` regardless of the flag.

- **`TrainPart` is now abstract**, with `ScheduledTrainPart` as the concrete portion
  used inside vehicle schedules.

- **XPLN import conventions moved out of the model.** The helpers that derive the stop
  flags from the call times — `WithFixedSingleCallTrain`, `WithOriginAndTerminusDwell`,
  `WithFirstCallDepartureOnlyAndLastCallArrivalOnly` and `WithPassthroughCalls`, along
  with `SetFirstCallDepartureOnly` / `SetLastCallArrivalOnly` — are removed from the
  public `TrainExtensions` in `Tellurian.Trains.Schedules.Model`. Equating arrival and
  departure times with a pass-through is an XPLN convention only, so it now lives in the
  XPLN importer as internal code and cannot be applied to non-XPLN data by mistake.

- **One naming for rendering members.** Every model type that renders itself now
  exposes the pair `ToText` (plain text) and `ToHtml` (`MarkupString` markup).
  `ICallNote.Text` / `Html` are therefore renamed to `ToText` / `ToHtml`, on the
  interface and on `CallNote`, `TextCallNote` and `GeneratedNote` alike; `Region` and
  `Destination` lost the older `ToHtmlMarkup` spelling. Stored note *text* is
  untouched — `DriverDutyNote.Text` remains the persisted value it always was.

### New Features

#### Vehicle-schedule (turnus) building

New building blocks turn a timetable into vehicle working schedules (turnus):
`Plan`, `PlanFactory`, `ScheduledObject`, `ScheduledTrainPart` and `ScheduledUnit`.
`Plan.CreateComplementarySchedule` derives the turnus for the sessions an origin
schedule leaves out, and `ScheduledObject.SessionCombinations` (with the
`SessionCombination` record) enumerates the unique session/day combinations a
vehicle works — one turnus card each.

#### Cargo-flow planning

`CargoFlowTrainPart` models freight wagons a train couples at one call and uncouples
at a later one, referencing a reusable `CargoFlowOptions` description held on the
`Timetable`.

#### Call notes

A new `Notes` model attaches localised instructions to station calls through the
`ICallNote` hierarchy — `CoupleNote`, `UncoupleNote`, `ReinforcementNote`,
`FromParkingNote`, `ToParkingNote`, `UseNote` and `TextCallNote` — with generated
text available in all supported languages.

A generated note now describes itself once, as a localised format string plus the
values substituted into it, and `ToText` and `ToHtml` are two renderings of that one
description. The values are what varies in a note — the vehicle to fetch, the train
to meet — so the markup form emphasises them: they render as
`<b class="value">…</b>` inside the note's `callnote` span. Values that would dilute
the emphasis (a position in the train, a meet time) and values that already carry a
visual form (region chips, session circles) are excluded.

Every value is now HTML-encoded on the way into the markup, so a station, vehicle or
company name containing `&`, `<` or `>` produces a correct note instead of broken
markup. The same applies to the region chip and the cargo destination.

A manual note (`TextCallNote`) may use two Markdown emphases — `*italic*` and
`**bold**` — so a planner can stress the part of a note that matters. They nest,
`\*` escapes a literal asterisk, an unpaired asterisk stays literal, and underscores
are not emphasis. `TextCallNote.Text` is the stored text with its markers, `ToText`
the same text without them, and `ToHtml` the rendered markup. `NoteMarkdown` renders
the same two forms for text that is not yet a note, which is what an editing field
needs. A run of three or more markers is ambiguous without a full Markdown parser and
is left as literal text.

`StationCall.ManualNote`, `ManualNoteText` and `SetManualNote` read and write the
manual note of a call: the note is created on first text, updated in the language it
is read back in, and removed when the text is cleared, so no caller edits
`StationCall.Notes` directly. `TextCallNote.SetText` replaces one translation, and
treats a stored text with no language code — as the XPLN import leaves a remark — as
text to replace rather than a translation to keep.

#### Structured layout settings

`LayoutSettings` gathers configuration into focused groups — `GeneralSettings`,
`GraphicTimetableSettings`, `IdentitySettings`, `IntegrationSettings` and
`TimeAndSpeedSettings` (including `StationTimings` and `SpeedPoint`).

#### Layout identity and catalogues

Layouts now carry a `Theme`, `Scale` and country/`Region` identity, backed by
curated catalogues (countries, train categories and sessions) so new layouts and
plans can be created from scratch via `PlanFactory`.

#### Localised display names

The `ITranslatable` convention provides localised class and note display names in
all supported languages.

### XPLN Importer Improvements

- `XplnImportOptions` supplies the per-import language and country that an XPLN file
  itself does not carry, read from a culture segment in the file name (for example
  `Givskud2021.da-DK.ods`) and falling back to the current culture.
- Station calls are imported as stops or pass-throughs, with origin/terminus dwell
  handled correctly.
- Fixes to importing routes, locomotives and trainsets.

---

## Version 2.1.0

### Breaking Changes

- **`OperationLocation` is now abstract** - The base class `OperationLocation` is now abstract with three concrete subclasses:
  - `Station` - A manned operation location with a dispatcher
  - `SignalControlledLocation` - An unmanned location controlled by signals from another station
  - `OtherLocation` - An unmanned location without signal control

  Code that previously used `new OperationLocation(id, name, signature)` must now use `new Station(id, name, signature)` or one of the other subclasses.

- **`IsShadow` property moved** - The `IsShadow` property has been moved from `OperationLocation` to `Station`, as shadow yards are only applicable to manned stations.

### New Features

#### DispatchStretch

A new `DispatchStretch` class represents a stretch between two manned `Station` objects, useful for dispatch planning:

```csharp
var dispatchStretch = new DispatchStretch(id, fromStation, toStation);
```

The `Layout` class now includes a `DispatchStretches` collection and a `CreateDispatchStretches()` extension method that automatically generates dispatch stretches by following track stretches from station to station:

```csharp
var dispatches = layout.CreateDispatchStretches();
```

#### SignalControlledLocation.ControlledBy

Signal-controlled locations can now reference their controlling station:

```csharp
var block = new SignalControlledLocation(id, "Block A", "BA");
block.ControlledBy = controllingStation;
```

#### JSON Polymorphic Serialization

The `OperationLocation` hierarchy now supports JSON polymorphic serialization using System.Text.Json:

```csharp
// Serialization includes a $type discriminator
// "Station", "SignalControlled", or "Other"
```

#### EF Core Table-Per-Hierarchy (TPH) Support

The `ScheduleDbContext` now supports TPH inheritance mapping for `OperationLocation` with a `LocationType` discriminator column.

### XPLN Importer Improvements

- The importer now creates the correct `OperationLocation` subclass based on the XPLN SubType field:
  - `Station` (or empty) creates a `Station`
  - `Block` creates a `SignalControlledLocation`
  - Other values create an `OtherLocation`
- The `Controlled` field in XPLN can specify the controlling station for `SignalControlledLocation`

### Package Updates

All packages have been updated to target .NET 10.0.

---

## Version 2.0.1

Initial stable release with:
- Core domain model for railway scheduling
- XPLN ODS/XLSX import support
- Comprehensive validation system
- Multi-language support (EN, DE, DA, NB, SV)
- Entity Framework Core support
- JSON import/export services
