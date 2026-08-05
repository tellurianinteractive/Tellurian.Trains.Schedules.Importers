# Versjonsnyheter

## Version 0.4.0

### Brytende endringer

- **Et kjøretøy du oppretter, identifiseres nå av operatøren og nummeret sitt.** De to sammen peker ut
  ett virkelig kjøretøy, så på én og samme sesjon kan kombinasjonen bare tilhøre ett kjøretøy — uansett
  hvilken slags kjøretøy det er. Et vognsett og et lokomotiv kan ikke lenger begge være *DB 5*. Et
  kjøretøy uten operatør identifiseres av nummeret alene. To kjøretøy kan fortsatt ha samme operatør og
  nummer så lenge sesjonene de går på ikke overlapper, for da er de aldri på treffet samtidig.

  Et **importert** kjøretøy identifiseres fortsatt av den eksterne id-en det ble importert med, som
  allerede er entydig i planen det kom fra, så en importert plan gir ingen nye konflikter av dette.

  Å legge til eller endre et kjøretøy under fanen Omløp avviser nå en identitet som et annet kjøretøy
  allerede har, og et nummer må oppgis. Planer laget før denne regelen beholdes nøyaktig som de er —
  ingenting nummereres om for deg — og hvert kjøretøy som deler identitet står blant konfliktene, én
  gang hver, så du ser hva som trenger et nytt nummer.

### Endringer

- **Det finnes en ny rapport: togekspederingslisten.** Ett sett ark for hver stasjon som er bemannet —
  alle bemannede stasjoner og alle skyggestasjoner, enten de er bemannet eller ikke — med togene
  stasjonen ekspederer, i tidsrekkefølge. Et tog som står på stasjonen, står oppført to ganger, én gang
  for ankomsten og én gang for avgangen, fordi det å ekspedere et tog inn og å ekspedere det videre til
  neste stasjon er to forskjellige handlinger med noen minutters mellomrom. Ankomster står på hvit
  bakgrunn og avganger på lysegul, slik at de to aldri kan forveksles. Tog som bare kjører forbi, er
  også med, for de må også ekspederes forbi. Hver side har navnet på stasjonen, den delen av døgnet
  siden dekker, og telefonnumrene til stasjonene i den andre enden av togekspederingsstrekningene, og
  hver rad har en rute per kjøresesjon til å krysse av underveis, gråtonet for de kjøresesjonene toget
  ikke går. Hver stasjon begynner på en ny side, slik at bunken uten videre kan deles og deles ut.
  Skrives ut fra menyen Rapporter.

- **Feltene for å legge til og endre et kjøretøy har fått ny rekkefølge,** den samme begge steder:
  kjøretøytype, trekkrafttype, antall enheter, operatør, nummer, klasse, sesjoner og til slutt den
  eksterne id-en — hva kjøretøyet er, så hva som identifiserer det, så hvordan det beskrives og når det
  går. Feltet som før het *Selskap*, heter nå *Operatør*.

- **En ekstern id kan rettes, men ikke lenger finnes på.** Den eksterne id-en er navnet et tog eller et
  kjøretøy bærer i systemet det ble importert fra, så den betyr noe bare der den kommer fra noe. Det som
  er importert med en id, har fortsatt feltet sitt — under fanen Tog, og i kjøretøydialogen under fanen
  Omløp — og kan rettes der; det som aldri har hatt en id, har nå ingen rute å skrive i. Et kjøretøy du
  oppretter i planleggeren, får derfor ingen ekstern id i det hele tatt, der det før fikk en oppdiktet
  av klasse og nummer.

- **Den minste tiden mellom to bruk av samme spor kontrolleres nå.** Innstillingen fantes, men
  ingenting brukte den. Står den på 0 — der den begynner, og der den blir til du endrer den — endres
  ingenting i kontrollen: to tog er i konflikt der de står på samme spor samtidig, og ett som kommer
  akkurat idet et annet går, er en avløsning, ikke en konflikt. Sett den til for eksempel 5, og sporet
  må i tillegg være ledig i fem minutter mellom dem, slik at en plan som snur sporet raskere enn
  stasjonen rekker, blir rapportert. Nøyaktig fem ledige minutter holder; fire gjør det ikke.

  En slik konflikt sier hvor kort mellomrommet faktisk er og hvor langt det måtte være, i stedet for å
  påstå at de to togene overlapper når tidene viser at de ikke gjør det.

- **Et driftssted kan nå ha sine egne instruksjoner.** Skjemaet for å legge til og endre et driftssted
  har feltet **Instruksjoner**, skrevet i Markdown og vist ved siden av en forhåndsvisning slik som de
  generelle instruksjonene i Innstillinger. Det er til for hvordan nettopp det driftsstedet kjøres på
  dette treffet — hvilke spor som brukes til hva, hvordan skiftingen er lagt opp, og hva lokførerne og
  de som bemanner stedet ellers trenger å vite der. Hvordan driftsstedet betjenes generelt, og annen
  beskrivelse av det, er eierens oppgave å skaffe og hører ikke hjemme i feltet. Det du skriver, lagres
  sammen med driftsstedet og vises i Info-visningen for det.

  Feltet tilbys på en stasjon eller et industriområde, der det utveksles reisende og/eller gods. Det
  tilbys ikke der det ikke er noe å instruere om: togene kjører bare forbi et signalstyrt sted, og ingen
  bemanner et annet sted, så toget gjør der det stoppet sier og ikke mer.

- **Et sted der det kjøres gods uten bemanning, kan nå kreve en nøkkel.** Der sporvekslene på en
  ubetjent stasjon eller et industriområde er låst, kan du i redigeringsskjemaet velge den betjente
  stasjonen som oppbevarer nøkkelen, under **Nøkkel oppbevares ved**, og gi nøkkelen et navn hvis
  stasjonen oppbevarer flere.

  Mer trenger ikke planlegges. Et godstog som stopper på stasjonen med nøkkelen og senere stopper på
  stedet nøkkelen låser opp, får ved avgangen derfra beskjeden *hent nøkkel A1 for å låse opp Bruket*;
  neste gang toget stopper der, sier ankomsten *lever nøkkel A1 fra Bruket*. Et tog som bare kjører
  forbi et av stedene, får ingen beskjed, for det låser ikke opp noe. Nøkkelen hentes ved den siste
  stoppen på stasjonen før arbeidet og leveres tilbake ved den første etterpå, så et tog som stopper der
  to ganger, slipper å ha den med en ekstra runde.

  En nøkkel betyr noe bare så lenge begge endene holder. Merk stedet selv som betjent, eller ta
  betjeningen bort fra stasjonen som oppbevarer nøkkelen, så slutter nøkkelen å gjelde: det lages ingen
  beskjeder av den, og **Konflikter** sier hvilken av de to endringene som gjorde det. Nøkkelen beholdes
  i stedet for å kastes, så angrer du endringen, gjelder den straks igjen, og den blir stående i skjemaet
  der du kan peke den mot en annen stasjon eller fjerne den.

### Feilrettinger

- **To strekninger som går ut fra samme driftssted, ble tegnet som om de aldri møttes.** Begynte en
  kjøreplanstrekning på nettopp det første driftsstedet på en annen, var det ingenting som bandt de to
  sammen i Topologi-diagrammet: hver ble tegnet som sin egen linje, uten grein mellom dem. Den andre
  forlater nå det driftsstedet som enhver annen grein, og faller bort fra det i samme faste vinkel.

- **Hver grenseverdi for kontrollene sier nå hvilken klokke den måles etter.** Den minste tiden mellom
  to bruk av samme spor manglet enhet helt, og de to toghastighetene oppga bare *klokkeminutter*, som
  kunne leses på begge måter. Alle tre oppgir nå hurtigklokkeminutter — klokka togene går etter, ikke
  virkelig tid.

- **Lengder og distanser skrives nå ut i meter,** slik også telleren i toghastighetene gjør, så *m*
  ikke kan leses som et minutt. Minste opphold på en stasjon oppgis nå også i hurtigklokkeminutter.

## Version 0.3.5

### Feilrettinger

- **En lagret plan kunne nekte å åpne seg.** Å åpne en plan appen nettopp hadde lagret, stoppet med en
  feil om et land, og ingenting ble lest inn — det fantes ingen vei forbi. En fil leses et stykke om
  gangen mens den kommer inn, og lesingen av landene i den snublet i det. En allerede lagret plan
  åpnes som den er; du trenger ikke gjøre noe med den.

- **En lagret planfil er omtrent sju ganger mindre.** Å lagre en plan til en fil skrev den i en annen
  form enn den som holdes i nettleseren, så gevinstene fra de to siste versjonene nådde aldri fram til
  filen: hvert opphold ble skrevet to ganger, og hver togkategori, hver operatør og hvert land om igjen
  ved hvert tog, hvert kjøretøy og hver tjeneste som brukte det. En fil som tok 8 MB, tar nå godt over
  1 MB, og lagres og åpnes tilsvarende raskere. En plan lagret av en tidligere versjon kan fortsatt
  åpnes.

## Version 0.3.4

- **Feltene Ank og Avg på et stopp følger nå hvor toget faktisk kan stoppe.** Et tog stopper for å
  utveksle noe og trenger derfor et sted å utveksle det: et persontog der driftsstedet tar imot
  passasjerer, et godstog der det tar imot gods, og ingen av delene på et signalstyrt driftssted. Der
  toget ikke kan stoppe, vises begge feltene tomme og kan ikke krysses av, og stoppet er en
  gjennomkjøring i ruteplanen og i grafen. Ingenting av det du har planlagt kastes bort — slå
  utvekslingen på igjen, så er stoppene der. En skyggestasjon har alltid utveksling av både passasjerer
  og gods, siden den representerer alt utenfor anlegget, så dens to felter vises avkrysset og låst.

- **Et stopp som noe henger på, kan ikke lenger fjernes.** En togdel går fra et stopp der toget går,
  til et der det kommer fram, så begge endene må være stopp. Togets eget første og siste stopp, og
  endene på hver togdel som et materiellomløp, en tjeneste eller en godsflyt er planlagt over, beholder
  nå feltet avkrysset og låst; hold pekeren over det, så sies det hva som holder det. Der en togdel
  slutter et sted toget ikke kan stoppe — en plan laget før denne regelen — sies det rett ut, så du kan
  flytte stoppet eller togdelen.

- **En togkategori bærer nå forberedelses- og avslutningstidene togene dens planlegges med.** Hvert
  nytt tog i kategorien gjøres klart så mange minutter før det går og settes bort så mange minutter
  etter at det er kommet fram, så du slipper å skrive de samme to tallene for hvert tog. Ved siden av
  hvert av de to feltene står en knapp *Bruk på nytt*, som gir den ene tiden til alle togene
  kategorien allerede har, og forteller hvor mange som ble endret. De to er hver sin handling, så du
  kan endre forberedelsestiden uten å røre avslutningstiden. Å bruke på nytt flytter bare minuttene
  ytterst på et tog: det går, stopper og kommer fram fortsatt nøyaktig på de tidene det gjorde.

- **Operatørene er lettere å lese på forsiden av et tjenestehefte.** Linjen settes nå i dobbel
  størrelse mot før, slik at en logo er stor nok til å kjennes igjen med et blikk og en signatur stor
  nok til å leses tvers over et bord. Har alle operatørene i tjenesten en logo, utelates ordet
  *Operatør* — logoene sier det selv. Mangler en av dem logo, står alle fortsatt med signatur, i fet
  skrift og med etiketten beholdt.

### Feilrettinger

- **Et tjenestehefte kunne skrive ut en togdel forbi nederste sidekant.** Rapporten regner ut før
  utskriften hvor mange togdeler det er plass til på en side, og regnet med omtrent halvparten mer
  plass enn en A5-side faktisk har. Det som går forbi sidekanten, blir klippet bort uten varsel: den
  andre togdelen på en slik side manglet slutten av ruteplanen sin — eller manglet helt, slik at en
  lokfører sto med en tjeneste der det siste toget manglet. Togdeler måles nå mot det siden faktisk
  rommer, og en togdel det ikke er plass til, flyttes til neste side. Noen hefter trenger derfor ett
  ark mer enn før.

- **Topologi-diagrammet kunne skrive signaturene for to driftssteder oppå hverandre.** Driftsstedene ble
  plassert bare etter avstanden mellom dem, så to som ligger tett på hverandre på en lang strekning ble
  tegnet nesten på samme sted, og signaturene deres gikk inn i hverandre. De tegnes nå aldri tettere på
  hverandre enn de to signaturene deres trenger, mens resten av strekningen beholder sine virkelige
  proporsjoner. En lang signatur ved kanten av diagrammet blir heller ikke lenger klippet bort.

- **En grein i Topologi-diagrammet kunne tegnes tvers gjennom en annen strekning.** En grein faller bort
  fra strekningen den forlater i en fast vinkel, så en grein som møtte en strekning i veien, aldri kom
  forbi den, uansett hvor langt ned i diagrammet den ble skjøvet — den ble rett og slett tegnet tvers
  over den. De greinene som forlater en strekning lengst ute, tegnes nå først, noe som gir dem bak en
  fri vei nedover. En lang grein kan derfor nå bli tegnet under en kort grein som forlater strekningen
  lenger ute.

- **En plan kunne vise togene sine under togkategorier som fanen Togkategorier ikke hadde.** Et tog
  bærer kategorien sin med seg, så en plan lagret av en tidligere versjon ble åpnet med togene gruppert
  etter kategori mens listen over kategorier var tom: kategorimenyen hadde ingenting å tilby, og ingen
  tog kunne flyttes til en annen kategori. Flere kategorier kunne også tas for en og samme, slik at
  togene deres ble samlet under én enkelt overskrift, og to tog av ulike kategorier med samme nummer
  ble meldt som ett nummer brukt to ganger. Når en plan åpnes, fylles listen over kategorier nå ut med
  kategoriene togene bruker, og hver kategori holdes atskilt fra de andre.

- **To selskaper som aldri hadde fått sitt eget nummer, ble tatt for den samme operatøren.** Et selskap
  skilles fra de andre på et nummer appen fører for det, og en plan kunne inneholde flere som aldri
  hadde fått et. Tog fra ulike selskaper som delte tognummer, ble da meldt som ett nummer brukt to
  ganger. Hvert selskap får nå sitt eget nummer når en plan åpnes eller lagres; et selskap fra Module
  Registry beholder nummeret det kom med.

- **En plan lagret togkategoriene, selskapene og landene sine flere steder.** Hver av dem ble skrevet
  der den først ble møtt under lagringen — som regel inne i det første toget som brukte den — mens
  listen den hører hjemme i, ikke inneholdt mer enn en henvisning til den. Slik kunne en plan få tog i
  kategorier som fanen Togkategorier ikke kjente til. Hver av dem skrives nå én gang, i sin egen liste,
  og alt som bruker den, beholder bare en henvisning. Land kopieres ikke lenger inn i planen i det hele
  tatt, så en retting av språkene til et land nå også når planer som er lagret på forhånd. En plan
  lagret av en tidligere versjon leses som før og blir rettet neste gang den lagres.

- **Et tjenestehefte oppga bare tognummeret i overskriften for en togdel.** Et tog identifiseres like
  mye av prefikset og suffikset til kategorien som av nummeret — Gt 1234, ikke 1234 — og en lokfører
  som sammenligner heftet med ruteplanen, eller med det som ropes opp, har bare den overskriften å gå
  etter. Overskriften viser nå hele togidentiteten, med prefiks og suffiks, etter operatørens
  signatur.

## Version 0.3.3

- **Konflikter kan nå leses der de vises.** En rad med konflikter — et tog eller en togkategori under
  **Tog**, et omløp eller ett av kjøretøyene i det under **Omløp**, en tjeneste under **Tjenester** —
  har nå et varselsymbol, og et klikk på det åpner meldingene som en lesbar liste. Symbolet får farge
  etter den alvorligste konflikten og teller dem når det er mer enn én. Hittil sto meldingene bare i et
  lite felt som kom fram mens pekeren hvilte på raden — lett å overse og vanskelig å lese.
- **En togkategori viser konfliktene for togene i den**, slik at de ikke lenger skjules når kategorien
  lukkes.
- **Fanen Tog åpner nå på listen over togkategorier**, med togene i hver kategori skjult til du åpner
  den, slik at en plan med mange tog er lettere å få oversikt over. *Utvid alle* åpner alle på én gang,
  og en kategori åpner seg selv når du legger til et tog i den eller flytter et tog dit.
- **Når en togdel i et omløp redigeres, står det nå hvilke slags kjøretøy omløpet gjelder** — lok,
  togsett eller vognsett. Deler flere kjøretøy det samme omløpet, nevnes hver slags én gang, og peker
  du på den, nevnes kjøretøyene selv.

### Feilrettinger

- **Appen kunne slutte å lagre arbeidet ditt uten å si fra.** Planen lagres i nettleseren mens du
  arbeider, og en plan appen ikke fikk skrevet ut — et tog med færre enn to stopp, eller en strekning
  under **Strekninger › Ruteplanstrekninger** der alle banestrekningene var fjernet — gjorde at
  lagringen mislyktes lydløst. Alt som ble gjort etterpå ble stående på skjermen, men ble aldri tatt
  vare på, så når nettleseren ble åpnet igjen lå planen som før: med driftsstedene, men uten
  strekningene og togene som var lagt til siden. Begge planene kan nå lagres, og mislykkes en lagring
  likevel, sier den øverste linjen fra med en gang, slik at du kan angre endringen i stedet for å miste
  arbeidet.

- **En lagret planfil er omtrent 40 % mindre.** Hvert stopp ble skrevet to ganger — én gang i toget sitt og
  én gang under sporet det står på — og den andre kopien dro med seg store deler av resten av planen.
  En plan lagret med en tidligere versjon kan fortsatt åpnes.

- **Et tog som er latt uten trekkraft på en del av løpet sitt, rapporteres nå.** Kontrollen spurte bare
  om et lok eller togsett kjørte toget *et eller annet sted*, så når et omløp ble kortet av i den ene
  enden, ble resten av toget stående uten trekkraft uten at noe ble sagt. Nå kontrolleres hver
  strekning toget kjører, for hver kjøresesjon det kjøres, og konflikten sier mellom hvilke driftssteder
  og i hvilke kjøresesjoner toget mangler trekkraft. Planer som så rene ut, kan nå rapportere dette —
  hullet har alltid vært der.

## Version 0.3.2

- Under **Godsstrøm › Godsbeskrivelser** kan et opprinnelsessted eller en destinasjon nå være
  hvilket som helst driftssted som utveksler gods, ikke bare en stasjon. Et industriområde
  håndterer alltid godsvogner, men kunne ikke velges før, så gods til og fra en industri måtte
  beskrives som om det gikk til nærmeste stasjon.
- De samme listene sier nå **driftssted** der de sa *stasjon*, siden de ikke lenger bare
  inneholder stasjoner.
- Å endre en tid for et opphold i fanen **Tog** **tar nå med seg resten av toget**. En **avgang** virker
  framover, den veien toget kjører: la et tog stå fem minutter lenger ved et driftssted, og det kommer fram
  fem minutter senere til alle senere driftssteder. En **ankomst** virker bakover: be toget om å ankomme
  fem minutter senere, og det går fem minutter senere fra alle tidligere driftssteder, slik at løpet fram
  til endringen følger med. Tidene på den andre siden blir stående, kjøre- og oppholdstidene beholdes, og
  endringen avvises — og feltet faller tilbake — hvis den ville føre toget utenfor planens driftstider.
- Oppholdene til et tog listes alltid i den **rekkefølgen toget kjører** dem.
- Et tog hvis togvei **hopper over et driftssted** — to opphold etter hverandre uten en strekning imellom —
  rapporteres nå som en konflikt. Den kan slås av under **Innstillinger › Validering**.
- **Toghastigheten kontrolleres nå også på den siste strekningen**, inn til driftsstedet der toget avslutter
  løpet sitt. Den strekningen ble hoppet over før.

- En togdel i et **omløp** kan nå **redigeres**: pennen på en togdel åpner fra- og til-stoppet, slik
  at et omløp kan formes om uten at alt etter det fjernes. En nabodel som knytter seg til den du
  endrer, følger med — forkort en del fra A–C til A–B, og returløpet blir B–A av seg selv. En
  nabodel der toget selv ikke stopper på det nye stoppet, står uendret, og gapet meldes som en
  konflikt du selv løser.

- Alt som leser togets rute følger nå **rekkefølgen toget kjører stoppene i**, ikke rekkefølgen de ble
  lagt inn. For et tog der stoppene er lagt inn i feil rekkefølge — et stopp lagt til etter et toget
  først kommer til senere — gikk linjen i den **grafiske ruteplanen** i sikksakk mellom stopp toget
  aldri kjører mellom, og toget kunne havne i kolonnen for feil retning; den utskrevne **ruteplanen**
  kunne vise en avgang der toget ankommer; **bygg automatisk** kjedet ikke toget i det hele tatt,
  siden det så ut til å starte et annet sted; **gjenta tog** målte intervallet fra feil stopp; og
  omregning av tidene etter en endret stoppeplan mislyktes helt. Valg av en del av et tog viser også
  stoppene i kjørerekkefølge. Importerte planer har aldri vært berørt — der er de to rekkefølgene like.

- **Legg til tog** kan nå opprette **returtoget** samtidig. Kryss av for *Retur?*, så opprettes toget
  tilbake fra destinasjonen sammen med det første, med samme strekning i motsatt retning, samme togslag
  og hastighet og neste nummer i motsatt retning. Avgangen er enten *så tidlig som mulig* — det første
  togets ankomst pluss etterarbeids- og forberedelsestiden — eller et tidspunkt du skriver inn, som kan
  ligge både før og etter det første togets avgang. Sammen med *Gjenta?* gjentas begge retningene, slik
  at hele trafikken i begge retninger planlegges på én gang.

### Feilrettinger

- **Kilometertallene** i den utskrevne ruteplanen og langs den grafiske ruteplanen avrundes nå til
  hele kilometer. De ble skrevet med en desimal, og avstandsfaktoren under **Innstillinger › Tid &
  hastighet** kunne gjøre lengden på en strekning til en skjev del av en kilometer. En sidebane viser
  nå også samme kilometertall som banen den går ut fra ved forgreningsstasjonen.

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
