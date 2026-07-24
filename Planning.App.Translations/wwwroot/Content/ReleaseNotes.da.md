# Versionsnyheder

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
