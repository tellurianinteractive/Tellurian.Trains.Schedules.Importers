# Welcome to the Timetable Planner
This is where you plan a model railway operating session: the
track layout, the trains and their timings, and the schedules and duties that go with them.

## Getting started

- **Start from scratch** with the **New layout** button below — it creates a new plan with sensible
  defaults (in your language), ready for you to fill in, or
- **Partially import** reusable data and then refine it and enter the rest, or
- **Open an existing plan** on the **Import** tab. You can open a previously saved planning
  document (`.json`) or import an XPLN spreadsheet (`.ods` / `.xlsx`).

Once a plan is loaded, the other tabs become useful: build trains on **Trains**, view the
**Graphical timetable**, and assign vehicles and duties onto **Schedules**.

## The workspace tab

Each tab can be opened as a dockable view in the **Workspace**, so you can see several views side
by side — for example the graphical timetable next to the trains list. Drag a tab into the
workspace to dock it.

## Order of work
The tabs are ordered in the recommended steps to enter or import data:

- **Home** — create a new plan, or open/import an existing one.
- **Settings** — values controlling visual appearance and timetable calculation; the layout name,
  theme, scale and default country; and the API key for external services.
- **Countries** — the countries the layout uses; companies and regions each belong to one. Start
  from the default country and add more as you need them.
- **Regions** — geographical directions and foreign countries, used for cargo-flow routing to and
  from operation locations outside the layout.
- **Operation Locations** — all stations and other places where a train's passing times are
  recorded, each with its tracks.
- **Stretches** — the track, dispatch and timetable stretches that connect the operation locations.
- **Companies** — the railway companies that operate the trains; each belongs to a country.
- **Train Categories** — the types of train (such as passenger and freight), each with its prefix,
  colour and optional operating company.
- **Trains** — the trains, their station calls and their times.
- **Graphical Timetable** — the time–distance diagram for each timetable stretch.
- **Schedules** — the vehicle schedules and driver duties that operate the trains.
- **Vehicle Owners** — who brings which of the needed rolling stock.

## Minimal Data Entry

The minimal need for data before adding trains are:
- **Settings** — values controlling visual appearance and timetable calculation; the layout name,
  theme, scale and default country; and the API key for external services.
  from the default country and add more as you need them.
  from operation locations outside the layout.
- **Operation Locations** — all stations and other places where a train's passing times are
  recorded, each with its tracks.
- **Stretches** — the track, dispatch and timetable stretches that connect the operation locations.
- **Train Categories** — the types of train (such as passenger and freight), each with its prefix,
  colour.


## Print Reports
Printable content is prepared on the **Reports** tab and kept separate from the editing tabs.

- **Turnus Cards** for locomotives, trainsets, wagonsets and wagons.

## Reuse Earlier Work
- **Import** — open a saved plan (`.json`) or import an XPLN spreadsheet (`.ods` / `.xlsx`).

## Use the Plan in Other Applications
Share the plan with other tools, or save it to reopen later.

- **Export as JSON** — save the whole plan as a planning document (`.json`) that you can reopen here later
  or share with others.
- **Export to SQL** - posts the JSON document to an online service that creates
  an SQLite database, that can be dowloaded and used in other applications.
