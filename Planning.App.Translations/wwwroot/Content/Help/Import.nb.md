Fanen **Importer** laster en plan, eller en del av en plan, inn i programmet.

### Støttede filer

- **`.json`** – et planleggingsdokument som tidligere er lagret fra Timetable Planner.
- **`.ods` / `.xlsx`** – et XPLN-regneark med ruteplan, som importeres til planleggingsmodellen.
- **`.json`** – en hallplan fra Modultegneren, eller et baneanlegg som et annet program har skrevet i samme form. Den blir
  en ny plan med hallplanens driftssteder, spor og strekninger; hver strekning får en kjøretid ut fra lengden.

Velg en fil med **Åpne ruteplan**. Store filer (opptil 50 MB) støttes.

### En nyere versjon av hallplanen

Når den åpne planen ble laget fra en hallplan og du åpner en annen versjon av samme hallplan, velger du: oppdater den
åpne planen med den, eller lag en ny plan av den. Ved oppdatering tas nye lengder inn med en gang, og det samme gjør
kjøretider, med mindre du har angitt egne.

Endringer som flytter eller legger til stopp, vises under **Må bekreftes**: et spor som fjernes, et driftssted av en
annen type, et som fjernes eller settes inn mellom to andre. **Gå gjennom** viser hva endringen gjør med togene;
**Utfør** gjennomfører den. Endringer som endrer rekkefølgen på driftsstedene, kan ikke utføres, fordi togene ikke kan
følge dem; gjør dem manuelt. Åpner du den samme filen igjen, vises det som gjenstår.

### Delvis import

*Kommer*: Med delvis import kan du velge hvilke deler av en plan som skal importeres.

### Importmeldinger

Etter importen listes eventuelle meldinger opp, gruppert etter alvorlighetsgrad (informasjon, advarsel, feil).
Listen åpnes automatisk når det er feil. Meldingene rapporterer referanseproblemer (for eksempel
et opphold som viser til en stasjon som mangler) og ruteplankonflikter, så de er verdt å gå gjennom selv
når importen lykkes. Meldingene viser oftest importproblemer med XPLN-filer.

En vellykket import blir den aktive planen, og oppsummeringen viser antall tog, strekninger
og stasjoner som er lastet inn.
