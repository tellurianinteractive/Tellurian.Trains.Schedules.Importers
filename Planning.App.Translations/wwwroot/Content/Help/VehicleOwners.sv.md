Fliken **Fordonsägare** registrerar vem som tar med vilket rullande materiel till träffen.

I modulär drift tas lok, tågsätt och vagnar med av olika deltagare. Genom att registrera
ägarna till varje fordon blir det tydligt vem som levererar vad, och varje ägare får de uppgifter hen behöver för att
ställa upp sitt materiel på banan före den första köromgången.

### Rullande materiel

Varje lok, tågsätt, vagnsätt och växellok i planen listas, med:

- dess **typ**, **fordon**sbeteckning och **klass** – för ett vagnsätt som listar sina vagnar visas varje vagns
  klass en gång, som i *A/B/Fv*,
- **antalet** enheter – eller listade vagnar – som bara visas när det är fler än en,
- den **första köromgång** (eller **första dag**) det är i trafik,
- den **station**, det **spår** och den **avgångs**tid där det ska stå före den köromgången – början
  på det första tåg det kör den köromgången. Ett fordon som ännu inte har fått något omlopp visar **Inte i trafik**,
  ett växellok visar bara den plats det är stationerat vid,
- dess **ägare**, ägarens **DCC-adress**, de ägare som tar med **reserver** och en **anmärkning** om fordonet.

Kryssa i **Bara de utan ägare?** för att se vad ingen ännu har erbjudit sig att ta med.

Ett fordon som är **Inte i trafik** kan tas bort ur planen med sin knapp **Ta bort**, och
**Ta bort de som inte är i trafik** tar bort alla sådana som listas – bara de som visas när filtret
är ikryssat. Deras ägare tas bort med dem; deltagarna finns kvar. Ett fordon som har fått arbete måste först tas ur
sina omlopp på fliken **Omlopp**. Ett växellok tas bort på fliken **Driftplatser**, där det är stationerat.

### Ett fordons ägare

Öppna en rad för att se och redigera dess ägare. Den första ägaren är den **ordinarie**, som tar med enheten och
ställer upp den på banan. Varje ytterligare ägare tar med en **reserv**. **Gör ordinarie** flyttar en reservägare till
toppen; om den ordinarie ägaren tas bort blir den första reservägaren ordinarie.

Skriv de första bokstäverna i ett namn och välj deltagaren i listan, eller tryck **Enter** för att ta den
markerade. Början av vilket ord som helst i namnet matchar, så ett efternamn fungerar också. Ett namn som inte matchar
någon erbjuds som en **Ny deltagare** – kontrollera stavningen innan du lägger till den.

### DCC-adresser

Ett lok, tågsätt eller växellok behöver en **DCC-adress** för varje ägare, reserver inräknade, eftersom var och en av dem
körs på banan. Ange **0** när ägaren ännu inte har sagt vilken adress det är; den visas som
**Ägaren lämnar**. En ägare kan inte läggas till ett dragfordon utan adress, och en adress kan
ändras men inte tas bort. En varningssymbol markerar ett dragfordon där en ägare fortfarande saknar en – vilket
händer när ett vagnsätt ändras till ett lok efter att dess ägare angetts.

### Deltagare

Vyn **Deltagare** listar alla som anges som ägare och vad var och en tar med; det fordon de
tar med själva visas i fetstil. Ett namn sparas en gång, så om du rättar ett felstavat namn här rättas det
överallt. En deltagare som fortfarande tar med rullande materiel kan inte tas bort.

### Utskrift

**Rapporter ▸ Fordon och ägare** skriver ut den här listan på A4 liggande, ordnad på ett av tre sätt som väljs
ovanför sidorna:

- **Per driftplats** – en sida för varje station, att lämna till dess ägare: de fordon som ska ställas upp där,
  i ordning efter första köromgång och avgång, med sina ägare och DCC-adresser, och de växellok som är stationerade
  där sist. Varje reserv står på en egen rad, direkt under den enhet den står för. De fordon som inte är i trafik följer på en egen sida.
- **Per ägare** – en sida för varje deltagare: vad hen tar med, med de enheter hen ställer upp själv först, med
  DCC-adress och var varje fordon ska ställas upp. De fordon som ingen ännu tar med kommer först, på en sida
  med rubriken **Ännu inte bokade**.
- **Efter DCC-adress** – alla lok och tågsätt som tas med, i en lista, lägsta adress först; de saknade
  adresserna kommer efter de kända. Vagnsätt har ingen adress och utelämnas.

Varje station eller ägare börjar på en ny sida och fortsätter på nästa sida, under samma rubrik, när en
sida inte räcker. Där ägaren till ett lok eller tågsätt inte har angett någon DCC-adress, eller har angett
**0**, skrivs adressen ut som **Ange!** i rött.

Radens bakgrund visar när enheten behövs:

- **vit** – i trafik alla köromgångar (eller dagar), eller inte i trafik alls,
- **ljusgrå** – en reserv,
- **ljusblå**, **ljusgrön** eller **ljusröd** – först i trafik första, andra eller tredje köromgången,
  men inte alla.
