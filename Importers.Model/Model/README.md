# TimetablePlanning.Importers.Model

Core domain model for railway timetable planning and scheduling.

## Key Types

| Type | Description |
|------|-------------|
| `Schedule` | Complete schedule with timetable, loco schedules, trainset schedules, and driver duties |
| `Timetable` | Collection of trains within a track layout |
| `Train` | Individual train with station calls and category |
| `Layout` | Physical track layout with stations and stretches |
| `Station` | Railway station with tracks |
| `StationTrack` | Track within a station |
| `StationCall` | Scheduled stop with arrival/departure times |
| `TrackStretch` | Connection between two stations |
| `LocoSchedule` / `TrainsetSchedule` | Equipment assignments |
| `DriverDuty` | Driver shift assignments |

## Installation

```
dotnet add package TimetablePlanning.Importers.Model
```

## Usage

```csharp
using TimetablePlanning.Importers.Model;

// Create a layout with stations
var layout = new Layout { Name = "MyLayout" };
var station = new Station { Name = "Central", Signature = "C" };
station.Add(new StationTrack("1") { IsMain = true });
layout.Add(station);

// Create a timetable with trains
var timetable = new Timetable("Morning", layout);
var train = new Train(101, "P101") { Category = "Passenger" };
train.Add(new StationCall(station.Tracks.First(),
    arrival: new Time(8, 0),
    departure: new Time(8, 5)));
timetable.Add(train);

// Create a complete schedule
var schedule = Schedule.Create("Schedule2024", timetable);
```

## Validation

The model includes a message system for validation feedback:

```csharp
// Messages have severity levels
Message.Error("Track not found");
Message.Warning("Using default route");
Message.Information("Reading worksheet...");
```
