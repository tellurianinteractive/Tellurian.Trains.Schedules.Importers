Du bruger fanen **Indstillinger** til at styre, hvordan det aktuelle anlægs
køreplan beregnes, tegnes og valideres.
Indstillingerne gemmes i planlægningsdokumentet.

### Generelt

Giv anlægget et navn, og vælg dets driftsvindue.
Du kan vælge at bruge **nummererede køresessioner** eller **ugedage**, når du har brug for
forskellig drift på forskellige køresessioner. Hvis alle tog, omløb og tjenester
kører ens alle køresessioner, behøver du ikke gøre noget – alt gælder som standard **alle køresessioner**.

**Brug passagerbilletter** betyder, at træffet køres med billetter: en passager køber en, hvor
rejsen begynder, og rejser på den. Når den er slået til, kan du på fanen **Driftssteder** markere de
stationer, hvor passagerer skifter tog. Disse omstigninger vises derefter som en ankomstnote i
førertjenesterne og kan opføres i hæftet med generelle instruktioner.

Rapporter udskrives på anlæggets **standardsprog**: det første sprog i dets standardland,
uanset hvilket sprog du selv arbejder på. **Udskriv rapporter på lokale sprog** udskriver i stedet hver førertjeneste på sproget
hos det selskab, der kører den, hvert omløbskort på sproget hos køretøjets selskab og hver stations
togekspeditionsliste på sproget i stationens land.
Alt, der ikke har et sådant sprog, beholder standardsproget.

### Køresessioner & dage

I nogle planlægningsscenarier er det nyttigt at kunne vælge, hvilke køresessioner/dage der køres.
Du kan angive kørende køresessioner/dage for:
- Tog, for eksempel ét tog, der kører køresession 1–3, og et andet, der kører 4–6.
- Køretøjer, for eksempel to eller tre dages rotation af lokomotiver.
- Tjenester, når opgaverne er forskellige mellem køresessioner.

### Tid & hastighed

Angiv hurtigurets hastighed, og tildel hastighedsklasser til skala og reelle hastigheder. Standardtiderne for
stationer (mindste ophold, lokomotivomløb, togfrigivelse) bruges ved beregning af tider for
stationer, der ikke har egne værdier.

### Validering

Vælg, hvilke kontroller der skal køre, og hvilke tærskler de bruger. Slår du en kontrol fra, undertrykkes dens
meddelelser uden at de underliggende data fjernes.

### Grafisk køreplan

**Fælles indstillinger** styrer, hvad grafen viser – tidsaksens orientering og hvilke
etiketter (ankomst-/afgangsminutter, togkategori, selskab) der tegnes. De gælder både på skærmen
og i den udskrevne rapport. Bemærk, at hvis tog fortsætter efter 24:00, ombrydes de og fortsætter fra 00:00.

Under dem rummer blokkene **på skærmen** og **udskrevet rapport** de samme slags afstande for
hver af de to: skærmafstand angives i pixel, udskriftsafstand i millimeter papir. Udskriftsmålene er i skala,
så udskrevne grafer kan sammenlignes og måles; antallet af sider følger af dem.

### Integration

*Kommer*: API-nøgler og indstillinger til forbindelse til eksterne tjenester, såsom Module Registry.
