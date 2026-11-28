# Tellurian.Trains.Schedules.Model.Drawings

The drawn model railway layout: what Module Draw makes and the Module Registry shows. It builds on
`Tellurian.Trains.Schedules.Model.Layouts` (the operating layout), `Tellurian.Trains.Schedules.Model.Geometry` and
`Tellurian.Trains.Schedules.Model.Files`.

```
dotnet add package Tellurian.Trains.Schedules.Model.Drawings
```

| Type | What it is |
|---|---|
| `Module`, `ModuleDrawing` | A module with its drawing: exits, sides, sections, track elements, labels and areas |
| `ModuleRegistryData` | Exactly what a module drawing shares with its row in the Module Registry: read and derived from a module file, and written back |
| `EndProfile`, `PlateShape` | The end of a module, its plate and its tracks with their gauges and rail system |
| `ModuleStandard`, `Catalogue` | A module standard, with its catalogue GUID, and the catalogue of standards, end profiles and overrides |
| `Hall`, `HallPlan` | A hall, and a layout set up in it: the drawn layout, anchors, areas, paths and acknowledgements |
| `DrawnLayout` | The operating layout with the modules it is made of, the lines that order them, the joints, and the end profiles, standards and overrides they need. `drawnLayout.ToLayout()` gives the operating layout with its stretches worked out from the lines |
| `ModuleMeasurement`, `StationTrackMeasurement` | The measures of a module and the lengths of its station tracks, from its drawing |
| `DrawingJsonContext`, `ModuleFileContent` | The drawing files: module, end profile, catalogue, hall and hall plan |

Reading and writing the files is done by `Tellurian.Trains.Schedules.Importers.Drawing` and
`Tellurian.Trains.Schedules.Exporters.Drawing`.
