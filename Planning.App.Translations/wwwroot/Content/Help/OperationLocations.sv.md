Fliken **Driftplatser** beskriver de driftmässiga platserna på din järnväg – de stationer och andra
platser där tåg går, stannar eller styrs, tillsammans med deras spår.

### Hur väljer jag typ av driftplats?

1. **Signalstyrd**: om den är **obemannad** och styrs av **signaler** från en angränsande **station**, ange **styrs från**.
   Det är en **växel** där fler än två bansträckor möts, en **mötesplats** där du kryssar i
   **Tåg kan mötas?**, båda, eller ingen av dem – en mellanliggande **blockpost**.
2. **Industriområde**: om det är **obemannat** och **enbart gods**. Kan definieras som **styrs från** en angränsande **station** och ha en **nyckel** för upplåsning/låsning, se *Låsnycklar* nedan.
3. **Station**: Kan som tillval vara **bemannad**. En station kan anges som **magasin**.
   Om den är obemannad kan en station definieras som **styrs från** en angränsande **station** och ha en **nyckel** för upplåsning/låsning, se *Låsnycklar* nedan.
4. **Annan**: inte **bemannad**, inte styrd av **signaler** men behöver finnas i tidtabellen, som en driftmässig plats av typen:

En obemannad station, ett industriområde eller en signalstyrd växel eller mötesplats som **styrs
från** en bemannad station sköts av den stationens tågklarerare som om det vore den egna: dess tåg står på
den stationens tågklareringslista. En blockpost delar bara linjen i avsnitt. Kryssa själv i **Tåg kan
mötas?**: spåren kan inte avgöra det, eftersom en växel har två oavsett om tåg kan mötas där eller inte. Där ingen styr en obemannad plats är den bara en del av linjen mellan
tågklarerarna, och tåg kan inte mötas där. Se *Trafikledningssträckor* på fliken **Sträckor**.

#### Spår på en driftplats

De spår du definierar här används när du planerar tågens uppehåll vid driftplatser.
- En driftplats **måste** ha minst ett **tidtabellsspår**.
- Spårtilldelningen för tåg valideras, så ett spår kan bara vara upptaget av ett tåg åt gången.

#### Plattformar

Där en plats utbyter **resande** har varje spår också en **plattformslängd** i meter, med en
decimal. En längd över noll betyder att det finns en plattform vid spåret; noll betyder att det inte finns någon, och
resande kan varken stiga på eller av där. Kolumnen visas bara där resande utbyts,
eftersom en plattform inte betyder något någon annanstans.

Ett nytt persontåg ställs på ett spår med plattform – helst det **huvudspår** som har det – vid varje
plats det **stannar** vid. Där platsen saknar plattform, och där tåget bara går igenom, tar
det huvudspåret som vilket annat tåg som helst. Det är vad som händer på en plats som inte utbyter några
resande: tåget står där utan att utbyta något, vilket är precis vad ett möte eller en
omkörning är.

Kryssa i **Resande?** för en plats vars spår ännu inte har några plattformar så får varje spår
en plattform på en meter, som du sedan justerar. Detsamma görs med en plan som gjordes eller importerades innan plattformslängder fanns, första gången den öppnas: varje spår på en plats som utbyter resande får en
meter, så att planen fungerar precis som förut, och du förkortar eller rensar de spår som i verkligheten
saknar plattform. En plats där en plattform redan har registrerats lämnas ifred, liksom ett
spår du lägger till på en sådan plats: du anger själv om det spåret har en plattform.

Ett persontåg som stannar för att utbyta resande vid ett spår utan plattform listas under
**Konflikter**, och du avgör vilket av två det är: ge spåret en plattformslängd, eller rensa
uppehållets rutor **Ank** och **Avg** på fliken **Tåg**, vilket säger att tåget bara står där
utan att utbyta något. Ingenting rättas åt dig, eftersom bara du vet vilket det är. Där en plats har en
plattform vid bara ett spår – det vanliga på en liten station – kan två persontåg som möts
där inte båda få den, och det som saknar den är det som rapporteras. Hela kontrollen kan stängas
av under **Inställningar › Validering**.

#### Vilket spår ett tåg ställs på

Där en plats har mer än ett spår och någonstans att fortsätta till kan varje spår ange vilken väg
genom platsen det är till för: den **föregående** platsen ett tåg kommer från, den **nästa** det går
vidare till, eller båda. Kryssa i **båda riktningarna** så gäller samma spår också för tåg som går åt andra hållet.
Bara de platser som nås med en sträcka härifrån erbjuds, så ett spår kan bara pekas ut för en
väg som ett tåg faktiskt kan ta.

Det är vad ett **dubbelspår** behöver: ge ett spår den ena riktningens föregående och nästa platser och det andra spåret
samma par omvänt, lämna båda riktningarna utan kryss, så får varje riktning
sitt eget spår. Lämna kolumnerna tomma så ändras ingenting från förut.

Ett nytt tåg ställs på det spår som passar dess väg bäst. Ett spår som pekar ut exakt var tåget
har kommit ifrån och vart det går vidare slår ett som bara pekar ut ett av dem, vilket i sin tur slår ett spår
som inte pekar ut något; ett spår som pekar ut en plats tåget aldrig berör används inte alls, såvida inte varje
spår på platsen är sådant, och då står tåget på det bästa av dem ändå. Där två spår passar
vägen lika bra tar ett persontåg som **stannar** ett spår med plattform, och ett tåg som går
**igenom** – och varje tåg utan resande att utbyta – tar huvudspåret. Bara spår i
tidtabellen beaktas så länge det finns något sådant.

Kolumnerna visas bara där det finns ett val att göra. Ingenting kastas bort där de är
dolda: ta en plats ner till ett spår, så finns det spåren var utpekade för tillbaka så snart
du lägger till ett till. Tar du bort en plats frigörs de spår som pekats ut för tåg till och från den.

#### Var kan tåg mötas/köra om?

- På en **bemannad** station.
- På en obemannad **signalstyrd plats**.

#### Var tåg stannar

Platstypen avgör om ett tåg får stanna, utöver hur du ställer in varje uppehåll.
- Du anger om en driftplats utbyter **resande** och/eller **gods**:
- Ett **magasin** utbyter alltid båda, vad de två rutorna än säger, eftersom det står för
  allt bortom den modellerade banan – så rutorna visas ikryssade och inaktiverade för ett sådant.

Ett tåg behöver också något att utbyta. Ett persontåg stannar bara där **resande** utbyts, och
ett godståg bara där **gods** utbyts.
Ett blandat tåg med både person- och godsvagnar kan stanna vid driftplatser som utbyter **resande** och/eller **gods**.

Där ett tåg inte kan stanna visas dess rutor Ank och Avg på fliken **Tåg** rensade och
kan inte kryssas i. Ingenting kastas bort: slå på utbytet igen så finns varje uppehåll som planerats tidigare
där igen.

### Arbeta med listan

Listan visar varje plats signatur, namn, typ, ägare, om den är en magasinsstation (slutstation för yttre stationer) och hur
många spår den har. För varje rad kan du:

- **Info** – se alla uppgifter, inklusive spåren, skrivskyddat.
- **Redigera** – öppna ett formulär på plats med fälten för den platsens typ.
- **Ta bort** – ta bort platsen. Först visas allt som borttagningen skulle ändra, och
  du bekräftar eller avbryter. Tåg som har uppehåll där förlorar de uppehållen: ett tåg som går igenom
  går nu rakt förbi, och ett som börjar eller slutar där börjar eller slutar nu vid sitt nästa eller föregående
  uppehåll. Där platsen ligger mellan exakt två grannar ersätts dess två bansträckor av en som
  förbinder grannarna (lika lång, och med samma gångtid som de två tillsammans), och tidtabellssträckorna
  genom den går i stället över den. Borttagningen nekas, med skälen listade,
  så länge ett fordonsomlopp, en förartjänst eller ett godsflöde börjar eller slutar där, eller så länge ett tåg skulle
  få en sträckning som inte hänger ihop – ett som vänder där, eller går igenom som
  växel.

**Lägg till ny** frågar först efter typ (station, signalstyrd eller annan plats) och öppnar sedan
redigeringsformuläret för den typen. En ny plats sparas när dess namn och en unik signatur är ifyllda.

#### Instruktioner

En **station** eller ett **industriområde** har ett fält för **instruktioner**, skrivet i Markdown och visat bredvid en levande
förhandsgranskning. Det är till för hur *den här* platsen sköts vid *den här* träffen: vilka spår som används till vad, hur
växlingen ordnas och vad lokförarna och de som bemannar platsen mer behöver veta. Hur
platsen sköts i allmänhet, och all annan beskrivning av den, ska ägaren lämna och
hör inte hemma här.

Fältet visas inte där det inte finns något att instruera: tåg går bara förbi en signalstyrd
plats, och ingen arbetar på en annan plats, så ett tåg där gör vad dess uppehåll säger och inte mer.

#### Redigera spår

Redigeringsformuläret listar platsens spår, där du kan lägga till, redigera och ta bort dem. Ett spår kan bara
tas bort så länge inget tåg använder det; annars är dess borttagningsknapp inaktiverad och visar hur många
tåg som använder det. Flytta eller ta bort de stationsuppehållen först.

### Stationer och regioner

En station kan kopplas till en eller flera **regioner** ur banans regionkatalog (som hanteras på fliken
**Regioner**). Det används mest för magasinsbangårdar, som representerar yttre stationer eller regioner
bortom den modellerade järnvägen. Om banan ännu inte har några regioner kan du lägga till standarduppsättningen från formuläret.

### Omstigningsstationer

Där träffen körs med **resandebiljetter** – aktiverat under **Inställningar › Allmänt** – är vissa
stationer de platser där en resande lämnar ett tåg och fortsätter med nästa. Kryssa i **Omstigningsstation?**
på var och en av dem.

Inget mer behöver planeras. Ett persontåg som **stannar** vid en sådan får vid ankomsten veta att det ska *byta
resande här*. Ett tåg som bara går förbi lämnar över ingen, och ett godståg har ingen att lämna över, så
ingetdera får någon anmärkning.

Omstigningsstationerna kan också listas i häftet med allmänna instruktioner genom att skriva `<Interchanges/>` på
en egen rad i de stående instruktionerna under **Inställningar › Information**. De skrivs ut där
du placerar taggen och ingen annanstans.

Rutan erbjuds bara där den betyder något: träffen måste använda biljetter och stationen måste utbyta
**resande**. Stänger du av biljetterna behålls markeringarna, precis som en låsnyckel – inget
härleds från dem under tiden, och slår du på biljetterna igen kommer omstigningsstationerna direkt tillbaka.

### Låsnycklar

Där gods utbyts men ingen är i tjänst – en obemannad station eller ett industriområde – är
växlarna oftast låsta med hänglås, och nyckeln förvaras på en bemannad station längs linjen. Ange **Låsnyckel förvaras vid**
till den stationen och ge nyckeln ett namn om stationen förvarar fler än en.

Inget mer behöver planeras. Ett godståg som stannar vid den station som förvarar nyckeln och senare stannar vid
den plats nyckeln låser upp får vid sin avgång från nyckelstationen veta att det ska *hämta nyckel A1
för upplåsning av Bruket*; när det nästa gång stannar vid den stationen på återvägen får det vid ankomsten veta att det ska *lämna nyckel
A1 från Bruket*. Ett tåg som bara går förbi någon av platserna får ingen anmärkning, eftersom det inte låser upp något.

Nyckeln hämtas vid det sista uppehållet vid den förvarande stationen före arbetet och lämnas tillbaka vid det första
efter det, så ett tåg som har uppehåll där två gånger ombeds inte släpa nyckeln med sig under ett extra besök.

En nyckel betyder bara något så länge båda ändarna håller. Markera själva platsen som bemannad, eller ta bort
bemanningen från stationen som förvarar nyckeln, så gäller nyckeln inte längre: inga anmärkningar görs av den, och
**Konflikter** säger vilken av de två ändringarna som orsakade det. Själva nyckeln finns kvar, den kastas inte bort, så om
ändringen ångras kommer den direkt tillbaka – och den står kvar i formuläret, där du kan peka ut en annan
station eller rensa den helt.

#### Växellok

En station, ett industriområde eller en annan plats kan ha **växellok**: lok som är stationerade där för
den växling platsen behöver. Öppna platsen med **Redigera** och klicka på **Lägg till växellok** under
**Växellok**. Ange operatör, nummer, klass och dragkraftstyp för varje, och en anmärkning om du vill. Ett
växellok har inget omlopp och sätts aldrig på ett tåg. Det finns där under varje köromgång, så dess operatör och
nummer får inte användas av något annat fordon; ett nummer som redan är upptaget nekas.

Vem som tar med ett växellok, dess DCC-adress och dess enhetsnummer anges på fliken **Fordonsägare**, som för
vilket lok som helst. En signalstyrd plats kan inte ha växellok, så en plats med växellok kan inte
ändras till en sådan. Om en plats tas bort tas dess växellok bort också, vilket du får veta innan du bekräftar.
