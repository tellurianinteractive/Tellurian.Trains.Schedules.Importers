# Tellurian.Trains.Schedules.Model.Layouts

The operating model of a model railway layout: **operation locations**, their **tracks** and the **stretches** between
them, with what a hall plan's drawings measured. It is what the planner and Dispatch take, and what Module Draw and the
Module Registry share with them. It has no dependencies.

```
dotnet add package Tellurian.Trains.Schedules.Model.Layouts
```

The drawn layout (modules with their drawings, end profiles, halls, hall plans and the drawing files) is in
`Tellurian.Trains.Schedules.Model.Drawings`; a hall plan gives a `Layout` with `drawnLayout.ToLayout()`. The planner's
own data (settings, companies, timetable and dispatch stretches, and what it decides for each location and track) is in
`Tellurian.Trains.Schedules.Model`.

| Type | What it is |
|---|---|
| `OperationLocation` | A closed class: `Station`, `SignalControlledLocation`, `IndustrialArea` or `OtherLocation`, each with only what its kind can have: name, signature, country, tracks, what can be exchanged, manned, controlled by, lock key, regions |
| `OperationLocationKind` | Station, SignalControlled, IndustrialArea, Other: the kind as a value (`location.Kind`), and in JSON the type discriminator `"kind"` |
| `StationTrack` | A track, as train planning sees it: designation, lengths in metres, direction, main, stub, scheduled, lockable, usage |
| `OperationLocationRules` | The rules between properties, as checks with a code per broken rule |
| `Layout` | The locations and stretches of a layout, its countries and regions, and, when made from a hall plan, its source and revision, scale and standard, the modules without their drawings (`LayoutModule`) and the order of the locations along each line (`StretchLine`) |
| `TrackStretch` | The line between neighbouring locations: distance, tracks, speed, time, and what the hall plan measured (`MeasuredDistance`, `MeasuredSpeed`) |
| `LayoutComparison`, `LayoutUpdate` | The differences between a layout and a newer revision of its hall plan, and taking those that change only distances and speeds |
| `Scale` | A scale, from the suite's list of scales (`Scale.Scales`) |

The types are mutable classes. `Copy()` gives a deep copy of what an object owns, and `x.With(c => c.Name = "Tellby")`
a copy with a change. A reader of older files calls `OperationLocationJson.Normalise(node)` before deserializing.
