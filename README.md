# Tellurian.Trains.Schedules.Importers

**This repository contains features to validate and import schedule data into an object model.
The objetc model can then be mapped to storage in databases or files.**

> NOTE: This software only reads data into an object model in menory and has no logic for storing data.
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
