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

A train also needs something to exchange. A passenger train stops only where **Passengers** is
ticked, a freight train only where **Cargo** is ticked, and a train that is both stops where either
is. Where a train cannot stop, its Arr and Dep boxes on the **Trains** tab are shown cleared and
cannot be ticked. Nothing is thrown away: turn the exchange back on and any stops planned earlier
are there again.

A **shadow yard** always exchanges both, whatever the two boxes say, because it stands for
everything beyond the modelled layout — so the boxes are shown ticked and disabled for one.

### Working with the list

The list shows each location's signature, name, type, owner, whether it is a shadow station (terminal for external stations), and how
many tracks it has. For each row you can:

- **Info** — view all details, including the tracks, read-only.
- **Edit** — open an in-place form with the fields for that location's type.
- **Delete** — remove the location. Blocked while any train calls there.

**Add new** asks for the type first (station, signal-controlled, or other location), then opens the
edit form for that type. A new location is saved once its name and a unique signature are filled in.

#### Instructions

A station or industrial area has an **Instructions** field, written in Markdown and shown beside a live
preview. It is for how *this* location is worked at *this* meeting: which tracks are used for what, how
the shunting is arranged, and what else the loco drivers and the people staffing it need to know. How
the location is operated in general, and any other description of it, is for its owner to provide and
does not belong here.

The field is not shown where there is nothing to instruct: trains only run past a signal-controlled
location, and nobody works an other location, so a train there does what its call says and no more.

#### Editing tracks

The edit form lists the location's tracks, where you can add, edit and delete them. A track can only
be deleted while no train references it; otherwise its delete button is disabled and shows how many
trains use it. Reassign or remove those station calls first.

### Stations and regions

A station can be linked to one or more **regions** from the layout's region catalogue (managed on the
**Regions** tab). This is mainly used for shadow shunting yards, which represent external stations or regions
beyond the modelled railway. If the layout has no regions yet, you can add the standard set from the form.

### Lock keys

Where cargo is exchanged but nobody is on duty — an unmanned station or an industrial area — the
switches are usually padlocked, and the key is kept at a manned station along the line. Set **Lock key
held at** to that station, and give the key a name if the station keeps more than one.

Nothing else has to be planned. A freight train that stops at the key-holding station and later stops at
the location the key unlocks is told, at its departure from the key-holding station, to *pick up key A1
for unlocking Bruket*; when it next calls at that station on the way back, its arrival tells it to *leave
key A1 from Bruket*. A train that only runs past either place is told nothing, since it unlocks nothing.

The key is fetched at the last call at the holding station before the work and handed back at the first
one after it, so a train calling there twice is not asked to carry the key around for an extra visit.

A key only means something while both ends of it hold. Mark the location itself as manned, or take the
manning off the station that keeps the key, and the key stops applying: no notes are made from it, and
**Conflicts** says which of the two changes did it. The key itself is kept, not thrown away, so undoing
that change brings it straight back — and it stays on the form, where you can point it at another
station or clear it altogether.
