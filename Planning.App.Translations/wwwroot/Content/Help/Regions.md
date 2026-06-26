The **Regions** tab defines the destinations *outside* your layout — the domestic regions and
foreign countries that wagons can be routed to. Regions are used for cargo flow routing: they let a
train's destination note say where its wagons are bound beyond the modelled railway.

## What a region is

Each region has:

- a **name**, written in the layout's default language and shown wherever the region is referenced;
- a **country** it belongs to — a new region defaults to the layout's default country, but a region
  standing for a foreign destination can be set to that country instead;
- a **background colour**, used to render the region as a coloured chip in notes (the text colour is
  contrasted automatically for readability).

## How regions are used

A region on its own is just a label. It becomes meaningful once it is associated with a
**station** — normally a **shadow station** (shadow yard), which stands in for the outside world at
the end of a line. A station can be associated with zero, one, or several regions; ordinary
stations seldom need any.

When a freight destination is set to *include regions*, the destination note for that station lists
its regions as coloured chips — telling the operator that wagons brought there continue on to those
regions or countries.

Define the regions you need here, then assign them to your shadow stations on the
**Operation Locations** tab.

## Managing regions

The list shows each region as a coloured chip alongside its name and country.

- **Add new** creates a region; give it a name, choose its country, and pick a colour from the
  palette.
- **Edit** changes a region's name, country or colour.
- **Delete** removes a region. It is blocked while any station is assigned the region — remove the
  assignment on the **Operation Locations** tab first.
- **Default regions** adds the standard set of regions, named in the layout's default language and
  handy when starting a new layout.

Regions are stored with the layout, and this catalogue is the source for the region choices shown
when editing a station.
