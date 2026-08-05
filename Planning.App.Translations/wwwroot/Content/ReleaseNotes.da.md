# Versionsnyheder

## Version 0.4.0

### Brydende ændringer

- **Et køretøj, du opretter, identificeres nu af sin operatør og sit nummer.** De to tilsammen udpeger
  ét virkeligt køretøj, så på én og samme køresession må kombinationen kun tilhøre ét køretøj — uanset
  hvilken slags køretøj det er. Et vognsæt og et lokomotiv kan ikke længere begge være *DB 5*. Et
  køretøj uden operatør identificeres af nummeret alene. To køretøjer må stadig have samme operatør og
  nummer, så længe de køresessioner, de kører, ikke overlapper, for så er de aldrig til træffet samtidig.

  Et **importeret** køretøj identificeres fortsat af det eksterne id, det blev importeret med, og som
  allerede er entydigt i den plan, det kom fra, så en importeret plan giver ingen nye konflikter af dette.

  At tilføje eller rette et køretøj under fanen Omløb afviser nu en identitet, som et andet køretøj
  allerede har, og et nummer skal angives. Planer lavet før denne regel bevares præcis som de er — der
  bliver ikke omnummereret for dig — og hvert køretøj, der deler identitet, står blandt konflikterne,
  én gang hver, så du kan se, hvad der skal have et nyt nummer.

### Ændringer

- **Der er en ny rapport: togekspeditionslisten.** Et sæt ark for hver station, der er bemandet — alle
  bemandede stationer og alle skyggestationer, uanset om de er bemandede — med de tog, stationen
  ekspederer, i tidsrækkefølge. Et tog, der holder på stationen, optræder to gange, én gang for
  ankomsten og én gang for afgangen, fordi det at ekspedere et tog ind og at ekspedere det videre til
  næste station er to forskellige handlinger med nogle minutters mellemrum. Ankomster står på hvid
  baggrund og afgange på lysegul, så de to aldrig kan forveksles. Tog, der blot kører igennem, er også
  med, for de skal også ekspederes forbi. Hver side har stationens navn, den del af døgnet siden
  dækker, og telefonnumrene til stationerne i den anden ende af togekspeditionsstrækningerne, og hver
  række har et felt pr. køresession til at krydse af undervejs, gråtonet for de køresessioner, toget
  ikke kører. Hver station begynder på en ny side, så bunken uden videre kan deles og uddeles. Udskrives
  fra menuen Rapporter.

- **Felterne til at tilføje og rette et køretøj har fået ny rækkefølge,** den samme begge steder:
  køretøjstype, trækkrafttype, antal enheder, operatør, nummer, klasse, køresessioner og til sidst det
  eksterne id — hvad køretøjet er, så hvad der identificerer det, så hvordan det beskrives, og hvornår
  det kører. Feltet, der før hed *Selskab*, hedder nu *Operatør*.

- **Et eksternt id kan rettes, men ikke længere opfindes.** Det eksterne id er det navn, et tog eller et
  køretøj bærer i det system, det blev importeret fra, så det betyder kun noget, hvor det kommer fra
  noget. Det, der er importeret med et id, har stadig sit felt — under fanen Tog og i køretøjsdialogen
  under fanen Omløb — og kan rettes der; det, der aldrig har haft et id, har nu intet felt at skrive i.
  Et køretøj, du opretter i planlæggeren, får derfor slet intet eksternt id, hvor det før fik et
  opdigtet af klasse og nummer.

- **Den mindste tid mellem to anvendelser af samme spor kontrolleres nu.** Indstillingen fandtes, men
  intet brugte den. Står den på 0 — hvor den begynder, og hvor den bliver, indtil du ændrer den —
  ændres intet i kontrollen: to tog er i konflikt, hvor de står på samme spor samtidig, og et, der
  ankommer netop som et andet kører, er en afløsning, ikke en konflikt. Sæt den til f.eks. 5, og sporet
  skal desuden være frit i fem minutter imellem dem, så en plan, der vender sporet hurtigere, end
  stationen kan nå, bliver rapporteret. Præcis fem frie minutter er nok; fire er ikke.

  En sådan konflikt angiver, hvor kort mellemrummet faktisk er, og hvor langt det skulle være, i stedet
  for at påstå, at de to tog overlapper, når tiderne viser, at de ikke gør.

- **Et driftssted kan nu have sine egne instruktioner.** Formularen til at tilføje og rette et
  driftssted har feltet **Instruktioner**, skrevet i Markdown og vist ved siden af en forhåndsvisning
  ligesom de generelle instruktioner i Indstillinger. Det er til, hvordan netop det driftssted køres på
  dette træf — hvilke spor der bruges til hvad, hvordan rangeringen er tilrettelagt, og hvad lokoførerne
  og dem, der bemander stedet, ellers har brug for at vide der. Hvordan driftsstedet betjenes i
  almindelighed, og anden beskrivelse af det, er ejerens opgave at levere og hører ikke til i feltet.
  Det, du skriver, gemmes sammen med driftsstedet og vises i dets Info-visning.

  Feltet tilbydes på en station eller et industriområde, hvor der udveksles rejsende og/eller gods. Det
  tilbydes ikke, hvor der intet er at instruere om: togene kører bare forbi et signalstyret sted, og
  ingen bemander et andet sted, så toget gør der, hvad standsningen siger, og intet mere.

- **Et sted, hvor der køres gods uden bemanding, kan nu kræve en nøgle.** Hvor sporskifterne på en
  ubemandet station eller et industriområde er aflåst, kan du i redigeringsformularen vælge den
  bemandede station, der opbevarer nøglen, under **Nøgle opbevares på**, og navngive nøglen, hvis
  stationen opbevarer flere.

  Mere skal der ikke planlægges. Et godstog, der standser på stationen med nøglen og senere standser på
  det sted, nøglen låser op, får ved afgangen derfra beskeden *hent nøgle A1 til oplåsning af Bruket*;
  næste gang toget standser der, siger ankomsten *aflever nøgle A1 fra Bruket*. Et tog, der blot kører
  forbi et af stederne, får ingen besked, for det låser intet op. Nøglen hentes ved den sidste
  standsning på stationen før arbejdet og afleveres ved den første derefter, så et tog, der standser
  der to gange, slipper for at have den med en ekstra tur.

  En nøgle betyder kun noget, så længe begge ender holder. Markér stedet selv som bemandet, eller tag
  bemandingen af den station, der opbevarer nøglen, så holder nøglen op med at gælde: der laves ingen
  beskeder ud fra den, og **Konflikter** fortæller, hvilken af de to ændringer der gjorde det. Nøglen
  bevares i stedet for at blive kastet væk, så fortryder du ændringen, gælder den straks igen, og den
  bliver stående i formularen, hvor du kan pege den mod en anden station eller fjerne den.

### Fejlrettelser

- **To strækninger, der udgår fra samme driftssted, blev tegnet, som om de aldrig mødtes.** Begyndte en
  køreplanstrækning netop på det første driftssted på en anden, var der ingenting, der bandt de to
  sammen i Topologi-diagrammet: hver blev tegnet som sin egen linje, uden gren imellem. Den anden
  forlader nu det driftssted som enhver anden gren og falder væk fra det i samme faste vinkel.

- **Hver grænseværdi for kontrollerne angiver nu, hvilket ur den måles efter.** Den mindste tid mellem
  to anvendelser af samme spor manglede helt en enhed, og de to toghastigheder angav kun *ur-minutter*,
  som kunne læses på begge måder. Alle tre angiver nu hurtigursminutter — det ur, togene kører efter,
  ikke virkelig tid.

- **Længder og distancer skrives nu ud i meter,** ligesom tælleren i toghastighederne, så *m* ikke kan
  tages for et minut. Mindste ophold på en station angives nu også i hurtigursminutter.

## Version 0.3.5

### Fejlrettelser

- **En gemt plan kunne nægte at åbne.** At åbne en plan, som appen lige havde gemt, blev afbrudt med en
  fejl om et land, og der blev ikke indlæst noget — der var ingen vej udenom. En fil læses et stykke ad
  gangen, mens den kommer ind, og læsningen af landene i den snublede over det. En allerede gemt plan
  åbnes, som den er; du behøver ikke gøre noget ved den.

- **En gemt planfil er omkring syv gange mindre.** At gemme en plan til en fil skrev den i en anden
  form end den, der holdes i browseren, så gevinsterne fra de to seneste versioner nåede aldrig frem
  til filen: hvert ophold blev skrevet to gange, og hver togkategori, hver operatør og hvert land igen
  ved hvert tog, hvert køretøj og hver tjeneste, der brugte det. En fil, der fyldte 8 MB, fylder nu godt
  1 MB og gemmes og åbnes tilsvarende hurtigere. En plan gemt af en tidligere version kan stadig åbnes.

## Version 0.3.4

- **Felterne Ank og Afg på et stop følger nu, hvor toget faktisk kan standse.** Et tog standser for
  at udveksle noget og har derfor brug for et sted at udveksle det: et persontog hvor driftsstedet
  tager imod passagerer, et godstog hvor det tager imod gods, og ingen af delene på et signalstyret
  driftssted. Hvor toget ikke kan standse, vises begge felter tomme og kan ikke sættes, og stoppet er
  en gennemkørsel i køreplanen og i grafen. Intet af det, du har planlagt, smides væk — slå
  udvekslingen til igen, så er stoppene der. En skyggebanegård har altid udveksling af både passagerer
  og gods, da den repræsenterer alt uden for anlægget, så dens to felter vises satte og låste.

- **Et stop, som noget afhænger af, kan ikke længere fjernes.** En togdel går fra et stop, hvor toget
  afgår, til et, hvor det ankommer, så begge ender skal være stop. Togets eget første og sidste stop,
  og enderne på hver togdel, som et materielomløb, en tjeneste eller et godsflow er planlagt over,
  beholder nu deres felt sat og låst; hold markøren over det, så fortælles det, hvad der holder det.
  Hvor en togdel slutter et sted, toget ikke kan standse — en plan lavet før denne regel — siges det
  ligeud, så du kan flytte stoppet eller togdelen.

- **En togkategori bærer nu de forberedelses- og afslutningstider, dens tog planlægges med.** Hvert
  nyt tog i kategorien gøres klar så mange minutter før det afgår og sættes væk så mange minutter
  efter det er ankommet, så du ikke længere skal skrive de samme to tal for hvert tog. Ved siden af
  hvert af de to felter er der en knap *Anvend igen*, som giver den ene tid til alle de tog,
  kategorien allerede har, og fortæller hvor mange der blev ændret. De to er hver sin handling, så du
  kan ændre forberedelsestiden uden at røre afslutningstiden. At anvende igen flytter kun minutterne
  yderst på et tog: det afgår, holder og ankommer stadig præcis på de tider, det gjorde.

- **Operatørerne er lettere at læse på forsiden af et tjenestehæfte.** Linjen sættes nu i dobbelt
  størrelse i forhold til før, så et logo er stort nok til at genkendes med et blik og en signatur stor
  nok til at læses tværs over et bord. Har alle operatører i tjenesten et logo, udelades ordet
  *Operatør* — logoerne siger det selv. Mangler en af dem et logo, står alle stadig med signatur, med
  fed skrift og med etiketten bevaret.

### Fejlrettelser

- **Et tjenestehæfte kunne udskrive en togdel ud over sidens nederste kant.** Rapporten beregner før
  udskriften, hvor mange togdele der er plads til på en side, og regnede med omkring halvdelen mere
  plads, end en A5-side faktisk har. Det, der går ud over sidekanten, skæres væk uden varsel: den anden
  togdel på en sådan side manglede slutningen af sin køreplan — eller manglede helt, så en lokofører
  stod med en tjeneste, hvor det sidste tog manglede. Togdele måles nu mod det, siden faktisk rummer,
  og en togdel, der ikke er plads til, flyttes til næste side. Nogle hæfter får derfor et ark mere end
  før.

- **Topologi-diagrammet kunne skrive signaturerne for to driftssteder oven på hinanden.** Driftsstederne
  blev placeret alene efter afstanden mellem dem, så to, der ligger tæt på hinanden på en lang
  strækning, blev tegnet næsten samme sted, og deres signaturer løb ind i hinanden. De tegnes nu aldrig
  tættere på hinanden, end deres to signaturer har brug for, mens resten af strækningen beholder sine
  virkelige proportioner. En lang signatur ved diagrammets kant bliver heller ikke længere skåret væk.

- **En gren i Topologi-diagrammet kunne tegnes tværs gennem en anden strækning.** En gren falder væk fra
  den strækning, den forlader, i en fast vinkel, så en gren, der mødte en strækning i vejen, aldrig kunne
  komme forbi den, uanset hvor langt ned i diagrammet den blev skubbet — den blev simpelthen tegnet tværs
  over den. De grene, der forlader en strækning længst ude, tegnes nu først, hvilket giver dem bagved en
  fri vej nedad. En lang gren kan derfor nu blive tegnet under en kort gren, der forlader strækningen
  længere ude.

- **En plan kunne vise sine tog under togkategorier, som fanen Togkategorier ikke havde.** Et tog bærer
  sin kategori med sig, så en plan gemt af en tidligere version blev åbnet med togene grupperet efter
  kategori, mens listen over kategorier var tom: kategorimenuen havde ingenting at tilbyde, og intet tog
  kunne flyttes til en anden kategori. Flere kategorier kunne også tages for en og samme, så deres tog
  blev samlet under en enkelt overskrift, og to tog af forskellige kategorier med samme nummer blev
  meldt som ét nummer brugt to gange. Når en plan åbnes, fyldes listen over kategorier nu op med de
  kategorier, togene bruger, og hver kategori holdes adskilt fra de andre.

- **To selskaber, der aldrig havde fået deres eget nummer, blev taget for den samme operatør.** Et
  selskab kendes fra de andre på et nummer, appen fører for det, og en plan kunne indeholde flere, der
  aldrig havde fået et. Tog fra forskellige selskaber, der delte tognummer, blev så meldt som ét nummer
  brugt to gange. Hvert selskab får nu sit eget nummer, når en plan åbnes eller gemmes; et selskab fra
  Module Registry beholder det nummer, det kom med.

- **En plan gemte sine togkategorier, selskaber og lande flere steder.** Hver enkelt blev skrevet der,
  hvor den først blev mødt ved gemningen — som regel inde i det første tog, der brugte den — mens
  listen, den hører hjemme i, ikke indeholdt mere end en henvisning til den. Sådan kunne en plan få tog
  i kategorier, som fanen Togkategorier ikke kendte. Hver enkelt skrives nu én gang, i sin egen liste,
  og alt, der bruger den, beholder kun en henvisning. Lande kopieres slet ikke længere ind i planen, så
  en rettelse af et lands sprog nu også når planer, der er gemt forinden. En plan gemt af en tidligere
  version læses som før og bliver rettet, næste gang den gemmes.

- **Et tjenestehæfte angav kun tognummeret i overskriften for en togdel.** Et tog identificeres lige
  så meget af kategoriens præfiks og suffiks som af nummeret — Gt 1234, ikke 1234 — og en lokofører,
  der sammenligner hæftet med køreplanen eller med det, der råbes op, har kun den overskrift at gå
  efter. Overskriften viser nu hele togidentiteten, præfiks og suffiks med, efter operatørens
  signatur.

## Version 0.3.3

- **Konflikter kan nu læses dér, hvor de vises.** En række med konflikter — et tog eller en togkategori
  under **Tog**, et omløb eller et af dets køretøjer under **Omløb**, en tjeneste under **Tjenester** —
  har nu et advarselssymbol, og et klik på det åbner meddelelserne som en læsbar liste. Symbolet får
  farve efter den alvorligste konflikt og tæller dem, når der er mere end én. Hidtil stod
  meddelelserne kun i et lille felt, der kom frem, mens markøren hvilede på rækken — nemt at overse og
  svært at læse.
- **En togkategori viser konflikterne for togene i den**, så de ikke længere skjules, når kategorien
  lukkes.
- **Fanen Tog åbner nu på listen over togkategorier**, hvor togene i hver kategori er skjult, indtil du
  åbner den, så en plan med mange tog er lettere at overskue. *Udvid alle* åbner dem alle på én gang,
  og en kategori åbner af sig selv, når du føjer et tog til den eller flytter et tog derind.
- **Når en togdel i et omløb redigeres, står der nu, hvilke slags køretøjer omløbet gælder** —
  lokomotiv, togsæt eller vognsæt. Deler flere køretøjer det samme omløb, nævnes hver slags én gang, og
  peger du på den, nævnes køretøjerne selv.

### Fejlrettelser

- **Appen kunne holde op med at gemme dit arbejde uden at sige det.** Planen gemmes i browseren, mens
  du arbejder, og en plan, appen ikke kunne skrive ud — et tog med færre end to standsninger eller en
  strækning under **Strækninger › Køreplansstrækninger**, hvor alle banestykker var fjernet — fik den
  lagring til at mislykkes lydløst. Alt derefter blev stående på skærmen, men blev aldrig gemt, så når
  browseren blev åbnet igen, lå planen som før: med driftsstederne, men uden de strækninger og tog, der
  var kommet til siden. Begge planer kan nu gemmes, og mislykkes en lagring alligevel, siger den øverste
  linje det med det samme, så du kan fortryde ændringen i stedet for at miste arbejdet.

- **En gemt planfil er omkring 40 % mindre.** Hver standsning blev skrevet to gange — én gang i sit tog og
  én gang under det spor, den ligger på — og den anden kopi trak store dele af resten af planen med sig.
  En plan gemt med en tidligere version kan stadig åbnes.

- **Et tog, der er efterladt uden trækkraft på en del af sit løb, rapporteres nu.** Kontrollen spurgte
  kun, om et lokomotiv eller togsæt kørte toget *et eller andet sted*, så når et omløb blev afkortet i
  den ene ende, stod resten af toget uden trækkraft, uden at der blev sagt noget. Nu kontrolleres hver
  strækning, toget kører, for hver køresession det køres, og konflikten siger, mellem hvilke
  driftssteder og i hvilke køresessioner toget mangler trækkraft. Planer, der så rene ud, kan nu
  rapportere dette — hullet har altid været der.

## Version 0.3.2

- Under **Godsstrøm › Godsbeskrivelser** kan en oprindelse eller en destination nu være et hvilket
  som helst driftssted, der udveksler gods, ikke kun en station. Et industriområde håndterer
  altid godsvogne, men kunne ikke vælges før, så gods til og fra en industri måtte beskrives, som
  om det gik til den nærmeste station.
- De samme lister siger nu **driftssted**, hvor de sagde *station*, da de ikke længere kun
  indeholder stationer.
- At ændre en tid for et ophold i fanen **Tog** **tager nu resten af toget med sig**. En **afgang** virker
  fremad, den vej toget kører: lad et tog stå fem minutter længere ved et driftssted, og det ankommer fem
  minutter senere til alle senere driftssteder. En **ankomst** virker baglæns: bed toget om at ankomme fem
  minutter senere, og det afgår fem minutter senere fra alle tidligere driftssteder, så løbet frem til
  ændringen følger med. Tiderne på den anden side bliver stående, køre- og opholdstiderne bevares, og
  ændringen afvises — og feltet falder tilbage — hvis den ville føre toget uden for planens driftstider.
- Et togs ophold vises altid i den **rækkefølge, toget kører** dem.
- Et tog, hvis togvej **springer et driftssted over** — to ophold i rækkefølge uden en strækning imellem —
  rapporteres nu som en konflikt. Den kan slås fra under **Indstillinger › Validering**.
- **Toghastigheden kontrolleres nu også på den sidste strækning**, ind til det driftssted, hvor toget
  slutter sit løb. Den strækning blev sprunget over før.

- En togdel i et **omløb** kan nu **redigeres**: pennen på en togdel åbner dens fra- og til-stop, så
  et omløb kan formes om, uden at alt efter det fjernes. En tilstødende togdel, der slutter til den,
  du ændrer, følger med — afkort en del fra A–C til A–B, og returløbet bliver B–A af sig selv. En
  nabodel, hvis eget tog ikke standser på det nye stop, står uændret, og hullet rapporteres som en
  konflikt, du selv løser.

- Alt, der læser et togs togvej, følger nu **den rækkefølge, toget kører sine stop i**, ikke den
  rækkefølge, de blev indtastet. For et tog, hvis stop er indtastet i forkert rækkefølge — et stop
  tilføjet efter et, toget først når senere — gik linjen i den **grafiske køreplan** i siksak mellem
  stop, som toget aldrig kører imellem, og toget kunne havne i den forkerte retnings kolonne; den
  udskrevne **køreplan** kunne vise en afgang, hvor toget ankommer; **byg automatisk** kædede slet
  ikke toget, da det så ud til at starte et andet sted; **gentag tog** målte intervallet fra det
  forkerte stop; og genberegning af tiderne efter en ændret standsningsplan mislykkedes helt. Valg af
  en del af et tog viser også stoppene i køreorden. Importerede planer har aldrig været berørt — der
  er de to rækkefølger ens.

- **Tilføj tog** kan nu oprette **returtoget** samtidig. Sæt kryds i *Retur?*, så oprettes toget tilbage
  fra destinationen sammen med det første, med samme strækning i modsat retning, samme togart og
  hastighed og det næste nummer i den modsatte retning. Afgangen er enten *så tidligt som muligt* — det
  første togs ankomst plus efterarbejds- og forberedelsestiden — eller et tidspunkt, du indtaster, som
  gerne må ligge både før og efter det første togs afgang. Sammen med *Gentag?* gentages begge
  retninger, så en hel trafik i begge retninger planlægges på én gang.

### Fejlrettelser

- **Kilometertallene** i den udskrevne køreplan og langs den grafiske køreplan afrundes nu til hele
  kilometer. De blev skrevet med en decimal, og afstandsfaktoren under **Indstillinger › Tid &
  hastighed** kunne gøre en stræknings længde til en skæv del af en kilometer. En sidebane viser nu
  også samme kilometertal som den bane, den udgår fra, ved forgreningsstationen.

## Version 0.3.1

- Afsnittet **Trækkraftenheder** på en togdelsside i hæftet Førertjenester har nu sin
  overskrift på det valgte sprog. Det var den eneste overskrift i hæftet uden oversættelse, så
  afsnittet kunne ikke genkendes som trækkraftenhederne.
- Trækkraftenheden udskrives nu for hver togdel, der har en. I planer importeret med en
  tidligere version viste nogle togdele en trækkraftenhed under **Tjenester** men ingen i hæftet.
- Noter om tog i samme retning fortæller nu, hvilket tog der kommer forbi det andet —
  **Overhaler GD 42757 12:02-12:05** eller **Overhales af GD 42757 12:02** — i stedet for det
  hidtidige *"Møder GD 42757 i samme retning"*, der aldrig sagde, hvilket tog der kom foran. To
  tog, der blot står på samme station samtidig, giver ingen note overhovedet, for ingen af dem er
  kommet forbi det andet.
- Et møde uden varighed — det andet tog kører igennem uden ophold — skrives som ét klokkeslæt i
  stedet for et interval fra et tidspunkt til sig selv.
- Et tog, der begynder eller afslutter sin kørsel på en station, medtages ikke længere som mødt,
  krydset eller overhalet der. De tidspunkter er, når dets lokofører møder ind eller går af, ikke
  når toget kører.

## Version 0.3.0

- En ny rapport, **Førertjenester**, udskriver ét A5-hæfte pr. tjeneste. Forsiden
  viser tjenestens nummer, hvilke sessioner eller dage den kører, dens start- og
  sluttidspunkt og -stationer, en sværhedsgrad, bemandingsbehov og eventuelle
  tjenestenoter. Hver togdel får sin egen side med hvilke trækkraftenheder der skal
  bruges, hvilke vognsæt der skal medbringes, og til hvilke destinationer der skal
  medbringes godsvogne, samt køreplanen – hver vist i sin egen tydeligt afgrænsede
  blok. Hæftets sidste side viser anlæggets sporplan og en tabel over
  rangerbanegårde, til nem opslag under kørslen.
- En ny rapport, **Generelle instruktioner**, er et separat trykt hæfte med træffets
  program og instruktioner, der gælder for et anlæg i hele træffets varighed. Her er
  træfarrangøren fri til at skrive hvad som helst – for eksempel køreinstruktioner,
  signalgivning, radio-/telefonbrug, hvad man gør ved forsinkelser og hvem man
  spørger – og det uddeles én gang til alle.
- Både programmet og instruktionerne skrives under **Indstillinger › Information** og
  kan formateres med Markdown – overskrifter, lister, fed og kursiv – så selv en lang
  instruktionstekst er læsbar på tryk.
- Hæftet indledes med træffets navn, hvilke datoer det gælder, og udskriftsdatoen,
  efterfulgt af programmet: sessionernes tider, pauser og måltider – det, enhver
  deltager har brug for at vide før den første session.
- Instruktionerne følger derefter over så mange sider, som de har brug for. Der brydes
  side mellem afsnit, og en overskrift holdes altid sammen med den tekst, den indleder.
- Sidste side viser anlæggets sporplan og tabellen over rangerbanegårde, så også de,
  der aldrig har et tjenestehæfte i hånden – først og fremmest stationspersonalet – får
  et overblik over anlægget.
- Hæftet udskrives i samme A5-format som tjenestehæfterne: A4 liggende, dobbeltsidet,
  foldet på midten, med tomme sider tilføjet hvor det er nødvendigt, så arkene foldes
  korrekt.
- Tjenester kan nu graderes **Let**, **Middel** eller **Erfaren**, vist farvekodet
  på hæftet, så en deltager kan vælge en tjeneste, der matcher deres erfaring.
- En tjeneste kan nu angive, at den kræver to eller tre personer – for eksempel en
  lokofører og en konduktør – og dette vises på hæftet.
- En tjeneste kan fastgøres til et **fast nummer**, så automatisk omnummerering
  lader den urørt, for eksempel særlige tjenester, der uddeles, inden en session
  begynder.
- Planen kontrolleres nu også, så hver togdel med et lokomotiv eller togsæt
  tildelt har en førertjeneste, der dækker den i hver session, den kører – en del,
  som ingen er planlagt til at køre, rapporteres session for session. En tjeneste
  med fast nummer kontrolleres også: den skal have et nummer, og ingen to
  tjenester med fast nummer må få samme nummer.
- Selskaber kan nu have et uploadet **logo**, vist på rapporter i stedet for
  tekstsignaturen.
- Stationer kan nu markeres som den **rangerbanegård**, der betjener en anden
  lokalitets lokalgods; anlægget lister automatisk hver rangerbanegård og hvad den
  dækker, vist på tjenestehæftets sidste side. Dette hjælper stationspersonale og
  godstogsførere med at vide, hvor vogne med en given godsdestination skal sendes
  hen.
- Hver køreplansstrækning kan nu tildeles en **farve**, som bruges til at tegne
  den i Topologi-diagrammet.
- En ny **afstandsfaktor** (under Indstillinger › Tid & hastighed) lader et anlæg
  vise et andet – typisk større, mere forbilledetro – kilometertal i rapporter og
  den grafiske køreplan end den afstand, der faktisk er modelleret, uden at det
  påvirker nogen køretidsberegning.
- Appen holder nu flere åbne browserfaner eller -vinduer synkroniseret med
  hinanden. **Bemærk**, at dette kun virker mellem vinduer på samme maskine i samme
  browser.
- Indstillinger kan nu gemme træffets **gælder fra**- og **gælder til**-datoer,
  trykt som en gyldighedslinje på rapporter; lad dem stå tomme, hvis intet træf er
  booket endnu.
- En ny indstilling, **udvid plantider automatisk?** (under Indstillinger ›
  Generelt), udvider planens start- eller sluttidspunkt for at dække et tog i
  stedet for at blokere ændringen, når togets egen tid falder uden for det. Slået
  fra som standard.
- En ny knap, **opdatér alle tider**, i den grafiske køreplan genberegner alle tog
  i køreplanen på én gang i stedet for først at skulle vælge en delmængde.
- Sporbelægningskontrollen kan nu valgfrit tage højde for, at et lokomotiv eller
  togsæt holder på et spor mellem to tog, medmindre det er booket til eller fra
  opstilling (under Indstillinger › Validering). Slået fra som standard, da det kun
  giver mening på anlæg, hvor opstilling er modelleret bevidst – slå den til der
  for at opdage et tredje tog, der i det skjulte bruger et spor, som et holdende
  køretøj allerede optager.
- Hvert ophold i fanen **Tog** har nu et felt til **Bemærkning** – en note, der udskrives
  ved det ophold, for eksempel “vent på modkørende tog”. Bemærkningen vises færdigformateret
  og skifter til den rå opmærkning, så snart du går ind i feltet, så du kan fremhæve det,
  der betyder noget: skriv `*langsomt*` for kursiv og `**første**` for fed. Tømmer du
  feltet, fjernes bemærkningen igen.

### Fejlrettelser

- Når man tilføjer et nyt tog, sættes dets standardstarttidspunkt nu under hensyn
  til den angivne forberedelsestid, så det ikke starter før planens
  starttidspunkt.

## Version 0.2.4

- En ny fane **Tjenester** lader dig planlægge førertjenester – det arbejde, en lokofører
  udfører i løbet af en session, som en række af de togdele, føreren kører. Hver tjeneste
  er en række: dens betegnelse, firma og sessioner til venstre, togdelene i køreorden til
  højre.
- Tilføj de togdele, en fører kører, med **Tilføj togdel**. Listen viser de
  trækkraftstrækninger, en fører kan tage som det næste – dem, der ikke støder sammen i
  tid med tjenesten, og, når den har en togdel, dem, der afgår ved eller efter, at den
  ankommer. Togdelene behøver ikke starte på samme station: mellem to togdele går føreren
  ganske enkelt hen, hvor den næste starter.
- Den samme togdel kan køres af flere tjenester, så længe de kører i forskellige
  sessioner, så én tjeneste kan dække de ulige sessioner og en anden de lige.
- Hvor to togdele for samme tog i en tjeneste køres af forskellige trækkraftenheder,
  viser fanen nu en note ved stationen, hvor trækkraftenheden skiftes – du indtaster den
  ikke i hånden.
- Du kan give hver tjeneste en betegnelse og et firma, vælge de sessioner, den kører, og
  tilføje frie noter, der gælder hele tjenesten.
- Tjenester importeret fra XPLN deler nu de togdele, der er defineret i køretøjernes
  omløb, så hver togdel viser den trækkraftenhed, der kører den.
- Planen kontrolleres, så ingen togdel køres af to tjenester i samme session, og ingen
  tjeneste har togdele, der overlapper i tid; eventuelle konflikter vises og åbnes på
  fanen **Tjenester**. Du kan slå kontrollen til eller fra under **Indstillinger ›
  Validering**.

## Version 0.2.2

### Fejlrettelser

- To tog, der aldrig kører i samme køresession, rapporteres ikke længere som et møde
  på en enkeltsporet strækning. Et tog, der kører session 1, 3, 5, og et, der kører
  2, 4, 6, kan nu dele samme spor uden en falsk advarsel, fordi de aldrig er ude
  samtidig.
- Konfliktkontrollen på dobbeltsporede (og flersporede) strækninger er nu præcis: en
  strækning markeres kun, når der er flere tog på den samtidig, end den har spor, og
  kun tog, der kører i en fælles session, tælles med.

## Version 0.2.1

- Konfliktadvarsler vises nu, hvor du kan rette dem. Togkonflikter vises kun i den
  grafiske køreplan og på fanen **Tog**; køretøjs- og omløbskonflikter vises kun på
  fanen **Omløb**.
- På fanen **Omløb** fremhæver en køretøjskonflikt nu kun det pågældende køretøj, og
  en omløbskonflikt fremhæver kun det pågældende omløb, så det er tydeligt, hvad der
  kræver opmærksomhed.
- Kontrollen af, at et køretøj vender tilbage til sit udgangspunkt, omfatter nu også
  vognsæt og gods, ikke kun lokomotiver og togsæt, så et vognsæt eller gods, der
  bliver efterladt det forkerte sted ved køresessionens slutning, nu rapporteres.

## Version 0.2.0

- Navnet på den plan, du arbejder med, vises nu øverst i vinduet, så du altid kan
  se, hvilket dokument der er åbent.
- Den grafiske køreplan viser nu søjler for lokomotivførerbehovet, hvilket gør det
  lettere at se, hvor mange førere der er brug for gennem køresessionen.
- En ny **Topologi**-visning (under fanen **Strækninger**) viser et skematisk
  diagram over køreplanens strækninger og deres grene.

### Fejlrettelser

- Strækninger bevarer nu som standard den rækkefølge, du indtastede dem i, så listen
  er lettere at følge, når du kontrollerer dine input. Du kan stadig sortere efter
  enhver kolonne.
- Konflikter henviser ikke længere til tog, du ikke kan finde: når et tog slettes,
  fjernes dets stop sammen med det, så der ikke er forældreløse stop eller falske
  konflikter tilbage.

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
