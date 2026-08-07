The **Schedules** tab is where you turn a timetable into the vehicle and crew plans needed to run
the session.

Here you build **vehicle schedules** and assign them to locomotives, wagon sets and cargo flows,
so that every train has the equipment it needs and every vehicle has a continuous, sensible
working through the day. Driver duties tie this together into work that one person can perform.

### Schedule

Each schedule is a row: the first column lists the **vehicle(s)** working it, followed by the
**train parts** in working order. 

Use **Build automatically** to chain trains of the same category
that continue from where the previous one arrived (mark a category *Exclude from automatic
scheduling* on the **Train categories** tab to keep it out of this), or **New schedule** to build
one by hand. 

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
-  **+ vehicle** assigns a vehicle, creating a new one when needed; a schedule may carry
several vehicles (such as a locomotive and its coach set). Cargo flows are shown as turnus cards in
the reports rather than here.

#### Operating sessions or days

A schedule's operating sessions becomes the subset of sessions all trains operate. With a schedule with most trains 
operating daily and a pair only sessions 1-5, the whole schedule becomes 1-5. 

#### Validation

The validation rules under **Settings › Validation** (locomotive coverage, driver duties) check
that the schedules you build here actually cover the trains in the timetable.
