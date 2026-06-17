The **Import** tab loads a plan into the application.

## Supported files

- **`.json`** — a planning document previously saved from Timetable Planner.
- **`.ods` / `.xlsx`** — an XPLN timetable spreadsheet, which is imported into the planning model.

Choose a file with **Open schedule**. Large files (up to 50 MB) are supported.

## Import messages

After importing, any messages are listed and grouped by severity (information, warning, error).
The list opens automatically when there are errors. Messages report referential problems (such as
a call referring to a missing station) and scheduling conflicts, so they are worth reviewing even
when the import succeeds.

A successful import becomes the active plan and the summary shows the number of trains, stretches
and stations loaded.
