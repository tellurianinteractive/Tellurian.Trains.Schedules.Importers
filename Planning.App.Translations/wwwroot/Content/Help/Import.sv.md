Fliken **Importera** läser in en plan, eller en del av en plan, i programmet.

### Filer som stöds

- **`.json`** – ett planeringsdokument som tidigare sparats från Timetable Planner.
- **`.ods` / `.xlsx`** – ett XPLN-tidtabellskalkylblad, som importeras till planeringsmodellen.
- **`.json`** – en hallplan från Modulritaren, eller en bana som ett annat program skrivit i samma form. Den blir en ny
  plan med hallplanens driftplatser, spår och sträckor; varje sträcka får en gångtid utifrån sin längd.

Välj en fil med **Öppna tidtabell**. Stora filer (upp till 50 MB) stöds.

### En nyare version av hallplanen

När den öppna planen gjordes från en hallplan och du öppnar en annan version av samma hallplan väljer du: uppdatera
den öppna planen med den, eller gör en ny plan av den. Vid uppdatering tas nya längder in direkt, och likaså gångtider,
om du inte har angett egna.

Ändringar som flyttar eller lägger till uppehåll listas under **Att bekräfta**: ett spår som tas bort, en driftplats
av en annan typ, en som tas bort eller läggs in mellan två andra. **Granska** visar vad ändringen gör med tågen;
**Gör ändringen** genomför den. Ändringar som ändrar driftplatsernas ordning kan inte göras, eftersom tågen inte kan
följa dem; gör dem för hand. Öppnar du samma fil igen listas det som återstår.

### Delvis import

*Kommer*: Med delvis import kan du välja vilka delar av en plan som ska importeras.

### Importmeddelanden

Efter importen listas eventuella meddelanden, grupperade efter allvarlighetsgrad (information, varning, fel).
Listan öppnas automatiskt när det finns fel. Meddelandena rapporterar referensproblem (till exempel
ett uppehåll som hänvisar till en station som saknas) och tidtabellskonflikter, så de är värda att granska även
när importen lyckas. Meddelandena visar oftast importproblem med XPLN-filer.

En lyckad import blir den aktiva planen, och sammanfattningen visar antalet tåg, sträckor
och stationer som lästs in.
