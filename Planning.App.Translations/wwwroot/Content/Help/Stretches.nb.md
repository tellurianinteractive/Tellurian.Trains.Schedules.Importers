Fanen **Strekninger** definerer hvordan driftssteder henger sammen, og hvilke forbindelser som utgjør
hver ruteplan. Den har tre underavsnitt, som du arbeider deg gjennom i rekkefølge.

### Banestrekninger

En **banestrekning** er linjen mellom to tilstøtende driftssteder, med avstand, antall
spor, hastighet og kjøretid. Banestrekninger beskriver det fysiske nettet og gir
avstandene som brukes til å kontrollere toghastigheter og til å tegne den grafiske ruteplanen.

En banestrekning har en **retning** (fra ett sted til det neste). Alle banestrekninger må
defineres i samme retning slik at et tog kan kjøre langs dem uten utilsiktede retningsskifter. Med for eksempel
`A→B B→C D→C C→E E→F E→G` kan et tog kjøre `A→G`, `D→G`, `A→F` og `D→F` uten å
skifte retning; `A→D` eller `F→G` krever et retningsskifte der linjene møtes. Hvis du definerer en
strekning feil vei (slik at to strekninger peker mot hverandre eller danner en sløyfe), lister en advarsel opp
strekningene som er uenige – rett det ved å redigere én av dem slik at alle går samme vei.

En banestrekning som inngår i en ruteplanstrekning, kan ikke slettes før den er fjernet fra
den ruteplanstrekningen.

### Togledelsesstrekninger

En **togledelsesstrekning** går mellom to steder der tog klareres – en **bemannet** stasjon, en
**skyggestasjon** eller et sted som **styres fra** en bemannet stasjon – og går rett gjennom alle
ubemannede steder imellom. Så `A→B` og `B→C` med ubemannet `B` danner én togledelsesstrekning `A→C`.

En sporforgrening, et kryssingsspor, en ubemannet stasjon eller et industriområde som **styres fra** en bemannet stasjon,
betjenes av den stasjonens togekspeditør som om det var dens egen stasjon. Det avslutter togledelsesstrekningene som
møtes der, togene står på den styrende stasjonens togekspederingsliste, merket med dens signatur, og
stasjonene hinsides det er blant dem den styrende stasjonen ringer til. En **blokkpost** – et signalstyrt
sted som verken er en forgrening eller et sted der tog kan krysse – er fortsatt en del av linjen.

Togledelsesstrekninger genereres automatisk fra banestrekningene: trykk **Generer på nytt fra
banestrekninger** hver gang du endrer nettet eller endrer hvilke stasjoner som er bemannet. Merk en stasjon
som bemannet eller som skyggestasjon på fanen **Driftssteder**. Kolonnen *Via* viser de
ubemannede stedene en togledelsesstrekning går gjennom.

#### Hvilke tog som kan være på en togledelsesstrekning samtidig

Togekspeditørene i de to endene blir enige om hvilke tog som kan være på togledelsesstrekningen mellom dem, så
listen **Konflikter** bedømmer hver togledelsesstrekning som en helhet:

- Hvert **signalstyrte sted** på den, for eksempel en blokkpost, deler den i **delstrekninger**. En
  delstrekning rommer ett tog per spor om gangen, uansett hvilken retning togene går i. Ubemannede steder
  som ingen styrer, deler den ikke.
- På **enkeltspor** kan tog i motsatt retning bare møtes i endene eller på et
  signalstyrt sted merket **Tog kan krysse?**. De kan aldri være mellom to slike steder samtidig,
  men tog i samme retning kan følge hverandre der, ett per delstrekning.
- Et tog som passerer et signalstyrt sted uten tid der, opptar hver delstrekning det passerer
  helt til det ankommer, siden ingenting sier når det passerte.

Banestrekninger som ikke inngår i noen togledelsesstrekning, bedømmes i stedet én for én.

### Ruteplanstrekninger

En **ruteplanstrekning** er en ordnet rekke av sammenhengende banestrekninger som du vil planlegge og
tegne sammen – vanligvis en linje fra én endestasjon til en annen. Ruteplanstrekninger er det du
velger blant på fanen **Grafisk ruteplan**, og hver blir én graf.

For å bygge én gir du den et nummer (og en valgfri beskrivelse) og legger deretter til banestrekninger i traséen
én om gangen. Bare banestrekninger som fortsetter der traséen for øyeblikket slutter, tilbys, slik at
traséen alltid forblir sammenhengende. Editoren viser de resulterende stasjonene og den samlede avstanden.

### Anbefalt arbeidsrekkefølge

Definer banestrekningene først, og grupper dem deretter i ruteplanstrekningene du vil arbeide med.
