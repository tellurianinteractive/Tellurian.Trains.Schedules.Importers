The **Trains** tab is where you build trains and their timings.

A train runs along a stretch and calls at operational places along the way. For each train you
set its number and category, and its **station calls** — the places it passes or stops at, with
arrival and departure times. The default station times from **Settings › Time and speed** are
used as a starting point and can be overridden per call.

Trains drawn here appear as lines on the **Graphic timetable**, coloured by their category, and
are checked against the validation rules in **Settings › Validation** (speeds, track usage, train
numbers and so on).

## Editing a Call Time

A time is never changed on its own — the rest of the train comes with it, keeping the run and dwell
times it already has. A **departure** works forwards, the way the train runs: let a train stand five
minutes longer and it reaches every later place five minutes later. An **arrival** works backwards:
ask the train to arrive five minutes later and it leaves every earlier place five minutes later. The
times on the other side of the call you edit stay where they are, so what changes at that call is how
long the train stands there. A change that would take the train outside the plan's operating times is
refused as a whole, and the field falls back to the time already stored.

## Add Train Dialogue

To add trains should be easy.
You add trains in the **Add Train** dialogue. There are a number of options:
- Create a single train.
- Create a single train with a return train in opposite direction.
- Create a repeated number of trains with a given interval between them.
- Combination of return train and repeated trains. 

A return train runs the same route back again, with the same category and speed, and takes the next
free train number of the opposite direction. Its departure is either **as soon as possible** — the
first train's arrival, plus the time to finish it and prepare the return — or a time you enter,
which may be either before or after the first train's own departure. Combined with repeated trains,
both directions are created first and then repeated, so the two directions are numbered as pairs.

Creating trains fails when some part of any train falls outside the
start- and end time of the plan. However, in **Settings** you can enable  
to let the plan's start- and end times expand with added trains.

## Operating Sessions or Days
The planning of trains can utilise the option to define what sessions (or weekdays) a train runs.
The default is always **All/Daily**. You defined the different session patterns in **Settings > Session & Days**. 
There are common predefined patters, but you can add your own.
