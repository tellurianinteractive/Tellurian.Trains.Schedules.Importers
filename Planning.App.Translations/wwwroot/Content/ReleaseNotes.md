# Release Notes

## Version 0.2.4

- A new **Duties** tab lets you plan driver duties — the work one loco driver performs
  across a session, as a sequence of the train parts they drive. Each duty is a row: its
  identity, company and operating sessions on the left, the train parts in running order
  on the right.
- Add the parts a driver works with **+ train part**. The picker offers the traction
  parts a driver could take next — those that do not clash in time with the duty and, once
  it has a part, those departing at or after it arrives. Parts need not join at the same
  station: between two parts the driver simply walks to where the next one starts.
- The same train part can be worked by several duties as long as they run on different
  sessions, so one duty can cover the odd sessions and another the even ones.
- Where two parts of the same train in a duty are worked by different traction units, the
  tab now shows a note at the station where the traction unit is exchanged — you do not
  enter it by hand.
- You can give each duty an identity and operating company, choose the sessions it runs,
  and add free-text notes that apply to the whole duty.
- Duties imported from XPLN now share the train parts defined in the vehicle schedules, so
  each part shows the traction unit that works it.
- The plan is checked so that no train part is driven by two duties on the same session
  and no duty has parts that overlap in time; any conflicts are listed and open on the
  **Duties** tab. You can turn this check on or off under **Settings › Validation**.

## Version 0.2.2

### Fixes

- Two trains that never run on the same operating session are no longer reported as
  meeting on a single-track stretch. A train running sessions 1, 3, 5 and one running
  2, 4, 6 can now share the same track without a false warning, because they are never
  out at the same time.
- Conflict checks on double-track (and multi-track) stretches are now precise: a
  stretch is flagged only when more trains occupy it at the same time than it has
  tracks, and only counting trains that run a session in common.

## Version 0.2.1

- Conflict warnings are now shown where you can act on them. Train conflicts appear
  only on the graphical timetable and the **Trains** tab; vehicle and schedule
  conflicts appear only on the **Schedules** tab.
- On the **Schedules** tab a vehicle conflict now highlights just the vehicle it
  concerns, and a schedule conflict highlights just that schedule, so it is clear
  which one needs attention.
- The check that a vehicle returns to where it started now also covers wagonsets and
  cargo, not only locomotives and trainsets, so a wagonset or cargo left out of place
  at the end of the operating period is now reported.

## Version 0.2.0

- The name of the plan you are currently working on is now shown in the top bar,
  so you can always see which document is open.
- The graphical timetable now shows loco driver demand bars, making it easier to
  see how many drivers are needed through the operating session.
- A new **Topology** view (under the **Stretches** tab) shows a schematic diagram
  of your timetable stretches and their branches.

### Fixes

- Track stretches now keep the order you entered them in by default, so the list
  is easier to follow while you check your input. You can still sort by any column.
- Conflicts no longer refer to trains you cannot find: when a train is deleted, its
  station calls are removed with it, so no orphaned calls or false conflicts remain.

## Version 0.1.0

First preview of the Timetable Planner. You can:

- Define track layouts with stations, tracks and stretches.
- Create and edit train schedules with automatic time calculations.
- Assign locomotives and trainsets to trains.
- Build vehicle working schedules (turnus) and print turnus cards.
- Plan cargo flows between stations.
- Display graphical timetables (time–distance diagrams).
- Validate schedules for conflicts and inconsistencies.
- Generate printed output: train cards, station books and driver duty sheets.
- Work in English, German, Danish, Norwegian and Swedish.
