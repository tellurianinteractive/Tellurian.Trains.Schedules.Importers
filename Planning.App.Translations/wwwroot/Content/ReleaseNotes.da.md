# Versionsnyheder

## Version 0.5.2

### Ændringer

- **De grafiske køreplaner kan nu udskrives.** En ny rapport under **Rapporter** tegner hver
  køreplansstrækning i en fast papirskala — så mange millimeter pr. hurtigurstime og pr. kilometer — og
  lægger så mange strækninger på et ark, som papiret rummer. Hvordan papiret vender følger den orientering,
  du har valgt til den grafiske køreplan: en vandret tidsakse udskrives på A4 liggende med strækningerne
  stablet under hinanden, en lodret på A4 stående med dem ved siden af hinanden.

  Fordi skalaen er fast i stedet for trykket sammen for at passe til papiret, kan tider og hældninger
  sammenlignes og måles fra det ene ark til det næste. Et tidsvindue, der er for langt til ét ark, deles
  langs tidsaksen — først ved pausen, derefter i lige store ark, der overlapper hinanden — så et tog, der
  krydser snittet, kan følges på begge ark, og det sidste ark bliver lige så fyldt som de øvrige i stedet
  for at bære nogle få minutter. Skalaen indstilles under **Indstillinger → Grafisk køreplan**; det er ved
  at mindske stationsafstanden der, at to eller tre strækninger får plads på samme ark. Togene udskrives i
  deres togartsfarver som på skærmen, medmindre du beder om sort-hvid — hvilket er værd at gøre på en
  sort-hvid printer, som gør farver, der er tydelige på skærmen, til omtrent samme grå.

- **Indstillinger → Grafisk køreplan er nu ordnet efter, hvad hver indstilling påvirker.** Det, køreplanen
  viser — hvilken vej tidsaksen løber, hvilke minutter der tegnes, og hvad togetiketten bærer — kommer
  først, for det gælder både på skærmen og på papiret. Under det står to blokke ved siden af hinanden:
  afstandene på skærmen, i billedpunkter, og afstandene i den udskrevne rapport, i millimeter papir. Hver
  blok bærer de samme slags afstande, så skærmens indstilling og dens modstykke på papiret kan læses mod
  hinanden, og den ene ikke kan forveksles med den anden. Talfelter er højrejusterede, så cifrene står
  under hinanden.

- **Du kan nu angive, hvad der skal ske med lokomotivet, hvor et togafsnit slutter.** Når du redigerer et
  togafsnit under **Omløb**, stilles der to spørgsmål mere: skal lokomotivet drejes, og skal det køres om
  til den anden ende af toget, så toget kan afgå den vej, det kom fra? Hver af dem udskrives som en
  ankomstbemærkning for både lokomotivfører og fjernstyringsleder, og beder du om begge, bliver det én
  enkelt bemærkning — lokomotivet forlader toget, kører til drejeskiven og kommer tilbage i den anden
  ende — i stedet for to, der læses som adskilte bevægelser.

  Drejning tilbydes kun, hvor det driftssted, togafsnittet slutter ved, har en drejeskive, hvilket er en
  ny indstilling under **Driftssteder**; ingen andre steder har en. Omløb udelades af bemærkningen, når
  trækkraften på togafsnittet kan vende, som den står — et motortog eller et lokomotiv i et vendetog —
  for så er der intet at køre om. Det, du har bedt om, bevares i begge tilfælde, så det siger sit igen,
  så snart et andet lokomotiv kører togafsnittet.

- **Topologi-diagrammet tegner nu hele anlæggets spor, med hvert driftssted vist én eneste gang.** Det
  var før en række vandrette linjer, en for hver køreplansstrækning, og et driftssted, som flere
  strækninger nåede, blev tegnet på hver af dem. Nu optræder hvert driftssted præcis én gang, og sporet
  mellem to af dem er en lige linje i den vinkel, de nu ligger i, enkelt- eller dobbeltsporet som
  strækningen virkelig er og i farverne på de køreplansstrækninger, der går over det. Spor, som ingen
  køreplansstrækning dækker, tegnes i gråt, så et hul i dine strækninger kan ses i stedet for bare at
  mangle. En signatur, der ellers ville få spor gennem sig, flyttes til den side af cirklen, der er
  renest — over, under eller ved siden af den — hvilket er svaret, hvor der går spor både opad og nedad
  fra samme driftssted.

- **Du kan nu selv arrangere Topologi-diagrammet.** Træk et driftssted hen, hvor det hører til, så følger
  sporene med. Det lægger sig i de samme rækker og afstande, som den automatiske tegning bruger, så det,
  du flytter, kommer på linje med det, du lader stå. Hvor du har placeret driftsstederne, gemmes med
  planen og er det, der udskrives på oversigtssiden i tjenestehæfterne. **Placer automatisk** glemmer
  alle de driftssteder, du har flyttet, og tegner hele diagrammet igen. Det er dette, et anlæg med et
  trekantspor, en vendesløjfe eller to strækninger, der hænger sammen i begge ender, har brug for: ingen
  regel, der kun læser sporene, kan forventes at tegne et sådant anlæg, som det virkelig ser ud, og du
  ved, hvordan det ser ud.

- **Knapperne, der gælder et helt omløb, står nu i deres egen kolonne.** Under **Omløb** stod klon,
  komplementer og slet forrest blandt togene, så togfelterne i hver række begyndte forskellige steder, og
  spørgsmålet, der stilles, før et omløb slettes, skubbede dem endnu længere til siden. Nu står de i en
  kolonne **Handlinger** mellem køretøjerne og togene: hver rækkes tog begynder samme sted, også hvor de
  fortsætter på næste linje, og knappen til at slette bliver stående og markeres, mens spørgsmålet
  stilles ved siden af den.

## Version 0.5.1

### Ændringer

- **Hvad der skal gøres med lokomotiverne, står nu i førernes tjenestehæfter og i
  togekspeditionslisterne.** Hvilket lokomotiv der skal bruges, hvad der skal kobles til og fra, og at det
  skal hentes fra — eller køres tilbage til — opstillingssporet, blev hele tiden regnet ud af
  materielomløbene, men aldrig skrevet ud; nu står de blandt de øvrige noter ved den standsning, de hører
  til, og både føreren og togekspedienten ser dem. Nyt blandt dem er beskeden til et lokomotiv, der skal
  køres rundt til den anden ende af toget, eller vendes, før toget kører tilbage.

- **Hæftet med generelle instruktioner udskriver nu hele din tekst, på sider der kan læses.** En side blev
  regnet for rummeligere, end den faktisk er, så det der løb forbi bundkanten faldt stiltiende bort;
  teksten fortsætter nu på den næste side i stedet, og en side slutter aldrig med en overskrift alene.
  **Topologi** og **Rangerbanegårde** kommer nu på hæftets allersidste side ligesom i tjenestehæfterne, og
  programmet på forsiden er sat i hæftets egne størrelser i stedet for browserens.

## Version 0.5.0

### Ændringer

- **Et vendetog står ikke længere og venter på lokomotivrundgang.** Sæt kryds i den nye boks **Vendetog?**
  på et lokomotiv under **Omløb**, hvor det fremfører et tog, der kan køres fra begge ender — et tog med
  styrevogn eller endnu et lokomotiv i den anden ende — så regner **Opdatér tider** rundgangen fra og lader
  toget stå det korteste ophold i stedet, hvilket fremrykker alle følgende ophold. Et motorvognstog
  behandles på samme måde uden noget at sætte kryds i, og et ophold, du bevidst har gjort længere, bliver
  stående, som du har sat det.

- **Et spor kan nu angive, hvilken vej gennem driftsstedet det er beregnet til.** Hvert spor kan angive det
  **forrige** driftssted, et tog kommer fra, det **næste**, det fortsætter til, eller begge — med feltet
  **begge retninger** — og et nyt tog lægges på det spor, der passer bedst til dets vej. Det er netop, hvad
  en **dobbeltsporet strækning** har brug for: giv de to spor samme par driftssteder omvendt, så holder
  hver retning sig til sit spor. Hvor to spor passer lige godt, tager et persontog, der standser, et spor
  med perron, mens et tog, der kører igennem, tager hovedsporet; lad kolonnerne stå tomme, så ændres intet
  i forhold til før.

- **Et tog kan nu kopieres i modsat retning og gentages.** Sæt flueben i **Modsat retning?**, så kører
  kopien strækningen baglæns, med alle køretider og ophold bevaret, forberedelses- og afslutningstiden
  byttet ende og et nummer fra den modsatte retnings række. Kopidialogen har nu også valget **Gentag tog**,
  så et tog kan oprettes for sig, justeres, til det kører, som det skal, og først derefter gentages hen
  over dagen.

- **Et spor kan nu angive, hvor lang dets perron er.** Hvert spor på et driftssted, der udveksler
  passagerer, har en **perronlængde** i meter — over nul betyder, at passagerer kan stige på og af der — og
  et nyt passagertog lægges på et spor med perron, hvor driftsstedet har en. Sæt flueben i
  **Passagerer?**, så får hvert spor en perron på én meter, som du kan justere, og en plan oprettet før
  dette behandles på samme måde, første gang den åbnes, så den fungerer nøjagtig som før, indtil du
  afkorter eller nulstiller de spor, der i virkeligheden ingen perron har. Et passagertog, der standser for
  passagerudveksling ved et spor uden perron, står nu under **Konflikter**: giv enten sporet en
  perronlængde eller fjern fluebenene i standsningens **Ank** og **Afg**, hvilket siger, at toget intet
  udveksler der. Kontrollen kan slås fra under **Indstillinger › Validering**.

### Fejlrettelser

- **At give anlægget et nyt navn ændrer nu navnet alle de steder, det vises.** Forsiden på hæftet med de
  generelle instruktioner, navnet i den øverste linje og filnavnet, en plan gemmes under, blev alle ved med
  at vise, hvad anlægget hed før. En plan, der har fået nyt navn tidligere, rettes næste gang den åbnes.

## Version 0.4.2

### Ændringer

- **Nu kan et tog sættes ind midt i et omløb.** Mellem togafsnittene på en række er der nu små samlinger,
  der viser, hvor køretøjet holder og hvor længe, og før det første afsnit en, der viser, hvorfra det skal
  hentes; klik på en af dem for at sætte et tog ind i hullet, så tilbydes kun de tog, køretøjet faktisk kan
  nå. En tur, der ikke bringer køretøjet tilbage, sættes ind alligevel og rapporteres som en konflikt,
  indtil du sætter returen ind — sådan passes en tur-retur ind i et ophold. En samling, hvor omløbet er
  brudt, som en import kan efterlade det, er markeret med gult.

- **Appen har fået sit eget ikon** — fronten af et moderne tog på en mørkeblå flade — i stedet for mærket,
  der følger med de værktøjer, den er bygget med. Ikonet ses i browserens faneblad og på hjemmeskærmen
  eller i Start-menuen for den, der installerer appen.

- **Der er nu plads til tolv omløbskort på et ark i stedet for ti.** Kortene er 48 mm brede i stedet for
  50, så seks kan være ved siden af hinanden på et liggende A4-ark, og arket har stadig en margen, som
  almindelige printere kan nå. Kortene er lige så høje som før, og indholdet er uændret.

- **Rækkerne i køreplanen står nu længere fra hinanden.** Der er nu en syvendedel mere luft omkring hver
  linje, så en række er lettere at følge tværs over siden, og en station lettere at finde i kolonnen.
  Skriften og kolonnerne er uændrede, så bladet rummer de samme tog; en side tager nu niogtredive linjer i
  stedet for femogfyrre.

### Fejlrettelser

- **Den udskrevne køreplan mister ikke længere de sidste rækker på en side.** Begge retninger af en
  strækning blev sat på samme side, også når de ikke begge kunne være der, og rækkerne, der blev til
  overs, blev klippet af — rapporten på skærmen var sat i en større skrift end den udskrevne, så dens
  rækker var næsten to tredjedele højere end dem, der blev talt. De to sættes nu ens, hvor meget der kan
  være måles på en virkelig side i stedet for at blive regnet ud fra skriftstørrelsen, og tre linjer holdes
  frie nederst på hver side.

- **Godsstrømslisten nævner nu de destinationer, vognene skal til.** Under **Godsstrøm › Godstog** stod der
  kun "Vogne til" i listen, man vælger fra, uden destinationerne, så posterne ikke kunne skelnes fra
  hinanden. Underfanen og dens kolonne hedder nu **Godsdestinationer** i stedet for *Godsbeskrivelser*.

## Version 0.4.1

### Ændringer

- **Togekspeditionslisterne kan nu gemmes som dokumenter, stationsejerne kan redigere.** Vælg
  *Togekspeditionslister* i menuen Eksportér, så får hver bemandet station sit eget dokument i
  OpenDocument-format, tænkt til at sende hver ejer deres egen liste før træffet, så de kan tilføje de
  lokale instruktioner, kun de kender; er mere end én station bemandet, kommer dokumenterne samlet i en
  zip-fil. Hvor siderne brydes, er overladt til tekstbehandleren, så siderne brydes fornuftigt også efter,
  at ejeren har skrevet — stationens navn, telefonnumrene til de stationer, den ekspederer tog til og fra,
  og kolonneoverskrifterne gentages øverst på hver side, men den del af døgnet, en side dækker, kan ikke
  angives, så siderne nummereres i stedet. De udskrevne ark i menuen Rapporter er uændrede og er fortsat
  dem, man arbejder fra under en køresession.

- **Et tog, der trækkes af to lokomotiver på én gang, siger nu hvilke to.** Konflikten nævnte kun toget og
  minutterne, så var begge booket over nøjagtig samme strækning, lød dens to halvdele ord for ord ens. Den
  markeres nu også kun på de to omløb, der holder det dobbeltbookede arbejde, i stedet for på hvert omløb,
  der kørte det tog et sted på dagen.

- **To lokomotiver, der deles om et tog mellem køresessioner, rapporteres ikke længere som en konflikt.**
  Kun klokkeslættene blev sammenlignet, så et lokomotiv på ulige køresessioner og et andet på lige — hele
  pointen med at lægge det sådan an — blev rapporteret som dobbelttrækning. Nu rapporteres det kun, hvor
  begge er booket på en fælles køresession, og konflikten nævner de køresessioner.

## Version 0.4.0

### Brydende ændringer

- **Et køretøj, du opretter, identificeres nu af sin operatør og sit nummer.** På én og samme køresession
  må kombinationen kun tilhøre ét køretøj, uanset hvilken slags køretøj det er, så et vognsæt og et
  lokomotiv kan ikke længere begge være *DB 5*; et køretøj uden operatør identificeres af nummeret alene,
  og to køretøjer må dele identitet, så længe de køresessioner, de kører, ikke overlapper. Et
  **importeret** køretøj identificeres fortsat af det eksterne id, det blev importeret med, så en
  importeret plan giver ingen nye konflikter af dette. At tilføje eller rette et køretøj afviser nu en
  identitet, som et andet køretøj allerede har, og kræver et nummer, mens eksisterende planer bevares
  præcis som de er, med hvert køretøj, der deler identitet, blandt konflikterne.

### Ændringer

- **Der er en ny rapport: togekspeditionslisten.** Et sæt ark for hver bemandet station med de tog,
  stationen ekspederer, i tidsrækkefølge — et tog, der holder der, optræder to gange, ankomster på hvid
  baggrund og afgange på lysegul, fordi det at ekspedere et tog ind og at ekspedere det videre er to
  forskellige handlinger, og tog, der blot kører igennem, er også med. Hver side har stationens navn, den
  del af døgnet siden dækker, og telefonnumrene til stationerne i den anden ende af
  togekspeditionsstrækningerne, og hver række har et felt pr. køresession til at krydse af. Hver station
  begynder på en ny side, så bunken kan deles og uddeles; udskrives fra menuen Rapporter.

- **Felterne til at tilføje og rette et køretøj har fået ny rækkefølge,** den samme begge steder:
  køretøjstype, trækkrafttype, antal enheder, operatør, nummer, klasse, køresessioner og til sidst det
  eksterne id. Feltet, der før hed *Selskab*, hedder nu *Operatør*.

- **Et eksternt id kan rettes, men ikke længere opfindes.** Det eksterne id er det navn, et tog eller et
  køretøj bærer i det system, det blev importeret fra, så det, der er importeret med et id, har stadig sit
  felt og kan rettes der, mens det, der aldrig har haft et id, nu intet felt har at skrive i. Et køretøj,
  du opretter i planlæggeren, får derfor slet intet eksternt id, hvor det før fik et opdigtet af klasse og
  nummer.

- **Den mindste tid mellem to anvendelser af samme spor kontrolleres nu.** Indstillingen fandtes, men intet
  brugte den: står den på 0, hvor den begynder, ændres intet i kontrollen. Sæt den til 5, og sporet skal
  desuden være frit i fem minutter mellem to tog — præcis fem er nok, fire er ikke — og konflikten angiver,
  hvor kort mellemrummet faktisk er, og hvor langt det skulle være.

- **Et driftssted kan nu have sine egne instruktioner.** Redigeringsformularen har feltet
  **Instruktioner**, skrevet i Markdown ved siden af en forhåndsvisning, til hvordan netop det driftssted
  køres på dette træf: hvilke spor der bruges til hvad, hvordan rangeringen er tilrettelagt, og hvad
  lokoførerne og dem, der bemander stedet, ellers har brug for at vide. Feltet tilbydes på en station eller
  et industriområde og vises i driftsstedets Info-visning; det tilbydes ikke, hvor der intet er at
  instruere om.

- **Et sted, hvor der køres gods uden bemanding, kan nu kræve en nøgle.** Vælg den bemandede station, der
  opbevarer nøglen, under **Nøgle opbevares på**, og navngiv nøglen, hvis stationen opbevarer flere — et
  godstog, der standser begge steder, får da ved afgangen beskeden *hent nøgle A1 til oplåsning af Bruket*
  og ved næste standsning der *aflever nøgle A1 fra Bruket*. Nøglen hentes ved den sidste standsning før
  arbejdet og afleveres ved den første derefter, og et tog, der blot kører forbi, får ingen besked. Markér
  stedet som bemandet, eller tag bemandingen af den station, der opbevarer nøglen, så holder nøglen op med
  at gælde — **Konflikter** fortæller, hvilken ændring der gjorde det, og nøglen bevares, så den gælder
  straks igen, hvis du fortryder ændringen.

### Fejlrettelser

- **To strækninger, der udgår fra samme driftssted, blev tegnet, som om de aldrig mødtes.** Begyndte en
  køreplanstrækning netop på det første driftssted på en anden, var der ingenting, der bandt de to sammen i
  Topologi-diagrammet. Den anden forlader nu det driftssted som enhver anden gren, i samme faste vinkel.

- **Hver grænseværdi for kontrollerne angiver nu, hvilket ur den måles efter.** Den mindste tid mellem to
  anvendelser af samme spor manglede helt en enhed, og de to toghastigheder angav kun *ur-minutter*. Alle
  tre angiver nu hurtigursminutter — det ur, togene kører efter, ikke virkelig tid.

- **Længder og distancer skrives nu ud i meter,** ligesom tælleren i toghastighederne, så *m* ikke kan
  tages for et minut. Mindste ophold på en station angives nu også i hurtigursminutter.

## Version 0.3.5

### Fejlrettelser

- **En gemt plan kunne nægte at åbne.** At åbne en plan, som appen lige havde gemt, blev afbrudt med en
  fejl om et land, og der blev ikke indlæst noget. En allerede gemt plan åbnes, som den er; du behøver ikke
  gøre noget ved den.

- **En gemt planfil er omkring syv gange mindre.** Gemningen skrev planen i en anden form end den, der
  holdes i browseren, så hvert ophold blev skrevet to gange, og hver togkategori, hver operatør og hvert
  land igen ved hvert tog, hvert køretøj og hver tjeneste, der brugte det. En fil, der fyldte 8 MB, fylder
  nu godt 1 MB; en plan gemt af en tidligere version kan stadig åbnes.

## Version 0.3.4

### Ændringer

- **Felterne Ank og Afg på et stop følger nu, hvor toget faktisk kan standse.** Et persontog har brug for
  et driftssted, der tager imod passagerer, og et godstog et, der tager imod gods, og ingen af delene kan
  lade sig gøre på et signalstyret driftssted; hvor toget ikke kan standse, vises begge felter tomme og kan
  ikke sættes, og stoppet er en gennemkørsel. Intet af det, du har planlagt, smides væk — slå udvekslingen
  til igen, så er stoppene der — og en skyggebanegård har altid udveksling af både passagerer og gods, da
  den repræsenterer alt uden for anlægget.

- **Et stop, som noget afhænger af, kan ikke længere fjernes.** Togets eget første og sidste stop, og
  enderne på hvert togafsnit, som et materielomløb, en tjeneste eller et godsflow er planlagt over,
  beholder nu deres felt sat og låst, og holder du markøren over det, fortælles det, hvad der holder det.
  Hvor et togafsnit slutter et sted, toget ikke kan standse, siges det ligeud, så du kan flytte stoppet
  eller togafsnittet.

- **En togkategori bærer nu de forberedelses- og afslutningstider, dens tog planlægges med,** så du ikke
  længere skal skrive de samme to tal for hvert tog. Ved siden af hvert felt er der en knap *Anvend igen*,
  som giver den ene tid til alle de tog, kategorien allerede har, og fortæller hvor mange der blev ændret;
  de to er hver sin handling, og at anvende igen flytter kun minutterne yderst på et tog.

- **Operatørerne er lettere at læse på forsiden af et tjenestehæfte.** Linjen sættes nu i dobbelt
  størrelse, så et logo er stort nok til at genkendes med et blik og en signatur stor nok til at læses
  tværs over et bord. Har alle operatører et logo, udelades ordet *Operatør*; mangler en af dem et logo,
  står alle med signatur, med fed skrift og med etiketten bevaret.

### Fejlrettelser

- **Et tjenestehæfte kunne udskrive et togafsnit ud over sidens nederste kant.** Hver side blev regnet med
  omkring halvdelen mere plads, end en A5-side faktisk har, og det, der går ud over sidekanten, skæres væk
  uden varsel, så det andet togafsnit på en sådan side manglede slutningen af sin køreplan eller manglede
  helt. Togafsnit måles nu mod det, siden faktisk rummer, så nogle hæfter får et ark mere end før.

- **Topologi-diagrammet kunne skrive signaturerne for to driftssteder oven på hinanden.** Driftsstederne
  blev placeret alene efter afstanden mellem dem, så to, der ligger tæt på hinanden på en lang strækning,
  blev tegnet næsten samme sted. De tegnes nu aldrig tættere på hinanden, end deres signaturer har brug
  for, og en lang signatur ved diagrammets kant bliver ikke længere skåret væk.

- **En gren i Topologi-diagrammet kunne tegnes tværs gennem en anden strækning.** En gren falder væk i en
  fast vinkel, så en gren, der mødte en strækning i vejen, blev simpelthen tegnet tværs over den. De grene,
  der forlader en strækning længst ude, tegnes nu først, så en lang gren kan nu blive tegnet under en kort
  gren, der forlader strækningen længere ude.

- **En plan kunne vise sine tog under togkategorier, som fanen Togkategorier ikke havde.** Flere kategorier
  kunne også tages for en og samme, så deres tog blev samlet under en enkelt overskrift, og to tog af
  forskellige kategorier med samme nummer blev meldt som ét nummer brugt to gange. Når en plan åbnes,
  fyldes listen over kategorier nu op med de kategorier, togene bruger, og hver kategori holdes adskilt fra
  de andre.

- **To selskaber, der aldrig havde fået deres eget nummer, blev taget for den samme operatør,** så tog fra
  forskellige selskaber, der delte tognummer, blev meldt som ét nummer brugt to gange. Hvert selskab får nu
  sit eget nummer, når en plan åbnes eller gemmes; et selskab fra Module Registry beholder det nummer, det
  kom med.

- **En plan gemte sine togkategorier, selskaber og lande flere steder** — hver enkelt blev skrevet der,
  hvor den først blev mødt, som regel inde i det første tog, der brugte den. Hver enkelt skrives nu én
  gang, i sin egen liste, og alt, der bruger den, beholder kun en henvisning; lande kopieres slet ikke
  længere ind i planen, så en rettelse af et lands sprog nu også når planer, der er gemt forinden.

- **Et tjenestehæfte angav kun tognummeret i overskriften for et togafsnit.** Et tog identificeres lige så
  meget af kategoriens præfiks og suffiks som af nummeret — Gt 1234, ikke 1234 — og overskriften er alt, en
  lokofører har at sammenligne med køreplanen. Den viser nu hele togidentiteten, efter operatørens
  signatur.

## Version 0.3.3

### Ændringer

- **Konflikter kan nu læses dér, hvor de vises.** En række med konflikter — et tog eller en togkategori
  under **Tog**, et omløb eller et af dets køretøjer under **Omløb**, en tjeneste under **Tjenester** — har
  nu et advarselssymbol, og et klik på det åbner meddelelserne som en læsbar liste. Symbolet får farve
  efter den alvorligste konflikt og tæller dem; hidtil stod de kun i et lille felt, der kom frem, mens
  markøren hvilede på rækken.
- **En togkategori viser konflikterne for togene i den**, så de ikke længere skjules, når kategorien
  lukkes.
- **Fanen Tog åbner nu på listen over togkategorier**, hvor togene er skjult, indtil du åbner en kategori.
  *Udvid alle* åbner dem alle på én gang, og en kategori åbner af sig selv, når du føjer et tog til den
  eller flytter et tog derind.
- **Når et togafsnit i et omløb redigeres, står der nu, hvilke slags køretøjer omløbet gælder** —
  lokomotiv, togsæt eller vognsæt. Hver slags nævnes én gang, og peger du på den, nævnes køretøjerne selv.

### Fejlrettelser

- **Appen kunne holde op med at gemme dit arbejde uden at sige det.** En plan, appen ikke kunne skrive ud —
  et tog med færre end to standsninger eller en køreplansstrækning, hvor alle banestykker var fjernet — fik
  lagringen til at mislykkes lydløst, så alt derefter blev stående på skærmen, men blev aldrig gemt. Begge
  planer kan nu gemmes, og mislykkes en lagring alligevel, siger den øverste linje det med det samme.

- **En gemt planfil er omkring 40 % mindre.** Hver standsning blev skrevet to gange — én gang i sit tog og
  én gang under det spor, den ligger på — og den anden kopi trak store dele af resten af planen med sig. En
  plan gemt med en tidligere version kan stadig åbnes.

- **Et tog, der er efterladt uden trækkraft på en del af sit løb, rapporteres nu.** Kontrollen spurgte kun,
  om et lokomotiv eller togsæt kørte toget *et eller andet sted*, så når et omløb blev afkortet i den ene
  ende, stod resten af toget uden trækkraft, uden at der blev sagt noget. Nu kontrolleres hver strækning
  for hver køresession, toget køres, og konflikten siger, mellem hvilke driftssteder og i hvilke
  køresessioner; planer, der så rene ud, kan nu rapportere dette.

## Version 0.3.2

### Ændringer

- Under **Godsstrøm › Godsbeskrivelser** kan en oprindelse eller en destination nu være et hvilket som
  helst driftssted, der udveksler gods, ikke kun en station — et industriområde håndterer altid godsvogne,
  men kunne ikke vælges før. De samme lister siger nu **driftssted**, hvor de sagde *station*.
- Et togs ophold vises altid i den **rækkefølge, toget kører** dem.
- At ændre en tid for et ophold i fanen **Tog** **tager nu resten af toget med sig**: en **afgang** virker
  fremad, den vej toget kører, og en **ankomst** baglæns, så løbet frem til ændringen følger med. Tiderne
  på den anden side bliver stående, køre- og opholdstiderne bevares, og ændringen afvises, hvis den ville
  føre toget uden for planens driftstider.
- Et tog, hvis togvej **springer et driftssted over** — to ophold i rækkefølge uden en strækning imellem —
  rapporteres nu som en konflikt. Den kan slås fra under **Indstillinger › Validering**.
- Et togafsnit i et **omløb** kan nu **redigeres**: pennen åbner dets fra- og til-stop, så et omløb kan
  formes om, uden at alt efter det fjernes. Et tilstødende togafsnit, der slutter til det, du ændrer,
  følger med; et naboafsnit, hvis eget tog ikke standser på det nye stop, står uændret, og hullet
  rapporteres som en konflikt, du selv løser.
- **Tilføj tog** kan nu oprette **returtoget** samtidig. Sæt kryds i *Retur?*, så oprettes toget tilbage
  sammen med det første, med samme strækning i modsat retning, samme togart og hastighed og det næste
  nummer i den modsatte retning; afgangen er enten *så tidligt som muligt* eller et tidspunkt, du
  indtaster. Sammen med *Gentag?* gentages begge retninger.

### Fejlrettelser

- **Kilometertallene** i den udskrevne køreplan og langs den grafiske køreplan afrundes nu til hele
  kilometer, og en sidebane viser samme kilometertal som den bane, den udgår fra, ved forgreningsstationen.
- Alt, der læser et togs togvej, følger nu **den rækkefølge, toget kører sine stop i**, ikke den
  rækkefølge, de blev indtastet. For et tog, hvis stop er indtastet i forkert rækkefølge, gik linjen i den
  **grafiske køreplan** i siksak, kunne den udskrevne **køreplan** vise en afgang, hvor toget ankommer,
  kædede **byg automatisk** slet ikke toget, målte **gentag tog** intervallet fra det forkerte stop, og
  genberegning af tiderne mislykkedes helt. Importerede planer har aldrig været berørt.
- **Toghastigheden kontrolleres nu også på den sidste strækning**, ind til det driftssted, hvor toget
  slutter sit løb.

## Version 0.3.1

### Ændringer

- Afsnittet **Trækkraftenheder** på siden for et togafsnit i hæftet Førertjenester har nu sin overskrift på
  det valgte sprog. Det var den eneste overskrift i hæftet uden oversættelse.
- Trækkraftenheden udskrives nu for hvert togafsnit, der har en. I planer importeret med en tidligere
  version viste nogle togafsnit en trækkraftenhed under **Tjenester**, men ingen i hæftet.
- Noter om tog i samme retning fortæller nu, hvilket tog der kommer forbi det andet — **Overhaler GD 42757
  12:02-12:05** eller **Overhales af GD 42757 12:02** — i stedet for det hidtidige *"Møder GD 42757 i samme
  retning"*, der aldrig sagde, hvilket tog der kom foran. To tog, der blot står på samme station samtidig,
  giver ingen note overhovedet.
- Et møde uden varighed — det andet tog kører igennem uden ophold — skrives som ét klokkeslæt i stedet for
  et interval fra et tidspunkt til sig selv.
- Et tog, der begynder eller afslutter sin kørsel på en station, medtages ikke længere som mødt, krydset
  eller overhalet der. De tidspunkter er, når dets lokofører møder ind eller går af.

## Version 0.3.0

### Ændringer

- En ny rapport, **Førertjenester**, udskriver ét A5-hæfte pr. tjeneste. Forsiden viser tjenestens nummer,
  hvilke sessioner eller dage den kører, dens start- og sluttidspunkt og -stationer, en sværhedsgrad,
  bemandingsbehov og eventuelle tjenestenoter; hvert togafsnit får derefter sin egen side med hvilke
  trækkraftenheder der skal bruges, hvilke vognsæt der skal medbringes, til hvilke destinationer der skal
  medbringes godsvogne, samt køreplanen, hver i sin egen blok.
- En ny rapport, **Generelle instruktioner**, er et separat hæfte med træffets program og de instruktioner,
  der gælder for anlægget i hele træffets varighed — køreinstruktioner, signalgivning, radio- og
  telefonbrug, hvad man gør ved forsinkelser og hvem man spørger — og det uddeles én gang til alle. Det
  indledes med træffets navn og datoer, så programmet, enhver deltager har brug for at vide før den første
  session, og derefter instruktionerne over så mange sider, som de har brug for, brudt mellem afsnit og
  aldrig med en overskrift efterladt alene.
- Sidste side i begge hæfter viser anlæggets sporplan og tabellen over rangerbanegårde, så også de, der
  aldrig har et tjenestehæfte i hånden — først og fremmest stationspersonalet — får et overblik over
  anlægget.
- Både programmet og instruktionerne skrives under **Indstillinger › Information** og kan formateres med
  Markdown. Begge hæfter udskrives i A5: A4 liggende, dobbeltsidet, foldet på midten, med tomme sider
  tilføjet hvor det er nødvendigt, så arkene foldes korrekt.
- Tjenester kan nu graderes **Let**, **Middel** eller **Erfaren**, vist farvekodet på hæftet, kan angive,
  at de kræver to eller tre personer — for eksempel en lokofører og en konduktør — og kan fastgøres til et
  **fast nummer**, som automatisk omnummerering lader urørt.
- Planen kontrolleres nu også, så hvert togafsnit med et lokomotiv eller togsæt tildelt har en
  førertjeneste, der dækker det i hver session, det kører. En tjeneste med fast nummer skal have et nummer,
  og ingen to sådanne må få samme nummer.
- Selskaber kan nu have et uploadet **logo**, vist på rapporter i stedet for tekstsignaturen.
- Stationer kan nu markeres som den **rangerbanegård**, der betjener en anden lokalitets lokalgods, og
  anlægget lister hver rangerbanegård og hvad den dækker på tjenestehæftets sidste side.
- Hver køreplansstrækning kan nu tildeles en **farve**, som bruges til at tegne den i Topologi-diagrammet.
- En ny **afstandsfaktor** (Indstillinger › Tid & hastighed) lader et anlæg vise et større, mere
  forbilledetro kilometertal i rapporter og den grafiske køreplan end den afstand, der faktisk er
  modelleret, uden at det påvirker nogen køretidsberegning.
- Appen holder nu flere åbne browserfaner eller -vinduer synkroniseret med hinanden. **Bemærk**, at dette
  kun virker mellem vinduer på samme maskine i samme browser.
- Indstillinger kan nu gemme træffets **gælder fra**- og **gælder til**-datoer, trykt som en gyldighedslinje
  på rapporter; lad dem stå tomme, hvis intet træf er booket endnu.
- En ny indstilling, **udvid plantider automatisk?** (Indstillinger › Generelt), udvider planens start-
  eller sluttidspunkt for at dække et tog i stedet for at blokere ændringen. Slået fra som standard.
- En ny knap, **opdatér alle tider**, i den grafiske køreplan genberegner alle tog i køreplanen på én gang
  i stedet for først at skulle vælge en delmængde.
- Sporbelægningskontrollen kan nu valgfrit tage højde for, at et lokomotiv eller togsæt holder på et spor
  mellem to tog, medmindre det er booket til eller fra opstilling (Indstillinger › Validering). Slået fra
  som standard, da det kun giver mening på anlæg, hvor opstilling er modelleret bevidst.
- Hvert ophold i fanen **Tog** har nu et felt til **Bemærkning** — en note, der udskrives ved det ophold,
  for eksempel "vent på modkørende tog". Bemærkningen vises færdigformateret og skifter til den rå
  opmærkning, så snart du går ind i feltet, så skriv `*langsomt*` for kursiv og `**første**` for fed.

### Fejlrettelser

- Når man tilføjer et nyt tog, sættes dets standardstarttidspunkt nu under hensyn til den angivne
  forberedelsestid, så det ikke starter før planens starttidspunkt.

## Version 0.2.4

### Ændringer

- En ny fane **Tjenester** lader dig planlægge førertjenester — det arbejde, en lokofører udfører i løbet
  af en session, som en række af de togafsnit, føreren kører. Hver tjeneste er en række: dens betegnelse,
  firma og sessioner til venstre, togafsnittene i køreorden til højre.
- Tilføj de togafsnit, en fører kører, med **Tilføj togafsnit**. Listen viser de trækkraftstrækninger, en
  fører kan tage som det næste — dem, der ikke støder sammen i tid med tjenesten, og, når den har et
  togafsnit, dem, der afgår ved eller efter, at det ankommer. Togafsnittene behøver ikke starte på samme
  station: føreren går ganske enkelt hen, hvor det næste starter.
- Det samme togafsnit kan køres af flere tjenester, så længe de kører i forskellige sessioner, så én
  tjeneste kan dække de ulige sessioner og en anden de lige.
- Hvor to togafsnit for samme tog i en tjeneste køres af forskellige trækkraftenheder, viser fanen en note
  ved stationen, hvor trækkraftenheden skiftes — du indtaster den ikke i hånden.
- Tjenester importeret fra XPLN deler nu de togafsnit, der er defineret i køretøjernes omløb, så hvert
  togafsnit viser den trækkraftenhed, der kører det.
- Planen kontrolleres, så intet togafsnit køres af to tjenester i samme session, og ingen tjeneste har
  togafsnit, der overlapper i tid. Kontrollen kan slås fra under **Indstillinger › Validering**.

## Version 0.2.2

### Fejlrettelser

- To tog, der aldrig kører i samme køresession, rapporteres ikke længere som et møde på en enkeltsporet
  strækning. Et tog, der kører session 1, 3, 5, og et, der kører 2, 4, 6, er aldrig ude samtidig.
- Konfliktkontrollen på dobbeltsporede og flersporede strækninger er nu præcis: en strækning markeres kun,
  når der er flere tog på den samtidig, end den har spor, og kun tog, der kører i en fælles session, tælles
  med.

## Version 0.2.1

### Ændringer

- Konfliktadvarsler vises nu, hvor du kan rette dem: togkonflikter i den grafiske køreplan og på fanen
  **Tog**, køretøjs- og omløbskonflikter på fanen **Omløb**.
- På fanen **Omløb** fremhæver en køretøjskonflikt nu kun det pågældende køretøj, og en omløbskonflikt kun
  det pågældende omløb.
- Kontrollen af, at et køretøj vender tilbage til sit udgangspunkt, omfatter nu også vognsæt og gods, ikke
  kun lokomotiver og togsæt.

## Version 0.2.0

### Ændringer

- Navnet på den plan, du arbejder med, vises nu øverst i vinduet.
- Den grafiske køreplan viser nu søjler for lokomotivførerbehovet, hvilket gør det lettere at se, hvor
  mange førere der er brug for gennem køresessionen.
- En ny **Topologi**-visning (under fanen **Strækninger**) viser et skematisk diagram over køreplanens
  strækninger og deres grene.

### Fejlrettelser

- Strækninger bevarer nu som standard den rækkefølge, du indtastede dem i. Du kan stadig sortere efter
  enhver kolonne.
- Konflikter henviser ikke længere til tog, du ikke kan finde: når et tog slettes, fjernes dets stop sammen
  med det, så der ikke er forældreløse stop eller falske konflikter tilbage.

## Version 0.1.0

Første forhåndsvisning af Køreplanlæggeren. Du kan:

- Definere sporplaner med stationer, spor og strækninger.
- Oprette og redigere togkøreplaner med automatisk tidsberegning.
- Tildele lokomotiver og togstammer til tog.
- Bygge køretøjsomløb og udskrive omløbskort.
- Planlægge godsstrømme mellem stationer.
- Vise grafiske køreplaner (tid-afstands-diagrammer).
- Validere køreplaner for konflikter og inkonsistenser.
- Generere udskrifter: togkort, stationsbøger og vagtplaner.
- Arbejde på engelsk, tysk, dansk, norsk og svensk.
