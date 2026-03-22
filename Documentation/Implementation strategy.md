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

| Concern | Blazor WASM PWA | Blazor Interactive Server | MAUI Blazor Hybrid |
|---|---|---|---|
| Drag-and-drop performance | Excellent (local execution) | Good on LAN; degrades over WAN | Excellent (local execution) |
| Offline capability | Full (service worker caches app) | Requires self-hosted Kestrel exe | Full (native app) |
| File read/write | All browsers (upload + download) | Full (server-side) | Full (native APIs) |
| Deployment | URL — nothing to install | URL or installable exe | Installable (MSIX/ClickOnce) |
| Updates | Automatic on next visit | Manual for installed exe | Manual |
| Multi-user collaboration | Needs added SignalR service | Built-in SignalR | Needs added SignalR service |
| Cross-platform | Any modern browser | Any modern browser | Windows only (no Linux) |

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

## 2. Local Data Storage

### 2.1 Strategy

Use the browser's **IndexedDB** (via SQLite compiled to WASM, or directly) for
working data. The primary data format for save/load remains **JSON files**.

### 2.2 Entity Identity — Surrogate IDs

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

| Entity | Natural key | Notes |
|---|---|---|
| Company | Signature | Case-insensitive |
| OperationLocation (Station etc.) | Signature | Case-insensitive |
| Train | Company + Number | ExternalId excluded — it is an XPLN import artefact, not relevant in the planning app |
| TrackStretch | Start station + End station | Derived from station signatures |
| TimetableStretch | Number | Case-insensitive |
| StationTrack | Station + Number | Case-insensitive on number |
| StationCall | Train + Track + Arrival + Departure | Composite — fully identifies a call |
| TrainPart | From call + To call | Derived from station call keys |
| DriverDuty | Identity | Case-insensitive |
| TrainCategory | Name | Despite being a record type, only name defines uniqueness |
| Vehicle | Company + Number | |
| VehicleSchedule | Sequence of trains | The actual train sequence defines the schedule's identity |
| Wagon | Company + Number | |
| WagonGroup | Company + Number | |
| Timetable | Name | Single per layout in practice |
| Schedule | Name | Top-level container |

### 2.2 File Operations

Standard browser mechanisms cover all requirements:

| Operation | Mechanism | Browser support |
|---|---|---|
| Open/import a file | `<input type="file">` or drag-and-drop onto page | All browsers |
| Save/export a file | Browser download (blob URL) — effectively "Save As" | All browsers |
| Save back to same file (no dialog) | File System Access API | Chromium only (Chrome, Edge) |

The Chromium-only File System Access API provides a more desktop-like save
experience but is **not essential**. The app must work with the standard
upload/download flow on all browsers.

### 2.3 Browser Storage Quotas

SQLite-in-WASM persists data via the Origin Private File System (OPFS) or
IndexedDB. Storage limits are governed by the browser:

| Browser | Quota |
|---|---|
| Chrome / Edge | Up to 60% of total disc space |
| Firefox | Up to 10% of total disc space (max 10 GB per origin) |
| Safari | ~1 GB before prompting; can grow with permission |

For timetable planning data (kilobytes to low megabytes), these limits are not
a practical concern. The app should request **persistent storage**
(`navigator.storage.persist()`) on first launch to prevent the browser from
evicting data under disc pressure.

---

## 3. Collaboration Architecture

### 3.1 Offline-First, Sync-Optional

The primary workflow is **single-user, offline**. Users save and share timetable
files (JSON) manually — sufficient for most FREMO planning scenarios.

### 3.2 File-Based Collaborative Workflow

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
- **Remaps entity IDs** — all entities use surrogate IDs (see §2.2). During
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

| Entity type | Owned by |
|---|---|
| Layout, stations, stretches | Planner 1 (master) |
| Trains operating within a single stretch | Stretch planner |
| Cross-stretch trains | Planner 1 (after merge) |
| Vehicle schedules, vehicles, wagons, wagon groups | Stretch planner (initially), Planner 1 adjusts after merge |
| Driver duties | Planner 1 |

This ownership is a convention enforced by the merge UI, not a hard access
control mechanism. The merge function should display a clear summary of
incoming changes for review before applying them.

### 3.3 Online Collaboration (Future)

When multi-user editing is needed, the architecture adds:

- A **web API** for reading and writing timetable data to a shared store.
- A **SignalR hub** for broadcasting change notifications to connected clients.
- **Entity-level optimistic concurrency** (version stamps on trains, timetables,
  etc.) with conflict prompts when two users modify the same entity.

CRDTs were considered but are unnecessarily complex for structured domain data
like train schedules. Pessimistic locking (lock-on-edit) is an alternative for
simpler scenarios but reduces concurrency.

### 3.3 Solution Structure for Reuse

To support both standalone and collaborative modes (and a potential future
MAUI desktop app), the solution should be structured as:

```
Shared RCL (Razor Class Library)
  └── All Razor components, services, and view models

Blazor WASM PWA project
  └── References shared RCL, hosts in browser

(Future) Web API + SignalR project
  └── Shared data store, collaboration hub

(Future) MAUI Blazor Hybrid project
  └── References shared RCL, hosts in native window
```

Platform-specific services (file access, storage, HTTP) are abstracted behind
interfaces with per-host implementations.

---

## 4. Risks and Caveats

### 4.1 Blazor WASM PWA — Known Issues (as of March 2026)

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

### 4.2 General Recommendations

- **Do not customise the service worker** beyond the update prompt unless
  absolutely necessary — the default template is kept in sync with runtime
  changes across .NET versions.
- **Pin the `service-worker.js` URL** — never fingerprint or rename it, as
  browsers identify the registration by URL.
- **Test upgrade scenarios** — deploy a new version and verify the update flow
  in a real browser, not just via `dotnet run`.
- **Track the upstream issues** listed above; several are under active
  development and may be resolved in .NET 10 servicing updates or .NET 11.
