# Tellurian.Trains.Schedules.Model.Databases

Entity Framework Core support for the `Tellurian.Trains.Schedules.Model` domain model: a plan written to a relational
database and read back.

## Installation

```bash
dotnet add package Tellurian.Trains.Schedules.Model.Databases
```

This will also install `Tellurian.Trains.Schedules.Model` as a transitive dependency.

## Usage

```csharp
var options = new DbContextOptionsBuilder<ScheduleDbContext>()
    .UseSqlite("DataSource=plan.db")
    .Options;

// Write a plan. The database holds one plan: it is created anew on each export.
var exported = await new DatabaseExportService(options).ExportScheduleAsync(plan);

// Read it back.
var imported = await new DatabaseImportService(options).ImportScheduleAsync(plan.Name);
```

`context.ReadPlanAsync(name)` reads a whole plan from a `ScheduleDbContext` you already have.

## One plan per database

A plan's ids are local to it, and they are the keys in the database, so a database holds one plan. Before saving, the
plan is prepared as for writing to a file (`plan.PrepareForWriting()`): every track, call and train part is given an id
of its own, and the references by id are brought in line. A plan read from the database is written to a plan file
exactly as it was before.

The database holds any plan a plan file can. Rules such as unique track designations are the model's: they are reported
by validation, not enforced by the database.

## How the model is mapped

- **Kinds of location** share one table (`OperationLocations`), with the kind in the `Kind` column: `Station`,
  `SignalControlled`, `IndustrialArea` or `Other`, as in the plan file.
- **Order.** Every list keeps its order through a `ListPosition` column: on the item where the list owns it, in the join
  table where the list refers to items owned elsewhere (the regions of a station, the stretches of a timetable or
  dispatch stretch, the train parts of a driver duty).
- **JSON columns** hold what is read and written as a whole and never queried: the layout settings, the country
  catalogue, the topology positions, a station's staffing, the participants and vehicle contributions, and, in the
  shared model's own JSON form, the hall plan's modules and standard and a location's lock key. What these refer to by
  id is restored after reading.
- **Unknown properties** (`ExtensionData`) and **external references** are kept as JSON text, so nothing read from
  another system is lost on the way through the database.

## Database provider

This package depends on `Microsoft.EntityFrameworkCore.Relational` but does not include a specific database provider.
Add one of the following packages based on your database:

- `Microsoft.EntityFrameworkCore.SqlServer` - SQL Server
- `Microsoft.EntityFrameworkCore.Sqlite` - SQLite
- `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL
- `Pomelo.EntityFrameworkCore.MySql` - MySQL/MariaDB
