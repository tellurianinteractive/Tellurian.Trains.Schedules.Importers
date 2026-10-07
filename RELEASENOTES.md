# Release Notes

## Unreleased

### New Features

- **Report languages.** New `ReportCultureExtensions` decide the culture a report, or one item of it, is
  printed in. `Layout.DefaultLanguage` is the first language of the layout's default country and
  `Layout.DefaultReportCulture` that language in that country (for example `de-CH`); `Country.PrimaryLanguage`
  and `Country.PrimaryCulture` give the same for any country. With `GeneralSettings.UseObjectLanguageInReports`
  set, `DriverDuty.ReportCulture(layout, isAvailable)` gives the language of the duty's company, else of its
  trains' operators when they all share one; `OperationLocation.ReportCulture(layout, isAvailable)` gives the
  language of the location's country; and `Layout.ReportCultureOf(company, operators, isAvailable)` is the
  company-then-operators rule for any other item. Each falls back to the default culture, and passes over a
  language the `isAvailable` predicate rejects, so an application states which languages it can print in.

- **A train category says where its trains stop.** New **`TrainCategory.StopLocationIds`** is the
  category's *stop pattern*: a positive list of the operating locations where its trains stop on their way.
  Building a route (`Plan.Create`, and so every creator built on it) gives a new train a stop at each
  location the pattern names and runs it through the rest, and **`Train.CheckStopPattern`** (rule T8,
  switched by `ValidationSettings.ValidateStopPatterns`) reports a stop the pattern does not name — leaving
  the planner to decide whether the train or the pattern is wrong.

  A pattern can only narrow: a location it names is still stopped at only where the train can stop there at
  all, and a shadow station is always a stop. An empty list is **no pattern rather than a pattern of
  nowhere**, so a category the planner has not got to yet behaves exactly as before. Reading a plan gives
  every category without a pattern the one its trains already run (`Plan.DeduceStopPatterns`, run by
  `Plan.Reconcile` and on deserialisation), so no plan is reported for a stop that was not a fault before;
  `Plan.DeduceStopPattern(category)` is the same for one category and replaces what is there.
  `TrainCategory.HasStopPattern`, `AllowsStopAt(location)` and `StopLocations(layout)` read it, and it is
  written as a plain array of ids, left out altogether where the category has none.

  New **`TrainCategory.CanStopAt(location)`** holds the rule `Train.CanStopAt` has always answered by — a
  train needs somewhere to hand over what it carries, and never stops at a signal-controlled location —
  asked of the category alone, which is what the locations a pattern may name are chosen from.
  `Train.CanStopAt` now delegates to it and is unchanged in what it answers.

- **A train category says what it exchanges, and can now exchange nothing.** New
  **`TrainCategory.Content`**, a `[Flags]` **`TrainContent`** of `None`, `Passenger` and `Cargo`, replaces
  the two booleans that said the same thing less exactly. `TrainContent.None` is a *service* category —
  a construction train, or a locomotive or trainset moved out of service — whose trains hand nothing over
  and so need no exchange of the locations they call at: `Train.CanStopAt` lets them stop wherever the
  location type allows, which is what makes a work site modelled as an `OtherLocation` a legal stop.

  `TrainCategory.IsPassenger` and `TrainCategory.IsFreight` remain, as extension properties over the
  content, joined by **`TrainCategory.IsService`**. `IsFreight` is true for a shunting category whatever
  its content says, so the pair can no longer disagree. Building a route (`Plan.Create`) gives a service
  train no stops between its ends — deliberately narrower than `CanStopAt`, since only the planner knows
  where the work site is.

- **A station says whether it has a turntable, and a train part says what is to be done with its
  traction on arrival.** New **`Station.HasTurntable`** records the facility, and
  **`OperationLocation.CanTurnLoco`** answers the question a caller actually asks — a location can be
  turned at only where it is a station with a turntable, since nowhere else has one.

  `TractionOptions.TurnLoco` and its neighbour are now settable from the planning app rather than only by
  an import, and the notes they generate (`TurnNote`, `CirculateNote`, `TurnAndCirculateNote`) are
  unchanged in text and in the rule that both flags together give one note, not two.

- **A runaround is asked for only where the traction actually needs one.** New
  **`ScheduledTrainPart.NeedsRunaround`** is false where every traction unit working the part reverses as
  it stands — a trainset, or a locomotive working a reversible train — and true where the part's traction
  cannot be resolved at all, so what was asked for stands until the vehicles say otherwise. It is the
  part-scope counterpart of the existing `Plan.NeedsLocoRunaround`, which answers the same question for a
  whole train and is what the timings allow the standing time from. `CirculateNote` and
  `TurnAndCirculateNote` are now generated through it, so a trainset is never told to run round; the flag
  itself is kept as set, and takes effect again as soon as traction that needs the move works the part.

- **A working whose traction cannot change tracks now arrives where its next train departs.** New
  **`Schedule.AlignArrivalTracks()`** puts the arrival call of each part of a working on the track the next
  part departs from, and the last arrival on the first part's departure track where the working ends where
  it began. It applies only where new **`Schedule.IsWorkedWithoutRunaround`** holds — every traction unit
  assigned to the schedule reverses as it stands (a trainset, or a locomotive on a reversible train) and at
  least one is assigned — because such a unit *is* the train and leaves from the very track it came in on,
  while a locomotive runs light to whatever track its next train stands on. Only arrival calls are moved,
  and a joint where the parts do not meet at one location is passed over.

  The guarded `Schedule.Append` and `Schedule.Insert` align the working they accept a part onto, so both
  automatic building (`Plan.BuildSchedulesAutomatically`, which chains through `Append`) and building by
  hand keep the tracks in step as the working grows. `Plan.AssignVehicle` aligns the schedule it assigns
  to, so a working built before its traction was known is put right when the vehicle is assigned. The
  unguarded `Schedule.Add` does not align, so an XPLN import and the clone and complement paths — which
  reference the origin's own trains — keep the tracks they were built with. A track thereby occupied twice
  over is left to the track-occupancy validation to report.

- **A train part can put its vehicles on another track than the train's.** New
  **`ScheduledTrainPart.FromTrack`** and **`ScheduledTrainPart.ToTrack`** name the track the part's vehicles
  are fetched from before it departs and put on after it arrives, where that is not the track the train
  uses. **`OtherFromTrack`** and **`OtherToTrack`** read them, counting a track only where it is another
  track of the location the part starts or ends at — so one left behind by a call moved elsewhere says
  nothing. `Schedule.EditPart` forgets a track on every part whose end it moves to another station.

  They generate the new **`FromTrackNote`** (departure) and **`ToTrackNote`** (arrival), both for the
  driver and the dispatcher and routed through `StationCall.VehicleNotes` like the rest of the vehicle
  family. Each names when the move is made and what kind of vehicle is moved, the kind written as the
  current language writes it inside a sentence (new `ClassNames.InSentenceFor`): en *"Before departure,
  fetch {0} {1} from track {2}."* / *"After arrival, shunt {0} {1} to track {2}."*, sv *"Innan avgång
  hämta vagnsätt … från spår …"* / *"Efter ankomst växla in vagnsätt … till spår …"*, de *"Vor Abfahrt …
  von Gleis … holen."* / *"Nach Ankunft … auf Gleis … rangieren."*, da *"Før afgang, hent …"* / *"Efter
  ankomst, rangér …"*, nb *"Før avgang, hent …"* / *"Etter ankomst, skift …"*. One note per vehicle of
  the part's **own** schedule, not per vehicle working the part, since a loco and a wagonset over the
  same calls would otherwise both be sent. A track note takes the place of the use, couple and
  from-parking notes at the departure and of the uncouple and to-parking notes at the arrival; turning
  and circulating notes are still given.

  A wagonset's track notes are for the dispatcher only (`IsDriverNote` false): the driver duty booklet's
  wagonset block already gives each wagonset's tracks. The loco driver instead gets one of the new
  **`ShuntWagonsToDepartureTrackNote`** / **`ShuntWagonsToArrivalTrackNote`** (abstract base
  **`ShuntWagonsNote`**, driver only), naming neither wagonset nor track: en *"Shunt wagons to departure
  track before departure."* / *"Shunt wagons to their arrival track after arrival."*, sv *"Växla vagnar till
  avgångsspåret före avgång."* / *"… till deras ankomstspår efter ankomst."*, de *"Wagen vor Abfahrt zum
  Abfahrtsgleis rangieren."* / *"Wagen nach Ankunft zu ihrem Ankunftsgleis rangieren."*, da *"Rangér vogne
  …"*, nb *"Skift vogner …"*. One per part for all the wagonsets of its schedule, and `VehicleNotes` says
  each kind once for all the schedules shunting wagons at the call. Its `Sessions` are those any of the
  wagonsets is shunted on, null where together that is every session the train runs — new internal
  `IEnumerable<Sessions?>.SharedPartOf(whole)` combines the per-vehicle qualifiers.

  Each note compares the vehicle's assignment to the schedule with the sessions the train runs. Where the
  vehicle works the train on only some of them, the note's `Sessions` holds those and the text is led by
  them — *"1,3,5: Before departure, fetch …"*, as session circles in the markup or as day names on a
  layout counting in
  days — through the new `NoteResources.OnSessions` (`"{0}: {1}"`). Where it works every session the train
  runs, `Sessions` is null and nothing is added; where it works none of them, there is no note for that
  vehicle. The sessions are written with the plan's own `GeneralSettings.SessionSettings()`. New internal
  `Sessions.SharedPartOf(whole)` does the comparison, ignoring the on-demand marker.

  `StationCall.TrackOccupancy` extends a call's occupancy by the unit's stay only where the unit is left on
  the call's track and fetched from it again, so a unit put on or fetched from another track no longer
  holds this one. New **`DeletionRules.MayDelete(StationTrack)`** / **`TryDelete(StationTrack)`** refuse a
  track any train calls at or any part names; `TryDelete` also clears a part still holding the deleted track
  where it no longer counted. `ScheduleDbContext` maps both tracks as optional references (shadow keys
  `FromTrackId`, `ToTrackId`).

- **A note now says which half of its call it belongs to.** New **`CallNoteTarget`** (`Arrival`,
  `Departure`) and **`ManualNoteRules`** say what a call offers: `StationCall.ManualNoteTargets` is both
  halves where the train arrives and departs, one where it does only one of them, and none at a
  pass-through, where the train stands for neither. `StationCall.SetManualNote` takes the half to write
  for, falls back to `StationCall.DefaultManualNoteTarget` (the departure wherever there is a choice) and
  writes nothing where the call offers no half at all. Clearing a note still works everywhere, so one
  left behind by a call that has since become a pass-through can be taken away.

  A note naming neither half was printed nowhere the arrival and the departure are told apart — the
  station dispatch lists and the driver duty booklets — which is what every note written before this,
  and every call remark the XPLN import carries over, looked like.
  **`Plan.ApplyManualNoteTargetRules`** gives such a note the half its call implies. It runs when a plan
  is read and from `Plan.Reconcile`, so an older plan and a fresh import are both put right on the way
  in. A note at a pass-through is left as it is: the single row a pass-through prints as shows the notes
  it carries whole.

- **A plan records who brings its rolling stock.** New **`Plan.Participants`**, a catalogue of
  **`Participant`** (id and name), and **`Plan.VehicleContributions`**: per rolling stock item a
  **`VehicleContribution`** tied to its vehicle by `ScheduledObjectId`, with a note and an ordered list of
  **`VehicleContributor`** — the first the primary one, who brings the unit and sets it up, every later one
  bringing a spare. A contributor names its participant by `ParticipantId`, and carries a note and a
  `DccAddress`: required for every contributor of a traction unit, spares included, where
  **`DccAddresses.ToBeProvided`** (0) says the owner is still to provide it, and absent for a wagonset.
  Both collections are written with the plan and read back empty from a plan saved before they existed.

  **`VehicleOwnershipExtensions`** holds the operations: `ScheduledObject.IsRollingStock` (locomotive,
  trainset or wagonset), `NeedsDccAddress` and `AcceptsDccAddress`; `Plan.FindOrAddParticipant`,
  `RenameParticipant` (refusing a blank name or another participant's) and `ParticipantsMatching`, which
  matches the start of a name or of any later word in it, whole-name matches first; `Plan.AddContributor`
  (refusing an address the vehicle does not accept), `MakePrimary`, `SetDccAddress`,
  `SetContributionNote` and `RollingStockBroughtBy`. A contribution left with no contributor and no note is
  removed. **`ScheduledObject.Start(useDays, maxSessions)`** gives a **`VehicleStart`**: the first session or
  day the vehicle is in operation and the first train part it works on it, whose start is where the vehicle
  is to stand, and whether it works on every session or day of the operating period. `DeletionRules` gains `MayDelete`/`TryDelete` for a `Participant` (refused while they bring
  rolling stock) and a `VehicleContributor` (the first spare taking a removed primary's place). In
  `ScheduleDbContext` both collections are JSON columns on the plan.

- **Automatic schedule building offers the work to the schedules already there before it starts new
  ones.** `Plan.BuildSchedulesAutomatically` now gives the unassigned trains to the plan's own schedules
  first: each working is chained on with the unassigned trains of the category it currently works, and a
  schedule with no parts is seeded as a new one would be. A vehicle that is already turning therefore takes
  on more work before another is put in service, and only the trains that fit no existing schedule create
  schedules of their own. A cargo-flow schedule is left as it is, being the circulation of a consignment
  rather than of a turning vehicle, and a category flagged `ExcludeFromAutomaticScheduling` is now skipped
  when chaining as well as when seeding — which it could not be before, when only a seed was ever excluded.

  The build reports what it did as a **`ScheduleBuildResult`**: the schedules `Created`, the existing ones
  `Extended`, and the number of `AddedParts` worked into them.

- **Session numbers and short day names are written without spaces.** `Sessions.DaysShort` and
  `SessionsFormatting.ToText`/`ToHtml` now join a scattered value with a bare comma — *1,3,5*, *Mo,We,Fr*,
  sv *M,O,F* — where they wrote *1, 3, 5* and *Mo, We, Fr* before, so a value takes no more room than it
  must. `Sessions.SessionsNumbers` and the turnus cards already wrote them this way, so the renderings now
  agree. Only day names written out in full keep the comma and space of a sentence.

- **A display name can be written inside a sentence.** New **`ClassNames.InSentence(name)`** and
  **`ClassNames.InSentenceFor(value)`** give the localised name with a lower-case first letter, except in
  German, which capitalises its nouns wherever they stand. The track notes name the kind of vehicle
  through it. The Swedish name for a wagonset is now *Vagnsätt*, the word the application itself uses.

- **Passenger interchanges: where passengers change trains, and the note that says so.** New
  **`GeneralSettings.UsePassengerTickets`** says the meeting is worked with passenger tickets, and new
  **`Station.IsPassengerInterchange`** marks a station as one of the places passengers change at. Both
  have to hold, which is what **`OperationLocation.ExchangesTransferringPassengers`** answers, so no
  caller has to combine them; **`Layout.PassengerInterchanges`** lists the stations that qualify, and is
  empty where the tickets are not in use. **`OperationLocation.CanBePassengerInterchange`** says where the
  mark may be set at all — a station that exchanges passengers.

  A passenger train that stops at an interchange gets a new arrival note,
  **`ExchangeTransferringPassengersNote`**, from **`StationCall.PassengerInterchangeNotes`** and hence from
  `StationCall.DriverNotes`. A train that only passes, and one carrying no passengers, gets nothing. Turn
  the tickets off and the station marks are kept but nothing is derived from them, exactly as with a lock
  key that is out of force.

- **A train composition places cargo flow wagons at two levels.** The Train compositions report drew one
  rectangle per cargo flow position, gathering every destination of every flow there into it. A cargo flow
  says where its wagons stand in the train, and each of its destinations says where they stand within that,
  so wagons standing in different places were drawn as one unit. **`CargoPositionComposition`** now carries
  **`DestinationPosition`** beside `Position`, and one is built per pair: the groups are ordered by the
  flow's position first and by the destination's within it second, with position zero — anywhere — last at
  each level, and each pair is drawn as a rectangle of its own. Destinations sharing both positions still
  share one rectangle, whichever flows name them, since they are one unit of wagons; a flow to all
  destinations names no destination and so stands alone at its position as before.

- **A destination can be written without its regions, so several can share them.** Where destinations are
  listed together, as the Train compositions report does for one position in a train, writing each with its
  own regions repeats a region for every place in it. New **`PlaceTextWithoutRegions`** and
  **`PlaceHtmlWithoutRegions`** give the named locations and the *and beyond* qualifier alone, and
  **`StatedRegions`** gives the regions the destination states — its station's regions where `AndRegions`
  is set, else none — so a caller can list all the places first and each region once after them.
  `PlaceText` and `PlaceHtml` are unchanged.

- **Stretch conflicts are judged per dispatch stretch.** Rule L3 (`ValidationSettings.ValidateStretches`,
  still reported as `TrackStretchConflict`) now asks what the dispatchers at the two ends of a dispatch
  stretch would allow, instead of looking at one track stretch at a time. A dispatch stretch is divided into
  sections at its control points (`IsControlPoint`): the signal-controlled locations inside it, such as block
  posts. A section holds as many trains as it has tracks, both directions together.
  On single track, trains in opposite directions may not be at once between two places where they can meet
  (`AllowsMeets`: a dispatch endpoint, or a signal-controlled location where trains can cross), while trains in the same
  direction may follow each other one per section. A train passing a control point without a call there
  holds every section it passes. So two trains crossing at an unmanned station nobody controls, or following
  each other past one, are now reported; the per-track-stretch check missed both. Track stretches outside
  any dispatch stretch (all of them where a layout has none recorded) are judged one by one as before.

- **Locations dispatched from afar.** **`OperationLocation.ControlledBy`** replaces
  `SignalControlledLocation.ControlledBy`, and is in force where the new **`CanBeControlled`** holds: a
  signal-controlled location, an industrial area or an unmanned, non-shadow station. Every controlled
  location is **`IsRemotelyDispatched`** — the controlling station's dispatcher clears its trains as their
  own — except a block post (**`IsBlockPost`**: a signal-controlled location that is neither a junction,
  **`IsJunction`** being more than two track stretches meeting there, nor a crossing place). So controlled
  junctions and crossing places are dispatched remotely, and block posts stay part of the line. New
  **`SignalControlledLocation.TrainsCanCross`** says where trains can cross; it is stated rather than
  judged from the tracks, since a junction has two tracks whether or not trains can cross, and the XPLN
  import leaves it off. **`IsDispatchEndpoint`** now covers these as well as the stations with a
  dispatcher of their own (the new **`HasDispatcher`**), so `CreateDispatchStretches` ends dispatch
  stretches there, and **`Dispatcher`** gives the station whose dispatcher clears trains at a location.
  **`Layout.Dispatchers`** lists the stations with a dispatcher, which are the ones that get a dispatch list,
  and **`Layout.LocationsDispatchedBy(station)`** gives a station and the locations it works.
  `DispatchNeighboursOf` now names the dispatcher beyond every location the station works, so a controller
  rings the stations beyond its junction or crossing place and they ring the controller. `Layout.ChangeOperationLocationType`
  keeps the controller across a type change. The JSON property name is unchanged, so saved plans read as
  before; in `ScheduleDbContext` the `ControlledByStationId` foreign key moves from the signal-controlled
  locations to all operation locations.

### Fixes

- **A station with many tracks no longer draws over the next station in the graphical timetable.** The
  minimum spacing between two operation locations on the distance axis — `GraphicTimetableSettings.StationSpacing`
  on screen, `PrintStationSpacingMm` in the printed report — was measured from the first track of one location
  to the first track of the next, with the location's own tracks fanning out inside it. A station with more
  tracks than that spacing allowed for therefore drew them over the tracks of the following location. The
  minimum is now a floor on the gap alone — from the last track of one location to the first track of the next —
  so it means the same whatever the locations are made of, and a station with many tracks pushes the next one
  further away instead of into it. Spacing taken from the real distance is unchanged wherever it already
  exceeds the minimum, and a printed graph that grows taller than a sheet is squeezed by the same rule as
  before.

- **Times after midnight are stored on the next day.** With `GeneralSettings.RunsOverMidnight` set, a train
  running past midnight keeps its later times on the next day (24:10 as `1.00:10`), which is what keeps them
  after the times before midnight wherever calls are ordered by time. New `Timetable.PlaceTimesAfterMidnight()`
  restores this for plans that hold such times on the first day: a train runs for less than a day, so the
  longest gap on the clock is when it does not run, and every time before the end of that gap moves to the next
  day. It is idempotent, does nothing unless the layout runs over midnight, and runs on deserialisation and in
  `Plan.Reconcile`.

### Breaking Changes

- **`SignalControlledLocation.ControlledBy` moved to `OperationLocation`.** Source using it through a
  `SignalControlledLocation` still compiles; code that pattern-matched on the signal-controlled type to
  reach it can now read it from any location, and should check `CanBeControlled`.

- **A dispatch endpoint need not be a station.** `DispatchStretch.From` and `To`, and the constructor's
  endpoints, are `OperationLocation` rather than `Station`, and `Layout.DispatchEndpoints` yields
  `OperationLocation`. Code that used `DispatchEndpoints` for the stations that get a dispatch list should
  use `Layout.Dispatchers`. Regenerate the dispatch stretches of a layout with controlled locations to get
  the new endpoints.

- **`TrainCategory` compares by `Id`.** The record's field-by-field equality is replaced by the same
  identity the rest of the plan picks a category out by, as `Layout`, `Plan`, `Timetable` and `Schedule`
  already do. A collection member — the new stop pattern — would otherwise have broken value equality
  silently, two categories holding equal but separate lists comparing unequal because a list is compared
  by reference. Categories in one timetable have unique ids greater than zero
  (`Timetable.RebuildTrainCategories`), so within a plan nothing changes; a caller comparing two
  categories of different plans, or two unreconciled ones sharing an id, now gets identity rather than a
  field-by-field answer.

- **`StationCall.SetManualNote` takes the half of the call the note is written for.** The signature is
  now `SetManualNote(string? text, CallNoteTarget? target = null, string? languageCode = null)`, so a
  caller that passed the language positionally must name it: `SetManualNote(text, languageCode: "sv")`.
  A null target leaves the choice to the call, which is what the previous signature always did.

- **`TrainCategory.IsPassenger` and `TrainCategory.IsFreight` are no longer settable.** Both are now
  read-only extension properties over `TrainCategory.Content`; set the content instead
  (`TrainContent.From(isPassenger, isCargo)` states it as the old pair). Reading either is unchanged,
  except through the null-conditional operator, which extension members do not allow: write
  `train.IsPassenger` rather than `train.Category?.IsPassenger`.

  A plan written by an earlier version stores the two booleans and reads back into the content, so
  nothing is lost. A category with neither set becomes a service category — which is what it already
  meant, there being no other way to say it. The train category catalogue CSV read by
  `TrainCategoriesFromCsvService` keeps both columns and gains an `IsShunting` column, so a file written
  for the previous format needs that column adding.

- **`TractionOptions.ReverseLoco` is renamed `TractionOptions.RunaroundLoco`**, after the manoeuvre the
  rest of the model and the app already name that way (`Plan.NeedsLocoRunaround`,
  `StationTimings.LocoRunaroundRealMinutes`). Nothing but the name changed. A plan written by an earlier
  version stores the flag under its old name and reads back without it; the XPLN importer does not set
  it, so only a plan that had it set by hand is affected.

- **`Plan.BuildSchedulesAutomatically` returns a `ScheduleBuildResult`** rather than
  `IReadOnlyList<Schedule>`, now that a build can extend schedules as well as create them. A caller that
  wants what it got before reads `.Created`.

### Dependencies

- **Tellurian.Localization is raised to 1.5.0.** Its markdown providers now take the language the
  suffixless content files are written in, and stop looking for a culture-specific file for that
  language — a lookup that could only ever miss. Nothing in this model's own API changes.

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

- **A station track says where the platform is.** The new **`StationTrack.PlatformLength`** is the length
  in metres, to one decimal, of the platform along a track, and zero where there is none.
  **`StationTrack.HasPlatform`** is that length above zero, and **`StationTrack.HasPassengerExchange`**
  is what decides whether passengers get on and off there: the location must exchange passengers
  (`OperationLocation.HasPassengerExchange`) *and* the track must have a platform.

  Nothing stops a passenger train standing at a track without one — `Train.CanStopAt` is unchanged and
  still asks the location alone, which is what makes a meet at a location that exchanges no passengers
  an ordinary stop. What the platform decides is *where* a train is put: `Plan.Create` now gives a
  passenger train a scheduled track with a platform, the main one of them for choice, and falls back to
  the scheduled main track as before where the location has none.

  A plan written before platforms existed has none recorded, and every track of a passenger location
  served passengers then. **`Layout.EnsurePlatforms()`** — and **`OperationLocation.EnsurePlatforms()`**
  for one location — therefore gives every track of a location that exchanges passengers the minimum
  **`StationTrack.DefaultPlatformLength`** of one metre, but only where not one of its tracks has a
  platform already; a location where the planner has recorded one is left as it stands, and so is a
  location that exchanges no passengers. It runs when a timetable is read and from `Plan.Reconcile()`, so
  every reading and importing path gets it, and it is idempotent.

  **A passenger train that stops to exchange passengers must stand at a platform (validation rule T6).**
  The new **`Train.CheckPassengerExchange()`** reports a call where the train carries passengers, the
  location exchanges them, the call is an arrival and/or a departure, and the track has no platform. A
  call that is neither an arrival nor a departure is not reported at all — a passenger train standing at a
  platformless track for a meet is exactly that — and neither is a location that exchanges no passengers.
  The error type is the new `ValidationErrorType.PassengerExchangeWithoutPlatform` (train scope,
  warning severity), with the message `TrainStopsForPassengerExchangeWithoutPlatform` in all five
  languages, and the rule is gated by the new `ValidationSettings.ValidatePassengerExchange` (default
  **true**). Nothing is repaired automatically: giving the track a platform and clearing the call's stop
  flags are both valid answers, and only the planner knows which.

  The rule judges a **reconciled** plan, as every consumer validates one. An importer builds a plan
  rather than reading one — and XPLN records no platforms at all — so call `Plan.Reconcile()` on an
  imported plan before validating it, or every passenger stop in it is reported.

- **The Timetable report lists only the locations trains stop at.** A row used to be printed for every
  operation location along a timetable stretch, whether anything stopped there or not, so a signal
  controlled location ran down the table as a column of pass-through marks and a station every train
  runs past took a row to say nothing. A location now earns its row when at least one train running the
  stretch stops there (`StationCall.IsStop`), and one train is enough — the list grows by itself as
  stops are planned. A signal controlled location is never a stop for any train, so it is never listed.

  Both directions are judged together, so a stretch's up and down tables still list the same locations
  and can be read side by side. The trains that run a stretch are now public as the new
  **`TimetableStretch.RunningTrains(trains)`**, read the same way the graph reads a train's direction.
  Nothing else changes: a train passing a location where another stops still shows its pass-through
  mark, and the connection rows that show where trains start and end on a neighbouring stretch are
  unaffected.

- **A circulated or turned locomotive now says so, to the driver and to the dispatcher.**
  `TractionOptions.ReverseLoco` and `TurnLoco` were recorded and imported but produced no note at all.
  They now generate **`CirculateNote`** ("Circulate locomotive."), **`TurnNote`** ("Turn locomotive.")
  and, where a part asks for both, the single **`TurnAndCirculateNote`** ("Turn and circulate
  locomotive.") rather than two notes for what is one errand — the loco leaves the train, goes to the
  turntable and comes back on the other end. All three are arrival notes and are read by driver and
  station alike, in all five languages.

  They name no vehicle and explain no procedure. A note on an arrival needs no *after arrival*, the
  locomotive is the one standing in front of the driver, and circulating is a manoeuvre every loco
  driver can already do. That also makes it one note per train part rather than one per traction unit,
  which is what double-headed traction wants: the whole consist circulates once.

- **Everything a vehicle schedule says is done with its vehicles now reaches the two people who have to
  do it.** The notes generated from a train part's options — `UseNote`, `CoupleNote`, `UncoupleNote`,
  the two parking notes and the new turning ones — were built on `ScheduledTrainPart` and read by
  nothing: the driver duties report and the station dispatch list both assemble a call's notes through
  `StationCall.DriverNotes` / `StationNotes`, which knew only the persisted notes, the stop, lock key
  and meeting notes. Both now include the new **`StationCall.VehicleNotes(plan)`**, which collects the
  arrival notes of every scheduled part ending at that call and the departure notes of every part
  beginning there.

  Nothing on a station call leads back to the schedules that work it, so the plan holding them is
  passed in — a new optional last argument on `DriverNotes`, `StationNotes`, `DispatchRow.Build` and
  `DispatchList.Create`. Omitting it keeps the previous behaviour exactly: a timetable is readable
  before a single vehicle has been scheduled, and there is then nothing to say about the vehicles.
  Identical notes are collapsed, so two schedules working the same part do not repeat an instruction
  that names no vehicle. **`ScheduledTrainPart.GeneratedArrivalNotes`** and
  **`GeneratedDepartureNotes`** expose what the part generates without the call's own persisted notes,
  which is what keeps every hand-written note from printing twice; `ArrivalNotes` and `DepartureNotes`
  still return both families together as before.

### Fixes

- **A locomotive overlap is now reported against the two locomotives that are actually doubled.** Rule
  **P4** finds a train hauled twice over one stretch, but the error it raised named neither locomotive
  and was keyed to the train alone. `ValidationError.Involves(Schedule)` therefore fell back to marking
  every schedule holding any part of that train — so a locomotive working the same train hours away, on
  a leg nothing doubles, was marked for a conflict none of its own parts are in. The error now carries
  the two schedules the offending parts belong to, and marks only those.

  The message states which locomotives they are. It read `Train GD 42754 has overlapping locomotive
  assignments: 'GD 42754' Fullerup Terminal 14:51->Fullerup 15:05 and 'GD 42754' …` — the train named
  three times over and the locomotives not at all, the two halves often identical to the minute. It now
  reads `… : MZ 5 Fullerup Terminal 14:51->Fullerup 15:05 and MZ 5 …`. The five resource strings are
  unchanged; what is substituted into them is. The new
  `ScheduledTrainPart.TractionWorkingSpanText` composes that form, resolving the traction through the
  part's own `Schedule` rather than through `TractionUnits`, because a part is equal to any part over the
  same two calls and two schedules covering one leg each would otherwise both name both locomotives.

- **A locomotive rotation is no longer reported as a locomotive overlap.** **P4** compared the two
  workings' times and nothing else, so one locomotive taking a train on the odd sessions and another on
  the even — never at the meeting on the same day, and the whole point of a rotation — was reported as
  the train being hauled twice over. Two parts must now overlap in **sessions** as well as in time. The
  sessions a part is hauled on are the schedule's traction bookings unioned, narrowed to the sessions the
  train itself runs; an on-demand train runs on no numbered session and so has nothing to narrow by,
  leaving the bookings to stand on their own.

  The schedules are also grouped rather than taken one assignment at a time, so a working two
  locomotives share stays one claim on the train: **double heading is not double booking.**

  Where the doubling is confined to some sessions, the message now names them — the new
  `TrainHasLocomotiveCoverageOverlapOnSessions` in all five languages. Doubled on every session the
  train runs, the plain string is used, since `on sessions All` says nothing the rest of the message
  does not.

- **`ValidationError.Schedule` is now `ValidationError.Schedules`, a list.** A conflict between two
  schedules could not be attributed to both, which is what let the locomotive overlap above fall back to
  matching by train. The rules that key to a single schedule (**S1**, **S2**, orphan **S4**) fill it with
  that one schedule and behave exactly as before; an empty list still means a train-keyed schedule-scope
  error, located through `Trains`.

- **A region is written only in the layout's region catalogue.** `Station.Regions` was where a region
  ended up being stored: `Layout.Regions` was declared *after* `Layout.OperationLocations`, so the writer
  met each region under the first station using it, wrote it whole there, and left the catalogue — the
  one place a region belongs — as a list of `$ref`. Every other station wrote an empty list of its own on
  top of that, on a property nearly no station uses.

  The catalogue is now declared, and so written, before the locations, and a station keeps just the ids:
  the new **`Station.RegionIds`**, left out entirely where a station has no regions. `Layout.Regions` is
  unchanged as the catalogue, and `Station.Regions` still holds the catalogue's own objects — it is only
  how the association is stored that changed. This follows exactly what `Train.Category`, `Train.Company`
  and the rest already do (version 3.1.0).

  Supporting members: **`Layout.ResolveCatalogueReferences()`** puts each station's regions back from the
  ids after a read, called from `Timetable.ResolveCatalogueReferences` so no reading path has to remember
  both; **`Layout.RebuildRegions()`** reconciles the catalogue with the regions the stations hold before
  a plan is written, so a region reaching a station some other way is added to the catalogue rather than
  written nowhere at all, and no two regions are left sharing an id. The ids are written as a plain JSON
  array rather than the `$id`/`$values` object `ReferenceHandler.Preserve` wraps every collection in.

  As with the other catalogues, only writing changed: a plan written by an earlier version stores its
  regions under the stations and still reads exactly as it did.

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
