På fliken **Inställningar** styr du hur den aktuella banans
tidtabell beräknas, ritas och valideras.
Inställningarna sparas i planeringsdokumentet.

### Allmänt

Ge banan ett namn och välj dess trafikfönster.
Du kan välja att använda **numrerade köromgångar** eller **veckodagar** när du behöver
olika trafik under olika köromgångar. Om alla tåg, omlopp och tjänster
går likadant alla köromgångar behöver du inte göra något – allt gäller då som standard **alla köromgångar**.

**Använd resandebiljetter** innebär att träffen körs med biljetter: en resande köper en där
resan börjar och reser med den. När det är aktiverat kan du på fliken **Driftplatser** markera de
stationer där resande byter tåg. Dessa omstigningar visas sedan som en ankomstanmärkning i
förartjänsterna och kan listas i häftet med allmänna instruktioner.

Rapporter skrivs ut på banans **standardspråk**: det första språket i dess standardland,
oavsett vilket språk du själv arbetar på. **Skriv ut rapporter på lokala språk** skriver i stället ut varje förartjänst på språket
hos det företag som kör den, varje omloppskort på språket hos fordonets företag och varje stations
tågklareringslista på språket i stationens land.
Allt som saknar ett sådant språk behåller standardspråket.

### Köromgångar & dagar

I vissa planeringsfall är det användbart att kunna välja vilka köromgångar/dagar som trafikeras.
Du kan ange trafikerade köromgångar/dagar för:
- Tåg, till exempel ett tåg som går köromgång 1–3 och ett annat som går 4–6.
- Fordon, till exempel två eller tre dagars rotation av lok.
- Tjänster, när uppgifterna skiljer sig mellan köromgångar.

### Tid & hastighet

Ange snabbklockans fart och koppla hastighetsklasser till skala och verkliga hastigheter. Standardtiderna för
stationer (kortaste uppehåll, lokomkörning, tågklarering) används när tider beräknas för
stationer som inte har egna värden.

### Validering

Välj vilka kontroller som ska köras och vilka tröskelvärden de använder. Stänger du av en kontroll döljs dess
meddelanden utan att underliggande data tas bort.

### Grafisk tidtabell

**Gemensamma inställningar** styr vad grafen visar – tidsaxelns orientering och vilka
etiketter (ankomst-/avgångsminuter, tågkategori, företag) som ritas. De gäller både på skärmen
och i den utskrivna rapporten. Observera att om tåg fortsätter efter 24:00 viker de runt och fortsätter från 00:00.

Under dem innehåller blocken **på skärmen** och **utskriven rapport** samma slags avstånd för
vardera: skärmavstånd anges i pixlar, utskriftsavstånd i millimeter på papperet. Utskriftsmåtten är i skala,
så utskrivna grafer kan jämföras och mätas; antalet sidor följer av dem.

### Integration

*Kommer*: API-nycklar och inställningar för anslutning till externa tjänster, till exempel Module Registry.
