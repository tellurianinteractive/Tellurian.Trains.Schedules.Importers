# Survey: what the Dispatching layer must contain

Written 2026-11-20. Read-only survey: nothing was changed, built or run in either repository. It builds on `Documentation/CrossAppReview.md` (section 0 is the direction; sections 1–6 are not repeated) and `Documentation/ModelHarmonisation.md` (decisions §9, progress §10).

Paths: `S\` = `C:\Utveckling\Github\Tellurian.Trains\Tellurian.Trains.Schedules\`, `X\` = `C:\Utveckling\Github\Tellurian.Trains\Tellurian.Trains.Dispatch\`. Inside `X\`, `Core\` = `Tellurian.Trains.Dispatch\`, `Data\` = `Tellurian.Trains.Dispatch.Data\`, `Server\` = `Tellurian.Trains.Dispatch.Server\`, `Shared\` = `Tellurian.Trains.Dispatch.Shared\`. Facts have `path:line`; what I inferred without running it is marked *(unverified)*.

State of the Dispatch working copy: the `.Api` project is deleted and `.Server` is untracked (`git status`), version 1.1.0 on .NET 10 (`X\Directory.Build.props:4,7`). There is no client project: the Blazor client of `X\SPECIFICATION.md:79` does not exist.

## 0. Summary

1. **Most of Dispatch's layout model is already in the model here, and better.** Dispatch endpoints, remote dispatching, block posts, control points, who dispatches a location and who its neighbours are, are all defined once in `S\Model\Layouts\Layout.cs:575-717`. Dispatch's `OperationPlace` hierarchy, `StationTrack`, `TrackStretch`, `DispatchStretch`, its path finding and its `IDispatcher` can go without replacement types.
2. **Two things are missing from Layouts**: the tracks of a line with their directions (only `TracksCount` today), and a stable id for a dispatch stretch (today it is the id of its first track stretch).
3. **Three things are missing from Timetables**: a mark on a train created during dispatch, a public way to ask whether a train runs on session *n*, and the train's runs over the dispatch stretches as a public type (it exists, privately, inside the L3 validation).
4. **The Dispatching layer is small**: an operation, its operating sessions, a train's run on a session with its state, the run's sections with their state, what was observed at each call, and an append-only log of actions. The two state machines move here unchanged in substance, with five corrections (section 3).
5. **Dispatch persists nothing today.** The server registers the in-memory state provider and starts with `isRestart: false` (`X\Server\Program.cs:11,31`). The CSV providers exist but are not used by the server. So there is no stored state to migrate.
6. **Several rules Dispatch documents are not implemented**: capacity is never checked or occupied, observed times are never recorded, no action knows which dispatcher performed it, the fast clock is never asked. They are listed in 1.6 so that they are designed here once rather than carried over.
7. **The database needs three things before dispatch tables are added**: a version, a guard so that an export cannot wipe an operation, and a way to bring a corrected plan in without losing state.

## 1. What Dispatch has

### 1.1 Layout (domain; all of it is replaced by Layouts)

| Dispatch type | What it holds | Path | Here |
|---|---|---|---|
| `OperationPlace` (abstract record) | `Id` (generated when ≤ 0), `Name`, `Signature`, `Tracks`, `IsControlled`; equality by id | `X\Core\Layout\OperationPlace.cs:12-38` | `OperationLocation` (`S\Model.Layouts\OperationLocation.cs:30`) |
| `Station` | `Dispatcher` (set by the broker), `IsManned` (default true), `PreferredLanguage` | `X\Core\Trains\Station.cs:12-28` | `Station`, `HasDispatcher` (`S\Model\Layouts\Layout.cs:575`); language from the country, as the planner's reports do |
| `SignalControlledPlace` | `ControlledByDispatcherId`, `ControlledBy`, `IsJunction`, `CanHostMeeting` = more than one track | `X\Core\Layout\SignalControlledPlace.cs:34-49` | `SignalControlledLocation.TrainsCanCross` (`S\Model.Layouts\SignalControlledLocation.cs:19`), `ControlledBy` (`OperationLocation.cs:132`), `IsJunction` derived (`Layout.cs:624`) |
| `OtherPlace` | `IsJunction`, `RequiresCascadingOccupancy` (always true) | `X\Core\Layout\OtherPlace.cs:30-43` | `OtherLocation`; cascading is what a control section already is (`S\Model\Validations\DispatchStretchValidation.cs:13-19`) |
| `StationTrack` (record, no id) | `Number`, `MaxLength`, `IsMainTrack`, `DisplayOrder`, `PlatformLength` (all `int?`) | `X\Core\Trains\StationTrack.cs:7-32` | `StationTrack` with `Id`, `Designation`, `Length`, `IsMain`, `DisplayOrder`, `PlatformLength` (`S\Model.Layouts\StationTrack.cs:45-75`) |
| `TrackStretch` | `Start`, `End`, `Length` (metres, `int?`), `CssClass`, `Tracks`, `ActiveOccupancies` | `X\Core\Layout\TrackStretch.cs:16-55` | `TrackStretch` (`S\Model.Layouts\TrackStretch.cs:60`); the tracks are missing (2.1); `CssClass` is user interface |
| `Track` | `Number`, `Direction`, `IsUpTrack` | `X\Core\Layout\Track.cs:9-16` | Missing: `LineTrack` (2.1) |
| `TrackOperationDirection` | `ForwardOnly`, `BackwardOnly`, `DoubleDirected`, `Closed` | `X\Core\Layout\TrackOperationDirection.cs:6-27` | Direction is layout (2.1); `Closed` is state during operation (2.3) |
| `DispatchStretch` | `From`, `To` (both `Station`), `TrackStretches` found by breadth-first search at construction, `Forward`/`Reverse`, `IntermediateControlPoints`, `ActiveTrains` (unused), `CssClass` | `X\Core\Layout\DispatchStretch.cs:23-82`, `DispatchStretchExtensions.cs:54-105` | `DispatchStretch` with its stretches stored in order (`S\Model.Layouts\DispatchStretch.cs:6-50`) |
| `DispatchStretchDirection`, `StretchDirection` | One direction of a dispatch stretch | `X\Core\Layout\DispatchStretchDirection.cs:10-56`, `StretchDirection.cs:3-7` | Becomes `Direction` of a section (2.3) |
| `TrackStretchCapacityManager`, `TrackStretchOccupancy` | Occupy, release, cascade over `OtherPlace` | `X\Core\Layout\TrackStretchCapacityManager.cs:8-113`, `TrackStretch.cs:119` | Never called (1.6); replaced by the control-section rule of L3 |

### 1.2 Trains and calls (domain; replaced by Timetables)

| Dispatch type | What it holds | Path | Here |
|---|---|---|---|
| `Train` (record) | `Company`, `Identity(Prefix, Number)`, generated `Id`, `State`, `PreviousState`, `MaxLength` | `X\Core\Trains\Train.cs:5-31`, `Identity.cs:3`, `Company.cs:3` | `Train` (`S\Model\Timetables\Train.cs:17`); state moves to `TrainRun` (2.3) |
| `TrainStationCall` (record) | `Train`, `At`, `Scheduled` (`CallTime`), generated `Id`, `PlannedTrack`, `NewTrack`, `Observed`, `IsArrival`, `IsDeparture`, `SequenceNumber` | `X\Core\Trains\TrainStationCall.cs:6-22`, `CallTime.cs:3-7` | `StationCall` (`S\Model\Timetables\StationCall.cs:15`); `NewTrack` and `Observed` move to `ObservedCall` (2.3) |
| `TrainSection` | Generated `Id`, `StretchDirection`, `Departure`, `Arrival`, `State`, `Previous`, `CurrentTrackStretchIndex` | `X\Core\TrainSection.cs:13-141` | Derived from the plan (2.2), state stored (2.3) |

### 1.3 Dispatch state, actions and roles (domain; this is the new layer)

- **`TrainState`**: `Planned`, `Manned`, `Running`, `Canceled`, `Aborted`, `Completed` (`X\Core\Trains\TrainState.cs:3-11`).
- **`DispatchState`** of a section: `None`, `Requested`, `Accepted`, `Rejected`, `Revoked`, `Departed`, `Arrived`, `Canceled` (`X\Core\DispatchState.cs:6-24`). It also returns colour names, which is user interface (`:34-40`).
- **`DispatchAction`**: `Request`, `Accept`, `Reject`, `Revoke`, `Depart`, `Pass`, `Arrive`, `Clear`, `AssignDriver`, `Start`, `Cancel`, `Abort`, `Complete`, `UndoTrainState` (`X\Core\DispatchAction.cs:7-84`).
- **`ActionContext`**: section, action, dispatcher, pass target; its `DisplayName` is English text built in the domain (`X\Core\ActionContext.cs:14-28`).
- **`ActionStateMachine`**: which actions a dispatcher has for a section (`X\Core\ActionStateMachine.cs:11-200`). **`ActionContextExtensions.Execute`**: what an action does (`X\Core\ActionContextExtensions.cs:15-196`).
- **Roles**: `IDispatcher` (`Id`, `Name`, `Signature`) and `StationDispatcher`, one per `Station`, with the station's id (`X\Core\IDispatcher.cs:5-23`, `StationDispatcher.cs:6-15`, `Brokers\Broker.cs:57`). The role of a dispatcher for a section is worked out: departure dispatcher = dispatcher of `From`, arrival dispatcher = dispatcher of `To`, signal controller = `ControlledBy` of the next control point (`ActionStateMachine.cs:60-64,139-143`). There is no person, no sign-in and no shift.
- **`Broker`**: singleton that loads everything, builds the sections and answers "actions for dispatcher" (`X\Core\Brokers\Broker.cs:15-122`). Domain orchestration; the loading half goes, the query half becomes model functions.
- **Clock**: `ITimeProvider.GetTimeStatusAsync` returning `TimeStatus(Time, IsRealTime, IsRunning, Message)` (`X\Core\ITimeProvider.cs:6-24`); the fast clock client reads `GET /api/clocks/{name}/time` and parses "HH:MM" into a `TimeSpan` with no day (`X\Data\TellurianFastClockTimeProvider.cs:40,72,76-93`). App integration, apart from the small contract.

### 1.4 Persisted state (Data project)

- **Interfaces**: `ITrainStateProvider` (train state, observed arrival and departure, track change), `IDispatchStateProvider` (section state with track-stretch index, pass), `ICompositeStateProvider` (`X\Core\Brokers\ITrainStateProvider.cs:16-48`, `IDispatchStateProvider.cs:13-31`, `ICompositeStateProvider.cs:16-54`).
- **CSV, append-only**, one file per provider: `train-state.csv` with `Timestamp, ChangeType, TrainId, CallId, State, Time, NewTrack` and `dispatch-state.csv` with `Timestamp, ChangeType, SectionId, State, TrackStretchIndex, SignalPlaceId` (`X\Data\Csv\CsvTrainStateRecord.cs:27-62`, `CsvDispatchStateRecord.cs:21-51`). The timestamp is the real clock, UTC; there is no fast-clock time, no dispatcher and no session in a row. Expected volume about 2,000 rows per file per session (`X\Data\STATEPROVIDERs.md:113`).
- **Restore** replays the rows onto the freshly loaded objects, by generated id (`X\Core\Brokers\Broker.cs:75-94`, `X\Data\Csv\CsvDispatchStateProvider.cs:95-109`).
- **In memory**: `InMemoryCompositeStateProvider`, the one the server uses (`X\Server\Program.cs:11`).
- **Conversion from the planner's model**: `TimetableConversionExtensions` and `TimetableBrokerDataProvider` (findings in `CrossAppReview.md` §5; more in section 6 here).

### 1.5 Server and shared contracts (app-specific; stay in Dispatch)

- REST under `/api/v1`: dispatchers, a dispatcher with arrivals and departures, `POST train-sections/{id}/actions/{action}` and `.../pass/{controlPointIndex}` (`X\Server\Endpoints\TrainSectionEndpoints.cs:20-35`); server-sent events through a channel (`X\Server\Services\DispatchEventService.cs`, `Endpoints\EventEndpoints.cs`); HATEOAS links (`X\Server\Mapping\DataMappingExtensions.cs:94-101`).
- DTOs: `DispatcherData`, `TrainSectionData`, `ActionData`, `ActionResultData`, `DispatchEvent` and three subtypes (`X\Shared\*.cs`). Strings only: signatures, display names, "HH:mm".
- Configuration: clock type and URL, data source `Xpln` or `Json` (`X\Server\appsettings.json:9-25`, `Services\DataProviderServiceExtensions.cs:37-42`). A `Database` source is documented, not built (`X\Server\README.md:243-262`). The project already references `Microsoft.EntityFrameworkCore.Sqlite` without using it (`X\Server\Tellurian.Trains.Dispatch.Server.csproj:14`).
- `RgZm.md` and `ZN800.md` describe two existing systems (RgZm; the ZN800 train-number protocol). No code uses them; they are background for the future features in `X\SPECIFICATION.md:667-676` (block detectors, train numbers over LocoNet or ZLV).

### 1.6 What Dispatch documents but does not do

These matter because the Dispatching layer should define them once, not inherit the gap.

| Claim | Fact |
|---|---|
| "The library enforces capacity constraints" (`X\README.md:106-112`) | `TrackStretchCapacityManager.Occupy`, `ReleaseAll` and `DispatchStretch.HasFreeTrackFor` have no callers (grep over the solution). `Depart` only sets the state and index (`X\Core\ActionContextExtensions.cs:156-169`) |
| Observed times are recorded | `SetDepartureTime` and `SetArrivalTime` are called only when saved state is replayed (`X\Data\Csv\CsvTrainStateProvider.cs:131,138`); no action records a time |
| Time comes from the fast clock | Nothing calls `GetTimeStatusAsync`; every stored timestamp is `DateTimeOffset.UtcNow` (`X\Data\Csv\CsvDispatchStateRecord.cs:58`) |
| "Each dispatcher sees only the actions they are authorised to perform" (`X\README.md:104`) | The action endpoint does not say who acts: it takes the first available action of that name from any dispatcher (`X\Server\Endpoints\TrainSectionEndpoints.cs:69-71,177-183`). Who did what is not known |
| `Completed` is set when the last section arrives (`X\Core\ActionStateMachine.cs:151`) | `Arrive` only sets the section state (`ActionContextExtensions.cs:26,150-154`); `Start` and `Complete` are never offered |
| State survives a restart (`X\README.md:129-130`) | The server uses the in-memory provider and `isRestart: false` (`X\Server\Program.cs:11,31`) |
| A session selects the trains (`X\TODO:5`, unfinished sentence) | All trains are loaded (`X\Data\Timetable\TimetableBrokerDataProvider.cs:164`); "on demand" (`X\TODO:7`) is not built |
| A train on a dispatch stretch with an unsignalled place inside can arrive | *(unverified)* `Pass` is offered only when the next place is a `SignalControlledPlace` (`ActionStateMachine.cs:139-143`), and `Arrive` only on the last track stretch (`:117`), so with an `OtherPlace` between two stations the index never advances. In the reverse direction `NextControlPoint` reads `End` of the stretch, which is the place behind the train (`X\Core\TrainSection.cs:50-53`) |

## 2. What each layer here needs

### 2.1 Layouts (`Model.Layouts`)

**Add: the tracks of a line.** Dispatch needs direction per line track; the contract has a count (`S\Model.Layouts\TrackStretch.cs:140`) and the planner one layout-wide choice, "double track counts as two single tracks" (`S\Model\Validations\DispatchStretchValidation.cs:44`).

| Type | Properties | Notes |
|---|---|---|
| `LineTrack : LayoutPart` | `Id` (`int`, unique in the layout), `Designation` (`string?`, e.g. "U", "N", "1"), `Direction` (`LineDirection`), `NormalDirection` (`LineDirection`) | Owned by its `TrackStretch`, in order |
| `LineDirection` (enum, numbered) | `Both = 0`, `StartToEnd = 1`, `EndToStart = 2` | Relative to the stretch's `Start` and `End`; `TrackStretch.Reverse()` (`TrackStretch.cs:198-203`) must swap them |
| `TrackStretch.Tracks` | `IList<LineTrack>` | Empty means "as `TracksCount` says, each track both ways", so every existing plan stays valid. `TracksCount` then becomes worked out when the list is filled |

- `Direction` is what is permitted; `NormalDirection` is the up/down preference of `X\Core\Layout\Track.cs:15` for double track worked both ways (the Swedish case in `X\TODO:50,53`).
- The L3 rule then reads the tracks instead of the layout-wide setting where a stretch has them; the setting stays as the default for stretches without. That is a rule change for the planner, so it is its own step.
- Module Draw can fill them from the hall plan *(unverified: whether its drawings know the running direction of a line track)*.

**Change: the id of a dispatch stretch.** `CreateDispatchStretches` gives each the id of its first track stretch (`S\Model\Layouts\Layout.cs:739`). Two dispatch stretches that begin with the same track stretch, as from a station over an uncontrolled junction to two neighbours, get the same id *(unverified, not run)*; the id is the primary key (`S\Model.Databases\Mappings\DispatchStretchMapping.cs:14`), and the section state will refer to it. Give them ids of their own, kept when the stretches are regenerated where the end points and the path are the same.

**Change: `DispatchStretch` derives from `LayoutPart`** (already recommendation 10 of the review), so that Dispatch can keep something on it without a property here.

**Nothing to add for:**

- **Signals and blocks.** Dispatch's `BlockSignal` was replaced by `SignalControlledPlace` (`X\TODO:47-55`; `X\CLAUDE.md:20,25` is out of date). `IsBlockPost`, `IsControlPoint` and `AllowsMeets` say what Dispatch needs (`S\Model\Layouts\Layout.cs:595,641,650`).
- **Dispatcher roles.** `layout.Dispatchers`, `location.Dispatcher`, `layout.LocationsDispatchedBy(station)`, `layout.DispatchNeighboursOf(station)` (`Layout.cs:616,667,675,699`). A dispatcher is a `Station` with `HasDispatcher`; its identity is the station's id, exactly as in Dispatch.
- **Language of a dispatcher.** From the station's `CountryCode`, as the dispatch lists do (`S\Documentation\Requirements specification.md:283`). No `PreferredLanguage` property.
- **Lengths.** `StationTrack.Length`, `PlatformLength` and `TrackStretch.Distance` are metres as `double`; Dispatch's `int?` fields go.

**Where these live.** The dispatch-endpoint members are in `Model` (the Timetables project), as extension members on layout types. Dispatch will reference `Model` anyway, so they can stay; they are layout facts and could move to `Model.Layouts` when that project is next released, so that Module Draw can show who dispatches what.

### 2.2 Timetables (`Model`)

| Need | Today | Add |
|---|---|---|
| A train created during dispatch | Nothing marks it | `Train.Origin` of enum `TrainOrigin { Planned = 0, Dispatch = 1 }`, written to the plan file and the database. `Planned` is the default and is left out of the file, so no migration. A call needs no mark: it belongs to its train |
| Does the train run on session *n*? | `Sessions.Includes(int)` is `internal` (`S\Model\Timetables\Sessions.cs:168`) | Make it public, e.g. `train.RunsOn(sessionNumber)`. `IsOnDemand` is already public (`:277`) and gives Dispatch's "on demand" trains (`X\TODO:7`) |
| The train's runs over the dispatch stretches (Dispatch's train sections) | Private classes `Line` and `Visit` inside the L3 validation (`S\Model\Validations\DispatchStretchValidation.cs:90-228`) | A public derived type, e.g. `DispatchRun(Train, DispatchStretch, Direction, Calls)` with `First`, `Last`, and `train.DispatchRuns` / `dispatchStretch.RunsOf(train)`; the validation then uses it. It already handles what Dispatch cannot: calls inside the stretch, a run that starts or ends inside it, a train turning back (`:149-162`) |
| Control sections of a dispatch stretch | Private, inside `Line.Of` (`:105-135`) | Public: `dispatchStretch.ControlSections`, each with its track stretches, its capacity (tracks) and the control point that ends it. This replaces Dispatch's track-stretch index |
| A time on its day, as text | `HHMM()` drops the day (`S\Model\Timetables\Time.cs:174`) | `time.ClockTime` (time of day) and `time.Day` as extension members; needed by every display of a dispatched train past midnight |
| Use on a server | `Model` references `Microsoft.AspNetCore.Components` (`S\Model\Model.csproj:13`) | Remove it (already in the review §5); a prerequisite for Dispatch |
| Ids for trains and calls created in dispatch | `PlanReferences` gives a call the next free number (`S\Model\Schedules\PlanReferences.cs:242,252`); trains are not renumbered there | A reserved range for what dispatch creates (section 4.5) |

Already there and used as it is: run order (`CallsInRunOrder`, `StationCall.Sequence`, `S\Model\Timetables\Train.cs:379-380`, `StationCall.cs:95`), pass-through against stop (`IsStop`, `StationCall.cs:180`), adding calls (`Train.Add`, `AddAfter`), the next free train number per category (`Timetable.NextTrainNumber`, `S\Model\Timetables\Timetable.cs:469`), train length (`Train.Length`), vehicles and duties of a train (`plan.ScheduledObjectsFor(part)`, `S\Model\Schedules\Plan.cs:382`), and the dispatch list rows per dispatcher (`S\Planning.Components\Reporting\Dispatch\DispatchList.cs:62-105`), which is the same question Dispatch's `GetDispatcher` answers and should move down from `Planning.Components` to the model so that both use it.

### 2.3 Dispatching (new project `Model.Dispatching`, namespace `Tellurian.Trains.Schedules.Model.Dispatching`)

References `Model`. No EF Core, no HTTP, no logging dependency. Mutable classes with `int` ids, parents not written, references by id, as the other two layers.

**Types**

| Type | Properties | Identity and relations |
|---|---|---|
| `Operation` | `Id`, `PlanId`, `Sessions : IList<OperatingSession>`, `Log : IList<DispatchEntry>` | One per database, as the plan. Refers to the `Plan`; the plan does not know it |
| `OperatingSession` | `Id`, `Number` (1–14: the session or day position the trains' `Sessions` bits mean), `State` (`OperatingSessionState { Planned, Running, Ended }`), `StartedAt`, `EndedAt` (real clock, `DateTimeOffset?`), `Runs : IList<TrainRun>` | Owned by the operation. The same `Number` may be run twice (a session repeated at a meeting), so the id is not the number |
| `TrainRun` | `Id`, `TrainId`, `State` (`TrainState`), `PreviousState` (`TrainState?`), `Version`, `Sections : IList<TrainSection>`, `Calls : IList<ObservedCall>` | A train on one operating session. Unique on (`OperatingSessionId`, `TrainId`). This is where a **cancellation** is: `State = Cancelled` |
| `TrainSection` | `Id`, `DispatchStretchId`, `Direction` (`RunDirection { Forward, Reverse }`), `DepartureCallId`, `ArrivalCallId`, `State` (`DispatchState`), `ControlSection` (`int`, 0-based, set at departure), `LineTrackId` (`int?`, the line track given), `Version` | Owned by its run, in run order (`ListPosition`); "previous" is the one before it in the list, not stored. Unique on (`TrainRunId`, `DepartureCallId`): a call departs over one dispatch stretch, so the planner's call id identifies the section |
| `ObservedCall` | `CallId`, `Arrival`, `Departure` (`Time?`, fast clock, on its day), `ArrivedAt`, `DepartedAt` (`DateTimeOffset?`, real clock), `TrackId` (`int?`, the track used when not the planned one) | Owned by its run. Unique on (`TrainRunId`, `CallId`). Created when first needed |
| `DispatchEntry` | `Id` (`long`), `OperatingSessionId`, `Action` (`DispatchAction`), `TrainRunId`, `TrainSectionId?`, `CallId?`, `LocationId?` (the control point passed, or where a train was aborted), `TrackId?`, `DispatcherId` (the `Station` whose dispatcher acted), `By` (`string?`, the person or the external system), `At` (`DateTimeOffset`, real clock), `FastTime` (`Time?`), `FromState`, `ToState` (`string`), `UndoesEntryId?` | The log: append-only, never changed. One entry per action performed, also the ones the model performs itself (running, completed) |
| `LineClosure` (later) | `TrackStretchId`, `LineTrackId?`, `From`, `To` (`Time?`), `Reason` | Dispatch's `Closed` direction as state. Not needed for the first version |

Names: "TrainSection" is kept from Dispatch's API (`/train-sections`); it is close to the planner's `TrainPart`, so it is one of the questions in section 8. "Cancelled" is spelt the British way; Dispatch has `Canceled` in both enums.

**Enums** (numbered explicitly, written by name in JSON)

- `TrainState { Planned = 0, Manned = 1, Running = 2, Cancelled = 3, Aborted = 4, Completed = 5 }`.
- `DispatchState { None = 0, Requested = 1, Accepted = 2, Rejected = 3, Revoked = 4, Departed = 5, Arrived = 6, Cleared = 7 }`. `Cleared` replaces Dispatch's second `Canceled`, which is what the `Clear` action sets (`X\Core\ActionContextExtensions.cs:27`).
- `DispatchAction { Request, Accept, Reject, Revoke, Depart, Pass, Arrive, Clear, Man, Cancel, Abort, Undo, ChangeTrack, AddTrain }`, plus `Run` and `Complete` for the entries the model writes itself. `AssignDriver` becomes `Man` (the state is `Manned`; a duty, not Dispatch, assigns the loco driver).
- `DispatcherRole { Departure, Arrival, ControlPoint }`: worked out, never stored.

**Derived members (extension members, not stored)**

- `section.From`, `section.To` (locations), `section.DepartureDispatcher`, `section.ArrivalDispatcher` (`Station?`, through `location.Dispatcher`), `section.NextControlPoint`, `section.IsOnLastControlSection`, `section.Previous`, `section.IsFirst`.
- `run.Train`, `run.IsDispatchable`, `run.ScheduledInstant(call)` (section 5).
- `operation.SectionsFor(station, session)`: arrivals and departures of a dispatcher, in time order.
- `operation.AvailableActions(section, station)` and `operation.Perform(action, section, station, by, clock)`: the state machines of section 3.

**Clock**

- `ClockReading(Time? FastTime, ClockState State, string? Message)` with `ClockState { Unavailable, Stopped, Running, RealTime }`, and `IFastClock { ValueTask<ClockReading> ReadAsync(...) }`. The model takes a reading as an argument of `Perform`; it never fetches one. The HTTP client for the Tellurian fast clock stays in Dispatch.
- Every log entry has both clocks: `At` is the real clock, `FastTime` the fast clock, null when the clock was unavailable (`X\Data\TellurianFastClockTimeProvider.cs:49-50` returns no time then).

**Serialisation.** A `JsonSerializerContext` for the dispatching types (for the API and for an export of an operation), source-generated from the start, since nothing reads the old form.

**Who did what.** `DispatcherId` + `By` on every entry. The model does not define users. Whether Dispatch has sign-in is app-specific (question 8.6).

## 3. State machines

### 3.1 Train run

```
Planned ──Man──▶ Manned ──(first Depart)──▶ Running ──(last Arrive)──▶ Completed
   │               │                           │
 Cancel          Cancel                      Abort
   ▼               ▼                           ▼
Cancelled       Cancelled                   Aborted          Undo: Manned, Cancelled, Aborted → the state before
```

As Dispatch has it (`X\SPECIFICATION.md:154-222`, `X\Core\ActionStateMachine.cs:153-188`), with these rules:

| # | Rule | Dispatch today |
|---|---|---|
| T1 | `Man` and `Cancel` only before the first section has departed; `Abort` only while `Running` | `ActionStateMachine.cs:157-180` |
| T2 | `Running` is set by the first `Depart`, not by an action | `ActionContextExtensions.cs:161-166` |
| T3 | `Completed` is set by `Arrive` on the last section | Stated (`ActionStateMachine.cs:151`), not done |
| T4 | `Undo` reverts `Manned`, `Cancelled` or `Aborted` to the state before; one level | `Trains\Train.cs:23-30` |
| T5 | An on-demand train (`Sessions.IsOnDemand`) is shown only to the dispatcher of its first departure until it is manned | `X\TODO:7`, not built |
| T6 | `Man` and `Cancel` are for the dispatcher of the train's first departure; `Abort` for the dispatchers of the section the train is on | Dispatch offers them to any dispatcher of the section (`SPECIFICATION.md:200-203`) |

### 3.2 Train section

```
None ──Request──▶ Requested ──Accept──▶ Accepted ──Depart──▶ Departed ──Pass*──▶ … ──Arrive──▶ Arrived
                     │  ▲                  │                    │
                  Reject │               Revoke               Clear (run cancelled or aborted)
                     ▼  │                  ▼                    ▼
                  Rejected ─Request     Revoked ─Request      Cleared
```

| # | Rule | Dispatch today |
|---|---|---|
| S1 | Nothing on a section until the run is `Manned` or `Running` | `ActionStateMachine.cs:72,100,133` |
| S2 | Nothing on a section until the one before it has departed | `TrainSectionExtensions.cs:24-26` |
| S3 | `Request`: departure dispatcher, from `None`, `Rejected`, `Revoked` | `ActionStateMachine.cs:77-81` |
| S4 | `Accept`, `Reject`: arrival dispatcher, from `Requested` | `:105-108` |
| S5 | `Revoke`: departure dispatcher from `Requested` or `Accepted`; arrival dispatcher from `Accepted` | `:83-90,110-113` |
| S6 | `Depart`: departure dispatcher, from `Accepted`; sets control section 0, records the departure in `ObservedCall`, sets the run `Running` | `:87-88`; the time is not recorded |
| S7 | `Pass`: from `Departed`, not on the last control section, by the dispatcher who controls the next control point; moves to the next control section | `:131-143`, counted in track stretches |
| S8 | `Arrive`: arrival dispatcher, from `Departed`, on the last control section; records the arrival; completes the run if it is the last section | `:115-121`; time and completion not done |
| S9 | `Clear`: a `Departed` section of a cancelled or aborted run | `:193-199` |
| S10 | **Capacity** (new as a rule that is applied): `Accept` and `Depart` are refused when the first control section has no free line track in the train's direction, and `Pass` when the next has none; a meet-free zone refuses an opposing train. The same definitions as planning rule L3 (`S\Model\Validations\DispatchStretchValidation.cs:13-34`) | Written, never called (1.6) |
| S11 | `ChangeTrack`: the dispatcher of the location gives a call another track of the same location, before the train has arrived there | `ChangeTrack` exists, no action uses it (`Trains\TrainStationCallExtensions.cs:12-15`) |

**Corrections to make when the machines move**

1. **Control sections, not track stretches.** A section advances over the control sections of its dispatch stretch (2.2), so that an uncontrolled halt or junction inside does not stop the train from arriving (1.6, last row).
2. **Who passes a control point.** Its `ControlledBy` station where it has one; otherwise the arrival dispatcher. A block post can have a controlling station without being a dispatch endpoint (`S\Model\Layouts\Layout.cs:584-587`).
3. **A section that starts or ends where nobody dispatches.** The planner's runs can begin or end inside a dispatch stretch, at an unmanned station nobody controls (`DispatchStretchValidation.cs:144-147`). Dispatch has no such case because it makes every `Station` a dispatcher (`X\Core\Brokers\Broker.cs:57`). Recommended: the dispatcher at the end of the dispatch stretch that lies behind the train acts as departure dispatcher, the one ahead as arrival dispatcher, on the loco driver's word (question 8.4).
4. **Stop flags are not dispatch flags.** Dispatch requires `IsDeparture` on the first call and `IsArrival` on the second (`X\Core\Layout\DispatchStretchDirection.cs:66-69`) and reads "last section" as `Arrival.IsDeparture == false` (`X\Core\TrainSectionExtensions.cs:16`). In the planner both flags are false for a pass-through (`S\Model\Timetables\StationCall.cs:180,190`), so a train passing a manned station without stopping would get no section there. A section is defined by the run order and the dispatch stretch alone.
5. **The acting dispatcher is an argument.** `Perform` takes the station that acts and refuses an action that station does not have; the server passes who called (1.6).

### 3.3 Domain or app

| In Schedules (Dispatching) | In Dispatch (app) |
|---|---|
| States, actions, rules T1–T6 and S1–S11, capacity, who may act | Endpoints, links, the event stream, which client is which dispatcher |
| Building runs and sections for a session; arrivals and departures of a dispatcher in time order | How many rows are shown, filtering, colours (`DispatchState.BackgroundColor`), CSS classes |
| Recording both clocks; the log | Reading the fast clock over HTTP; fallback when it is down |
| Adding a train with its calls, cancelling, undoing | The form for a new train; confirmation dialogs |
| Resource keys for states and actions (as `ITranslatable` does for the model's names) | The translated texts shown; "Undo manned" as a composed label (today English built in the domain, `X\Core\ActionContext.cs:23-28`) |
| Reading and writing: database mapping, JSON context | Connection string, where the database file is, backup |

## 4. Database

### 4.1 Where the tables go

In the same `ScheduleDbContext` model, each with a mapping of its own in `Mappings\` like the others (`S\Model.Databases\ScheduleDbContext.cs:163-192`). A second context cannot be used with `EnsureCreated`, which creates nothing when the database already exists; one model also means one version. `Model.Databases` then references `Model.Dispatching`.

### 4.2 Tables and columns (PascalCase)

| Table | Columns | Keys to plan entities |
|---|---|---|
| `DatabaseInfo` (one row) | `SchemaVersion`, `PlanSchemaVersion`, `AppVersion`, `CreatedAt`, `PlanExportedAt`, `PlanRevision` | — |
| `Operations` (one row) | `Id`, `PlanId` | `Plans.Id` |
| `OperatingSessions` | `Id`, `OperationId`, `Number`, `State`, `StartedAt`, `EndedAt`, `ListPosition` | — |
| `TrainRuns` | `Id`, `OperatingSessionId`, `TrainId`, `State`, `PreviousState`, `Version`, `ListPosition`; unique (`OperatingSessionId`, `TrainId`) | `Trains.Id` |
| `TrainSections` | `Id`, `TrainRunId`, `DispatchStretchId`, `Direction`, `DepartureCallId`, `ArrivalCallId`, `State`, `ControlSection`, `LineTrackId`, `Version`, `ListPosition`; unique (`TrainRunId`, `DepartureCallId`) | `DispatchStretches.Id`, `StationCalls.Id`, `LineTracks.Id` |
| `ObservedCalls` | `Id`, `TrainRunId`, `CallId`, `Arrival`, `Departure`, `ArrivedAt`, `DepartedAt`, `TrackId`; unique (`TrainRunId`, `CallId`) | `StationCalls.Id`, `StationTracks.Id` |
| `DispatchLog` | `Id` (generated), `OperatingSessionId`, `Action`, `TrainRunId`, `TrainSectionId`, `CallId`, `LocationId`, `TrackId`, `DispatcherId`, `By`, `At`, `FastTime`, `FromState`, `ToState`, `UndoesEntryId` | `OperationLocations.Id`, `StationCalls.Id`, `StationTracks.Id` |
| `LineTracks` (Layouts, 2.1) | `Id`, `TrackStretchId`, `Designation`, `Direction`, `NormalDirection`, `ListPosition`, `ExtensionData` | `TrackStretches.Id` |
| `Trains` (existing) | add `Origin` | — |

- **Ids.** The plan's ids stay the keys, as today (`S\Model.Databases\README.md:30-35`). The dispatch tables' own ids are generated by the database; nothing outside it refers to them except the API during one operation.
- **State and log.** `TrainRuns`, `TrainSections` and `ObservedCalls` hold the current state and are updated; `DispatchLog` only grows. An action is one transaction: check, update the state rows, add the log entry. The state can be rebuilt from the log, which is the test of both.
- **Times.** `Time` as ticks, as the plan's (`S\Model.Databases\Mappings\Conversions.cs:13-15`); real-clock instants as UTC text.
- **Enums.** The plan's tables store enums as numbers. For the dispatch tables I recommend names: other on-premise programs (station displays) will read them with SQL (`S\Documentation\Requirements specification.md:1770`), and the review already lists numbers as a hazard (§1, point 10). It is a deviation from the plan's tables, so it is question 8.7.

### 4.3 Living with an export that recreates the database

Today the export deletes and creates the database (`S\Model.Databases\DatabaseExportService.cs:36-37`), which would delete an operation.

1. **Export before operation.** The planner's export creates the database with empty dispatch tables and writes `DatabaseInfo`.
2. **A guard.** `ExportScheduleAsync` refuses when `OperatingSessions` has rows, unless the caller asks for replacement explicitly. One line of protection against the worst mistake at a meeting.
3. **A corrected plan during a meeting.** A second service, e.g. `DatabaseUpdateService.ReplacePlanAsync(plan)`: reads the operation, recreates the database with the new plan, writes the operation back, in one transaction where the provider allows it, else to a new file that replaces the old. It works because ids are given once and never changed. Before writing back it checks every reference (train, call, track, dispatch stretch): a section whose calls are gone is reported and kept in the log only. Trains with `Origin = Dispatch` are carried over from the old database, since the planner's copy does not have them.
4. **Reading back in the planner.** `ReadPlanAsync` (`S\Model.Databases\DatabaseImportService.cs:36`) already returns the trains, so it returns the additional ones too, marked by `Origin`. A separate `ReadOperationAsync` returns the `Operation`; the planner needs it only to show what was cancelled.
5. **The WebAssembly planner cannot write SQLite** (`Requirements specification.md:1765-1787` plans a conversion service for this). With Dispatch on this model, the Dispatch server can do it itself: open a plan file, read it with `PlanFile`, export with `DatabaseExportService`. That removes the dependency on a service that does not exist yet (question 8.1).

### 4.4 Version

A one-row `DatabaseInfo` table rather than `PRAGMA user_version`, so that it works on every provider the README lists (`S\Model.Databases\README.md:54-62`). Reading refuses a newer `SchemaVersion`. For an older one: the plan part can always be re-exported from the plan file; the dispatch part is the only thing that needs migration steps, and only once a version with dispatch tables has been used at a meeting. Until then, recreate.

### 4.5 Ids for what dispatch creates

An additional train and its calls need ids that cannot collide with ids the planner gives later to the same plan. Recommended: a reserved range, ids from 1,000,000 upwards for trains and calls created in dispatch, allocated as highest-in-range plus one inside the transaction. The planner's `GiveUniqueIds` (`S\Model\Schedules\PlanReferences.cs:252`) must then take its next number from below the range. The alternative, highest id plus one, collides as soon as the planner adds a train to its own copy during the meeting.

### 4.6 Several dispatchers at once

- **One writer.** All dispatchers are clients of one server process (`X\SPECIFICATION.md:581-600`), so the server performs actions one at a time (a lock or a channel around `Perform` and its transaction). No two processes write the database.
- **Stale clients.** A client acts on what it last saw. The state check already refuses a second `Depart`; `Version` on run and section, sent with the action and compared, also refuses an action taken on an out-of-date view of a different kind (accepting a request that was revoked and requested again). `Version` is also the EF Core concurrency token, in case a second writer ever appears.
- **Readers.** Other programs read the same file; with SQLite, write-ahead logging lets them read while the server writes. That is a setting of the app, not of the model.

## 5. Operating day

**How the plan says when a train runs.** `Train.Sessions` is a bit pattern; session or day *n* is bit *n − 1* (`S\Model\Timetables\Sessions.cs:168-169`), a day pattern is mirrored into the upper seven bits (`:22`), and one more bit marks on demand (`S\Model\Timetables\CommonSessionPatterns.cs:21`). Whether the numbers mean sessions or weekdays, which weekday is number 1, and how many there are, are layout settings (`S\Model.Layouts\Settings\GeneralSettings.cs:9,12,20`).

**Selecting the trains.** Starting operating session with `Number = n` creates a `TrainRun` for every train whose sessions include *n*, with its sections from the dispatch runs of 2.2. On-demand trains get a run too, hidden by rule T5. Additional trains get their run when they are created. Dispatch today loads every train regardless (`X\Data\Timetable\TimetableBrokerDataProvider.cs:164`).

**Times past midnight.** A time with day 1 or more is the same train continuing after midnight (`S\Model\Timetables\Time.cs:59-68`); a train never starts there. So a call belongs to the session of its train, whatever the clock's day is. Define the scheduled instant of a call in a run as

`instant = (session.Number − 1) × 24 h + call time (with its days)`

and order every list by it. Two cases follow:

- **Sessions run separately** (each a day from `StartTime` to `EndTime`, `GeneralSettings.cs:36,39`): only one session's runs exist at a time; a late train past midnight simply has the largest instants.
- **Round the clock** (`RunsOverMidnight`, `GeneralSettings.cs:46`): at 00:15 on day 2 the tail of session 1 (calls at `1.00:15`) and the start of session 2 (calls at `0.00:15`) are both current, with the same instant. Two operating sessions are then running at once; a session ends when its last run is completed, cancelled or aborted, not at midnight. `FirstSessionStartTime` (`GeneralSettings.cs:55`) says where the first begins.

**The fast clock gives only "HH:MM"** (`X\Data\TellurianFastClockTimeProvider.cs:76-93`). The day therefore has to come from the operation: the instant "now" is the day of the latest running session plus the clock time, moved to the next day when the clock time is earlier than that session's start of day. Whether the clock service can also say the weekday was not checked *(unverified)*; if it can, it should be compared, not trusted alone.

**Observed times** are stored as `Time` with their day in the run's own frame (`0.23:58`, `1.00:07`), so lateness is a plain subtraction from the scheduled time. Dispatch today has `TimeSpan` without meaning for the day and prints `time.Hours`, which drops it (`X\Server\Mapping\DataMappingExtensions.cs:110-111`).

**Order within a train** is `CallsInRunOrder`, by `Sequence` (`S\Model\Timetables\Train.cs:379-380`), never by time. Dispatch orders by departure time (`TimetableBrokerDataProvider.cs:186`) and again by scheduled departure in the broker (`X\Core\Brokers\Broker.cs:113`).

## 6. Gaps and conflicts

| # | Conflict | Fact | Recommendation |
|---|---|---|---|
| 1 | Generated ids | One static counter for every type (`X\Core\Utilities\UtilityExtensions.cs:6,10`); trains, calls and track stretches converted with `Id = 0` (`X\Data\Timetable\TimetableConversionExtensions.cs:83,131,168`) | No generated ids for plan entities: the plan's ids. Section identity is run + departure call (2.3) |
| 2 | Train identity | Trains are matched by prefix and number (`TimetableBrokerDataProvider.cs:210-212`); the planner's equality is company and number (`S\Model\Timetables\Train.cs:221-224`) | Refer to a train by id everywhere in Dispatching; never by number |
| 3 | `Single()` | `FindFor` takes the single dispatch stretch between two consecutive calls (`X\Core\Layout\DispatchStretchExtensions.cs:40-41`); a call at a halt inside a stretch matches none and two routes between the same stations match two, and both throw *(unverified, not run)* | Sections from the public dispatch runs (2.2), which accept calls inside a stretch |
| 4 | A fourth kind | `IndustrialArea` throws (`TimetableConversionExtensions.cs:46`) | The conversion goes; rules switch over the closed hierarchy and the compiler checks them |
| 5 | Unknown kind read as `Station` | `S\Model.Layouts\OperationLocationJson.cs:49-52` (review §3) | For Dispatch this would create a dispatcher nobody staffs; refuse the file instead (review recommendation 8) before Dispatch reads plans |
| 6 | Unmanned locations | `IsManned = true` for every station (`TimetableConversionExtensions.cs:41`), and every `Station` gets a dispatcher (`Broker.cs:57`) | `HasDispatcher`: manned, or a shadow station (`S\Model\Layouts\Layout.cs:575`). An unmanned station is remotely dispatched or part of the line |
| 7 | Dispatch stretches ending at a non-station | Dropped (`TimetableConversionExtensions.cs:188-191`); the planner's end points include remotely dispatched junctions, industrial areas and unmanned stations (`Layout.cs:609`) | A section's dispatcher is `location.Dispatcher` (`Layout.cs:616`); its rows are on the controlling station's list, as on the printed dispatch list |
| 8 | Missing controller | `ControlledBy?.Id ?? 0`, which no dispatcher has (`TimetableConversionExtensions.cs:43`), so nobody can pass it | Correction 2 in 3.2; the planner's rule on who controls a location reports it before the meeting |
| 9 | Distance | `× 1000` and truncated to `int` (`TimetableConversionExtensions.cs:84`) | Metres as `double`, read as they are |
| 10 | Line tracks invented | Every track double-directed, odd ones "up" (`TimetableConversionExtensions.cs:89-112`) | `LineTrack` in Layouts (2.1) |
| 11 | Unscheduled tracks | Filtered away (`TimetableConversionExtensions.cs:34`); a call on an unknown track silently gets the first track (`:155-156`) | Keep all tracks; a dispatcher may move a train to an unscheduled track (rule S11). A call always has its track by id |
| 12 | Stop flags used as section flags | 3.2, correction 4 | Sections by run order |
| 13 | Track-stretch index | 1.6 and 3.2, correction 1 | Control sections |
| 14 | Dispatch stretch id not unique | 2.1 | Ids of their own |
| 15 | Equality by signature | `OperationLocation` equals by signature without regard to country (`S\Model.Layouts\OperationLocation.cs:194-200`); Dispatch equals by id (`X\Core\Layout\OperationPlace.cs:33-34`) | Dispatching compares ids. The model's equality is an open point of the review (§3), not to be solved here |
| 16 | The export deletes the database | `DatabaseExportService.cs:36-37` | 4.3 |
| 17 | No database version | Review §1, point 6 | 4.4, before the first dispatch table |
| 18 | Enum numbers | Layout enums unnumbered and stored as numbers (review §1, point 10) | Number every enum the dispatch tables or Dispatch's API rely on; names in the dispatch tables (4.2) |
| 19 | `Model` needs a UI framework | `S\Model\Model.csproj:13` | Remove before Dispatch references it |
| 20 | .NET version | Dispatch is `net10.0` (`X\Directory.Build.props:4`); the closed hierarchy needs `net11.0` (`ModelHarmonisation.md` §9, decision 11) | Dispatch moves to .NET 11 as its first step |
| 21 | Two "sessions" | Dispatch's TODO means one run of the whole timetable (`X\TODO:5`); the planner's `Sessions` is the set of session numbers a train runs on | `OperatingSession` for the first, with `Number` pointing into the second |
| 22 | Plan format | Dispatch reads plans with `JsonImportService` 2.1.0 (`X\Server\Services\DataProviderServiceExtensions.cs:128`), before the envelope | Reads through `PlanFile`, or the database |
| 23 | API key in the plan | `IntegrationSettings.ModuleRegistryApiKey` is written to the database (review §1, point 9) | Must be gone before the database is handed to a meeting's dispatch server (review recommendation 6) |
| 24 | Spelling | `Canceled` in two enums and in the API's action names | `Cancelled`, `Cleared`; the API is Dispatch's to change, it has no clients yet |

## 7. Proposed order of work

Each step builds and keeps the tests green on its own. S = Schedules, X = Dispatch.

1. **S: prerequisites in `Model`.** Remove `Microsoft.AspNetCore.Components`; make `Sessions.Includes(int)` public as `RunsOn`; add `Time` day and clock-time members. No file or database change.
2. **S: dispatch runs and control sections public.** Lift `Line`/`Visit` out of `DispatchStretchValidation` into public derived types; the validation uses them; its tests are the proof nothing changed. Move the dispatch list's row building from `Planning.Components` to `Model` if wanted here.
3. **S: dispatch stretch ids of their own**, and `DispatchStretch : LayoutPart`. Plan schema version 5 only if ids change in saved plans (they do where two shared one).
4. **S: database version and guard.** `DatabaseInfo`, checked on reading; the export refuses a database with operating sessions. No dispatch tables yet.
5. **S: `Train.Origin`**, in the model, the plan file (left out when `Planned`) and the database; the reserved id range respected by `PlanReferences`.
6. **S: `Model.Dispatching`, types and enums only**, with the JSON context. Then the two state machines with rules T1–T6 and S1–S9, S11, ported together with Dispatch's tests (`X\Tellurian.Trains.Dispatch.Tests\SimpleDispatchTests.cs`, `JunctionTests.cs`, `SignalControlledPlaceTests.cs`), rewritten against plans built with `PlanFactory`.
7. **S: dispatch tables** in `Model.Databases`, `ReadOperationAsync`, an action as one transaction, state rebuilt from the log as a test, and the round trip of a plan with an operation.
8. **S: operating sessions**: starting one, the instant of a call, round-the-clock overlap; tests with a plan that runs over midnight.
9. **X: move to .NET 11 and onto the packages.** Reference `Model`, `Model.Dispatching`, `Model.Databases`; delete `Layout\`, `Trains\`, `Brokers\` data and state providers, `Data\Timetable\`, `Data\Csv\`; the server opens the database (or a plan file, question 8.1), maps the model to its DTOs, passes the acting dispatcher. The DTOs gain the plan's ids.
10. **S: additional trains and cancellation read back**: `AddTrain` in Dispatching (a `Train` with `Origin = Dispatch`, its calls, its run), `ReplacePlanAsync`, and the planner showing dispatch-created trains.
11. **S: `LineTrack`** in Layouts, the file and the database; then rule S10 (capacity) in Dispatching and L3 reading the tracks where present. Last, because nothing else waits for it and it changes a planning rule.
12. **S, later:** `LineClosure`; the Module Registry or Module Draw filling line tracks.

Steps 1–5 can be released as they are (NuGet 4.x); 6–8 are the new package; 9 is the switch in Dispatch.

## 8. Questions for the owner

1. **Who creates the operating database?** The planner runs in the browser and cannot write SQLite; the specification plans a conversion service in the Module Registry (`Requirements specification.md:1765-1787`). With Dispatch on this model, the Dispatch server could take a plan file and create the database itself. Is that the intended route, with the conversion service dropped or kept only for other consumers?
2. **Can the plan change during a meeting?** If yes, `ReplacePlanAsync` (4.3, point 3) is needed and the id range of 4.5 matters; if the plan is frozen once operation starts, both are simpler.
3. **Is an operating session one number run once, or can a session be repeated?** The design allows a repeat (the id is not the number). If it never happens, `Number` can be the key.
4. **A train starting or ending at an unmanned station nobody controls: who clears it?** Recommended in 3.2, correction 3: the dispatchers at the two ends of the dispatch stretch, on the loco driver's word. Or are such trains outside dispatch until they reach a dispatch endpoint?
5. **Names.** `TrainSection` (Dispatch's word, close to `TrainPart`) or another word for a train's run over one dispatch stretch; `Operation`, `OperatingSession`, `TrainRun`, `ObservedCall`, `DispatchEntry`; `Man` for the action.
6. **Who is the dispatcher in the log?** The station is known. Is the person wanted too, and if so by sign-in or by a name typed once per device?
7. **Enums in the dispatch tables by name**, while the plan's tables keep numbers; or the same in both?
8. **Line tracks: where do their directions come from?** Entered in the planner, or drawn in Module Draw and read from the hall plan? And does the layout-wide "double track as two single tracks" setting stay as the default?
9. **Which train-state actions belong to whom?** Dispatch lets any dispatcher of the section man, cancel or abort a train; rule T6 narrows it. Is that right, or should, say, a meeting's traffic controller have them all?
10. **Should the fast clock's day be stored by the operation** (5), or will the clock service give the session or weekday?
