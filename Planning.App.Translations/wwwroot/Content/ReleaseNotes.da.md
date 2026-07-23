# Versionsnyheder

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
