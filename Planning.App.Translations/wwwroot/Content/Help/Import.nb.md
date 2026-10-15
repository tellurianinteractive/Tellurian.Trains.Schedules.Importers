Fanen **Importer** laster en plan, eller en del av en plan, inn i programmet.

### Støttede filer

- **`.json`** – et planleggingsdokument som tidligere er lagret fra Timetable Planner.
- **`.ods` / `.xlsx`** – et XPLN-regneark med ruteplan, som importeres til planleggingsmodellen.

Velg en fil med **Åpne ruteplan**. Store filer (opptil 50 MB) støttes.

### Delvis import

*Kommer*: Med delvis import kan du velge hvilke deler av en plan som skal importeres.

### Importmeldinger

Etter importen listes eventuelle meldinger opp, gruppert etter alvorlighetsgrad (informasjon, advarsel, feil).
Listen åpnes automatisk når det er feil. Meldingene rapporterer referanseproblemer (for eksempel
et opphold som viser til en stasjon som mangler) og ruteplankonflikter, så de er verdt å gå gjennom selv
når importen lykkes. Meldingene viser oftest importproblemer med XPLN-filer.

En vellykket import blir den aktive planen, og oppsummeringen viser antall tog, strekninger
og stasjoner som er lastet inn.
