# Release notes

## Version 0.7.0

### New features

- **The shunting yards table can now be placed in the general instructions.** The table of shunting
  yards is printed on the layout page at the back of the general instructions booklet. On a layout with
  many shunting yards it filled that page and pushed the explanation of the cargo flow marks off it. To
  print it in the standing instructions instead, write

  ```
  <ShuntingYards/>
  ```

  on a line of its own under **Settings**, wherever in the text it belongs. The layout page then leaves
  it out, so it is never printed twice. The preview beside the text shows a box where the table will
  go. Written nowhere, the table stays on the layout page as before.

- **Vehicle owners: who brings which rolling stock to the meeting, and where it is to be set up.** The
  **Vehicle owners** tab lists every locomotive, trainset and wagonset — with its count of units where there
  is more than one, and for a wagonset that lists its wagons each wagon class once — with the first session
  (or day) it is in operation, and the station, track and departure where it is to stand before it. Open a row to add its owners: the first brings the unit and sets
  it up on the layout, any further owners bring spares. Pick an owner by typing the first letters of their
  name — or of their surname — so that a name is spelt the same way throughout; a name that matches nobody
  is offered as a new participant. Every owner of a locomotive or trainset, spares included, must give a DCC
  address; enter **0** when the owner is still to provide it. Each item and each owner has a note, and the
  **Participants** view lists everyone with what they bring, where a misspelt name is corrected once for
  all.

  The **Vehicle contributors** report under **Reports** prints the same list on A4 landscape, arranged in
  one of three ways chosen above the pages: **By operation location**, a page per station for its owner,
  with the vehicles to set up there in order of first session and departure; **By owner**, a page per
  participant with what they bring, their DCC addresses and where each item starts; or **By DCC address**,
  every locomotive and trainset brought, in one list. Each station or owner starts on a new page, and
  continues on the next when one page is not enough. Items not in operation are listed last; those nobody
  brings yet come first in the arrangement by owner, under **Not yet booked**. A row's background shows when
  its unit is needed: white when it is in operation on every session, light grey for a spare, and otherwise
  light blue, green or red for an item first in operation on the first, second or third session.

- **A schedule can say its vehicles stand on another track than the train.** When editing a train part
  under **Schedules**, **Fetch from** names the track the vehicles stand on before the train departs,
  and **Put on** the track they are put on after it arrives — a wagonset left in a siding at an
  intermediate station, say. The driver duty booklets and the station dispatch lists print it as a
  note: *Before departure, fetch wagonset 21 from track 3.* on the departure, *After arrival, shunt
  wagonset 21 to track 3.* on the arrival, in place of the note to couple or uncouple the vehicle.
  Where the vehicle works the train on only some of the sessions or days it runs, the note starts with
  those — *1,3,5: Before departure, fetch …* — and a vehicle that works it on none of them gets no
  note. A track a vehicle schedule uses this way cannot be deleted under **Operation locations**.

- **Train compositions can now be printed.** A new report under **Reports** gives each manned station a page
  of its own on A4 landscape, listing every train that departs from it carrying cargo flow wagons or a
  wagonset that lists its wagons. The trains are listed track by track, and in order of departure on each
  track, each with the sessions or days it runs, when it arrives and when it leaves, where it is bound, the
  most it may be made up of, and the turnus of every wagonset it works with. The arrival is empty where the
  train starts its run at the station, and the maximum where nothing restricts the train — the same marks
  as everywhere else say which limit each figure is: a wheel end-on for axles, a wagon end-on for wagons.
  Beside each train its composition is drawn from the front of the train as rectangles: a wagonset as one
  rectangle per wagon, in rake order, with the wagon's class and number; and cargo flow wagons as one
  rectangle per position in the train, listing where they go — with *and local destinations* and *and
  beyond* where the cargo destination says so, its regions in their colours, and the most that may be
  brought there after the place itself. The rectangles stand in the order the wagons stand in the train, so
  nothing has to name the positions; the wagons gathered in one rectangle may be marshalled in any order
  among themselves. Each rectangle is only as wide as what it holds. The composition is what the train leaves with, so wagons it arrived with are shown as well as those
  coupled at the station. A wagonset in the train on only some of the sessions or days is marked with them
  beside its turnus, and cargo flow wagons without a position are shown last.

- **A trainset now arrives at the track it is to leave from.** A trainset, and a locomotive working a
  reversible train, never runs round its train: it leaves the station on the very track it came in on. Where
  such a vehicle works one train after another, the arrival track of each train is therefore put on the track
  its next train departs from, and — where the working ends where it began — the last arrival is put on the
  track the first train departs from, so the vehicle stands ready where the next session fetches it. Only
  arrival tracks are moved; the track a train departs from stays as you set it. A working hauled by an
  ordinary locomotive is left untouched, since the locomotive runs light across the station to whichever
  track its next train stands on. The tracks are put right whenever a train is added to a working — by
  **Build automatically**, or by you, at its end or into a layover — and whenever you assign a vehicle to a
  working, so a working built before its trainset was known is corrected as soon as the trainset is put on
  it. An imported working keeps the tracks it was read with. Should the move put two trains on the same
  track at once, it is listed among the conflicts for you to resolve.

### Changes

- **Days and sessions are listed without spaces.** Where a note or a column names the days or the sessions
  something holds on, they are now written *Mo,We,Fr* and *1,3,5* rather than *Mo, We, Fr* and *1, 3, 5*,
  so the value takes no more room than it must. Days written out in full — *Monday, Wednesday, Friday* —
  are unchanged.

- **A wagonset that lists its wagons now shows them in its label under Schedules.** The label gives the
  number of wagons and each wagon class once — *SJ 05 5 x A/B/Fv* — in place of the wagonset's own class.

- **A remark on a call now says whether it belongs to the arrival or the departure.** The driver duty
  booklets and the station dispatch lists print a call's arrival and its departure on separate lines, so
  under **Trains** the box beside each **Remark** asks which of the two it is for: **Arr** for something
  met or done on pulling in, **Dep** for something done before or on leaving. Where the train only
  arrives or only departs, that half is the only one on offer. Where it passes through, no remark can be
  written — tick **Arr** or **Dep** first — although one already there can still be cleared.

  A remark written with an earlier version, or brought in by an XPLN import, said neither, and so was
  printed in neither the booklets nor the dispatch lists. The first time a plan is opened, each such
  remark is given the departure where the train departs, and the arrival where it only arrives.

- **Each block of a train page in a driver duty booklet now has a colour bar down its left edge.**
  Traction units are marked red, scheduled wagonsets green, cargo wagons with waybills blue and the
  timetable grey, so the blocks can be told apart at a glance, and where a grey bar stops, that train part
  ends. The bars print without background graphics having to be switched on, and every block keeps its
  heading, so a black-and-white print loses nothing.

- **The traction units and scheduled wagonsets on a train page in a driver duty booklet now show their
  tracks.** Each row gives the track the vehicle stands on at the start and the track it is left on at the
  end — the train's own track, or the one its schedule names under **Fetch from** or **Put on**. The column
  naming the vehicle is now headed **Turnus** in both blocks, after the card the vehicle's identity is
  written on. With the tracks in the table, the timetable below no longer names each wagonset and its track:
  it says *Shunt wagons to departure track before departure.* or *Shunt wagons to their arrival track after
  arrival.*, led by the sessions or days when the wagons are shunted on only some of those the train runs.
  The station dispatch lists still name each wagonset and its track.

- **The general instructions booklet now explains how cargo is written in the other reports.** Under
  **Cargo flows** on its last page, a key says that every load limit is a maximum and what the mark after a
  figure counts, what the globe stands for, what *and local destinations* and *and beyond* add to a
  destination, and what a coloured region name means.

- **The front page of the general instructions booklet has room for a longer programme.** The programme is
  set with less space between its lines, its entries and its day headings — the type size is unchanged —
  which makes room for four or five more entries. A programme too long for the page loses its last
  entries, and those are the end of the meeting, which is exactly what people look up.

- **Build automatically now adds to the schedules you already have before it makes new ones.** The
  trains that are not yet in a schedule are offered to the existing schedules first: a working carries on
  with whatever continues it from where it arrives, in the category it is already working, and an empty
  schedule you have made yourself is filled before any new one is created. Only the trains that fit no
  existing schedule start new schedules, so building again after adding a few trains extends the vehicles
  already turning instead of putting new ones in service. Cargo flows are left as they are. The result
  beside the button now says how many train parts the existing schedules took on, how many of them were
  extended, and how many schedules were built.

### Fixes

- **The explanation of the cargo flow marks no longer runs off the general instructions booklet.** The
  wordings *and local destinations* and *and beyond*, and the explanation of a region, were set in a
  column so narrow that each ran to four lines, and on a layout with several shunting yards the last of
  them fell off the foot of the page. The marks and the wordings are now set one list under the other,
  each across the whole page.

- **A timetable continued on the facing page of a driver duty booklet now looks like every other.** When
  a train part is too long for one page, its timetable moves to the facing page, and there it was printed
  without the booklet's own layout: in larger type, without the bold stations and times and without the
  lines between the calls, so a long one could run off the foot of the page.

- **Changing a train's number or category under Trains no longer leaves the edit showing in another
  row.** Either change re-sorts the list, and the number you typed, or the category you picked, could stay
  behind in the row of the train that moved into its place.

- **Dialogues now take in a number as you type it.** The **Duration (minutes)** of a new shunting task,
  the **Minutes** to move or clone trains by, and a vehicle's **Number** under **Schedules** were only read
  once you left the field, so the button that confirms the dialogue — and the warning that a vehicle
  number is already taken — lagged behind until you clicked somewhere else.

- **The graphical timetable now draws a location's tracks in the order you gave them.** It ignored the
  **Order** of the tracks under **Operation locations**, and so could draw them in a different order from
  every list of tracks in the app.

- **A turnus card for a vehicle running on days that do not follow on from each other now names them.** A
  card for Monday, Wednesday and Friday printed *MondayShort,WednesdayShort,FridayShort* instead of
  *Mo,We,Fr*.

## Version 0.6.0

### Changes

- **Service trains are a new kind of train category.** Give a train category the type **Service train**
  under **Train categories** for trains that hand nothing over where they stop: a construction train, or
  a locomotive or trainset moved out of service. Such a train may call where a location exchanges neither
  passengers nor cargo — a work site, for one — and building its route gives it no stops between its
  ends, leaving the one that matters for you to place. Name the category for what its trains do: a train
  that leaves material wagons behind exchanges cargo, and belongs in a freight category.

  A category in a plan made by an earlier version that was neither passenger nor freight — which an XPLN
  import can leave behind — is now shown as a service train, where before it was shown as a passenger
  train.

- **Shunting tasks are a new kind of train.** Give a train category the type **Shunting task** under
  **Train categories**, and the trains of that category are worked at one station over a span of time
  instead of travelling: each has a single call whose arrival is when the work starts and whose
  departure is when it ends.

- **A shunting task's cargo flows say which wagons to shunt.** Add cargo flows to the task under **Cargo
  flow** as to any freight train. A flow whose destination is the task's own station carries wagons that
  have arrived, and the loco driver is told to shunt them out to the cargo customers, naming where they
  came from; a flow bound anywhere else is fetched in from those customers, naming where the wagons are
  going. The instruction is printed in the driver duty booklets and in the station reports.

- **Passenger tickets can now be printed.** A new report under **Reports** gives a return ticket between
  every pair of operation locations that exchanges passengers, folded down the middle, with the busiest
  passenger operator at the selling location printed at the foot of both halves.

- **The timetable report now puts several stretches on one sheet.** Tables too narrow to fill the width
  stand side by side, so a short branch line no longer takes a whole sheet to itself.

- **The graphical timetables can now be printed.** A new report under **Reports** draws every stretch to
  the fixed paper scale set under **Settings → Graphical timetable**, so times and gradients can be
  measured from one sheet to the next.

- **Settings → Graphical timetable is now arranged by what each setting affects.** What the graph shows
  comes first, and under it the spacing used on screen, in pixels, beside the spacing used on paper, in
  millimetres.

- **You can now say what is to be done with the locomotive where a train part ends.** Editing a train
  part under **Schedules** asks whether the locomotive is to be turned and whether it is to be run round
  to the other end, and prints either as an arrival note for the loco driver and the dispatcher.

- **The Topology diagram now draws the whole layout's track, with every operation location shown once.**
  Track is single or double as it really is and in the colours of the timetable stretches running over
  it, and grey where no stretch covers it at all.

- **You can now arrange the Topology diagram yourself.** Drag an operation location to where it belongs
  and the track follows; what you arrange is saved with the plan and printed in the driver duty booklets.

- **A cargo destination limited by both wagons and axles now shows both.** The wagon figure used to
  disappear wherever an axle figure was also set — in the driver duty booklets and in the cargo notes
  alike — although the two boxes stand side by side under **Cargo flow** and either limit can be the one
  that binds: sixteen axles is four bogie wagons but eight two-axle ones.

- **The train pages of a driver duty booklet now say the same in less room.** The sessions column is
  headed by what it tells you about the vehicle — **Runs** — instead of a long noun over a column of
  circles, and the cargo wagons are headed **From** and **To**, as the vehicles above them already were.
  The limits under the heading are written as figures under a single **Max**: the speed with its unit, a
  figure with a circle after it for axles, a square for wagons, and the length as *2.5m*. How many wagons or axles a destination
  takes has moved out of **To** into a **Max** column of its own, where it is read straight down the page
  instead of at the end of a row of place names — and that column appears only when something on the page
  is limited at all.

- **The controls that act on a whole working now stand in a column of their own.** Under **Schedules**
  they have moved to an **Actions** column between the vehicles and the trains, so every row's trains
  begin at the same place.

- **The Reports menu has a new order**, from the general instructions through to the passenger tickets.

### Fixes

- **The installed app now works without an internet connection.** The built-in help, the About and
  release notes texts and the catalogue of ready-made train categories were fetched from the web
  every time they were shown, so they stayed empty when you were offline. They are now stored
  together with the rest of the app when it is installed.

## Version 0.5.1

### Changes

- **What is to be done with the locomotives now appears in the driver booklets and the station dispatch
  lists.** Which locomotive to use, what to couple and uncouple, and fetching it from — or driving it
  back to — the parking track were all worked out from the vehicle schedules but never printed; they now
  stand with the other notes at the call they belong to, and both the driver and the dispatcher see them.
  New among them is the note for a locomotive that has to be circulated to the other end of the train, or
  turned, before the train leaves the other way.

- **The general instructions booklet now prints all of your text, on pages that read properly.** A page
  was credited with more room than it really has, so whatever ran over the foot was quietly dropped; the
  text now carries on to the next page, and a page never ends on a heading alone. **Topology** and
  **Shunting yards** have moved to the very last page, as in the driver duty booklets, and the programme
  on the front page is now set in the booklet's own type sizes instead of the browser's.

## Version 0.5.0

### Changes

- **A reversible train no longer stands waiting for a runaround.** Tick the new **Reversible train?** box
  on a locomotive under **Schedules** where it works a train that can be driven from either end — one with
  a driving trailer or a second locomotive at the far end — and **Update timings** gives the train the
  minimum stop instead of runaround time, bringing every later call forward. A trainset is treated this
  way without anything to tick, and a stand you have deliberately made longer is left as you set it.

- **A track can now say which way through the location it is for.** Each track can name the **previous**
  location a train comes from, the **next** one it goes on to, or both, with a **both ways** box, and a
  new train is put on the track that fits its route best. This is what a **double line** needs: give the
  two tracks the same pair of locations reversed and each direction keeps to its own track. Where two
  tracks fit equally well, a passenger train that stops takes a track with a platform and a train running
  through takes the main track; leave the columns empty and nothing changes from before.

- **A train can now be copied the other way round, and copied over and over.** Tick **Opposite
  direction?** and the copy runs the route backwards, keeping every run and stop time, swapping the
  preparation and finishing-up times and taking a number from the opposite direction's series. The copy
  dialogue also has the **Repeat trains** option, so a train can be created on its own, adjusted until it
  runs as it should, and only then repeated across the day.

- **A track can now say how long its platform is.** Each track of a location that exchanges passengers has
  a **platform length** in metres — above zero means passengers can get on and off there — and a new
  passenger train is put on a track with a platform wherever the location has one. Ticking **Passengers?**
  gives every track a one-metre platform for you to adjust, and a plan made before this is treated the
  same way the first time it is opened, so it goes on working as it did until you shorten or clear the
  tracks that in truth have no platform. A passenger train that stops to exchange passengers where there
  is no platform is now listed under **Conflicts**: either give the track a platform length or clear the
  call's **Arr** and **Dep** boxes, which says it exchanges nothing there. The check can be switched off
  under **Settings › Validation**.

### Fixes

- **Renaming the layout now changes the name everywhere it is shown.** The front page of the general
  instructions booklet, the name in the top bar and the file name a plan is saved under all went on
  showing what the layout was called before. A plan renamed before this is put right the next time it is
  opened.

## Version 0.4.2

### Changes

- **A train can now be worked into the middle of a schedule.** Between the train parts of a row there are
  now small joints saying where the vehicle stands and for how long, and one before the first part saying
  where it has to be brought from; click one to add a train into that gap, and only the trains the vehicle
  could actually make are offered. A leg that does not bring the vehicle back is added all the same and
  reported as a conflict until you add the leg back, which is how an out-and-back trip is fitted into a
  layover. A joint the working is broken across, as an import can leave it, is marked in amber.

- **The app has an icon of its own** — the front of a modern train on a dark blue tile — instead of the
  mark that comes with the tools it is built with. It appears in the browser tab, and on the home screen
  or in the Start menu for anyone who installs the app.

- **Twelve turnus cards now go on a sheet instead of ten.** The cards are 48 mm wide instead of 50, so six
  fit across an A4 sheet held landscape and the sheet still has a margin ordinary printers can reach. They
  are the same height as before and hold the same thing.

- **The timetable rows now stand further apart.** There is a seventh more space around every line, so a
  row is easier to follow across the page and a station easier to pick out of the column. The type and the
  columns are unchanged, so the sheet holds the same trains; a page now takes thirty-nine lines instead of
  forty-five.

### Fixes

- **The timetable report no longer loses the last rows of a page.** Where both directions of a stretch
  were put on one page and did not both fit, the rows with nowhere left to go were cut off rather than
  carried over — the report on screen was set in a larger type than the printed sheet, so its rows stood
  nearly two-thirds taller than the ones being counted. The two are now set identically, how much fits is
  measured on a real page rather than reckoned from the type size, and three lines are kept clear at the
  foot of every page.

- **The cargo flow list now names the destinations wagons are going to.** On **Cargo flow › Cargo
  trains**, the list to choose from read only "Wagons to" with the destinations left off, so one entry
  could not be told from another. The sub-tab and its column are now called **Cargo destinations** rather
  than *Cargo descriptions*.

## Version 0.4.1

### Changes

- **The station dispatch lists can now be saved as documents the station owners can edit.** Choose
  *Station dispatch lists* on the Export menu and each station on duty gets its own document in
  OpenDocument format, meant for sending every owner their own list before the meeting so they can add
  the local instructions only they know; where more than one station is on duty the documents arrive
  together in a zip. Where the pages break is left to the word processor, so the pages still break
  sensibly after an owner has typed — the station name, the phone numbers of the stations it clears
  trains to and from and the column headings repeat at the top of every page, but the part of the day a
  page covers cannot be stated, so pages are numbered instead. The printed sheets on the Reports menu are
  unchanged, and are still the ones to work from during a session.

- **A train hauled by two locomotives at once now says which two.** The conflict named only the train and
  the minutes, so where both were booked over the very same stretch its two halves read word for word the
  same. It is now also marked only on the two schedules holding the doubled work, instead of on every
  schedule working that train anywhere in the day.

- **Two locomotives sharing a train between sessions are no longer reported as a conflict.** Only the
  clock times were compared, so one locomotive taking the train on the odd sessions and another on the
  even — the whole point of arranging it that way — was reported as double-heading. The conflict is now
  raised only where the two are booked for a session in common, and it names those sessions.

## Version 0.4.0

### Breaking changes

- **A vehicle you create is now identified by its operator and number.** On any one session the
  combination may belong to only one vehicle, whichever kind it is, so a wagonset and a locomotive can no
  longer both be *DB 5*; a vehicle with no operator is identified by its number alone, and two vehicles
  may share an identity as long as the sessions they work do not overlap. An **imported** vehicle keeps
  the external id it was imported under, so an imported plan raises no new conflicts. Adding or editing a
  vehicle now refuses an identity another vehicle already holds and requires a number, while existing
  plans are kept exactly as they are, with every vehicle that shares an identity listed among the
  conflicts.

### Changes

- **There is a new report: the station dispatch list.** One set of sheets per station with somebody on
  duty, listing the trains that station handles in time order — a train that stands there appears twice,
  arrivals on white and departures on light yellow, because clearing a train in and clearing it on are
  separate actions, and trains that only run past are listed too. Each page carries the station's name,
  the part of the day it covers and the phone numbers of the stations at the other end of its dispatch
  stretches, and every row has a box per session to tick off. Each station starts on a fresh page, so the
  pile can be torn apart and handed out; print it from the Reports menu.

- **The fields for adding and editing a vehicle are in a new order,** the same in both places: type of
  vehicle, type of traction, number of units, operator, number, class, sessions and last the external id.
  The field previously labelled *Company* is now *Operator*.

- **An external id can be corrected but no longer invented.** The external id is the name a train or a
  vehicle carries in the system it was imported from, so one imported with an id still has its field and
  can be corrected there, while one that never had an id now has no field to type into. A vehicle you
  create in the planner is therefore given no external id, where it used to be given one made up from its
  class and number.

- **The shortest time between two uses of the same track is now checked.** The setting was there but
  nothing acted on it: left at 0, where it starts, nothing about the checking changes. Set it to 5 and the
  track must also be free for five minutes between two trains — exactly five is enough, four is not — and
  the conflict says how short the gap actually is and how long it had to be.

- **An operation location can now carry its own instructions.** The edit form has an **Instructions**
  field, written in Markdown beside a live preview, for how that location is worked at this meeting: which
  tracks are used for what, how the shunting is arranged, and what else the loco drivers and the people
  staffing it need to know. It is offered at a station or an industrial area and shown on the location's
  Info view; it is not offered where there is nothing to instruct.

- **A location where cargo is worked with nobody on duty can now require a key.** Pick the manned station
  that keeps the key under **Lock key held at**, and name the key if that station keeps more than one — a
  freight train that stops at both is then told, as it leaves the key-holding station, to *pick up key A1
  for unlocking Bruket*, and to *leave key A1 from Bruket* when it next calls there. The key is fetched at
  the last call before the work and handed back at the first one after it, and a train that only runs past
  either place is told nothing. Mark the location as manned, or take the manning off the station that
  keeps the key, and the key stops applying — **Conflicts** says which change did it, and the key is kept
  so undoing that change brings it straight back.

### Fixes

- **Two stretches setting off from the same operation location were drawn as if they never met.** Where a
  timetable stretch began at the very first operation location of another, nothing joined the two in the
  Topology diagram. The second now leaves that operation location like any other branch, at the same fixed
  angle.

- **Every validation threshold now says which clock it is measured against.** The shortest time between
  two uses of the same track gave no unit at all, and the two train speeds said only *clock minutes*. All
  three now say fast-clock minutes — the clock the trains run to, not real time.

- **Lengths and distances now spell metres out,** as does the top half of the train speeds, so the *m*
  cannot be taken for a minute. The minimum stop at a station is now labelled in fast-clock minutes too.

## Version 0.3.5

### Fixes

- **A saved plan could refuse to open.** Opening a plan the app had just saved stopped with an error
  naming a country, and nothing was loaded. A plan already saved opens as it stands; there is nothing you
  need to do to it.

- **A saved plan file is about seven times smaller.** Saving wrote the plan in a different form from the
  one kept in the browser, so every stop was written twice, and every train category, operator and country
  again at each train, vehicle and duty that used it. A file that took 8 MB now takes a little over 1 MB;
  a plan saved by an earlier version still opens.

## Version 0.3.4

### Changes

- **The Arr and Dep boxes on a call now follow where the train can actually stop.** A passenger train
  needs a place that takes passengers and a freight train one that takes cargo, and neither can stop at a
  signal-controlled location; where a train cannot stop, both boxes are cleared and cannot be ticked, and
  the call is a pass-through. Nothing you planned is thrown away — turn the exchange back on and the stops
  are there again — and a shadow yard always exchanges both, since it stands for everything beyond the
  layout.

- **A stop something depends on can no longer be taken away.** The train's own first and last call, and
  the ends of every part a vehicle schedule, a driver duty or a cargo flow is planned over, now keep their
  box ticked and disabled, and hovering says what is holding it. Where a part ends somewhere its train
  cannot stop, the box says so plainly, so you can move the call or the part.

- **A train category now carries the preparation and finishing-up times its trains are planned with,** so
  you no longer type the same two numbers for every train. A *Reapply* button beside each field gives that
  one time to all the trains the category already has and says how many were changed; the two are separate
  actions, and reapplying moves only the minutes at the very ends of a train.

- **The operators are easier to read on the front page of a duty booklet.** The line is now twice the size
  it was, so a logo is large enough to be recognised at a glance and a signature large enough to be read
  across a table. Where every operator has a logo the word *Operator* is left out; where any one has none,
  all are given as signatures, in bold and with the label kept.

### Fixes

- **A duty booklet could print a train part off the foot of the page.** Each page was credited with about
  half as much room again as an A5 page really has, and anything past the foot is cut away without a word,
  so the second train part on such a page lost the end of its timetable or did not appear at all. Train
  parts are now measured against what the page really holds, so some booklets will need a sheet more than
  before.

- **The Topology diagram could print the signatures of two operation locations on top of each other.**
  Operation locations were placed purely by the distance between them, so two lying close together on a
  long stretch were drawn almost in the same place. They are now never drawn closer together than their
  signatures need, and a long signature at the edge of the diagram is no longer cut off.

- **A branch in the Topology diagram could be drawn straight through another stretch.** A branch falls
  away at a fixed angle, so one that met a stretch in its way could never get past it and was simply drawn
  across it. The branches that leave furthest along a stretch are now drawn first, so a long branch may now
  be drawn below a short one that leaves the stretch further along.

- **A plan could show its trains under train categories the Train categories tab did not list.** Several
  categories could also be taken for one and the same, gathering their trains under a single heading and
  reporting two trains of different categories that share a number as one number used twice. When a plan is
  opened, the list of categories is now completed from the categories its trains use, and every category is
  kept apart from the others.

- **Two companies that had never been given a number of their own were taken for the same operator,** so
  trains of different companies that shared a train number were reported as one number used twice. Every
  company is now given a number of its own when a plan is opened or saved; a company from the Module
  Registry keeps the number it came with.

- **A plan stored its train categories, companies and countries in more than one place** — each written
  wherever it was first met, usually inside the first train that used it. Each is now written once, in its
  own list, and everything that uses one keeps only a reference; countries are no longer copied into the
  plan at all, so a correction to a country's languages now reaches plans saved before it.

- **A duty booklet gave only the train number in the heading of a train part.** A train is identified by
  the prefix and suffix of its category as much as by its number — Gt 1234, not 1234 — and the heading is
  all a loco driver has to compare with the timetable. It now carries the whole train identity, after the
  operator's signature.

## Version 0.3.3

### Changes

- **Conflicts can now be read where they are shown.** A row that has conflicts — a train or a train
  category under **Trains**, a working or one of its vehicles under **Schedules**, a duty under
  **Duties** — carries a warning symbol, and clicking it opens the messages in a list you can read. The
  symbol takes the colour of the most serious conflict and counts them; they were previously only in a
  tooltip.
- **A train category shows the conflicts of the trains inside it**, so closing the category no longer
  hides them.
- **The Trains tab now opens on the list of train categories**, with the trains hidden until you open one.
  *Expand all* opens them all at once, and a category opens by itself when you add a train to it or move
  one into it.
- **Editing a train part in a working now says which kinds of vehicle the working is for** — locomotive,
  trainset or wagonset. Each kind is named once, and pointing at it names the vehicles themselves.

### Fixes

- **The app could stop saving your work without telling you.** A plan the app could not write out — a
  train left with fewer than two calls, or a timetable stretch whose track stretches had all been
  removed — failed its save silently, so everything done from that moment on stayed on screen but was
  never kept. Both plans now save, and a failed save is reported in the top bar straight away.

- **A saved plan file is about 40 % smaller.** Each stop was written twice — once in its train and once
  under the track it stands on — and the second copy dragged much of the rest of the plan along with it. A
  plan saved by an earlier version still opens.

- **A train left part of its run without a traction unit is now reported.** The check asked only whether a
  locomotive or trainset worked the train *somewhere*, so shortening a working at one end left the rest
  unworked without a word. Every stretch is now checked on every session the train runs, and the conflict
  says between which locations and on which sessions; plans that looked clean may report this now.

## Version 0.3.2

### Changes

- Under **Cargo flow › Cargo descriptions**, an origin or a destination can now be any operation location
  that exchanges cargo, not only a station — an industrial area always handles cargo wagons but could not
  be chosen before. The same lists now say **location** where they said *station*.
- The calls of a train are always listed in the **order the train travels** them.
- Editing a call time in the **Trains** tab now **takes the rest of the train with it**: a **departure**
  works forwards, the way the train runs, and an **arrival** works backwards, so the run up to the change
  follows it. The times on the other side stay where they are, the run and dwell times are kept, and the
  change is refused if it would take the train outside the plan's operating times.
- A train whose route **jumps a location** — two calls in a row with no track stretch between them — is
  now reported as a conflict. It can be switched off under **Settings › Validation**.
- A train part in a **schedule** can now be **edited**: the pen opens its from- and to-stop, so a working
  can be reshaped without removing everything after it. A neighbouring part that joins the one you change
  follows along; one whose own train does not call at the new stop is left as it is, and the gap is
  reported as a conflict for you to resolve.
- **Add train** can now create the **return train** at the same time. Tick *Return?* and the train back is
  created with the outbound one, running the same route in reverse with the same category and speed and
  taking the next number of the opposite direction; its departure is either *as soon as possible* or a
  time you enter. Combined with *Repeat?*, both directions are repeated.

### Fixes

- The **kilometre figures** in the printed timetable and along the graphical timetable are now rounded to
  whole kilometres, and a branch line shows the same kilometre as the line it leaves at their junction
  station.
- Everything that reads a train's route now follows the **order the train runs its stops**, not the order
  they were entered. On a train whose stops went in out of order this drew zig-zag lines on the
  **Graphical timetable**, could show a departure where the train arrives in the printed table, stopped
  **Build automatically** from chaining the train, measured the interval from the wrong stop when
  repeating a train, and made recomputing its times fail outright. Imported plans were never affected.
- **Train speed is now checked on the last leg too**, into the station where the train ends its run.

## Version 0.3.1

### Changes

- The **Traction units** section on a train part page in the Driver duties booklet now has its heading in
  the chosen language. It was the only heading in the booklet left untranslated.
- The traction unit is now printed for every train part that has one. In plans imported with an earlier
  version, some parts showed a traction unit under **Duties** but none in the booklet.
- Notes about trains going the same way now say who passes whom — **Overtakes GD 42757 12:02-12:05** or
  **Is overtaken by GD 42757 12:02** — instead of the old *"Meets GD 42757 in the same direction"*, which
  never said which train got ahead. Two trains that merely stand at the same station at the same time give
  no note at all.
- A meet that lasts no time — the other train runs through without stopping — is printed as a single time
  instead of an interval from a time to itself.
- A train that begins or ends its run at a station is no longer reported as met, crossed or overtaken
  there. Those times are when its loco driver reports for duty or stands down.

## Version 0.3.0

### Changes

- A new **Driver duties report** prints one A5 booklet per duty. The front page shows the duty number, the
  sessions or days it runs, its start and end time and station, a difficulty grade, staffing needs and any
  duty notes; each train part then gets its own page with the traction units to use, the wagonsets to
  bring, the destinations to bring cargo wagons to, and the timetable, each in its own block.
- A new **General instructions** report is a separate booklet with the meeting programme and the
  instructions that apply to a layout for the whole meeting — driving instructions, signalling practice,
  radio and phone use, running late, who to ask — handed out once to everyone. It opens with the meeting
  name and dates, then the programme every participant needs before the first session, then the
  instructions over as many pages as they need, broken between paragraphs and never leaving a heading
  behind.
- The last page of both booklets shows the layout's track plan and the table of shunting yards, so those
  who never hold a duty booklet — station staff above all — still get an overview of the layout.
- The programme and the instructions are both written under **Settings › Information** and can be
  formatted with Markdown. Both booklets print in A5: A4 landscape, double-sided, folded down the middle,
  with blank pages added where needed so the sheets fold correctly.
- Duties can now be graded **Easy**, **Medium** or **Experienced**, shown colour-coded on the booklet, can
  say that they need two or three people — for example a loco driver and a conductor — and can be pinned
  to a **fixed number** that automatic renumbering leaves untouched.
- The plan is now checked so that every train part with a locomotive or trainset assigned has a driver
  duty covering it on each session it runs. A pinned duty must have a number, and no two pinned duties can
  be given the same one.
- Companies can now have an uploaded **logo**, shown in reports in place of the text signature.
- Stations can now be marked as the **shunting yard** that handles another location's local freight, and
  the layout lists every shunting yard and what it covers on the last page of the duty booklet.
- Each timetable stretch can now be given a **colour**, used to draw it in the Topology diagram.
- A new **distance display factor** (Settings › Time and speed) lets a layout show a larger, more
  prototype-like kilometre figure in reports and the graphical timetable than the distance actually
  modelled, without affecting any travel-time calculation.
- The app now keeps multiple open browser tabs or windows in sync with each other. **Note** that this only
  works across windows on the same machine in the same browser.
- Settings can now record the meeting's **valid from** and **valid to** dates, printed as a validity line
  on reports; leave them empty when no meeting is booked yet.
- A new **extend plan times automatically** option (Settings › General) widens the plan's start or end
  time to cover a train instead of blocking the change. Off by default.
- A new **update all timings** button on the graphical timetable recomputes every train in the timetable
  in one go, instead of selecting a subset first.
- Track occupancy checks can now optionally account for a locomotive or trainset standing on a track
  between two trains, unless it is booked to or from parking (Settings › Validation). Off by default,
  since it only makes sense on layouts where parking is modelled deliberately.
- Every call in the **Trains** tab now has a **Remark** field for a note printed at that call — for
  example "wait for the oncoming train". The note reads as finished text and shows the markup you typed as
  soon as you enter the field, so write `*slowly*` for italics and `**first**` for bold.

### Fixes

- Adding a new train now sets its default start time to account for the given preparation time, so it does
  not start before the plan's start time.

## Version 0.2.4

### Changes

- A new **Duties** tab lets you plan driver duties — the work one loco driver performs across a session,
  as a sequence of the train parts they drive. Each duty is a row: its identity, company and operating
  sessions on the left, the train parts in running order on the right.
- Add the parts a driver works with **+ train part**. The picker offers the traction parts a driver could
  take next — those that do not clash in time with the duty and, once it has a part, those departing at or
  after it arrives. Parts need not join at the same station: the driver simply walks to where the next one
  starts.
- The same train part can be worked by several duties as long as they run on different sessions, so one
  duty can cover the odd sessions and another the even ones.
- Where two parts of the same train in a duty are worked by different traction units, the tab shows a note
  at the station where the traction unit is exchanged — you do not enter it by hand.
- Duties imported from XPLN now share the train parts defined in the vehicle schedules, so each part shows
  the traction unit that works it.
- The plan is checked so that no train part is driven by two duties on the same session and no duty has
  parts that overlap in time. The check can be switched off under **Settings › Validation**.

## Version 0.2.2

### Fixes

- Two trains that never run on the same operating session are no longer reported as meeting on a
  single-track stretch. A train running sessions 1, 3, 5 and one running 2, 4, 6 are never out at the same
  time.
- Conflict checks on double-track and multi-track stretches are now precise: a stretch is flagged only
  where more trains occupy it at the same time than it has tracks, counting only trains that run a session
  in common.

## Version 0.2.1

### Changes

- Conflict warnings are now shown where you can act on them: train conflicts on the graphical timetable
  and the **Trains** tab, vehicle and schedule conflicts on the **Schedules** tab.
- On the **Schedules** tab a vehicle conflict now highlights just the vehicle it concerns, and a schedule
  conflict just that schedule.
- The check that a vehicle returns to where it started now also covers wagonsets and cargo, not only
  locomotives and trainsets.

## Version 0.2.0

### Changes

- The name of the plan you are currently working on is now shown in the top bar.
- The graphical timetable now shows loco driver demand bars, making it easier to see how many drivers are
  needed through the operating session.
- A new **Topology** view (under the **Stretches** tab) shows a schematic diagram of your timetable
  stretches and their branches.

### Fixes

- Track stretches now keep the order you entered them in by default. You can still sort by any column.
- Conflicts no longer refer to trains you cannot find: when a train is deleted, its station calls are
  removed with it, so no orphaned calls or false conflicts remain.

## Version 0.1.0

First preview of the Timetable Planner. You can:

- Define track layouts with stations, tracks and stretches.
- Create and edit train schedules with automatic time calculations.
- Assign locomotives and trainsets to trains.
- Build vehicle working schedules (turnus) and print turnus cards.
- Plan cargo flows between stations.
- Display graphical timetables (time–distance diagrams).
- Validate schedules for conflicts and inconsistencies.
- Generate printed output: train cards, station books and driver duty sheets.
- Work in English, German, Danish, Norwegian and Swedish.
