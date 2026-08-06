# Implementation Strategy

> **Status:** Draft
> **Last Updated:** 2026-03-22

This document captures architectural decisions and implementation strategies
for the Timetable Planning System.

---

## 1. Application Architecture — Blazor Hosting Model

### 1.1 Decision

**Primary platform: Blazor WebAssembly (AOT-compiled) as a Progressive Web App (PWA).**

Optional future additions:

- A SignalR-based backend for multi-user collaboration.
- A MAUI Blazor Hybrid desktop app if richer native integration is needed.

### 1.2 Rationale

The choice is driven by the project's core design principle: **local-first, online-capable**.

Three hosting models were evaluated:


| Concern                   | Blazor WASM PWA                  | Blazor Interactive Server        | MAUI Blazor Hybrid           |
| --------------------------- | ---------------------------------- | ---------------------------------- | ------------------------------ |
| Drag-and-drop performance | Excellent (local execution)      | Good on LAN; degrades over WAN   | Excellent (local execution)  |
| Offline capability        | Full (service worker caches app) | Requires self-hosted Kestrel exe | Full (native app)            |
| File read/write           | All browsers (upload + download) | Full (server-side)               | Full (native APIs)           |
| Deployment                | URL — nothing to install        | URL or installable exe           | Installable (MSIX/ClickOnce) |
| Updates                   | Automatic on next visit          | Manual for installed exe         | Manual                       |
| Multi-user collaboration  | Needs added SignalR service      | Built-in SignalR                 | Needs added SignalR service  |
| Cross-platform            | Any modern browser               | Any modern browser               | Windows only (no Linux)      |

**Blazor WASM PWA wins on deployment simplicity and offline capability** while
delivering the local execution performance needed for drag-and-drop-intensive
timetable editing.

**Blazor Interactive Server** was considered but its reliance on WebSocket
round-trips for every UI event introduces latency concerns for drag-and-drop,
particularly over the internet. The standard mitigation — handling drag
animation in JavaScript and calling .NET only on drop — adds development
complexity that WASM avoids entirely.

**MAUI Blazor Hybrid** remains a viable future option. By structuring the UI as
a shared Razor Class Library (RCL), the same components can be hosted in both
a WASM web app and a MAUI desktop app without rewriting.

### 1.3 AOT Compilation

AOT (Ahead-of-Time) compilation to WebAssembly is recommended over the
interpreted mode. AOT provides roughly 2–5x better CPU performance, which
benefits the rendering pipeline and event handling for interactive UI.

Trade-offs:

- **Download size** increases to approximately 30–50 MB (vs 5–15 MB interpreted).
  .NET 9+ supports **partial AOT** — compiling only performance-critical paths
  while leaving the rest interpreted — as a practical middle ground.
- **Build times** increase significantly (minutes rather than seconds).

For a PWA, the larger initial download is acceptable because the service worker
caches the app; subsequent launches load from cache.

### 1.4 .NET Auto Render Mode

.NET 8+ offers an **Auto** render mode that starts as Interactive Server (instant
load) and switches to WASM once the runtime has downloaded in the background.
This was evaluated but **not recommended** for this project due to:

- Components must function correctly in both Server and WASM contexts.
- State is not automatically transferred during the mode switch.
- The added complexity is not justified for a local-first app where users
  will typically revisit the same cached PWA.

---

## 2. UI Styling — No External CSS Frameworks

### 2.1 Decision

**Use only custom CSS with Blazor's scoped CSS isolation (`.razor.css` files).
No external CSS frameworks (Bootstrap, Tailwind, etc.).**

### 2.2 Rationale

- **Minimal footprint.** External frameworks add tens of kilobytes of unused
  CSS. For a PWA cached by a service worker, every byte affects the initial
  download and cache storage.
- **Full control.** The app has a specialised UI (graphical timetable editor,
  split panes, custom tab bar) that does not map well to generic framework
  components. Custom CSS avoids fighting framework defaults.
- **No version coupling.** Framework upgrades can introduce breaking visual
  changes. Custom CSS evolves with the app.
- **Scoped by default.** Blazor's `.razor.css` files are automatically scoped
  to their component, preventing style leakage between components without
  naming conventions or CSS-in-JS tooling.

### 2.3 Conventions

- Each component's styles live in a co-located `.razor.css` file.
- Global styles (resets, CSS custom properties for colours and spacing) live
  in `wwwroot/css/app.css`.
- Use CSS custom properties for theming (e.g. `--primary-colour`, `--border`)
  so the visual identity can be adjusted in one place.
- Use flexbox and CSS grid for layout; avoid absolute positioning.

---

## 3. Local Data Storage

### 3.1 Strategy

Use the browser's **IndexedDB** (via SQLite compiled to WASM, or directly) for
working data. The primary data format for save/load remains **JSON files**.

### 3.2 Entity Identity — Surrogate IDs

All entities use **surrogate integer IDs** as their primary identity, both in
the database and in JSON documents. This is essential because during planning,
the properties that would otherwise serve as natural keys change frequently:

- Train numbers are renumbered as the timetable takes shape.
- Station signatures change as layout decisions evolve.
- Arrival and departure times are adjusted iteratively.

With surrogate IDs, changing any descriptive property is a single field update.
All references (station calls pointing to trains, vehicle schedules pointing
to station calls, etc.) remain intact because they reference the stable ID,
not the mutable property.

Natural keys remain useful for **display**, **validation**, **uniqueness
checks**, and **conflict detection during merge**, but they are not used as
foreign keys in the data model.

#### Natural keys by entity


| Entity                           | Natural key                         | Notes                                                                                  |
| ---------------------------------- | ------------------------------------- | ---------------------------------------------------------------------------------------- |
| Company                          | Signature                           | Case-insensitive                                                                       |
| OperationLocation (Station etc.) | Signature                           | Case-insensitive                                                                       |
| Train                            | Company + Number                    | ExternalId excluded — it is an XPLN import artefact, not relevant in the planning app |
| TrackStretch                     | Start station + End station         | Derived from station signatures                                                        |
| TimetableStretch                 | Number                              | Case-insensitive                                                                       |
| StationTrack                     | Station + Number                    | Case-insensitive on number                                                             |
| StationCall                      | Train + Track + Arrival + Departure | Composite — fully identifies a call                                                   |
| TrainPart                        | From call + To call                 | Derived from station call keys                                                         |
| DriverDuty                       | Identity                            | Case-insensitive                                                                       |
| TrainCategory                    | Name                                | Despite being a record type, only name defines uniqueness                              |
| Vehicle                          | Company + Number                    |                                                                                        |
| VehicleSchedule                  | Sequence of trains                  | The actual train sequence defines the schedule's identity                              |
| Wagon                            | Company + Number                    |                                                                                        |
| Timetable                        | Name                                | Single per layout in practice                                                          |
| Schedule                         | Name                                | Top-level container                                                                    |

### 3.3 File Operations

Standard browser mechanisms cover all requirements:


| Operation                          | Mechanism                                            | Browser support              |
| ------------------------------------ | ------------------------------------------------------ | ------------------------------ |
| Open/import a file                 | `<input type="file">` or drag-and-drop onto page     | All browsers                 |
| Save/export a file                 | Browser download (blob URL) — effectively "Save As" | All browsers                 |
| Save back to same file (no dialog) | File System Access API                               | Chromium only (Chrome, Edge) |

The Chromium-only File System Access API provides a more desktop-like save
experience but is **not essential**. The app must work with the standard
upload/download flow on all browsers.

### 3.4 Browser Storage Quotas

SQLite-in-WASM persists data via the Origin Private File System (OPFS) or
IndexedDB. Storage limits are governed by the browser:


| Browser       | Quota                                                |
| --------------- | ------------------------------------------------------ |
| Chrome / Edge | Up to 60% of total disc space                        |
| Firefox       | Up to 10% of total disc space (max 10 GB per origin) |
| Safari        | ~1 GB before prompting; can grow with permission     |

For timetable planning data (kilobytes to low megabytes), these limits are not
a practical concern. The app should request **persistent storage**
(`navigator.storage.persist()`) on first launch to prevent the browser from
evicting data under disc pressure.

---

## 4. Collaboration Architecture

### 4.1 Offline-First, Sync-Optional

The primary workflow is **single-user, offline**. Users save and share timetable
files (JSON) manually — sufficient for most FREMO planning scenarios.

### 4.2 File-Based Collaborative Workflow

The expected collaboration model is **stretch-scoped division of work** with
iterative merge rounds. This requires no online infrastructure — only a merge
function within the application.

#### Typical workflow

1. **Planner 1** creates the full layout, defines timetable stretches (e.g.
   A–B, B–C), and saves the document.
2. **Planner 1** sends the document to **Planner 2**, who is responsible for
   stretch B–C.
3. Each planner works independently on their assigned stretch — adding train
   categories, trains, and station calls.
4. **Planner 2** sends the updated document back to **Planner 1**, who merges
   Planner 2's additions into the master document, resolves any conflicts,
   and makes adjustments (e.g. cross-stretch trains that span both A–B and
   B–C).
5. Steps 2–4 repeat in successive rounds, each adding a deeper planning
   layer:
   - **Round 1:** Trains and station calls
   - **Round 2:** Vehicle schedules, vehicles, wagons, wagon groups
   - **Round 3:** Driver duties (typically done by Planner 1 alone, possibly
     automated)

#### Merge function requirements

The application must provide a **merge/import** operation that:

- **Accepts a source document** (the file from the contributing planner).
- **Scopes by timetable stretch** — identifies which entities belong to the
  contributing planner's stretch and imports or updates them.
- **Remaps entity IDs** — all entities use surrogate IDs (see §3.2). During
  merge, incoming entities are assigned new IDs in the master document's ID
  space. All internal references within the imported batch (e.g. station
  calls pointing to trains, vehicle schedules pointing to station calls) are
  rewritten to use the new IDs. This is a mechanical, fully automated step.
  After merge, the contributed document is invalid (its IDs no longer match
  the master) and the contributing planner must receive a fresh copy to
  continue work.
- **Detects conflicts** — when both planners have modified the same entity
  (e.g. a cross-stretch train), the merge should flag the conflict and let
  Planner 1 choose which version to keep or manually reconcile.
- **Supports incremental layers** — a merge in round 2 must add vehicle
  schedules to trains that were merged in round 1, without disturbing the
  trains themselves.
- **Preserves layout integrity** — the layout (stations, stretches) is owned
  by Planner 1. Changes to layout structure in the contributed file should be
  ignored or flagged, not silently applied.

#### Preventing stale merges

Once a contributed document has been merged, it must not be merged again —
doing so would create duplicates. The document should carry a **merge token**
(a unique identifier generated when the master document is exported for a
contributor). The merge function:

1. Records each consumed merge token in the master document.
2. On import, checks the incoming document's token against the consumed list.
3. Rejects the import with a clear message if the token has already been used.

When Planner 1 exports a new copy for the next round, a new merge token is
generated. This ensures each round of contributions can only be merged once.

#### Entity ownership model

To make merging predictable, entities have an implicit ownership scope:


| Entity type                                       | Owned by                                                   |
| --------------------------------------------------- | ------------------------------------------------------------ |
| Layout, stations, stretches                       | Planner 1 (master)                                         |
| Trains operating within a single stretch          | Stretch planner                                            |
| Cross-stretch trains                              | Planner 1 (after merge)                                    |
| Vehicle schedules, vehicles, wagons, wagon groups | Stretch planner (initially), Planner 1 adjusts after merge |
| Driver duties                                     | Planner 1                                                  |

This ownership is a convention enforced by the merge UI, not a hard access
control mechanism. The merge function should display a clear summary of
incoming changes for review before applying them.

### 4.3 Online Collaboration (Future)

When multi-user editing is needed, the architecture adds:

- A **web API** for reading and writing timetable data to a shared store.
- A **SignalR hub** for broadcasting change notifications to connected clients.
- **Entity-level optimistic concurrency** (version stamps on trains, timetables,
  etc.) with conflict prompts when two users modify the same entity.

CRDTs were considered but are unnecessarily complex for structured domain data
like train schedules. Pessimistic locking (lock-on-edit) is an alternative for
simpler scenarios but reduces concurrency.

### 4.4 Solution Structure for Reuse

Two hosting scenarios define what has to be shared:

- **a) Blazor WebAssembly** — the standalone offline-first PWA. One user, one
  browser, data in browser storage. This is what runs today.
- **b) Blazor interactive Server** — the route to online collaboration. One
  central instance of the data, with SignalR propagating one user's edits to
  every other browser working on the same plan.

Anything both hosts need therefore lives in `Planning.Components`, and the host
project keeps only what is genuinely host-specific:

```
Planning.Components (Razor Class Library)
  └── Every Razor component — routable pages, reports, layouts, dialogs —
      plus the services they inject and the CSS, JavaScript and data files
      they load. Registered with a single AddPlanningComponents() call.

Planning.App (Blazor WASM PWA)
  └── References Planning.Components. Owns App.razor, Program.cs, the
      index.html shell and its CSS, and the PWA assets (manifest, icons,
      service worker).

(Future) Blazor interactive Server project
  └── References Planning.Components, adds the shared data store and the
      SignalR collaboration hub.

(Future) MAUI Blazor Hybrid project
  └── References Planning.Components, hosts in a native window.
```

Two rules keep the library usable by both hosts:

- **No WebAssembly-only dependencies or APIs.** `Planning.Components` must not
  reference `Microsoft.AspNetCore.Components.WebAssembly`, and must not use
  synchronous JS interop (`IJSInProcessRuntime`), which does not exist over a
  Server circuit. Asynchronous `IJSRuntime` works in both.
- **Component services are Scoped, never Singleton.** Under WebAssembly there
  is one scope, so the two are indistinguishable. Under Server a scope is one
  SignalR circuit — one user's session — while a singleton is shared by every
  connected user. Registering the open plan, dock layout, UI preferences or
  validation state as singletons would leak one user's session into everyone
  else's. Sharing data between users is then something scenario (b) introduces
  deliberately, through a store behind an interface, rather than by accident
  through a service lifetime.

Platform-specific services (file access, storage, HTTP) are abstracted behind
interfaces with per-host implementations. `BrowserStorageService` and
`CrossWindowSyncService` are the ones still tied to browser APIs; scenario (b)
will need them behind an interface so the Server host can persist centrally
instead.

---

## 5. Risks and Caveats

### 5.1 Blazor WASM PWA — Known Issues (as of March 2026)

The Blazor PWA story has several active issues that must be tracked and
mitigated during development.

#### PWA template only available for Standalone WASM

Since .NET 8, the recommended project template is **Blazor Web App** (unified
template with render-mode support). However, the PWA checkbox **only exists on
the older Blazor WebAssembly Standalone App** template. Manually adding PWA
support to a Blazor Web App causes problems — most notably the service worker
asset manifest references non-fingerprinted filenames while the published
output contains fingerprinted files
([dotnet/aspnetcore #65254](https://github.com/dotnet/aspnetcore/issues/65254)).

Microsoft has acknowledged this gap
([dotnet/aspnetcore #48935](https://github.com/dotnet/aspnetcore/issues/48935))
but has not committed to adding PWA support to the Blazor Web App template,
because offline PWA only makes sense for pure WASM — not for server-rendered
or mixed-mode apps.

**Mitigation:** Use the **Blazor WebAssembly Standalone App** template with
PWA enabled. This is the supported path and aligns with our local-first
architecture. The unified Blazor Web App template is not needed since we are
not mixing render modes.

#### AOT + PWA publish failures

There have been reports of AOT-compiled WASM PWA builds failing after
upgrading to .NET 9 and .NET 10, including Mono runtime
`instantiate_wasm_module()` errors
([dotnet/aspnetcore #64083](https://github.com/dotnet/aspnetcore/issues/64083),
[dotnet/runtime #111663](https://github.com/dotnet/runtime/issues/111663)).

**Mitigation:** Verify AOT + PWA publish on each .NET version upgrade before
adopting it. Consider starting with interpreted WASM or partial AOT until the
AOT publish pipeline stabilises, then switch to full AOT when confirmed
working.

#### Service worker cache integrity

A recurring cross-version issue: the service worker asset manifest
(`service-worker-assets.js`) contains SHA-256 integrity hashes. If any
post-processing step (CDN, compression, rewriting) modifies files after
publish, hashes no longer match and the service worker fails silently or
throws during activation
([dotnet/aspnetcore #39016](https://github.com/dotnet/aspnetcore/issues/39016)).

**Mitigations:**

- Host on a static file server that does not rewrite response bodies.
- If using a CDN, disable integrity checks via
  `<BlazorCacheBootResources>false</BlazorCacheBootResources>`.
- Test the full publish-and-update cycle in the real hosting environment,
  not just locally.

#### Service worker update behaviour

The default service worker detects new versions but does not force a reload.
Developers must implement an "update available" prompt. Without this, users
can remain on stale versions indefinitely, or the update mechanism can
flip-flop between old and new versions.

**Mitigation:** Implement the `updatefound` event handler in the service
worker and display a user-visible prompt to reload. Do not use `skipWaiting()`
as it can break in-flight sessions.

### 5.2 General Recommendations

- **Do not customise the service worker** beyond the update prompt unless
  absolutely necessary — the default template is kept in sync with runtime
  changes across .NET versions.
- **Pin the `service-worker.js` URL** — never fingerprint or rename it, as
  browsers identify the registration by URL.
- **Test upgrade scenarios** — deploy a new version and verify the update flow
  in a real browser, not just via `dotnet run`.
- **Track the upstream issues** listed above; several are under active
  development and may be resolved in .NET 10 servicing updates or .NET 11.

---

## 6. Selective Data Import

### 6.1 Problem

The planning app needs to import **specific slices** of data — not just full
schedules. Typical scenarios:

- Import train categories from an earlier plan or a shared reference file.
- Import companies from a web service providing official railway operator data.
- Import a layout (stations and stretches) from a previous plan and build a
  new timetable on top of it.
- Import trains from one timetable stretch in an earlier plan into the current
  plan.

Data sources fall into two categories:


| Source type     | Examples                                        | Access method                                      |
| ----------------- | ------------------------------------------------- | ---------------------------------------------------- |
| **Document**    | Earlier plan (JSON), XPLN file, SQLite database | Read the full document, extract the relevant slice |
| **Web service** | Company registry API, shared category service   | Query with filters, map response to model objects  |

### 6.2 Per-Type Import Interfaces

The existing `ICompaniesService` and `ITrainCategoriesService` already follow
this pattern — one interface per importable data type. The concept extends this
to all data types a user might want to import selectively:

```
ICompaniesService              → IEnumerable<Company>
ITrainCategoriesService        → IEnumerable<TrainCategory>
ILayoutService                 → Layout (stations, tracks, stretches)
ITrainsService                 → IEnumerable<Train> (with station calls)
IVehicleSchedulesService       → IEnumerable<VehicleSchedule>
```

Each interface returns model objects directly. The implementation handles
source-specific details (file parsing, API calls, field mapping) internally.

### 6.3 Data Source Abstraction

A **data source** combines a user-visible identity with the set of import
capabilities it provides:

```csharp
interface IDataSource
{
    string Name { get; }              // Shown in UI dropdown
    string Description { get; }       // Tooltip or detail text
    DataSourceKind Kind { get; }      // Document or WebService

    // Which data types can this source provide?
    bool CanProvide<T>();
}

enum DataSourceKind { Document, WebService }
```

Each data source implements one or more of the per-type interfaces. For
example, a JSON document source implements all of them (it contains a full
schedule), while a company registry web service implements only
`ICompaniesService`.

```
JsonDocumentSource : IDataSource, ICompaniesService, ITrainCategoriesService,
                     ILayoutService, ITrainsService, IVehicleSchedulesService

CompanyRegistryApiSource : IDataSource, ICompaniesService

SharedCategoriesApiSource : IDataSource, ITrainCategoriesService
```

### 6.4 Document Sources — Partial Extraction

For document-based sources (JSON, XPLN, SQLite), the import reads the full
document into memory and extracts only the requested data type. This reuses
existing import infrastructure:

- **JSON files** — deserialise the `Schedule` object, then return only the
  requested collection (e.g. `schedule.Timetable.Trains`).
- **XPLN files** — use the existing `XplnDataImporter` to import the full
  schedule, then extract the relevant slice.
- **SQLite databases** — query only the relevant tables.

The key difference from a full import is that the extracted entities are
**not wired into the current schedule's object graph**. They are returned as
detached objects that the user can review, filter, and selectively add to
the current plan. During addition, surrogate IDs are reassigned (same
mechanism as the merge workflow in §4.2).

### 6.5 Web Service Sources

Web service sources need additional considerations:

- **Filtering** — the interface methods may accept filter parameters (e.g.
  country code for companies, stretch name for trains) to avoid downloading
  unnecessary data.
- **Mapping** — the service implementation maps the API response format to
  model objects. This mapping is encapsulated within the source class.
- **Caching** — results can be cached locally (IndexedDB) to support offline
  use after first fetch.
- **Authentication** — if required, credentials are configured per source
  in the app settings.

### 6.6 Source Registration and UI

Data sources are registered at application startup via dependency injection.
The UI presents them as follows:

1. User opens an **Import** panel and selects **what** to import (e.g.
   "Train categories", "Layout", "Trains").
2. The panel shows a dropdown of **available sources** — filtered to only
   those sources that can provide the selected data type.
3. For document sources, the user selects or uploads a file. For web
   services, the source is pre-configured.
4. The app fetches the data and presents it in a **review list** where the
   user can select which items to import.
5. Selected items are added to the current plan with new surrogate IDs.

```
┌─────────────────────────────────────────────┐
│  Import                                     │
│                                             │
│  What:   [ Train categories      ▾ ]        │
│  From:   [ Earlier plan (JSON)   ▾ ]        │
│          [ Browse... ]                      │
│                                             │
│  ┌─ Available ─────────────────────────┐    │
│  │ ☑ PassengerTrain   Pt   #ff4000    |    │
│  │ ☑ LocalTrain       Lt   #ff0000    │    │
│  │ ☐ FreightTrain     G    #0040ff     │    │
│  │ ☑ InterCity        IC   #ff4000    │    │
│  └─────────────────────────────────────┘    │
│                                             │
│  [ Import selected ]                        │
└─────────────────────────────────────────────┘
```

### 6.7 Conflict Handling on Selective Import

When importing entities that already exist in the current plan (matched by
natural key — see §3.2), the UI should:

- **Highlight duplicates** in the review list.
- Let the user choose per item: **skip**, **replace**, or **keep both**
  (the latter assigns a new natural key, e.g. a different train number).
- For layout elements (stations, stretches), warn that replacing may affect
  existing trains and station calls.

### 6.8 Relationship to Existing Interfaces

The per-type import interfaces complement — not replace — the existing
`IImportService` and `IExportService` interfaces:


| Interface           | Purpose               | Scope                                |
| --------------------- | ----------------------- | -------------------------------------- |
| `IImportService`    | Full schedule import  | Whole document →`Schedule`          |
| `IExportService`    | Full schedule export  | `Schedule` → whole document         |
| Per-type interfaces | Selective data import | Specific entity type from any source |

Full import remains the primary path for opening a plan. Selective import
is used for **enriching** an existing plan with data from other sources.

---

## 7. Localisation

### 7.1 Decision

**Use the `Tellurian.Localization` NuGet package as the unified translation
service. Centralise all planning-app translations in a dedicated
`Planning.App.Translations` project.**

### 7.2 Rationale

The Requirements Specification (§1.2, §7.4) mandates multi-language support
for UI labels, validation messages, note action texts, and report output — at
minimum English, German, Danish, Norwegian, and Swedish.

The existing codebase already contains `.resx` files scattered across three
projects (`Model/Resources/`, `Importers.Xpln/Resources/`,
`Importers.Access/Resources/`). These cover **validation messages** for
their respective domains and should remain where they are — they are part
of the library API and used independently of the planning app.

The planning app introduces a new, larger category of translatable content:

- **UI labels** — tab names, button text, field labels, headings.
- **Note action texts** — localised verbs and phrases assembled at render
  time (see Requirements Spec §4.5).
- **Help and guidance content** — longer-form markdown texts (tooltips,
  onboarding, help pages).

A separate project keeps all this translation material in one place,
co-located and easy to maintain, while the `Tellurian.Localization` library
provides a consistent retrieval API across all resource types.

### 7.3 The Tellurian.Localization Library

The library provides:

- **Multiple resource providers** — `ResxResourceProvider` for compiled
  `.resx` resources, `MarkdownResourceProvider` for `.md` files with
  language suffixes, and `ObjectResourceProvider` for database entities
  with per-language columns.
- **Fallback chain** — specific culture (e.g. `sv-SE`) → language (`sv`)
  → default culture (`en-GB`) → language (`en`) → resource key as
  last resort.
- **Dependency injection** — providers are registered as keyed singletons
  and resolved via `[FromKeyedServices("Resx")]` etc.
- **`ILanguageService`** — reports supported languages and the fallback
  language; used by the UI to populate language selectors.

### 7.4 Supported Languages

```csharp
var languages = new List<Language>
{
    new("en", true) { IsFallback = true, CultureCode = "GB" },
    new("sv", true) { CultureCode = "SE" },
    new("de", false) { CapitalizesNouns = true },
    new("da", false),
    new("nb", false),
};
```

English (British) is the fallback language. German is marked with
`CapitalizesNouns = true` for correct noun casing in generated text.
Languages are marked `IsFullySupported = false` until their translations
are complete; the library falls back to English for any missing keys.

### 7.5 Project: Planning.App.Translations

A new class library project referenced by `Planning.App`:

```
Planning.App.Translations/
├── Planning.App.Translations.csproj
├── Resources/
│   ├── Labels.resx              ← UI labels (English baseline)
│   ├── Labels.sv.resx
│   ├── Labels.de.resx
│   ├── Labels.da.resx
│   ├── Labels.nb.resx
│   ├── NoteActions.resx         ← Note action texts (English baseline)
│   ├── NoteActions.sv.resx
│   ├── NoteActions.de.resx
│   ├── NoteActions.da.resx
│   └── NoteActions.nb.resx
└── Content/
    ├── help-settings.md         ← English (neutral, no suffix)
    ├── help-settings.sv.md
    ├── help-settings.de.md
    └── ...
```

The `.csproj` references `Tellurian.Localization` and embeds the markdown
content files:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>Tellurian.Trains.Schedules.Planning.App.Translations</AssemblyName>
    <RootNamespace>Tellurian.Trains.Schedules.Planning.App.Translations</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Tellurian.Localization" Version="*" />
  </ItemGroup>
  <ItemGroup>
    <Content Include="Content\**\*.md" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
```

### 7.6 Registration in Planning.App

In `Program.cs`, the localisation services are configured and registered:

```csharp
using Tellurian.Localization;
using Tellurian.Localization.DependencyInjection;

builder.Services.Configure<Tellurian.Localization.Settings>(options =>
{
    options.Languages = new List<Language>
    {
        new("en", true) { IsFallback = true, CultureCode = "GB" },
        new("sv", true) { CultureCode = "SE" },
        new("de", false) { CapitalizesNouns = true },
        new("da", false),
        new("nb", false),
    };
    options.ResxTypeNames = new[]
    {
        "Tellurian.Trains.Schedules.Planning.App.Translations.Resources.Labels, Planning.App.Translations",
        "Tellurian.Trains.Schedules.Planning.App.Translations.Resources.NoteActions, Planning.App.Translations",
    };
    options.MarkdownFilesBasePath = "Content";
});

var options = builder.Services.BuildServiceProvider()
    .GetRequiredService<IOptions<Tellurian.Localization.Settings>>();
builder.Services.AddTellurianLocalization(options);
```

### 7.7 Usage in Blazor Components

Components inject the RESX provider group to retrieve translated labels:

```razor
@inject IResourceProviderGroup ResxProviders

<label>@labelText</label>

@code {
    private string labelText = "";

    protected override async Task OnInitializedAsync()
    {
        var content = await ResxProviders.Translated<Labels>("TrainNumber");
        labelText = content.Text;
    }
}
```

For markdown help content, components inject the Markdown provider:

```razor
@inject IResourceProvider MarkdownProvider

@((MarkupString)helpHtml)

@code {
    private string helpHtml = "";

    protected override async Task OnInitializedAsync()
    {
        var content = await MarkdownProvider.GetTranslationAsync(
            "help-settings", CultureInfo.CurrentUICulture);
        helpHtml = Markdig.Markdown.ToHtml(content.Text);
    }
}
```

### 7.8 Language Selection

The app determines the active language from (in priority order):

1. **User preference** — stored in browser local storage via the Settings
   tab (see Requirements Spec §3.1).
2. **Browser language** — `navigator.language`, mapped to the nearest
   supported language.
3. **Fallback** — English (GB).

The selected language is applied by setting `CultureInfo.CurrentUICulture`
at app startup and when the user changes language in settings. This
propagates automatically to all resource provider lookups.

### 7.9 Relationship to Existing RESX Files

The existing `.resx` files in `Model/`, `Importers.Xpln/`, and
`Importers.Access/` remain in place. They provide **domain validation
messages** used by the libraries independently of any UI — for example,
when running imports from a CLI tool or test harness.

The `Planning.App.Translations` project covers a different concern:
**UI-facing translations** for the planning application. There is no
duplication — validation messages and UI labels serve different purposes
and change at different rates.

If the planning app needs to display validation messages (e.g. in an
import results panel), it accesses them through the model's existing
`ResourceManager` instances, not through the `Tellurian.Localization`
providers. This keeps the library contracts stable.

### 7.10 Note Localisation

Structured notes (see Requirements Spec §4.5) are assembled at render
time from:

- **Action text** — retrieved from `NoteActions.resx` via the RESX
  provider (e.g. key `ContinuesAs` → "Continues as" / "Fortsätter som").
- **Value** — the train number, vehicle identity, or destination from
  schedule data.
- **Days prefix and remark** — also from resource files where applicable.

Manual notes use the default-text-plus-translations model defined in the
Requirements Spec §4.5.5. These translations are stored in the schedule
data (per-note, per-language), not in resource files, and are retrieved
via the `ObjectResourceProvider` at render time.
