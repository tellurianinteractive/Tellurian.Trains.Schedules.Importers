Fanen **Tog** er stedet, hvor du opbygger tog og deres tider.

Et tog kører langs en strækning og har ophold ved driftssteder undervejs. For hvert tog
angiver du dets nummer og kategori samt dets **stationsophold** – de steder, det passerer eller standser ved, med
ankomst- og afgangstider. Standardstationstiderne fra **Indstillinger › Tid & hastighed**
bruges som udgangspunkt og kan tilsidesættes for hvert ophold.

Tog, der tegnes her, vises som linjer i den **Grafiske køreplan**, farvet efter deres kategori, og
kontrolleres mod valideringsreglerne i **Indstillinger › Validering** (hastigheder, sporbrug, tognumre
og så videre).

## Pr. tog eller pr. driftssted

Fanen har to visninger af de samme ophold:

- **Pr. tog** viser togene efter kategori. Åbn et tog for at redigere dets ophold, ét tog ad gangen.
- **Pr. driftssted** viser hvert tog, der har ophold ved ét sted, i den rækkefølge, de kommer
  dertil: om det begynder, slutter, standser eller passerer, hvorfra det kommer og hvor det kører videre til, dets tider og
  det spor, det bruger. Vælg et andet spor på listen for at flytte toget dertil. Det er visningen til at
  få styr på sporene på en travl station, hvor visningen pr. tog ville kræve, at man åbnede hvert tog, der har ophold
  dér. En konflikt på stedet, for eksempel to tog på ét spor på samme tid, markeres på rækken.

## Hvad gør et tog unikt i planen?

Tog identificeres ved deres **tognummer**.
Disse numre skal være unikke inden for en plan.
Hvis du angiver et kørende selskab,
enten på **togkategorien** eller på bestemte tog,
skal et tognummer være unikt inden for
det selskab. Så du kan have **DB 123** og **DSB 123**,
og de er to forskellige tog.

## Start- og sluttidspunkt for arbejdet med et tog
To tidspunkter har en særlig betydning, så planlæggeren kan angive forberedelses- og afslutningstid:
- **Første ankomst** er starttidspunktet for forberedelsestiden.
  En fører begynder arbejdet med et tog på dette tidspunkt. Det er tiden til at hente et lokomotiv fra henstilling
  eller kontrollere, at alle vognkort stemmer med vognene i toget og så videre.
- **Sidste afgang** er sluttidspunktet for afslutningen.
  En fører afslutter arbejdet med et tog på dette tidspunkt. Det er tiden til at køre lokomotivet til henstilling og så videre.

## Redigering af en opholdstid

En tid ændres aldrig for sig selv – resten af toget følger med og beholder de køre- og
opholdstider, det allerede har. En **afgang** virker fremad, den vej toget kører: lad et tog holde fem
minutter længere, og det når hvert senere sted fem minutter senere. En **ankomst** virker baglæns: bed
toget om at ankomme fem minutter senere, og det forlader hvert tidligere sted fem minutter senere. Tiderne
på den anden side af det ophold, du redigerer, bliver, hvor de er, så det, der ændres ved opholdet, er, hvor
længe toget holder dér. En ændring, der ville føre toget uden for planens driftstider,
afvises som helhed, og feltet vender tilbage til den tid, der allerede er gemt.

## Ophold og gennemkørsler

Felterne **An** og **Afg** på et ophold angiver, om toget standser dér. Afkryds ingen, og det kører
igennem. To ting fratager dig valget:

- **Hvor toget slet ikke kan standse**, er begge felter ryddet og deaktiveret. Det gælder ethvert
  signalstyret sted og ethvert sted, der ikke udveksler noget, toget fører – et persontog
  kræver, at **Passagerer** er afkrydset for stedet, et godstog **Gods** (se **Driftssteder**).
  En skyggebanegård udveksler altid begge. Når udvekslingen slås til igen, kommer ethvert ophold,
  du tidligere har planlagt, tilbage, så intet går tabt ved at prøve.
- **Hvor noget afhænger af opholdet**, er feltet afkrydset og deaktiveret. Et togafsnit kører fra et
  ophold, hvor toget afgår, til et ophold, hvor det ankommer, så begge ender skal være ophold: togets eget første
  og sidste ophold og enderne af hvert afsnit, som et køretøjsomløb, en førertjeneste eller en godsstrøm
  er planlagt over. For at frigøre feltet flytter du opholdet eller afsnittet, der ender dér.

## Bemærkninger

**Bemærkning** på et ophold er en note af din egen, der udskrives til lokoføreren og stationspersonalet
sammen med de noter, planen selv udregner. Brug *kursiv* og **fed** til fremhævelse; kodningen
vises, når du redigerer noten.

Rapporterne udskriver et opholds ankomst og afgang som separate linjer, så en bemærkning skal angive, hvilken
af de to den hører til. Feltet ved siden af noten er der, hvor du vælger: **An** for noget, der mødes eller
gøres ved indkørsel, **Afg** for noget, der gøres før eller ved afgang. Hvor toget kun ankommer eller
kun afgår, er den halvdel den eneste, der tilbydes. Hvor det kører igennem, kan der slet ikke skrives en bemærkning
– afkryds først **An** eller **Afg**, så er der et sted at vise en.

## Dialogen Tilføj tog

Det skal være let at tilføje tog.
Du tilføjer tog i dialogen **Tilføj tog**. Der er en række muligheder:
- Opret et enkelt tog.
- Opret et enkelt tog med et returtog i modsat retning.
- Opret et antal gentagne tog med et givet interval imellem dem.
- Kombination af returtog og gentagne tog.

Et returtog kører den samme rute tilbage igen, med samme kategori og hastighed, og får det næste
ledige tognummer i modsat retning. Dets afgang er enten **så hurtigt som muligt** – det
første togs ankomst plus tiden til at afslutte det og forberede returen – eller et tidspunkt, du angiver,
som kan ligge både før og efter det første togs egen afgang. Kombineret med gentagne tog
oprettes begge retninger først og gentages derefter, så de to retninger nummereres som par.

Oprettelse af tog mislykkes, hvis en del af et tog falder uden for planens
start- og sluttidspunkt. Men i **Indstillinger** kan du lade planens start- og sluttidspunkter
udvide sig, når tog tilføjes.

## Kørende køresessioner eller dage
Planlægningen af tog kan benytte muligheden for at angive, hvilke køresessioner (eller ugedage) et tog kører.
Standard er altid **Alle/Dagligt**. Du definerer de forskellige køresessionsmønstre i **Indstillinger › Køresessioner & dage**.
Der er almindelige foruddefinerede mønstre, men du kan tilføje dine egne.
