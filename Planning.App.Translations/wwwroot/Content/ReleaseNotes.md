# Release Notes

## Version 0.3.0

- A new **Driver Duties report** that prints one A5 booklet per duty. The front page shows
  the duty number, which sessions or days it runs,
  its start and end time and station, a difficulty grade, staffing needs and any duty
  notes. Each train part gets its own page, with which traction units to use,
  which wagonsets to bring, and to which destinations to bring cargo
  wagons, and the timetable — each shown in its own clearly separated
  block. The last page of every booklet shows the layout's track plan and a table of
  shunting yards, for easy reference while running.
- A new **General Instructions** report is a separate printed booklet with the meeting
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
