You use the **Settings** tab to control how the current layout's 
timetable is calculated, drawn and validated. 
Settings are saved withing the planning document.

### General

Name the layout and choose its operating window. 
You can optionally select to use **numbered sessions** or **weekdays** when you need to 
have different operations for different session. If all trains, schedules and duties
runs the same all sessions you dont have to anyting, all defaults to **all sessions**.

### Sessions & days

In some planning scenarios the possibility to select sessions/days to operate is useful.
You can set operation sessions/days for;
- Trains, for instance running one train session 1-3 and another 4-6.
- Vehicles, for instance two or three days circulation of locomotives.
- Duties, when they differ in tasks between sessions.

### Time and speed

Set the fast-clock speed and map speed classes to scale and real speeds. The default station
times (minimum stop, loco run-around, train clearance) are used when calculating timings for
stations that do not override them.

### Validation

Choose which checks run and the thresholds they use. Switching a check off suppresses its
messages without removing the underlying data.

### Graphical timetable

Control the appearance of the graphical timetable: the orientation of the time axis, the spacing
of stations and tracks, and which labels (arrival/departure minutes, train category, company) are
shown. Note that if trains continues after 24:00 they wraps and continue 00:00.

### Integration

*Coming*: Api-keys and settings for connecting to external services, such as the Module Registry.
