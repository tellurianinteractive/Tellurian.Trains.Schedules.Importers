Fliken **Regioner** definierar destinationerna *utanför* din bana – de inhemska regioner och
främmande länder som vagnar kan styras till. Regioner används för att styra godsflöden: de gör att ett
tågs destinationsanmärkning kan säga vart vagnarna är på väg bortom den modellerade järnvägen.

### Vad en region är

Varje region har:

- ett **namn**, skrivet på banans standardspråk och visat överallt där regionen nämns,
- ett **land** som den hör till – en ny region får banans standardland, men en region
  som står för ett utländskt mål kan i stället sättas till det landet,
- en **bakgrundsfärg**, som används för att visa regionen som en färgad bricka i anmärkningar (textfärgen
  kontrasteras automatiskt för läsbarhet).

### Hur regioner används

En region är i sig bara en etikett. Den får betydelse först när den kopplas till en
**station** – normalt en **magasinsbangård**, en slutstation som representerar yttre stationer och
världen bortom den modellerade järnvägen. En station kan kopplas till ingen, en eller flera regioner;
vanliga stationer behöver sällan någon.

När en godsdestination är inställd på att *inkludera regioner* listar destinationsanmärkningen för den stationen
dess regioner som färgade brickor – vilket talar om för operatören att vagnar som kommer dit fortsätter vidare till de
regionerna eller länderna.

Definiera de regioner du behöver här och koppla dem sedan till dina magasinsstationer på fliken
**Driftplatser**.

### Hantera regioner

Listan visar varje region som en färgad bricka bredvid sitt namn och land.

- **Lägg till ny** skapar en region. Ange ett namn, välj land och välj en färg ur
  paletten.
- **Redigera** ändrar en regions namn, land eller färg.
- **Ta bort** tar bort en region. Det går inte så länge någon station har regionen tilldelad – ta bort
  kopplingen på fliken **Driftplatser** först.
- **Standardregioner** lägger till standarduppsättningen av regioner, namngivna på banans standardspråk och
  praktiska när du börjar med en ny bana.

Regioner sparas tillsammans med banan, och den här katalogen är källan till de regionval som visas
när en station redigeras.
