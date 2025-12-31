# Tellurian.Trains.Schedules.Model.EntityFramework

Entity Framework Core support for the `Tellurian.Trains.Schedules.Model` domain model.

## Installation

```bash
dotnet add package Tellurian.Trains.Schedules.Model.Database
```

This will also install `Tellurian.Trains.Schedules.Model` as a transitive dependency.

## Usage

Register the `ScheduleDbContext` in your application:

```csharp
services.AddDbContext<ScheduleDbContext>(options =>
    options.UseSqlServer(connectionString));
```

The context includes DbSets for all domain entities:
- Layout, Company, OperationLocation, StationTrack, TrackStretch, TimetableStretch
- Timetable, TrainCategory, Train, StationCall
- Schedule, VehicleSchedule (LocoSchedule, TrainsetSchedule), DriverDuty, TrainPart
- Note

## Database Provider

This package depends on `Microsoft.EntityFrameworkCore.Relational` but does not include a specific database provider.
Add one of the following packages based on your database:

- `Microsoft.EntityFrameworkCore.SqlServer` - SQL Server
- `Microsoft.EntityFrameworkCore.Sqlite` - SQLite
- `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL
- `Pomelo.EntityFrameworkCore.MySql` - MySQL/MariaDB
