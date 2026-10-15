Fanen **Tog** er stedet der du bygger tog og tidene deres.

Et tog kjører langs en strekning og har opphold ved driftssteder underveis. For hvert tog
angir du nummer og kategori samt **stasjonsoppholdene** – stedene det passerer eller stopper ved, med
ankomst- og avgangstider. Standardtidene for stasjoner fra **Innstillinger › Tid & hastighet**
brukes som utgangspunkt og kan overstyres per opphold.

Tog som tegnes her, vises som linjer i den **Grafiske ruteplanen**, farget etter kategori, og
kontrolleres mot valideringsreglene i **Innstillinger › Validering** (hastigheter, sporbruk, tognumre
og så videre).

## Per tog eller per driftssted

Fanen har to visninger av de samme oppholdene:

- **Per tog** lister togene etter kategori. Åpne et tog for å redigere oppholdene, ett tog om gangen.
- **Per driftssted** lister hvert tog som har opphold ved ett sted, i den rekkefølgen de kommer
  dit: om det begynner, slutter, stopper eller passerer, hvor det kommer fra og hvor det går videre, tidene og
  sporet det bruker. Velg et annet spor i listen for å flytte toget dit. Det er visningen for å
  få orden på sporene på en travel stasjon, der visningen per tog ville kreve å åpne hvert tog som har opphold
  der. En konflikt på stedet, for eksempel to tog på ett spor samtidig, er markert på raden.

## Hva gjør et tog unikt i planen?

Tog identifiseres ved **tognummeret** sitt.
Disse numrene må være unike innenfor en plan.
Hvis du angir et trafikkselskap,
enten på **togkategorien** eller på bestemte tog,
må et tognummer være unikt innenfor
det selskapet. Så du kan ha **DB 123** og **DSB 123**,
og de er to ulike tog.

## Start- og sluttidspunkt for arbeidet med et tog
To tidspunkter har en spesiell betydning slik at planleggeren kan angi forberedelses- og avslutningstid:
- **Første ankomst** er starttidspunktet for forberedelsestiden.
  En fører begynner arbeidet med et tog på dette tidspunktet. Det er tiden til å hente et lokomotiv fra oppstilling
  eller kontrollere at alle vognkort stemmer med vognene i toget, og så videre.
- **Siste avgang** er sluttidspunktet for avslutningen.
  En fører avslutter arbeidet med et tog på dette tidspunktet. Det er tiden til å kjøre lokomotivet til oppstilling, og så videre.

## Redigere en oppholdstid

En tid endres aldri for seg selv – resten av toget følger med og beholder kjøre- og
oppholdstidene det allerede har. En **avgang** virker framover, den veien toget går: la et tog stå fem
minutter lenger, så når det hvert senere sted fem minutter senere. En **ankomst** virker bakover: be
toget ankomme fem minutter senere, så forlater det hvert tidligere sted fem minutter senere. Tidene
på den andre siden av oppholdet du redigerer, blir stående, så det som endres ved oppholdet, er hvor
lenge toget står der. En endring som ville ført toget utenfor planens driftstider,
avvises som helhet, og feltet går tilbake til tiden som allerede er lagret.

## Opphold og gjennomkjøringer

Feltene **Ank** og **Avg** på et opphold sier om toget stopper der. Kryss av ingen, og det kjører
gjennom. To ting tar valget ut av hendene dine:

- **Der toget ikke kan stoppe i det hele tatt**, er begge feltene tømt og deaktivert. Det gjelder ethvert
  signalstyrt sted og ethvert sted som ikke utveksler noe toget fører – et persontog
  trenger at **Passasjerer** er avkrysset for stedet, et godstog **Gods** (se **Driftssteder**).
  En skyggestasjon utveksler alltid begge. Når utvekslingen slås på igjen, kommer ethvert opphold
  du planla tidligere, tilbake, så ingenting går tapt ved å prøve.
- **Der noe avhenger av oppholdet**, er feltet avkrysset og deaktivert. Et togavsnitt går fra et
  opphold der toget avgår til et opphold der det ankommer, så begge endene må være opphold: togets eget første
  og siste opphold og endene av hvert avsnitt et kjøretøyomløp, en førertjeneste eller en godsstrøm
  er planlagt over. For å frigjøre feltet flytter du oppholdet eller avsnittet som ender der.

## Merknader

**Merknad** på et opphold er et notat fra deg selv, skrevet ut til lokføreren og stasjonspersonalet
sammen med merknadene planen regner ut selv. Bruk *kursiv* og **fet** for utheving; kodingen
vises når du redigerer merknaden.

Rapportene skriver ut et opphold sin ankomst og avgang som separate linjer, så en merknad må si hvilken
av de to den hører til. Feltet ved siden av merknaden er der du velger: **Ank** for noe som møtes eller
gjøres ved innkjøring, **Avg** for noe som gjøres før eller ved avgang. Der toget bare ankommer eller
bare avgår, er den halvdelen den eneste som tilbys. Der det kjører gjennom, kan ingen merknad skrives
i det hele tatt – kryss først av **Ank** eller **Avg**, så finnes det et sted å vise én.

## Dialogen Legg til tog

Det skal være enkelt å legge til tog.
Du legger til tog i dialogen **Legg til tog**. Det finnes flere alternativer:
- Opprett ett enkelt tog.
- Opprett ett enkelt tog med et returtog i motsatt retning.
- Opprett et antall gjentatte tog med et gitt intervall mellom dem.
- Kombinasjon av returtog og gjentatte tog.

Et returtog kjører samme trasé tilbake igjen, med samme kategori og hastighet, og får det neste
ledige tognummeret i motsatt retning. Avgangen er enten **så snart som mulig** – det
første togets ankomst pluss tiden til å avslutte det og forberede returen – eller et tidspunkt du skriver inn,
som kan ligge både før og etter det første togets egen avgang. Kombinert med gjentatte tog
opprettes begge retninger først og gjentas deretter, så de to retningene nummereres som par.

Det mislykkes å opprette tog hvis noen del av et tog faller utenfor planens
start- og sluttidspunkt. Men i **Innstillinger** kan du la planens start- og sluttidspunkter
utvides når tog legges til.

## Trafikkerte kjøresesjoner eller dager
Planleggingen av tog kan bruke muligheten til å angi hvilke kjøresesjoner (eller ukedager) et tog går.
Standard er alltid **Alle/Daglig**. Du definerer de ulike kjøresesjonsmønstrene i **Innstillinger › Kjøresesjoner & dager**.
Det finnes vanlige forhåndsdefinerte mønstre, men du kan legge til egne.
