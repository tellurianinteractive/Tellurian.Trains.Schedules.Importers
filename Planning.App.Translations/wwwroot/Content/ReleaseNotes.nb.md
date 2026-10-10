# Versjonsnyheter

## Versjon 0.8.5

### Endringer

- **Bekreftelser vises midt i vinduet.** Spørsmålet før noe slettes, eller før en ny bane erstatter den
  nåværende, åpnes nå i en boks midt på skjermen, så du slipper å rulle for å finne den. Trykk **Esc** for
  å avbryte. Et klikk utenfor boksen avbryter også en enkel sletting, men ikke det utfyllende spørsmålet
  ved sletting av et driftssted eller spørsmålet ved utskifting av banen.

## Versjon 0.8.4

### Nye funksjoner

- **Skiftelok kan stasjoneres på et driftssted.** Under fanen **Driftssteder** åpner du en stasjon, et
  industriområde eller et annet driftssted med **Rediger** og klikker på **Legg til skiftelok**. Et
  skiftelok står der for den skiftingen stedet trenger: det går ikke i noe omløp og settes aldri på et tog.
  Det vises sammen med det øvrige rullende materiellet under **Kjøretøyseiere**, der eier, DCC-adresse og
  kjøretøynummer angis som for et hvilket som helst lokomotiv, og rapporten over medbrakte kjøretøy skriver
  det ut på siden for stasjonen der det er stasjonert.

## Versjon 0.8.3

### Endringer

- **Rapporten Medbrakte kjøretøy er ordnet etter hvordan hver side brukes.** Ordnet **Per eier** viser en
  deltakers side det vedkommende tar med etter den første kjøresesjonen eller dagen det trengs, deretter
  etter stasjon, avgang og spor. Ordnet **Per driftssted** viser en stasjons side kjøretøyene etter
  spornummer, spor 2 før spor 10, og på hvert spor etter avgang.
- **Radenes farger er enklere.** På en eiers side er bare et kjøretøy som ikke trengs den første
  kjøresesjonen eller dagen, lysegult. På en stasjons side er et kjøretøy som ikke er i drift alle
  kjøresesjoner eller dager, lysegult når det starter en odde kjøresesjon eller dag, og lyseblått når det
  starter en jevn. Reserver er fortsatt lysegrå.
- **Lok og togsett kan få et kjøretøynummer.** Under **Kjøretøyseiere**, **Rullende materiell**, kan hver
  eier av et lok eller et togsett skrive inn nummeret på kjøretøyet de tar med, også reservene, ved siden av
  DCC-adressen. Rapporten Medbrakte kjøretøy viser det rett etter omløpet. Et vognsetts numre er fortsatt
  vognenes.

## Versjon 0.8.2

### Endringer

- **Togene i en kategori kan nummereres om.** Endre **Startnummer** på fanen **Togkategorier**, og klikk
  på **Nummerer om** ved siden av. Alle tog i kategorien flyttes like mye, slik at toget med lavest nummer
  får det første nummeret på eller over startnummeret. Ingen tog bytter mellom odde og like, og hull og
  par av numre beholdes: med startnummer 100 blir tog 1, 2 og 3 til 101, 102 og 103. En skiftekategori har
  ingen odde og like numre, så den første oppgaven får selve startnummeret.
- **En enhet som hentes fra hensetting før sitt første tog, skal settes til hensetting etter sitt
  siste.** Der et lok eller et togsett hentes fra hensetting, eller løftes på, før sitt første tog i en
  kjøresesjon, viser valideringen nå en advarsel, med mindre det også settes til hensetting, eller løftes av,
  etter sitt siste tog der det kommer tilbake til den stasjonen — i samme kjøresesjon, eller i en senere
  når det går i omløp over flere kjøresesjoner.

## Versjon 0.8.1

### Endringer

- **Et lok eller et togsett kan settes til hensetting eller løftes av mellom to tog.** Når du redigerer
  et togavsnitt i et vognløp, angir **Trekkraft før avgang** og **Trekkraft etter ankomst** hvor
  trekkraften er: på sporet, på hensetting, eller løftet av anlegget, for eksempel opp på et bord under en
  lang ventetid. Lokfører og togekspeditør får en merknad om å kjøre den til eller fra hensetting, eller å
  løfte den av eller på. Valget settes også på togavsnittet før eller etter, slik at en avløftet enhet
  løftes på igjen før neste tog. Der de to ikke stemmer overens, viser valideringen en advarsel.
- **Hent fra og Sett på holdes i takt med togavsnittet før eller etter.** Velger du sporet kjøretøyene
  settes på etter ankomsten, henter neste togavsnitt dem fra det sporet, og omvendt. Der et spor er angitt
  i den ene enden, men ikke møtes i den andre, viser valideringen en advarsel.
- **Merknadene om å hente fra eller sette på et spor sier hva sporet brukes til.** Et spor med en bruk på
  fanen **Driftssteder** nevnes med den, som i «spor 5 (Lokstall)». Et spor uten nummer nevnes bare med
  bruken sin.

## Versjon 0.8.0

### Endringer

- **Fanen Tog kan vise anropene ved ett driftssted.** På fanen **Tog** kan det nå velges mellom
  **Per tog** og **Per driftssted**. Per driftssted viser alle tog som anroper det valgte driftsstedet, i
  den rekkefølgen de kommer dit: om toget starter, slutter, stopper eller kjører gjennom, hvor det kommer
  fra og fortsetter til, tidene og sporet. Velg et annet spor i listen for å flytte toget dit, uten å åpne
  hvert tog for seg. En konflikt ved driftsstedet, for eksempel to tog på samme spor samtidig, markeres på
  raden. Fanen husker visningen og driftsstedet du valgte.
- **Et driftssted som tog anroper, kan slettes.** På fanen **Driftssteder** er **Slett** ikke lenger
  sperret mens tog anroper der. I stedet får du først se alt slettingen ville endre, og du bekrefter eller
  avbryter. Togene mister anropene sine der: et tog som kjører gjennom, kjører nå rett forbi, og et som
  starter eller slutter der, starter eller slutter nå ved neste eller forrige anrop. Der driftsstedet
  ligger mellom nøyaktig to naboer, erstattes de to sporstrekningene av én som forbinder naboene, like lang
  og med samme kjøretid som de to til sammen, og ruteplanstrekningene gjennom det går over den i stedet.
  Slettingen avvises, med årsakene listet, så lenge en omløpsplan, en lokførertjeneste eller en godsstrøm
  starter eller slutter der, eller så lenge et tog snur der eller kjører gjennom det som et
  forgreningspunkt.

### Rettelser

- **En nedtrekksliste viser ikke lenger et annet valg enn det som ble gjort.** Når valgene i en liste
  endret seg, men den valgte verdien ikke gjorde det, kunne listen vise en annen oppføring enn den lagrede.

## Versjon 0.7.9

### Endringer

- **Togsammensetninger angir videresendte vogner etter hvor de kommer fra.** En godsstrøm med
  **Opprinnelsesdriftssteder** vises nå som "Vogner fra" opprinnelsene sine i rektangelet sitt i stedet for
  med destinasjonene sine, slik at vognene kan finnes etter hvor de kommer fra. Andre godsstrømmer på samme
  plass i toget viser fortsatt destinasjonene sine, og begrensningen teller fortsatt med alle.

## Versjon 0.7.8

### Endringer

- **Det varsles om et tog som ankommer til eller går fra et spor som ikke er planlagt.** Et tog med
  ankomst eller avgang på et spor der feltet **Planlagt?** ikke er avkrysset på fanen **Driftssteder**,
  vises nå under **Konflikter**, med tog, driftssted, tid og spor. Et tog som bare venter på et slikt
  spor, for eksempel på en kryssing, vises ikke, og heller ikke et skifteoppdrag. Ingenting endres for
  deg: flytt enten stoppet til et planlagt spor på fanen **Tog**, eller kryss av i sporets felt
  **Planlagt?**. Kontrollen kan slås av under **Innstillinger › Validering**.

## Versjon 0.7.7

### Endringer

- **Kjøretøy som ikke er i drift, kan slettes.** På fanen **Kjøretøyseiere** har et kjøretøy som vises som
  **Ikke i drift**, en knapp **Slett**, og **Slett dem som ikke er i drift** fjerner alle slike viste
  kjøretøy — bare de viste når **Bare de uten eier?** er krysset av. Begge spør først. Kjøretøyets eiere
  fjernes sammen med det; deltakerne blir. Et kjøretøy med oppgaver må først tas ut av omløpene sine på
  fanen **Omløp**.
- **Rapporten Medbrakte kjøretøy benevner hvert kjøretøy slik fanen Omløp gjør.** Den separate kolonnen
  **Klasse** er borte: kolonnen **Omløp** har nå operatørens signatur, nummer og klasse (f.eks. "SJ 01 Rc"),
  eller det eksterne id-et for et kjøretøy som har et, som på fanen **Omløp**.

## Versjon 0.7.6

### Endringer

- **Listen over kjøretøyseiere benevner hvert kjøretøy slik fanen Omløp gjør.** På fanen **Kjøretøyseiere**
  er de separate kolonnene **Kjøretøy** og **Klasse** nå én kolonne **Kjøretøy** med operatørens signatur,
  nummer og klasse (f.eks. "DB 05 BR 218"), eller for et vognsett vognene det oppgir
  (f.eks. "SJ 05 5 x A/B/Fv"). Et kjøretøy med eksternt id vises med denne id-en, som på fanen **Omløp**.
- **Rapporten Medbrakte kjøretøy tegner vognene i et vognsett.** Et vognsett som oppgir vognene sine, viser
  dem nå i merknaden, et rektangel per vogn med klasse og nummer, i den rekkefølgen de står i toget, slik
  rapporten **Togsammensetninger** tegner dem. Et langt vognsett fortsetter på flere linjer.

## Versjon 0.7.5

### Endringer

- **En ny tjeneste begynner med sitt første togavsnitt.** **Ny tjeneste** på fanen **Tjenester** åpner nå
  dialogen **Legg til togavsnitt** med en gang, slik at tjenesten straks får sin plass i diagrammet i stedet
  for å stå tom nederst. Lukkes dialogen uten at et avsnitt legges til, blir ingen tom tjeneste liggende
  igjen.
- **Legg til tog tilbyr bare steder der kategorien stopper.** I dialogen **Legg til tog** er fra- og
  tildriftsstedene begrenset til dem i den valgte togkategoriens stoppmønster. En kategori uten
  stoppmønster begrenser ingenting. Byttes kategorien, tømmes et driftssted som ikke lenger er blant valgene.
- **Et døgnåpent treff kan begynne når som helst på døgnet.** Når **Går over midnatt?** er krysset av på
  fanen **Innstillinger**, angir **Første kjøresesjon starter** tiden da den første kjøresesjonen eller
  dagen begynner. Hvert kjøretøy starter der det står på det tidspunktet: et togavsnitt i den første
  kjøresesjonen som går tidligere, er ikke der det starter, og et kjøretøy uten noe senere i den
  kjøresesjonen starter i den neste det kjører. Fanen **Kjøretøyseiere** og rapporten **Medbrakte
  kjøretøy** viser starten på denne måten.

## Versjon 0.7.4

### Endringer

- **Et tjenestehefte viser hvert tog én gang.** Der en tjeneste deler et tog opp i flere avsnitt — fordi
  trekkraftenheten byttes, eller vogner kobles til eller fra underveis — men lokføreren blir på toget hele
  veien, skriver heftet nå ut toget én gang, fra der føreren tar det til der føreren forlater det, i stedet
  for én side per avsnitt. Blokkene for trekkraftenheter og vognsett viser hvilket kjøretøy som går hvilken
  del av toget.
- **Godsvogner som står sammen, er én rad i et tjenestehefte.** Godsstrømmer som kobles til på samme
  stasjon på samme plass i toget, deler nå én rad i blokken med godsvogner med fraktbrev, slik
  togsammensetningene viser dem: hvert sted nevnt én gang, regionene etter stedene og én største last for
  hele gruppen, summen av destinasjonenes. Radene ordnes etter stasjonen der vognene kobles til, i den
  rekkefølgen toget kommer dit, og deretter etter plass.
- **Togsammensetninger viser hele toget der et vognsett kobles til.** Et tog får fortsatt bare en rad der
  et vognsett kobles til eller godsstrømvogner tas med, men raden viser nå hvert vognsett toget går med,
  også dem det allerede har med seg, slik at plassene til dem som kobles til der, gir mening.
- **Togsammensetninger rammer inn hvert vognsett.** Et vognsetts omløp og vognene står nå sammen i én
  skyggelagt ramme, slik at de leses som én gruppe under ett omløp i stedet for at omløpet leses som enda
  et vognsett ved siden av dem.
- **Én største last per godsrektangel i togsammensetninger.** Et godsrektangel med flere destinasjoner
  angir nå summen av deres største last én gang, til slutt, i stedet for ett tall per destinasjon.
- **En vogn som legges til et vognsett, er en personvogn.** En ny vogn i dialogen **Rediger kjøretøy** er
  nå i utgangspunktet en personvogn, ikke en godsvogn.

### Feilrettinger

- **Tider etter midnatt havner på riktig døgn.** På en bane med **Går over midnatt?** krysset av havner en
  tid etter midnatt som skrives inn på et tog som går over midnatt — for eksempel 00:10 skrevet over
  23:55 — nå på neste døgn, slik at togets opphold blir stående i den rekkefølgen toget kjører. Planer som
  er lagret tidligere med slike tider, rettes når de åpnes.

## Versjon 0.7.3

### Endringer

- **Ankomstspor kan legges der neste tog går fra.** En ny knapp for **ankomstspor** på hvert omløp på
  fanen **Omløp** flytter omløpets ankomster til sporet neste tog går fra, uansett kjøretøy. Et omløp med
  togsett eller vendetog rettes fortsatt av seg selv, og nå også når **Vendetog?** krysses av på et
  lokomotiv som allerede er tildelt.
- **Listen over driftssteder er enklere å arbeide i.** Knappene for et driftssted står nå rett etter
  navnet i stedet for helt ytterst på den brede raden, så det er tydelig hvilket driftssted de hører til.
  Et klikk på selve navnet åpner informasjonen om driftsstedet. Annenhver rad er skyggelagt, og raden
  under musepekeren markeres tydelig.
- **En stasjon viser stedene den betjener med gods.** På fanen **Driftssteder** viser informasjonen om en
  stasjon nå stedene som får godset betjent fra den, og informasjonen om et sted viser hvilken stasjon som
  betjener det. De utskrevne arkene for driftssteder viser også de betjente stedene.
- **Togsammensetninger viser ankommende godsstrømvogner.** Et tog som ankommer med godsstrømvogner som
  kobles fra på en stasjon, får nå en egen rad på stasjonens ark, med avgangstiden der toget går videre,
  og med ett stiplet rektangel per plass i toget som angir hvor vognene kom fra. Bare godsstrømmer med
  **Kople fra?** krysset av vises, unntatt på en skyggestasjon, der alle ankommende vogner vises.
  Skyggestasjoner får nå også egne ark.
- **Togsammensetninger viser hvert vognsett der det kobles til.** Et vognsett som ikke angir vognene sine,
  tegnes nå også, bare med det skyggelagte omløpsrektangelet. Et vognsett vises bare der det kobles til:
  ved den første avgangen i omløpet, og senere bare der det uttrykkelig kobles til — med en merknad om
  tilkobling, hentet fra et annet spor eller koblet til et tog som allerede går. Et vognsett som blir med
  loket sitt fra tog til tog, vises ikke igjen. Hvor et vognsett står i toget, angis per tilkobling:
  **Plass** i dialogen **Rediger togavsnitt** på fanen **Omløp**, slik at flere vognsett som kobles til
  på samme stasjon, tegnes i den rekkefølgen.
- **Togsammensetninger tar mindre plass.** Destinasjonene, og opprinnelsesstedene for ankommende vogner,
  skrives nå etter hverandre som en kommaseparert liste i rektangelet i stedet for én per linje. Kolonnen
  **Omløp** er borte: hvert vognsetts omløp står i et skyggelagt rektangel foran vognene, noe som gir
  sammensetningene mer bredde. Kolonnen **Til** heter nå **Til/fra** og sier *til* hvor et avgående tog
  skal, og *fra* hvor et ankommende tog kom fra.
- **Togsammensetninger tegnes slik togene kjører.** Hvert tog begynner nå med et lokrektangel i den enden
  det er på vei mot, med en pil, og vognene følger bak, slik at rekkefølgen på papiret er rekkefølgen på
  sporet. Loket rommer dagene eller kjøresesjonene, toget og tidene på stasjonen (**06:00-06:45**) og til
  slutt togets største last, og erstatter fem kolonner. Et tog som går i den definerte retningen til
  banestrekningen, peker mot høyre; et som går mot den, peker mot venstre. Hver side følges av sitt
  speilbilde for den andre siden av sporene: skriv ut tosidig og snu arket til den siden som stemmer med
  det du ser. Nabodriftsstedene nevnes i hver sin ende av overskriften til sammensetningen.
- **Regioner sist i togsammensetninger.** Der flere destinasjoner deler en plass i toget, listes alle
  stedene først og regionene etter dem, hver region én gang.

## Versjon 0.7.2

### Nye funksjoner

- **Driftssteder kan nå skrives ut.** En ny rapport under **Rapporter** gir hvert driftssted på
  anlegget sitt eget ark på A4 stående: egenskapene, regionene, instruksjonene for hvordan det betjenes
  på treffet, og sporene med lengder, plattformer, ruter og bruk. Bare egenskapene som gjelder for den
  typen driftssted, vises, og driftstidene som vises, er de som gjelder — driftsstedets egne, ellers
  anleggets. Instruksjonene skrives ut her for første gang. Et driftssted med mer enn det er plass til på
  ett ark, fortsetter på det neste, og det neste driftsstedet begynner likevel på et nytt ark. Med **Skriv
  ut rapporter på lokale språk** krysset av skrives hvert ark ut på språket i driftsstedets land.

### Endringer

- **Bare driftstidene som gjelder, tilbys.** På fanen **Driftssteder** tilbys tiden for å kjøre lokomotivet
  rundt bare på stasjoner, skyggestasjoner medregnet, og tiden for togklarering bare på bemannede
  stasjoner, siden det trengs en togekspeditør på vakt for å klarere et tog. En tid som er satt tidligere
  der den ikke lenger gjelder, beholdes, men verken vises eller skrives ut.

## Versjon 0.7.1

### Nye funksjoner

- **Rapporter skrives ut på banens språk.** Alle rapporter skrives nå ut på banens standardspråk, det
  første språket i standardlandet, uansett hvilket språk du selv arbeider på. De utskrevne arkene leses
  av deltakerne på treffet, ikke av deg. Datoer og tall følger også landet.

  Kryss av for **Skriv ut rapporter på lokale språk** under **Innstillinger › Generelt**, så skrives
  hver førertjeneste ut på språket til selskapet som kjører den, hvert omløpskort på språket til
  kjøretøyets selskap og hver stasjons togekspederingsliste på språket til stasjonens land. Det gjelder
  også togekspederingslister som lagres som dokumenter. En tjeneste eller et kort uten selskap får
  språket til togenes operatører, hvis alle har samme språk. Alt uten et språk appen kan skrive ut på,
  beholder standardspråket.

- **En togkategori angir hvor togene stopper.** Fanen **Togkategorier** har et **stoppmønster**: kryss
  ved driftsstedene der kategoriens tog stopper underveis. Et nytt tog som opprettes på fanen **Tog**, får
  et stopp ved hvert avkrysset driftssted det passerer og kjører gjennom de øvrige — der toget begynner,
  og der det slutter, er stopp uansett hva som er krysset av, for det er der det gjøres klart og settes
  bort. Bare de driftsstedene kategorien i det hele tatt kan stoppe ved, tilbys.

  Et tog som stopper et sted mønsteret ikke angir, vises under **Konflikter**, med tog, driftssted og tid.
  Ingenting rettes for deg: bare du kan avgjøre om det er toget som stopper der det burde kjøre gjennom,
  eller mønsteret som mangler et driftssted der kategoriens tog stopper. Kontrollen kan slås av under
  **Innstillinger › Validering**.

  **Hent fra togene** krysser av for driftsstedene der kategoriens eksisterende tog stopper, og det gjøres
  for deg første gang en plan fra en tidligere versjon åpnes — hver kategori får mønsteret togene har
  kjørt hele tiden, så ingenting rapporteres som ikke var en feil før. Krysser du ikke av for noe, står
  kategorien ubundet: togene stopper da alle steder der de kan levere det de fører med seg, som før. En
  skiftekategori har ikke noe stoppmønster, siden oppdragene ikke kjører noen steder.

- **Et skifteoppdrag trenger ikke eget lok, men det trenger en lokfører.** Et oppdrag utføres like ofte
  av toglokomotivet som allerede står på stasjonen, eller av et skiftelok du ikke har lagt inn som
  kjøretøy, som av et som er satt opp for det. Et **omløp** som bare inneholder skifteoppdrag, er derfor
  ferdig uten tildelt kjøretøy og listes ikke lenger under **Konflikter** som et omløp uten kjøretøy.
  Legger du et tog som kjører et sted inn i det samme omløpet, kreves det kjøretøy igjen.

  Det et oppdrag derimot trenger, er noen til å utføre det, så det tilbys nå på fanen **Tjenester** som
  et hvilket som helst annet togavsnitt, med eller uten eget lok — skrevet med stasjonen én gang og
  tidene arbeidet går mellom, siden det ikke kjører noen steder. Et oppdrag ingen tjeneste dekker, listes
  under **Konflikter** for de øktene det blir stående ubemannet, ved siden av avsnittene et lok trekker.

  I et trykt tjenestehefte har oppdragssiden overskriften **Skifteoppdrag** med signaturen til operatøren,
  ikke et tognummer ingen bruker, og der et tog har ruteplanen sin, har et oppdrag sin egen blokk:
  **Arbeidstider**, én linje med stasjonen og tidene arbeidet **starter** og **slutter**, med
  skifteinstruksjonene under. Det oppgis ikke spor — et oppdrag utføres over hele stasjonen, ikke fra
  ett spor.

- **Søylene for lokførere regner med ventetiden i en tjeneste.** Søylene over den grafiske ruteplanen
  regnet en lokfører som nødvendig bare mens et tog eller et skifteoppdrag ble kjørt. En lokfører med et
  opphold mellom to tog i sin **tjeneste** er likevel opptatt i mellomtiden, i påvente av eller på vei til
  det neste, så den tiden regnes nå også — på de sesjonene tjenesten kjøres. Det gjør også en starttid du
  har angitt før tjenestens første tog, eller en sluttid etter dens siste.

- **Tog på linjen kontrolleres slik togekspeditørene ser det.** Listen **Konflikter** så før på én
  banestrekning om gangen, så to tog kunne krysse på en ubemannet stasjon, eller følge hverandre forbi
  en, uten at noe ble sagt. Nå ser den på hver togledelsesstrekning som en helhet. Et signalstyrt sted,
  for eksempel en blokkpost, deler togledelsesstrekningen i avsnitt som hvert kan romme ett tog per spor.
  På enkeltspor kan tog i motsatt retning bare møtes ved endene, eller på et signalstyrt sted der tog kan
  krysse. Tog i samme retning kan følge hverandre, ett per avsnitt. Se **Togledelsesstrekninger** i
  hjelpen på fanen **Strekninger**.

- **En bemannet stasjon kan fjernstyre en avgreining eller en ubemannet stasjon.** Feltet **Styres fra**
  på fanen **Driftssteder**, som til nå bare fantes på signalstyrte steder, tilbys nå også på ubemannede
  stasjoner og industriområder. Bare bemannede stasjoner tilbys som styrende stasjon. En fjernstyrt
  avgreining, et kryssingsspor, en ubemannet stasjon eller et industriområde betjenes av den stasjonens
  togekspeditør som sitt eget: togledelsesstrekninger slutter der, togene står på den styrende
  stasjonens togekspederingsliste i tidsrekkefølge blant stasjonens egne, med signaturen foran sporet, og
  overskriften nevner det. Stasjonene bortenfor er blant dem den styrende stasjonen ringer, og de ringer
  den styrende stasjonen. En blokkpost, et signalstyrt sted som verken er avgreining eller kryssingsspor,
  forblir en del av linjen. Trykk på **Generer på nytt fra banestrekninger** på fanen **Strekninger**
  etter å ha angitt en styrende stasjon.

- **Angi hvor tog kan krysse.** Et signalstyrt sted har en ny avkrysningsboks **Tog kan krysse?** på
  fanen **Driftssteder**. Kryss av der ett tog kan vente mens et annet passerer. Antall spor kan ikke
  avgjøre dette, for en avgreining har to spor for å vite hvilken vei et tog skal, uansett om tog kan
  krysse der. Et signalstyrt sted som verken er avgreining eller avkrysset, er en blokkpost. Importerte
  steder starter uten kryss, så kryss av for kryssingssporene etter importen.

### Endringer

- **Lokale destinasjoner navngis.** En godsdestinasjon med **Og lokale?** krysset av lyder ikke lenger
  *Stilkøbing og lokale destinasjoner*: den nevner stedene, *Stilkøbing, Vig, Rubjerg* — stasjonen fulgt
  av hvert sted som får godset betjent derfra (**Godsbetjenes fra** på fanen **Driftssteder**). En
  stasjon som ikke betjener noe, nevnes alene. Uttrykket er også borte fra godsforklaringen i de generelle
  instruksjonene, siden ingenting skriver det ut lenger.

## Versjon 0.7.0

### Nye funksjoner

- **Tabellen over skiftestasjoner kan nå plasseres i de allmenne instruksjonene.** Tabellen over
  skiftestasjoner skrives ut på anleggssiden bakerst i heftet med de allmenne instruksjonene. På et
  anlegg med mange skiftestasjoner fylte den siden og skjøv forklaringen av godsstrømmenes merker bort.
  Skriv

  ```
  <ShuntingYards/>
  ```

  på en egen linje under **Innstillinger**, der i teksten den hører hjemme, for i stedet å skrive den ut
  der. Anleggssiden utelater den da, så den skrives aldri ut to ganger. Forhåndsvisningen ved siden av
  teksten viser en boks der tabellen kommer. Skrives den ingen steder, blir tabellen stående på
  anleggssiden som før.

- **Kjøretøyseiere: hvem som tar med hvilket rullende materiell til treffet, og hvor det skal settes opp.**
  Fanen **Kjøretøyseiere** viser hvert lok, togsett og vognsett — med antall enheter der det er flere enn én,
  og for et vognsett som angir vognene sine, hver vognklasse én gang — med den første kjøresesjonen (eller dagen) det er i drift, og stasjonen, sporet og avgangen der det skal
  stå før den. Åpne en rad for å legge til eiere: den første tar med kjøretøyet og setter det opp på
  anlegget, øvrige eiere tar med reserver. Velg en eier ved å skrive de første bokstavene i navnet — eller i
  etternavnet — slik at et navn staves likt overalt; et navn som ikke passer til noen, tilbys som en ny
  deltaker. Hver eier av et lok eller togsett, også reservene, må oppgi en DCC-adresse; skriv **0** når
  eieren ennå ikke har oppgitt den. Hvert kjøretøy og hver eier har en merknad, og visningen **Deltakere**
  viser alle med det de tar med, og der rettes et feilstavet navn én gang for alle.

  Rapporten **Medbrakte kjøretøy** under **Rapporter** skriver ut den samme listen på A4 liggende, ordnet på
  en av tre måter som velges over sidene: **Per driftssted**, en side per stasjon til eieren, med
  kjøretøyene som skal settes opp der i rekkefølge etter første kjøresesjon og avgang; **Per eier**, en side
  per deltaker med det vedkommende tar med, DCC-adressene og hvor hvert kjøretøy starter; eller **Etter
  DCC-adresse**, alle medbrakte lok og togsett i én liste. Hver stasjon eller eier begynner på en ny side og
  fortsetter på neste når én side ikke er nok. Kjøretøy som ikke er i drift, vises sist; de som ingen tar
  med ennå, kommer først per eier under **Ikke booket ennå**. Radens bakgrunn viser når enheten brukes: hvit
  når den er i drift i alle kjøresesjoner, lysegrå for en reserve og ellers lyseblå, lysegrønn eller lyserød
  når kjøretøyet først er i drift fra første, andre eller tredje kjøresesjon.

- **Et omløp kan angi at kjøretøyene står på et annet spor enn toget.** Når et togavsnitt redigeres
  under **Omløp**, angir **Hent fra** sporet kjøretøyene står på før toget går, og **Sett på** sporet
  de settes på etter ankomsten — for eksempel et vognsett som blir stående på et sidespor på en
  mellomstasjon. Tjenesteheftene og togekspederingslistene skriver det ut som en merknad: *Før avgang,
  hent vognsett 21 fra spor 3.* ved avgangen og *Etter ankomst, skift vognsett 21 til spor 3.* ved
  ankomsten, i stedet for merknaden om å koble kjøretøyet til eller fra. Der kjøretøyet bare går med
  toget i noen av kjøresesjonene eller dagene toget går, innledes merknaden med dem — *1,3,5: Før
  avgang, hent …* — og et kjøretøy som ikke går med toget i noen av dem, får ingen merknad. Et spor
  som et omløp bruker på den måten, kan ikke slettes under **Driftssteder**.

- **Togsammensetninger kan nå skrives ut.** En ny rapport under **Rapporter** gir hver bemannet stasjon en
  egen side på A4 liggende med alle tog som går derfra med godsstrømvogner eller med et vognsett som angir
  vognene sine. Togene vises spor for spor, og i avgangsrekkefølge på hvert spor. Ved siden av hvert tog
  tegnes sammensetningen forfra som rektangler: et vognsett som ett rektangel per vogn, i vognrekkefølge,
  med vognens litra og nummer; og godsstrømvogner som ett rektangel per plass i toget, med hvor de skal —
  med *og lokale destinasjoner* og *og videre* der godsdestinasjonen sier det, og regionene i sine farger.
  Sammensetningen er den toget går med, så vogner det kom med, vises like godt som de som kobles til på
  stasjonen. Et vognsett som bare går med toget i noen kjøresesjoner eller dager, merkes med dem, og
  godsstrømvogner uten plass vises sist, som *Hvor som helst i toget*.

- **Et motorvognsett ankommer nå sporet det skal gå fra.** Et motorvognsett — og et lok i vendetog —
  kjører aldri rundgang: det går fra nettopp det sporet det kom inn på. Når et slikt kjøretøy kjører tog
  etter tog, legges derfor hvert togs ankomstspor på sporet neste tog går fra, og — slutter omløpet der det
  begynte — legges siste togets ankomstspor på sporet første tog går fra, slik at kjøretøyet står klart der
  neste kjøresesjon henter det. Bare ankomstspor flyttes; sporet et tog går fra, blir stående slik du har
  angitt det. Et omløp som trekkes av et vanlig lok, røres ikke, for loket kjøres alene over stasjonen til
  sporet neste tog står på. Sporene rettes hver gang et tog legges til i et omløp — av **Bygg automatisk**
  eller av deg, sist i omløpet eller der kjøretøyet står — og hver gang du tildeler et kjøretøy til et
  omløp, slik at et omløp som er bygget før motorvognsettet var kjent, rettes så snart motorvognsettet
  settes på det. Et importert omløp beholder sporene det ble lest inn med. Ville flyttingen sette to tog på
  samme spor samtidig, står det blant konfliktene du kan løse.

### Endringer

- **Dager og kjøresesjoner listes uten mellomrom.** Der en merknad eller en kolonne angir hvilke dager
  eller kjøresesjoner noe gjelder, skrives de nå *M,O,F* og *1,3,5* i stedet for *M, O, F* og *1, 3, 5*,
  slik at angivelsen tar så lite plass som mulig. Dager som skrives helt ut — *Mandag, Onsdag, Fredag* —
  er uendret.

- **Et vognsett som angir vognene sine, viser dem nå i etiketten under Omløp.** Etiketten viser antall vogner
  og hver vognklasse én gang — *SJ 05 5 x A/B/Fv* — i stedet for vognsettets egen klasse.

- **En merknad ved et stopp sier nå om den hører til ankomsten eller avgangen.** Tjenesteheftene og
  togekspederingslistene skriver ut ankomsten og avgangen ved et stopp på hver sin linje, så under **Tog**
  spør feltet ved siden av hver **Merknad** hvilken av dem den gjelder: **Ank** for noe som møtes eller skal
  gjøres ved innkjøringen, **Avg** for noe som skal gjøres før eller ved avgangen. Der toget bare ankommer
  eller bare går, er bare den ene å velge. Der det kjører forbi, kan ingen merknad skrives — kryss av **Ank**
  eller **Avg** først — men en som allerede står der, kan fortsatt fjernes.

  En merknad som ble skrevet med en tidligere versjon, eller som fulgte med en XPLN-import, anga ingen av
  delene og ble derfor verken skrevet ut i heftene eller i listene. Første gang en plan åpnes, får hver slik
  merknad avgangen der toget går, og ankomsten der det bare ankommer.

- **Hver blokk på en togside i et tjenestehefte har nå en farget strek langs venstre kant.**
  Trekkraftenheter er merket med rødt, planlagte vognsett med grønt, godsvogner med fraktbrev med blått og
  ruteplanen med grått, slik at blokkene kan skilles fra hverandre med et blikk, og der en grå strek slutter,
  slutter det togavsnittet. Strekene skrives ut uten at bakgrunnsgrafikk må slås på, og hver blokk beholder
  overskriften sin, så en utskrift i svart-hvitt mister ingenting.

- **Trekkraftenheter og planlagte vognsett på en togside i et tjenestehefte viser nå sporene sine.** Hver rad
  angir sporet kjøretøyet står på ved starten, og sporet det blir stående på ved slutten — togets eget spor,
  eller det omløpet angir under **Hent fra** eller **Sett på**. Kolonnen som angir kjøretøyet, har nå
  overskriften **Omløp** i begge blokkene, etter kortet kjøretøyets identitet står på. Med sporene i
  tabellen angir ruteplanen nedenfor ikke lenger hvert vognsett og sporet dets: den sier *Skift vogner til
  avgangssporet før avgang.* eller *Skift vogner til deres ankomstspor etter ankomst.*, innledet med de
  kjøresesjonene eller dagene det gjelder, når vognene bare skiftes i noen av dem toget går.
  Togekspederingslistene angir fortsatt hvert vognsett og sporet dets.

- **Heftet med generelle instruksjoner forklarer nå hvordan gods skrives i de andre rapportene.** Under
  **Godsstrømmer** på heftets siste side sier en forklaring at hver lastegrense er et maksimum, og hva merket
  etter et tall teller, hva globusen står for, hva *og lokale destinasjoner* og *og videre* legger til en
  destinasjon, og hva et farget regionnavn betyr.

- **Forsiden av heftet med generelle instruksjoner har plass til et lengre program.** Programmet settes med
  mindre avstand mellom linjene, punktene og dagsoverskriftene — skriftstørrelsen er den samme — noe som gir
  plass til fire eller fem punkter til. Et program som er for langt for siden, mister de siste punktene sine,
  og de er slutten på treffet, nettopp det folk slår opp.

- **Bygg automatisk utvider nå omløpene du allerede har, før det lager nye.** Togene som ennå ikke er i et
  omløp, tilbys først de eksisterende omløpene: et omløp fortsetter med det som fortsetter det der det
  ankommer, i kategorien det allerede går i, og et tomt omløp du selv har laget, fylles før et nytt
  opprettes. Bare togene som ikke passer i noe eksisterende omløp, starter nye omløp, så når du bygger på
  nytt etter å ha lagt til noen tog, forlenges kjøretøyene som allerede går, i stedet for at nye settes i
  drift. Godsstrømmer blir latt være som de er. Resultatet ved siden av knappen sier nå hvor mange
  togavsnitt de eksisterende omløpene tok på seg, hvor mange av dem som ble forlenget, og hvor mange omløp
  som ble bygget.

### Feilrettinger

- **Forklaringen av godsstrømmenes merker renner ikke lenger av heftet med de allmenne
  instruksjonene.** Formuleringene *og lokale destinasjoner* og *og videre*, samt forklaringen av en
  region, sto i en så smal kolonne at hver av dem gikk over fire linjer, og på et anlegg med flere
  skiftestasjoner falt den siste av dem utenfor nederkanten av siden. Merkene og formuleringene settes
  nå som to lister under hverandre, hver over hele sidebredden.

- **En ruteplan som fortsetter på motstående side i et tjenestehefte, ser nå ut som alle andre.** Når et
  togavsnitt er for langt for én side, flyttes ruteplanen til motstående side, og der ble den skrevet ut uten
  heftets egen utforming: med større skrift, uten de fete stasjonene og tidene og uten strekene mellom
  stoppene, så en lang ruteplan kunne gå ut over bunnen av siden.

- **Å endre et togs nummer eller kategori under Tog etterlater ikke lenger endringen på en annen linje.**
  Begge endringene sorterer listen på nytt, og nummeret du skrev eller kategorien du valgte, kunne bli stående
  på linjen til toget som rykket inn på plassen.

- **Dialogene leser nå inn et tall mens du skriver det.** **Varighet (minutter)** for et nytt skifteoppdrag,
  **Minutter** å flytte eller kopiere tog med og et kjøretøys **Nummer** under **Omløp** ble først lest når du
  forlot feltet, så knappen som bekrefter dialogen — og advarselen om at et kjøretøynummer allerede er tatt —
  hang etter til du klikket et annet sted.

- **Den grafiske ruteplanen tegner nå sporene til et driftssted i den rekkefølgen du har gitt dem.** Den så
  bort fra sporenes **Rekkefølge** under **Driftssteder** og kunne derfor tegne dem i en annen rekkefølge enn
  alle andre sporlister i appen.

- **Et omløpskort for et kjøretøy som går på dager som ikke følger etter hverandre, nevner nå dagene.** Et
  kort for mandag, onsdag og fredag skrev ut *MondayShort,WednesdayShort,FridayShort* i stedet for *M,O,F*.

## Versjon 0.6.0

### Endringer

- **Tjenestetog er en ny slags togkategori.** Gi en togkategori typen **Tjenestetog** under
  **Togkategorier** for tog som verken leverer eller tar opp noe der de stopper: et arbeidstog, eller et
  lok eller togsett som kjøres ut av drift. Et slikt tog kan stoppe der en driftsplass verken utveksler
  reisende eller gods — for eksempel en arbeidsplass — og når ruten bygges, får det ingen opphold mellom
  endepunktene sine, så det oppholdet som betyr noe, legger du inn selv. Gi kategorien navn etter hva
  togene gjør: et tog som setter igjen materialvogner utveksler gods og hører hjemme i en godskategori.

  En kategori i en plan laget med en tidligere versjon som verken var person eller gods — noe en
  XPLN-import kan etterlate — vises nå som et tjenestetog, der den før ble vist som et persontog.

- **Skifteoppdrag er en ny slags tog.** Gi en togkategori typen **Skifteoppdrag** under
  **Togkategorier**, så utføres togene i den kategorien på én stasjon i et bestemt tidsrom i stedet for
  å kjøre noe sted: hvert av dem har bare ett opphold, der ankomsttiden er når arbeidet begynner, og
  avgangstiden når det slutter.

- **Godsstrømmene i et skifteoppdrag angir hvilke vogner som skal skiftes.** Legg til godsstrømmer i
  oppdraget under **Godsstrøm** på samme måte som for et hvilket som helst godstog. En strøm med
  oppdragets egen stasjon som destinasjon inneholder vogner som har ankommet, og lokføreren får beskjed
  om å skifte dem ut til godskundene, med opplysning om hvor de kommer fra. En strøm til et annet sted
  hentes i stedet inn fra godskundene, med opplysning om hvor vognene skal. Instruksjonen skrives ut i
  lokførernes turhefter og i stasjonsrapportene.

- **Passasjerbilletter kan nå skrives ut.** En ny rapport under **Rapporter** gir en returbillett mellom
  hvert par av driftssteder med passasjerutveksling, brettet på midten og med operatøren som har flest
  persontogavganger fra salgsstedet, nederst på begge halvdelene.

- **Ruterapporten legger nå flere strekninger på ett ark.** Tabeller som er for smale til å fylle bredden,
  står ved siden av hverandre, så en kort sidebane ikke lenger tar et helt ark for seg selv.

- **De grafiske ruteplanene kan nå skrives ut.** En ny rapport under **Rapporter** tegner hver strekning i
  den faste papirskalaen som stilles inn under **Innstillinger → Grafisk ruteplan**, slik at tider og
  stigninger kan måles fra ett ark til det neste.

- **Innstillinger → Grafisk ruteplan er nå ordnet etter hva hver innstilling påvirker.** Det ruteplanen
  viser kommer først, og under det avstandene på skjermen, i bildepunkter, ved siden av dem på papiret, i
  millimeter.

- **Du kan nå angi hva som skal gjøres med loket der et togavsnitt slutter.** Når du redigerer et
  togavsnitt under **Omløp**, spørres det om loket skal snus og om det skal kjøres om til den andre enden,
  og hver av dem skrives ut som en ankomstmerknad for lokfører og togekspeditør.

- **Topologi-diagrammet tegner nå hele anleggets spor, med hvert driftssted vist én eneste gang.** Sporet
  er enkelt- eller dobbeltsporet slik strekningen virkelig er, og i fargene til ruteplanstrekningene som
  går over det — grått der ingen strekning dekker det.

- **Du kan nå ordne Topologi-diagrammet selv.** Dra et driftssted dit det hører hjemme, så følger sporene
  med; det du ordner, lagres med planen og skrives ut i tjenesteheftene.

- **En godsdestinasjon med grense for både vogner og aksler viser nå begge.** Vogntallet forsvant før
  der det også sto et aksseltall — både i tjenesteheftene og i godsmerknadene — selv om de to feltene står
  side om side under **Godsstrøm**, og hver av grensene kan være den som binder: seksten aksler er fire
  boggivogner, men åtte toakslede.

- **Togsidene i et tjenestehefte sier nå det samme på mindre plass.** Kolonnen med kjøresesjoner har nå
  overskriften **Kjører** — det den forteller om kjøretøyet — i stedet for et langt ord over en kolonne
  med sirkler, og godsvognene har overskriftene **Fra** og **Til**, slik kjøretøyene over dem allerede
  hadde. Begrensningene under overskriften skrives som tall under et enkelt **Maks**: hastigheten med
  enheten sin, tallet med en sirkel etter for aksler, en firkant for vogner og lengden som *2,5m*. Hvor mange vogner eller
  aksler en destinasjon tar, er flyttet ut av **Til** til sin egen kolonne **Maks**, der det leses rett
  nedover siden i stedet for til slutt i en rekke stedsnavn — og den kolonnen vises bare når noe på siden
  i det hele tatt er begrenset.

- **Knappene som gjelder et helt omløp, står nå i sin egen kolonne.** Under **Omløp** er de flyttet til en
  kolonne **Handlinger** mellom kjøretøyene og togene, så hver rads tog begynner på samme sted.

- **Rapportmenyen har en ny rekkefølge**, fra de generelle instruksjonene til passasjerbillettene.

### Feilrettinger

- **Den installerte appen virker nå uten internettforbindelse.** Hjelpetekstene, tekstene under Om
  og Versjonsnyheter og katalogen med ferdige togkategorier ble hentet fra nettet hver gang de ble
  vist, og var derfor tomme når du var uten forbindelse. De lagres nå sammen med resten av appen når
  den installeres.

## Versjon 0.5.1

### Endringer

- **Hva som skal gjøres med lokomotivene, vises nå i førernes tjenestehefter og i
  togekspederingslistene.** Hvilket lokomotiv som skal brukes, hva som skal kobles til og fra, og at det
  skal hentes fra — eller kjøres tilbake til — hensettingssporet, ble hele tiden regnet ut fra
  materiellomløpene, men aldri skrevet ut; nå står de blant de andre merknadene ved stoppet de hører til,
  og både føreren og togekspeditøren ser dem. Nytt blant dem er beskjeden for et lokomotiv som må kjøres
  rundt til den andre enden av toget, eller vendes, før toget går tilbake.

- **Heftet med generelle instruksjoner skriver nå ut hele teksten din, på sider som lar seg lese.** En
  side ble regnet som romsligere enn den faktisk er, så det som gikk forbi bunnen falt stille bort;
  teksten fortsetter nå på den neste siden i stedet, og en side slutter aldri med en overskrift alene.
  **Topologi** og **Skiftestasjoner** kommer nå på heftets aller siste side, slik som i tjenesteheftene,
  og programmet på forsiden er satt i heftets egne størrelser i stedet for nettleserens.

## Versjon 0.5.0

### Endringer

- **Et vendetog står ikke lenger og venter på lokrundgang.** Kryss av i den nye boksen **Vendetog?** på et
  lokomotiv under **Omløp** der det framfører et tog som kan kjøres fra begge ender — et tog med styrevogn
  eller enda et lokomotiv i den andre enden — så regner **Oppdater tider** bort rundgangen og lar toget stå
  det korteste oppholdet i stedet, noe som framskynder alle følgende opphold. Et motorvogntog behandles på
  samme måte uten noe å krysse av, og et opphold du bevisst har gjort lengre, blir stående slik du har satt
  det.

- **Et spor kan nå angi hvilken vei gjennom driftsstedet det er ment for.** Hvert spor kan angi det
  **forrige** driftsstedet et tog kommer fra, det **neste** det fortsetter til, eller begge — med feltet
  **begge retninger** — og et nytt tog legges på det sporet som passer best til veien det kjører. Det er
  nettopp dette en **dobbeltsporet strekning** trenger: gi de to sporene samme par driftssteder omvendt, så
  holder hver retning seg til sitt spor. Der to spor passer like godt, tar et persontog som stopper et spor
  med plattform, mens et tog som kjører gjennom tar hovedsporet; la kolonnene stå tomme, så endres
  ingenting fra før.

- **Et tog kan nå kopieres i motsatt retning og gjentas.** Kryss av for **Motsatt retning?**, så kjører
  kopien strekningen baklengs, med alle kjøretider og opphold beholdt, forberedelses- og avslutningstiden
  byttet ende og et nummer fra rekka til motsatt retning. Kopidialogen har nå også valget **Gjenta tog**,
  så et tog kan opprettes for seg, justeres til det går som det skal, og først deretter gjentas utover
  dagen.

- **Et spor kan nå si hvor lang plattformen er.** Hvert spor ved et driftssted som utveksler passasjerer,
  har en **plattformlengde** i meter — over null betyr at passasjerer kan gå på og av der — og et nytt
  passasjertog legges på et spor med plattform der driftsstedet har en. Kryss av for **Passasjerer?**, så
  får hvert spor en plattform på én meter som du kan justere, og en plan laget før dette behandles likedan
  første gang den åpnes, så den virker akkurat som før til du korter ned eller nullstiller de sporene som i
  virkeligheten ikke har noen plattform. Et passasjertog som stopper for passasjerutveksling ved et spor
  uten plattform, står nå under **Konflikter**: gi enten sporet en plattformlengde eller fjern krysset i
  stoppets **Ank** og **Avg**, som sier at toget ikke utveksler noe der. Kontrollen kan slås av under
  **Innstillinger › Validering**.

### Feilrettinger

- **Å gi anlegget et nytt navn endrer nå navnet alle stedene det vises.** Forsiden på heftet med de
  generelle instruksjonene, navnet i den øverste linja og filnavnet en plan lagres under fortsatte alle å
  vise hva anlegget het før. En plan som har fått nytt navn tidligere, rettes neste gang den åpnes.

## Versjon 0.4.2

### Endringer

- **Nå kan et tog settes inn midt i et omløp.** Mellom togavsnittene på en rad er det nå små skjøter som
  viser hvor kjøretøyet står og hvor lenge, og før det første avsnittet en som viser hvor det må hentes
  fra; klikk på en av dem for å sette inn et tog i hullet, så tilbys bare togene kjøretøyet faktisk rekker.
  En tur som ikke bringer kjøretøyet tilbake, settes inn likevel og rapporteres som en konflikt til du
  setter inn returen — slik passes en tur-retur inn i et opphold. En skjøt der omløpet er brutt, slik en
  import kan etterlate det, er merket med gult.

- **Appen har fått sitt eget ikon** — fronten på et moderne tog mot en mørkeblå flate — i stedet for merket
  som følger med verktøyene den er bygd med. Ikonet vises i fanen i nettleseren, og på hjemskjermen eller i
  Start-menyen for den som installerer appen.

- **Det er nå plass til tolv omløpskort på et ark i stedet for ti.** Kortene er 48 mm brede i stedet for
  50, så seks får plass i bredden på et liggende A4-ark, og arket har fortsatt en marg som vanlige skrivere
  når. Kortene er like høye som før, og innholdet er uendret.

- **Radene i ruteplanen står nå lenger fra hverandre.** Det er nå en sjuendedel mer luft rundt hver linje,
  så en rad er lettere å følge tvers over siden og en stasjon lettere å finne i kolonnen. Skriften og
  kolonnene er uendret, så bladet rommer de samme togene; en side tar nå trettini linjer i stedet for
  førtifem.

### Feilrettinger

- **Den utskrevne ruteplanen mister ikke lenger de siste radene på en side.** Begge retninger av en
  strekning ble satt på samme side også når det ikke var plass til begge, og radene det ikke ble plass til
  ble klippet bort — rapporten på skjermen var satt i en større skrift enn den utskrevne, så radene der var
  nesten to tredjedeler høyere enn dem som ble talt. De to settes nå likt, hvor mye det er plass til måles
  på en virkelig side i stedet for å regnes ut fra skriftstørrelsen, og tre linjer holdes frie nederst på
  hver side.

- **Godsstrømlisten nevner nå destinasjonene vognene skal til.** Under **Godsstrøm › Godstog** sto det bare
  "Vogner til" i listen man velger fra, uten destinasjonene, så postene kunne ikke skilles fra hverandre.
  Underfanen og kolonnen dens heter nå **Godsdestinasjoner** i stedet for *Godsbeskrivelser*.

## Versjon 0.4.1

### Endringer

- **Togekspederingslistene kan nå lagres som dokumenter stasjonseierne kan redigere.** Velg
  *Togekspederingslister* i menyen Eksporter, så får hver bemannet stasjon sitt eget dokument i
  OpenDocument-format, ment for å sende hver eier deres egen liste før treffet slik at de kan legge til de
  lokale instruksjonene bare de kjenner; er mer enn én stasjon bemannet, kommer dokumentene samlet i en
  zip-fil. Hvor sidene brytes er overlatt til tekstbehandleren, så sidene brytes fornuftig også etter at
  eieren har skrevet — navnet på stasjonen, telefonnumrene til stasjonene den ekspederer tog til og fra og
  kolonneoverskriftene gjentas øverst på hver side, men den delen av døgnet en side dekker lar seg ikke
  oppgi, så sidene nummereres i stedet. De utskrevne arkene i menyen Rapporter er uendret og er fortsatt
  dem man arbeider fra under en kjøresesjon.

- **Et tog som trekkes av to lokomotiver samtidig, sier nå hvilke to.** Konflikten nevnte bare toget og
  minuttene, så var begge booket over nøyaktig samme strekning, lød de to halvdelene ord for ord like. Den
  markeres nå også bare på de to omløpene som holder det dobbeltbookede arbeidet, i stedet for på hvert
  omløp som kjørte det toget et sted på dagen.

- **To lokomotiver som deler et tog mellom sesjoner, rapporteres ikke lenger som en konflikt.** Bare
  klokkeslettene ble sammenlignet, så et lokomotiv på ulike sesjoner og et annet på like — hele poenget med
  å legge det opp slik — ble rapportert som dobbeltkjøring. Nå rapporteres det bare der begge er booket på
  en felles sesjon, og konflikten nevner de sesjonene.

## Versjon 0.4.0

### Brytende endringer

- **Et kjøretøy du oppretter, identifiseres nå av operatøren og nummeret sitt.** På én og samme sesjon kan
  kombinasjonen bare tilhøre ett kjøretøy, uansett hvilken slags kjøretøy det er, så et vognsett og et
  lokomotiv kan ikke lenger begge være *DB 5*; et kjøretøy uten operatør identifiseres av nummeret alene,
  og to kjøretøy kan dele identitet så lenge sesjonene de går på ikke overlapper. Et **importert** kjøretøy
  identifiseres fortsatt av den eksterne id-en det ble importert med, så en importert plan gir ingen nye
  konflikter av dette. Å legge til eller endre et kjøretøy avviser nå en identitet som et annet kjøretøy
  allerede har, og krever et nummer, mens eksisterende planer beholdes nøyaktig som de er, med hvert
  kjøretøy som deler identitet blant konfliktene.

### Endringer

- **Det finnes en ny rapport: togekspederingslisten.** Ett sett ark for hver bemannet stasjon, med togene
  stasjonen ekspederer i tidsrekkefølge — et tog som står der står oppført to ganger, ankomster på hvit
  bakgrunn og avganger på lysegul, fordi det å ekspedere et tog inn og å ekspedere det videre er to
  forskjellige handlinger, og tog som bare kjører forbi er også med. Hver side har navnet på stasjonen, den
  delen av døgnet siden dekker, og telefonnumrene til stasjonene i den andre enden av
  togekspederingsstrekningene, og hver rad har en rute per kjøresesjon til å krysse av. Hver stasjon
  begynner på en ny side, slik at bunken kan deles og deles ut; skrives ut fra menyen Rapporter.

- **Feltene for å legge til og endre et kjøretøy har fått ny rekkefølge,** den samme begge steder:
  kjøretøytype, trekkrafttype, antall enheter, operatør, nummer, klasse, sesjoner og til slutt den eksterne
  id-en. Feltet som før het *Selskap*, heter nå *Operatør*.

- **En ekstern id kan rettes, men ikke lenger finnes på.** Den eksterne id-en er navnet et tog eller et
  kjøretøy bærer i systemet det ble importert fra, så det som er importert med en id har fortsatt feltet
  sitt og kan rettes der, mens det som aldri har hatt en id nå ikke har noen rute å skrive i. Et kjøretøy
  du oppretter i planleggeren, får derfor ingen ekstern id i det hele tatt, der det før fikk en oppdiktet
  av klasse og nummer.

- **Den minste tiden mellom to bruk av samme spor kontrolleres nå.** Innstillingen fantes, men ingenting
  brukte den: står den på 0, der den begynner, endres ingenting i kontrollen. Sett den til 5, og sporet må
  i tillegg være ledig i fem minutter mellom to tog — nøyaktig fem holder, fire gjør det ikke — og
  konflikten sier hvor kort mellomrommet faktisk er og hvor langt det måtte være.

- **Et driftssted kan nå ha sine egne instruksjoner.** Redigeringsskjemaet har feltet **Instruksjoner**,
  skrevet i Markdown ved siden av en forhåndsvisning, til hvordan nettopp det driftsstedet kjøres på dette
  treffet: hvilke spor som brukes til hva, hvordan skiftingen er lagt opp, og hva lokførerne og de som
  bemanner stedet ellers trenger å vite. Feltet tilbys på en stasjon eller et industriområde og vises i
  Info-visningen for driftsstedet; det tilbys ikke der det ikke er noe å instruere om.

- **Et sted der det kjøres gods uten bemanning, kan nå kreve en nøkkel.** Velg den betjente stasjonen som
  oppbevarer nøkkelen under **Nøkkel oppbevares ved**, og gi nøkkelen et navn hvis stasjonen oppbevarer
  flere — et godstog som stopper begge steder får da ved avgangen beskjeden *hent nøkkel A1 for å låse opp
  Bruket*, og ved neste stopp der *lever nøkkel A1 fra Bruket*. Nøkkelen hentes ved den siste stoppen før
  arbeidet og leveres tilbake ved den første etterpå, og et tog som bare kjører forbi får ingen beskjed.
  Merk stedet som betjent, eller ta betjeningen bort fra stasjonen som oppbevarer nøkkelen, så slutter
  nøkkelen å gjelde — **Konflikter** sier hvilken endring som gjorde det, og nøkkelen beholdes, så den
  gjelder straks igjen om du angrer endringen.

### Feilrettinger

- **To strekninger som går ut fra samme driftssted, ble tegnet som om de aldri møttes.** Begynte en
  kjøreplanstrekning på nettopp det første driftsstedet på en annen, var det ingenting som bandt de to
  sammen i Topologi-diagrammet. Den andre forlater nå det driftsstedet som enhver annen grein, i samme
  faste vinkel.

- **Hver grenseverdi for kontrollene sier nå hvilken klokke den måles etter.** Den minste tiden mellom to
  bruk av samme spor manglet enhet helt, og de to toghastighetene oppga bare *klokkeminutter*. Alle tre
  oppgir nå hurtigklokkeminutter — klokka togene går etter, ikke virkelig tid.

- **Lengder og distanser skrives nå ut i meter,** slik også telleren i toghastighetene gjør, så *m* ikke
  kan leses som et minutt. Minste opphold på en stasjon oppgis nå også i hurtigklokkeminutter.

## Versjon 0.3.5

### Feilrettinger

- **En lagret plan kunne nekte å åpne seg.** Å åpne en plan appen nettopp hadde lagret, stoppet med en feil
  om et land, og ingenting ble lest inn. En allerede lagret plan åpnes som den er; du trenger ikke gjøre
  noe med den.

- **En lagret planfil er omtrent sju ganger mindre.** Lagring skrev planen i en annen form enn den som
  holdes i nettleseren, så hvert opphold ble skrevet to ganger, og hver togkategori, hver operatør og hvert
  land om igjen ved hvert tog, hvert kjøretøy og hver tjeneste som brukte det. En fil som tok 8 MB, tar nå
  godt over 1 MB; en plan lagret av en tidligere versjon kan fortsatt åpnes.

## Versjon 0.3.4

### Endringer

- **Feltene Ank og Avg på et stopp følger nå hvor toget faktisk kan stoppe.** Et persontog trenger et
  driftssted som tar imot passasjerer og et godstog ett som tar imot gods, og ingen av delene lar seg gjøre
  på et signalstyrt driftssted; der toget ikke kan stoppe, vises begge feltene tomme og kan ikke krysses
  av, og stoppet er en gjennomkjøring. Ingenting av det du har planlagt kastes bort — slå utvekslingen på
  igjen, så er stoppene der — og en skyggestasjon har alltid utveksling av både passasjerer og gods, siden
  den representerer alt utenfor anlegget.

- **Et stopp som noe henger på, kan ikke lenger fjernes.** Togets eget første og siste stopp, og endene på
  hvert togavsnitt som et materiellomløp, en tjeneste eller en godsflyt er planlagt over, beholder nå
  feltet avkrysset og låst, og holder du pekeren over det, sies det hva som holder det. Der et togavsnitt
  slutter et sted toget ikke kan stoppe, sies det rett ut, så du kan flytte stoppet eller togavsnittet.

- **En togkategori bærer nå forberedelses- og avslutningstidene togene dens planlegges med,** så du slipper
  å skrive de samme to tallene for hvert tog. Ved siden av hvert felt står en knapp *Bruk på nytt*, som gir
  den ene tiden til alle togene kategorien allerede har og forteller hvor mange som ble endret; de to er
  hver sin handling, og å bruke på nytt flytter bare minuttene ytterst på et tog.

- **Operatørene er lettere å lese på forsiden av et tjenestehefte.** Linjen settes nå i dobbel størrelse,
  slik at en logo er stor nok til å kjennes igjen med et blikk og en signatur stor nok til å leses tvers
  over et bord. Har alle operatørene en logo, utelates ordet *Operatør*; mangler en av dem logo, står alle
  med signatur, i fet skrift og med etiketten beholdt.

### Feilrettinger

- **Et tjenestehefte kunne skrive ut et togavsnitt forbi nederste sidekant.** Hver side ble regnet med
  omtrent halvparten mer plass enn en A5-side faktisk har, og det som går forbi sidekanten blir klippet
  bort uten varsel, så det andre togavsnittet på en slik side manglet slutten av ruteplanen sin eller
  manglet helt. Togavsnitt måles nå mot det siden faktisk rommer, så noen hefter trenger ett ark mer enn
  før.

- **Topologi-diagrammet kunne skrive signaturene for to driftssteder oppå hverandre.** Driftsstedene ble
  plassert bare etter avstanden mellom dem, så to som ligger tett på hverandre på en lang strekning ble
  tegnet nesten på samme sted. De tegnes nå aldri tettere på hverandre enn signaturene deres trenger, og en
  lang signatur ved kanten av diagrammet blir ikke lenger klippet bort.

- **En grein i Topologi-diagrammet kunne tegnes tvers gjennom en annen strekning.** En grein faller bort i
  en fast vinkel, så en grein som møtte en strekning i veien ble rett og slett tegnet tvers over den. De
  greinene som forlater en strekning lengst ute, tegnes nå først, så en lang grein kan nå bli tegnet under
  en kort grein som forlater strekningen lenger ute.

- **En plan kunne vise togene sine under togkategorier som fanen Togkategorier ikke hadde.** Flere
  kategorier kunne også tas for en og samme, slik at togene deres ble samlet under én enkelt overskrift, og
  to tog av ulike kategorier med samme nummer ble meldt som ett nummer brukt to ganger. Når en plan åpnes,
  fylles listen over kategorier nå ut med kategoriene togene bruker, og hver kategori holdes atskilt fra de
  andre.

- **To selskaper som aldri hadde fått sitt eget nummer, ble tatt for den samme operatøren,** så tog fra
  ulike selskaper som delte tognummer ble meldt som ett nummer brukt to ganger. Hvert selskap får nå sitt
  eget nummer når en plan åpnes eller lagres; et selskap fra Module Registry beholder nummeret det kom med.

- **En plan lagret togkategoriene, selskapene og landene sine flere steder** — hver av dem ble skrevet der
  den først ble møtt, som regel inne i det første toget som brukte den. Hver av dem skrives nå én gang, i
  sin egen liste, og alt som bruker den beholder bare en henvisning; land kopieres ikke lenger inn i planen
  i det hele tatt, så en retting av språkene til et land når nå også planer som er lagret på forhånd.

- **Et tjenestehefte oppga bare tognummeret i overskriften for et togavsnitt.** Et tog identifiseres like
  mye av prefikset og suffikset til kategorien som av nummeret — Gt 1234, ikke 1234 — og overskriften er alt
  en lokfører har å sammenligne med ruteplanen. Den viser nå hele togidentiteten, etter operatørens
  signatur.

## Versjon 0.3.3

### Endringer

- **Konflikter kan nå leses der de vises.** En rad med konflikter — et tog eller en togkategori under
  **Tog**, et omløp eller ett av kjøretøyene i det under **Omløp**, en tjeneste under **Tjenester** — har nå
  et varselsymbol, og et klikk på det åpner meldingene som en lesbar liste. Symbolet får farge etter den
  alvorligste konflikten og teller dem; hittil sto de bare i et lite felt som kom fram mens pekeren hvilte
  på raden.
- **En togkategori viser konfliktene for togene i den**, slik at de ikke lenger skjules når kategorien
  lukkes.
- **Fanen Tog åpner nå på listen over togkategorier**, med togene skjult til du åpner en kategori. *Utvid
  alle* åpner alle på én gang, og en kategori åpner seg selv når du legger til eller flytter et tog dit.
- **Når et togavsnitt i et omløp redigeres, står det nå hvilke slags kjøretøy omløpet gjelder** — lok,
  togsett eller vognsett. Hver slags nevnes én gang, og peker du på den, nevnes kjøretøyene selv.

### Feilrettinger

- **Appen kunne slutte å lagre arbeidet ditt uten å si fra.** En plan appen ikke fikk skrevet ut — et tog
  med færre enn to stopp, eller en ruteplanstrekning der alle banestrekningene var fjernet — gjorde at
  lagringen mislyktes lydløst, så alt som ble gjort etterpå ble stående på skjermen, men ble aldri tatt
  vare på. Begge planene kan nå lagres, og mislykkes en lagring likevel, sier den øverste linjen fra med en
  gang.

- **En lagret planfil er omtrent 40 % mindre.** Hvert stopp ble skrevet to ganger — én gang i toget sitt og
  én gang under sporet det står på — og den andre kopien dro med seg store deler av resten av planen. En
  plan lagret med en tidligere versjon kan fortsatt åpnes.

- **Et tog som er latt uten trekkraft på en del av løpet sitt, rapporteres nå.** Kontrollen spurte bare om
  et lok eller togsett kjørte toget *et eller annet sted*, så når et omløp ble kortet av i den ene enden,
  ble resten av toget stående uten trekkraft uten at noe ble sagt. Nå kontrolleres hver strekning for hver
  kjøresesjon toget kjøres, og konflikten sier mellom hvilke driftssteder og i hvilke kjøresesjoner; planer
  som så rene ut kan nå rapportere dette.

## Versjon 0.3.2

### Endringer

- Under **Godsstrøm › Godsbeskrivelser** kan et opprinnelsessted eller en destinasjon nå være hvilket som
  helst driftssted som utveksler gods, ikke bare en stasjon — et industriområde håndterer alltid godsvogner,
  men kunne ikke velges før. De samme listene sier nå **driftssted** der de sa *stasjon*.
- Oppholdene til et tog listes alltid i den **rekkefølgen toget kjører** dem.
- Å endre en tid for et opphold i fanen **Tog** **tar nå med seg resten av toget**: en **avgang** virker
  framover, den veien toget kjører, og en **ankomst** bakover, slik at løpet fram til endringen følger med.
  Tidene på den andre siden blir stående, kjøre- og oppholdstidene beholdes, og endringen avvises hvis den
  ville føre toget utenfor planens driftstider.
- Et tog hvis togvei **hopper over et driftssted** — to opphold etter hverandre uten en strekning imellom —
  rapporteres nå som en konflikt. Den kan slås av under **Innstillinger › Validering**.
- Et togavsnitt i et **omløp** kan nå **redigeres**: pennen åpner fra- og til-stoppet, slik at et omløp kan
  formes om uten at alt etter det fjernes. Et naboavsnitt som knytter seg til det du endrer, følger med; et
  naboavsnitt der toget selv ikke stopper på det nye stoppet står uendret, og gapet meldes som en konflikt
  du selv løser.
- **Legg til tog** kan nå opprette **returtoget** samtidig. Kryss av for *Retur?*, så opprettes toget
  tilbake sammen med det første, med samme strekning i motsatt retning, samme togslag og hastighet og neste
  nummer i motsatt retning; avgangen er enten *så tidlig som mulig* eller et tidspunkt du skriver inn.
  Sammen med *Gjenta?* gjentas begge retningene.

### Feilrettinger

- **Kilometertallene** i den utskrevne ruteplanen og langs den grafiske ruteplanen avrundes nå til hele
  kilometer, og en sidebane viser samme kilometertall som banen den går ut fra ved forgreningsstasjonen.
- Alt som leser togets rute følger nå **rekkefølgen toget kjører stoppene i**, ikke rekkefølgen de ble lagt
  inn. For et tog der stoppene er lagt inn i feil rekkefølge gikk linjen i den **grafiske ruteplanen** i
  sikksakk, kunne den utskrevne **ruteplanen** vise en avgang der toget ankommer, kjedet **bygg automatisk**
  ikke toget i det hele tatt, målte **gjenta tog** intervallet fra feil stopp, og omregning av tidene
  mislyktes helt. Importerte planer har aldri vært berørt.
- **Toghastigheten kontrolleres nå også på den siste strekningen**, inn til driftsstedet der toget avslutter
  løpet sitt.

## Versjon 0.3.1

### Endringer

- Avsnittet **Trekkraftenheter** på siden for et togavsnitt i heftet Førertjenester har nå overskriften sin
  på det valgte språket. Det var den eneste overskriften i heftet uten oversettelse.
- Trekkraftenheten skrives nå ut for hvert togavsnitt som har en. I planer importert med en tidligere
  versjon viste noen togavsnitt en trekkraftenhet under **Tjenester**, men ingen i heftet.
- Merknader om tog i samme retning sier nå hvilket tog som kommer forbi det andre — **Kjører forbi GD 42757
  12:02-12:05** eller **Blir forbikjørt av GD 42757 12:02** — i stedet for det tidligere *"Møter GD 42757 i
  samme retning"*, som aldri sa hvilket tog som kom foran. To tog som bare står på samme stasjon samtidig
  gir ingen merknad i det hele tatt.
- Et møte uten varighet — det andre toget kjører gjennom uten opphold — skrives som ett klokkeslett i
  stedet for et intervall fra et tidspunkt til seg selv.
- Et tog som begynner eller avslutter kjøringen sin på en stasjon, tas ikke lenger med som møtt, krysset
  eller forbikjørt der. Disse tidene er når lokføreren møter til tjeneste eller går av.

## Versjon 0.3.0

### Endringer

- En ny rapport, **Førertjenester**, skriver ut ett A5-hefte per tjeneste. Forsiden viser tjenestens
  nummer, hvilke økter eller dager den kjøres, dens start- og sluttid og -stasjoner, en vanskelighetsgrad,
  bemanningsbehov og eventuelle tjenestemerknader; hvert togavsnitt får så sin egen side med hvilke
  trekkraftenheter som skal brukes, hvilke vognsett som skal tas med, til hvilke destinasjoner godsvogner
  skal tas med, samt ruteplanen, hver i sin egen blokk.
- En ny rapport, **Generelle instruksjoner**, er et eget hefte med treffets program og instruksjonene som
  gjelder for anlegget gjennom hele treffet — kjøreinstruksjoner, signalgiving, radio- og telefonbruk, hva
  man gjør ved forsinkelser og hvem man spør — og det deles ut én gang til alle. Det innledes med treffets
  navn og datoer, så programmet hver deltaker trenger å vite før den første økten, så instruksjonene over
  så mange sider som de trenger, brutt mellom avsnitt og aldri med en overskrift igjen alene.
- Siste side i begge heftene viser anleggets sporplan og tabellen over skiftestasjoner, slik at også de som
  aldri holder et tjenestehefte — først og fremst stasjonspersonalet — får en oversikt over anlegget.
- Både programmet og instruksjonene skrives under **Innstillinger › Informasjon** og kan formateres med
  Markdown. Begge heftene skrives ut i A5: A4 liggende, tosidig, brettet på midten, med tomme sider lagt
  til der det trengs slik at arkene brettes riktig.
- Tjenester kan nå graderes **Lett**, **Middels** eller **Erfaren**, vist fargekodet på heftet, kan angi at
  de trenger to eller tre personer — for eksempel en lokfører og en konduktør — og kan festes til et **fast
  nummer** som automatisk omnummerering lar være urørt.
- Planen kontrolleres nå også slik at hvert togavsnitt med lokomotiv eller togsett tildelt har en
  førertjeneste som dekker det i hver økt det kjøres. En tjeneste med fast nummer må ha et nummer, og ingen
  to slike kan få samme nummer.
- Selskaper kan nå ha en opplastet **logo**, vist i rapporter i stedet for tekstsignaturen.
- Stasjoner kan nå merkes som den **skiftestasjonen** som betjener en annen stasjons lokalgods, og anlegget
  lister opp hver skiftestasjon og hva den dekker på tjenesteheftets siste side.
- Hver ruteplanstrekning kan nå gis en **farge**, brukt til å tegne den i Topologi-diagrammet.
- En ny **avstandsfaktor** (Innstillinger › Tid & hastighet) lar et anlegg vise et større, mer forbildetro
  kilometertall i rapporter og den grafiske ruteplanen enn avstanden som faktisk er modellert, uten at det
  påvirker noen kjøretidsberegning.
- Appen holder nå flere åpne nettleserfaner eller -vinduer synkronisert med hverandre. **Merk** at dette
  bare fungerer mellom vinduer på samme maskin i samme nettleser.
- Innstillinger kan nå lagre treffets **gjelder fra**- og **gjelder til**-datoer, skrevet ut som en
  gyldighetslinje på rapporter; la dem stå tomme hvis ikke noe treff er booket ennå.
- En ny innstilling, **utvid plantider automatisk?** (Innstillinger › Generelt), utvider planens start-
  eller sluttid for å dekke et tog i stedet for å blokkere endringen. Av som standard.
- En ny knapp, **oppdater alle tider**, i den grafiske ruteplanen beregner alle tog i ruteplanen på nytt
  samtidig, i stedet for å måtte velge en delmengde først.
- Sporbelegningskontrollen kan nå valgfritt ta hensyn til at et lokomotiv eller togsett står på et spor
  mellom to tog, med mindre det er booket til eller fra hensetting (Innstillinger › Validering). Av som
  standard, siden det bare gir mening på anlegg der hensetting er modellert bevisst.
- Hvert opphold i fanen **Tog** har nå et felt for **Merknad** — en merknad som skrives ut ved det
  oppholdet, for eksempel «vent på møtende tog». Merknaden vises ferdig formatert og bytter til den rå
  oppmerkingen så snart du går inn i feltet, så skriv `*sakte*` for kursiv og `**første**` for fet.

### Feilrettinger

- Når man legger til et nytt tog, settes nå standard starttid under hensyn til den angitte
  forberedelsestiden, slik at det ikke starter før planens starttid.

## Versjon 0.2.4

### Endringer

- En ny fane **Tjenester** lar deg planlegge førertjenester — arbeidet en lokfører utfører i løpet av en
  økt, som en rekke av togavsnittene føreren kjører. Hver tjeneste er en rad: betegnelse, selskap og økter
  til venstre, togavsnittene i kjørerekkefølge til høyre.
- Legg til togavsnittene en fører kjører med **Legg til togavsnitt**. Listen viser trekkraftstrekningene en
  fører kan ta som det neste — de som ikke kolliderer i tid med tjenesten, og, når den har et togavsnitt,
  de som avgår ved eller etter at det ankommer. Togavsnittene trenger ikke starte på samme stasjon: føreren
  går rett og slett dit det neste starter.
- Det samme togavsnittet kan kjøres av flere tjenester så lenge de kjører i forskjellige økter, så én
  tjeneste kan dekke oddetallsøktene og en annen partallsøktene.
- Der to togavsnitt for samme tog i en tjeneste kjøres av forskjellige trekkraftenheter, viser fanen en
  merknad ved stasjonen der trekkraftenheten byttes — du skriver den ikke inn for hånd.
- Tjenester importert fra XPLN deler nå togavsnittene som er definert i kjøretøyenes turnuser, så hvert
  togavsnitt viser trekkraftenheten som kjører det.
- Planen kontrolleres slik at intet togavsnitt kjøres av to tjenester i samme økt og ingen tjeneste har
  togavsnitt som overlapper i tid. Kontrollen kan slås av under **Innstillinger › Validering**.

## Versjon 0.2.2

### Feilrettinger

- To tog som aldri kjører i samme driftsøkt, rapporteres ikke lenger som et møte på en enkeltsporet
  strekning. Et tog som kjører økt 1, 3, 5, og ett som kjører 2, 4, 6, er aldri ute samtidig.
- Konfliktkontrollen på dobbeltsporede og flersporede strekninger er nå presis: en strekning merkes bare
  når det er flere tog på den samtidig enn den har spor, og bare tog som kjører i en felles økt telles med.

## Versjon 0.2.1

### Endringer

- Konfliktvarsler vises nå der du kan rette dem: togkonflikter i den grafiske ruteplanen og på fanen
  **Tog**, kjøretøy- og omløpskonflikter på fanen **Omløp**.
- På fanen **Omløp** fremhever en kjøretøykonflikt nå bare det aktuelle kjøretøyet, og en omløpskonflikt
  bare det aktuelle omløpet.
- Kontrollen av at et kjøretøy vender tilbake til utgangspunktet, omfatter nå også vognsett og gods, ikke
  bare lok og togsett.

## Versjon 0.2.0

### Endringer

- Navnet på planen du arbeider med, vises nå øverst i vinduet.
- Den grafiske ruteplanen viser nå søyler for lokomotivførerbehovet, noe som gjør det lettere å se hvor
  mange førere som trengs gjennom driftsøkten.
- En ny **Topologi**-visning (under fanen **Strekninger**) viser et skjematisk diagram over ruteplanens
  strekninger og deres greiner.

### Feilrettinger

- Strekninger beholder nå rekkefølgen du la dem inn i som standard. Du kan fortsatt sortere på hvilken som
  helst kolonne.
- Konflikter viser ikke lenger til tog du ikke finner: når et tog slettes, fjernes stoppene sammen med det,
  slik at ingen foreldreløse stopp eller falske konflikter blir igjen.

## Versjon 0.1.0

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
