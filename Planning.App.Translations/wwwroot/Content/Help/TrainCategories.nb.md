Fanen **Togkategorier** definerer hvilke typer tog du kjører, og hvordan de vises.

En kategori grupperer tog som deler en klassifisering – for eksempel ekspresstog, lokaltog, godstog eller
skifting. Hver kategori har:

- et **prefiks** som brukes i tognumre og etiketter,
- en **farge** som brukes til å tegne togets linje i den grafiske ruteplanen,
- en **klassifisering** som påvirker validering og sortering,
- et **selskap** som kjører alle tog i kategorien, og som kan overstyres for enkelttog,
- en **forberedelsestid** og en **avslutningstid** i minutter, som brukes for hvert nytt tog i
  kategorien,
- et **stoppmønster** som angir hvor togene stopper underveis.

Sett opp kategoriene du trenger før du tildeler dem til tog på fanen **Tog**, slik at hvert
tog kan arve utseendet og reglene sine fra kategorien.

## Forberedelses- og avslutningstider

Et tog gjøres klart noen minutter før avgang og settes bort noen minutter etter
ankomst. Det er ekte arbeid: da er kjøretøyene og lokføreren bundet, og det
telles overalt der programmet leter etter sammenstøt.

De to tidene du angir her, gis til hvert tog du oppretter i kategorien etterpå. Knappen
**Bruk på nytt** ved hver av dem gir den tiden til alle togene kategorien allerede
har, og forteller hvor mange som ble endret. Å bruke forberedelsestiden på nytt flytter hvert togs første
ankomst; å bruke avslutningstiden på nytt flytter hvert togs siste avgang. Ingen av dem endrer hvor
eller når et tog kjører – hver avgang og ankomst underveis forblir akkurat som den var. De to er
separate handlinger, så du kan endre den ene og la den andre være.

## Stoppmønster

Stoppmønsteret er listen over driftssteder der togene i kategorien stopper underveis.
Kryss av et sted, og togene stopper der; la det stå uten kryss, og de kjører gjennom. Bare stedene
der tog i kategorien i det hele tatt kan stoppe, tilbys – en persontogkategori tilbys
stedene der passasjerer går på og av, en godstogkategori stedene der vogner utveksles.

Mønsteret er det et nytt togs trasé bygges etter. Opprett et tog fra **Tog**, og det får et opphold
ved hvert avkrysset sted det passerer og kjører gjennom resten. Der et tog begynner og der
det slutter, er alltid opphold, uansett hva som er avkrysset: der gjøres det klart og settes bort.

Krysser du ikke av noe, har kategorien **ikke noe mønster**, og ingenting er begrenset: togene stopper
overalt der de kan levere fra seg det de fører, som var slik traseer ble bygget før det fantes
mønstre. En kategori hvis tog kjører uten stopp, settes opp ved å krysse av bare stedene der de stopper.

Et tog som stopper et sted mønsteret ikke nevner, rapporteres under **Konflikter**, med
tog, sted og tidspunkt. Ingenting rettes for deg, fordi bare du kan si hvilken av
de to som er feil: toget som stopper der det burde kjøre gjennom, eller mønsteret som mangler
et sted togene betjener.

### Fylle ut mønsteret

**Hent fra togene** krysser av stedene der togene kategorien allerede har, stopper,
og erstatter det som var avkrysset. Slik settes en kategori opp når de første togene er planlagt,
og det gjøres for deg første gang en plan som ble laget før det fantes stoppmønstre, åpnes: hver
kategori uten mønster får det togene har gått etter hele tiden, slik at ingenting rapporteres
som ikke var en feil før.

**Tøm** fjerner mønsteret igjen og lar kategorien være ubegrenset. Merk at når planen åpnes
etterpå, fylles det ut fra togene på nytt, så en kategori med tog kan ikke stå
uten mønster lenge.

En skiftekategori har ikke noe stoppmønster. Oppgavene utføres ett sted og går ingen steder, så
det finnes ingen trasé et mønster kan forme.
