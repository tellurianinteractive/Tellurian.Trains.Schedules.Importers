# Release Notes

## Version 3.2.0

### New Features

- **`ValidationSettings.MinMinutesBetweenTrackUsage` is now enforced (validation rule L2).** The setting
  existed but nothing read it. It is the free time a station track needs between two occupants, in
  fast-clock minutes, and it generalises the double-booking check rather than adding a second one: at
  its default of **0** the rule is exactly what it was — two occupancies conflict only where they cover
  the same time, and a train arriving as another leaves is a handover — while above 0 the track must
  also stand free for that many minutes in between. Exactly the required number of minutes is enough;
  one minute less is a conflict.

  The predicate behind it is public as
  **`(Time From, Time To).ConflictsInTime(other, minMinutesBetween)`**, with the existing
  `OverlapsInTime` now defined as its zero case, and **`FreeMinutesBetween(other)`** gives the free
  time between two spans. `StationTrack.GetValidationErrors` takes the required gap as a new optional
  third argument, so existing calls keep the overlap-only behaviour.

  A conflict that is only a missing gap is reported as one: the new resource string
  `CallAtStationTooCloseInTimeToOtherCall` names how much free time there is and how much was required,
  in all five languages, instead of claiming an overlap the times plainly do not show. The error type is
  unchanged (`ValidationErrorType.StationTrackConflict`).

- **A vehicle has an identity that may name only one vehicle per session (validation rule P5).** The new
  **`VehicleIdentity`** is a vehicle's `ExternalId` where it carries one — the identifier it was imported
  under, unique in the system it came from — and otherwise its operating `Company` and `Number`, the
  number alone with no company. The two kinds never match each other. An identity names one physical
  vehicle, so on any one session it may belong to only one of a plan's vehicles, across every
  `ScheduledObjectType`: a `Wagonset` and a `Locomotive` may not share one either. Two vehicles may reuse
  an identity only for strictly non-overlapping sessions.

  `Plan.GetValidationErrors` reports a clash as the new `ValidationErrorType.VehicleIdentityDuplicated`
  (`ValidationScope.Vehicle`, placeless and timeless, keyed to the duplicate vehicle), under the existing
  `ValidateSchedules` setting. Each duplicate is reported **once**, against the first earlier vehicle of
  its identity whose sessions it shares, rather than once per pair — a plan can hold many vehicles under
  one identity, and the pairs of such a group would bury the rest of the list. Imported plans are
  unaffected: every XPLN vehicle carries its own identifier, and all the importer test files report
  exactly the conflicts they did before.

  New members supporting the rule: **`Plan.VehicleClaiming(identity, sessions, excluding)`** answers the
  same question before an edit is made, so an editor can refuse a taken identity;
  `ScheduledObject.Identity`, `IdentityText`, `ClaimedSessions` (a vehicle assigned nowhere claims every
  session, since it holds its identity in the pool) and `HasVehicleIdentity` (false for a cargo flow,
  which carries a synthesised identifier standing for a group of wagons).

- **`Plan.CreateVehicle` no longer composes an `ExternalId`.** It used to set one from the class and
  number (`"BR 218 12"`), which is not an external id at all — that is the identifier a vehicle was
  *imported* under. A vehicle created through the API now carries none, so its `Designation` falls back
  to the composed operator signature, class and number as it always did for an id-less vehicle, and it is
  its operator and number that identify it under rule P5. Callers that relied on a created vehicle having
  a non-null `ExternalId` should use `Designation` instead.

- **Two operation locations are joined by one track stretch.** A track stretch is bidirectional
  infrastructure, so one defined the opposite way round joins the same pair and is the same connection:
  a second stretch between them would duplicate what the layout already holds.
  **`Layout.StretchBetween(from, to, excluding)`** finds the stretch that already joins a pair, matching
  either direction, and returns `null` when nothing does. Its `excluding` argument is the stretch being
  edited, compared by reference, so a stretch is never reported as its own duplicate while its own
  endpoints are being changed. `Layout.IsConnected` now answers through it, so connectivity and duplicate
  detection share one definition of what "joined" means. Where a layout already holds more than one
  stretch between a pair — a fault of its own — the first is returned rather than the caller failing.

  `Layout.Add(TrackStretch)` is unchanged: it still ignores an exact duplicate and still accepts a
  reversed one, so a route that reverses at a station and comes back can be expressed.

- **An operation location can require a lock key, and the notes for it are generated.** Where cargo is
  exchanged but nobody is on duty — an unmanned station or an `IndustrialArea` — the switches are
  padlocked and the key is kept at a manned station along the line. The new
  **`OperationLocation.LockKey`** holds that station (`LockKey.HeldAt`) and optionally what the key is
  called (`LockKey.Name`); `null` means no key is needed. Two extension properties say where it applies:
  **`OperationLocation.CanRequireLockKey`** (exchanges cargo and is not a manned station) and
  **`Layout.LockKeyHoldingStations`** (every manned station).

  From that, **`StationCall.LockKeyNotes`** derives the two notes a loco driver reads, both written at
  the key-holding station and neither at the location the key unlocks: a departure `PickUpLockKeyNote`
  ("Pick up key A1 for unlocking Bruket.") and, on the way back, an arrival `LeaveLockKeyNote` ("Leave
  key A1 from Bruket."). Only a cargo train gets them, and only where it *stops* at both ends — a key
  cannot be collected in passing, and a train running through unlocks nothing. Which visit to the
  holding station a key belongs to is decided by the stops in between: it is collected at the last stop
  there before the work and handed back at the first one after it, so a train calling there twice is
  not told to fetch the same key twice. Both notes are included in `StationCall.DriverNotes` at
  `DisplayOrder` 200, after the stop notes and before the crossings. The texts are resources
  (`PickUpKeyForUnlocking`, `LeaveKeyFrom`, and the two forms used when the key has no name) in all five
  languages.

  A key is in force only while both ends of it hold: the location must still need one and the station
  holding it must still be manned. Manning is edited on both sides long after a key is set, so either
  change can leave the key meaningless — it is then **kept but ignored**, since the change may well be
  undone. **`OperationLocation.EffectiveLockKey`** is the key that actually applies (what the notes read)
  and **`OperationLocation.LockKeyFault`** says why one is ignored: `LocationIsManned`,
  `LocationExchangesNoCargo` or `HolderIsNotManned`. `Plan.GetValidationErrors` reports each ignored key
  as the new `ValidationErrorType.LockKeyIgnored` under the new `ValidationScope.Layout` — a conflict of
  the layout itself rather than of anything running on it, so it carries no track, time or train. The
  rule (**L4**) is always enforced, like the other checks for a model that contradicts itself, and needs
  no setting.

### XPLN Importer Improvements

- **A section two lines share is imported as one track stretch.** A line is listed in the Routes
  worksheet one section per row, so two lines running over the same section list it once each. The
  importer built a second `TrackStretch` for the repeat, gave it to the second line, and then had
  `Layout.Add` drop it as a duplicate — leaving that line running over a stretch the layout did not
  hold. A repeat that agrees with the existing stretch on direction, distance, tracks, speed and time
  now joins the second line to that same stretch. `FREMODERN-2023-Final-1-1`, whose two lines both leave
  Ing over the section to Wei, is the file this shows on.

- **A section defined twice with different data is an import error.** Where a repeated pair disagrees
  with what the layout already holds, or is defined the opposite way round, the file contradicts itself
  and there is no saying which of the two the layout should take, so the row is reported
  (`TrackStretchAlreadyExists`) rather than silently resolved. `Magdeburg_v_DB33_DSB32_WTB11` has such a
  pair: its Routes rows 26 and 29 both join Fgr and Pa, one over 1.4 km on three tracks and the other
  over 7.4 km on two.

- **A message now names the worksheet and the row number the spreadsheet shows.** A message read
  `Row 81: …`; it now reads `Trains row 87: …`. Both halves of that changed. The worksheet is new — a
  bare row number does not say which of the three to open. The row number was also wrong: the ODS reader
  does not carry a blank row into its table, and the importer's counter also passed over rows it
  skipped, so the number quoted was how many rows had been taken in rather than the row in the file, and
  it drifted further behind with every blank row above. `OdsDataSetProvider` now records the number the
  spreadsheet shows for each row it keeps, and every message quotes that. In `LTK2020` the drift was six
  rows: what was reported as row 81, an unrelated train's row, is row 87.

  The row prefix now lives in one resource per language (`WorksheetRow`) instead of being repeated in
  each of the eighteen row-scoped messages. The German prefix is `Zeile` — the term for a spreadsheet
  row — where the repeated form had said `Reihe`.

---

## Version 3.1.0

### New Features

- **A train stops only where it can exchange what it carries.** `Train.CanStopAt(location)` answers
  whether a train may stop at an operation location at all: never at a `SignalControlledLocation`,
  and elsewhere only where a passenger train finds `HasPassengerExchange` or a freight train
  `HasCargoExchange` (a category that is both needs either; one that is neither — empty stock, a
  light engine — is restricted by the location type alone). `StationCall.CanBeStop` asks it for a
  call, and **`StationCall.IsStop` now answers through it**, so the rule is applied everywhere at
  once, the same way the signal-controlled rule already was. The `IsArrival` and `IsDeparture` flags
  are never cleared: restore a location's exchange and any stop planned over it is there again.

  A **shadow station** (`Station.IsShadow`) now always has both exchanges, whatever the two
  properties were set to. It stands for everything beyond the modelled layout, so whatever a train
  brings there has somewhere to come from and go to.

- **`StopRules`** (in `Model.Validations`) holds the other half of the rule: a train part runs from a
  call the train departs to one it arrives at, so both ends must be stops. `Plan.IsDepartureRequired(call)`
  and `Plan.IsArrivalRequired(call)` say whether a flag is held up by the train's own run or by a part
  a vehicle schedule, a driver duty or a cargo flow is planned over — an editor uses them to disable
  the flag rather than let it be taken away. `Plan.ApplyStopRules()` sets what the parts need, clears
  nothing, and returns how many calls changed; it runs from `Plan.Reconcile()` and when a plan is
  read, so no reading path can forget it.

- **`TrainCategory.DefaultPreparationMinutes` and `TrainCategory.DefaultFinishingMinutes`** hold the
  preparation and finishing-up times, in minutes, that trains of a category are planned with (both
  default to 10). `Plan.Create` and the other creating methods take these as `int?` and fall back to
  the category's defaults when given `null`, in the same way `maxSpeed` already falls back to
  `TrainCategory.DefaultSpeed`.

  **`Timetable.ApplyDefaultPreparationMinutes(category)`** and
  **`Timetable.ApplyDefaultFinishingMinutes(category)`** give a category's default to the trains of
  that category which already exist, and return how many were changed. They are separate operations,
  so one time can be reapplied without touching the other. Underneath, **`Train.SetPreparationMinutes`**
  moves a train's origin arrival that many minutes before its departure and
  **`Train.SetFinishingMinutes`** moves its destination departure that many minutes after its arrival;
  nothing else moves, so the run itself is untouched. A preparation reaching back before midnight is
  refused rather than applied. **`Train.PreparationMinutes`** and **`Train.FinishingMinutes`** read the
  two times back, and **`Timetable.TrainsIn(category)`** gets a category's trains.

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

- **`Timetable.TrainCategories` is reconciled with the categories the trains use.** The new
  `RebuildTrainCategories(this Timetable)` adds every category a train holds that the catalogue does
  not know, then gives each category an id that is unique and greater than zero. A `Train` carries its
  `Category` as part of itself, so a plan written before the catalogue existed — or by an importer
  that did not fill it in — read back with trains that have categories and a catalogue that has none,
  and with every category left on the default id zero. A category is picked out by `Train.CategoryId`,
  so those categories were taken for one another and for the trains that have no category at all;
  `ValidateTrainNumbers` (rule T4) reported trains of different categories sharing a number as a
  duplicate identity.
  Run from `Timetable.OnDeserialized` alongside `RebuildStationCalls`, so every reading path gets it.
  `TrainCategory.Id` is settable for this, where it was init-only.

- **A catalogue entry is written only in its catalogue.** `Train.Category`, `Train.Company`,
  `TrainCategory.Company`, `ScheduledObject.Company` and `DriverDuty.Company` are no longer written to
  a plan (`PlanJson.WriteCatalogueEntriesOnlyInTheirCatalogue`); each is kept as its foreign key alone
  and put back on read by `Timetable.ResolveCatalogueReferences` and `Plan.ResolveCatalogueReferences`.
  `ReferenceHandler.Preserve` wrote each entry once already, but wherever the writer first met it —
  under the first train that used it — leaving `Timetable.TrainCategories` and `Layout.Companies` as
  lists of `$ref`. The catalogues are now declared, and so written, before what refers to them.
  As with `StationTrack.Calls`, only writing is turned off: a plan written by an earlier version
  defines its entries under a train and points everything else at that copy.

- **`Plan.RebuildCompanies`** reconciles `Layout.Companies` with the companies the trains, categories,
  vehicles and duties refer to, in the same way and by the same helper as `RebuildTrainCategories`.
  Companies left on id zero were already indistinguishable to anything reading `CompanyId` — trains of
  two such companies counted as one operator in `ValidateTrainNumbers`. A company with an id of its own
  (a Module Registry id) keeps it. `TrainCategory.CompanyId` is new, for the same reason.

- **A foreign key follows its navigation.** `Train.CategoryId`, `Train.CompanyId`,
  `TrainCategory.CompanyId`, `ScheduledObject.CompanyId` and `DriverDuty.CompanyId` now read
  `Navigation?.Id ?? stored`, so assigning the object is enough to keep the two in step. Several call
  sites set only the navigation, which would have silently dropped the reference once the key became
  the only thing written.

- **`Plan.Reconcile`** performs the whole sequence — rebuild the call index, resolve the catalogue
  references, reconcile both catalogues — for plans that arrive from an importer rather than from a
  reader. `Plan.OnSerializing` reconciles the catalogues before a plan is written, so no path can save
  a plan whose catalogue does not yet hold everything it uses.

- **A `Country` is stored as its id** (`CountryByIdConverter`) and read back through `Country.ById`.
  A country's name, languages and code belong to the catalogue in the code; copying them into every
  plan meant a correction could never reach a plan already saved. Reading still accepts the whole
  object an earlier version wrote, and falls back to its stored values for an id the catalogue no
  longer offers.

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
