# Release notes

## Version 0.5.2

### Changes

- **The graphical timetables can now be printed.** A new report under **Reports** draws every timetable
  stretch to a fixed paper scale — so many millimetres to the fast-clock hour, so many to the kilometre —
  and puts as many stretches on a sheet as the paper holds. Which way round the paper goes follows the
  orientation you have chosen for the graphical timetable: a horizontal time axis prints on A4 landscape
  with the stretches stacked one under another, a vertical one on A4 portrait with them side by side.

  Because the scale is fixed rather than squeezed to fit the paper, times and gradients can be compared
  and measured from one sheet to the next. A time window too long for one sheet is divided along the time
  axis — at the break time first, then into equal sheets that overlap — so a train crossing the cut can be
  followed on both sheets, and the last sheet is as full as the others rather than carrying a few stray
  minutes. The scale is set under **Settings → Graphical timetable**; lowering the station spacing there
  is what makes two or three stretches share a sheet. Trains print in their category colours, as on
  screen, unless you ask for black and white — worth doing on a monochrome printer, which turns colours
  chosen to be distinct on screen into much the same grey.

- **Settings → Graphical timetable is now arranged by what each setting affects.** What the graph shows —
  which way the time axis runs, which minutes are printed, what a train label carries — comes first, since
  it applies on screen and on paper alike. Under it stand two blocks side by side: the spacing used on
  screen, in pixels, and the spacing used by the printed report, in millimetres of paper. Each block
  carries the same kinds of spacing, so the screen setting and its paper counterpart can be read against
  each other, and neither can be mistaken for the other. Number fields are right-justified, so their
  digits line up.

- **You can now say what is to be done with the locomotive where a train part ends.** Editing a train
  part under **Schedules** asks two more questions: is the locomotive to be turned, and is it to be run
  round to the other end of the train so the train can leave the way it came? Either one is printed as an
  arrival note for both the loco driver and the dispatcher, and asking for both gives a single note — the
  locomotive leaves the train, goes to the turntable and comes back on the other end — rather than two
  that read as separate movements.

  Turning is only offered where the station the part ends at has a turntable, which is a new setting
  under **Operation locations**; nowhere else has one. Running round is left out of the note where the
  traction working the part reverses as it stands — a trainset, or a locomotive on a reversible train —
  since there is then nothing to run round. What you asked for is kept either way, so it says what it
  should again as soon as another locomotive works the part.

- **The Topology diagram now draws the whole layout's track, with every operation location shown once.**
  It was a row of horizontal lines, one for each timetable stretch, and a location several stretches
  reached was drawn on each of them. Every location now appears exactly once, and the track between two
  of them is a straight line at whatever angle they lie, single or double as the stretch really is and in
  the colours of the timetable stretches running over it. Track no timetable stretch covers at all is
  drawn in grey, so a gap in your stretches can be seen instead of simply being absent. A signature that
  would have track running through it moves to whichever side of its circle is clearest — over, under or
  beside it — which is the answer where track runs both up and down from the same location.

- **You can now arrange the Topology diagram yourself.** Drag an operation location to where it belongs
  and the track follows it, settling on the same rows and spacing the automatic drawing uses so that what
  you move stays in line with what you leave. Where you have put things is saved with the plan and is
  what the overview page of the driver duty booklets prints. **Arrange automatically** forgets every
  location you have moved and draws the whole diagram again. This is what a layout with a triangle, a
  balloon loop or two lines joined at both ends needs: no rule that reads the track alone can be relied
  on to draw such a layout as it really is, and you know what it looks like.

- **The controls that act on a whole working now stand in a column of their own.** Under **Schedules**,
  copying, complementing and deleting a working stood at the head of its trains, so each row's train
  blocks began in a different place, and the question asked before deleting pushed them further along
  still. They are now in an **Actions** column between the vehicles and the trains: every row's trains
  begin at the same place, including where they wrap onto the next line, and the delete control stays
  where it is and is marked while the question stands beside it.

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
