The **Graphical timetable** tab draws a train-graph (time–distance diagram) for the timetable
stretches you select.

### Choosing what to show

Tick one or more **timetable stretches** to draw. Each selected stretch is drawn as its own
graph. Stretches come from the **Stretches** tab; if none are defined, there is nothing to draw.

#### Showing one half

When a **break time** is set under **Settings › General**, a **Show** selector appears with
**Whole graph**, **First half** and **Last half**. The first half runs from the start of operation
to the break; the last half runs from the break to the end. Picking a half draws only that part of
the day, which keeps the graph readable on smaller screens — especially with a vertical time axis.
The selector is hidden when no break time is set.

### Copying trains

**Clone trains** copies the trains you select. Give the number of minutes the copy is offset by —
positive is later, negative earlier — and each selected train is copied once.

Tick **Opposite direction?** and the copy runs the same route backwards instead, from where the train
ended to where it began. Every run time and every stop is kept, the preparation and finishing-up times
follow the copy to its own ends, and it takes a number from the opposite direction's series. The
minutes are then counted from the copied train's last departure, so they say how long after that train
is put away the working back sets off.

Tick **Repeat?** under **Repeat trains** to make a series instead of a single copy: the first copy is
made at the offset you gave, and one more every interval until a copy would depart after the end time.
This is the same repeat as when a train is added, but on a train that already exists — so the first
train can be adjusted until it runs as it should before the rest are copied from it.

A copy that would run outside the plan's start and end times is not made.

### Reading the graph

Stations are spaced along one axis and time along the other; each train is a sloped line coloured
by its category. The orientation, spacing and labels are controlled from
**Settings › Graphical timetable**.

### Start and end time
The graph displays the timeframe defined in **Settings > General**. 
A special option makes the graph shows a full day from 00:00 to 24:00 for continous operation. 
A train whose timing continues past
midnight wraps around and continues from 00:00 at the start of the axis.
