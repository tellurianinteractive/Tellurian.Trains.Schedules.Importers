Fanen **Strækninger** definerer, hvordan driftssteder er forbundet, og hvilke forbindelser der udgør
hver køreplan. Den har tre underafsnit, som du arbejder igennem i rækkefølge.

### Banestrækninger

En **banestrækning** er linjen mellem to tilstødende driftssteder, med afstand, antal
spor, hastighed og køretid. Banestrækninger beskriver det fysiske net og giver de
afstande, der bruges til at kontrollere toghastigheder og til at tegne den grafiske køreplan.

En banestrækning har en **retning** (fra et sted til det næste). Alle banestrækninger skal
defineres i samme retning, så et tog kan køre langs dem uden utilsigtede retningsskift. Med for eksempel
`A→B B→C D→C C→E E→F E→G` kan et tog køre `A→G`, `D→G`, `A→F` og `D→F` uden at
skifte retning; `A→D` eller `F→G` kræver et retningsskift, hvor linjerne mødes. Hvis du definerer en
strækning den forkerte vej (så to strækninger peger mod hinanden eller danner en sløjfe), viser en advarsel
de strækninger, der er uenige – ret det ved at redigere en af dem, så alle løber samme vej.

En banestrækning, der indgår i en køreplansstrækning, kan ikke slettes, før den er fjernet fra
den køreplansstrækning.

I en plan lavet ud fra en hal-plan, hvor modulerne er sat sammen i linjer, laves banestrækningerne
ud fra linjerne: længden er modulernes længder fra midten af ét driftssted til det næste, og
hastigheden er modulernes hastighedsgrænser, hver vægtet efter sit moduls længde. Kolonnen
**Målt (m)** viser længden målt i hal-planen. En længde, hastighed eller tid, du ændrer, er din og
bevares, når en nyere version af hal-planen indlæses; **Brug målt** og **Brug modulernes hastighed**
får strækningen til at følge hal-planen igen. En banestrækning uden hastighed giver en advarsel;
togene kører der med deres egen hastighed, højst 200 km/t. En sådan strækning kan vendes ved at bytte dens driftssteder, men ikke flyttes til andre: modulernes rækkefølge ændres i hal-planen, i Modultegneren.

### Togledelsesstrækninger

En **togledelsesstrækning** løber mellem to steder, hvor tog gives fri – en **bemandet** station, en
**skyggestation** eller et sted, der **styres fra** en bemandet station – og går lige igennem alle
ubemandede steder imellem. Så `A→B` og `B→C` med ubemandet `B` danner én togledelsesstrækning `A→C`.

En forgrening, et krydsningsspor, ubemandet station eller et industriområde, der **styres fra** en bemandet station,
betjenes af den stations togleder, som var det dennes egen station. Den afslutter de togledelsesstrækninger,
der mødes dér, dens tog står på den styrende stations togekspeditionsliste, mærket med dens signatur, og
stationerne hinsides den er blandt dem, den styrende station ringer til. En **blokpost** – et signalstyret
sted, der hverken er en forgrening eller et sted, hvor tog kan krydse – forbliver en del af linjen.

Togledelsesstrækninger oprettes automatisk ud fra banestrækningerne: tryk på **Gendan ud fra
banestrækninger**, når du ændrer nettet eller ændrer, hvilke stationer der er bemandet. Markér en station
som bemandet eller som skyggestation på fanen **Driftssteder**. Kolonnen *Via* viser de
ubemandede steder, en togledelsesstrækning passerer.

#### Hvilke tog der må være på en togledelsesstrækning samtidig

Toglederne i de to ender aftaler, hvilke tog der må være på togledelsesstrækningen imellem dem, så
listen **Konflikter** bedømmer hver togledelsesstrækning som en helhed:

- Hvert **signalstyret sted** på den, for eksempel en blokpost, deler den i **strækningsafsnit**. Et
  afsnit rummer ét tog pr. spor ad gangen, uanset hvilken retning togene kører i. Ubemandede steder,
  som ingen styrer, deler den ikke.
- På **enkeltspor** kan tog i modsatte retninger kun mødes i enderne eller på et
  signalstyret sted markeret **Tog kan krydse?**. De kan aldrig være mellem to sådanne steder på samme tid,
  men tog i samme retning kan følge efter hinanden dér, ét pr. afsnit.
- Et tog, der passerer et signalstyret sted uden tid dér, optager hvert afsnit, det passerer,
  indtil det ankommer, da intet siger, hvornår det passerede.

Banestrækninger, der ikke indgår i nogen togledelsesstrækning, bedømmes i stedet én for én.

### Køreplansstrækninger

En **køreplansstrækning** er en ordnet række af sammenhængende banestrækninger, som du vil planlægge og
tegne sammen – typisk en linje fra en endestation til en anden. Køreplansstrækninger er det, du
vælger imellem på fanen **Grafisk køreplan**, og hver bliver til én graf.

For at opbygge en giver du den et nummer (og en valgfri beskrivelse) og tilføjer derefter banestrækninger til dens rute
én ad gangen. Kun banestrækninger, der fortsætter, hvor ruten for øjeblikket slutter, tilbydes, så
ruten altid forbliver sammenhængende. Editoren viser de resulterende stationer og den samlede afstand.

I en plan lavet ud fra en hal-plan foreslår **Foreslå fra linjerne** én køreplansstrækning for hver
af hal-planens linjer, hvor en sidelinje begynder ved det driftssted, den udgår fra. Intet tilføjes,
før du vælger **Tilføj**: køreplansstrækninger kan overlappe, og en lang linje deles ofte i kortere,
så du kan også lave dem selv.

### Anbefalet arbejdsrækkefølge

Definér banestrækningerne først, og grupper dem derefter i de køreplansstrækninger, du vil arbejde med.
