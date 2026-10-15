Fliken **Importera** läser in en plan, eller en del av en plan, i programmet.

### Filer som stöds

- **`.json`** – ett planeringsdokument som tidigare sparats från Timetable Planner.
- **`.ods` / `.xlsx`** – ett XPLN-tidtabellskalkylblad, som importeras till planeringsmodellen.

Välj en fil med **Öppna tidtabell**. Stora filer (upp till 50 MB) stöds.

### Delvis import

*Kommer*: Med delvis import kan du välja vilka delar av en plan som ska importeras.

### Importmeddelanden

Efter importen listas eventuella meddelanden, grupperade efter allvarlighetsgrad (information, varning, fel).
Listan öppnas automatiskt när det finns fel. Meddelandena rapporterar referensproblem (till exempel
ett uppehåll som hänvisar till en station som saknas) och tidtabellskonflikter, så de är värda att granska även
när importen lyckas. Meddelandena visar oftast importproblem med XPLN-filer.

En lyckad import blir den aktiva planen, och sammanfattningen visar antalet tåg, sträckor
och stationer som lästs in.
