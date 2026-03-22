# Tellurian.Trains.Schedules.Importers.Services

Shared import/export services and reference data for railway scheduling.

## Installation

```
dotnet add package Tellurian.Trains.Schedules.Importers.Services
```

## Services

### JsonImportService

Imports schedules from JSON files:

```csharp
var source = new FileInfo("schedule.json");
var importer = new JsonImportService(source);

var result = await importer.ImportScheduleAsync("MySchedule");
if (result.IsSuccess)
{
    var schedule = result.Item;
    // Use the schedule...
}
```

### JsonExportService

Exports schedules to JSON files with reference preservation:

```csharp
var destination = new FileInfo("schedule.json");
var exporter = new JsonExportService(destination);

var result = await exporter.ExportScheduleAsync(schedule);
if (result.IsSuccess)
{
    Console.WriteLine($"Exported to {destination.FullName}");
}
```

The JSON format uses `ReferenceHandler.Preserve` to handle circular references in the object graph.

### CompaniesFromJsonService

Provides railway company data from a bundled JSON file containing European railway operators:

```csharp
var service = new CompaniesFromJsonService();
var companies = await service.GetAllCompaniesAsync();

// Or with custom data file
var service = new CompaniesFromJsonService("/path/to/companies.json");
```

The bundled data includes companies like:
- DB (Germany), SBB (Switzerland), ÖBB (Austria)
- SJ (Sweden), DSB (Denmark), NSB (Norway)
- And many more European operators

### TrainCategoriesFromCsvService

Provides train category definitions from a bundled CSV file:

```csharp
var service = new TrainCategoriesFromCsvService();
var categories = await service.GetAllTrainCategoriesAsync();

// Or with custom data file
var service = new TrainCategoriesFromCsvService("/path/to/categories.csv");
```

Each category includes:
- `Name` - the category name (unique within a layout)
- `Prefix` / `Suffix` - for train number formatting (e.g., "IC 123")
- `IsPassenger` / `IsFreight` - classification flags
- `Color` - for timetable graph display

Bundled categories include:
- Passenger: LocalTrain, RegionalTrain, InterCity, EuroCity, ExpressTrain, etc.
- Freight: FreightTrain, ContainerTrain, OreTrain, TimberTrain, etc.
- Other: EmptyTrain, Shunting, LocoTransport, ConstructionTrain, etc.

## Bundled Data Files

The package includes reference data that is copied to the output directory:

```
CSV/
  TrainCategories.csv    # Train category definitions

JSON/
  OperatingCompanies.json  # European railway operators
```

`NOTE:` Train catecories and operating companies are often specific to a layout.
You can use customised files by providing your own path to the service constructors.

