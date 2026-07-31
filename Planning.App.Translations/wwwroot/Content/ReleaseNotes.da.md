# Versionsnyheder

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
