# Tellurian.Trains.Schedules

**This repository contains features to read schedule data into an object model,
and write it to `JSON` or relational databases.**

## NuGet Packages

The following packages are published to [NuGet.org](https://www.nuget.org/):

| Package | Description |
|---------|-------------|
| [Tellurian.Trains.Schedules.Model](Model/README.md) | Domain model for schedules, timetables, trains, layouts |
| [Tellurian.Trains.Schedules.Importers.Interfaces](Importers.Interfaces/README.md) | Contracts for import/export operations |
| [Tellurian.Trains.Schedules.Importers.Services](Importers.Services/README.md) | Shared import services (JSON/CSV data) |
| [Tellurian.Trains.Schedules.Model.Databases](Model.Databases/README.md) | Entity Framework Core support |
| [Tellurian.Trains.Schedules.Importers.Xpln](Importers.Xpln/README.md) | XPLN ODS/XLSX file importer |

**Read more:** [Model](Model/README.md) | [Interfaces](Importers.Interfaces/README.md) | [Services](Importers.Services/README.md) | [Databases](Model.Databases/README.md) | [Xpln](Importers.Xpln/README.md)

The following packages are not yet published (experimental):

| Package | Description |
|---------|-------------|
| [Tellurian.Trains.Schedules.Importers.Access](Importers.Access/README.md) | Microsoft Access database importer (Windows-only) |
| [Tellurian.Trains.Schedules.Model.Planning](Model.Planning/README.md) | Planning utilities for creating layouts and schedules |

## Import Data

### XPLN Importer
Imports  and validates `ODS`/`XLSX` files containing XPLN planning data.
XPLN is the defacto tool within the FREMO community to create model railway schedules.
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
