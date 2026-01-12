# Release Notes

## Version 2.1.0

### Breaking Changes

- **`OperationLocation` is now abstract** - The base class `OperationLocation` is now abstract with three concrete subclasses:
  - `Station` - A manned operation location with a dispatcher
  - `SignalControlledLocation` - An unmanned location controlled by signals from another station
  - `OtherLocation` - An unmanned location without signal control

  Code that previously used `new OperationLocation(id, name, signature)` must now use `new Station(id, name, signature)` or one of the other subclasses.

- **`IsShadow` property moved** - The `IsShadow` property has been moved from `OperationLocation` to `Station`, as shadow yards are only applicable to manned stations.

### New Features

#### DispatchStretch

A new `DispatchStretch` class represents a stretch between two manned `Station` objects, useful for dispatch planning:

```csharp
var dispatchStretch = new DispatchStretch(id, fromStation, toStation);
```

The `Layout` class now includes a `DispatchStretches` collection and a `CreateDispatchStretches()` extension method that automatically generates dispatch stretches by following track stretches from station to station:

```csharp
var dispatches = layout.CreateDispatchStretches();
```

#### SignalControlledLocation.ControlledBy

Signal-controlled locations can now reference their controlling station:

```csharp
var block = new SignalControlledLocation(id, "Block A", "BA");
block.ControlledBy = controllingStation;
```

#### JSON Polymorphic Serialization

The `OperationLocation` hierarchy now supports JSON polymorphic serialization using System.Text.Json:

```csharp
// Serialization includes a $type discriminator
// "Station", "SignalControlled", or "Other"
```

#### EF Core Table-Per-Hierarchy (TPH) Support

The `ScheduleDbContext` now supports TPH inheritance mapping for `OperationLocation` with a `LocationType` discriminator column.

### XPLN Importer Improvements

- The importer now creates the correct `OperationLocation` subclass based on the XPLN SubType field:
  - `Station` (or empty) creates a `Station`
  - `Block` creates a `SignalControlledLocation`
  - Other values create an `OtherLocation`
- The `Controlled` field in XPLN can specify the controlling station for `SignalControlledLocation`

### Package Updates

All packages have been updated to target .NET 10.0.

---

## Version 2.0.1

Initial stable release with:
- Core domain model for railway scheduling
- XPLN ODS/XLSX import support
- Comprehensive validation system
- Multi-language support (EN, DE, DA, NB, SV)
- Entity Framework Core support
- JSON import/export services
