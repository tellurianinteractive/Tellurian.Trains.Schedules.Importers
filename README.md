# Tellurian.Trains.Schedules.Importers

**This repository contains features to validate and import schedule data into an object model.
The objetc model can then be mapped to storage in databases or files.**

> NOTE: This software only reads data into an object model in memory and has no logic for storing data.
> That have to be implemented elsewhere. This separation of reading and writing is flexible in choosing storage format.

## Output
This project produces four NuGet packages:
- **Tellurian.Trains.Schedules.Importers.Interfaces** defining import operations ([README](Interfaces/README.md))
- **Tellurian.Trains.Schedules.Importers.Model** defining the object model ([README](Model/README.md))
- **Tellurian.Trains.Schedules.Importers.Access** with logic to read the prototype's Access database ([README](Access/README.md))
- **Tellurian.Trains.Schedules.Importers.Xpln** with logic to read XPLN .ODS-files ([README](Xpln/README.md))

## Access Importer
Validates and imports timetable data from the [timetable prototype app](https://github.com/fjallemark/TimetablePlanningApp).
It us currently only experimental and incomplete.

## XPLN Importer
Validates and imports .ODS-files containing XPLN planning data.
XPLN is the defacto tool within the FREMO community to create model railway schedules.
See the [Xpln README](Xpln/README.md) for detailed information.

## Validation
The importers perform extensive validation in two phases:
1. **Referential integrity** - ensures all references between objects are valid before import.
2. **Scheduling conflicts** - detects potential issues like track occupation conflicts,
   single-track collisions, unrealistic train speeds, and overlapping vehicle assignments.

Validation messages are available in English, German, Danish, Norwegian, and Swedish.
See the [Model README](Model/README.md#validation) for details on all validation checks.
