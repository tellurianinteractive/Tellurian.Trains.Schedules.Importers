# Tellurian.Trains.Schedules

**This repository contains features to validate and import schedule data into an object model.
The object model can then be mapped to storage in databases or files.**

> NOTE: This software only reads data into an object model in memory and has no logic for storing data.
> That has to be implemented elsewhere. This separation of reading and writing is flexible in choosing storage format.

## NuGet Packages

The following packages are published to [NuGet.org](https://www.nuget.org/):

| Package | Description |
|---------|-------------|
| [Tellurian.Trains.Schedules.Model](Model/README.md) | Domain model for schedules, timetables, trains, layouts |
| [Tellurian.Trains.Schedules.Importers.Interfaces](Importers.Interfaces/README.md) | Contracts for import/export operations |
| [Tellurian.Trains.Schedules.Importers.Services](Importers.Services/README.md) | Shared import services (JSON/CSV data) |
| [Tellurian.Trains.Schedules.Model.Databases](Model.Databases/README.md) | Entity Framework Core support |
| [Tellurian.Trains.Schedules.Importers.Xpln](Importers.Xpln/README.md) | XPLN ODS/XLSX file importer |

The following packages are not yet published (experimental):

| Package | Description |
|---------|-------------|
| [Tellurian.Trains.Schedules.Importers.Access](Importers.Access/README.md) | Microsoft Access database importer (Windows-only) |
| [Tellurian.Trains.Schedules.Model.Planning](Model.Planning/README.md) | Planning utilities for creating layouts and schedules |

## XPLN Importer
Validates and imports ODS/XLSX files containing XPLN planning data.
XPLN is the defacto tool within the FREMO community to create model railway schedules.
See the [Xpln README](Importers.Xpln/README.md) for detailed information.

## Access Importer
Validates and imports timetable data from the [timetable prototype app](https://github.com/fjallemark/TimetablePlanningApp).
It is currently experimental and incomplete.

## Validation
The importers perform extensive validation in two phases:
1. **Referential integrity** - ensures all references between objects are valid before import.
2. **Scheduling conflicts** - detects potential issues like track occupation conflicts,
   single-track collisions, unrealistic train speeds, and overlapping vehicle assignments.

Validation messages are available in English, German, Danish, Norwegian, and Swedish.
See the [Model README](Model/README.md#validation) for details on all validation checks.
