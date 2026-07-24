# Versjonsnyheter

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
