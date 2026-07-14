# Implementation plan: scheduling-conflict feedback in the GUI

## Goal

Surface the scheduling conflicts and inconsistencies computed by `ValidationExtensions`
(in `Model/Validations/`) in the appropriate GUI components, using two complementary
mechanisms:

- **(a) Inline highlight** — a yellow (severity-coloured) background on the offending
  object in a GUI component (graphical timetable, Schedules/Trains tabs), with the message
  shown on hover.
- **(b) Toolbar indicator + list** — a warning icon with a count in the top bar; clicking
  it opens a panel listing every conflict, regardless of the active tab.

## Guiding decision

Both mechanisms share one validation engine and one cached error set. The **list is the
source of truth** (it covers every error type, including cross-cutting ones such as
`ScheduleNotClosed`, `VehicleDoubleBooked`, `SessionCombinationNotClosed` that have no
natural pixel to highlight); **inline highlights are a filtered projection** of that same
set onto the objects a component renders.

`ValidationError` already carries the highlight metadata by design
(`FromTrack`/`ToTrack`/`FromTime`/`ToTime`/`Trains`/`ErrorType`/`Message`), and `Message`
already carries a `Severity`. `ScheduleStateService.OnChanged` is the single change signal.
So this is a wiring task, not a modelling one.

## Phasing

Ship the cheap, total-coverage layer first; add inline highlights component-by-component.

### Phase 0 — Shared foundation (prerequisite)

- **`ValidationStateService`** (new, `Planning.App/Services/`): singleton mirroring
  `ScheduleStateService`. Subscribes to its `OnChanged`; on change, **debounced** (~300 ms,
  reusing the `CancellationTokenSource` debounce idiom) recompute of
  `plan.GetValidationErrors(plan.Layout.Settings.Validation)`. Caches
  `IReadOnlyList<ValidationError> Errors`, exposes `Count`/`HasErrors`/grouping, raises
  `OnValidationChanged`. Recompute wrapped in try/catch + `ILogger` so a throwing rule never
  breaks the UI. Registered in `Program.cs`.
- **Matching predicates on `ValidationError`** (Model): `Involves(Train)`,
  `Involves(StationTrack)`, `OverlapsTimeRange(Time, Time)`, `Severity => Message.Severity`.
  Shared by all components; unit-tested in `Model.Tests`.

### Phase 1 — Approach (b): toolbar indicator + list  *(ship first)*

- **`ValidationIndicator.razor`** (new, `Planning.App/Components/`): warning triangle +
  count badge in the `top-bar-actions` of `MainLayout`; hidden/green when clean; colour by
  max severity. Click toggles a popover listing errors grouped by type/severity, each row
  showing `Message.ToString()`. Subscribes to `OnValidationChanged`; `IDisposable`.
- New localised UI-label keys in `Planning.App.Translations/Resources/Labels.resx` (+ 4
  translations) for the panel header/empty state. (The validation *messages* themselves
  already arrive localised from the Model via `Message.ToString()`.)

**Checkpoint: user reviews the GUI before later phases.**

### Phase 2 — Approach (a): inline highlight on the graphical timetable

- `GraphicalTimetableTab` injects `ValidationStateService`, passes the stretch-filtered
  error subset to each `GraphicalScheduleEditor`.
- `GraphicalScheduleEditor`: **MVP** — restyle `<line>` segments of any train in
  `error.Trains` this graph shows (`.conflict` class / yellow underlay) + `<title>` =
  message(s) for the hover text; reuses existing geometry, no new maths. **Refinement** —
  translucent marker over the `FromTrack..ToTrack × FromTime..ToTime` region.

### Phase 3 — Approach (a) on tabular views

- `SchedulesTab` / `TrainsTab`: each row filters the cached list via `Involves(train)` /
  schedule match → `.has-conflict` parent class + tooltip. Covers schedule-level errors the
  graph can't show inline.

### Phase 4 — Polish (optional)

- Click-to-locate from the list to the owning tab + object.
- Severity colours once rules emit `Warning`/`Error` (all `Information` today).
- Settings toggle to disable live validation, reusing `ValidationSettings` group flags.

## Performance stance

- One centralised, **debounced** recompute per idle pause — never per keystroke, never per
  pane.
- Rule loops are O(n²) *within* a track/stretch/schedule, not across the whole plan; a
  club-sized timetable is likely single-digit milliseconds even under WASM Mono.
  **Measure before optimising.**
- If a category proves costly, use the existing `ValidationSettings` group flags as the
  coarse lever. **Do not** build per-rule "what changed ⇒ which rules" scoping — the blast
  radius is wide and a stale-but-green highlight is worse than a slightly slow one.

## Files

| Phase | File | Change |
|---|---|---|
| 0 | `Planning.App/Services/ValidationStateService.cs` | new |
| 0 | `Model/Validations/ValidationError.cs` | predicates + `Severity` |
| 0 | `Model.Tests/ValidationTests.cs` | predicate tests |
| 0 | `Planning.App/Program.cs` | register service |
| 1 | `Planning.App/Components/ValidationIndicator.razor` (+`.css`) | new |
| 1 | `Planning.App/Layout/MainLayout.razor` | mount indicator |
| 1 | `Planning.App.Translations/Resources/Labels*.resx` (×5) | new UI-label keys |
| 2 | `Planning.App/Pages/GraphicalTimetableTab.razor` | feed errors |
| 2 | `Planning.Components/.../GraphicalScheduleEditor.razor` (+`.css`) | highlight + `<title>` |
| 3 | `Planning.App/Pages/SchedulesTab.razor`, `TrainsTab.razor` | row highlight |
