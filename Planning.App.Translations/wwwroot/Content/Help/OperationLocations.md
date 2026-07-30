The **Operation Locations** tab describes the operational places of your railway — the stations and other
locations where trains run, stop or are controlled, together with their tracks.

An operational place is one of:

- a **station**, with one or more tracks where trains can stop, meet, be overtaken or change direction.
- a **signal-controlled location**, such as a block post or junction, and trains do not have timetabled stops.
- an **other location** is any other timed location, for example an unmanned halt or a non-signal controlled junction.

The tracks you define here are referenced when planning station calls and when validating that a
train has somewhere to stand. Define your places and tracks before building trains so their calls
have somewhere to go.

### Where trains stop

The location type decides whether a train may stop, on top of how you set each call:

- At a **station**, a train stops when its call is marked to arrive and/or depart — to meet, be
  overtaken, or exchange passengers or cargo.
- At an **other location** (for example an unstaffed halt), a train may likewise stop per its call,
  but only passengers are exchanged, never cargo.
- At a **signal-controlled location**, a train **never** has scheduled stops; it always passes through, whatever
  the call says.

### Working with the list

The list shows each location's signature, name, type, owner, whether it is a shadow station (terminal for external stations), and how
many tracks it has. For each row you can:

- **Info** — view all details, including the tracks, read-only.
- **Edit** — open an in-place form with the fields for that location's type.
- **Delete** — remove the location. Blocked while any train calls there.

**Add new** asks for the type first (station, signal-controlled, or other location), then opens the
edit form for that type. A new location is saved once its name and a unique signature are filled in.

#### Editing tracks

The edit form lists the location's tracks, where you can add, edit and delete them. A track can only
be deleted while no train references it; otherwise its delete button is disabled and shows how many
trains use it. Reassign or remove those station calls first.

### Stations and regions

A station can be linked to one or more **regions** from the layout's region catalogue (managed on the
**Regions** tab). This is mainly used for shadow shunting yards, which represent external stations or regions
beyond the modelled railway. If the layout has no regions yet, you can add the standard set from the form.
