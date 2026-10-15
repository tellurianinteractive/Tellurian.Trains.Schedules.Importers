Fliken **Sträckor** definierar hur driftplatser är förbundna och vilka förbindelser som bildar
varje tidtabell. Den har tre underavdelningar, som du arbetar igenom i ordning.

### Bansträckor

En **bansträcka** är linjen mellan två angränsande driftplatser, med avstånd, antal
spår, hastighet och gångtid. Bansträckor beskriver det fysiska nätet och ger de
avstånd som används för att kontrollera tåghastigheter och för att rita den grafiska tidtabellen.

En bansträcka har en **riktning** (från en plats till nästa). Alla bansträckor måste
definieras i samma riktning så att ett tåg kan gå längs dem utan oavsiktliga riktningsbyten. Med till exempel
`A→B B→C D→C C→E E→F E→G` kan ett tåg gå `A→G`, `D→G`, `A→F` och `D→F` utan att
byta riktning; `A→D` eller `F→G` kräver ett riktningsbyte där linjerna möts. Om du definierar en
sträcka åt fel håll (så att två sträckor pekar mot varandra eller bildar en slinga) listar en varning
de sträckor som inte stämmer – åtgärda det genom att redigera en av dem så att alla går åt samma håll.

En bansträcka som ingår i en tidtabellssträcka kan inte tas bort förrän den har tagits bort från
den tidtabellssträckan.

### Trafikledningssträckor

En **trafikledningssträcka** går mellan två platser där tåg klareras – en **bemannad** station, en
**magasinsstation** eller en plats som **styrs från** en bemannad station – och går rakt igenom alla
obemannade platser däremellan. Alltså bildar `A→B` och `B→C` med obemannad `B` en trafikledningssträcka `A→C`.

En växel, mötesplats, obemannad station eller ett industriområde som **styrs från** en bemannad station
sköts av den stationens tågklarerare som om det vore den egna stationen. Den avslutar de trafikledningssträckor
som möts där, dess tåg står på den styrande stationens tågklareringslista, märkta med dess signatur, och
stationerna bortom den hör till dem som den styrande stationen ringer. En **blockpost** – en signalstyrd
plats som varken är en växel eller en plats där tåg kan mötas – är fortsatt en del av linjen.

Trafikledningssträckor skapas automatiskt utifrån bansträckorna: tryck på **Skapa om från
bansträckor** när du ändrar nätet eller ändrar vilka stationer som är bemannade. Markera en station
som bemannad eller som magasinsstation på fliken **Driftplatser**. Kolumnen *Via* visar de
obemannade platser som en trafikledningssträcka passerar.

#### Vilka tåg som får vara på en trafikledningssträcka samtidigt

Tågklarerarna i de två ändarna kommer överens om vilka tåg som får vara på trafikledningssträckan mellan dem, så
listan **Konflikter** bedömer varje trafikledningssträcka som en helhet:

- Varje **signalstyrd plats** på den, till exempel en blockpost, delar den i **avsnitt**. Ett
  avsnitt rymmer ett tåg per spår åt gången, oavsett i vilken riktning tågen går. Obemannade platser
  som ingen styr delar den inte.
- På **enkelspår** kan tåg i motsatta riktningar bara mötas i ändarna eller på en
  signalstyrd plats markerad **Tåg kan mötas?**. De kan aldrig befinna sig mellan två sådana platser samtidigt,
  men tåg i samma riktning kan följa efter varandra där, ett per avsnitt.
- Ett tåg som passerar en signalstyrd plats utan tid där upptar varje avsnitt det passerar
  tills det anländer, eftersom inget säger när det passerade.

Bansträckor som inte ingår i någon trafikledningssträcka bedöms i stället en och en.

### Tidtabellssträckor

En **tidtabellssträcka** är en ordnad följd av sammanhängande bansträckor som du vill planera och
rita tillsammans – vanligtvis en linje från en ändstation till en annan. Tidtabellssträckor är det du
väljer bland på fliken **Grafisk tidtabell**, och var och en blir en graf.

För att bygga en ger du den ett nummer (och en valfri beskrivning) och lägger sedan till bansträckor i dess sträckning
en i taget. Bara bansträckor som fortsätter där sträckningen för tillfället slutar erbjuds, så
sträckningen förblir sammanhängande. Redigeraren visar de stationer och det totala avstånd som blir resultatet.

### Rekommenderad arbetsordning

Definiera bansträckorna först och gruppera dem sedan i de tidtabellssträckor du vill arbeta med.
