# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Tellurian.Trains.Schedules is a .NET 10.0 solution for validating and importing railway schedule data into an object model. It reads data from XPLN spreadsheets (ODS/XLSX) and Microsoft Access databases, with optional Entity Framework Core support for persistence.

## Build Commands

```bash
# Build entire solution
dotnet build

# Build release
dotnet build --configuration Release

# Run all tests (CI runs Model.Tests, Interfaces.Tests, Xpln.Tests)
dotnet test

# Run a single test project
dotnet test Model.Tests/Model.Tests.csproj

# Run specific test by name
dotnet test --filter "FullyQualifiedName~TrainTests"

# Pack for NuGet
dotnet pack --configuration Release --output ./nupkgs
```

Note: Access.Tests and Model.Databases.Tests are excluded from CI (require Windows-specific drivers).

## Architecture

```
Interfaces/         → Contracts: IImportService, ImportResult<T>
    ↓
Model/              → Domain model: Schedule, Timetable, Train, Layout, Station, etc.
    ↓
├─ Xpln/            → XPLN ODS/XLSX importer (published to NuGet)
├─ Access/          → Microsoft Access importer (experimental, Windows-only)
├─ Services/        → Shared import services (JSON/CSV data files)
├─ Model.Planning/  → Planning utilities for creating layouts and schedules
└─ Model.Databases/ → Entity Framework Core support (ScheduleDbContext)
```

### Key Patterns

- **Rich Results**: `ImportResult<T>` encapsulates success/failure with detailed validation messages
- **Two-Phase Validation**: First referential integrity, then scheduling conflicts (see VALIDATION.md)
- **Provider Pattern**: `IDataSetProvider` abstracts spreadsheet formats (OdsDataSetProvider, XlsxDataSetProvider)
- **Multi-Language Validation**: Messages in English, German, Danish, Norwegian, Swedish via .resx resources
- **Severity Levels**: None, Information, Warning, Error, System

### Core Domain Types (in Model/)

- `Schedule` - Complete schedule with timetables, equipment assignments, driver duties
- `Timetable` - Collection of trains within a track layout
- `Train` / `TrainPart` - Train with station calls and locomotive/trainset assignments
- `Layout` / `Station` / `TrackStretch` - Physical railway infrastructure
- `StationCall` - Scheduled stop with arrival/departure times
- `LocoSchedule` / `TrainsetSchedule` - Equipment assignments (implement `VehicleSchedule`)
- `ValidationOptions` - Configurable validation parameters

### XPLN Importer (in Xpln/)

- `XplnDataImporter` - Main import orchestrator
- `DataSetProviders/` - Spreadsheet reading abstraction
- `Extensions/` - Data transformation logic (DataSetExtensions, LayoutExtensions, TrainExtensions, StringExtensions)

## Testing

Test projects use MSTest.Sdk with Microsoft.Testing.Platform runner (parallel execution enabled). Test data includes real-world XPLN files with intentional data integrity issues to demonstrate validation.

## CI/CD

- **CI** (`.github/workflows/ci.yml`): Runs on push/PR to master, builds and tests Model.Tests, Interfaces.Tests, Xpln.Tests on Ubuntu
- **Publish** (`.github/workflows/publish.yml`): Publishes packages to NuGet.org on release

## Language & Framework

- C# with nullable reference types enabled
- Implicit usings enabled
- SourceLink GitHub integration for debugging
- Local NuGet output configured to `C:\NuGets\`
