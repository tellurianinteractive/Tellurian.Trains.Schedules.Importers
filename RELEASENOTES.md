# Release Notes

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
the same text without them, and `ToHtml` the rendered markup.

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
