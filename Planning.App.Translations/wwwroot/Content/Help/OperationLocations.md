The **Operation locations** tab describes the operational places of your railway — the stations and other
locations where trains run, stop or are controlled, together with their tracks.

### How to choose type of operation location?

1. **Signal-controlled**: if **unmanned** and controlled by **signals**, like an intermediate block, a meet/overtake location or a junction.
   This type can also be defined as **controlled by** an adjacent **station**.
2. **Industrial area**: if **unmanned** and **freight-only**. Can have s **key** for unlocking/locking, see *Lock keys** below.. 
3. **Station**: Can optionally be **manned**. A station can be designated as **shadow yard**.
   If unmanned, a station can  be defined as **controlled by** an adjacent **station**, and have a **key** for unlocking/lockin, see *Lock keys* below.
4. **Other**: not **manned**, not controlled by **signals** but needs to be in the timetable, like an operational location is one of:

#### Tracks at an operation location

The tracks you define here are referenced when planning train calls at operation locations. 
- An operation location **must** have at least one **timetabled** track.
- Track allocation for trains will validated, so one track can only be occupied by one train at a time.

#### Platforms

Where a location exchanges **passengers**, each track also has a **platform length** in metres, to one
decimal. A length above zero means there is a platform along that track; zero means there is none, and
passengers can neither get on nor off there. The column is shown only where passengers are exchanged,
since a platform means nothing anywhere else.

A new passenger train is put on a track with a platform — the **main** one of them for choice — at every
location it **stops** at. Where the location has no platform, and where the train merely runs through, it
takes the main track like any other train. That is what happens at a location that exchanges no
passengers: the train stands there without exchanging anything, which is exactly what a meet or an
overtake is.

Tick **Passengers?** for a location whose tracks have no platforms yet and every track is given
a one-metre platform, which you then adjust. The same is done to a plan made or imported before platform
lengths existed, the first time it is opened: every track of a location that exchanges passengers gets one
metre, so the plan goes on working exactly as it did, and you shorten or clear the tracks that in truth
have no platform. A location where one platform has already been recorded is left alone, and so is a
track you add to such a location: you say whether that one has a platform.

A passenger train that stops to exchange passengers at a track with no platform is listed under
**Conflicts**, and you decide which of two things it is: give the track a platform length, or clear the
call's **Arr** and **Dep** boxes on the **Trains** tab, which says the train is merely standing there and
exchanging nothing. Nothing is put right for you, since only you know which it is. Where a location has a
platform at one track only — the usual arrangement at a small station — two passenger trains meeting
there cannot both have it, and the one without it is what gets reported. The whole check can be switched
off under **Settings › Validation**.

#### Which track a train is put on

Where a location has more than one track and somewhere to run on to, each track can say which way
through the location it is for: the **previous** location a train comes from, the **next** one it goes
on to, or both. Tick **both ways** and the same track counts for trains running the other way round as
well. Only the locations reached by a stretch from here are offered, so a track can only be named for a
route a train can actually take.

This is what a **double line** needs: give one track the previous and next locations of the one
direction and the other track the same pair reversed, leave both ways unticked, and each direction gets
its own track. Leave the columns empty and nothing changes from before.

A new train is put on the track that fits its route best. A track named for exactly where the train has
come from and where it is going on to beats one that names only one of them, which in turn beats a track
that names nothing; a track named for a location the train never touches is not used at all, unless every
track at the location is, in which case the train stands on the best of them anyway. Where two tracks fit
the route equally well, a passenger train that **stops** takes a track with a platform, and a train that
runs **through** — and any train with no passengers to exchange — takes the main track. Only tracks in
the timetable are considered while there is one to be had.

The columns are shown only where there is a choice to make. Nothing is thrown away where they are
hidden: take a location down to one track, and what its tracks were named for is there again as soon as
you add another. Delete a location and the tracks named for trains to and from it are released.

#### Where can train meet/overtake?

- At a **manned** station.
- At an unmanned **signal-controlled location**.

#### Where trains stop

The location type decides whether a train may stop, on top of how you set each call. 
- You define if an operation location has exchange of **passengers** and/or **cargo**:
- A **shadow yard** always exchanges both, whatever the two boxes say, because it stands for
everything beyond the modelled layout — so the boxes are shown ticked and disabled for one.

A train also needs something to exchange. A passenger train stops only where **passengers** are exchanged, and
a freight train only where **cargo** is exchanged. 
A mixed train with both passenger and freight wagons can stop at operation locations that exchanges **passengers** and/or **cargo**.

Where a train cannot stop, its Arr and Dep boxes on the **Trains** tab are shown cleared and
cannot be ticked. Nothing is thrown away: turn the exchange back on and any stops planned earlier
are there again.

### Working with the list

The list shows each location's signature, name, type, owner, whether it is a shadow station (terminal for external stations), and how
many tracks it has. For each row you can:

- **Info** — view all details, including the tracks, read-only.
- **Edit** — open an in-place form with the fields for that location's type.
- **Delete** — remove the location. Blocked while any train calls there.

**Add new** asks for the type first (station, signal-controlled, or other location), then opens the
edit form for that type. A new location is saved once its name and a unique signature are filled in.

#### Instructions

A **station** or **industrial area** has an **instructions** field, written in Markdown and shown beside a live
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
