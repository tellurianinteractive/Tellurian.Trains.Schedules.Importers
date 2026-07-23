# Release Notes

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
