The **Cargo flow** tab plans freight that is directed by waybills, in two steps.

### Cargo Description tab

**Cargo descriptions** are reusable descriptions of where wagons are routed. Each has a name and a
set of **destinations** — the operational places wagons are brought to, optionally with a position in
the train and a maximum number of wagons or axles. 

#### Destinations

A destination can also include the station's
- everything **beyond** it, when a shunting yard in the middle of the layout shunts wagons to later destinations,
- its **local** destinations, when the station is a hub for one or several local  freight servces.
- **regions**, when a shadow shunting yard represents some part of the rest of the world.

A description may forward
wagons from one or more **origin** stations, and can be marked as going to **all destinations**. The
same description can be referenced by many cargo flows, so editing it updates them all.

### Cargo Train tab
**Cargo trains** is where you attach cargo flows to trains. Choose a train, then for each cargo flow
set where wagons are **connected** (the from-call, a departure where the train stops) and
**disconnected** (the to-call, a later arrival), its position in the train, and which cargo
description applies. A cargo flow shows as a "brings wagons to …" note at its from-call.

Cargo descriptions belong to the timetable; deleting one is blocked while any cargo flow still uses it.
