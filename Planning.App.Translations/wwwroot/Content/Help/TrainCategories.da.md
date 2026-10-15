Fanen **Togkategorier** definerer, hvilke slags tog du kører, og hvordan de vises.

En kategori grupperer tog, der deler en klassifikation – for eksempel lyntog, lokaltog, godstog eller
rangering. Hver kategori har:

- et **præfiks**, der bruges i tognumre og etiketter,
- en **farve**, der bruges til at tegne togets linje i den grafiske køreplan,
- en **klassifikation**, der påvirker validering og sortering,
- et **selskab**, der kører alle tog i kategorien, og som kan tilsidesættes for enkelte tog,
- en **forberedelsestid** og en **afslutningstid** i minutter, der bruges for hvert nyt tog i
  kategorien,
- et **standsningsmønster**, der angiver, hvor dens tog standser undervejs.

Opsæt de kategorier, du har brug for, før du tildeler dem til tog på fanen **Tog**, så hvert
tog kan arve sit udseende og sine regler fra sin kategori.

## Forberedelses- og afslutningstider

Et tog gøres klar nogle minutter før afgang og stilles væk nogle minutter efter
ankomst. Det er rigtigt arbejde: det er dér, køretøjerne og lokoføreren er bundet, og det
tælles overalt, hvor programmet leder efter sammenstød.

De to tider, du angiver her, gives til hvert tog, du derefter opretter i kategorien. Knappen
**Anvend igen** ved hver af dem giver den tid til alle de tog, kategorien allerede
har, og fortæller dig, hvor mange der blev ændret. Genanvendelse af forberedelsestiden flytter hvert togs første
ankomst; genanvendelse af afslutningstiden flytter hvert togs sidste afgang. Ingen af dem ændrer, hvor
eller hvornår et tog kører – hver afgang og ankomst undervejs forbliver præcis, som den var. De to er
separate handlinger, så du kan ændre den ene og lade den anden være.

## Standsningsmønster

Standsningsmønstret er listen over driftssteder, hvor tog i kategorien standser undervejs.
Afkryds et sted, og togene standser dér; lad det stå uafkrydset, og de kører igennem. Kun de
steder, hvor tog i kategorien overhovedet kan standse, tilbydes – en persontogskategori tilbydes
de steder, hvor passagerer stiger på og af, en godstogskategori de steder, hvor vogne udveksles.

Mønstret er det, et nyt togs rute bygges efter. Opret et tog fra **Tog**, og det får et ophold
ved hvert afkrydset sted, det passerer, og kører igennem resten. Hvor et tog begynder, og hvor
det ender, er altid ophold, uanset hvad der er afkrydset: dér gøres det klar og stilles væk.

Afkrydses intet, har kategorien **intet mønster**, og intet er begrænset: dens tog standser
overalt, hvor de kan aflevere det, de fører, hvilket er sådan, ruter blev bygget, før der fandtes
mønstre. En kategori, hvis tog kører uden ophold, opsættes ved kun at afkrydse de steder, hvor de standser.

Et tog, der standser et sted, mønstret ikke nævner, rapporteres under **Konflikter** med
tog, sted og tidspunkt. Intet rettes for dig, fordi kun du kan sige, hvilket af
de to der er forkert: toget, der standser, hvor det burde køre igennem, eller mønstret, der mangler
et sted, dets tog betjener.

### Udfyldning af mønstret

**Hent fra togene** afkryder de steder, hvor de tog, kategorien allerede har, standser,
og erstatter det, der var afkrydset. Sådan opsættes en kategori, når dens første tog er planlagt,
og det gøres for dig, første gang en plan, der blev lavet før standsningsmønstre fandtes, åbnes: hver
kategori uden mønster får det, dens tog hele tiden har kørt efter, så intet rapporteres,
som ikke var en fejl før.

**Ryd** fjerner mønstret igen og lader kategorien være ubegrænset. Bemærk, at når planen åbnes
bagefter, udfyldes det fra togene igen, så en kategori med tog ikke kan lades
uden mønster ret længe.

En rangerkategori har intet standsningsmønster. Dens opgaver udføres ét sted og går ingen steder, så
der er ingen rute, et mønster kan forme.
