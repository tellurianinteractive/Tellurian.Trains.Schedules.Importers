The **Vehicle owners** tab records who brings which rolling stock to the meeting.

In modular operation, locomotives, trainsets and wagons are brought by different participants. Recording
the owners of each item makes it clear who supplies what, and gives each owner the facts they need to
put their stock on the layout before the first session.

### Rolling stock

Every locomotive, trainset and wagonset in the plan is listed, with:

- its **type**, **vehicle** designation and **class** — for a wagonset that lists its wagons, each wagon
  class once, as in *A/B/Fv*;
- the **count** of units — or of listed wagons — shown only when there is more than one;
- the **first session** (or **first day**) it is in operation;
- the **station**, **track** and **departure** time where it is to stand before that session — the start
  of the first train it works that session. A vehicle not yet given a schedule shows **Not in operation**;
- its **owner**, the owner's **DCC address**, the owners bringing **spares**, and a **note** about the item.

Tick **Only those without an owner?** to see what nobody has offered to bring yet.

A vehicle **Not in operation** can be removed from the plan with its **Delete** button, and
**Delete those not in operation** removes every one of them listed — only those shown when the filter is
ticked. Their owners are removed with them; the participants stay. A vehicle given work has to be taken out
of its schedules on the **Schedules** tab first.

### Owners of an item

Open a row to see and edit its owners. The first owner is the **primary** one, who brings the unit and
sets it up on the layout. Every further owner brings a **spare**. **Make primary** moves a spare owner to
the top; deleting the primary owner makes the first spare owner primary.

Type the first letters of a name and pick the participant from the list, or press **Enter** to take the
highlighted one. The start of any word in the name matches, so a surname works too. A name that matches
nobody is offered as a **New participant** — check the spelling before adding it.

### DCC addresses

A locomotive or trainset needs a **DCC address** for every owner, spares included, because each of them
is driven on the layout. Enter **0** when the owner has not yet said which address it is; it is shown as
**Owner to provide**. An owner cannot be added to a traction unit without an address, and an address can be
changed but not cleared. A warning sign marks a traction unit where an owner still lacks one — which
happens when a wagonset is changed into a locomotive after its owners were entered.

### Participants

The **Participants** view lists everyone named as an owner and what each of them brings; the item they
bring themselves is shown in bold. A name is kept once, so correcting a misspelt name here corrects it
everywhere. A participant who still brings rolling stock cannot be deleted.

### Printing

**Reports ▸ Vehicle contributors** prints this list on A4 landscape, arranged in one of three ways chosen
above the pages:

- **By operation location** — a page for each station, to hand to its owner: the vehicles to be set up there,
  in order of first session and departure, with their owners and DCC addresses. Each spare is on a row of its
  own, right below the unit it stands in for. The items not in operation follow on a page of their own.
- **By owner** — a page for each participant: what they bring, the units they set up themselves first, with
  the DCC address and where each item is to be set up. The items nobody brings yet come first, on a page
  headed **Not yet booked**.
- **By DCC address** — every locomotive and trainset brought, in one list, lowest address first; the missing
  addresses come after the known ones. Wagonsets take no address and are left out.

Each station or owner starts on a new page, and continues on the next page, under the same heading, when one
page is not enough. Where the owner of a locomotive or trainset has not given a DCC address, or has entered
**0**, the address is printed as **Missing** in red.

The background of a row shows when its unit is needed:

- **white** — in operation on every session (or day), or not in operation at all;
- **light grey** — a spare;
- **light blue**, **light green** or **light red** — first in operation on the first, second or third session,
  but not on all of them.
