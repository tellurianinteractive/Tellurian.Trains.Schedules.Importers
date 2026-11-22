# Tellurian.Trains.Schedules.Model.Layouts

The shared model of a model railway layout: **operation locations**, their **tracks** and the **stretches** between
them. It is the contract between the planner (Schedules), the Module Registry, Module Draw and Dispatch
(`Documentation/OperationLocations.md`, section 4.2). It refers to nothing but the base library.

```
dotnet add package Tellurian.Trains.Schedules.Model.Layouts
```

Version 0.3 targets .NET 11 (C# 15): the operation locations are a closed class hierarchy. Module Draw uses 0.3.5.

| Type | What it is |
|---|---|
| `OperationLocation` | A closed class: `Station`, `SignalControlledLocation`, `IndustrialArea` or `OtherLocation`, each with only what its kind can have. What the location is (name, signature, country, tracks, what can be exchanged) and what is decided for it in a layout (instructions, controlled by, manned, timings, start labels, lock key, regions …) are properties of the same object |
| `OperationLocationKind` | Station, SignalControlled, IndustrialArea, Other: the kind as a value, worked out from the type (`location.Kind`), and in JSON the type discriminator `"kind"` |
| `StationTrack` | A track, as train planning sees it: designation, lengths in metres, direction, main, stub, scheduled, lockable, usage, and the route through its location |
| `TrackDirection` | Both, LeftToRight, RightToLeft: as seen from the viewing side of the module |
| `OperationLocationRules` | The rules between properties, as checks with a code per broken rule; what a kind can have at all is its type |
| `Layout` | The locations and stretches of a layout, its catalogues (countries, companies, regions), its settings and, when derived from a hall plan, its modules |
| `TrackStretch`, `TimetableStretch`, `DispatchStretch` | The line between neighbouring locations, a route shown as one graphical timetable, and the line between two dispatch endpoints |
| `Module`, `LayoutModule` | What a module is called and who owns it, and a module as placed in a layout, also the plain-line ones |
| `ModuleStandard`, `Scale` | The standard a module is built to, with its catalogue GUID (the Module Registry's id is an external reference), and its scale, from the suite's list of scales (`Scale.Scales`) |

The types are mutable classes. An app that keeps earlier states of what it edits, such as Module Draw with its undo,
copies them: `Copy()` gives a deep copy of what an object owns, and `x.With(c => c.Name = "Tellby")` a copy with a
change, as `with` does for a record. `OperationLocation.Create(kind)` starts a location without a name, and
`OperationLocationRules.Conforming(location, kind)` changes its kind (`ClearedBy` says what would be lost). A reader of
older files calls `OperationLocationJson.Normalise(node)` before deserializing, so that a `"kind"` in another case, not
first, or missing is read.

A module file holds a `Module` with its optional `OperationLocation`, and leaves out
what is decided for a layout. A layout holds its locations with their decisions; when a module is updated from its
source, what the module is is copied onto the existing location and what is decided for it stays.

## The name of a module

- **Designation**: the prefix, a space and the number with four digits, e.g. `SF 0012`; without a prefix the number
  alone, as it is, e.g. `12`.
  The prefix is a property of the module.
- **A FREMO designation** only when an owning person is a FREMO member with a FREMO prefix. Whoever knows the owners
  says so (`HasFremoDesignation`); otherwise the designation must not be presented as a FREMO name.
- **Owners** are one text, the names of all owners separated by commas (`ModuleOwners`). The Module Registry keeps
  who owns a module, and sets or overwrites the text, the prefix and the FREMO mark when a drawing is uploaded.
- **Display name**: the standard and its scale, when stated, followed by its title, without a word said twice
  (`Wording.JoinWithoutRepeatedWords`): standard `FREMO/H0-RE`, scale `H0` and `SF 0012 Tellby` give
  `FREMO/H0-RE SF 0012 Tellby`. Words are runs of letters and digits, compared ignoring case.
- **Title**: the name without standard and scale, e.g. for a label on the module. With a
  prefix, the designation, followed by the name of the operation location when the module is one, e.g.
  `SF 0012 Tellby`. With a number and no prefix: the module's name followed by the number, e.g.
  `Curve 12`. Without a number: the module's name, followed by its configuration label in lower case, e.g.
  `Tellby short`.
- These are worked out, not stored: extension members beside the type they extend (`Module.Designation`, `Module.Title`,
  `Module.DisplayName`), so they are never written to JSON.

## Rules of the contract

- **Lengths are in metres**, of the model, as `double`. No platform is a platform length of 0.
- **Countries are ISO 3166-1 alpha-2 codes** (`SE`), never the id of a table.
- **Ids in a layout are `int`**, unique within the layout for their kind (module, location, track, stretch), and
  everything else refers by them. What identifies a thing outside the layout is in its external references.
- **Left and right** are those of a viewer standing on the viewing side of the module.
- **The kind is the type** of a location, from a closed hierarchy, and in JSON the type discriminator `"kind"`.
- **Parents are never written**: a location's layout, a track's location, a stretch's layout. A stretch is written
  with the ids of its ends (`startId`, `endId`).
- **Unknown JSON properties are kept** (`LayoutPart.ExtensionData`) and written back, so that a reader of an older
  version doesn't damage what a newer one wrote. Set `AllowOutOfOrderMetadataProperties` when reading files
  where `"kind"` may not come first.
- In JSON, property names are written in camelCase and enum values by their name in the code, in PascalCase
  (`"kind": "SignalControlled"`). The serializer options are the user's: `JsonSerializerDefaults.Web` with a
  `JsonStringEnumConverter` that has no naming policy.
- In a database, tables, views and columns have the names of the code, in PascalCase.
- Additions are allowed at any time. A property is never renamed, removed or given another meaning or unit.
