# Driver Duties Report — Specification

**Status:** Draft 1 — purpose and framing. Sections 5–8 are deliberately thin until we have
reviewed the prototype.
**Last updated:** 2026-07-27

---

## 1. Purpose

The Driver Duties Report produces the printed booklet that a loco driver is handed at the start of
an operating session. It is the driver's working document *during* the session: it tells them what
to do, where to be, at what time, with which vehicles, and what to expect at each station.

One companion document exists — the general instructions (§5.5), read once before the first session
and shared with everyone at the meeting, station staff included. Everything a driver needs *while
running* is in the booklet; nothing in the booklet is meeting-wide standing text.

### 1.1 Who reads it

A **meeting participant** — someone who has been to module meetings before and has driven trains
before, with experience ranging from a few meetings to many. Not a beginner being taught to operate,
and not an expert on *this* plan either.

Usually one person, the **loco driver**. On duties with enough wagon-card work, a **conductor**
works alongside them (G9), and the booklet then serves both: the driver runs the train, the
conductor works the cargo block (§5.3.5).

Three consequences:

- **Assume the craft, explain the plan.** The booklet need not teach how to drive, shunt or read a
  timetable. It must say everything specific to this layout, this plan and this duty, because none
  of that is carried over from the last meeting.
- **Difficulty is a real choice, not decoration.** Because experience varies, the front page's
  difficulty grade (element 11) is what lets a participant pick a duty matching what they are
  comfortable with.
- **Asking is normal and encouraged.** Participants are always told to ask someone when anything is
  unclear, and most duties are started without needing to. So the target is that the booklet answers
  the ordinary questions — not that a driver is ever left to work something out alone. A question
  asked is not a defect in the report.

  Two things follow. The booklet is designed for the common case rather than padded with
  explanation for the rare one; a rare situation is better handled by asking than by text every
  driver has to read past. And **who to ask** must be findable — which is one of the things the
  general instructions carry (§5.5).

### 1.2 What makes it different from every other view

Everything else in the application serves planning. This report serves *operating*. That difference
drives every design decision below:

- **It is read while standing at a layout, under time pressure.** Legibility and a predictable
  position for each piece of information matter more than density. Type size has a floor: text too
  small to read at arm's length has failed regardless of how much it let us fit on the page.
- **It is read in sequence.** The driver works down the booklet from the first duty part to the
  last. Information must appear in the order the driver needs it, not in the order the model
  stores it.
- **It is paper.** Once printed it cannot be scrolled, searched or re-flowed. A page that overflows
  is a bug, not a cosmetic issue. Pagination must be deterministic and safe.
- **It covers several sessions, usually all of them.** One booklet is one duty across every session
  that duty runs (D1) — it is not reprinted per session. So the driver *does* have to work out what
  applies today, and the report's job is to make that decision instant and unambiguous rather than
  to pretend it does not arise. This is why the session indicator (§5.1.1) uses one visual language
  everywhere it appears, and why the wagonset and cargo blocks carry a Sessions column instead of
  being filtered: on session 3 the driver reads the rows marked 3 and ignores the rest.

### 1.3 It must get the driver to the right locomotive

Beyond telling the driver what to run, the booklet is what gets them physically started. At the
start station a participant has to:

1. **Find the traction unit** among the ones standing there.
2. **Find its loco card**, and through that its turnus card.
3. **Find the throttle** that drives it.

All three are matched by the same thing: the identity printed in the traction block (§5.3.3). It
therefore has to be the identity written on the loco card — the same literal string, not a
paraphrase of it — or the chain breaks at the first step and the driver has to ask someone.

That is a constraint on data, not on layout: whatever the traction block prints must be the vehicle
identity the physical cards use. Concretely it is **`ScheduledObject.Designation`**, which is the
`ExternalId` when there is one and a composed identity otherwise. Neither `ToString()` nor `Number`
will do — see §5.3.3, where the drafted column currently gets this wrong.

The report replaces the equivalent output from the earlier prototype, which has been in real use at
operating sessions. The prototype is therefore the primary source of proven content decisions; this
specification records what to keep, what to change, and why.

### 1.4 What success looks like

1. Most drivers can pick up the booklet cold and start work without needing to ask — including
   locating the traction unit, its cards and its throttle. Asking remains encouraged (§1.1); the
   measure is that it is rarely *necessary*, not that it never happens.
2. On any session the duty runs, the driver can tell in one reading which rows apply today.
3. Every timing, track and vehicle shown is traceable to the plan — no derived value that the
   planner cannot see and correct in the application.
4. Printing an entire plan's duties is one action, producing a stack that can be folded and handed
   out without manual sorting.
5. A duty that changes in the plan reprints identically apart from the change.

### 1.5 Out of scope

- Editing duties. That is the Duties tab; the report is read-only output.
- Duty *construction* rules (overlap, one driver per segment per session). Those are model
  concerns, already implemented.
- Station-side documents (arrival/departure lists, shunting lists), vehicle turnus cards and the
  graphical timetable. Those are separate reports.

In scope, though separate output: **the general instructions report** (§5.5). It is specified here
because it is where the general instructions went when the booklet was split (D50), and because it
reuses this report's pagination and imposition.

---

## 2. Users and usage context

| Aspect | Current assumption | Confidence |
|---|---|---|
| Reader | Loco driver — a meeting participant with some, but varying, experience of module meetings and of driving; often new to *this* layout (§1.1) | High |
| When produced | Once before the meeting, by the plan owner — **not** before each session | High |
| Physical form | A5 portrait booklet, one booklet per duty, two A5 pages per side of an A4L sheet | High |
| Binding | Folded; stapled as well when more than one sheet. Page count per duty is a multiple of 4 | High |
| Quantity | Exactly one copy of each duty for the whole meeting | High |
| Reprinting | Only on error found, or when a booklet is not returned | High |
| Output path | Browser → PDF → proof-read → paper, never straight to paper (§8.0) | High |
| Language | The plan's language, one language per printing | High |

### 2.1 Booklet lifecycle — the pile

This is the decisive usage fact and it shapes the whole report.

The booklets are printed **once for the whole meeting** and then **reused session after session**:

1. Before the meeting, one copy of every duty is printed.
2. Before each session, someone sorts the stack by hand: duties that do not run in the coming
   session are taken out, duties that were excluded from the previous session are put back in. The
   resulting pile holds every duty for that session, **ordered by duty number**.
3. During the session a participant takes a booklet from the pile and works it.
4. On finishing, the participant returns the booklet to the *ready* pile — and usually takes
   another. **A participant often works several duties in one session.**
5. Steps 3–4 repeat until every duty in the pile has been worked. Then step 2 again, for the next
   session.

Consequences for the design:

- **A booklet is never session-specific.** One booklet covers every session its duty runs. Nothing
  inside may be printed for one session only.
- **The sessions a duty runs must be readable from the outside**, while sorting a stack of closed
  booklets. This is a hard requirement on the front page, not a nice-to-have: whoever sorts the
  pile makes an include/exclude decision per booklet without opening it.
- **The duty number must be equally readable from the outside**, since the pile is ordered by it.
  Sorting by hand into numeric order is the second physical operation performed on the closed
  booklet.
- **The front page is also a chooser.** Because a participant works several duties in a session
  (step 4), they return to the pile mid-session and pick the next one — deciding from the closed
  front pages which duty to take now. That decision uses four fields the sorter does not care
  about:

  | Field | The question it answers |
  |---|---|
  | Start time (7) | Does this start soon enough to be worth taking, or is there a wait? |
  | Start station (7) | Is it near where I have just finished, or across the layout? |
  | Difficulty (11) | Do I want another like the last one, or something quieter? |
  | Staffing (15) | Can I take this alone, or must I find someone first? |

  This is a third reader of the front page, alongside the sorter and the driver confirming what
  they hold. It is the reason start time and start station belong together in one line rather than
  being split, and it raises the standing of difficulty from a nice-to-have to something read at
  every changeover.
- **Booklets are handled repeatedly over several days.** They are picked up, worked, returned and
  re-sorted many times — several times per participant per session — which argues for robust
  binding and against loose sheets.
- **A single duty must be printable on its own**, for the two reprint cases: a correction, or a
  booklet that never came back. Bulk printing alone is not sufficient. The *replacement* case is
  better served from the meeting's saved PDF than from a fresh render (D62); the *correction* case
  is what the single-duty render exists for.

---

## 3. Vocabulary

Terms used throughout this document, matching the model.

| Term | Meaning |
|---|---|
| **Duty** (`DriverDuty`) | The work one loco driver performs, identified by `Identity`, running on a set of `Sessions`. |
| **Duty part** (`ScheduledTrainPart`) | One continuous stretch of one train worked by the driver, from one station to another. A duty is an ordered set of these. |
| **Call** (`StationCall`) | A scheduled arrival at and/or departure from an operation location within a part. |
| **Traction unit** | The loco or trainset that hauls a part. Resolved through the plan, not stored on the part. |
| **Traction exchange** | Two consecutive parts of the same train at the same station worked by different traction units — the driver stays, the loco changes. |
| **Session** | One operating occasion. A duty may run on several; the booklet must make clear which. |
| **Booklet** | The printed pages for one duty. |
| **Participant** | Anyone taking part in the meeting. The reader of a duty booklet is a participant who is driving — **not** a "guest". |
| **Loco driver** | The participant who drives the train. Every duty has one. |
| **Conductor** | The second participant on a duty that needs one: works the wagon cards — which wagons to bring, which to set off, and their order in the train. On a simple duty the loco driver does this too. See `StaffCount` (G9). |

**Avoid "guest".** People at a module meeting are participants; they bring modules, stock and work,
and they are not visiting someone else's railway. The word also implies less responsibility than a
duty actually carries. Where the point is unfamiliarity with a particular layout, say that instead —
*new to this layout* — because that is the real condition, and it applies to regulars too.

---

## 4. Report structure

### 4.1 Three kinds of information, two documents

Everything a driver reads that is not a train part falls into one of three kinds, and they do not
belong in the same place:

| Kind | Scope | Where it goes |
|---|---|---|
| **General instructions** | The whole meeting — signalling practice, radio use, shunting rules, running late, who to ask | A **separate report** (§5.5), handed out to every participant |
| **Duty notes** (`DriverDuty.Notes`) | This one duty | **Front page** of the booklet (§5.1, element 6) |
| **Layout overview** — topology and shunting yards | The whole layout, but consulted *during* the run | **Last page** of the booklet (§5.2) |

The general instructions are not duty-specific and are needed by people who never hold a duty
booklet — station staff above all. Printing them into every booklet would repeat identical pages
across the whole pile and still leave the station staff without a copy. As a separate A5 booklet
they are handed out once, before the first session, to everyone.

That leaves the booklet carrying only what is either specific to this duty or needed with the
booklet open in one hand.

### 4.2 Page order

A booklet has four page kinds in a fixed order:

```
┌─ Front page ──────────────┐  Page 1. Identifies the booklet and carries the duty notes; see §5.1.
├─ Train part page(s) ──────┤  One or more pages, each holding one or more train parts,
│                           │  in working order. Per part:
│                           │    · traction units
│                           │    · wagonset / train composition
│                           │    · cargo wagons with waybills
│                           │    · the timetable of calls
├─ Blank page(s) ───────────┤  Padding, so that…
└─ Layout overview page ────┘  …the topology and shunting yards page is always the LAST page.
```

Two structural rules follow:

- **The layout overview page is always last.** A driver finds it by turning to the back, without
  knowing the page count — and turns there repeatedly during a run, which is why a fixed position
  matters more here than for a page read once at the start. This is why blank padding sits between
  the part pages and the overview page rather than at the very end.
- **Blank pages are padding, not filler.** They exist to push the overview page onto the last
  physical sheet, which for a folded booklet also means bringing the total to a multiple of 4.

The scaffolding in `Planning.Components/Reporting/Duties/` already models this as a
`DriverDutyPage` with `Blank` / `Front` / `Part` / `Instructions` variants. The structure is right,
but `Instructions` is now a misnomer twice over: it is *the last page*, not a page following the
front, and it no longer carries instructions. Rename to `Overview`.

---

## 5. Page content design

*To be filled in after reviewing the prototype. For each page type we need: the fields shown,
their order, their grouping, what is emphasised, and what is omitted deliberately.*

### 5.1 Front page

The front page serves **three** readers, in the order they meet it:

| Reader | Handles | Needs |
|---|---|---|
| **The sorter** (§2.1) | A closed booklet in a stack | Duty number and sessions, at a glance |
| **The chooser** (D59) | Several closed front pages, mid-session | Start time, start station, difficulty, staffing |
| **The driver** | The booklet just picked up | Confirmation of what they hold, and the shape of the work |

The first two read it **closed and in bulk**, which is why nothing they need may depend on opening
the booklet, and why their fields must sit in fixed positions rather than flow with content length.
The proven layout satisfies all three by putting the duty number in the optical centre of the page
at the largest size used anywhere in the report, with the choosing fields immediately below it.

Reference layout — see `Documentation/DutyFromPageExample.PNG`:

| # | Content | Example | Source | Typographic role |
|---|---|---|---|---|
| 1 | Layout name | *Grimslöv H0* | `Layout.Name` | Largest heading, coloured |
| 2 | Plan validity dates | *Gäller 2026-03-06 – 2026-03-08* | **missing — see G1** | Subheading, coloured |
| — | *rule* | | | Separates identity from duty |
| 3 | "Duty" label | *Tjänst* | translation | Large, plain |
| 4 | **Duty number** | *1* | `DriverDuty.Identity` | Dominant element of the page |
| 5 | Highlighted note | *Read the general instructions before you start!* | translation, constant | Boxed, red, unmissable |
| 6 | Sessions / days | *Måndag till tisdag* | `Duty.Sessions` + `SessionsSettings` | Bold, prominent — see §5.1.1 |
| 7 | Start time at station | *Startar: 04:55 vid Mohult* | `Duty.StartTime`, `StartLocation` | Value bold, labels plain |
| 8 | End time at station | *Slutar: 05:30 vid Växjö* | `Duty.EndTime`, `EndLocation` | As above |
| 9 | Operator(s) | *Operatör: SmoK* | `Company.Logo` when every one has a logo, else `Company.Signature` (G8) | As above |
| — | *rule* | | | Separates work from grading |
| 10 | Train category names | *Persontåg* | `TrainCategory.Name` of the duty's trains | Plain |
| 11 | Difficulty | *Svårighet: 1* | **missing — see G2** | Coloured by grade |
| 15 | Staffing, **only when > 1** | *Bemanning: 2 personer* | **missing — see G9** | Beside difficulty, emphasised |
| 14 | **Duty notes** | *Fikapaus i Växjö 05:00.* | `DriverDuty.Notes` | Free-flowing list, plain |
| 12 | Print date and time | *Utskrivet 2026-03-07 14:15* | render-time clock (D61) | Small, subdued |
| 13 | Page number | *– Sida 1 –* | `PageFormat.PageNumber` | Footer |

Numbers 1–13 are the callouts of the example image and are kept stable; the table is ordered by
position on the page, so 14 and 15 — which have no counterpart in the example — sit where they
belong rather than at the end.

Notes on individual fields:

- **Operator (9) and train categories (10) follow the same rule**: take the duty's train parts,
  project to the field, **select distinct**, and join with commas. Both are plural in the general
  case — a duty may work trains of more than one company, and of more than one category. The
  example shows the single-company, single-category case.

  ```csharp
  // Both fields, one shape. Train.Company and Train.Category are nullable, so drop the blanks.
  duty.OrderedParts.Select(p => p.Train.Company).OfType<Company>().Distinct()
  duty.OrderedParts.Select(p => p.Train.Category?.Name).OfType<string>().Distinct()
  ```

  Three details:

  - **Both are nullable** on `Train`, so a part with no company or no category contributes nothing
    rather than an empty entry between commas.
  - **Order is working order**, taken from `OrderedParts` — the first company or category the
    driver works appears first. Nothing else: no alphabetical sort, no reordering for looks. The
    booklet is read in sequence (§1.2), so the front page's summary runs in the same order as the
    part pages behind it, and a driver checking one against the other finds them in step. Any other
    order would introduce a second sequence competing with the one the booklet is built on. It also
    needs no sort at all — `OrderedParts` is already in that order.
  - **Operator uses `Company.Signature`, not `Company.Name`.** The example's *SmoK* is a signature,
    and signatures keep the line short when several companies are joined, which matters on a page
    where this competes with the duty number for space.

  **Operators may render as logos instead (G8).** The rule is **all or nothing**, decided over the
  distinct companies of *this* duty:

  ```csharp
  var companies = duty.OrderedParts.Select(p => p.Train.Company).OfType<Company>().Distinct().ToList();
  var useLogos = companies.Count > 0 && companies.All(c => !string.IsNullOrWhiteSpace(c.Logo));
  ```

  - **All have a logo** → render the logos, separated by a gap.
  - **Any lacks one** → render *every* company as a signature, comma-joined, including those that
    do have a logo.

  Mixing the two would put a graphic and a text abbreviation on one line as if they were the same
  kind of thing. The eye cannot weigh them against each other, and the company without a logo reads
  as an omission — a missing image rather than a deliberate rendering. Falling back for all of them
  keeps the line internally consistent at the cost of some polish, which is the right trade on a
  page whose job is to be read fast and identically every time.

  Two consequences worth naming:

  - **The decision is per duty, not per plan.** Two booklets from the same plan may legitimately
    differ — one duty working only companies that have logos shows logos, another does not. That is
    correct: each front page is internally consistent, which is what a reader sees.
  - **Logos are separated by a gap, not by commas.** Commas belong between words; between images
    they read as stray punctuation. This is why the join cannot simply be reused.
- **Print date (12)** is what distinguishes a reprint from the original after a correction (§2.1) —
  it is the only way to tell two booklets of the same duty apart. It is the **render** time, fixed
  when the PDF is produced, not when paper comes out of the printer (D61); a PDF printed a week
  later still carries the timestamp of the render it came from, which is exactly what makes it
  identify a version.
- **Difficulty (11)** lets a participant pick a duty matching their experience — at the start of a
  session and again at every changeover (D59). Colour-coding implies a small ordered scale rather
  than a free number.
- **Staffing (15) appears only when more than one person is needed.** One is the overwhelming
  default, so printing *"Staffing: 1"* on every booklet would add a line to every front page to say
  what the reader already assumes — against D58's rule of designing for the common case. When it is
  2 or 3 the line is not a detail but an instruction: **this duty cannot be started alone**, and the
  participant must find someone before taking it. That makes it a chooser's field (D59), which is
  why it sits beside difficulty rather than among the identity fields: both answer *what kind of
  commitment is this?*
- **Duty notes (14)** are `DriverDuty.Notes` — a collection of `DriverDutyNote`, each carrying plain
  `Text`, authored by hand and applying to the whole duty rather than to one call. They are rendered
  as a list in insertion order; there is no `DisplayOrder` on the type, and the editing UI can
  reorder the collection if an order is ever wanted.

  Two reasons for the position, **last before the footer**:

  - The elements above it are what the **sorter** reads off a closed booklet (D2), and they must
    stay in fixed positions. Duty notes are the only variable-height element on the page, so they
    can only grow downwards into free space without displacing anything.
  - The driver reads them once, on picking the booklet up — after confirming they have the right
    duty, before turning to the first part.

  **Overflow follows D26**: the notes still print, overflowing the page, and the planner is told by
  a validation message. Silently truncating a hand-written instruction is the one failure mode worth
  ruling out; the notes are the planner's channel for anything the model cannot express.
- **The highlighted note (5)** points at the separate general instructions report (§5.5), not at a
  page of this booklet. It is weaker than a pointer to the back cover — the document is in another
  hand — but the instruction is genuinely "read this before you start", which no page of the duty
  booklet now satisfies. It stays boxed and red because a participant who skips it is the case it
  exists for.

#### 5.1.1 The session indicator

The single most important element for the pile-sorting step (D2), and the one that differs most
from the prototype. It has **two modes**, chosen by `SessionsSettings.UseDaysInsteadOfSessionNumbers`.

**Days mode — textual.** What the prototype did, and what the example image shows. Two shapes,
depending on whether the days are contiguous:

| Pattern | Rendering |
|---|---|
| Contiguous run | *Monday to Wednesday* |
| Scattered | *Monday, Wednesday, Friday* |

Day names come from `FullDayNamesResourceKey(startDay)`; `UseShortWeekdayNames` selects the short
forms.

**Session-numbers mode — graphical.** Session numbers are a new concept the prototype never had, so
there is no proven precedent to copy. Plain text — "1,3,5" or "1-3" — is weak precisely where it
matters most: read at arm's length, off a closed booklet, in a stack. The specified rendering is
therefore a row of **filled black circles, each with its session number centred in white**:

```
   ●①  ●③  ●⑤          ←  a 1,3,5 duty
   ●①  ●②  ●③          ←  a 1-3 duty
```

Note that the *text* form already exists and handles both shapes: `Sessions.SessionsNumbers`
collapses consecutive runs into ranges and joins them with commas ("1-5,8-12"). The graphical form
replaces that string on the front page, but the underlying `Numbers` are the same data.

Rendering rules:

- **Only active sessions are shown.** No placeholders for sessions the duty does not run.
- One circle per active session, in ascending order.
- Numeral centred, white, bold; circle filled solid black.
- Two abbreviations apply — all sessions, and contiguous runs — see the table below.

**The four display forms.** Session-numbers mode and days mode take the same three shapes, plus an
on-demand marker that combines with any of them:

| # | Condition | Session-numbers mode | Days mode |
|---|---|---|---|
| F1 | Runs every session of the operating period | *All sessions* (text) | *Daily* (text) |
| F2 | A contiguous run, **per run** | ③–⑧ — two circles joined by a dash | *Monday to Wednesday* |
| F3 | Short runs and lone sessions | one circle each: ①  ③  ⑤ | *Monday, Wednesday, Friday* |
| F4 | On demand — *combines with F1–F3* | above **plus** *On demand only* | as above **plus** *On demand only* |

Notes on each:

- **F1 must use `CoversAllWithin(useDays, maxSessions)`, not a count of 14.** A three-session
  meeting where the duty runs all three sessions is "all sessions" for that meeting. The existing
  `SessionsNumbers` returns the literal string `"All"` only when all fourteen bits are set, which
  would be wrong here. Likewise `"None"` for an empty pattern is a planning error worth showing as
  such rather than printing an empty band.
- **F2 is graphical and applies per run.** The short form is *two circles joined by a dash* — the
  first and last of the run — not a text string. The visual language is preserved; only the count
  of circles falls. A pattern of several runs abbreviates each one independently, so 1,2,3,7,8,9
  renders ①–③ ⑦–⑨. Runs and lone sessions mix freely in one indicator: 1,2,3,5 renders ①–③ ⑤.
  The dash is an en dash, matching the day form's "to".
- **F3 is therefore not a separate mode** but what a run shorter than the threshold falls back to.
  The rendering algorithm is: split the active numbers into contiguous runs (the logic already in
  `FormatSessions`), then render each run as F2 or F3 by its length.
- **F4 is additive, never a replacement.** On demand does *not* hide the sessions. A duty may run
  sessions ①–③ *and* be worked on demand on those three sessions, so the reader needs both facts.
  The model already carries the flag: `Sessions.IsOnDemand`. The existing `OnDemand` property
  returns the untranslated literal `"OnDemand"`, so the report needs a proper translation key.

**Open — Q19a, the only remaining detail:** the run length at which F2 replaces F3.
**Recommendation: three.** It matches days mode, where three contiguous days already render as
"Monday to Wednesday" rather than a list, so both modes abbreviate at the same point. A run of two
is excluded because ②–③ occupies the same width as ② ③ while reading worse.

**Implementation constraint:** the circles must be drawn as **SVG with a `fill` attribute**, not as
a CSS `background-color` on a rounded element. Browsers omit background colours when printing
unless `print-color-adjust: exact` is set, which would make the indicator disappear on exactly the
artefact it exists for. SVG fills always print.

#### 5.1.2 The session number component — shared, not report-specific

Session circles are used **everywhere a session number is displayed**, in the application as well as
in this report. They are therefore a shared component, not part of the Duties report.

The application already displays sessions in at least nine places — `DutiesTab`, `TrainsTab`,
`SchedulesTab`, `SettingsTab`, `DutyChart`, `EditDutyDialog`, `ScheduleTurnChart`,
`AssignVehicleDialog`, `EditVehicleDialog` — and the report adds more. All of them should use the
same component.

**Placement.** `Planning.App` references `Planning.Components`, so a component placed in
`Planning.Components` is reachable from both the GUI and the reports. That project has only
`Reporting/` and `Scheduling/` today, so this introduces a shared folder — see Q18.

**Scaling.** The contexts differ enormously: the front page wants circles as tall as a heading, a
table row wants them at body-text size, a chart smaller still. The component must scale rather than
offer fixed sizes.

The mechanism is to size the SVG in `em` and let it inherit the surrounding font size, so a circle
is always as tall as the text beside it and needs no size parameter at all:

```razor
@* SessionNumber.razor — one circle *@
<svg class="sessionnumber" viewBox="0 0 100 100" role="img" aria-label="@Number">
    <circle cx="50" cy="50" r="50" fill="black" />
    <text x="50" y="50" fill="white" text-anchor="middle" dominant-baseline="central"
          font-size="@FontSize" font-weight="bold">@Number</text>
</svg>

@code {
    [Parameter] public int Number { get; set; }
    string FontSize => Number < 10 ? "62" : "48";
}
```

```css
.sessionnumber { width: 1em; height: 1em; vertical-align: -0.125em; }
```

Points that matter:

- **`viewBox` with no width/height attributes**, sized entirely in CSS `em`. One component, every
  context; a caller that wants larger circles simply raises the font size.
- **Two-digit numbers.** `MaxSessions` allows up to 14, so the numeral must shrink for 10–14 to
  stay inside the circle. A fixed `font-size` per digit count is enough; `textLength` with
  `lengthAdjust="spacingAndGlyphs"` is the alternative if the eye rejects it.
- **`dominant-baseline="central"`** centres the numeral vertically. It is well supported in Chrome
  and Edge, but it should be verified in *print* output specifically, since that is where this
  report lives. A `dy` offset is the fallback if it proves unreliable.
- **`aria-label`** so the number is available to screen readers and to text extraction from the
  browser, which a bare circle would not be.

A companion component renders a whole `Sessions` value — day text or a row of circles, chosen by
`SessionsSettings.UseDaysInsteadOfSessionNumbers` — so no caller has to decide the mode itself.
This is the component that should sit behind every one of the nine call sites above.

### 5.2 Layout overview page (last page)

Two graphical references, identical in every booklet, on the page a driver reaches by turning to the
back. Unlike the general instructions — read once, before the meeting — these are consulted
*during* a run, which is why they stay bound into the booklet the driver is holding.

| Content | Source |
|---|---|
| Layout topology | The schematic already drawn by the Stretches ▸ Topology sub-tab |
| Shunting yards and what they cover | Derived: each shunting yard, with the locations it serves **and the regions it serves** |

Both answer questions that arise mid-duty: *where does this line go?* and *which shunting yard does
this industry hang off?* Both are also compact — a diagram and a short table — which is what makes
the one-page rule (D4) hold now that the authored markdown has moved out to §5.5.

**Reuse for the topology.** `TopologyDiagram.Build(layout)` in
`Planning.App/Components/Stretches/TopologyDiagram.cs` is a static pure function returning a record
of nodes, lines and connectors; `TopologyView.razor` renders it as SVG. Both are reusable as-is —
the report needs no topology drawing code of its own, only a print-appropriate size. Being SVG, it
prints reliably (D8).

#### 5.2.1 The shunting yards table

Three columns, because a shunting yard covers two different kinds of destination:

| Shunting Yard | Serves locations | Serves regions |
|---|---|---|
| Munkeröd | Rubjerg, Stilkøbing, Bruket *(industrial area)* | — |
| Mohult *(shadow)* | — | ⬛ ØST  ⬛ FYN  ⬛ NORGE |
| Växjö | Ålsheda, Lenhovda | ⬛ SYD |

- **Serves locations** is the inverse of the `CargoServedFrom` relation (G6): the on-layout stations and
  industrial areas whose local freight is worked from this shunting yard.
- **Serves regions** is `Station.Regions` — the off-layout destinations that station stands for.
  `Region` is documented as *"a destination outside the layout — a domestic region or a foreign
  country — used for cargo flow routing. A `Station` (normally a shadow shunting yard) can be
  associated with zero, one, or several regions."*

Together they answer the question a driver actually has when holding a wagon card: **where does this
wagon go from here, and is that on the layout or off it?** One column alone answers only half.

**Regions render as the same coloured chips used everywhere else**, through
`Region.ToHtml`. The driver then meets an identical visual token on the overview page and in
the cargo notes of the timetable (`CargoFlowDestinationNote`), so the overview page reads as a key
to the notes rather than as separate information. This is also why the `.region`
`print-color-adjust` fix (D21b) matters here and not only in the notes.

**This widens what counts as a shunting yard — see D68.** A shadow station carrying regions but
serving no on-layout location is exactly the case G6's derivation would have omitted, and it is
precisely the kind of station a driver needs listed.

**The shunting yards table needs new model data** for the locations column — see gap G6. The regions
column needs nothing new; `Station.Regions` already exists and is already edited.

**Overflow.** A layout with many shunting yards could still exceed the page. The overview page must
not split, because D4 depends on it being exactly the last page. If a layout ever outgrows it, the
shunting yards table is the part that scales — a two-column layout, then station signatures instead
of full names — before a second overview page is contemplated. Shrinking the type is not on that
list; D57 puts a floor under it. Treated as a tuning problem, not a structural one.

### 5.3 Train part page

A train part is rendered as a **header block followed by four table blocks**, separated by
horizontal rules, in this fixed order:

```
Duty in SB train 1234                                    ← heading
Passenger train.  Starts at: Munkeröd 11:20, ends at Stilkøbing 14:20
Max speed: 100 km/h. Max axles: 24. Max length: 2.5 m    ← only the limits that are set
──────────────────────────────────────────────────────────────────────
Traction units          │ Sessions │ Traction unit │ From │ Dep │ To │ Arr │
──────────────────────────────────────────────────────────────────────
Scheduled wagonsets     │ Sessions │ Wagonset      │ From │ Dep │ To │ Arr │
──────────────────────────────────────────────────────────────────────
Cargo wagons with waybills │ Position │ Sessions │ Wagons from │ Classes │ Wagons to │
──────────────────────────────────────────────────────────────────────
Train timetable       │ Arr/Dep │ Station │ Track │ Time │ Note │
──────────────────────────────────────────────────────────────────────
              Duty continues on next page!              ← foot of page, §5.3.8
```

The order is deliberate and matches how the driver works: *what am I driving* (traction), *what am
I pulling* (wagonsets), *what is in it and where does it go* (cargo), then *where and when* (the
timetable). The timetable is last because it is the block the driver returns to repeatedly during
the run, so it sits closest to the following part.

**Why there are four blocks and not one — the central lesson from the prototype.** The prototype
printed a train timetable *and nothing else*, so every fact about traction, wagonsets and cargo had
to be squeezed into the note column of a call. The notes grew long, repetitive and hard to scan
precisely where a driver can least afford it.

The three tables above the timetable exist to **absorb that information into structured columns**.
The consequence is a rule for note design:

> Where a fact is already in one of the three tables, the timetable note does not repeat it — it
> points at it. *"Brings wagonset and cargo wagons from here, see above."*

So timetable notes fall into two kinds:

1. **Pointers** — short reminders to consult the traction, wagonset or cargo block above.
2. **Genuinely per-call facts** that have no other home, attached to a specific arrival or
   departure: crossings and overtakings, parking movements, *No stop*, *No exchange*.

Anything longer than that is a sign the fact belongs in a table instead.

**The existing `TrainPartTractionView.razor` is the template for all four blocks.** Its shape —
render-guard, translated heading, table with a header row, trailing rule — is the pattern the other
three follow:

```razor
@if (HasData)
{
    <h3>@Translator("TractionUnits")</h3>
    <table class="dutytrain">
        <thead><tr class="dutytrainheader"> … </tr></thead>
        <tbody> @foreach(…) { <tr class="dutytrainrow"> … </tr> } </tbody>
    </table>
    <hr/>
}
```

#### 5.3.1 Common conventions

These apply to every block, and stating them once keeps the four components consistent.

- **Empty blocks are omitted entirely** — heading, table and rule. A part with no wagonsets shows
  no "Scheduled wagonsets" heading. The `@if (HasData)` guard in the traction view already does
  this. This matters for page fitting: a suppressed block costs zero height (§6.2).
- **The Sessions column comes first in the traction and wagonset blocks**, and uses the session
  indicator of §5.1.1 — circles, not text (D10). The cargo block puts **Position** first instead,
  because that is what its rows are sorted by (§5.3.5); the general rule is that the leading column
  is the sort key. The train timetable has no Sessions column at all; session-dependent facts there
  live in the note text instead.
- **Times are `Time`**, rendered as in the rest of the application.
- **CSS follows the nesting convention**: a single parent class with nested selectors, not flat
  prefixed names. The current `dutytrain` / `dutytrainheader` / `dutytrainrow` triple should become
  `.dutytrain { thead tr { … } tbody tr { … } }`.

#### 5.3.2 Part header block

| Element | Example | Source |
|---|---|---|
| Heading | *Duty in SB train 1234* | `Company.Signature` + `Train.Number` |
| Category, full name | *Passenger train* | `Train.Category?.Name` |
| Start and end | *Starts at: Munkeröd 11:20, ends at Stilkøbing 14:20* | part `From` / `To` calls |
| Limits | *Max speed: 100 km/h. Max axles: 24. Max length: 2.5 m* | `Train.MaxSpeed`, `Train.Length` |

- The heading uses the **train's own** company signature, which need not be the duty's — a duty may
  work several operators' trains (D27).
- `Train.Category` is nullable; the line is omitted when unset.

**The limits line carries every restriction the train has, and only those.** Four values are
possible — maximum speed, axles, wagons and length — and each is independently optional:

| Limit | Source | Type |
|---|---|---|
| Max speed | `Train.MaxSpeed` | `int?` |
| Max axles | `Train.Length.Axles` | `int?` |
| Max wagons | `Train.Length.Wagons` | `int?` |
| Max length | `Train.Length.Meters` | `double?` |

**An unset limit contributes nothing at all — no value, and no label either.** A train with only an
axle limit prints *"Max axles: 24"*, not *"Max speed: –. Max axles: 24. Max wagons: –."* The rule is
built into the construction rather than applied afterwards:

```csharp
// Each limit yields a label-value pair only when it has a value; the line is the join of what survives.
string?[] limits =
[
    train.MaxSpeed is { } speed ? $"{Translator("MaxSpeed")}: {speed} km/h" : null,
    train.Length.Axles is { } axles ? $"{Translator("MaxAxles")}: {axles}" : null,
    train.Length.Wagons is { } wagons ? $"{Translator("MaxWagons")}: {wagons}" : null,
    train.Length.Meters is { } metres ? $"{Translator("MaxLength")}: {metres:F1} m" : null,
];
var line = string.Join(". ", limits.OfType<string>());
```

Three reasons this matters more than it looks:

- **A printed label with no value is worse than silence.** On paper a driver cannot tell "not
  restricted" from "the planner forgot"; an absent line says the first unambiguously.
- **Most trains have one or two limits, not four.** Printing all four labels every time would
  usually be mostly empty and would cost a line the page fitting has to pay for.
- **The whole line disappears when nothing is set**, and costs zero height — the same rule as an
  empty block (§5.3.1).

**Do not use `TrainLenght.ToString()`.** It renders the symbolic compact form — `24ʘ 12■ 2.5m` —
built from `internal` extensions for dense tabular contexts, and it returns the literal
*"Undefined"* when nothing is set. Neither suits a header that must read plainly and vanish when
empty. The report reads `Axles`, `Wagons` and `Meters` directly and labels them in words.

**The labels are translated**, so `MaxAxles`, `MaxWagons` and `MaxLength` need keys in all five
languages alongside the existing speed key.

**Header start is the arrival, not the departure.** *"Starts at: Munkeröd 11:20"* against the
timetable's *"Dep Munkeröd … 12:20"* is deliberate: 11:20 is the train's **first arrival time** and
12:20 its departure, leaving an hour of preparation at Munkeröd. The header therefore takes
`From.Arrival` and the timetable shows `Departure` on its Dep row — the same distinction
`DriverDuty.DefaultStartTime` already makes at duty level (§5.1), where the duty starts when the
driver reports rather than when the train moves.

The end time follows the mirror rule: the header's *"ends at"* is the last call's **departure** —
the moment the driver stands down after the train has arrived — matching `DefaultEndTime`.

#### 5.3.3 Traction units — implemented

| Sessions | Traction unit | From | Dep | To | Arr |
|---|---|---|---|---|---|
| All | SB MX 1 | Munkeröd | 12:20 | Stilkøbing | 14:00 |

Already built in `TrainPartTractionView.razor` against `TrainPartTractionData`. Outstanding: the
`DriverDutyPart.TractionData` mapping still throws `NotImplementedException`, and the Sessions cell
calls the unimplemented `Sessions.Display` (G3), so it currently renders blank.

**The Traction unit column is the one the driver matches against the loco card (§1.3), and it is
currently wrong twice over:**

1. **It prints the wrong object.** The cell is `unit.TrainPart.ToString()`, and
   `TrainPartTractionUnit.TrainPart` is a `ScheduledTrainPart`, whose `ToString()` yields
   `'Train' From dep->To arr` — the movement, not the vehicle. The other four columns already show
   the movement, so this cell must carry the vehicle instead. The vehicle is reachable through
   `Plan.ScheduledObjectsFor(trainPart)` filtered by `IsTraction`, which
   `ScheduledTrainPartExtensions` already does privately.
2. **`ScheduledObject.ToString()` is not the identity to print either** — it returns
   `ComposedIdentity` (company signature, class, number), bypassing `ExternalId`. The property that
   respects the external id is **`Designation`**, documented as "how a vehicle is identified
   everywhere else in the app, e.g. `DBSCH EG 01`". A vehicle carrying an external id would print
   under a different name here than everywhere else — precisely the mismatch §1.3 rules out.

So the cell must be `unit.TractionUnit.Designation`, which means `TrainPartTractionUnit` needs the
`ScheduledObject` on it, not only the train part.

#### 5.3.4 Scheduled wagonsets

| Sessions | Wagonset | From | Dep | To | Arr |
|---|---|---|---|---|---|
| 1,3,5 | Hbis 1 | Munkeröd | 12:20 | Stilkøbing | 14:00 |
| 2,4,6 | Zacs 1 | Munkeröd | 12:20 | Stilkøbing | 14:00 |

Structurally identical to traction units — same six columns, same shape — differing only in which
scheduled objects it lists (`IsWagonSet` rather than `IsTraction`). `TrainPartWagonsetData` exists
as a stub; `TrainPartWagonsetView.razor` is empty.

The Wagonset column follows the same rule as the traction one: **`ScheduledObject.Designation`**,
because wagonsets carry cards too and the driver has to pick the right rake out of several standing
in the yard.

The example shows the case this block exists for: **the same train carries different wagonsets on
different sessions**. One booklet covers all sessions (D1), so both rows must appear, distinguished
by the Sessions column.

#### 5.3.5 Cargo wagons with waybills

Ordered by **position, then by session** — and **Position leads the columns**, unlike the two blocks
above:

| Position | Sessions | Wagons from | Classes | Wagons to |
|---|---|---|---|---|
| 1 | 1,3,5 | Munkeröd, also shunt | | Stilkøbing og lokalt, ØST, FYN |
| 1 | 2,4,6 | Munkeröd, also shunt | U,Z | Stilkøbing, NORGE |
| 2 | 1,3,5 | Rubjerg | | Stilkøbing og lokalt, ØST, FYN |
| 2 | 2,4,6 | Rubjerg | | Stilkøbing, NORGE |
| Any | All | Munkeröd | | Rubjerg |

Five columns, and no times — a cargo wagon's timing is the train's. **Position** is the wagon's place
in the rake, so the driver and conductor can find it.

**A row is one `CargoFlowTrainPart`.** Not one destination: a flow's destinations belong together in
a single statement of where these wagons go, and `CargoFlowTrainPart` is also what carries the
per-occurrence behaviour the row must show. Every column below is read from that one object or from
the `CargoFlowOptions` it references.

| Column | Source |
|---|---|
| Position | `CargoFlowTrainPart.PositionInTrain`, **rendered *"Any"* when 0** |
| Sessions | the cargo flow's sessions, as circles (§5.1.1) |
| Wagons from | `CargoFlowOptions.Origins`, plus the from-station when wagons are taken there — see below |
| Classes | `CargoFlowOptions.OnlyWagonClasses` — the UIC letters limiting which wagons the flow brings |
| Wagons to | the destinations — `CargoFlowTrainPart.ToHtml` already composes exactly this |

**Position 0 prints *"Any"*.** Zero means *anywhere in the train*, and a column showing `0` would be
read as a position — position zero, at the front. The word states the fact the number encodes.

⚠ **Two different `PositionInTrain` properties exist.** `CargoFlowTrainPart.PositionInTrain` (this
column) and `Destination.PositionInTrain` are separate values on separate types, both `int`, both
named the same. The row's position is the **flow's**, not the destination's. Worth checking at the
point of implementation, because the wrong one compiles perfectly.

**"Wagons from" is a union of two sources, each name appearing once:**

```csharp
// The flow's forwarded origins, plus this station itself unless nothing is taken here.
IEnumerable<string> names = flow.CargoFlowOptions.Origins.Select(o => o.Station.Name);
if (!flow.BringsNoWagonsFromHere)
    names = names.Append(flow.From.OperationLocation.Name);
var from = string.Join(", ", names.Distinct());
```

`BringsNoWagonsFromHere` is documented as *"no wagons are brought from the from-call's station; the
flow still forwards wagons from its origins"* — so it removes exactly one of the two sources, never
the whole column. The `Distinct()` matters: a flow whose origin list already includes the
from-station would otherwise print it twice.

**"Also shunt" is appended to a column, not given one of its own:**

| Qualifier | Condition | Column |
|---|---|---|
| *also shunt* | `AlsoShuntBeforeDeparture` | Wagons **from** |
| *also shunt* | `AlsoShuntAfterArrival` **and not** `BringsNoWagonsFromHere` | Wagons **to** |

Both mean the driver performs the shunting themselves — before departure in the first case, after
arrival in the second — so each belongs beside the movement it qualifies rather than in a separate
column that would be empty on most rows.

*One reading to confirm:* the `BringsNoWagonsFromHere` condition is taken to apply to the
arrival case only, as written. It is the less obvious of the two — a flow that brings no wagons from
here may still set wagons down — so it is worth a second look before implementation.

**An empty Classes cell means no restriction**, printed as nothing rather than *"all"* or a dash.
`OnlyWagonClasses` is documented as *"empty means any wagon class"*, and by D70's rule an absent
restriction says more by being absent than by being spelled out on every row.

**The leading column is the sort key.** Rows are ordered by position (D33), so position is what the
eye follows down the table: the repeated *1, 1, 2, 2, 3* reads as grouping, and a driver looking for
position 2 scans one column instead of tracking a value in the middle of the row. With Sessions
first, the sorted column sat second and the leading column repeated in no order at all — which reads
as an unsorted table.

Sorting by position first puts everything about one place in the rake together, so the driver
reading position 2 sees both session variants side by side. Note this **reorders the original
outline**, which grouped by session instead; the table above is the specified order.

Within a position, sessions sort by `FirstNumber` and then by flag value, which is deterministic
and puts 1,3,5 before 2,4,6.

#### 5.3.6 Train timetable

**Five columns, with Arr/Dep in one of its own:**

| Arr/Dep | Station | Track | Time | Note |
|---|---|---|---|---|
| Dep | Munkeröd | 5 | 12:20 | Pick up loco from parking. |
| | | | | Brings wagonset and cargo wagons from here, see above. |
| | Delsbo | 1 | 12:25 | No stop |
| Arr | Slokärr | 2 | 12:30 | No exchange. Meets SB 4321 in sessions 1,3,5 |
| Dep | Slokärr | 2 | 12:33 | |
| | Tomvik | 1 | 12:37 | No stop. |
| Arr | Rubjerg | 5 | 12:40 | |
| Dep | Rubjerg | 5 | 13:40 | Brings cargo wagons from here, see above. |
| Arr | Stilkøbing | 9 | 14:00 | Drive loco to parking. |

**Why its own column.** The prefix varies in presence and width — *"Dep Munkeröd"*, *"Delsbo"*,
*"Arr Slokärr"* — so sharing a column leaves the station names starting at three different
positions. The route is the thing the driver follows down the page, and it reads as a list only when
the names align on one left edge. Splitting the column aligns the names *and* the Arr/Dep tokens,
turning two ragged things into two straight ones. A pass-through then shows an **empty** Arr/Dep
cell rather than a missing prefix, which is a clearer statement of "neither" than the absence of a
word.

The station name is **repeated on both rows** of a two-row call, not printed once with the second
row left blank. Each row stays self-contained, so a time is never read against an empty station
cell — cheap insurance on paper, where the eye slips between adjacent rows.

Two differences from the current `TrainPartView.razor` stub, which must be reworked:

1. **The number of rows is decided by the times, not by `IsStop`.** A call whose arrival and
   departure differ occupies the platform for a while and gets two rows; a call whose times are
   equal is a single moment and gets one, with an empty Arr/Dep cell.
2. **The first and last calls of the part are special**, each showing one row only:

| Call | Rows | Why |
|---|---|---|
| First call of the part | **Dep only** | Its arrival is the header's *"starts at"* — preparation time (D32) |
| Last call of the part | **Arr only** | Its departure is the header's *"ends at"* — stand-down time |
| Intermediate, arrival = departure | **One**, empty Arr/Dep cell, showing the **departure** time | *Delsbo 12:25* — the train passes through |
| Intermediate, arrival < departure | **Two**, Arr then Dep | *Slokärr 12:30 / 12:33* — the train stands |

The stub already keeps Arr/Dep and the station in separate cells, so on this point it was right and
needs no change — only the row-structure rules above do.

**One note per row, stacking downwards — not one paragraph of concatenated text.** The prototype
printed a call's notes as a single running text in the note column, and it was hard to read and
wrapped unpredictably. Instead:

```
│ Arr │ Slokärr │ 2 │ 12:30 │ No exchange.                                  │
│     │ Crosses SB 4321 12:30–12:33 in sessions ①③⑤                         │
│ Dep │ Slokärr │ 2 │ 12:33 │                                               │
```

- The **first** note sits in the Note column of the call's own row, keeping it beside the time it
  belongs to.
- **Every further note gets its own row starting at the station column** (an empty Arr/Dep cell, then
  `colspan` over the remaining four), so it has almost the whole page to run in and almost never wraps,
  while its left edge still aligns with the station name above it rather than the Arr/Dep column.

Three things this buys, the third being the one that matters most:

1. **Each note is a discrete line.** A reader scanning for one fact does not have to parse a
   paragraph to find where one note ends and the next begins.
2. **Wrapping becomes rare instead of routine.** A full page width holds roughly twice what the note
   column does, so what used to wrap now fits.
3. **Height estimation becomes close to counting.** §6.2 charges note height per *N* characters
   precisely because wrapping was unpredictable. With one note per row, the estimate is **one unit
   per note**, plus wrapping only for the exceptional note that exceeds a full page width. That is a
   far more reliable estimate than the prototype's, and it is the single biggest improvement
   available to the pagination.

**`table-layout: fixed` with declared column widths is required, not optional.** With automatic
layout a full-width `colspan` cell participates in width calculation, so a long note would widen the
note column and shift every other column — and column widths would then depend on content, which
would make the characters-per-line rate vary from part to part. Fixed layout keeps the four narrow
columns constant down the whole table and turns the wrap width into a known constant, which is what
makes "estimate, never measure" (D25) sound for this table.

**The first note earns its place in the note column by being short enough to fit.** Rather than
accept two wrap widths, the first note is measured: if it is under a conservative character limit it
goes in the column, and otherwise it joins the others on a full-width row, leaving the note cell
empty.

```csharp
// A single constant, tuned with the rest of §6.2. Deliberately below the column's true capacity.
const int MaxCharsInNoteColumn = 25;

var stacked = notes.Count > 0 && notes[0].ToText.Length <= MaxCharsInNoteColumn
    ? notes.Skip(1)   // first note sits beside the time
    : notes;          // all notes stack below
```

This is why the limit works: **the notes that occur most often are the short ones.** *No stop* is
seven characters, *No exchange* eleven — they always fit, so the common call keeps its compact
single row, which is the density D103 was protecting. Only a genuinely long note takes a row of its
own, which is what it needed anyway.

What the rule buys:

- **No note ever wraps in the narrow column** — by construction, since anything that might is moved
  out of it. The two-rate estimate collapses to one.
- **Height is now `notes.Count`, less one when the first note is absorbed into the call row**, plus
  full-width wrapping which is rare. That is counting, with a single exception.
- **The limit is measured on `Text`, never `Html`** (D35) — markup would inflate the count and push
  short notes out of the column for no reason.

Start at **25 characters** and tune it with the other §6.2 constants against a real print. It is
deliberately below the column's true capacity: overflowing the column costs an unpredictable wrap,
while being too cautious costs one tidy row, so the error is worth biasing.

**Notes are never reordered to fit.** If the first note by `DisplayOrder` is too long, that is the
one that moves the whole group down — a shorter later note is not promoted into the column. Note
order carries meaning; layout does not get to rearrange it.

Continuation rows should be marked as belonging to the call above, so they are never mistaken for a
new call — done by starting them at the station column (D109) rather than by an arbitrary indent.

**A single-row call carries both arrival and departure notes.** This is the general rule, not a
special case for any one note type: whenever a call renders as one row, the split that D28 makes
between `IsForArrival` and `IsForDeparture` has nowhere to land, so both sets appear together on
that row, ordered by `DisplayOrder` as everywhere else — the first in the note column, the rest
stacked beneath it.

```csharp
// Two rows: notes split by audience half. One row: the halves merge.
var notes = rowCount == 1
    ? call.DriverNotes.OrderBy(n => n.DisplayOrder)
    : call.DriverNotes.Where(n => half).OrderBy(n => n.DisplayOrder);
```

Without this, any note classified for the missing half would be silently dropped — and the notes
most affected are the ones that only *occur* on single-row calls. It also removes the need to
choose a half for the sake of visibility rather than meaning: a note can be classified by what it
describes, knowing the renderer will show it either way.

For height estimation this means a single-row call is charged **all** of its notes on that one row
(§5.3.7), which is where a merged row can grow taller than either half would have been.

Each row carries its own notes, split by `IsForArrival` / `IsForDeparture`.

**`IsStop` is orthogonal to the row structure — it decides whether an exchange happens**, and drives
two of the notes:

| `IsStop` | Times | Note | Attaches to | Meaning |
|---|---|---|---|---|
| `false` | arrival = departure | *No stop* | **Departure** | The train runs through without stopping. |
| `false` | arrival < departure | *No exchange* | **Arrival** | The train stands — for a meet, a signal, a crossing — but nothing is picked up or set down. |
| `true` | arrival < departure | *(none)* | — | An ordinary working stop. |
| `true` | arrival = departure | *(none)* | — | The train stops, but briefly enough that no dwell is scheduled. One row, showing the departure. |

This is why both facts are needed: the times say whether the train stands, and `IsStop` says whether
standing means work. *Slokärr* in the example stands for three minutes to meet SB 4321 yet exchanges
nothing, so it reads *"No exchange"* rather than *"No stop"*.

**Which half each note attaches to follows from what the driver reads and when.**

- ***No stop* is a departure note** (`IsForDeparture`). The single row of a pass-through suppresses
  the arrival and shows the **departure** time — *Delsbo 12:25* — so a note attached to the arrival
  would have no row to appear on.
- ***No exchange* is an arrival note** (`IsForArrival`). It answers the question the driver asks on
  pulling in: *do I have work here?* Putting it on the arrival row means the answer is read on
  arriving, not three minutes later on the departure row.

`IsStop` true with equal times is contradictory — a stop that takes no time. Worth a validation
message rather than a rendering rule; see Q29.

**Notes are the substantial part.** `ICallNote` already provides exactly what is needed:

```csharp
public interface ICallNote
{
    int DisplayOrder { get; }        // sort order within a call
    bool IsDriverNote { get; }       // ← the filter for this report
    bool IsStationNote { get; }
    bool IsShuntingNote { get; }
    string Text { get; }
    MarkupString Html { get; }
}
```

- **Filter by `IsDriverNote`.** The same call carries notes for station staff and shunters that must
  not appear in a driver's booklet. This is what the audience flags are for.
- **Sort by `DisplayOrder`**, then render one note per line within the row.
- **Split by `IsForArrival` / `IsForDeparture`** (on `CallNote`) to decide whether a note belongs to
  the Arr row or the Dep row.
- **Both persisted and generated notes appear.** `ICallNote` deliberately spans both families, so a
  `TextCallNote` a planner typed and a `FromParkingNote` the model derived combine into one list.

**Render `Html`, not `Text`.** Notes already carry both — `Text` for plain rendering and `Html` as a
`MarkupString` — so formatting can be added per note type without changing any caller. The report
renders `Html` throughout.

This is not speculative: the mechanism is already in use. A generated note describes itself once, as a
localised format string plus the values substituted into it, and the two forms are two renderings of
that one description:

```csharp
UseNote(var so) => new(NoteResources.Use, so),
CoupleNote(var so, var position) => new(NoteResources.CoupleToTrainInPosition, so, NoteArg.Plain(position)),
CargoFlowDestinationNote(var part) when part.CargoFlowOptions is not null =>
    new(NoteResources.BringsWagonsTo, NoteArg.Markup(part.ToText, part.ToHtml)),
```

Four consequences worth building in from the start:

- **Substituted values print bold.** They are the part of a note that varies — the vehicle to fetch,
  the train to meet — and so the part the reader must not miss. Emphasis is a property of the renderer,
  not a decision taken again per note: an argument is emphasised unless it is passed as
  `NoteArg.Plain` (counts, positions, times) or `NoteArg.Markup` (values with a visual form of their
  own).
- **Session circles and region chips follow that same precedent.** A meet note that knows its sessions
  renders them as circles through `NoteArg.Markup`, exactly as the cargo note renders region chips —
  satisfying D10 with no change to `ICallNote`.
- **Height estimation must measure `ToText`, never `ToHtml`.** Wrapping is charged per N characters
  (§6.2); markup would inflate the count, charging a short note as a long one. The plain-text
  property exists for precisely this.
- **`MarkupString` bypasses HTML escaping.** Every value is therefore encoded on the way into the
  markup, so a station, train or company name containing `&`, `<` or `>` produces a correct note
  rather than broken markup. A manual note is encoded the same way before its Markdown emphasis
  (`*italic*`, `**bold**`) is rendered.

#### 5.3.6.1 Note types needed

Mapping the outline's notes against what the model already has:

| Note in the outline | Type | State |
|---|---|---|
| *Pick up loco from parking.* | `FromParkingNote` | **Exists** |
| *Drive loco to parking.* | `ToParkingNote` | **Exists** |
| *Brings wagonset and cargo wagons from here, see above.* | `UseNote` / `CoupleNote` | Exist, wording differs — the cross-reference to the tables above is new |
| *Brings cargo wagons from here, see above.* | `CargoFlowDestinationNote` | Exists as *"Brings wagons to …"*; the "from here, see above" form is new |
| *No stop* | — | **Missing** — derived: `!IsStop` and equal times |
| *No exchange.* | — | **Missing** — derived: `!IsStop` and arrival &lt; departure |
| *Meets SB 4321 in sessions 1,3,5* | — | **Missing** — two types, see below |

The existing generated family — `UseNote`, `CoupleNote`, `UncoupleNote`, `FromParkingNote`,
`ToParkingNote`, `ReinforcementNote`, `TractionUnitExchangeNote`, `CargoFlowDestinationNote` —
already covers the traction and cargo cases. Scope is the outline's notes; further types can follow
later without changing the report, since everything renders through `ICallNote`.

**Crossings and overtakings are two separate note types**, not one "meet":

| Note type | Condition |
|---|---|
| Crossing | Another train passes the driver's train **in the opposite direction** at this location |
| Overtaking | Another train passes the driver's train **in the same direction** |

**Both are suppressed by the single flag `OperationLocation.HideMeets`**, whose documented meaning
is exactly this — *"Suppress creating notes about trains meeting or overtaking at this location"*.
A location where such events are routine and not worth noting silences both at once. There is no
separate per-type flag, and none is proposed.

**`HidePassings` is unrelated to these notes.** It means *"suppresses the display of trains not
stopping at this location"* — whether non-stopping trains are listed at all, which concerns station
reports rather than the notes on a driver's call. Its bearing on this report, if any, is a separate
question: see Q31.

**The derivation is one overlap test plus a direction comparison.** Both note types share the same
condition for *"both trains are at this location at the same time"*; only the direction differs:

```csharp
// The two trains occupy the location together when each arrives before the other departs.
bool Overlaps(StationCall mine, StationCall other) =>
    other.Arrival < mine.Departure && other.Departure > mine.Arrival;
```

| Both at the location | Direction | Note |
|---|---|---|
| yes | opposite | **Crossing** |
| yes | same | **Overtaking** |
| no | — | none |

**Each meet carries the shared interval and the other train.** The interval is when both are actually
present — `max(arrivals)` to `min(departures)` — not either train's own dwell, because what the
driver needs is the window in which the other train is there to be met. A call can be crossed or
overtaken by more than one train at once, so these aggregate into at most one `CrossingNote` and one
`OvertakingNote` per call, each listing every meet of its kind (D108) rather than repeating a row per
other train.

The other train is named by **`Train.ToString()`**, which already composes exactly what is wanted:

```csharp
// Model/Timetables/Train.cs:193 — company signature, category prefix, number, category suffix.
string.Format(CultureInfo.CurrentCulture, "{0} {1} {2} {3}",
    this.EffectiveCompany?.Signature, Category?.Prefix, Number, Category?.Suffix).Trim();
```

Note it uses `EffectiveCompany`, so a train inheriting its category's company is still named
correctly — the reason to use this rather than assembling the four parts at the call site.

**Direction comes from the `TrackStretch` the train operates on — specifically the one it arrives
on.** A `TrackStretch` has a `Start` and an `End`, so a train traversing it either runs `Start → End`
or `End → Start`. That is the train's direction, and comparing two trains is comparing those two
senses:

```csharp
// The stretch the train came in on, and which way it ran along it.
var inbound = StretchBetween(previousCall.OperationLocation, call.OperationLocation);
var forward = inbound.Start.Equals(previousCall.OperationLocation);
```

Two trains are **opposite** when their senses differ — a crossing — and the **same** when they
match — an overtaking.

**Why senses are comparable across different stretches.** Two trains meeting at a station usually
arrive on *different* stretches, one from each side, so the comparison would be meaningless unless
stretch orientations agree layout-wide. They do, and the model already says so:

> *"All track stretches are expected to be defined in the same direction; any directed cycle in the
> `Start → End` graph breaks that rule."* — `Layout.DirectionInconsistencies()`

So this derivation rests on a consistency the planning UI already validates and surfaces as a
warning. A layout with direction inconsistencies will produce **wrong crossing and overtaking
notes** — not missing ones, wrong ones — which makes that existing check a precondition of this
report rather than a tidiness aid. Worth saying in the validation message.

**The inbound stretch is what resolves the reversal case.** A train may change direction at a
station that permits it (`IsChangingTrainDirectionPossible`), arriving on one stretch and leaving
towards another. Taking the direction *coming into* the station gives one unambiguous answer at
every call, and it is the correct one: what the driver meets is the train that came towards them,
whatever it does afterwards.

One fallback is needed: **a train that originates at the station has no inbound stretch.** Use its
outbound one instead — its direction of travel is still well defined by where it is going.

This is the one derivation the model does not already express, and the only genuinely new logic the
report requires.

**Both notes attach to the arrival** (`IsForArrival`), matching the direction they are derived from:
the driver reads *"who am I meeting here"* on pulling in, not on leaving.

A crossing at a location the driver runs through is a real event — the overlap test is satisfied
whenever the other train is standing there at that instant — and a pass-through renders as a single
row. The general single-row rule in §5.3.6 covers it: both halves' notes appear on that row, so an
arrival note is never lost for want of an arrival row. Nothing special is needed here.

Two further rules, unchanged:

- **Relative to the driver's own train only.** The note names the *other* train, and only trains
  that actually cross or overtake the one being driven produce one. Other traffic at the location
  is not the driver's concern.
- **Restricted to shared sessions.** The other train may run on only some of the sessions this duty
  runs, which is why the example reads *"in sessions 1,3,5"*. The session set is the intersection of
  the two trains' sessions; when it equals the duty's own sessions the qualifier is redundant and
  should be omitted. Sessions render as circles in the note's `Html` (D34), following the
  region-chip precedent.

*One refinement worth considering later, not now:* a same-direction overlap is symmetric, so the
rule produces a note whether the other train gets ahead or the driver's does. If the two cases ever
need different wording — *"overtakes you"* versus *"you overtake"* — the discriminator is which
train departs first. The specified rule prints one note for both.

#### 5.3.7 Height contributions

§6.2's height model charges only calls, notes and instructions. With the blocks now defined, each
needs its own term. A table costs its heading, its header row and its data rows:

| Block | Height |
|---|---|
| Part header | ~3 lines, plus 1 for the limits line when any limit is set (§5.3.2) |
| Each table, when present | heading + header row + rule ≈ 3 units, plus 1 per data row |
| Each table, when empty | 0 — the block is omitted entirely |
| Timetable rows | 2 per stop (Arr + Dep), 1 per pass-through |
| Notes | **1 per note**, less one when the first note is short enough to sit in the call row's note column (≤ 25 chars, D103). Wrapping is charged only at the page width, where it rarely triggers — a note in the column never wraps by construction. A single-row call is charged all its notes (D99) |
| Continuation marker | charged **once per page**, not per part — see §5.3.8 |

The prototype's constant of 6 units of fixed overhead per part is consistent with a header block
plus one table; it needs re-deriving now that up to four tables may appear.

#### 5.3.8 Continuation marker — the last thing on every page

**The failure this prevents is real and already observed:** a participant works the parts they can
see, does not turn the page, and never works the last train of the duty. A missed train is worse
than any formatting problem in this specification, and nothing on a full page currently says whether
anything follows.

So the foot of the page carries one of two statements:

| Condition | Text | Rendering |
|---|---|---|
| More parts of this duty follow | **Duty continues on next page!** | Centred, bold, red |
| This was the duty's last part | *No more trains in this duty.* | Centred, bold, plain |

**Only the warning is red.** Red is the alarm colour and must keep meaning *act on this*; the
terminal message says the opposite — there is nothing more to do — so colouring both would leave the
red meaning nothing. Colour is emphasis only here: the words carry the meaning, so greyscale
printing (D21a) loses nothing.

**Placement is per page, not per part.** This is the one point where the obvious reading has to be
resisted: a marker under *every* non-final part would print *"continues on next page"* under a part
whose successor is three centimetres below it on the same page. That statement is false, and a
notice that is sometimes false is a notice drivers learn to ignore — destroying the one thing it
exists for. So:

> The marker sits under the **last part on a page**, and says which of the two cases applies.

Two refinements follow from the rest of the design:

- **A split part's first page carries no marker** (§6.3.1). The part is not finished there, and its
  timetable is on the facing page, in view without turning — there is nothing to warn about.
- **The terminal message is what makes the blank pages safe.** After the last part come blanks and
  then the overview page (D4). *"No more trains in this duty"* tells the driver those pages hide
  nothing, which is precisely the doubt that makes someone flip back and forth.

**Height is reserved once per page, not per part.** The marker's height cannot be charged to a part,
because whether a part is last on its page is decided *by* the packing that the height feeds into.
Reserving it in the page budget instead — 45 units becomes 45 minus the marker — removes the
circularity entirely and costs only a slightly conservative budget on the rare page that ends
mid-split-part. The error is in the safe direction: a page reserving space it does not use, never
one overflowing.

Both strings need translation keys in all five languages.

### 5.4 Blank page

Carries "This page is intentionally blank" so a driver does not think pages are missing.

### 5.5 The general instructions report — a separate document

Not part of the duty booklet at all, but specified here because it is where the general instructions
went.

Authored standing instructions for the whole meeting: signalling practice, radio or telephone use,
shunting rules, what to do when running late, who to ask. Held as markdown on the `Plan` (gap G5).

**Why it is a separate report:**

- Its audience is **everyone at the meeting**, not only drivers. Station staff need the same
  conventions and never hold a duty booklet.
- It is **identical in every booklet**. Bound in, it would be reprinted once per duty — the same
  pages many times over, and still not reaching the station staff.
- It is read **once, before the first session**, whereas the duty booklet is carried and consulted
  throughout. Different reading occasions, so different documents.

**Same physical format**: A5 portrait, two-up on A4 landscape, saddle-stitch booklet order. The
markdown is of unbounded length, so this report needs pagination and imposition of its own — and
gets them nearly for free, because §6.5's imposition is a pure permutation over a page count (D24)
and knows nothing about duties. The only duty-specific part of §6 is the height estimation of a
train part (§6.2).

**Content is a single flow**, so its pagination is simpler than the duty booklet's: split rendered
markdown at block boundaries to fill pages, rather than packing indivisible items. It has no front
page and no fixed last page, so its only padding rule is the multiple-of-4 one.

Distribution is by hand, before the first session, in whatever quantity the organiser wants — the
one respect in which it differs from D3's "one of each".

---

## 6. Page fitting and pagination

The hard problem. The timetable report solved the analogous problem with `TimetablePaginator`,
which **estimates rather than measures** — it computes how much fits from known row and column
counts instead of asking the browser. That approach is the precedent here, because it is
deterministic, testable without a browser, and works under print preview where measurement is
unreliable.

**The unit of packing is the train part.** Fitting decides *how many whole train parts fit on one
page* — parts are not split. A part is atomic: its four blocks (traction, wagonset, cargo, calls)
belong together and a driver reads them as one unit. This is a simpler problem than the timetable's,
because it is bin-packing of indivisible items rather than splitting a continuous table.

The prototype's `PaginationExtensions` (`App.Contract/Extensions/PaginationExtensions.cs`) has this
working in production. The pipeline below follows it, with the generalisations noted.

### 6.1 The pipeline

```
duty ──▶ [1] estimate part heights ──▶ [2] pack into pages ──▶ [3] pad with blanks
                                                                      │
     printed sheets ◀── [5] impose onto A4L ◀── [4] append overview page
```

Steps 1–4 produce **logical pages** numbered 1…N as the driver reads them. Step 5 reorders them
into the physical sequence the printer needs. Keeping these separate is what makes the whole thing
testable: steps 1–4 are pure list-building, and step 5 is a pure permutation.

Steps 3 and 5 are **not duty-specific**: padding to a multiple of 4 and saddle-stitch imposition
depend only on a page count. The general instructions report (§5.5) reuses both, replacing steps 1,
2 and 4 with a single markdown flow-split.

### 6.2 Height estimation

Heights are abstract row units, not millimetres — estimated, never measured, matching the
`TimetablePaginator` precedent. The prototype's proven model for a driver duty part:

```csharp
// Prototype: 6 units of fixed overhead, plus per-call cost.
6 + calls.Sum(c => c.CallTimesCount() * 1.2                      // 1 or 2 time rows
                 + c.ArrivalAndDepartureNotesHeight() * 1.3      // notes, incl. wrapping
                 + train.Instruction.Length / 50)                // instruction text wrap
```

with a **page body of 45 units** and note wrapping charged at 40 characters per line. The constants
are empirical — they encode what actually fitted on an A5 page in the prototype's typography and
must be re-tuned if the type size or page margin changes.

Two things to carry across deliberately:

- **Text length drives height.** Long notes and instructions wrap, and the estimate accounts for it
  by charging per N characters. Any block added in §5.3 that can contain free text needs the same
  treatment. **Much less of this remains than in the prototype**: with one note per row and a full
  page width from the second note on (§5.3.6), notes are counted rather than measured, and wrapping
  is the exception instead of the rule. This is the largest single improvement in the reliability of
  the estimate, and it comes from a layout choice rather than from better arithmetic.
- **The four blocks of §5.3 are not yet in this model.** The prototype charges calls, notes and
  instructions. Traction, wagonset and cargo blocks each need their own term before the estimate is
  trustworthy for this report.

### 6.3 Packing

Greedy, in working order — place the first part, then keep adding while the next one still fits:

```csharp
pages.Add(Page(parts[i]));                       // first part goes on unconditionally
while (next part exists && height + next.Height() <= 45) { … add it … }
```

**Note the unconditional first placement.** The prototype never checks that a part fits a page *on
its own*, so an oversized part silently overflows the page rather than failing. This is exactly the
Q13 case, and it explains why it has never been observed: nothing reports it.

### 6.3.1 A part that does not fit one page — the spread rule

A part is no longer strictly indivisible (D5). When all four blocks will not fit one page, the
**timetable block moves to the next page**, and the part occupies two pages:

| Page | Blocks |
|---|---|
| First | Header, traction, wagonsets, cargo |
| Second | Train timetable |

The timetable is the right thing to move: it is already last (D29), it is the only block whose
height is unbounded by anything but the route, and the split falls on a boundary the reader
recognises rather than in the middle of a table.

**The hard constraint: the two pages must be visible together, without turning a page.** A driver
consulting the timetable must be able to see the tables it points at. That makes the pair a
**spread** — the two facing pages of the open booklet — and in a saddle-stitch booklet the spreads
are exactly:

```
(2,3)  (4,5)  (6,7)  …          i.e. (even, even+1)
```

Page 1 is the front cover and faces nothing. So:

> **A split part must begin on an even page.**

This is a genuinely new obligation on the packer, which until now never needed to know a page's
parity. The rule:

1. Estimate the part's height. If it fits the remaining space, place it as now.
2. If it does not fit a whole page on its own, it will be split. Check the parity of the page it
   would start on.
3. **Odd page → move the part to the next page.** Whatever parts already sit on the current page
   stay there and the page simply ends early. If the current page was empty, it becomes a blank
   page (§5.4) — the one case where a blank appears mid-booklet rather than before the overview.
4. If even the split halves do not each fit a page, the part genuinely overflows: print it anyway
   and raise a validation message (D26).

**A consequence for note wording.** §5.3.6's pointer notes read *"see above"*, which is false on a
split part — the tables are on the facing page, not above. The wording must not assume a vertical
relationship: *"see the tables for this train"* is true in both layouts, and a note that has to be
rewritten depending on pagination is a note that will eventually be wrong.

**The spread is not merely tolerable — it is better than a cramped single page.** With the tables on
the left and the timetable on the right, both are in view at once, which is exactly the relationship
the pointer notes describe. The rule exists to guarantee that, not to apologise for the split.

### 6.4 Blank padding

Blanks are inserted so that the total page count is a multiple of 4 **and** the overview page falls
last. The prototype passes the number of forthcoming trailing pages into the padding calculation so
the blanks land before them, not after — the right structure, but implemented as a hardcoded ladder
that stops at 16 pages:

```csharp
// Prototype — breaks above 16 pages.
var totalPages = afterPageNumber <= 4 - upto ? 4 - upto : … : 16 - upto;
```

Generalised, with `content` = front + part pages + any mid-booklet blank forced by the spread rule
(§6.3.1) + overview page — a spread blank is part of the sequence being padded, not part of the
padding:

```csharp
var blanks = (4 - content % 4) % 4;
```

The prototype also guards the invariant with `Debugger.Break()`, which does nothing in a release
build. It should be a real assertion or validation message.

**Worst case is three blank pages** — a duty whose front, parts and overview page total five. Three
"intentionally blank" pages in a row is the cost of the fold; acceptable, but worth knowing.

**Minimum booklet is four pages**: front, one part page, one blank, overview.

### 6.5 Imposition — booklet page order

Each printed A4 landscape sheet carries **two A5 portrait pages side by side**, and both sides are
printed. A5 portrait is 148 × 210 mm, so two of them are 296 × 210 mm against A4 landscape's
297 × 210 mm — an exact fit.

The prototype hardcodes the page order for the four sizes it supports:

| N | Booklet order |
|---|---|
| 4 | 4, 1, 2, 3 |
| 8 | 8, 1, 2, 7, 6, 3, 4, 5 |
| 12 | 12, 1, 2, 11, 10, 3, 4, 9, 8, 5, 6, 7 |
| 16 | 16, 1, 2, 15, 14, 3, 4, 13, 12, 5, 6, 11, 10, 7, 8, 9 |

and throws `ArgumentOutOfRangeException` for anything else — the limitation to remove.

**The general rule.** Read in pairs, the table is a sequence of sheet sides. For sheet `i`
(0-based) of a booklet of `N` pages:

| Side | Left half | Right half |
|---|---|---|
| Front | `N - 2i` | `1 + 2i` |
| Back | `2 + 2i` | `N - 1 - 2i` |

```csharp
static IEnumerable<int> BookletPageOrder(int pageCount)
{
    for (var i = 0; i < pageCount / 4; i++)
    {
        yield return pageCount - 2 * i;      // front left
        yield return 1 + 2 * i;              // front right
        yield return 2 + 2 * i;              // back left
        yield return pageCount - 1 - 2 * i;  // back right
    }
}
```

This reproduces all four of the prototype's tables exactly — verified term by term against each —
and extends to any multiple of 4, which is what the hardcoded version could not do.

**Why it works.** The sheets nest inside one another (saddle stitch): the outermost sheet carries
the first and last pages, and each sheet inwards moves one page in from each end. Hence `1 + 2i`
walking forwards and `N - 2i` walking backwards.

**Duplex flip must be on the short edge.** The fold runs vertically down the middle of the A4L
sheet, so the back must be the mirror of the front about that vertical axis — a book-style flip.
Checking the 4-page case: the front is `[4 | 1]`, so flipping about the vertical axis brings page 1
to the left, and the reverse of page 1 is page 2, giving `[2 | 3]` on the back. That matches the
prototype's order. A long-edge flip would print every back side upside down relative to its front.
This belongs in the printing instructions shown to the user, since it is a print-dialogue setting
the report cannot control — and specifically the **PDF viewer's** dialogue at step 3 of §8.0, not
the browser's (D63).

### 6.6 Consequences for the components

The report renders **A4 landscape sheets**, each containing two A5 page bodies — not a sequence of
A5 pages. `A5PPage` remains the right component for a page *body*, but a sheet wrapper is needed to
place two of them side by side inside an `A4LPage`.

This means the `a5p` named page (and the `@page a5p` rule fixed in G4) is *not* what this report
prints through; it applies only if a single A5 page is ever printed on A5 paper. See Q23.

Verification target is Chrome and Edge; Firefox print behaviour is known to be unreliable in this
codebase, and named-page `size` is only honoured in Chromium.

---

## 7. Data extraction

The report must not compute domain facts. Anything a driver reads should either be stored in the
model or be an existing model-side derivation (`OrderedParts`, `StartTime`, `EndTime`,
`TractionExchangeNotes`, `SessionCombinations`).

The report layer contributes flat, print-shaped view models — the same pattern as `TurnusData` for
turnus cards. Stubs exist: `DriverDutyPart`, `TrainPartTractionData`, `TrainPartWagonsetData`.

### 7.1 Where each block's fields come from

Everything below is settled except the cargo block, which is the one place where implementation
would still be invention rather than transcription.

**Front page** — §5.1: all sources are named in the element table there. The four gaps are model
work, not extraction: G1 validity dates, G2 difficulty, G8 company logo, G9 staff count.

**Traction (§5.3.3) and wagonsets (§5.3.4)** — `Plan.ScheduledObjectsFor(trainPart)` filtered by
`IsTraction` / `IsWagonSet`; identity is `ScheduledObject.Designation` (D56); times and stations from
the `ScheduledTrainPart`'s `From` / `To` calls; sessions from the scheduled object.

**Timetable (§5.3.6)** — the part's calls in order; notes from `ICallNote` filtered by
`IsDriverNote` (D28). Two of the note types are derived from `IsStop` and the times (D36) and are
trivial. **The crossing and overtaking notes are not** — see Q36.

**Cargo (§5.3.5)** — one row per `CargoFlowTrainPart`, five columns, all sources fixed in that
section. The block matters more than its size suggests: it is the conductor's working document
(D90).

Nothing in §5 is now unsourced. The only derivation the model does not already express is the
**direction of travel** behind crossings and overtakings (§5.3.6.1).

---

## 8. Settings, translation and printing

### 8.0 The printing workflow is two steps, not one

Reports are not printed straight to paper. The established practice is:

1. **Print to PDF** from the browser.
2. **Proof-read and check the appearance** in the PDF — content, pagination, page fitting.
3. **Print the PDF on paper** once it looks right.

This is the real target, and several things follow from it.

**The PDF is the artefact, not the browser output.** Verification means opening a PDF, not trusting
print preview. §8.1 already flags that Chrome's preview renders backgrounds the print job may drop;
the PDF is the first place that is visible. Anything this specification says "must be verified on
paper" can in fact be verified one step earlier, in the PDF — with the exception of how the printer
converts colour to grey, which only paper shows.

**The proof-reading step is a real safety net, but not the design's.** A page that overflows (D26)
or a booklet whose page count is not a multiple of 4 will be seen by a human before any driver holds
it. That is worth knowing — it makes those failures embarrassing rather than damaging — but it does
not soften the requirements. The failures should not reach the PDF either, because the person
proof-reading is checking content, not recomputing pagination.

**The PDF is the reprint source.** §2.1's two reprint cases — a correction found, or a booklet not
returned — are better served by keeping the meeting's PDFs than by re-rendering. Re-rendering after
the plan has been edited produces a booklet that may differ from every other booklet in the pile, in
ways nobody asked for and nobody notices. Printing page 12 out of the saved PDF is guaranteed to
match. Only the *correction* case wants a fresh render, and that one deliberately supersedes the old
version — which is what the print timestamp (element 12) exists to make visible.

**The timestamp is a render time, not a paper time.** It is fixed when the PDF is produced and does
not change when that PDF is printed a week later. That is the behaviour wanted: it identifies the
*version*, and two booklets of the same duty are told apart by which render they came from. Nothing
in the report may depend on when paper came out of the printer.

**Duplex and greyscale are set at step 3.** The short-edge flip (§6.5) and the colour/greyscale
choice (§8.1) are settings on the *PDF* print dialogue, not the browser's. The imposition order is
baked into the PDF page sequence, so the PDF is correct regardless — but the printing instructions
shown to the user must be written for the second step, where those settings actually apply.

**One gotcha to verify.** Chromium's *Save as PDF* destination has its own paper-size control, which
can conflict with a named `@page size`. Since the whole report depends on A4 landscape sheets
(§6.6), the first thing to confirm is that a saved PDF really is A4 landscape and not letter or
whatever the dialogue defaulted to.

### 8.0.1 Selecting which duties to print

Two mechanisms, because the two reprint cases of §2.1 are reached differently.

**1. Render everything, select in the print dialogue.** The default and the established practice:
open the report with every duty rendered, then choose a page range when printing to PDF. Nothing in
the report is needed for this beyond rendering all duties in a predictable order (D65).

It does rest on one property, which is worth stating because it is easy to break later:

> **A duty's booklet never shares a sheet with another duty's.** Each booklet is padded to a
> multiple of four A5 pages (§6.4), and four A5 pages are exactly one A4 landscape sheet, so every
> booklet occupies a whole number of sheets.

Without that, a page range could not isolate one duty at all — the last sheet of duty 5 would carry
the first pages of duty 6. The padding rule already guarantees it; the point is that it is now
*load-bearing* for selection, not only for the fold.

Since the print dialogue counts A4 landscape sides rather than duties, the on-screen toolbar should
show a **sheet index** — *"Duty 1: pages 1–2, Duty 3: pages 3–4, …"* — so the range can be read off
rather than counted. The toolbar is `.no-print`, so this costs nothing on paper.

**2. A `duties` query string** — `/driver-duties-report?duties=1,2,3,7` — filtering the render
itself. This is what serves a targeted reprint without scrolling a whole meeting's worth of pages,
and it is a link that can be kept.

```csharp
[SupplyParameterFromQuery(Name = "duties")]
public string? DutyIdentities { get; set; }
```

Three details:

- **The values are identities, matched as strings** — not indices, and not integers. `Identity` is a
  `string` and may be non-numeric once a duty is pinned (G7), so `?duties=1,L1,7` must work.
- **Absent or empty means all**, so the plain route keeps working unchanged.
- **An identity that matches nothing is reported, not ignored** — a `.no-print` warning naming the
  unmatched values. A typo would otherwise silently print a smaller set than asked for, and the
  whole point of the parameter is a reprint the user believes is complete.

Selection changes *which* duties render, never their order: the output stays in duty-number order
(D65) whichever mechanism is used.

### 8.1 Colour and greyscale printing

**Greyscale is left to the browser and printer driver. The report has no monochrome mode.**

This is safe because of what colour is *for* here: **no colour carries meaning on its own.** Colour
emphasises, so that the eye finds things faster — `TrainCategory.Color`, `Region.BackgroundColor`,
the difficulty grade — but every one of them sits next to the text that states the same fact. A
category shows its name, a region its name, a difficulty its number. Convert all of it to grey and
nothing is lost but speed of scanning.

That principle is what makes the decision cheap. Were any of them colour-only, a greyscale variant
would be forced, because the browser cannot detect a driver-level greyscale choice — `@media
(monochrome)` describes the display, not the print target — so the application would have to model
it explicitly. Keeping colour decorative avoids all of that.

**The one rule that is still needed: `print-color-adjust: exact` on background colours.**

Browsers drop *background* colours when printing, whatever the colour/greyscale choice. Foreground
text and SVG `fill` print normally (D8), so most of the report is unaffected. Region chips are not:

```csharp
// Model/Layouts/Region.cs — the text colour is chosen to contrast with the background.
<span class="region" style="background-color: {region.BackgroundColor}; color: {region.BackgroundColor.TextColor}">{region.Name}</span>
```

When the background is dropped but the contrast-computed text colour is kept, a light-on-dark chip
becomes **light text on white paper — invisible**. Region names appear in cargo notes through
`CargoFlowDestinationNote`, so this reaches the driver's booklet directly.

*Fixed 2026-07-27* in `Planning.App/wwwroot/css/app.css`, on the shared `.region` rule so it applies
everywhere the chip is used, not only in this report:

```css
.region {
    …
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
}
```

This forces the background to be *printed*; whether it prints as colour or grey remains the driver's
choice, which is precisely the behaviour D21a wants. Both spellings are set, as Chromium supported
the prefixed name long before the standard one.

**Still to verify in the PDF and then on paper**, not in print preview — Chrome's preview sometimes
renders backgrounds that the print job drops:

- that region chips now print at all;
- whether greyscale conversion leaves adjacent category colours distinguishable. If two converge on
  the same grey nothing breaks, since the names still distinguish them (D21), but the emphasis is
  lost.

- Sessions display uses `SessionsSettings` (start day, day names, use-days).
- All labels go through `Translator`; new keys must be added in all supported languages.
- Page footers carry page number and an optional footnote, via `PageFormat`.
- The report page uses `PrintLayout` and must call `ScheduleState.InitializeAsync()` so it works
  when opened directly.

---

## 9. Current state of the code

*Implemented 2026-07-29. Both reports are built and the whole solution's tests pass; what remains is
tuning against a real print (§12.3).*

| File | State |
|---|---|
| `Planning.App/Reports/DriverDutiesReport.razor` | Built: duty-number order, `?duties=` filter, sheet index |
| `Planning.App/Reports/GeneralInstructionsReport.razor` | Built: markdown booklet plus topology and shunting yards (§5.5) |
| `Reporting/BookletImposition.cs` | Padding and imposition, duty-independent and shared by both reports (D53) |
| `Reporting/Duties/DutyPage.cs` | Page model, `Instructions` renamed to `Overview`; split-part flags added |
| `Reporting/Duties/DutyPagination.cs` | Height estimation, greedy packing, the spread rule |
| `Reporting/Duties/DriverDutyPart.cs` | The four blocks projected from the model |
| `Reporting/Duties/TimetableRow.cs` | Row structure and note stacking (§5.3.6) |
| `Reporting/Duties/DutyBookletPage.razor` | Dispatches on page kind; carries the continuation marker |
| `Reporting/Duties/TrainPartVehicleView.razor` | Traction and wagonsets — one view, since the blocks differ only in what they list |
| `Reporting/Duties/TrainPartCargoWagonsWithWaybillsView.razor` | The cargo block (§5.3.5) |
| `Reporting/Duties/TrainPartView.razor` | The train timetable |
| `Reporting/Duties/FrontDutyPage.razor` | The front page (§5.1) |
| `Reporting/Duties/DutyOverviewPage.razor` | Topology and shunting yards (§5.2) |
| `Reporting/Instructions/InstructionsPagination.cs` | The markdown flow-split |
| `Shared/SessionsView.razor`, `CompanyLogo.razor`, `TopologyDiagramView.razor` | Shared components (D46) |

Superseded and removed: `TrainPartTractionView.razor` and `TrainPartWagonsetView.razor` (one view now
serves both), `TrainPartTractionData.cs` and `TrainPartWagonsetData.cs`, `DutyTrainPartsPage.razor`,
`BlankDutyPage.razor`, `DutyInformationPage.razor`, and the unimplemented
`Scheduling/Extensions/TrainPartExtensions.cs`, whose `TractionData` stub could not carry the session
settings the block needs. `TopologyDiagram.cs` and `SvgText.cs` moved from `Planning.App` into
`Planning.Components.Shared` so the report can reach them.

---

## 9a. Model gaps

Seven gaps, all planning data the user must be able to set and correct, so none may be invented by
the report (§7):

| Gap | Addition | Where |
|---|---|---|
| G1 | `ValidFrom` / `ValidTo` | `GeneralSettings` |
| G2 | `DutyDifficulty` enum + property | `DriverDuty` |
| G3 | `Sessions.ToText` + `ToHtml`, replacing the `Display` stub already called in production | `Sessions` |
| G4 | *(fixed)* `@page a5p` declaration | `print.css` |
| G5 | General instructions markdown | `Plan` |
| G6 | `CargoServedFrom` | `OperationLocation` |
| G7 | `IsExcludedFromRenumbering` | `DriverDuty` |
| G8 | `Logo` — an uploaded image or SVG | `Company` |
| G9 | `StaffCount` — how many people work the duty, 1–3 | `DriverDuty` |

**G1 — Validity dates.** The front page shows *"Gäller 2026-03-06 – 2026-03-08"*, the date span of
the meeting. Neither `Plan`, `Layout` nor `LayoutSettings` carries any date.

*Resolved:* the dates live in **`GeneralSettings`** (`Layout.Settings.General`) and are edited on
the **General settings tab**, alongside the other layout-wide operating properties — `StartDay`,
`MaxSessions`, `StartTime`, `EndTime`. Two nullable `DateOnly?` properties, so a layout that has no
meeting booked simply omits the line from the front page rather than printing a placeholder date.

```csharp
/// <summary>First day of the meeting this layout is planned for. Shown on printed reports.</summary>
public DateOnly? ValidFrom { get; set; }

/// <summary>Last day of the meeting this layout is planned for. Shown on printed reports.</summary>
public DateOnly? ValidTo { get; set; }
```

Names are fixed once chosen: renaming a settings property silently resets the saved value, because
the plan persists settings by property name.

**G2 — Duty difficulty.** The front page shows *"Svårighet: 1"*, colour-coded. No such concept
exists anywhere in the code.

*Resolved:* a three-grade scale, set by hand on the duty, telling a driver what kind of work to
expect before they take the booklet:

| Grade | Name | Colour | Meaning |
|---|---|---|---|
| 1 | Easy | Green | No extra moments — just drive. |
| 2 | Medium | Orange | Some extra moments. |
| 3 | Experienced | Red | Often involves shunting, handling wagon cards with waybills, sorting wagons. |

Modelled as an **enum on `DriverDuty`**, not an `int`, since the grades are named concepts with
fixed meanings rather than an open numeric range:

```csharp
public enum DutyDifficulty { Easy = 1, Medium = 2, Experienced = 3 }
```

Needs a property on `DriverDuty`, editing in the Duties tab, translation of the three names, and a
decision on the default for duties that have never been graded.

**Colour must not be the only carrier of the grade — see Q20.** Booklets are frequently printed in
black and white, and green, orange and red all reduce to similar mid-greys. The number survives
that, which is why the example shows *"Svårighet: 1"* rather than a bare colour swatch. Printing
the grade *name* next to the number would survive it better still. Note that text colour prints
reliably even when backgrounds do not (D8), so a coloured numeral is safe where a coloured
background would not be.

**G3 — `Sessions.Display(SessionsSettings)` is not implemented.** It is a stub returning an empty
string:

```csharp
// Model/Timetables/Sessions.cs:98
public string Display(SessionsSettings settings) =>
    settings.UseDaysInsteadOfSessionNumbers ? "" : ""; // TODO: implement …
```

This is not merely a missing feature — `TrainPartTractionView.razor` already calls it, so the
traction table currently renders a blank sessions column. The building blocks all exist
(`SessionsNumbers` for the collapsed numeric text, `FullDayNamesResourceKey(startDay)` for day
names, `UseShortWeekdayNames`, `CappedForDisplay`); what was missing was a decision about what it
returns.

**`Display` is replaced by a pair**, each taking the same `SessionsSettings` and each choosing
between sessions and days from it:

```csharp
/// <summary>Plain-text form: "All sessions", "1-3", "1,3,5", "Monday to Wednesday".</summary>
public string ToText(SessionsSettings settings) => …

/// <summary>Markup form: the same content rendered as session circles (§5.1.1) or day text.</summary>
public MarkupString ToHtml(SessionsSettings settings) => …
```

**Both are needed, and neither substitutes for the other:**

| Member | Used by |
|---|---|
| `ToText` | Plain-text note bodies (`ICallNote.ToText`), validation messages, tooltips and `title` attributes, exports, and **height estimation** — D35 already charges wrapping against `ToText` for exactly this reason: markup would inflate the character count and charge a short value as a long one. |
| `ToHtml` | The report blocks and the GUI, wherever the session circles are drawn — and, critically, wherever sessions must appear *inside* another `MarkupString`, such as a session-qualified note (D34). |

**They must share one core.** The four display forms of §5.1.1 — all-sessions, contiguous run,
individual, plus the additive on-demand marker — are a property of the *value*, not of the
rendering. If each member decides the form independently they will eventually disagree, and a
tooltip will say something different from the circles beside it. So the run-splitting and
form-selection logic (already present privately as `FormatSessions`) is computed once, and `ToText`
and `ToHtml` are two renderers over the same result.

**This also settles how the shared component (D46) relates to the model.** A Blazor component cannot
be embedded inside a `MarkupString`, so `ToHtml` cannot be replaced by one; equally, building markup
by hand at every Razor call site would be worse than a component. Both exist, with **`ToHtml` as the
single source of the markup** and the component as a thin wrapper that emits it. That way the
circles are drawn by one piece of code, whether they are rendered directly or embedded in a note.
The model already returns `MarkupString` this way — `Region.ToHtml`, `ICallNote.ToHtml` — so
this is the established shape rather than a new one.

*Naming note:* the codebase once spelled this `ToHtmlMarkup` on `Region` and `Destination`, and
`Html` / `Text` on `ICallNote`. Both were unified on `ToText` / `ToHtml` (D106, D110), so one
spelling now holds wherever a model object renders itself.

**G4 — The A5 page size was not declared, and A4 portrait was broken.** *Fixed 2026-07-27.* Found
while checking the print CSS. `Planning.App/wwwroot/css/print.css` declared `@page a4p` twice, the
second time with `size: A5 portrait`, where the third rule should have named `a5p`. Two
consequences, both now resolved:

1. **`a5p` was never declared.** `A5PPage.razor` renders `class="a5p"` and `.a5p { page: a5p; }`
   referenced a named page that did not exist, so A5 pages fell back to the default paper size.
   This report is built on `A5PPage`, so it was directly affected.
2. **`a4p` meant A5.** The duplicate declaration won by source order, so the existing A4 portrait
   reports printed at A5 size.

Still worth a test print of an A4 portrait report and an A5 one before page-fitting work begins:
every height estimate in §6 depends on the page really being the size it claims, and named-page
`size` is only honoured in Chromium.

**G5 — Plan-level general instructions.** The authored markdown behind the general instructions
report (§5.5) has no home; `Plan` carries only `Name`, `Timetable`, `ScheduledObjects`, `Schedules`
and `DriverDuties`. Needs a markdown property on `Plan`, plus editing in the GUI.

The property no longer feeds the duty booklet at all — it is the whole content of the separate
report. Nothing else about the gap changes: same owner, same editing surface.

**Editing goes in a new Settings ▸ Information sub-tab.** The sub-tab pattern already exists —
`StretchesTab.razor` uses a private enum, a row of `<button class="sub-tab">`, and a `switch` on the
selected section, which is how Topology sits under Stretches. Settings follows the same shape. Note
`TabRegistry.cs` registers only top-level tabs, so a sub-tab needs no registry entry.

**G6 — Shunting yards and what they cover.** **One new property, and no flag.**

| Addition | Where |
|---|---|
| **"Cargo served from"** (`CargoServedFrom`) — a nullable reference to the `Station` that serves this location with cargo | `OperationLocation`, settable only where `HasCargoExchange` is true |

**The name says *cargo* deliberately.** A bare "served from" would read as though it covered every
way one location is worked from another — passenger connections, banking assistance, whatever else a
reader imagines. This relation is only about local freight: which shunting yard the wagons for this
location are worked from. Naming it `CargoServedFrom` matches the `HasCargoExchange` gate that
governs it and puts the scope in the name, where it cannot be lost.

**A shunting yard is derived, not declared.** A station *is* a shunting yard when it serves
something — no `IsShuntingYard` bool, nothing to keep in step with reality, and no way for a station
to be marked a shunting yard while serving nothing.

**"Serves something" means either of two things** (D68): it has locations served from it, **or** it
carries regions. The locations column is the inverse of the `CargoServedFrom` relation; the regions
column is `Station.Regions` directly.

```csharp
// Shunting yards with what they cover. A station qualifies on either relation, so a shadow shunting
// yard that serves no on-layout location but stands for several regions is still listed.
var servedByShuntingYard = layout.OperationLocations
    .Where(l => l.CargoServedFrom is not null)
    .GroupBy(l => l.CargoServedFrom!)
    .ToDictionary(g => g.Key, g => g.ToList());

var shuntingYards = layout.OperationLocations.OfType<Station>()
    .Where(s => servedByShuntingYard.ContainsKey(s) || s.Regions.Any())
    .Select(s => new
    {
        ShuntingYard = s,
        Locations = servedByShuntingYard.TryGetValue(s, out var served) ? served : [],
        Regions = s.Regions,
    });
```

Only `Station` carries `Regions`, which costs nothing here: `CargoServedFrom` already targets a
`Station`, so every shunting yard is one either way.

**The relation is asymmetric, and the two ends have different rules:**

| End | Rule |
|---|---|
| **Served location** (the source) | Must have `HasCargoExchange` — a location exchanging no cargo has nothing to be delivered. And must **not** be a shadow station: `Station.IsShadow` means off-layout staging, which is where traffic comes *from*, never somewhere local freight is delivered *to*. |
| **Serving station** (the target) | **Any** `Station`, shadow ones included. A shadow shunting yard is a perfectly ordinary origin for local freight. |

So `CargoServedFrom` is offered on any `OperationLocation` with `HasCargoExchange` except shadow
stations, and its picker lists every station without exclusion. Both stations and industrial areas
can be served, since both derive from `OperationLocation`.

One consequence worth noting: a station that neither serves a location nor carries a region does not
appear as a shunting yard. That is correct — an entry with nothing under it says nothing — but it
does mean "shunting yard" is defined by what a station covers, not by `Station.IsShadow`. A shadow
station is the *typical* region-carrying station, not the definition of one.

**G7 — `DriverDuty.IsExcludedFromRenumbering`.** Some duties must keep a **fixed number** across
renumbering — a duty participants know by number from previous meetings, or one whose number carries
a meaning of its own. Today `RenumberDriverDuties()` overwrites every `Identity` with an ordinal, so
there is no way to hold one.

```csharp
/// <summary>
/// When true, <see cref="Plan.RenumberDriverDuties"/> leaves this duty's <see cref="Identity"/>
/// untouched and reserves it, so no renumbered duty is given the same number.
/// </summary>
public bool IsExcludedFromRenumbering { get; set; }
```

**Renumbering must skip the reserved numbers, not just the reserved duties.** Excluding a duty from
renumbering is only half the job: if duty *Loco 7* is pinned to "7", the ordinal walk would hand "7"
to somebody else and produce two booklets numbered 7 — worse than not renumbering at all, because
the pile is sorted by that number.

```csharp
public void RenumberDriverDuties()
{
    plan = plan.ValueOrException(nameof(plan));

    // Numbers held by the excluded duties; the ordinal walk must step over them.
    var reserved = plan.DriverDuties
        .Where(d => d.IsExcludedFromRenumbering)
        .Select(d => d.Identity)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    var ordered = plan.DriverDuties
        .Where(d => !d.IsExcludedFromRenumbering)
        .OrderBy(d => d.StartTime is null)
        .ThenBy(d => d.StartTime)
        .ThenBy(d => d.EndTime)
        .ThenBy(d => d.Sessions.FirstNumber)
        .ToList();

    var number = 1;
    foreach (var duty in ordered)
    {
        while (reserved.Contains(number.ToString(CultureInfo.InvariantCulture))) number++;
        duty.Identity = number++.ToString(CultureInfo.InvariantCulture);
    }
}
```

Reserved numbers are compared as **strings**, because a pinned identity need not be numeric — "L1"
or "Post" reserve nothing and simply never collide, which is the correct behaviour.

**Two things renumbering cannot fix, so validation must report them:**

| Condition | Why it matters |
|---|---|
| Two excluded duties sharing an `Identity` | No renumbering can separate them; the pile gets two booklets with the same number. |
| An excluded duty with an empty `Identity` | It is pinned to nothing, and its front page has no number for the sorter or the chooser to read. |

**Editing** is a checkbox per duty in the Duties tab, beside the identity field so the pairing is
obvious, with the identity editable only when it is ticked. House convention puts a question mark on
checkbox labels — *"Fixed number?"* reads better to a planner than *"Excluded from renumbering?"*,
which names the mechanism rather than the intent.

**This changes the printing order — see D65.** Until now, renumbering made number order and running
order the same thing, so D43's "print in running order" happened to produce a number-ordered stack
for free. A pinned number breaks that coincidence: a duty numbered 7 that starts last would be
printed last and then have to be moved by hand into the pile at position 7 — against success
criterion 4, which asks for a stack that can be folded and handed out without sorting.

So duties print in **duty-number order**, which is the order the pile is actually kept in:

```csharp
// Numeric identities first, in numeric order; anything non-numeric after them, alphabetically.
plan.DriverDuties
    .OrderBy(d => int.TryParse(d.Identity, out _) ? 0 : 1)
    .ThenBy(d => int.TryParse(d.Identity, out var n) ? n : 0)
    .ThenBy(d => d.Identity, StringComparer.CurrentCulture)
```

The numeric parse is what D43 was avoiding by sorting on time keys instead — with `Identity` sorted
as a string, "10" comes before "2". Sorting on the parsed number solves that directly, and the
fallback keeps non-numeric pinned identities in a stable, predictable place rather than scattered
among the numbers.

**G8 — `Company.Logo`.** A company may carry an uploaded logo, shown on the front page in place of
its signature (element 9). Nothing in the model holds one today.

```csharp
/// <summary>
/// The company's logo as a complete data URI (for example <c>data:image/svg+xml;base64,…</c>),
/// or <c>null</c> when none has been uploaded. Stored inline so the plan stays self-contained.
/// </summary>
public string? Logo { get; set; }
```

**Stored inline as a data URI, not as a file path.** The plan travels as a single JSON file between
machines and into browser storage; a path would resolve on the machine that set it and nowhere else.
Inline means the logo survives "Save as", reopening on another computer, and the whole
export/import round trip, with no second artefact to keep alongside the plan.

**Raster and SVG are handled identically — both go in an `<img>`:**

```razor
<img class="company-logo" src="@company.Logo" alt="@company.Signature" />
```

Two reasons this beats inlining the SVG:

- **Uploaded SVG is untrusted content.** An SVG can carry `<script>`, and inlined into the page it
  would run in the application's own origin. Inside an `<img>` a browser treats SVG as a static
  image: no script, no external fetches. Since the whole point is that users upload these files,
  this is the difference that matters, and it costs nothing a logo needs.
- **One code path.** PNG, JPEG and SVG render through the same element, so the component does not
  branch on the file type it was given.

The `alt` is the **signature**, so a logo that fails to load degrades to exactly the text it
replaced rather than to a broken-image icon.

**Sizing is height-normalised in `em`** — fixed height, automatic width — following D11's reasoning:
logos of wildly different aspect ratios then line up on a common baseline, several sit side by side
without one dwarfing the rest, and the whole thing scales with the surrounding font size instead of
needing a size parameter at each call site.

**Upload rules:**

| Rule | Value | Why |
|---|---|---|
| Accepted types | `.svg`, `.png`, `.jpg`, `.webp` | Covers what people actually have. |
| Size cap | ~64 KB encoded | The logo rides inside every plan save and load; a few unbounded images would bloat all of them. Rejected with a message, not silently truncated. |
| Preferred format | SVG | It prints at the printer's resolution rather than the screen's. |
| Raster minimum | ~300 px on the long side | A logo ~10 mm tall on paper at 300 dpi needs roughly 120 px; 300 leaves margin. A screen-sized logo looks visibly poor in print, and print is the only output that matters here. |
| Removable | Yes — clearing sets `Logo` to `null` | A logo added by mistake must not be permanent. |

**Editing** goes in the existing company edit form in `CompaniesTab.razor`, using the `InputFile`
pattern already established in `ImportTab.razor`: read the stream, base64-encode, prefix the MIME
type, assign. The list grid should show the logo in its own column, since "which companies have
one" is exactly the question the all-or-nothing rule (D66) makes the planner ask.

**Rendering is a shared component** — `CompanyLogo` in `Planning.Components.Shared`, beside the
session indicator (D46). The front page is the first place it is used, not the only one intended.

**G9 — `DriverDuty.StaffCount`.** A duty is worked by a **loco driver** and, when the work demands
it, a **conductor**: the person who manages the wagon cards — which wagons to bring, which to set
off, and in what order they sit in the train. On a simple duty the loco driver does both jobs. Some
freight duties carry enough of that work to need a second person. Nothing in the model records
which.

```csharp
/// <summary>
/// How many people are needed to work this duty. One by default; the editor allows up to three.
/// </summary>
public int StaffCount { get; set; } = 1;
```

**The property initialiser is what makes existing plans safe.** A plan saved before this property
existed has no `staffCount` in its JSON, so deserialisation leaves the initialised value alone and
the duty reads as 1 — the truth for almost every duty. Neither `DriverDuty` constructor assigns the
property, so nothing resets it back to `0`. Without the initialiser every existing duty would load
as *zero* people, which is both wrong and silently so.

**Range 1–3, enforced in the editor** as a small select rather than a free number field — three
options need no validation message and no way to type 7. The cap reflects practice: beyond three
people it is not one duty. Validation in the model should still reject values outside the range, so
a hand-edited plan file cannot smuggle one in.

**Editing** goes in the Duties tab, beside difficulty (G2) — both describe what kind of commitment
the duty is, and both are set by the planner at the same moment.

**Why the name is `StaffCount` and not a driver count.** The codebase otherwise avoids "staff" for
the people who drive, using **loco driver**, because a layout is staffed by more than drivers. This
property is the case that word exists for: it counts a driver *and a conductor*, two different jobs.
A "driver count" of 2 would state something false — the duty does not need two drivers, it needs one
driver and someone to work the cards.

**The conductor is a second reader of the booklet, and the cargo block is their document.** This
matters more than a count usually would. §5.3.5's cargo wagons block — position in the rake, where
the wagons come from, where they go — is precisely the conductor's work, and it is the one block a
non-driving reader uses on its own. It is a further argument for D41: had that information stayed
buried in the timetable's note column as it was in the prototype, the second person would have had
nothing they could work from without taking the whole booklet from the driver.

**Staffing and difficulty (G2) are related but independent.** Grade 3 is described as *"often
involves shunting, handling wagon cards with waybills, sorting wagons"* — conductor work, so hard
duties are the candidates for two people. They stay separate fields because an experienced
participant may well work a grade-3 duty alone: difficulty says how demanding the work is, staffing
says how many people it takes.

**One downstream consequence, outside this report.** A duty needing two people occupies two of the
available loco drivers for its whole length, so any loco-driver demand calculation based on
`GeneralSettings.ExpectedLocoDrivers` becomes wrong once `StaffCount` exists and is ignored. Worth
picking up when that calculation is next touched; it is not part of the Driver Duties report.

The remaining gaps are small changes, but the model ones touch persistence, editing and
translation, so they are best done before the report is built rather than stubbed inside it. Note
that adding properties is safe, but *renaming* them later is not — a renamed settings property
silently resets on load.

---

## 10. Open questions

Answer by number; each answer becomes a decision in §11.

~~**Q1 — Page size.**~~ *Answered — confirmed. A5 portrait, two per side of an A4 landscape sheet,
both sides printed (D22).*

~~**Q2 — Folding.**~~ *Answered — folded, and stapled only when there is more than one sheet. A
four-page booklet is a single sheet that is simply folded; anything longer is folded and stapled
(D76). Either way the page count is a multiple of 4 and the imposition of §6.5 applies, so nothing
in the pagination depends on which case it is.*

~~**Q3 — One booklet per session combination.**~~ *Answered — see decision D1.*

~~**Q4 — Front and instructions.**~~ *Answered — and then refined by D50. The last page is the
**layout overview** (topology + shunting yards), still the last page (D4). The authored instructions
became a separate report (§5.5) and the duty notes moved to the front page.*

~~**Q5 — Duty part ordering and gaps.**~~ *Answered — no. Parts are ordered by departure and gaps
are not shown; the times already tell the driver when the next part begins.*

~~**Q6 — Call notes.**~~ *Answered — all `ICallNote` values filtered by `IsDriverNote`, sorted by
`DisplayOrder`, split between the Arr and Dep rows by `IsForArrival` / `IsForDeparture` (D28). Both
persisted and generated note families appear.*

~~**Q7 — Cargo and waybills.**~~ *Answered — a four-column block: sessions, position in the rake,
where the wagons come from, where they go (§5.3.5). No times.*

~~**Q8 — Selecting what to print.**~~ *Answered — two mechanisms, and the report needs both (D77,
D78). The route renders **all** duties by default and the selection is normally made in the print
dialogue by page range; a `?duties=1,2,3,7` query string filters the render itself.*

~~**Q9 — What did the prototype get wrong?**~~ *Answered — **a lot of notes on the calls.** Everything
about traction, wagonsets and cargo had to become note text because the timetable was the only thing
printed, so the note column grew long and repetitive exactly where a driver can least afford it.
That is the whole reason for the three tables above the timetable (D40, D41), and the measure of
whether this report improves on the prototype is how short the note column becomes.*

~~**Q10 — Parts that do not run every session.**~~ *Answered — prevented by construction (D42): a
duty's sessions are bounded by the intersection of its trains' sessions, so every part runs whenever
the duty does and no per-part session marking is needed in the timetable.*

~~**Q11 — Duty numbering and sort order.**~~ *Answered, then revised. **Renumbering** assigns numbers
by start time, end time, first session number (D43) — skipping duties excluded from renumbering and
reserving their numbers (D64, G7). **Printing** goes in duty-number order (D65), which is the order
the pile is kept in and no longer the same thing.*

~~**Q12 — Information page overflow.**~~ *Answered, twice over. Structurally it was dissolved by D50
— the authored markdown left the booklet for its own report (§5.5), where length is a non-issue
because it paginates freely, and what remains on the last page is a diagram and a short table
(§5.2). And where authored text still does not fit anywhere, **the writer shortens it** (D82). The
report never truncates, shrinks or silently absorbs authored content.*

~~**Q13 — A train part taller than one page.**~~ *Answered — the part splits, with the **timetable
block moving to the next page**, and the two pages must form a **spread** so the driver sees both
without turning (D80, §6.3.1). A split part therefore begins on an even page. Only when the split
halves still do not fit does the overflow-and-report behaviour of D26 apply.*

~~**Q23 — Is a non-booklet output needed?**~~ *Not now — noted as a possible future feature. Booklet
order is the only output built. The pipeline already keeps imposition as a separate final step
(D24), so a reading-order output would be that step omitted, not a second implementation.*

~~**Q32 — "Cargo served from": shape and placement.**~~ *Answered — a nullable `Station` reference on
`OperationLocation`, offered only where `HasCargoExchange` is true; shunting yard status is derived
from the relation, with no flag (D48).*

~~**Q33 — Can the information page still be one page?**~~ *Answered — yes, by splitting the three
kinds of information three ways (D50). The markdown becomes its own report, the duty notes go on the
front page, and the last page keeps only the topology and the shunting yards list. D4 and §6.4 stand
unchanged.*

~~**Q35 — The cargo block's columns.**~~ *Answered — five columns, one row per `CargoFlowTrainPart`,
position 0 printing "Any", "Wagons from" a de-duplicated union of the flow's origins and the
from-station, "also shunt" appended to the column it qualifies, and wagon classes in a column of
their own between from and to (D91–D93, §5.3.5).*

~~**Q36 — How are crossings and overtakings derived?**~~ *Answered — one shared overlap test,
`other.Arrival < mine.Departure && other.Departure > mine.Arrival`, with direction deciding which
note. The note carries the shared interval and `Train.ToString()`, and attaches to the **arrival**.
Direction is the sense in which the train traverses the `TrackStretch` it **arrives on**, which
resolves the reversal case and relies on the layout-wide stretch orientation that
`DirectionInconsistencies()` already validates (D94–D98, §5.3.6.1).*

~~**Q37 — What is the difficulty default?**~~ *Answered — the property is nullable and an ungraded duty
prints no difficulty line at all (D105).*

~~**Q38 — `ToText` / `ToHtml` naming.**~~ *Answered — unified on `ToText` / `ToHtml`; `Region` and
`Destination` were renamed from `ToHtmlMarkup` (D106), and the notes followed (D110).*

~~**Q34 — Should the general instructions report also carry the topology and shunting yards list?**~~
*Answered — yes, both are appended (D107).*

~~**Q14 — Where do the validity dates live?**~~ *Answered — `GeneralSettings`, General settings tab
(G1).*

~~**Q15 — What is the difficulty scale?**~~ *Answered — three named grades, set by hand (G2).*

~~**Q20 / Q21 / Q22 — colour and greyscale.**~~ *Answered — colour is emphasis only and never the
sole carrier of meaning, so greyscale is left entirely to the browser and printer driver (D19–D21,
revised). No setting, no monochrome stylesheet. One rule is still required:
`print-color-adjust: exact` on region chips, whose meaning depends on a background colour that
printing otherwise drops (§8.1).*

~~**Q16 — Front page in the multi-company case.**~~ *Answered — distinct values from the duty's
train parts, comma-joined (D27). Note this means `DriverDuty.Company` is **not** the source; the
front page derives the operators from the trains actually worked. Whether the duty's own `Company`
still has a role on the front page is worth confirming.*

~~**Q24 — Operator: signature or name?**~~ *Answered — `Company.Signature` (D27).*

~~**Q25 — Part header start time.**~~ *Answered — the arrival; the gap to departure is preparation
time (D32).*

~~**Q26 — Cargo row order.**~~ *Answered — position, then session (D33).*

~~**Q27 — Session-qualified note text.**~~ *Answered — notes render as `MarkupString` via `Html`, so
circles go in the note's markup like the existing region chips (D34).*

~~**Q28 — Which note types are driver notes?**~~ *Answered — build the outline's notes; three are
new (§5.3.6.1). Further types can follow later.*

~~**Q29 — "No stop" and "No exchange".**~~ *Answered — both derived from `IsStop` combined with the
times (D36).*

~~**Q29a — `IsStop` true with equal times.**~~ *Answered — valid; the train stops with no scheduled
dwell. One row showing the departure, no note (D38).*

~~**Q31 — Does `HidePassings` apply to the driver's own timetable?**~~ *Answered — no. The driver
always sees every location on the route (D44).*

~~**Q30 — What defines a meet?**~~ *Answered — **two** note types, crossings and overtakings, both
restricted to trains that meet the driver's own train and qualified by the shared session set, both
suppressed by `OperationLocation.HideMeets` (D39).*

~~**Q17 — Session circles: active only, or all with inactive outlined?**~~ *Answered — active only
(D9).*

~~**Q18 — Where does the shared component live?**~~ *Answered — `Planning.Components.Shared`
(D46).*

~~**Q19 — Session circles when the count is large.**~~ *Answered — the "all sessions" text and the
contiguous short form cover it (D12).*

~~**Q19a — Short-form threshold.**~~ *Answered — three, matching days mode, where three contiguous
days already read as "Monday to Wednesday" (D16).*

~~**Q19b — Is the short form graphical or textual?**~~ *Answered — graphical, two circles joined by
a dash (D15).*

~~**Q19c — Scattered runs.**~~ *Answered — the short form applies per run (D15).*

---

## 11. Decisions

*Recorded as they are made, with the reasoning, so later changes do not silently undo them.*

| # | Decision | Reason | Date |
|---|---|---|---|
| D1 | One booklet per duty, covering **all** sessions the duty runs. No per-session variants. | Booklets are printed once for the meeting and reused across sessions; a session-specific booklet could not be returned to the pile and re-issued. | 2026-07-27 |
| D2 | Duty number and session set must be legible on the **closed** booklet's front page, in fixed positions. | Before each session the pile is sorted by hand: include/exclude by session, then order by duty number. Both decisions are made without opening the booklet. | 2026-07-27 |
| D3 | The report supports both printing all duties at once and printing one duty alone. | Normal use is a single bulk print before the meeting; single prints cover the two exception cases — a correction, or a booklet not returned. | 2026-07-27 |
| D4 | The layout overview page is the **last** page; blank padding goes before it. | *(Reason revised by D50.)* It is found by turning to the back, without knowing the page count — and it is turned to repeatedly during a run, so a fixed position matters more than it would for a page read once. | 2026-07-27 |
| D5 | *(relaxed by D80)* The train part is the atomic unit of page fitting; parts are never split across pages. | The four blocks of a part are read as one unit. Reduces pagination to bin-packing of indivisible items instead of table splitting. Still the rule for every part that fits; only an oversized one splits, and then onto a facing page so the four blocks stay in view together. | 2026-07-27 |
| D6 | The duty number is the dominant element of the front page, and the print timestamp is always shown. | The number is what the pile is sorted by; the timestamp is the only way to tell a reprint from the booklet it replaces. | 2026-07-27 |
| D7 | Session numbers render graphically as filled black circles with centred white numerals; day names stay textual. | Session numbers are new with no proven precedent, and bare digits read poorly off a closed booklet in a stack. Days keep the prototype's proven textual form. | 2026-07-27 |
| D8 | The session indicator is drawn as SVG with `fill`, never as a CSS background colour. | Browsers drop background colours when printing unless `print-color-adjust: exact` is set; the indicator would vanish on the printed artefact it exists for. | 2026-07-27 |
| D9 | Only the sessions a duty actually runs are shown. No outlined placeholders for inactive ones. | Simpler and less cluttered; the duty's own sessions are what the reader needs. | 2026-07-27 |
| D10 | Session circles are used in **every** place a session number is displayed, application and reports alike, via one shared component in `Planning.Components`. | One visual language for a concept that is new to users; avoids the GUI and the printed booklet disagreeing about what a session looks like. | 2026-07-27 |
| D11 | The component scales by inheriting font size — SVG with `viewBox`, sized `1em` in CSS — rather than taking a size parameter. | It is needed at heading size on the front page and body size in tables and charts; inheriting means one component with no size plumbing at any call site. | 2026-07-27 |
| D12 | Four display forms: all-sessions text, contiguous short form, individual circles, plus an additive on-demand marker. | Bounds the width without a size fallback, and gives session numbers and day names the same three shapes so users learn one pattern. | 2026-07-27 |
| D13 | "All sessions" is decided by `CoversAllWithin(useDays, maxSessions)`, not by a count of 14. | A three-session meeting where a duty runs all three *is* all sessions; the existing `SessionsNumbers` "All" literal only triggers on all fourteen bits and would be wrong. | 2026-07-27 |
| D14 | On demand is additive — the sessions stay visible and "On demand only" is added. | A duty can run sessions ①–③ *and* be worked on demand on those three; suppressing the sessions would lose which three. | 2026-07-27 |
| D15 | The short form is **two circles joined by a dash**, applied **per contiguous run**. | Keeps the visual language rather than dropping to text, while bounding width. Per-run means 1,2,3,7,8,9 reads ①–③ ⑦–⑨, and runs mix freely with lone circles. | 2026-07-27 |
| D16 | The short form applies to runs of **three or more**; runs of one or two render as individual circles. | Matches days mode, which already abbreviates three contiguous days as "Monday to Wednesday", so both modes abbreviate at the same point. A run of two saves no width and reads worse. | 2026-07-27 |
| D17 | Validity dates are two nullable `DateOnly?` properties on `GeneralSettings`, edited on the General settings tab. | They describe the meeting the whole layout is planned for, alongside the other layout-wide operating properties. Nullable so an unbooked layout omits the line rather than printing a placeholder. | 2026-07-27 |
| D18 | Difficulty is a three-value enum on `DriverDuty` — Easy / Medium / Experienced — set by hand. | Named grades with fixed meanings, not an open numeric range; an enum keeps the meaning in the model rather than in the reader's head. | 2026-07-27 |
| D19 | *(superseded by D21a)* Greyscale printing is an explicit choice in the application. | Withdrawn — unnecessary once colour carries no meaning of its own. | 2026-07-27 |
| D20 | *(superseded by D21a)* Report colours are custom properties overridden by a `.monochrome` class. | Withdrawn with D19; no monochrome stylesheet is built. | 2026-07-27 |
| D21 | Colour is never the sole carrier of meaning. Every coloured element sits beside text stating the same fact. | Colour is for emphasis and scanning speed — category, region, difficulty all show their name or number too. This is the premise that makes D21a safe. | 2026-07-27 |
| D21a | Greyscale is left entirely to the browser and printer driver. No setting, no monochrome mode. | Given D21, converting everything to grey loses only scanning speed, not information. Avoids modelling something the browser cannot detect anyway. | 2026-07-27 |
| D21b | Elements whose meaning depends on a **background** colour carry `print-color-adjust: exact`. | Printing drops backgrounds by default. Region chips compute their text colour to contrast with the background, so dropping it yields light text on white paper — invisible. | 2026-07-27 |
| D22 | Two A5 portrait pages per A4 landscape sheet, printed both sides in saddle-stitch booklet order. | Two A5P are 296 × 210 mm against A4L's 297 × 210 mm — an exact fit. Matches the prototype and the way the booklets are actually made. | 2026-07-27 |
| D23 | Booklet order comes from a formula — sheet `i`: front `(N−2i, 1+2i)`, back `(2+2i, N−1−2i)` — not a lookup table. | Reproduces all four of the prototype's hardcoded tables exactly while extending to any multiple of 4; the prototype threw above 16 pages. | 2026-07-27 |
| D24 | Logical page building (steps 1–4) is kept separate from imposition (step 5). | Page building is list construction and imposition is a pure permutation; separated, both are unit-testable without a browser. | 2026-07-27 |
| D25 | Height estimation is empirical row units with a fixed page budget, carried over from the prototype and re-tuned for this typography. | Estimating rather than measuring is deterministic, testable without a browser, and reliable under print preview — the same reasoning as `TimetablePaginator`. | 2026-07-27 |
| D26 | An oversized train part still prints (overflowing) but raises a validation message. | The prototype's silent overflow is why the case has never been observed; printing something beats printing nothing, but the planner must be told. | 2026-07-27 |
| D27 | Operator signatures (`Company.Signature`) and train category names are both **distinct sets derived from the duty's train parts**, comma-joined, in working order, nulls dropped. **Working order only** — no alphabetical alternative. | A duty may work trains of several companies and categories; one rule for both keeps the front page consistent and the derivation trivial. Signatures stay short when several are joined. Working order matches the sequence of the part pages behind the front page, so the two read in step; any other order would set up a second sequence competing with the one the booklet is built on. | 2026-07-27 |
| D28 | Call notes are `ICallNote` filtered by `IsDriverNote`, sorted by `DisplayOrder`, and assigned to the Arr or Dep row by `IsForArrival` / `IsForDeparture`. | The interface already models exactly this, spanning persisted and generated notes; the audience flags keep station and shunting notes out of a driver's booklet. | 2026-07-27 |
| D29 | A train part is a header block plus four table blocks in fixed order: traction, wagonsets, cargo, timetable. | Matches how the driver works — what am I driving, what am I pulling, what is in it, then where and when. The timetable is last because it is consulted repeatedly during the run. | 2026-07-27 |
| D30 | An empty block is omitted entirely — heading, table and rule — and costs zero height. | Keeps a simple part short and makes page fitting reflect what is actually printed. | 2026-07-27 |
| D31 | *(revised by D72)* In the timetable, the Arr/Dep prefix shares a column with the station name. Row count comes from the **times**: equal arrival and departure give one unprefixed row, differing times give two. The part's first call shows Dep only and its last call Arr only. | Reads as a single "what happens here" column. The times say whether the train stands; the first and last calls' other halves are already in the header (D32), so showing them twice would duplicate. | 2026-07-27 |
| D36 | `IsStop` is orthogonal to row structure — it says whether standing means work, and drives two notes: `!IsStop` with equal times → *No stop*; `!IsStop` with differing times → *No exchange*. | A train may stand for a meet or a signal yet exchange nothing. Both facts are needed: times say whether it stands, `IsStop` says whether work happens. | 2026-07-27 |
| D37 | *No stop* is a **departure** note; *No exchange* is an **arrival** note. A single-row pass-through shows the departure time. | *(First reason superseded by D99.)* The original argument — that an arrival note would have no row on a pass-through — no longer holds now that a single row carries both halves; and since *No stop* only arises on equal times, its classification no longer decides its visibility at all. *No exchange* stands on its own reason: it answers "do I have work here?", which the driver asks on pulling in, so it belongs on the arrival row and not three minutes later. | 2026-07-27 |
| D38 | `IsStop` with equal times is valid, not an error: the train stops, briefly enough that no dwell is scheduled. One row, showing the departure, no note. | Equal times mean "a single moment", which is a real operating case for a short stop; only the *absence* of a stop needs saying. | 2026-07-27 |
| D39 | Crossings and overtakings are **two** note types, both suppressed by the single existing `OperationLocation.HideMeets`, restricted to trains that cross or overtake the driver's own train, qualified by the shared session set. | They are different events for the driver — one comes towards you, one comes past you — but one flag already covers both, matching its documented meaning. `HidePassings` concerns listing non-stopping trains and is unrelated. | 2026-07-27 |
| D40 | Timetable notes are either **pointers** to the three tables above, or genuinely per-call facts with no other home. | The prototype printed only a timetable, so everything had to become a note; the tables absorb that. A long note now signals that a fact belongs in a table instead. | 2026-07-27 |
| D41 | The three tables exist to replace notes, which is why they precede the timetable. | Structured columns scan faster than prose and remove the repetition that made the prototype's note column hard to read. | 2026-07-27 |
| D42 | A duty's sessions are bounded by the intersection of its trains' sessions, and may be further restricted by hand — the same principle as schedules. | Guarantees every part runs whenever the duty runs, so a driver can never be sent to a train that is not running, and no per-part session marking is needed in the timetable. | 2026-07-27 |
| D43 | *(revised by D64)* Duties print in order of start time, then end time, then first session number — the same keys `RenumberDriverDuties()` uses. | That is the true running order; renumbering to ordinals then makes the pile hand-sortable by a plain number. Sorting on the same keys avoids `Identity` string ordering putting "10" before "2". | 2026-07-27 |
| D44 | The driver's timetable lists **every** location on the route. `HidePassings` never removes a call from it. | The route must read as an unbroken sequence — a missing location is a gap the driver cannot account for. `HidePassings` governs listing other, non-stopping trains in station reports. | 2026-07-27 |
| D45 | *(split by D50)* Information content lives on the `Plan`: authored markdown, the layout topology, and a shunting yards list. Edited in a new **Settings ▸ Information** sub-tab. | Ownership and editing surface stand; only the *placement* of the three contents changed. The topology reuses `TopologyDiagram.Build` unchanged, and the sub-tab reuses the pattern `StretchesTab` already uses for Topology. | 2026-07-27 |
| D46 | The shared session component lives in **`Planning.Components.Shared`**. | `Planning.App` already references `Planning.Components`, so one component there serves both the GUI and the reports; a `Shared` namespace separates it from the `Reporting` and `Scheduling` areas. | 2026-07-27 |
| D47 | A plain reading-order (non-booklet) output is **not** built now — recorded as a possible future feature. | Booklet order is what is actually printed. Because imposition is a separate final step (D24), adding it later means omitting that step, not writing a second implementation. | 2026-07-27 |
| D48 | A shunting yard is **derived**: a station is a shunting yard exactly when at least one other location is served from it. The only new data is a nullable **`CargoServedFrom`** `Station` reference on `OperationLocation`, labelled *"Cargo served from"*. | One relation replaces a flag plus a reference, so nothing can fall out of step — no station marked a shunting yard while serving nothing. The shunting yards list is the inverse of that relation. The name carries the scope: the relation is about local freight only, matching the `HasCargoExchange` gate, and a bare "served from" would read as covering every way one location is worked from another. | 2026-07-27 |
| D49 | The relation is asymmetric: the **served** location needs `HasCargoExchange` and must not be a shadow station; the **serving** station may be any station, shadow included. | A shadow station is off-layout staging — traffic originates there, so it can serve others but is never itself a delivery destination. | 2026-07-27 |
| D32 | The part header's "starts at" is the first call's **arrival** and "ends at" the last call's **departure**; the timetable shows the movement times. | The gap between them is preparation and stand-down time — an hour at Munkeröd in the example. Same distinction `DriverDuty.DefaultStartTime`/`DefaultEndTime` already make at duty level. | 2026-07-27 |
| D33 | Cargo rows are ordered by **position, then session**, and **Position is the first column** — so the cargo block leads with Position where traction and wagonsets lead with Sessions. | Puts everything about one place in the rake together, so a driver reading position 2 sees both session variants side by side. Reorders the original outline. The general rule is that the leading column is the sort key: the repeated 1, 1, 2, 2, 3 then reads as grouping, whereas a sorted second column under an unordered first one reads as an unsorted table. | 2026-07-27 |
| D34 | Notes render through `ICallNote.ToHtml` (`MarkupString`), never `ToText`. | Formatting can then be added per note type without touching any caller — already proven by `CargoFlowDestinationNote`'s coloured region chips. Session circles inside notes follow the same route. | 2026-07-27 |
| D35 | Height estimation measures a note's `Text`, not its `Html`. | Wrapping is charged per character; markup would inflate the count and charge a short note as a long one. | 2026-07-27 |
| D50 | The three kinds of non-part information are split three ways: **general instructions** become a separate report, **duty notes** go on the front page, **topology and shunting yards** stay as the last page. | Each has a different audience, a different scope and a different reading occasion. Splitting on those lines puts every item where its reader is, instead of binding meeting-wide text into every booklet and still leaving the station staff without it. | 2026-07-27 |
| D51 | The general instructions are a **separate A5 booklet**, same two-up A4L format, handed to every participant before the first session — station staff included. | Its audience is wider than the drivers and its content is identical in every duty; printing it once per duty would repeat the same pages across the pile and still miss the people who hold no booklet. | 2026-07-27 |
| D52 | Duty notes sit **last on the front page**, above the footer, rendered in collection order. | They are the only variable-height element there, so the fixed positions the pile-sorter depends on (D2) stay put while the notes grow into free space. Overflow prints and raises a validation message (D26) rather than truncating hand-written instructions. | 2026-07-27 |
| D53 | Padding (step 3) and imposition (step 5) are written **independent of duties** and shared with the instructions report. | Both depend only on a page count. The instructions report then replaces steps 1, 2 and 4 with a markdown flow-split and reuses the rest, so the second report costs one page-splitter, not a second pipeline. | 2026-07-27 |
| D54 | The reader is a **meeting participant with some but varying experience** — of module meetings and of driving. The booklet assumes the craft and explains the plan. | It need not teach operating, but nothing about this layout or plan carries over from the last meeting. Varying experience is also what makes the difficulty grade a real choice rather than decoration. | 2026-07-27 |
| D55 | The report does **not** filter by session; the driver reads the Sessions column and takes today's rows. | One booklet covers all the duty's sessions (D1), so a session-filtered rendering would need reprinting per session — the thing D1 exists to avoid. The report's job is to make "which rows are mine today" instant, which is what the shared session indicator (D10) is for. | 2026-07-27 |
| D56 | The booklet must get the driver to the **physical locomotive**: the traction identity printed is what matches the loco card, its turnus card and the throttle. It is `ScheduledObject.Designation`, never `ToString()` or `Number`. | `Designation` is the `ExternalId` when present, which is how the vehicle is identified everywhere else; `ToString()` returns the composed identity and silently bypasses it, so a vehicle with an external id would print under a name the cards do not use. Wagonsets follow the same rule. | 2026-07-27 |
| D57 | Type size has a **floor**: text too small to read at arm's length is a failure, whatever it buys in fitting. | Poor light is not the real constraint — small type is. This bounds what §6's height estimation may trade away when a part nearly fits; shrinking type is not an escape hatch. | 2026-07-27 |
| D58 | The booklet is designed for the **common case**, not padded with explanation for rare ones. Asking someone is an encouraged part of the meeting, not a failure of the report. | Participants are always told to ask when something is unclear, and most start without needing to. Text covering rare situations is read past by every driver who does not need it, so a rare case is better served by asking than by a longer booklet. | 2026-07-27 |
| D59 | The front page has a **third reader — the chooser**. A participant works several duties per session, returning to the pile mid-session to pick the next from the closed front pages. | Choosing uses start time, start station and difficulty — fields the sorter ignores. This keeps start time and station on one line rather than split apart, and makes difficulty something read at every changeover instead of once a meeting. | 2026-07-27 |
| D60 | Printing is **two steps**: browser → PDF, proof-read, then PDF → paper. The PDF is the verification target and the thing the specification's "verify on paper" items are checked in. | That is the established practice, and it is where appearance and pagination are actually judged. Only the printer's colour-to-grey conversion still needs paper. | 2026-07-27 |
| D61 | The print timestamp (element 12) is the **render** time, fixed in the PDF. Nothing depends on when paper came out of the printer. | It identifies the version, which is what tells two booklets of the same duty apart. A PDF printed a week later must still carry the timestamp of the render it came from. | 2026-07-27 |
| D62 | The saved PDFs are the **reprint source** for a lost booklet; only a *correction* justifies a fresh render. | Re-rendering after the plan has changed yields a booklet that silently differs from the rest of the pile. A correction is meant to supersede, and the timestamp makes that visible; a replacement is meant to match. | 2026-07-27 |
| D63 | Duplex flip and greyscale are settings of the **PDF print dialogue** (step 3), so the user-facing printing instructions are written for that step, not the browser's. | Imposition order is baked into the PDF page sequence and is correct either way, but the short-edge flip is applied where paper is produced. Instructions aimed at the browser dialogue would be given at the wrong moment. | 2026-07-27 |
| D64 | A duty can be **excluded from renumbering** to hold a fixed number. Renumbering then reserves that number so no ordinal reuses it, and validation reports duplicate or empty pinned identities. | Some duties are known by number across meetings. Excluding the duty alone is not enough — the ordinal walk would hand the same number to another duty, and the pile is sorted by exactly that number. | 2026-07-27 |
| D65 | *(revises D43)* Duties print in **duty-number order**, compared numerically where the identity is numeric and alphabetically after them where it is not — not in running order. | Success criterion 4 is a stack that needs no hand-sorting, and §2.1's pile is ordered by number. Renumbering used to make the two orders identical, so D43's running order was equivalent by accident; a pinned number (D64) breaks that, and the pile order is the one that must win. | 2026-07-27 |
| D66 | A company may carry an uploaded **logo**, shown on the front page in place of its signature — but **only if every company on that page has one**; otherwise all render as signatures. | Mixing a graphic with a text abbreviation puts two incomparable things on one line, and the company without a logo reads as a missing image rather than a deliberate choice. Consistency across the line beats polish on a page read fast and identically every time. The decision is per duty, so two booklets from one plan may legitimately differ. | 2026-07-27 |
| D67 | The logo is stored **inline on `Company` as a data URI**, and rendered through `<img>` for both raster and SVG — never inlined as `<svg>`. | Inline storage keeps the plan a single self-contained file that survives "Save as" and reopening elsewhere. `<img>` neutralises script in an uploaded SVG, which matters precisely because users supply these files, and gives one code path for every accepted format. The `alt` is the signature, so a failed load degrades to the text the logo replaced. | 2026-07-27 |
| D68 | The shunting yards table has **three columns** — shunting yard, locations served, regions served — and a station qualifies as a shunting yard on **either** relation, not only on `CargoServedFrom`. | A shunting yard covers on-layout destinations and off-layout ones, and the driver holding a wagon card needs both to answer "where does this go from here?". Requiring `CargoServedFrom` alone would omit the shadow shunting yard that serves no on-layout location but stands for several regions — the very station most in need of listing. | 2026-07-27 |
| D69 | Regions in the shunting yards table render as the **same coloured chips** as in the cargo notes, via `Region.ToHtml`. | The overview page then reads as a key to the notes rather than as separate information — the driver meets one visual token in both places. It also means the `.region` `print-color-adjust` fix (D21b) is load-bearing on the overview page, not only in the timetable. | 2026-07-27 |
| D70 | The part header carries **every limit that is set** — speed, axles, wagons, length — and an unset limit contributes neither value nor label. The whole line disappears when nothing is set. | On paper a driver cannot distinguish "not restricted" from "the planner forgot", so a label with no value is worse than silence. Most trains carry one or two limits, so printing four labels every time would be mostly empty and would still cost a line in page fitting. | 2026-07-27 |
| D71 | The limits are read from `Train.Length.Axles` / `.Wagons` / `.Meters` and labelled in words — **not** through `TrainLenght.ToString()`. | `ToString()` produces the symbolic compact form `24ʘ 12■ 2.5m` for dense tabular use, and returns the literal "Undefined" when empty. A header must read plainly and vanish when empty; neither behaviour fits. | 2026-07-27 |
| D72 | *(revises D31)* **Arr/Dep gets its own column**, first of five, with the station name repeated on both rows of a two-row call. A pass-through leaves the cell empty. | The prefix varies in presence and width, so sharing a column starts the station names at three different positions; the route reads as a list only when they align on one left edge. Repeating the name keeps every row self-contained, so a time is never read against a blank station cell. Row-structure rules from D31 are unchanged. | 2026-07-27 |
| D73 | `Sessions.Display` becomes a **pair** — `string ToText(SessionsSettings)` and `MarkupString ToHtml(SessionsSettings)` — each choosing sessions or days from the settings. | Both are needed and neither substitutes: text serves plain-text notes, validation messages, tooltips, exports and height estimation (D35 charges wrapping against `Text`), while markup serves the circles and anything embedded inside another `MarkupString`. One member returning one type could only serve half the call sites. | 2026-07-27 |
| D74 | The two renderers **share one core**: run-splitting and form-selection are computed once, and `ToText` / `ToHtml` render the same result. | The four display forms of §5.1.1 are a property of the value, not of the rendering. Deciding the form twice guarantees eventual disagreement — a tooltip saying something other than the circles beside it. | 2026-07-27 |
| D75 | `ToHtml` is the **single source of the session markup**; the shared component (D46) is a thin wrapper that emits it. | A component cannot be embedded in a `MarkupString`, so `ToHtml` cannot be replaced by one, and hand-building markup at every Razor call site would be worse than a component — so both exist. Making one delegate to the other keeps the circles drawn by one piece of code either way. Matches the established `Region.ToHtml` / `ICallNote.ToHtml` shape. | 2026-07-27 |
| D76 | Binding is **fold only for a single sheet, fold and staple for two or more**. Page size A5 portrait, two per side of an A4 landscape sheet, both sides printed — confirmed. | A four-page booklet is one sheet and needs no staple. Nothing in the pagination distinguishes the cases: both are a multiple of 4 and both use the imposition of §6.5, so the difference is a physical step, not a rendering one. | 2026-07-27 |
| D77 | **A duty's booklet never shares a sheet with another duty's** — and this is now load-bearing for selection, not only for the fold. | Padding to a multiple of four A5 pages makes every booklet a whole number of A4 landscape sheets. Without it, a print-dialogue page range could not isolate one duty, because the last sheet of one booklet would carry the first pages of the next. | 2026-07-27 |
| D78 | Selection works two ways: **render all and choose a page range in the print dialogue** (the default practice), or **filter with `?duties=1,2,3,7`**. Values are identities matched as strings; absent means all; unmatched values raise a visible, non-printing warning. | The bulk case wants everything on screen and a range chosen while proof-reading; a targeted reprint wants a link that renders just that duty. Identities are strings and may be non-numeric once pinned (G7), so integer parsing would break exactly the duties most likely to be reprinted. An ignored typo would silently print less than asked for. | 2026-07-27 |
| D79 | The on-screen toolbar shows a **sheet index** mapping each duty to its A4 landscape page range. | The print dialogue counts sheets, not duties, so without it the user counts by hand to build a range. The toolbar is `.no-print`, so it costs nothing on paper. | 2026-07-27 |
| D80 | *(relaxes D5)* A part that will not fit one page **splits, with the timetable block moving to the next page** — and the two pages must be a **spread**, so a split part begins on an **even** page. | The driver consulting the timetable must see the tables it points at; facing pages give that, a page turn does not. The timetable is the right block to move: already last (D29), the only one whose height is unbounded, and a boundary the reader recognises. | 2026-07-27 |
| D81 | Pointer notes must not assume a vertical relationship — *"see the tables for this train"*, not *"see above"*. | On a split part the tables are on the facing page, not above. A note whose wording depends on how pagination fell will eventually be wrong, and nothing in the note knows which case it is in. | 2026-07-27 |
| D82 | Authored text that does not fit is **shortened by its author**. The report never truncates, shrinks type or silently absorbs it. | Truncation loses a hand-written instruction without telling anyone, and shrinking type hits D57's floor. The planner is the only one who knows what can go; the report's job is to make the overflow visible (D26, D52). | 2026-07-27 |
| D83 | Every page of parts ends with a **continuation marker**: *"Duty continues on next page!"* (centred, bold, red) or *"No more trains in this duty."* (centred, bold, plain). | Participants have missed the last train of a duty by not turning the page. A missed train outranks every formatting concern in this specification, and nothing else on a full page says whether anything follows. The terminal message also tells the driver the trailing blanks hide nothing. | 2026-07-27 |
| D84 | The marker is placed **per page, under the last part on it** — not under every non-final part. A split part's first page carries none. | Under every non-final part it would claim "on next page" for a part sitting three centimetres below on the same page. A notice that is sometimes false is one drivers learn to ignore, which destroys the only thing it exists for. A split part is unfinished and its remainder is already in view on the facing page. | 2026-07-27 |
| D85 | The marker's height is reserved **once in the page budget**, not charged to a part. | Whether a part is last on its page is decided by the packing that the height estimate feeds into — charging it per part is circular. Reserving it in the budget costs only a slightly conservative page on the rare split, and errs towards unused space rather than overflow. | 2026-07-27 |
| D86 | Only the continuation warning is red; the terminal message is not. | Red must keep meaning *act on this*. The terminal message says there is nothing more to do, so colouring both would leave red meaning nothing. The words carry the meaning either way, so greyscale printing loses nothing (D21). | 2026-07-27 |
| D87 | `DriverDuty.StaffCount` records how many people work the duty — **default 1, editor range 1–3** as a select, with the model rejecting anything outside it. | A duty is a loco driver plus, when the card work demands it, a conductor. The name is "staff" because the two are different jobs: a *driver* count of 2 would state something false. Three options need no validation message and no way to type 7; beyond three it is not one duty. Model-side validation still guards a hand-edited plan file. | 2026-07-29 |
| D88 | The property carries an **initialiser** (`= 1`), and neither constructor assigns it. | A plan saved before the property existed has no value in its JSON, so deserialisation leaves the initialised 1 in place — the truth for almost every duty. Without it every existing duty would load as *zero* people, wrongly and silently. | 2026-07-29 |
| D89 | Staffing prints on the front page **only when greater than 1**, beside difficulty. | One is the overwhelming default, so printing it always would add a line to every front page to state what the reader already assumes (D58). At 2 or 3 it is not a detail but an instruction — this duty cannot be started alone — which makes it a chooser's field (D59), answering *what kind of commitment is this?* alongside difficulty. | 2026-07-29 |
| D90 | When `StaffCount` > 1 the booklet has **two readers**: the loco driver and the conductor, whose document is the cargo wagons block (§5.3.5). | The conductor works the wagon cards — which wagons to bring, which to set off, what order they sit in — which is exactly that block and nothing else. It is a further argument for D41: left in the timetable's note column as the prototype had it, the second person could not have worked from anything without taking the booklet off the driver. | 2026-07-29 |
| D91 | A cargo row is one **`CargoFlowTrainPart`**, not one destination. | A flow's destinations belong together as one statement of where these wagons go, and `CargoFlowTrainPart` also carries the per-occurrence behaviour the row must show — position, shunting, whether wagons are taken here. One object, one row. | 2026-07-29 |
| D92 | Position comes from `CargoFlowTrainPart.PositionInTrain` and prints **"Any" when 0**. | Zero means anywhere in the train; a cell showing `0` would be read as a position at the front. Note `Destination.PositionInTrain` is a different property of the same name and type — the row's position is the flow's. | 2026-07-29 |
| D93 | "Wagons from" is the **de-duplicated union** of `CargoFlowOptions.Origins` and the from-station, the latter included unless `BringsNoWagonsFromHere`. "Also shunt" is appended to the column it qualifies — from for `AlsoShuntBeforeDeparture`, to for `AlsoShuntAfterArrival`. Wagon classes get their own column; empty means no restriction and prints nothing. | The flag removes one of two sources, never the column. De-duplication matters because an origin list may already include the from-station. Appending the shunt qualifier avoids a column that would be empty on most rows, and an absent class restriction says more by being absent (D70). | 2026-07-29 |
| D94 | Crossings and overtakings share one overlap test — `other.Arrival < mine.Departure && other.Departure > mine.Arrival` — with **direction alone** deciding which note. | The two events differ only in relative direction; deriving them from one predicate keeps them consistent by construction and makes the pair impossible to get subtly out of step. | 2026-07-29 |
| D95 | The note carries the **shared interval** — `max(arrivals)` to `min(departures)` — and the other train as **`Train.ToString()`**. | The window in which the other train is actually there is what the driver needs, not either train's own dwell. `Train.ToString()` already composes company signature, category prefix, number and suffix, and resolves `EffectiveCompany`, so a train inheriting its category's company is still named correctly. | 2026-07-29 |
| D96 | Direction is **the sense in which the train traverses the `TrackStretch` it arrives on** — `Start → End` or `End → Start`. A train originating at the station falls back to its outbound stretch. | Nothing in the model states a train's direction, but a stretch has a `Start` and an `End`, and traversal sense is exactly what distinguishes a crossing from an overtaking. Taking the **inbound** stretch gives one unambiguous answer even where a train reverses (`IsChangingTrainDirectionPossible`) — and it is the right one, since what the driver meets is the train that came towards them. | 2026-07-29 |
| D97 | Senses are comparable across different stretches because **all track stretches are defined in the same direction**, a rule `Layout.DirectionInconsistencies()` already validates. | Two trains meeting at a station arrive on different stretches, one from each side, so the comparison would be meaningless without layout-wide consistent orientation. This makes an existing consistency check a precondition of the report: an inconsistent layout yields *wrong* crossing and overtaking notes, not missing ones, which the validation message should say. | 2026-07-29 |
| D98 | Crossing and overtaking notes are **arrival notes**. | They are derived from the inbound stretch direction and answer "who am I meeting here", which the driver asks on pulling in. Visibility on a pass-through is handled by D99, so the classification can follow the meaning rather than the layout. | 2026-07-29 |
| D99 | A **single-row call renders both arrival and departure notes**, merged and ordered by `DisplayOrder`. | With one row, D28's split by `IsForArrival` / `IsForDeparture` has nowhere to land, and a note classified for the missing half would be silently dropped — most damagingly for notes that only *occur* on single-row calls. It also lets every note be classified by what it describes instead of by where it would be visible. Height estimation charges all of a single-row call's notes to that row. | 2026-07-29 |
| D100 | Notes stack **one per row**: the first in the call row's note column, each further one on its own **full-width** row. Not one concatenated paragraph. | The prototype's running text was hard to read and wrapped unpredictably. One note per line makes each a discrete statement, and a full page width holds roughly twice the note column, so wrapping becomes the exception. | 2026-07-29 |
| D101 | Consequently, note height is **one unit per note** plus rare wrapping — counting rather than measuring. | This is the largest available improvement to the pagination's reliability, and it comes from a layout choice rather than better arithmetic: the prototype charged per character precisely because wrapping was unpredictable. | 2026-07-29 |
| D102 | The timetable table uses **`table-layout: fixed`** with declared column widths. | With automatic layout a full-width `colspan` cell participates in width calculation, so one long note would widen the note column and shift every other column. Fixed layout also turns the wrap width into a known constant, which is what makes "estimate, never measure" (D25) sound for this table rather than merely convenient. | 2026-07-29 |
| D103 | The first note sits in the call row's note column **only if its `ToText` is at most ~25 characters**; otherwise every note stacks on full-width rows and the cell is left empty. | Keeps the density of a compact single row for the common call — *No stop* is 7 characters, *No exchange* 11, and short notes are the frequent ones — while guaranteeing that nothing ever wraps in the narrow column. The two-rate estimate collapses to one, and the limit is measured on `ToText`, not `ToHtml` (D35). Tune with the §6.2 constants; bias low, since overflowing costs an unpredictable wrap while being cautious costs one tidy row. | 2026-07-29 |
| D104 | Notes are **never reordered to fit**. A long first note moves the whole group down rather than promoting a shorter later one into the column. | Note order is `DisplayOrder` and carries meaning; layout does not get to rearrange it. A reader who learns that the first note is the most important must not find that silently untrue on crowded calls. | 2026-07-29 |
| D105 | *(answers Q37)* `DriverDuty.Difficulty` is **nullable**, and an ungraded duty prints no difficulty line at all. | The chooser reads this field at every changeover (D59), so an ungraded duty rendered as grade 1 would actively mislead. The same rule as the validity dates and the limits line: an absent line says "not set" unambiguously, where a printed value cannot be told from a deliberate one. | 2026-07-29 |
| D106 | *(answers Q38)* The codebase unifies on **`ToText` / `ToHtml`**. `Region.ToHtmlMarkup` and `Destination.ToHtmlMarkup` were renamed, and `CargoFlowTrainPart.ToPlainText` became `ToText`. `ICallNote.Text` / `Html` stay as they are. | Cheapest while the call sites are few, and it removes a third spelling rather than adding one. `ToHtml` was already the name on `CargoFlowTrainPart`, so this settles on what the codebase had started doing. The `ICallNote` pair are interface properties rather than conversions, so they are not the same thing. | 2026-07-29 |
| D107 | *(answers Q34)* The general instructions report **appends the topology and the shunting yards table**. | The people who receive it and never hold a duty booklet — station staff above all — get no layout overview from anywhere else. Both are existing components, so it costs almost nothing, and the duplication is harmless because the two documents are held by different people. | 2026-07-29 |
| D108 | **At most one `CrossingNote` and one `OvertakingNote` per call**, each carrying every train met of that kind, not one note per other train. Listed as *"Crosses G 44780 05:50-05:52, IC 912 05:50-06:02, RE 75510 05:56-06:02"*, ordered by start time, then end time, then the other train's number. | A busy junction can be crossed or overtaken by several trains in the same window; one row each would read as several unrelated events instead of the one fact that this call meets a group of trains. The three-key sort keeps the order deterministic even when two meets start and end together. | 2026-07-30 |
| D109 | **A full-width note row starts at the station column**, not the Arr/Dep column: the Arr/Dep cell is left empty and the note spans the remaining four columns. | Aligning the note's left edge with the station name above it reads as "this belongs to that place," which an arbitrary indent (the previous `padding-left: 2.5em`) only approximated. | 2026-07-30 |
| D110 | *(supersedes the exception in D106)* The notes join the unification: `ICallNote.Text` / `Html` became **`ToText` / `ToHtml`**, on the interface, on `CallNote`, on `TextCallNote` and on `GeneratedNote`. | D106 exempted them on the grounds that a property is not a conversion, but the distinction was invisible where it mattered: a reader moving between `Destination.ToHtml` and `note.Html` in the same expression met two spellings of one idea. The pair is a rendering wherever it appears, so it carries one name. Persisted note *data* keeps its own name — `TextCallNote.Texts` and `DriverDutyNote.Text` are stored values, not renderings, and renaming those would move a database column and a JSON key. | 2026-07-30 |

---

## 12. Readiness

### 12.1 What is settled

The report's **shape** is fully specified: page structure and order (§4), all four page kinds (§5),
pagination, the spread rule, blank padding and booklet imposition (§6), the printing workflow and
selection (§8). Ninety decisions are recorded with their reasoning.

### 12.2 What is not

Every open question is now answered: Q35 and Q36 during specification, and Q37, Q38 and Q34 at
implementation (D105–D107).

Two smaller items are flagged in place for a second look rather than raised as questions: the
`BringsNoWagonsFromHere` condition on the arrival-side *"also shunt"* (§5.3.5), and whether a
same-direction overlap ever needs to distinguish *"overtakes you"* from *"you overtake"*
(§5.3.6.1). Both are implemented as written here.

### 12.3 What only implementation can settle

§6.2's height constants are empirical. The prototype's — 45 units per page body, 40 characters per
wrapped line, 6 units of fixed overhead per part — encode its typography, not this one's, and this
report has four blocks where the prototype had one. They can only be tuned by rendering real duties
and looking at the PDF (§8.0). Expect the first pagination to be wrong and to need one or two
rounds; that is the method working, not a defect in it.

### 12.4 Suggested order

Model gaps first, because they touch persistence, editing and translation, and because the report
cannot be tested without them:

1. **G1** validity dates, **G2** difficulty, **G7** `IsExcludedFromRenumbering`, **G9** `StaffCount`
   — four small properties on existing types, plus their editors.
2. **G3** `Sessions.ToText` / `ToHtml` — a live defect: `TrainPartTractionView` already calls the
   stub and renders blank.
3. **G6** `CargoServedFrom`, **G8** `Company.Logo`, **G5** plan instructions markdown — each with
   its editing surface.
4. The **session number component** (§5.1.2), which everything else displays through.
5. The **four block components** (§5.3). Crossings and overtakings are the deepest item here: the
   direction derivation (D96) is the only thing the model does not already express, and it deserves
   unit tests of its own before the note types are built on it.
6. **Pagination and imposition** (§6) — pure list-building and a pure permutation, both unit-testable
   without a browser (D24).
7. The **general instructions report** (§5.5), which reuses steps 3 and 5 of the pipeline (D53).
