På fliken **Tåg** bygger du tåg och deras tider.

Ett tåg går längs en sträcka och har uppehåll vid driftplatser på vägen. För varje tåg anger du
dess nummer och kategori samt dess **stationsuppehåll** – de platser det passerar eller stannar vid, med
ankomst- och avgångstider. Standardtiderna för stationer från **Inställningar › Tid & hastighet**
används som utgångspunkt och kan åsidosättas för varje uppehåll.

Tåg som ritas här visas som linjer i den **Grafiska tidtabellen**, färgade efter sin kategori, och
kontrolleras mot valideringsreglerna i **Inställningar › Validering** (hastigheter, spårbeläggning, tågnummer
och så vidare).

## Per tåg eller per driftplats

Fliken har två vyer av samma uppehåll:

- **Per tåg** listar tågen efter kategori. Öppna ett tåg för att redigera dess uppehåll, ett tåg i taget.
- **Per driftplats** listar varje tåg som har uppehåll vid en plats, i den ordning de kommer
  dit: om det börjar, slutar, stannar eller passerar, varifrån det kommer och vart det går vidare, dess tider och
  vilket spår det använder. Välj ett annat spår i listan för att flytta tåget dit. Det är vyn för att
  reda ut spåren på en trafikerad station, där vyn per tåg skulle innebära att öppna varje tåg som har uppehåll
  där. En konflikt på platsen, till exempel två tåg på ett spår samtidigt, markeras på raden.

## Vad gör ett tåg unikt i planen?

Tåg identifieras med sitt **tågnummer**.
Numren måste vara unika inom en plan.
Om du anger ett trafikföretag,
antingen på **tågkategorin** eller på enskilda tåg,
måste tågnumret vara unikt inom
det företaget. Du kan alltså ha **DB 123** och **DSB 123**,
och de är två olika tåg.

## Start- och sluttid för arbetet med ett tåg
Två tider har en särskild betydelse så att planeraren kan ange förberedelse- och avslutstid:
- **Första ankomst** är starttiden för förberedelsetiden.
  En förare börjar arbeta med ett tåg vid den tiden. Det är tiden för att hämta ett lok från uppställning
  eller kontrollera att alla vagnkort stämmer med vagnarna i tåget och så vidare.
- **Sista avgång** är sluttiden för avslutningen.
  En förare slutar arbeta med ett tåg vid den tiden. Det är tiden för att köra loket till uppställning och så vidare.

## Redigera en uppehållstid

En tid ändras aldrig för sig själv – resten av tåget följer med och behåller de gångtider och
uppehållstider det redan har. En **avgång** verkar framåt, så som tåget går: låt ett tåg stå fem
minuter längre så når det varje senare plats fem minuter senare. En **ankomst** verkar bakåt: be
tåget anlända fem minuter senare så lämnar det varje tidigare plats fem minuter senare. Tiderna
på andra sidan det uppehåll du redigerar står kvar, så det som ändras vid uppehållet är hur
länge tåget står där. En ändring som skulle föra tåget utanför planens trafiktider
nekas i sin helhet, och fältet går tillbaka till den tid som redan är sparad.

## Uppehåll och genomfarter

Rutorna **Ank** och **Avg** på ett uppehåll anger om tåget stannar där. Kryssa i ingen så passerar det
igenom. Två saker tar valet ur dina händer:

- **Där tåget inte kan stanna alls** är båda rutorna rensade och inaktiverade. Det gäller varje
  signalstyrd plats och varje plats som inte utbyter något tåget för med sig – ett persontåg
  kräver att **Resande** är ikryssat för platsen, ett godståg **Gods** (se **Driftplatser**).
  En magasinsbangård utbyter alltid båda. När utbytet slås på igen kommer varje uppehåll
  du planerat tidigare tillbaka, så ingenting går förlorat av att pröva.
- **Där något beror på uppehållet** är rutan ikryssad och inaktiverad. Ett tågavsnitt går från ett
  uppehåll där tåget avgår till ett uppehåll där det anländer, så båda ändarna måste vara uppehåll: tågets egna första
  och sista uppehåll samt ändarna på varje avsnitt som ett fordonsomlopp, en förartjänst eller ett godsflöde
  är planerat över. För att frigöra rutan flyttar du uppehållet eller avsnittet som slutar där.

## Anmärkningar

**Anmärkning** på ett uppehåll är din egen notering, som skrivs ut för lokföraren och stationspersonalen
tillsammans med de anmärkningar planen räknar fram själv. Använd *kursiv* och **fet stil** för betoning; kodningen
syns när du redigerar anmärkningen.

Rapporterna skriver ut ett uppehålls ankomst och avgång som separata rader, så en anmärkning måste ange vilken
av de två den hör till. Rutan bredvid anmärkningen är där du väljer: **Ank** för något som möts eller
görs vid inkörning, **Avg** för något som görs före eller vid avgång. Där tåget bara anländer eller
bara avgår är den halvan den enda som erbjuds. Där det passerar igenom kan ingen anmärkning skrivas
alls – kryssa först i **Ank** eller **Avg**, så finns det någonstans att visa en.

## Dialogrutan Lägg till tåg

Det ska vara enkelt att lägga till tåg.
Du lägger till tåg i dialogrutan **Lägg till tåg**. Det finns flera alternativ:
- Skapa ett enskilt tåg.
- Skapa ett enskilt tåg med ett returtåg i motsatt riktning.
- Skapa ett antal upprepade tåg med ett givet intervall mellan dem.
- Kombination av returtåg och upprepade tåg.

Ett returtåg går samma väg tillbaka, med samma kategori och hastighet, och får nästa
lediga tågnummer i motsatt riktning. Dess avgång är antingen **så snart som möjligt** – det
första tågets ankomst, plus tiden för att avsluta det och förbereda returen – eller en tid du anger,
som kan ligga både före och efter det första tågets egen avgång. Kombinerat med upprepade tåg
skapas båda riktningarna först och upprepas sedan, så de två riktningarna numreras som par.

Det går inte att skapa tåg om någon del av något tåg hamnar utanför planens
start- och sluttid. Men i **Inställningar** kan du låta planens start- och sluttider
utvidgas när tåg läggs till.

## Trafikerade köromgångar eller dagar
Planeringen av tåg kan använda möjligheten att ange vilka köromgångar (eller veckodagar) ett tåg går.
Standard är alltid **Alla/Dagligen**. Du definierar de olika köromgångsmönstren i **Inställningar › Köromgångar & dagar**.
Det finns vanliga fördefinierade mönster, men du kan lägga till egna.
