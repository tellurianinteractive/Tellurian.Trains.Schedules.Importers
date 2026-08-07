# Release notes

## Version 0.4.2

### Changes

- **A train can now be worked into the middle of a schedule.** Until now a schedule could only be
  built forwards: the one place to add a train was the end of the working. Between the train parts of
  a row there are now small joints saying where the vehicle stands and for how long, and one before
  the first part saying where it has to be brought from. Click one to add a train into that gap —
  only the trains the vehicle could actually make in the time available are offered. A leg that does
  not bring the vehicle back to where the working goes on is added all the same and reported as a
  conflict until you add the leg back; that is how an out-and-back trip is fitted into a layover, a
  leg at a time. A joint the working is broken across, as an import can leave it, is marked in amber,
  and clicking it offers the trains that bridge it.

- **The app has an icon of its own.** Until now it carried the mark that comes with the tools it is
  built with, which said nothing about what the app is for. It now shows the front of a modern train
  on a dark blue tile. The icon appears in the browser tab, and on the home screen or in the Start
  menu for anyone who installs the app, so it can be told apart from everything else open at the time.

- **Twelve turnus cards now go on a sheet instead of ten.** The cards were 50 mm wide, which let only
  five stand side by side across an A4 sheet held landscape, with a hand's breadth of paper wasted at
  the right-hand edge. They are now 48 mm wide, so six fit across and twelve to the sheet, and the
  sheet has a margin on every side that ordinary printers can reach. The cards are the same height as
  before, and what they hold is unchanged — they are simply a little narrower, so a sixth of the
  cutting and sorting falls away for every schedule printed.

- **The timetable rows now stand further apart.** The lines sat so close together that the eye lost its
  place running along a row of times, which is the one thing the sheet is read for. There is now a
  seventh more space around every line, so a row is easier to follow across the page and a station is
  easier to pick out of the column. The type is the same size as before and the columns are as wide as
  before, so the sheet holds the same trains — it is only the space between the lines that has grown.
  A page now takes thirty-nine lines instead of forty-five, which is as much air as could be given
  without the commonest stretch needing a second sheet.

### Fixes

- **The timetable report no longer loses the last rows of a page.** Both directions of a timetable
  stretch were put on one page where they did not both fit, and the second one stopped part-way down its
  list of stations — the rows with nowhere left to go were cut off rather than carried to the next page.

  How much fits on a page was reckoned for the sheet that comes out of the printer, but the report on
  screen was set in a larger type than the printed one, so its rows stood nearly two-thirds taller than
  the ones being counted. The report on screen and the printed sheet are now set identically, so the
  page on screen is a true picture of the paper: what fills it on screen fills it on paper, and what
  will not fit is carried to the next page instead of being cut off. How much fits is now measured on a
  real page rather than reckoned from the type size, and three lines are kept clear at the foot of
  every page, so a stretch that runs a row or two long gets a page of its own rather than going over
  the edge.

- **The cargo flow list now names the destinations wagons are going to.** On **Cargo flow › Cargo
  trains**, the list to choose from read only "Wagons to" with the destinations left off, so one entry
  could not be told from another. The destinations are back, and the sub-tab and its column are now
  called **Cargo destinations** rather than *Cargo descriptions*, since destinations are what they hold.

## Version 0.4.1

### Changes

- **The station dispatch lists can now be saved as documents the station owners can edit.** Choose
  *Station dispatch lists* on the Export menu and each station on duty gets its own document in
  OpenDocument format, which LibreOffice Writer and most other word processors open. It is meant for
  sending every station's owner their own list before the meeting, so they can add the local
  instructions only they know — which is why it is one document per station rather than one document
  holding everybody's sheets. Where more than one station is on duty the documents arrive together in a
  zip, one file per station inside it.

  Nothing in the document decides where its pages break. The station's name and the phone numbers of
  the stations it clears trains to and from repeat at the top of every page, and so do the column
  headings, but where the pages end is left to the word processor. So an owner who adds three trains
  and a long note gets pages that still break sensibly and still head themselves, rather than text
  running over page breaks that were right only until they started typing. The type sizes and the
  emphasis are named styles, so the appearance of the whole document can be changed at once instead of
  row by row.

  The one thing such a document cannot carry is the part of the day each page covers, which the printed
  sheet states in its heading: that depends on which rows land on which page, which is not known until
  the text has been laid out — and would be wrong again after the first edit. Each page is numbered
  instead, and its first and last row still say what it covers.

  The printed sheets are unchanged, and are still the ones to work from during a session: print them
  from the Reports menu as before.

- **A train hauled by two locomotives at once now says which two.** The conflict named the train and
  the minutes and left the locomotives out, and where both were booked over the very same stretch its
  two halves read word for word the same — so it told you a train was doubled without telling you what
  to unbook. It now names the locomotive on each side.

  It is also marked only on the two schedules holding the doubled work. It used to be marked on every
  schedule working that train anywhere in the day, so a locomotive taking the train on quite another
  leg, with nothing wrong with its own turn, was flagged for a conflict it has no part in.

- **Two locomotives sharing a train between sessions are no longer reported as a conflict.** Only the
  clock times were compared, so one locomotive taking the train on the odd sessions and another on the
  even — never at the meeting on the same day, and the whole point of arranging it that way — was
  reported as the train being hauled by two locomotives at once. It is now reported only where the two
  are booked for a session in common, and the conflict then names the sessions where it is only some of
  them. Two locomotives on one schedule are double-heading and were never the conflict either.

## Version 0.4.0

### Breaking changes

- **A vehicle you create is now identified by its operator and number.** The two together name one
  physical vehicle, so on any one session the combination may belong to only one vehicle — whichever
  kind of vehicle it is. A wagonset and a locomotive can no longer both be *DB 5*. A vehicle with no
  operator is identified by its number alone. Two vehicles may still carry the same operator and number
  as long as the sessions they work do not overlap, since they are then never at the meeting at the
  same time.

  An **imported** vehicle keeps being identified by the external id it was imported under, which is
  already unique in the plan it came from, so an imported plan raises no new conflicts over this.

  Adding or editing a vehicle on the Schedules tab now refuses an identity another vehicle already
  holds, and a number has to be given. Plans made before this rule are kept exactly as they are —
  nothing is renumbered for you — and every vehicle that shares an identity is listed among the
  conflicts, once each, so you can see what needs a new number.

### Changes

- **There is a new report: the station dispatch list.** One set of sheets per station with somebody on
  duty — every manned station, and every shadow station whether manned or not — listing the trains that
  station handles in time order. A train that stands there appears twice, once for the arrival and once
  for the departure, because clearing a train in and clearing it on to the next station are separate
  actions taken minutes apart; arrivals are on white and departures on a light yellow so the two can
  never be mistaken for one another. Trains that only run past are listed too, since they have to be
  cleared through as well. Each page carries the station's name, the part of the day it covers and the
  phone numbers of the stations at the other end of its dispatch stretches, and every row has a box per
  session to tick off as it is worked, greyed where the train does not run. Each station starts on a
  fresh page, so the pile can simply be torn apart and handed out. Print it from the Reports menu.

- **The fields for adding and editing a vehicle are in a new order,** the same in both places: type of
  vehicle, type of traction, number of units, operator, number, class, sessions and last the external
  id — what the vehicle is, then what identifies it, then how it is described and when it runs. The
  field previously labelled *Company* is now *Operator*.

- **An external id can be corrected but no longer invented.** The external id is the name a train or a
  vehicle carries in the system it was imported from, so it means something only where it came from
  something. One imported with an id still has its field — on the Trains tab, and in the vehicle dialog
  on the Schedules tab — and can be corrected there; one that never had an id now has no field to type
  into. A vehicle you create in the planner is therefore given no external id at all, where it used to
  be given one made up from its class and number.

- **The shortest time between two uses of the same track is now checked.** The setting was there but
  nothing acted on it. Left at 0 — where it starts, and where it stays until you change it — nothing
  about the checking changes: two trains are in conflict where they stand on the same track at the same
  time, and one arriving just as another leaves is a handover, not a conflict. Set it to, say, 5 and the
  track must also be free for five minutes between them, so a plan that turns a track round faster than
  the station can work it is reported. Exactly five minutes free is enough; four is not.

  A conflict of that kind says how short the gap actually is and how long it had to be, rather than
  claiming the two trains overlap when the times show they do not.

- **An operation location can now carry its own instructions.** The form for adding and editing a
  location has an **Instructions** field, written in Markdown and shown beside a live preview like the
  general instructions in Settings. It is for how that location is worked at this meeting — which
  tracks are used for what, how the shunting is arranged, and what else the loco drivers and the
  people staffing it need to know there. How the location is operated in general, and any other
  description of it, is for its owner to provide and does not belong in the field. What is written is
  saved with the location and shown on its Info view.

  The field is offered at a station or an industrial area, where passengers and/or cargo are exchanged.
  It is not offered where there is nothing to instruct: trains only run past a signal-controlled
  location, and nobody works an other location, so a train there does what its call says and no more.

- **A location where cargo is worked with nobody on duty can now require a key.** Where the switches at
  an unmanned station or an industrial area are padlocked, the edit form lets you pick the manned
  station that keeps the key, under **Lock key held at**, and name the key if that station keeps more
  than one.

  Nothing else has to be planned. A freight train that stops at the key-holding station and later stops
  at the location the key unlocks is told, as it leaves the key-holding station, to *pick up key A1 for
  unlocking Bruket*; when it next calls there, its arrival tells it to *leave key A1 from Bruket*. A
  train that only runs past either place is told nothing, since it unlocks nothing. The key is fetched
  at the last call at the holding station before the work and handed back at the first one after it, so
  a train calling there twice is not asked to carry it around for an extra visit.

  A key only means something while both ends of it hold. Mark the location itself as manned, or take the
  manning off the station that keeps the key, and the key stops applying: no notes are made from it, and
  **Conflicts** says which of the two changes did it. The key is kept rather than thrown away, so
  undoing that change brings it straight back, and it stays on the form where you can point it at
  another station or clear it.

### Fixes

- **Two stretches setting off from the same operation location were drawn as if they never met.** Where
  a timetable stretch began at the very first operation location of another, nothing joined the two in
  the Topology diagram: each was drawn as a line of its own, with no branch between them. The second now
  leaves that operation location like any other branch, falling away from it at the same fixed angle.

- **Every validation threshold now says which clock it is measured against.** The shortest time between
  two uses of the same track gave no unit at all, and the two train speeds said only *clock minutes*,
  which could be read either way. All three now say fast-clock minutes — the clock the trains run to,
  not real time.

- **Lengths and distances now spell metres out,** as does the top half of the train speeds, so the *m*
  cannot be taken for a minute. The minimum stop at a station is now labelled in fast-clock minutes too.

## Version 0.3.5

### Fixes

- **A saved plan could refuse to open.** Opening a plan the app had just saved stopped with an error
  naming a country, and nothing was loaded — there was no way past it. A file is read a piece at a time
  as it arrives, and reading the countries in it tripped over that. A plan already saved opens as it
  stands; there is nothing you need to do to it.

- **A saved plan file is about seven times smaller.** Saving a plan to a file wrote it in a different
  form from the one kept in the browser, so the savings the last two versions made never reached the
  file: every stop was written twice, and every train category, operator and country again at each
  train, vehicle and duty that used it. A file that took 8 MB now takes a little over 1 MB, and saves
  and opens correspondingly faster. A plan saved by an earlier version still opens.

## Version 0.3.4

- **The Arr and Dep boxes on a call now follow where the train can actually stop.** A train stops
  somewhere to exchange something, so it needs somewhere to exchange it: a passenger train where the
  place takes passengers, a freight train where it takes cargo, and neither at a signal-controlled
  location. Where a train cannot stop, both boxes are shown cleared and cannot be ticked, and the
  call is a pass-through in the timetable and on the graph. Nothing you planned is thrown away — turn
  the exchange back on and the stops are there again. A shadow yard always exchanges both, since it
  stands for everything beyond the layout, so its two exchange boxes are ticked and disabled.

- **A stop something depends on can no longer be taken away.** A train part runs from a call the
  train departs to a call it arrives at, so both ends have to be stops. The train's own first and
  last call, and the ends of every part a vehicle schedule, a driver duty or a cargo flow is planned
  over, now keep their box ticked and disabled; hovering says what is holding it. Where a part ends
  somewhere its train cannot stop — a plan made before this rule — the box says so plainly, so you
  can move the call or the part.

- **A train category now carries the preparation and finishing-up times its trains are planned with.**
  Every new train of the category is made ready that many minutes before it departs and put away that
  many minutes after it arrives, so you no longer type the same two numbers for every train. Beside
  each of the two fields is a *Reapply* button that gives that one time to all the trains the category
  already has, and says how many were changed. The two are separate actions, so you can change the
  preparation time without disturbing the finishing-up time. Reapplying moves only the minutes at the
  very ends of a train: it still departs, calls and arrives at exactly the times it did.

- **The operators are easier to read on the front page of a duty booklet.** The line is now set at
  twice the size it was, so a logo is large enough to be recognised at a glance and a signature large
  enough to be read across a table. Where every operator of the duty has a logo, the word *Operator*
  is left out — the logos say it themselves. Where any one of them has no logo, all are still given as
  signatures, in bold and with the label kept.

### Fixes

- **A duty booklet could print a train part off the foot of the page.** The report works out how many
  train parts fit on a page before it prints them, and it was crediting each page with about half as
  much room again as an A5 page really has. Anything past the foot of the page is cut away without a
  word, so the second train part on such a page lost the end of its timetable — or did not appear at
  all, leaving a loco driver holding a duty whose last train was missing. Train parts are now measured
  against what the page really holds, and one that does not fit is carried to the next page. Some
  booklets will therefore need a sheet more than before.

- **The Topology diagram could print the signatures of two operation locations on top of each other.**
  Operation locations were placed purely by the distance between them, so two that lie close together
  on a long stretch were drawn almost in the same place and their signatures ran into one another.
  They are now never drawn closer together than their two signatures need, while the rest of the
  stretch keeps its true proportions. A long signature at the edge of the diagram is no longer cut
  off either.

- **A branch in the Topology diagram could be drawn straight through another stretch.** A branch falls
  away from the stretch it leaves at a fixed angle, so a branch that met a stretch in its way could
  never get past it, however far down the diagram it was pushed — it was simply drawn across it. The
  branches that leave furthest along a stretch are now drawn first, which leaves the ones behind them
  a clear way down. A long branch may therefore now be drawn below a short one that leaves the stretch
  further along.

- **A plan could show its trains under train categories the Train categories tab did not list.** A
  train carries its category with it, so a plan saved by an earlier version opened with its trains
  grouped by category while the list of categories was empty: the category drop-down had nothing to
  offer, and no train could be moved to another category. Several categories could also be taken for
  one and the same, gathering their trains under a single heading and reporting two trains of
  different categories that share a number as one number used twice. When a plan is opened, the list
  of categories is now completed from the categories its trains use, and every category is kept apart
  from the others.

- **Two companies that had never been given a number of their own were taken for the same operator.**
  A company is told apart from the others by a number the app keeps for it, and a plan could hold
  several that had never been given one. Trains of different companies that shared a train number were
  then reported as one number used twice. Every company is now given a number of its own when a plan is
  opened or saved; a company that came from the Module Registry keeps the number it came with.

- **A plan stored its train categories, companies and countries in more than one place.** Each was
  written wherever it was first met on the way out — usually inside the first train that used it —
  while the list it belongs to held no more than a pointer to it. That is how a plan could come to have
  trains in categories the Train categories tab knew nothing about. Each is now written once, in its
  own list, and everything that uses one keeps only a reference. Countries are no longer copied into
  the plan at all, so a correction to a country's languages now reaches plans that were saved before
  it. A plan saved by an earlier version is read as before and put right the next time it is saved.

- **A duty booklet gave only the train number in the heading of a train part.** A train is identified
  by the prefix and suffix of its category as much as by its number — Gt 1234, not 1234 — and a loco
  driver comparing the booklet with the timetable, or with what is called out, has only that heading
  to go by. The heading now carries the whole train identity, prefix and suffix included, after the
  operator's signature.

## Version 0.3.3

- **Conflicts can now be read where they are shown.** A row that has conflicts — a train or a train
  category under **Trains**, a working or one of its vehicles under **Schedules**, a duty under
  **Duties** — now carries a warning symbol, and clicking it opens the messages in a list you can
  read. The symbol takes the colour of the most serious conflict and counts them when there is more
  than one. The messages were previously only in a tooltip that appeared while the pointer rested on
  the row, easy to miss and hard to read.
- **A train category shows the conflicts of the trains inside it**, so closing the category no longer
  hides them.
- **The Trains tab now opens on the list of train categories**, with the trains in each one hidden
  until you open it, so a plan with many trains is easier to find your way around. *Expand all* opens
  them all at once, and a category opens by itself when you add a train to it or move a train into it.
- **Editing a train part in a working now says which kinds of vehicle the working is for** —
  locomotive, trainset or wagonset. Where several vehicles share a working, each kind is named once,
  and pointing at it names the vehicles themselves.

### Fixes

- **The app could stop saving your work without telling you.** The plan is saved to the browser as you
  work, and a plan the app could not write out — a train left with fewer than two calls, or a route in
  **Stretches › Timetable stretches** whose track stretches had all been removed — failed that save
  silently. Everything done from that moment on stayed on screen but was never kept, so reopening the
  browser showed the plan as it was before, with the operation locations but without the stretches and
  trains added since. Both plans now save, and if a save ever fails again the top bar says so
  straight away, so you can undo the change that caused it instead of losing the work.

- **A saved plan file is about 40 % smaller.** Each stop was written twice — once in its train and once
  under the track it stands on — and the second copy dragged much of the rest of the plan along with it.
  A plan saved by an earlier version still opens.

- **A train left part of its run without a traction unit is now reported.** The check asked only
  whether a locomotive or trainset worked the train *somewhere*, so shortening a working at one end
  left the rest of the train unworked without a word. Every stretch the train runs is now checked, on
  every session it runs it, and the conflict says between which locations, and on which sessions, the
  train has no traction unit. Plans that looked clean may report this now — the gap was always there.

## Version 0.3.2

- Under **Cargo flow › Cargo descriptions**, an origin or a destination can now be any operation
  location that exchanges cargo, not only a station. An industrial area always handles cargo
  wagons but could not be chosen before. The same lists now say **location** where they said *station*, since stations are no longer the
  only thing they hold.
- The calls of a train are always listed in the **order the train travels** them.
- Editing a call time in the **Trains** tab now **takes the rest of the train with it**. A **departure**
  works forwards, the way the train runs: hold a train five minutes longer at one station and it reaches
  every later station five minutes later. An **arrival** works backwards: ask a train to arrive five
  minutes later and it leaves every earlier station five minutes later, so the run up to the change
  follows it. Either way the times on the other side stay where they are, the run and dwell times are
  kept, and the change is refused — with the field falling back — if it would take the train outside the
  plan's operating times.
- A train whose route **jumps a location** — two calls in a row with no track stretch between them — is
  now reported as a conflict. It can be switched off under **Settings › Validation**.
- A train part in a **schedule** can now be **edited**: the pen on a part opens its from- and to-stop,
  so a working can be reshaped without removing everything after it. A neighbouring part that joins
  the one you change follows along — shorten a part from A–C to A–B and the return working becomes
  B–A by itself. A neighbour whose own train does not call at the new stop is left as it is, and the
  gap it leaves is reported as a conflict for you to resolve.

- **Add train** can now create the **return train** at the same time. Tick *Return?* and the train back
  from the destination is created together with the outbound one, running the same route in reverse with
  the same category and speed, and taking the next number of the opposite direction. Its departure is
  either *as soon as possible* — the outbound train's arrival plus the finishing and preparation times —
  or a time you enter, which may be earlier or later than the outbound train's own departure. Combined
  with *Repeat?*, both directions are repeated, so a whole two-way service is planned in one go.

### Fixes

- The **kilometre figures** in the printed timetable and along the graphical timetable are now rounded
  to whole kilometres. They were printed with a decimal, and the distance factor under **Settings ›
  Time & speed** could turn a stretch length into an odd fraction of a kilometre. A branch line now
  also shows the same kilometre as the line it leaves at their junction station.
- Everything that reads a train's route now follows the **order the train runs its stops**, not the
  order they were entered. On a train whose stops went in out of order — one added after a stop it
  only reaches later — the **Graphical timetable** drew zig-zag lines between stops the train never
  runs between and could place the train in the wrong direction's column; the printed **timetable
  table** could show a departure where the train arrives; **Build automatically** did not chain the
  train at all, since it appeared to start where it does not; **repeating a train** measured the
  interval from the wrong stop; and recomputing its times after a stop change failed outright.
  Picking part of a train when adding it to a schedule also lists its stops in running order.
  Imported plans were never affected — there the two orders are the same.
- **Train speed is now checked on the last leg too**, into the station where the train ends its run.
  That leg was skipped before.

## Version 0.3.1

- The **Traction units** section on a train part page in the Driver duties booklet now has its
  heading in the chosen language. It was previously the only heading in the booklet left
  untranslated, so the section did not read as the traction units at all.
- The traction unit is now printed for every train part that has one. In plans imported with an
  earlier version, some parts showed a traction unit under **Duties** but none in the booklet.
- Notes about trains going the same way now say who passes whom — **Overtakes GD 42757 12:02-12:05**
  or **Is overtaken by GD 42757 12:02** — instead of the old *"Meets GD 42757 in the same
  direction"*, which never said which train got ahead. Two trains that merely stand at the same
  station at the same time give no note at all, since neither has passed the other.
- A meet that lasts no time — the other train runs through without stopping — is printed as a single
  time instead of an interval from a time to itself.
- A train that begins or ends its run at a station is no longer reported as met, crossed or
  overtaken there. Those times are when its loco driver reports for duty or stands down, not when
  the train is running.

## Version 0.3.0

- A new **Driver duties report** that prints one A5 booklet per duty. The front page shows
  the duty number, which sessions or days it runs,
  its start and end time and station, a difficulty grade, staffing needs and any duty
  notes. Each train part gets its own page, with which traction units to use,
  which wagonsets to bring, and to which destinations to bring cargo
  wagons, and the timetable — each shown in its own clearly separated
  block. The last page of every booklet shows the layout's track plan and a table of
  shunting yards, for easy reference while running.
- A new **General instructions** report is a separate printed booklet with the meeting
  programme and instructions that apply to a layout for the whole meeting. Here, the
  meeting organiser is free to write anything — for example driving instructions,
  signalling practice, radio/phone use, running late, who to ask — handed out once to
  everyone.
- The programme and the instructions are both written under **Settings › Information**,
  and can be formatted with Markdown — headings, lists, bold and italics — so even a
  long instruction text stays readable in print.
- The booklet opens with the meeting name, the dates it is valid between and the print
  date, followed by the programme: session times, breaks and meals — what every
  participant needs to know before the first session.
- The instructions follow over as many pages as they need. A page is broken between
  paragraphs, and a heading always stays with the text it introduces.
- The last page shows the layout's track plan and the table of shunting yards, so those
  who never hold a duty booklet — station staff above all — still get an overview of the
  layout.
- The booklet prints in the same A5 format as the duty booklets: A4 landscape,
  double-sided, folded down the middle, with blank pages added where needed so the
  sheets fold correctly.
- Duties can now be graded **Easy**, **Medium** or **Experienced**, shown
  colour-coded on the booklet, so a participant can choose a duty that matches their
  experience.
- A duty can now specify that it needs two or three people — for example a loco
  driver and a conductor — and this is shown on the booklet.
- A duty can be pinned to a **fixed number** so automatic renumbering leaves it
  untouched, for example special duties handed out in advance of a session starting.
- The plan is now also checked so that every train part with a locomotive or
  trainset assigned has a driver duty covering it on each session it runs — a part
  nobody is rostered to drive is reported, session by session. A pinned duty is
  checked too: it must have a number, and no two pinned duties can be given the
  same number.
- Companies can now have an uploaded **logo**, shown in reports in place of the text
  signature.
- Stations can now be marked as the **shunting yard** that handles another location's local
  freight; the layout automatically lists every shunting yard and what it covers, shown on the
  last page of the duty booklet. This helps station staff and freight train drivers
  know where to send wagons with a given freight destination.
- Each timetable stretch can now be given a **colour**, used to draw it in the
  Topology diagram.
- A new **distance display factor** (under Settings › Time and Speed) lets a layout
  show a different — typically larger, more prototype-like — kilometre figure in
  reports and the graphical timetable than the distance actually modelled, without
  affecting any travel-time calculation.
- The app now keeps multiple open browser tabs or windows in sync with each other.
  **Note** that this only works across windows on the same machine in the same browser.
- Settings can now record the meeting's **valid from** and **valid to** dates, printed
  as a validity line on reports; leave them empty when no meeting is booked yet.
- A new **extend plan times automatically** option (under Settings › General) widens
  the plan's start or end time to cover a train instead of blocking the change when
  the train's own time falls outside it. Off by default.
- A new **update all timings** button on the graphical timetable recomputes every
  train in the timetable in one go, instead of selecting a subset first.
- Track occupancy checks can now optionally account for a locomotive or trainset
  standing on a track between two trains, unless it is booked to or from parking
  (under Settings › Validation). Off by default, since it only makes sense on
  layouts where parking is modelled deliberately — turn it on there to catch a
  third train quietly using a track a standing vehicle already occupies.
- Every call in the **Trains** tab now has a **Remark** field for a note printed at that
  call — for example "wait for the oncoming train". The note reads as finished text and
  shows the markup you typed as soon as you enter the field, so you can emphasise the part
  that matters: write `*slowly*` for italics and `**first**` for bold. Emptying the field
  removes the note again.

### Fixes

- Adding a new train now sets its default start time to account for the given preparation
  time, so it does not start before the plan's start time.

## Version 0.2.4

- A new **Duties** tab lets you plan driver duties — the work one loco driver performs
  across a session, as a sequence of the train parts they drive. Each duty is a row: its
  identity, company and operating sessions on the left, the train parts in running order
  on the right.
- Add the parts a driver works with **+ train part**. The picker offers the traction
  parts a driver could take next — those that do not clash in time with the duty and, once
  it has a part, those departing at or after it arrives. Parts need not join at the same
  station: between two parts the driver simply walks to where the next one starts.
- The same train part can be worked by several duties as long as they run on different
  sessions, so one duty can cover the odd sessions and another the even ones.
- Where two parts of the same train in a duty are worked by different traction units, the
  tab now shows a note at the station where the traction unit is exchanged — you do not
  enter it by hand.
- You can give each duty an identity and operating company, choose the sessions it runs,
  and add free-text notes that apply to the whole duty.
- Duties imported from XPLN now share the train parts defined in the vehicle schedules, so
  each part shows the traction unit that works it.
- The plan is checked so that no train part is driven by two duties on the same session
  and no duty has parts that overlap in time; any conflicts are listed and open on the
  **Duties** tab. You can turn this check on or off under **Settings › Validation**.

## Version 0.2.2

### Fixes

- Two trains that never run on the same operating session are no longer reported as
  meeting on a single-track stretch. A train running sessions 1, 3, 5 and one running
  2, 4, 6 can now share the same track without a false warning, because they are never
  out at the same time.
- Conflict checks on double-track (and multi-track) stretches are now precise: a
  stretch is flagged only when more trains occupy it at the same time than it has
  tracks, and only counting trains that run a session in common.

## Version 0.2.1

- Conflict warnings are now shown where you can act on them. Train conflicts appear
  only on the graphical timetable and the **Trains** tab; vehicle and schedule
  conflicts appear only on the **Schedules** tab.
- On the **Schedules** tab a vehicle conflict now highlights just the vehicle it
  concerns, and a schedule conflict highlights just that schedule, so it is clear
  which one needs attention.
- The check that a vehicle returns to where it started now also covers wagonsets and
  cargo, not only locomotives and trainsets, so a wagonset or cargo left out of place
  at the end of the operating period is now reported.

## Version 0.2.0

- The name of the plan you are currently working on is now shown in the top bar,
  so you can always see which document is open.
- The graphical timetable now shows loco driver demand bars, making it easier to
  see how many drivers are needed through the operating session.
- A new **Topology** view (under the **Stretches** tab) shows a schematic diagram
  of your timetable stretches and their branches.

### Fixes

- Track stretches now keep the order you entered them in by default, so the list
  is easier to follow while you check your input. You can still sort by any column.
- Conflicts no longer refer to trains you cannot find: when a train is deleted, its
  station calls are removed with it, so no orphaned calls or false conflicts remain.

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
