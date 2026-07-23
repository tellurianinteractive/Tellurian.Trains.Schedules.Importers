# Versjonsnyheter

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
