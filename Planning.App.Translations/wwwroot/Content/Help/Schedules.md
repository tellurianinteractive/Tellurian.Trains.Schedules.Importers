The **Schedules** tab is where you turn a timetable into the vehicle and crew plans needed to run
the session.

Here you build **vehicle schedules** and assign them to locomotives, wagon sets and cargo flows,
so that every train has the equipment it needs and every vehicle has a continuous, sensible
working through the day. Driver duties tie this together into work that one person can perform.

### Schedule

Each schedule is a row: the first column lists the **vehicle(s)** working it, followed by the
**train parts** in working order. A wagonset that lists its wagons shows how many there are and each
wagon class once after its number, as in *SJ 05 5 x A/B/Fv*.

Use **Build automatically** to chain trains of the same category
that continue from where the previous one arrived (mark a category *Exclude from automatic
scheduling* on the **Train categories** tab to keep it out of this), or **New schedule** to build
one by hand. Building automatically offers the unscheduled trains to the schedules already there
first: a working carries on with whatever continues it in the same category, and an empty schedule
you have made is filled before any new one is created, so a vehicle that is already turning takes on
more work before another is put in service. Only the trains that fit no existing schedule start new
ones, and cargo flows are left as they are.
A schedule worked by a trainset, or by a locomotive ticked **Reversible train?**, has its arrivals
moved to the track its next train departs from, because such a train leaves from the track it came in
on. Where the working ends where it began, its last train arrives at the track the first one leaves
from. This happens when the trainset or locomotive is assigned, when the tick is set, and as trains are
added. A schedule with an ordinary locomotive, or with no vehicle yet, keeps its tracks: its next train
may well use other coaches, which is often decided later in the planning.

The **arrival tracks** button on a row moves the arrivals of that schedule at any time, whatever
vehicle works it, for example once you know that a locomotive keeps its coaches.

On a row, 
- **+ train** appends the next train — choose only part of it (a from/to stop)
when a train must be split between vehicles, for example at a change from electric to diesel
traction. 
- The small **joints** between the train parts say where the vehicle stands and for how long, and
before the first part where it has to be brought from. Click one to work a train into that gap: only
the trains the vehicle could actually make in the time available are offered. A leg that does not
bring the vehicle back to where the working goes on is added all the same and reported as a conflict
until you add the leg back — that is how an out-and-back trip is worked into a layover, a leg at a
time. A joint the working is broken across is marked in amber, and clicking it offers the trains
that bridge it.
- The **pen** on a train part changes how much of its train the schedule works: pick a new from- or
to-stop. The train itself stays; to work a different train, remove the part and add the other one.
The neighbouring part that joins the one you change follows along, so the working stays whole —
shorten a part from A–C to A–B and the return working becomes B–A by itself. When the neighbour's
own train does not call at the new stop it is left as it is, and the gap is reported as a conflict
for you to resolve.
The same dialog says where the vehicles stand when it is not the track the train uses: **Fetch from**
names the track they are fetched from before the train departs, and **Put on** the track they are put
on after it arrives. Each is printed as a note for the loco driver and the dispatcher, in place of the
usual note to couple or uncouple the vehicle. A wagonset's tracks are already in the loco driver's
wagonset table, so the loco driver's note only says to shunt the wagons to the departure track, or to
their arrival track. Where a vehicle works the train on only some of the sessions or days the train
runs, its note starts with those. A track chosen at a station the part no
longer starts or ends at is forgotten.
-  **+ vehicle** assigns a vehicle, creating a new one when needed; a schedule may carry
several vehicles (such as a locomotive and its coach set). Cargo flows are shown as turnus cards in
the reports rather than here. A schedule holding nothing but **shunting tasks** needs no vehicle at
all: such a task is worked by whatever stands at the station, so leaving it without one is complete
rather than unfinished, and it is not reported as a schedule left without a vehicle. Add a travelling
train to the same schedule and a vehicle is wanted again.

Editing a locomotive offers **Reversible train?**. Tick it where the locomotive works a train that can
be driven from either end — one with a driving trailer at the far end, or with a second locomotive
there. Where such a train's route reverses it changes direction by changing cab, so **Update timings**
no longer allows it the time to run the locomotive round, and the stop shrinks to the minimum stop.
A trainset is treated this way already and is offered no tick.

#### Operating sessions or days

A schedule's operating sessions becomes the subset of sessions all trains operate. With a schedule with most trains 
operating daily and a pair only sessions 1-5, the whole schedule becomes 1-5. 

#### Validation

The validation rules under **Settings › Validation** (locomotive coverage, driver duties) check
that the schedules you build here actually cover the trains in the timetable.
