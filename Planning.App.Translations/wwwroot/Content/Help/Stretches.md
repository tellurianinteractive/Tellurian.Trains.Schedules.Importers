The **Stretches** tab defines how operation locations are connected and which connections make up
each timetable. It has three sub-sections, worked through in order.

### Track stretches

A **track stretch** is the line between two adjacent operation locations, with its distance, number
of tracks, speed and running time. Track stretches describe the physical network and provide the
distances used to check train speeds and to draw the graphical timetable.

A track stretch has a **direction** (from one location to the next). All track stretches must be
defined in the same direction so that a train can run along them without unintended reversals. For
example, with `A→B B→C D→C C→E E→F E→G` a train can run `A→G`, `D→G`, `A→F` and `D→F` without
changing direction; going `A→D` or `F→G` requires a reversal where the lines meet. If you define a
stretch the wrong way round (so two stretches point at each other, or form a loop), a warning lists
the stretches that disagree — fix them by editing one so they all run the same way.

A track stretch that is part of a timetable stretch cannot be deleted until it is removed from that
timetable stretch.

### Dispatch stretches

A **dispatch stretch** runs between two staffed control points — a **manned** station or a **shadow**
station — passing straight through any unmanned locations in between. So `A→B` and `B→C` with `B`
unmanned form one dispatch stretch `A→C`.

Dispatch stretches are generated automatically from the track stretches: press **Regenerate from
track stretches** whenever you change the network or change which stations are manned. Mark a station
as manned or as a shadow station on the **Operation locations** tab. The *Via* column shows the
unmanned locations a dispatch stretch passes through.

### Timetable stretches

A **timetable stretch** is an ordered series of contiguous track stretches that you want to plan and
draw together — typically a line from one end station to another. Timetable stretches are what you
pick from on the **Graphical timetable** tab, and each becomes one graph.

To build one, give it a number (and an optional description), then add track stretches to its route
one at a time. Only track stretches that continue from where the route currently ends are offered, so
the route always stays connected. The editor shows the resulting stations and total distance.

### Recommended work order

Define the track stretches first, then group them into the timetable stretches you want to work with.
