# Versionsnyheder

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
