The **Import** tab loads a plan or part of a plan into the application.

### Supported files

- **`.json`** — a planning document previously saved from Timetable Planner.
- **`.ods` / `.xlsx`** — an XPLN timetable spreadsheet, which is imported into the planning model.
- **`.json`** — a hall plan from Module Draw, or a layout written by another tool in the same form. It becomes a new
  plan with the hall plan's operation locations, tracks and stretches; each stretch gets a running time from its distance.

Choose a file with **Open schedule**. Large files (up to 50 MB) are supported.

### A newer revision of the hall plan

When the open plan was made from a hall plan and you open another revision of the same hall plan, you choose: update
the open plan with it, or make a new plan from it. When updating, new distances are taken at once, and so are running
times, unless you have entered your own.

Changes that move or add train calls are listed under **To confirm**: a track that is removed, an operation location of
another type, one that is removed or inserted between two others. **Review** shows what the change does to the trains;
**Apply** makes it. Changes that alter the order of the operation locations can't be applied, since the trains can't
follow them; make them by hand. Opening the same file again lists what is left.

### Partial import

*Coming*: Partial import let you select what items to import from a a plan. 

### Import messages

After importing, any messages are listed and grouped by severity (information, warning, error).
The list opens automatically when there are errors. Messages report referential problems (such as
a call referring to a missing station) and scheduling conflicts, so they are worth reviewing even
when the import succeeds. These messsages most often displays import problems with XPLN-files.

A successful import becomes the active plan and the summary shows the number of trains, stretches
and stations loaded.
