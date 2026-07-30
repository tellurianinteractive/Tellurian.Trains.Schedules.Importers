# Versjonsnyheter

## Version 0.3.1

- Avsnittet **Trekkraftenheter** på en togdelsside i heftet Førertjenester har nå overskriften
  sin på det valgte språket. Det var den eneste overskriften i heftet uten oversettelse, så
  avsnittet var ikke til å kjenne igjen som trekkraftenhetene.
- Trekkraftenheten skrives nå ut for hver togdel som har en. I planer importert med en tidligere
  versjon viste noen togdeler en trekkraftenhet under **Tjenester** men ingen i heftet.
- Merknader om tog i samme retning sier nå hvilket tog som kommer forbi det andre —
  **Kjører forbi GD 42757 12:02-12:05** eller **Blir forbikjørt av GD 42757 12:02** — i stedet for
  det tidligere *"Møter GD 42757 i samme retning"*, som aldri sa hvilket tog som kom foran. To tog
  som bare står på samme stasjon samtidig gir ingen merknad i det hele tatt, for ingen av dem har
  kommet forbi det andre.
- Et møte uten varighet — det andre toget kjører gjennom uten opphold — skrives som ett klokkeslett
  i stedet for et intervall fra et tidspunkt til seg selv.
- Et tog som begynner eller avslutter kjøringen sin på en stasjon, tas ikke lenger med som møtt,
  krysset eller forbikjørt der. Disse tidene er når lokføreren møter til tjeneste eller går av,
  ikke når toget kjører.

## Version 0.3.0

- En ny rapport, **Førertjenester**, skriver ut ett A5-hefte per tjeneste. Forsiden
  viser tjenestens nummer, hvilke økter eller dager den kjøres, dens start- og
  sluttid og -stasjoner, en vanskelighetsgrad, bemanningsbehov og eventuelle
  tjenestemerknader. Hver togdel får sin egen side, med hvilke trekkraftenheter som
  skal brukes, hvilke vognsett som skal tas med, og til hvilke destinasjoner
  godsvogner skal tas med, samt ruteplanen – hver vist i sin egen tydelig
  avgrensede blokk. Siste side i hvert hefte viser anleggets sporplan og en tabell
  over skiftestasjoner, for enkelt oppslag under kjøringen.
- En ny rapport, **Generelle instruksjoner**, er et eget trykt hefte med treffets
  program og instruksjoner som gjelder for et anlegg gjennom hele treffet. Her står
  treffarrangøren fritt til å skrive hva som helst – for eksempel
  kjøreinstruksjoner, signalgiving, radio-/telefonbruk, hva man gjør ved
  forsinkelser og hvem man spør – og det deles ut én gang til alle.
- Både programmet og instruksjonene skrives under **Innstillinger › Informasjon** og
  kan formateres med Markdown – overskrifter, lister, fet og kursiv – slik at også en
  lang instruksjonstekst blir lesbar på trykk.
- Heftet innledes med treffets navn, hvilke datoer det gjelder, og utskriftsdatoen, fulgt
  av programmet: øktenes tider, pauser og måltider – det hver deltaker trenger å vite
  før den første økten.
- Instruksjonene følger deretter over så mange sider som de trenger. Det brytes side
  mellom avsnitt, og en overskrift holdes alltid sammen med teksten den innleder.
- Siste side viser anleggets sporplan og tabellen over skiftestasjoner, slik at også de
  som aldri holder et tjenestehefte – først og fremst stasjonspersonalet – får en
  oversikt over anlegget.
- Heftet skrives ut i samme A5-format som tjenesteheftene: A4 liggende, tosidig,
  brettet på midten, med tomme sider lagt til der det trengs slik at arkene brettes
  riktig.
- Tjenester kan nå graderes **Lett**, **Middels** eller **Erfaren**, vist
  fargekodet på heftet, slik at en deltaker kan velge en tjeneste som passer
  erfaringen deres.
- En tjeneste kan nå angi at den trenger to eller tre personer – for eksempel en
  lokfører og en konduktør – og dette vises på heftet.
- En tjeneste kan festes til et **fast nummer** slik at automatisk omnummerering
  lar den være urørt, for eksempel spesielle tjenester som deles ut før en økt
  starter.
- Planen kontrolleres nå også slik at hver togdel med lokomotiv eller togsett
  tildelt har en førertjeneste som dekker den i hver økt den kjøres – en del ingen
  er satt opp til å kjøre, rapporteres økt for økt. En tjeneste med fast nummer
  kontrolleres også: den må ha et nummer, og ingen to tjenester med fast nummer
  kan få samme nummer.
- Selskaper kan nå ha en opplastet **logo**, vist i rapporter i stedet for
  tekstsignaturen.
- Stasjoner kan nå merkes som den **skiftestasjonen** som betjener en annen
  stasjons lokalgods; anlegget lister automatisk opp hver skiftestasjon og hva den
  dekker, vist på tjenesteheftets siste side. Dette hjelper stasjonspersonale og
  godstogførere med å vite hvor vogner med en gitt godsdestinasjon skal sendes.
- Hver ruteplanstrekning kan nå gis en **farge**, brukt til å tegne den i
  Topologi-diagrammet.
- En ny **avstandsfaktor** (under Innstillinger › Tid & hastighet) lar et anlegg
  vise et annet – typisk større, mer forbildetro – kilometertall i rapporter og
  den grafiske ruteplanen enn avstanden som faktisk er modellert, uten at det
  påvirker noen kjøretidsberegning.
- Appen holder nå flere åpne nettleserfaner eller -vinduer synkronisert med
  hverandre. **Merk** at dette bare fungerer mellom vinduer på samme maskin i
  samme nettleser.
- Innstillinger kan nå lagre treffets **gjelder fra**- og **gjelder til**-datoer,
  skrevet ut som en gyldighetslinje på rapporter; la dem stå tomme hvis ikke noe
  treff er booket ennå.
- En ny innstilling, **utvid plantider automatisk?** (under Innstillinger ›
  Generelt), utvider planens start- eller sluttid for å dekke et tog i stedet
  for å blokkere endringen når togets egen tid faller utenfor den. Av som
  standard.
- En ny knapp, **oppdater alle tider**, i den grafiske ruteplanen beregner alle
  tog i ruteplanen på nytt samtidig, i stedet for å måtte velge en delmengde
  først.
- Sporbelegningskontrollen kan nå valgfritt ta hensyn til at et lokomotiv eller
  togsett står på et spor mellom to tog, med mindre det er booket til eller fra
  hensetting (under Innstillinger › Validering). Av som standard, siden det bare
  gir mening på anlegg der hensetting er modellert bevisst – slå den på der for å
  oppdage et tredje tog som i det stille bruker et spor et stillestående kjøretøy
  allerede opptar.
- Hvert opphold i fanen **Tog** har nå et felt for **Merknad** – en merknad som skrives ut
  ved det oppholdet, for eksempel «vent på møtende tog». Merknaden vises ferdig formatert
  og bytter til den rå oppmerkingen så snart du går inn i feltet, slik at du kan utheve det
  som betyr noe: skriv `*sakte*` for kursiv og `**første**` for fet. Tømmer du feltet,
  fjernes merknaden igjen.

### Feilrettinger

- Når man legger til et nytt tog, settes nå standard starttid under hensyn til
  den angitte forberedelsestiden, slik at det ikke starter før planens starttid.

## Version 0.2.4

- En ny fane **Tjenester** lar deg planlegge førertjenester – arbeidet en lokfører utfører
  i løpet av en økt, som en rekke av togdelene føreren kjører. Hver tjeneste er en rad:
  betegnelse, selskap og økter til venstre, togdelene i kjørerekkefølge til høyre.
- Legg til togdelene en fører kjører med **Legg til togdel**. Listen viser
  trekkraftstrekningene en fører kan ta som det neste – de som ikke kolliderer i tid med
  tjenesten, og, når den har en togdel, de som avgår ved eller etter at den ankommer.
  Togdelene trenger ikke starte på samme stasjon: mellom to togdeler går føreren rett og
  slett dit den neste starter.
- Den samme togdelen kan kjøres av flere tjenester så lenge de kjører i forskjellige
  økter, så én tjeneste kan dekke oddetallsøktene og en annen partallsøktene.
- Der to togdeler for samme tog i en tjeneste kjøres av forskjellige trekkraftenheter,
  viser fanen nå en merknad ved stasjonen der trekkraftenheten byttes – du skriver den
  ikke inn for hånd.
- Du kan gi hver tjeneste en betegnelse og et selskap, velge øktene den kjøres, og legge
  til frie merknader som gjelder hele tjenesten.
- Tjenester importert fra XPLN deler nå togdelene som er definert i kjøretøyenes turnuser,
  så hver togdel viser trekkraftenheten som kjører den.
- Planen kontrolleres slik at ingen togdel kjøres av to tjenester i samme økt og ingen
  tjeneste har togdeler som overlapper i tid; eventuelle konflikter listes og åpnes på
  fanen **Tjenester**. Du kan slå kontrollen på eller av under **Innstillinger ›
  Validering**.

## Version 0.2.2

### Feilrettinger

- To tog som aldri kjører i samme driftsøkt, rapporteres ikke lenger som et møte på en
  enkeltsporet strekning. Et tog som kjører økt 1, 3, 5, og ett som kjører 2, 4, 6, kan
  nå dele samme spor uten en falsk advarsel, fordi de aldri er ute samtidig.
- Konfliktkontrollen på dobbeltsporede (og flersporede) strekninger er nå presis: en
  strekning merkes bare når det er flere tog på den samtidig enn den har spor, og bare
  tog som kjører i en felles økt telles med.

## Version 0.2.1

- Konfliktvarsler vises nå der du kan rette dem. Togkonflikter vises bare i den
  grafiske ruteplanen og på fanen **Tog**; kjøretøy- og omløpskonflikter vises bare
  på fanen **Omløp**.
- På fanen **Omløp** fremhever en kjøretøykonflikt nå bare det aktuelle kjøretøyet,
  og en omløpskonflikt fremhever bare det aktuelle omløpet, slik at det er tydelig
  hva som krever oppmerksomhet.
- Kontrollen av at et kjøretøy vender tilbake til utgangspunktet, omfatter nå også
  vognsett og gods, ikke bare lok og togsett, slik at et vognsett eller gods som blir
  stående på feil sted ved slutten av driftsøkten, nå rapporteres.

## Version 0.2.0

- Navnet på planen du arbeider med, vises nå øverst i vinduet, slik at du alltid
  ser hvilket dokument som er åpent.
- Den grafiske ruteplanen viser nå søyler for lokomotivførerbehovet, noe som gjør
  det lettere å se hvor mange førere som trengs gjennom driftsøkten.
- En ny **Topologi**-visning (under fanen **Strekninger**) viser et skjematisk
  diagram over ruteplanens strekninger og deres greiner.

### Feilrettinger

- Strekninger beholder nå rekkefølgen du la dem inn i som standard, slik at listen
  er lettere å følge når du kontrollerer det du har lagt inn. Du kan fortsatt sortere
  på hvilken som helst kolonne.
- Konflikter viser ikke lenger til tog du ikke finner: når et tog slettes, fjernes
  stoppene sammen med det, slik at ingen foreldreløse stopp eller falske konflikter
  blir igjen.

## Version 0.1.0

Første forhåndsvisning av Ruteplanleggeren. Du kan:

- Definere sporplaner med stasjoner, spor og strekninger.
- Opprette og redigere togruteplaner med automatisk tidsberegning.
- Tildele lokomotiver og togsett til tog.
- Bygge kjøretøysomløp og skrive ut omløpskort.
- Planlegge godsstrømmer mellom stasjoner.
- Vise grafiske ruteplaner (tid-avstands-diagrammer).
- Validere ruteplaner for konflikter og inkonsistenser.
- Generere utskrifter: togkort, stasjonsbøker og vaktplaner.
- Arbeide på engelsk, tysk, dansk, norsk og svensk.
