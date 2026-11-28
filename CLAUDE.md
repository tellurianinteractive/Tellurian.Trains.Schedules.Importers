# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Tellurian.Trains.Schedules is a .NET 11.0 solution for validating and importing railway schedule data into an object model. It reads data from XPLN spreadsheets (ODS/XLSX) and Microsoft Access databases, with optional Entity Framework Core support for persistence.

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

### Role in the suite (owner's decision 2026-11-20)

This repository holds the complete model of the Tellurian Trains suite, in three layers: **Layouts** (`Model.Layouts`, what Module Draw makes), **Timetables** (`Model`, what the planner adds) and **Dispatching** (what Dispatch adds; not here yet). Module Draw, the Timetable Planner and Tellurian Dispatch use these models and add only app-specific functionality and user interface; the Module Registry keeps its own model and is extended later from the gaps. JSON import/export must use source-generated serialisation (`JsonSerializerContext`, `[JsonSerializable]`, `[JsonPolymorphic]`/`[JsonDerivedType]`), never reflection, for AOT. All reading and writing of data in every format (importers, and exporters still to be added) and all serialisation code belong here, so files, databases and API calls share one implementation. When an app needs something the model lacks, extend the model here instead of adding types or mapping code in the app. Background: `Documentation/CrossAppReview.md` (section 0 and the cross-app findings), `Documentation/ModelHarmonisation.md` (decisions and progress), `Documentation/OperationLocations.md`.

```
Importers.Interfaces/  → Reading contracts: IImporter<T> (ILayoutImporter is IImporter<Layout>), IImportService, ImportResult<T>
Exporters.Interfaces/  → Writing contracts: IExporter<T>, ExportOptions, IExportService, ExportResult<T> (reading and writing are separate packages, CrossAppReview decision 18)
    ↓
Model.Files/           → The file envelope and the chain of migrations (FileEnvelope, FileKind, FileMigration, FileError); no dependencies
Model.Geometry/        → Pure 2D geometry (points, poses, transforms, line/arc segments, paths, distances); millimetres and degrees; no domain knowledge, no dependencies
Model.Layouts/         → The operating layout: Layout, OperationLocation (closed hierarchy), StationTrack, TrackStretch (with what was measured), StretchLine, LayoutModule (a module without its drawing), LayoutComparison/LayoutUpdate; no dependencies
    ↓
Model.Drawings/        → The drawn layout (ModelSurfaceAnalysis.md 5): Module with ModuleDrawing, EndProfiles/, Catalogues/, Halls/ (HallPlan), Measures/, Modules/, Tracks/, Lines/, DrawnLayout (drawnLayout.ToLayout() gives the operating layout), ModuleRegistryData, Files/ (drawing file kinds, DrawingJsonContext); builds on Model.Layouts, Model.Geometry and Model.Files
Model/                 → Planning model: Plan, Timetable (settings, companies, timetable/dispatch stretches, LocationPlanning and StationTrackRoute records by id), Train, schedules, duties; plan files (PlanFiles/); builds on Model.Layouts and Model.Files, never on the drawings
    ↓
├─ Importers.Xpln/     → XPLN ODS/XLSX importer (published to NuGet)
├─ Importers.Access/   → Microsoft Access importer (experimental, Windows-only)
├─ Importers.Services/ → Shared import services (JSON/CSV data files, hall plans via Model.Drawings)
├─ Exporters.Services/ → Plan file writing: PlanJsonExporter, JsonExportService
├─ Importers.Drawing/  → Reads Module Draw's drawing files (end profiles, catalogues, halls, modules, hall plans), with the migrations
├─ Exporters.Drawing/  → Writes Module Draw's drawing files, counting up revisions
├─ Planning/           → Planning utilities for creating layouts and schedules
└─ Model.Databases/    → Entity Framework Core support (ScheduleDbContext)
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
- `Layout` / `TrackStretch` / `DispatchStretch` - Physical railway infrastructure
- `OperationLocation` (abstract) - Base class for locations; subclasses: `Station`, `SignalControlledLocation`, `OtherLocation`
- `StationCall` - Scheduled stop with arrival/departure times
- `LocoSchedule` / `TrainsetSchedule` - Equipment assignments (implement `VehicleSchedule`)
- `ValidationSettings` - Configurable validation parameters (part of `LayoutSettings`)

### XPLN Importer (in Importers.Xpln/)

- `XplnDataImporter` - Main import orchestrator
- `DataSetProviders/` - Spreadsheet reading abstraction
- `Extensions/` - Data transformation logic (DataSetExtensions, LayoutExtensions, TrainExtensions, StringExtensions)

## Testing

Test projects use MSTest.Sdk with Microsoft.Testing.Platform runner (parallel execution enabled). Test data includes real-world XPLN files with intentional data integrity issues to demonstrate validation.

## CI/CD

Single workflow (`.github/workflows/publish.yml`, named after the nuget.org Trusted Publishing policy) with three jobs:

1. **build** - Always runs: checkout, restore, build, upload artifacts
2. **test** - Always runs after build: runs Model.Tests, Importers.Interfaces.Tests, Importers.Xpln.Tests
3. **publish** - Only runs when version in Directory.Build.props is bumped: packs and publishes to NuGet.org

## Language & Framework

- C# with nullable reference types enabled
- Implicit usings enabled
- SourceLink GitHub integration for debugging
- Local NuGet output configured to `C:\NuGets\`

## Code Review

Reviews and the definition of done follow the user-level `tellurian-review` skill, the strict baseline common to all Tellurian.Trains repos. Rules in this file add to it; where they contradict it, raise the conflict.
