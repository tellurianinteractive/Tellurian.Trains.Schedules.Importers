The **Train categories** tab defines the kinds of train you run and how they are shown.

A category groups trains that share a classification — for example express, local, freight or
shunting. Each category has:

- a **prefix** used in train numbers and labels;
- a **colour** used to draw the train's line on the graphical timetable;
- a **classification** that influences validation and ordering.
- a **company** that runs all trains in the category, can be overriden for individual trains.
- a **preparation time** and a **finishing-up time** in minutes, used for every new train of the
  category;
- a **stop pattern** saying where its trains stop on their way.

Set up the categories you need before assigning them to trains on the **Trains** tab, so each
train can inherit its appearance and rules from its category.

## Preparation and finishing-up times

A train is made ready some minutes before it departs, and put away some minutes after it has
arrived. That is real work: it is when the vehicles and the loco driver are tied up, and it is
counted wherever the app looks for a clash.

The two times you set here are given to every train you create in the category afterwards. The
**Reapply** button beside each of them gives that one time to all the trains the category already
has, and tells you how many were changed. Reapplying the preparation time moves each train's first
arrival; reapplying the finishing-up time moves each train's last departure. Neither changes where
or when a train runs — every departure and arrival on the way stays exactly as it was. The two are
separate actions, so you can change one and leave the other alone.

## Stop pattern

The stop pattern is the list of operating locations where trains of the category stop on their way.
Tick a location and the trains stop there; leave it unticked and they run through. Only the
locations trains of the category could stop at at all are offered — a passenger category is offered
the places passengers get on and off, a freight category the places wagons are exchanged.

The pattern is what a new train's route is built to. Create a train from **Trains**, and it is given
a stop at every ticked location it passes and runs through the rest. Where a train starts and where
it ends are always stops, whatever is ticked: that is where it is made ready and put away.

Ticking nothing leaves the category with **no pattern**, and nothing is constrained: its trains stop
wherever they can hand over what they carry, which is how routes were built before there were
patterns. A category whose trains run non-stop is set up by ticking only the places they do stop at.

A train that stops somewhere the pattern does not name is reported under **Conflicts**, with the
train, the location and the time. Nothing is put right for you, because only you can say which of
the two is wrong: the train that stops where it should run through, or the pattern that is missing
a location its trains serve.

### Filling the pattern in

**Fill in from the trains** ticks the locations where the trains the category already has stop,
replacing what is ticked. It is how a category is set up once its first few trains are planned,
and it is done for you the first time a plan made before there were stop patterns is opened: every
category with no pattern is given the one its trains have been running all along, so nothing is
reported that was not a fault before.

**Clear** takes the pattern away again and leaves the category unconstrained. Note that opening the
plan afterwards fills it in from the trains once more, so a category with trains cannot be left
without a pattern for long.

A shunting category has no stop pattern. Its tasks work at one place and travel nowhere, so there is
no route for a pattern to shape.
