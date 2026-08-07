The **Cargo flow** tab plans freight that is directed by waybills, in two steps.

### Cargo destinations tab

**Cargo destinations** are reusable statements of where wagons are routed. Each holds a set of
**destinations** — the operational places wagons are brought to, optionally with a position in
the train and a maximum number of wagons or axles. 

#### Destinations

A destination can also include the station's
- everything **beyond** it, when a shunting yard in the middle of the layout shunts wagons to later destinations,
- its **local** destinations, when the station is a hub for one or several local  freight servces.
- **regions**, when a shadow shunting yard represents some part of the rest of the world.

A cargo destination can also be marked as going to **all destinations**. The
same one can be referenced by many cargo flows, so editing it updates them all.

**Only wagon classes** limits the flow to certain UIC wagon classes, and **specific cargo** names
the commodity those wagons carry, for example timber or coal. Specific cargo can only be entered
once wagon classes are set, since it narrows them further.

#### Origin locations

Origin locations are the exception, not the rule: leave them empty unless the train is expected to
take wagons that come from a location the train does not serve itself. The typical case is wagons
fed in from a branch line, which are to continue with this train — the origin location tells the
loco driver and the shunter which incoming wagons belong to this train.

An origin location is **not** where the train picks up the wagons. That follows from the cargo
flow's connect call on the Cargo trains tab.

### Cargo trains tab
**Cargo trains** is where you attach cargo flows to trains. Choose a train, then for each cargo flow
set where wagons are **connected** (the from-call, a departure where the train stops) and
**disconnected** (the to-call, a later arrival), its position in the train, and which cargo
destinations apply. A cargo flow shows as a "brings wagons to …" note at its from-call.

Cargo destinations belong to the timetable; deleting one is blocked while any cargo flow still uses it.
