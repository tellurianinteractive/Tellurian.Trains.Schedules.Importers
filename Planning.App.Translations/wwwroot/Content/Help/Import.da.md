Fanen **Importér** indlæser en plan eller en del af en plan i programmet.

### Understøttede filer

- **`.json`** – et planlægningsdokument, der tidligere er gemt fra Timetable Planner.
- **`.ods` / `.xlsx`** – et XPLN-køreplansregneark, som importeres til planlægningsmodellen.
- **`.json`** – en hal-plan fra Modultegneren, eller et anlæg, som et andet program har skrevet i samme form. Den bliver
  en ny plan med hal-planens driftssteder, spor og strækninger; hver strækning får en køretid ud fra sin længde.

Vælg en fil med **Åbn køreplan**. Store filer (op til 50 MB) understøttes.

### En nyere version af hal-planen

Når den åbne plan blev lavet fra en hal-plan, og du åbner en anden version af samme hal-plan, vælger du: opdatér den
åbne plan med den, eller lav en ny plan ud fra den. Ved opdatering overtages nye længder straks, og det samme gør
køretider, medmindre du har angivet dine egne.

Ændringer, der flytter eller tilføjer stop, vises under **Skal bekræftes**: et spor, der fjernes, et driftssted af en
anden type, et, der fjernes eller indsættes mellem to andre. **Gennemse** viser, hvad ændringen gør ved togene;
**Udfør** gennemfører den. Ændringer, der ændrer driftsstedernes rækkefølge, kan ikke udføres, fordi togene ikke kan
følge dem; lav dem manuelt. Åbner du den samme fil igen, vises det, der er tilbage.

### Delvis import

*Kommer*: Med delvis import kan du vælge, hvilke dele af en plan der skal importeres.

### Importmeddelelser

Efter importen vises eventuelle meddelelser, grupperet efter alvorlighed (information, advarsel, fejl).
Listen åbner automatisk, når der er fejl. Meddelelserne rapporterer referenceproblemer (for eksempel
et ophold, der henviser til en manglende station) og køreplanskonflikter, så de er værd at gennemgå, selv
når importen lykkes. Meddelelserne viser oftest importproblemer med XPLN-filer.

En vellykket import bliver den aktive plan, og oversigten viser antallet af tog, strækninger
og stationer, der er indlæst.
