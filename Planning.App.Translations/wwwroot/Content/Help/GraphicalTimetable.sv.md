Fliken **Grafisk tidtabell** ritar en tåggraf (tid–vägdiagram) för de
tidtabellssträckor du väljer.

### Välja vad som ska visas

Kryssa i en eller flera **tidtabellssträckor** att rita. Varje vald sträcka ritas som en egen
graf. Sträckorna kommer från fliken **Sträckor**; om inga är definierade finns inget att rita.

#### Visa en halva

När en **brytpunkt** är angiven under **Inställningar › Allmänt** visas väljaren **Visa** med
**Hela grafen**, **Första halvan** och **Sista halvan**. Första halvan går från trafikens början
till brytpunkten; sista halvan från brytpunkten till trafikens slut. Väljer du en halva ritas bara den delen av
dygnet, vilket gör grafen läsbar på mindre skärmar – särskilt med lodrät tidsaxel.
Väljaren är dold när ingen brytpunkt är angiven.

### Kopiera tåg

**Kopiera tåg** kopierar de tåg du väljer. Ange hur många minuter kopian förskjuts –
positivt är senare, negativt tidigare – och varje valt tåg kopieras en gång.

Kryssa i **Motsatt riktning?** så går kopian samma väg baklänges i stället, från där tåget
slutade till där det började. Alla gångtider och alla uppehåll behålls, förberedelse- och avslutstiderna
följer med kopian till dess egna ändar, och den får ett nummer ur motsatt riktnings nummerserie. Minuterna
räknas sedan från det kopierade tågets sista avgång, så de anger hur länge efter att tåget
ställts undan som returtåget ger sig av.

Kryssa i **Upprepa?** under **Upprepa tåg** för att göra en serie i stället för en enda kopia: den första kopian
görs vid den förskjutning du angav, och en till varje intervall tills en kopia skulle avgå efter sluttiden.
Det är samma upprepning som när ett tåg läggs till, men på ett tåg som redan finns – så
det första tåget kan justeras tills det går som det ska innan resten kopieras från det.

En kopia som skulle gå utanför planens start- och sluttid skapas inte.

### Läsa grafen

Stationerna placeras längs den ena axeln och tiden längs den andra. Varje tåg är en lutande linje, färgad
efter sin kategori. Orientering, avstånd och etiketter styrs från
**Inställningar › Grafisk tidtabell**.

### Lokförare som behövs

När **Förväntat antal lokförare** är angivet under **Inställningar › Allmänt** följer en remsa med staplar
grafernas tidsaxel – ovanför dem med vågrät tidsaxel, till vänster om dem med lodrät. Varje
stapel visar hur många lokförare som behövs vid den tidpunkten, på den mest belastade köromgången:

- varje tåg och varje växlingsuppgift behöver en förare från sin första ankomst till sin sista avgång,
  inklusive uppehållen på vägen,
- en förare i en **tjänst** är också bunden mellan de tåg tjänsten kör – väntar eller går till
  nästa – och före det första tåget eller efter det sista, när tjänstens start- eller sluttid
  har satts att sträcka sig utanför dem.

Staplarna färgas efter det antal du förväntar dig: röda när fler förare behövs än
förväntat, gröna när exakt så många, gula när en eller två är över och grå när tre eller fler
är över. En linje markerar det förväntade antalet. Peka på en stapel för att se dess tider och antal.

### Start- och sluttid

Grafen visar den tidsram som anges i **Inställningar › Allmänt**.
Ett särskilt alternativ gör att grafen visar ett helt dygn från 00:00 till 24:00 för kontinuerlig trafik.
Ett tåg vars tider fortsätter efter midnatt viker runt och fortsätter från 00:00 i början av axeln.
