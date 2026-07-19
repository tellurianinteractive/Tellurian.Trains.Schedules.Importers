# Tellurian.Trains.Schedules

**This repository contains features to read schedule data into an object model,
and write it to `JSON` or relational databases.**

## NuGet Packages

The following packages are published to [NuGet.org](https://www.nuget.org/):

| Package | Description |
|---------|-------------|
| [Tellurian.Trains.Schedules.Model](Model/README.md) | Domain model for layouts, timetables, trains, schedules, vehicle-schedule (turnus) building, cargo flow, call notes and validation |
| [Tellurian.Trains.Schedules.Importers.Interfaces](Importers.Interfaces/README.md) | Contracts for import/export operations (`IImportService`, `ImportResult<T>`) |
| [Tellurian.Trains.Schedules.Importers.Services](Importers.Services/README.md) | Shared import/export services for JSON schedule files and reference data |
| [Tellurian.Trains.Schedules.Model.Databases](Model.Databases/README.md) | Entity Framework Core persistence (`ScheduleDbContext`) |
| [Tellurian.Trains.Schedules.Importers.Xpln](Importers.Xpln/README.md) | XPLN ODS/XLSX file importer with two-phase validation |

**Read more:** [Model](Model/README.md) | [Interfaces](Importers.Interfaces/README.md) | [Services](Importers.Services/README.md) | [Databases](Model.Databases/README.md) | [Xpln](Importers.Xpln/README.md)

The following packages are not yet published (experimental):

| Package | Description |
|---------|-------------|
| [Tellurian.Trains.Schedules.Importers.Access](Importers.Access/README.md) | Microsoft Access database importer (Windows-only) |
| [Tellurian.Trains.Schedules.Planning](Planning/README.md) | Planning utilities for creating layouts and schedules |

## Domain Model

The **Model** package holds the whole railway domain, independent of any importer.
See the [Model README](Model/README.md) for the full type reference. Key capabilities:

- **Layouts** — stations, signal-controlled and other locations, track and timetable
  stretches, dispatch stretches, and a `Theme`/`Scale`/country identity.
- **Timetables** — trains with station calls, categories, and `IsStop`/`IsPassthrough`
  as the single source of truth for whether a train stops.
- **Schedules** — vehicles, driver duties, and vehicle schedules built from train parts.
- **Vehicle-schedule (turnus) building** — turn a timetable into vehicle workings with
  `Plan` and `PlanFactory`, derive complementary schedules for the sessions a plan
  leaves out, and enumerate the unique session/day combinations a vehicle works.
- **Cargo-flow planning** — model freight wagons coupled and uncoupled along a train's
  route from a reusable cargo-flow catalogue on the timetable.
- **Call notes** — localised coupling, parking, reinforcement and free-text instructions
  attached to station calls.
- **Structured settings** — layout, graphic-timetable, identity, integration and
  time/speed settings grouped on the layout.
- **Validation** — two-phase referential-integrity and scheduling-conflict checks with
  multi-language messages.

## Import Data

### XPLN Importer
Imports and validates `ODS`/`XLSX` files containing XPLN planning data.
XPLN is the defacto tool within the FREMO community to create model railway schedules.
Because an XPLN file carries no language or country, `XplnImportOptions` supplies these
per import — read from a culture segment in the file name (for example
`Givskud2021.da-DK.ods`) or falling back to the current culture.
See the [Xpln README](Importers.Xpln/README.md) for detailed information.

### JSON Importer
Imports and validated `JSON` files saved according to the data model.
It is a way to store work in progress or to read data created by any
other application that can write data in the correct format.

### Database Importer
Imports and validates data from a relational database that is compatible with 
the database schema defined. The database importer uses **Entity Framework Core**
that is supported by almost all common database brands.

### Access Importer
Validates and imports timetable data from the [timetable prototype app](https://github.com/fjallemark/TimetablePlanningApp).
It is currently experimental and incomplete.

## Export Data

### JSON Exporter
Writes the complete schedule to a `JSON` file. 

### Database Exporter
Writes the complete schedule to a relational database of choice. 
Creates the necessary database schema if it not exists or is complete.
Uses **Entity Framework Core** that is supported by almost all common database brands.

## Validation
The importers perform extensive validation in two phases:
1. **Referential integrity** - ensures all references between objects are valid before import.
2. **Scheduling conflicts** - detects potential issues like track occupation conflicts,
   single-track collisions, unrealistic train speeds, and overlapping vehicle assignments.

### Language Support
Validation messages are available in English, German, Danish, Norwegian, and Swedish.
See the [Model README](Model/README.md#validation) for details on all validation checks.

## Development

### Line endings
This repository uses **LF** line endings for all text files. This is enforced by two
committed files, so it applies to everyone regardless of platform:

- [`.gitattributes`](.gitattributes) — normalises files to LF in the repository.
- [`.editorconfig`](.editorconfig) — tells your editor/IDE to save files with LF.

Because both agree, no conversion happens and no warnings appear. To be safe on Windows,
you can also disable Git's automatic conversion once per machine:

```bash
git config --global core.autocrlf false
```
