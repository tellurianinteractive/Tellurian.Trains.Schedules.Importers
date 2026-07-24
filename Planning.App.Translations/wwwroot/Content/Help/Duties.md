The **Duties** tab is where you plan **driver duties** — the work one loco driver performs across a
session, as a sequence of the train parts they drive.

A duty is built from the **train parts already defined in the vehicle schedules**: the traction
segments a locomotive or trainset works. The same train can be split across several parts (for
example where the locomotive changes at an electrification boundary), and a duty simply strings the
parts a single driver takes together.

### Building a duty

Each duty is a row: the first column shows its **identity**, **company** and the **sessions** it
runs; the second column lists the **train parts** in the order the driver works them.

- **New duty** adds an empty duty.
- **+ train part** appends the next part. The picker offers the traction parts a driver could take
  next: those that do not clash in time with the parts already in the duty and — once the duty has a
  part — those departing at or after it arrives. Adding parts in running order keeps that list to
  the natural continuations, but the order you add them in does not matter.
- Parts need **not** join at the same station: between two parts the driver walks to where the next
  part starts.

A part may be worked by **several duties**, as long as they run on **different sessions** — so one
duty can cover the odd sessions and another the even sessions of the same segment.

### Sessions, company and notes

Use the **edit** (pencil) control to set the duty's identity, operating company and the sessions it
runs, and to add free-text **duty notes** that apply to the whole duty (distinct from the per-call
notes shown elsewhere).

### Traction unit exchange

When two consecutive parts of the **same train** in a duty are worked by **different traction
units**, the driver stays with the train while the locomotive is exchanged. The tab shows a derived
note at the station where this happens — you do not enter it by hand.

### Validation

With **Settings › Validation › Driver duties** enabled, the plan is checked so that no train part is
driven by two duties on a common session, and no duty has parts that overlap in time. Conflicts are
listed in the validation indicator and open here.
