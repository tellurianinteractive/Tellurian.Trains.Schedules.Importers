# Versjonsnyheter

## Versjon 0.5.2

### Endringer

- **De grafiske ruteplanene kan nå skrives ut.** En ny rapport under **Rapporter** tegner hver
  ruteplanstrekning i en fast papirskala — så mange millimeter per hurtigklokketime og per kilometer — og
  legger så mange strekninger på et ark som papiret rommer. Hvordan papiret vendes følger den orienteringen
  du har valgt for den grafiske ruteplanen: en vannrett tidsakse skrives ut på A4 liggende med strekningene
  stablet under hverandre, en loddrett på A4 stående med dem ved siden av hverandre.

  Fordi skalaen er fast i stedet for presset sammen for å passe papiret, kan tider og stigninger
  sammenlignes og måles fra ett ark til det neste. Et tidsvindu som er for langt for ett ark, deles langs
  tidsaksen — først ved pausen, deretter i like store ark som overlapper hverandre — slik at et tog som
  krysser snittet kan følges på begge arkene, og det siste arket blir like fullt som de andre i stedet for
  å bære noen få minutter. Skalaen stilles inn under **Innstillinger → Grafisk ruteplan**; det er ved å
  minske stasjonsavstanden der at to eller tre strekninger får plass på samme ark. Togene skrives ut i
  togslagsfargene sine, som på skjermen, med mindre du ber om svart-hvitt — noe som er verdt å gjøre på en
  svart-hvit skriver, som gjør farger valgt for å skilles på skjermen til omtrent samme grå.

- **Innstillinger → Grafisk ruteplan er nå ordnet etter hva hver innstilling påvirker.** Det ruteplanen
  viser — hvilken vei tidsaksen går, hvilke minutter som tegnes, og hva togetiketten bærer — kommer først,
  for det gjelder både på skjermen og på papiret. Under det står to blokker ved siden av hverandre:
  avstandene på skjermen, i bildepunkter, og avstandene i den utskrevne rapporten, i millimeter papir. Hver
  blokk bærer de samme slags avstander, slik at innstillingen på skjermen og motstykket på papiret kan
  leses mot hverandre, og den ene ikke kan forveksles med den andre. Tallfelter er høyrejustert, slik at
  sifrene står under hverandre.

- **Du kan nå angi hva som skal gjøres med loket der et togavsnitt slutter.** Når du redigerer et
  togavsnitt under **Omløp**, stilles det to spørsmål til: skal loket snus, og skal det kjøres om til den
  andre enden av toget slik at toget kan avgå den veien det kom fra? Hver av dem skrives ut som en
  ankomstmerknad for både lokfører og togekspeditør, og ber du om begge, blir det én enkelt merknad —
  loket forlater toget, kjører til svingskiven og kommer tilbake i den andre enden — i stedet for to som
  leses som atskilte bevegelser.

  Snuing tilbys bare der driftsstedet togavsnittet slutter ved har en svingskive, som er en ny
  innstilling under **Driftssteder**; ingen andre steder har en. Omløp utelates fra merknaden når
  trekkraften på togavsnittet kan snu som den står — et motorvognsett eller et lok i et vendetog — for da
  er det ingenting å kjøre om. Det du har bedt om, beholdes i begge tilfeller, så det sier sitt igjen så
  snart et annet lok kjører togavsnittet.

- **Topologi-diagrammet tegner nå hele anleggets spor, med hvert driftssted vist én eneste gang.** Det
  var før en rekke vannrette linjer, en for hver ruteplanstrekning, og et driftssted som flere
  strekninger nådde, ble tegnet på hver av dem. Nå opptrer hvert driftssted nøyaktig én gang, og sporet
  mellom to av dem er en rett linje i den vinkelen de nå ligger i, enkelt- eller dobbeltsporet slik
  strekningen virkelig er og i fargene til de ruteplanstrekningene som går over det. Spor som ingen
  ruteplanstrekning dekker, tegnes i grått, slik at et hull i strekningene dine kan ses i stedet for bare
  å mangle. En signatur som ellers ville fått spor gjennom seg, flyttes til den siden av sirkelen som er
  renest — over, under eller ved siden av den — som er svaret der det går spor både oppover og nedover
  fra samme driftssted.

- **Du kan nå ordne Topologi-diagrammet selv.** Dra et driftssted dit det hører hjemme, så følger sporene
  med. Det legger seg i de samme radene og avstandene som den automatiske tegningen bruker, slik at det
  du flytter, havner på linje med det du lar stå. Hvor du har plassert driftsstedene, lagres med planen
  og er det som skrives ut på oversiktssiden i tjenesteheftene. **Plasser automatisk** glemmer alle
  driftsstedene du har flyttet, og tegner hele diagrammet igjen. Det er dette et anlegg med et
  trekantspor, en vendesløyfe eller to strekninger som henger sammen i begge ender, trenger: ingen regel
  som bare leser sporene, kan forventes å tegne et slikt anlegg slik det virkelig ser ut, og du vet
  hvordan det ser ut.

- **Knappene som gjelder et helt omløp, står nå i sin egen kolonne.** Under **Omløp** sto klon,
  komplementer og slett først blant togene, så togfeltene i hver rad begynte på ulike steder, og
  spørsmålet som stilles før et omløp slettes, skjøv dem enda lenger til siden. Nå står de i en kolonne
  **Handlinger** mellom kjøretøyene og togene: hver rads tog begynner på samme sted, også der de
  fortsetter på neste linje, og knappen for å slette blir stående og markeres mens spørsmålet stilles ved
  siden av den.

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
