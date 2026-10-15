Fliken **Tågkategorier** definierar vilka slags tåg du kör och hur de visas.

En kategori grupperar tåg som delar en klassificering – till exempel snabbtåg, lokaltåg, godståg eller
växling. Varje kategori har:

- ett **prefix** som används i tågnummer och etiketter,
- en **färg** som används för att rita tågets linje i den grafiska tidtabellen,
- en **klassificering** som påverkar validering och sortering,
- ett **företag** som kör alla tåg i kategorin, vilket kan åsidosättas för enskilda tåg,
- en **förberedelsetid** och en **avslutstid** i minuter, som används för varje nytt tåg i
  kategorin,
- ett **uppehållsmönster** som anger var dess tåg stannar på vägen.

Ställ in de kategorier du behöver innan du tilldelar dem till tåg på fliken **Tåg**, så att varje
tåg kan ärva sitt utseende och sina regler från sin kategori.

## Förberedelse- och avslutstider

Ett tåg görs klart några minuter före avgång och ställs undan några minuter efter
ankomst. Det är verkligt arbete: då är fordonen och lokföraren bundna, och det
räknas överallt där programmet letar efter krockar.

De två tider du anger här ges till varje tåg du skapar i kategorin därefter. Knappen
**Tillämpa på nytt** vid vardera ger den tiden till alla tåg som kategorin redan
har, och talar om hur många som ändrades. Att tillämpa förberedelsetiden på nytt flyttar varje tågs första
ankomst; att tillämpa avslutstiden på nytt flyttar varje tågs sista avgång. Ingen av dem ändrar var
eller när ett tåg går – varje avgång och ankomst på vägen förblir precis som den var. De två är
separata åtgärder, så du kan ändra den ena och låta den andra vara.

## Uppehållsmönster

Uppehållsmönstret är listan över driftplatser där tåg i kategorin stannar på vägen.
Kryssa i en plats så stannar tågen där; lämna den utan kryss så går de igenom. Bara de
platser där tåg i kategorin överhuvudtaget kan stanna erbjuds – en persontågskategori erbjuds
de platser där resande stiger på och av, en godstågskategori de platser där vagnar utbyts.

Mönstret är det ett nytt tågs väg byggs efter. Skapa ett tåg från **Tåg**, så får det ett uppehåll
vid varje ikryssad plats det passerar och går igenom resten. Där ett tåg börjar och där
det slutar är alltid uppehåll, vad som än är ikryssat: där görs det klart och ställs undan.

Om du inte kryssar i något har kategorin **inget mönster** och ingenting är begränsat: dess tåg stannar
var de kan lämna över det de för med sig, vilket är hur sträckningar byggdes innan det fanns
mönster. En kategori vars tåg går utan uppehåll ställs in genom att bara kryssa i de platser där de stannar.

Ett tåg som stannar någonstans som mönstret inte nämner rapporteras under **Konflikter**, med
tåg, plats och tid. Ingenting rättas åt dig, eftersom bara du kan avgöra vilket av
de två som är fel: tåget som stannar där det borde gå igenom, eller mönstret som saknar
en plats som dess tåg trafikerar.

### Fylla i mönstret

**Hämta från tågen** kryssar i de platser där de tåg kategorin redan har stannar,
och ersätter det som var ikryssat. Så ställs en kategori in när dess första tåg är planerade,
och det görs åt dig första gången en plan som gjordes innan det fanns uppehållsmönster öppnas: varje
kategori utan mönster får det som dess tåg hela tiden har gått efter, så att ingenting rapporteras
som inte var ett fel förut.

**Rensa** tar bort mönstret igen och lämnar kategorin obegränsad. Observera att när planen öppnas
därefter fylls det i från tågen på nytt, så en kategori med tåg kan inte lämnas
utan mönster länge.

En växlingskategori har inget uppehållsmönster. Dess uppgifter utförs på ett ställe och går ingenstans, så
det finns ingen sträckning som ett mönster kan forma.
