# Versionsnyheter

## Version 0.7.9

### Ändringar

- **Tågsammansättningar anger vidarebefordrade vagnar efter var de kommer ifrån.** Ett godsflöde med
  **Ursprungsdriftplatser** visas nu som "Vagnar från" sina ursprung i sin rektangel, i stället för med
  sina destinationer, så att vagnarna kan hittas efter var de kommer ifrån. Andra godsflöden på samma plats
  i tåget visar fortfarande sina destinationer, och begränsningen räknar fortfarande med dem alla.

## Version 0.7.8

### Ändringar

- **Ett tåg som ankommer till eller avgår från ett spår som inte är tidtabellslagt varnas för.** Ett tåg
  som ankommer eller avgår på ett spår där rutan **Tidtabellslagd?** inte är ikryssad på fliken
  **Driftplatser** listas nu under **Konflikter**, med tåg, driftplats, tid och spår. Ett tåg som bara
  väntar på ett sådant spår, till exempel på ett möte, listas inte, och inte heller ett växlingsuppdrag.
  Ingenting ändras åt dig: flytta antingen uppehållet till ett tidtabellslagt spår på fliken **Tåg**,
  eller kryssa i spårets ruta **Tidtabellslagd?**. Kontrollen kan stängas av under
  **Inställningar › Validering**.

## Version 0.7.7

### Ändringar

- **Fordon som inte är i trafik kan tas bort.** På fliken **Fordonsägare** har ett fordon som visas som
  **Inte i trafik** en knapp **Ta bort**, och **Ta bort de som inte är i trafik** tar bort alla sådana
  fordon i listan — bara de som visas när **Bara de utan ägare?** är ikryssad. Båda frågar först. Fordonets
  ägare tas bort tillsammans med det; deltagarna finns kvar. Ett fordon som har uppgifter måste först tas ur
  sina omlopp på fliken **Omlopp**.
- **Rapporten Fordon och ägare benämner varje fordon som fliken Omlopp gör.** Den separata kolumnen
  **Klass** är borta: kolumnen **Omlopp** har nu operatörens signatur, nummer och klass (t.ex. "SJ 01 Rc"),
  eller det externa id:t för ett fordon som har ett, som på fliken **Omlopp**.

## Version 0.7.6

### Ändringar

- **Listan över fordonsägare benämner varje fordon som fliken Omlopp gör.** På fliken **Fordonsägare** är
  de separata kolumnerna **Fordon** och **Klass** nu en enda kolumn **Fordon** med operatörens signatur,
  nummer och klass (t.ex. "DB 05 BR 218"), eller för ett vagnsätt de vagnar det listar
  (t.ex. "SJ 05 5 x A/B/Fv"). Ett fordon med externt id visas med det id:t, som på fliken **Omlopp**.
- **Rapporten Fordon och ägare ritar vagnarna i ett vagnsätt.** Ett vagnsätt som listar sina vagnar visar
  dem nu i sin anmärkning, en rektangel per vagn med klass och nummer, i den ordning de står i tåget, så som
  rapporten **Tågsammansättningar** ritar dem. Ett långt vagnsätt fortsätter på fler rader.

## Version 0.7.5

### Ändringar

- **En ny tjänst börjar med sitt första tågavsnitt.** **Ny tjänst** på fliken **Tjänster** öppnar nu
  dialogen **Lägg till tågavsnitt** direkt, så att tjänsten genast får sin plats i diagrammet i stället för
  att stå tom längst ned. Stängs dialogen utan att något avsnitt läggs till, blir ingen tom tjänst kvar.
- **Lägg till tåg erbjuder bara platser där kategorin gör uppehåll.** I dialogen **Lägg till tåg** begränsas
  från- och tilldriftplatserna till dem i den valda tågkategorins uppehållsmönster. En kategori utan
  uppehållsmönster begränsar ingenting. Byts kategorin, töms en driftplats som inte längre finns bland valen.
- **En träff dygnet runt kan börja när som helst på dygnet.** När **Kör över midnatt?** är markerad på
  fliken **Inställningar** anger **Första köromgången börjar** tiden då den första köromgången eller dagen
  börjar. Varje fordon startar där det står vid den tiden: ett tågavsnitt i den första köromgången som går
  tidigare är inte där det startar, och ett fordon utan något senare i den köromgången startar i nästa
  som det kör. Fliken **Fordonsägare** och rapporten **Fordon och ägare** visar starten på detta sätt.

## Version 0.7.4

### Ändringar

- **Ett tjänstehäfte visar varje tåg en gång.** Där en tjänst delar upp ett tåg i flera avsnitt — för att
  dragfordonet byts, eller vagnar kopplas till eller från på vägen — men lokföraren stannar på tåget hela
  vägen, skriver häftet nu ut tåget en gång, från där föraren tar det till där föraren lämnar det, i stället
  för en sida per avsnitt. Blocken för dragfordon och vagnsätt visar vilket fordon som går vilken del av
  tåget.
- **Godsvagnar som står tillsammans är en rad i ett tjänstehäfte.** Godsflöden som kopplas till vid samma
  station på samma plats i tåget delar nu en rad i blocket med godsvagnar med fraktsedlar, på samma sätt
  som tågsammansättningarna visar dem: varje plats nämns en gång, regionerna efter platserna, och en högsta
  last för hela gruppen, summan av destinationernas. Raderna ordnas efter stationen där vagnarna kopplas
  till, i den ordning tåget når dit, och sedan efter plats i tåget.
- **Tågsammansättningar visar hela tåget där ett vagnsätt kopplas till.** Ett tåg får fortfarande en rad
  bara där ett vagnsätt kopplas till eller godsflödesvagnar tas med, men raden visar nu varje vagnsätt som
  tåget avgår med, även dem det redan har med sig, så att platserna för dem som kopplas till där blir
  begripliga.
- **Tågsammansättningar ramar in varje vagnsätt.** Ett vagnsätts omlopp och dess vagnar står nu tillsammans
  i en skuggad ram, så att de läses som en grupp under ett omlopp i stället för att omloppet läses som
  ännu ett vagnsätt bredvid dem.
- **En högsta last per godsrektangel i tågsammansättningar.** En godsrektangel med flera destinationer
  anger nu summan av deras högsta last en gång, sist, i stället för en siffra per destination.
- **En vagn som läggs till ett vagnsätt är en personvagn.** En ny vagn i dialogen **Redigera fordon** är
  nu från början en personvagn, inte en godsvagn.

### Rättningar

- **Tider efter midnatt hamnar på rätt dygn.** På en bana med **Kör över midnatt?** ikryssat hamnar en
  tid efter midnatt som anges på ett tåg som går över midnatt — till exempel 00:10 som skrivs över
  23:55 — nu på nästa dygn, så att tågets uppehåll står kvar i den ordning tåget går. Planer som sparats
  tidigare med sådana tider rättas när de öppnas.

## Version 0.7.3

### Ändringar

- **Ankomstspår kan läggas där nästa tåg avgår.** En ny knapp för **ankomstspår** på varje omlopp på
  fliken **Omlopp** flyttar omloppets ankomster till spåret nästa tåg avgår från, oavsett fordon. Ett
  omlopp med tågsätt eller vändtåg rättas fortfarande av sig självt, och nu även när
  **Vändtåg?** kryssas i på ett lok som redan är tilldelat.
- **Listan över driftplatser är lättare att arbeta i.** Knapparna för en driftplats står nu direkt efter
  dess namn i stället för längst bort på den breda raden, så det syns tydligt vilken driftplats de hör
  till. Ett klick på själva namnet öppnar informationen om driftplatsen. Varannan rad är skuggad, och
  raden under muspekaren markeras tydligt.
- **En station visar de platser den betjänar med gods.** På fliken **Driftplatser** listar informationen
  om en station nu de platser vars gods betjänas från den, och informationen om en plats visar vilken
  station som betjänar den. De utskrivna bladen för driftplatser listar också de betjänade platserna.
- **Tågsammansättningar visar ankommande godsflödesvagnar.** Ett tåg som ankommer med godsflödesvagnar som
  kopplas loss vid en station får nu en egen rad på stationens blad, med sin avgångstid där tåget går
  vidare, och med en streckad rektangel per plats i tåget som anger varifrån vagnarna kom. Bara godsflöden
  med **Koppla loss?** ikryssat visas, utom vid en skuggstation, där alla ankommande vagnar visas.
  Skuggstationer får nu också egna blad.
- **Tågsammansättningar visar varje vagnsätt där det kopplas till.** Ett vagnsätt som inte listar sina
  vagnar ritas nu också, med enbart sin skuggade omloppsrektangel. Ett vagnsätt visas bara där det kopplas
  till: vid den första avgången i sitt omlopp, och senare bara där det uttryckligen kopplas till — med en
  anmärkning om tillkoppling, hämtat från ett annat spår eller kopplat till ett tåg som redan går. Ett
  vagnsätt som följer sitt lok från tåg till tåg visas inte igen. Var ett vagnsätt står i tåget anges per
  tillkoppling: **Plats** i dialogen **Redigera tågavsnitt** på fliken **Omlopp**, så att flera vagnsätt
  som kopplas till vid samma station ritas i den ordningen.
- **Tågsammansättningar tar mindre plats.** Destinationerna, och ursprungen för ankommande vagnar, skrivs
  nu i följd som en kommaseparerad lista i sin rektangel i stället för en per rad. Kolumnen **Omlopp** är
  borta: varje vagnsätts omlopp anges i en skuggad rektangel framför dess vagnar, vilket ger
  sammansättningarna mer bredd. Kolumnen **Till** heter nu **Till/från** och anger *till* vart ett
  avgående tåg ska och *från* varifrån ett ankommande tåg kom.
- **Tågsammansättningar ritas åt det håll tågen går.** Varje tåg börjar nu med en lokrektangel i den ände
  det är på väg mot, med en pil, och vagnarna följer bakom, så att ordningen på papperet är ordningen på
  spåret. Loket rymmer dagarna eller köromgångarna, tåget och dess tider vid stationen (**06:00-06:45**)
  och sist tågets högsta last, och ersätter fem kolumner. Ett tåg som går i sin bansträckas definierade
  riktning pekar åt höger; ett som går mot den pekar åt vänster. Varje sida följs av sin spegelbild för
  spårens andra sida: skriv ut dubbelsidigt och vänd bladet till den sida som stämmer med det du ser. De
  angränsande driftplatserna anges i var sin ände av sammansättningens rubrik.
- **Regioner sist i tågsammansättningar.** Där flera destinationer delar en plats i tåget listas alla
  platser först och deras regioner efter dem, varje region en gång.

## Version 0.7.2

### Nya funktioner

- **Driftplatser kan nu skrivas ut.** En ny rapport under **Rapporter** ger varje driftplats på banan
  ett eget blad på A4 stående: dess egenskaper, dess regioner, instruktionerna för hur den ska skötas på
  träffen, och dess spår med längder, plattformar, vägar och användning. Bara de egenskaper som gäller
  för den sortens driftplats listas, och de driftstider som visas är de som gäller — driftplatsens egna,
  annars banans. Instruktionerna skrivs ut här för första gången. En driftplats med mer än vad som får
  plats på ett blad fortsätter på nästa, och nästa driftplats börjar ändå på ett nytt blad. Med **Skriv
  ut rapporter på lokala språk** ikryssat skrivs varje blad ut på språket i driftplatsens land.

### Ändringar

- **Bara de driftstider som gäller erbjuds.** På fliken **Driftplatser** erbjuds tiden för att köra runt
  loket bara på stationer, skuggstationer inräknade, och tiden för tågklarering bara på bemannade
  stationer, eftersom det krävs en tågklarerare i tjänst för att klarera ett tåg. En tid som angetts
  tidigare där den inte längre gäller behålls, men varken visas eller skrivs ut.

## Version 0.7.1

### Nya funktioner

- **Rapporter skrivs ut på banans språk.** Alla rapporter skrivs nu ut på banans standardspråk, det
  första språket i dess standardland, oavsett vilket språk du själv arbetar på. De utskrivna bladen läses
  av deltagarna på träffen, inte av dig. Datum och tal följer också landet.

  Kryssa i **Skriv ut rapporter på lokala språk** under **Inställningar › Allmänt**, så skrivs varje
  förartjänst ut på språket för företaget som kör den, varje omloppskort på språket för fordonets
  företag och varje stations tågklareringslista på språket för stationens land. Det gäller även
  tågklareringslistor som sparas som dokument. En tjänst eller ett kort utan företag får språket för
  tågens operatörer, om alla har samma språk. Allt utan ett språk som appen kan skriva ut på behåller
  standardspråket.

- **En tågkategori anger var dess tåg gör uppehåll.** Fliken **Tågkategorier** har ett
  **uppehållsmönster**: kryss för de driftplatser där kategorins tåg gör uppehåll under vägen. Ett nytt
  tåg som skapas på fliken **Tåg** får ett uppehåll vid varje ikryssad driftplats det passerar och kör
  genom de övriga — där tåget börjar och där det slutar är uppehåll oavsett vad som är ikryssat, eftersom
  det är där det görs i ordning och ställs av. Bara de driftplatser som kategorin över huvud taget kan
  stanna vid erbjuds.

  Ett tåg som stannar någonstans som mönstret inte anger listas under **Konflikter**, med tåg, driftplats
  och tid. Ingenting rättas åt dig: bara du kan avgöra om det är tåget som stannar där det borde köra
  genom, eller mönstret som saknar en driftplats där kategorins tåg gör uppehåll. Kontrollen kan stängas
  av under **Inställningar › Validering**.

  **Hämta från tågen** kryssar i de driftplatser där kategorins befintliga tåg gör uppehåll, och det görs
  åt dig första gången en plan från en tidigare version öppnas — varje kategori får det mönster dess tåg
  har kört hela tiden, så ingenting rapporteras som inte var ett fel förut. Kryssar du inte i något lämnas
  kategorin obunden: dess tåg gör då uppehåll överallt där de kan lämna av det de för med sig, som
  tidigare. En växlingskategori har inget uppehållsmönster, eftersom dess uppdrag inte går någonstans.

- **Ett växlingsuppdrag behöver inget eget lok, men det behöver en lokförare.** Ett uppdrag utförs lika
  ofta av det tåglok som redan står på stationen, eller av ett stationslok du inte lagt upp som fordon,
  som av ett lok som bokats för det. Ett **omlopp** som bara innehåller växlingsuppdrag är därför färdigt
  utan tilldelat fordon och listas inte längre under **Konflikter** som ett omlopp utan fordon. Lägg in
  ett tåg som går någonstans i samma omlopp, så krävs fordon igen.

  Vad ett uppdrag däremot behöver är någon som utför det, så det erbjuds nu på fliken **Tjänster** som
  vilket tågavsnitt som helst, med eller utan eget lok — skrivet med stationen en gång och de tider
  arbetet pågår mellan, eftersom det inte går någonstans. Ett uppdrag som ingen tjänst täcker listas under
  **Konflikter** för de sessioner det lämnas obemannat, jämte de avsnitt ett lok drar.

  I ett tryckt tjänstehäfte har uppdragets sida rubriken **Växlingsuppdrag** med operatörens signatur, inte
  ett tågnummer ingen använder, och där ett tåg har sin tidtabell har ett uppdrag ett eget block:
  **Arbetstider**, en rad med stationen och tiderna arbetet **startar** och **slutar**, med
  växlingsinstruktionerna under. Inget spår anges — ett uppdrag utförs över hela stationen, inte från
  ett spår.

- **Staplarna för lokförare räknar med väntetiden i en tjänst.** Staplarna ovanför den grafiska
  tidtabellen räknade en lokförare som behövd bara medan ett tåg eller ett växlingsuppdrag kördes. En
  lokförare vars **tjänst** har ett uppehåll mellan två tåg är ändå upptagen under tiden, i väntan på eller
  på väg till nästa, så den tiden räknas nu också — de sessioner tjänsten körs. Det gör även en starttid
  du angett före tjänstens första tåg, eller en sluttid efter dess sista.

- **Tåg på linjen kontrolleras så som tågklarerarna ser det.** Listan **Konflikter** såg tidigare på en
  bansträcka i taget, så två tåg kunde mötas vid en obemannad station, eller följa varandra förbi en, utan
  att det sades något. Nu ser den på varje trafikledningssträcka som helhet. En signalreglerad plats, till
  exempel en blockpost, delar trafikledningssträckan i avsnitt som vart och ett rymmer ett tåg per spår. På
  enkelspår kan tåg i motsatt riktning bara mötas vid ändarna, eller vid en signalreglerad plats där tåg
  kan mötas. Tåg i samma riktning kan följa varandra, ett per avsnitt. Se **Trafikledningssträckor** i
  hjälpen på fliken **Sträckor**.

- **En bemannad station kan fjärrstyra en förgreningspunkt eller en obemannad station.** Fältet **Styrs
  från** på fliken **Driftplatser**, som hittills bara fanns på signalreglerade platser, erbjuds nu även
  på obemannade stationer och industriområden. Bara bemannade stationer erbjuds som styrande station. En
  fjärrstyrd förgreningspunkt, mötesplats, obemannad station eller ett fjärrstyrt industriområde sköts av
  den stationens tågklarerare som en egen: trafikledningssträckor slutar där, dess tåg står på den
  styrande stationens tågklareringslista i tidsordning bland stationens egna, med sin signatur före
  spåret, och rubriken nämner den. Stationerna bortom den hör till dem som den styrande stationen ringer,
  och de ringer den styrande stationen. En blockpost, en signalreglerad plats som varken är
  förgreningspunkt eller mötesplats, förblir en del av linjen. Tryck på **Skapa om från bansträckor** på
  fliken **Sträckor** efter att ha angett en styrande station.

- **Ange var tåg kan mötas.** En signalreglerad plats har en ny kryssruta **Tåg kan mötas?** på fliken
  **Driftplatser**. Kryssa i den där ett tåg kan vänta medan ett annat passerar. Antalet spår kan inte
  avgöra det, eftersom en förgreningspunkt har två spår för att veta vart ett tåg ska, oavsett om tåg kan
  mötas där. En signalreglerad plats som varken är förgreningspunkt eller ikryssad är en blockpost.
  Importerade platser börjar utan kryss, så kryssa i mötesplatserna efter importen.

### Ändringar

- **Lokala destinationer namnges.** En godsdestination med **Och lokala?** ikryssat lyder inte längre
  *Stilkøbing och lokala destinationer*: den namnger platserna, *Stilkøbing, Vig, Rubjerg* — stationen
  följd av varje plats vars gods betjänas från den (**Godsbetjänas från** på fliken **Driftplatser**). En
  station som inte betjänar något namnges ensam. Uttrycket är också borta från godsförklaringen i de
  allmänna instruktionerna, eftersom inget skriver ut det längre.

## Version 0.7.0

### Nya funktioner

- **Tabellen över rangerbangårdar kan nu placeras i de allmänna instruktionerna.** Tabellen över
  rangerbangårdar skrivs ut på bansidan sist i häftet med de allmänna instruktionerna. På en bana med
  många rangerbangårdar fyllde den sidan och tryckte bort förklaringen av godsflödenas märken. Skriv

  ```
  <ShuntingYards/>
  ```

  på en egen rad under **Inställningar**, där i texten den hör hemma, för att i stället skriva ut den
  där. Bansidan utelämnar den då, så den skrivs aldrig ut två gånger. Förhandsvisningen bredvid texten
  visar en ruta där tabellen kommer. Skrivs den ingenstans står tabellen kvar på bansidan som förut.

- **Fordonsägare: vem som tar med vilket rullande materiel till träffen, och var det ska ställas upp.**
  Fliken **Fordonsägare** listar varje lok, tågsätt och vagnsätt — med antalet enheter där de är fler än en,
  och för ett vagnsätt som listar sina vagnar varje vagnklass en gång — med den första köromgång (eller dag) det är i trafik, och den station, det spår och den avgång där det
  ska stå före den. Öppna en rad för att lägga till ägare: den första tar med fordonet och ställer upp det
  på banan, övriga ägare tar med reserver. Välj en ägare genom att skriva de första bokstäverna i namnet —
  eller i efternamnet — så att ett namn stavas likadant överallt; ett namn som inte matchar någon erbjuds
  som en ny deltagare. Varje ägare till ett lok eller tågsätt, även reserverna, måste ange en DCC-adress;
  skriv **0** när ägaren ännu inte har lämnat den. Varje fordon och varje ägare har en anmärkning, och vyn
  **Deltagare** listar alla med vad de tar med, och där rättas ett felstavat namn en gång för alla.

  Rapporten **Fordon och ägare** under **Rapporter** skriver ut samma lista på A4 liggande, ordnat på ett av
  tre sätt som väljs ovanför sidorna: **Per driftplats**, en sida per station till dess ägare, med fordonen
  som ska ställas upp där i ordning efter första köromgång och avgång; **Per ägare**, en sida per deltagare
  med vad hen tar med, DCC-adresserna och var varje fordon börjar; eller **Efter DCC-adress**, alla medhavda
  lok och tågsätt i en lista. Varje station eller ägare börjar på en ny sida och fortsätter på nästa när en
  sida inte räcker. Fordon som inte är i trafik listas sist; de som ingen tar med ännu kommer först per
  ägare, under **Ännu inte bokade**. Radens bakgrund visar när enheten behövs: vit när den är i trafik alla
  köromgångar, ljusgrå för en reserv och annars ljusblå, ljusgrön eller ljusröd när fordonet är i trafik
  först från första, andra eller tredje köromgången.

- **Ett omlopp kan ange att fordonen står på ett annat spår än tåget.** När ett tågavsnitt redigeras
  under **Omlopp** anger **Hämta från** spåret fordonen står på innan tåget avgår, och **Ställ på**
  spåret de ställs på efter ankomsten — till exempel ett vagnsätt som lämnas på ett sidospår vid en
  mellanstation. Tjänstehäftena och tågklareringslistorna skriver ut det som en anmärkning: *Innan
  avgång hämta vagnsätt 21 från spår 3.* vid avgången och *Efter ankomst växla in vagnsätt 21 till
  spår 3.* vid ankomsten, i stället för anmärkningen om att koppla till eller loss fordonet. Där
  fordonet går i tåget bara vissa av de köromgångar eller dagar tåget går, inleds anmärkningen med dem
  — *1,3,5: Innan avgång hämta …* — och ett fordon som inte går i tåget någon av dem får ingen
  anmärkning. Ett spår som ett omlopp använder på det sättet kan inte tas bort under **Driftplatser**.

- **Tågsammansättningar kan nu skrivas ut.** En ny rapport under **Rapporter** ger varje bemannad station en
  egen sida på A4 liggande, med alla tåg som avgår därifrån med godsflödesvagnar eller med ett vagnsätt som
  listar sina vagnar. Tågen listas spår för spår, och i avgångsordning på varje spår. Bredvid varje tåg ritas
  dess sammansättning framifrån som rektanglar: ett vagnsätt som en rektangel per vagn, i vagnordning, med
  vagnens littera och nummer; och godsflödesvagnar som en rektangel per plats i tåget, med vart de ska — med
  *och lokala destinationer* och *och vidare* där godsdestinationen anger det, och dess regioner i sina
  färger. Sammansättningen är den tåget avgår med, så vagnar det kom med visas liksom de som kopplas till vid
  stationen. Ett vagnsätt som går i tåget bara vissa köromgångar eller dagar märks med dem, och
  godsflödesvagnar utan plats visas sist, som *Var som helst i tåget*.

- **Ett motorvagnståg ankommer nu till det spår det ska avgå från.** Ett motorvagnståg, och ett lok som
  går i vändtåg, gör aldrig rundgång: det avgår från samma spår som det ankom till. När ett sådant fordon
  går tåg efter tåg läggs därför varje tågs ankomstspår på det spår nästa tåg avgår från, och — om omloppet
  slutar där det började — läggs sista tågets ankomstspår på det spår första tåget avgår från, så att
  fordonet står färdigt där nästa köromgång hämtar det. Bara ankomstspår flyttas; det spår ett tåg avgår
  från står kvar som du har angett det. Ett omlopp som dras av ett vanligt lok rörs inte, eftersom loket
  kan köras ensamt över stationen till det spår nästa tåg står på. Spåren rättas varje gång ett tåg läggs
  till i ett omlopp — av **Bygg automatiskt** eller av dig, sist i omloppet eller där fordonet står — och
  varje gång du tilldelar ett fordon till ett omlopp, så ett omlopp som byggts innan motorvagnståget var
  känt rättas så snart motorvagnståget sätts på det. Ett importerat omlopp behåller de spår det lästes in
  med. Skulle flytten ställa två tåg på samma spår samtidigt listas det bland konflikterna för dig att lösa.

### Ändringar

- **Dagar och köromgångar listas utan mellanrum.** Där en anmärkning eller en kolumn anger vilka dagar
  eller köromgångar något gäller skrivs de nu *M,O,F* och *1,3,5* i stället för *M, O, F* och *1, 3, 5*,
  så att uppgiften tar så lite plats som möjligt. Dagar som skrivs ut i sin helhet — *Måndag, Onsdag,
  Fredag* — är oförändrade.

- **Ett vagnsätt som listar sina vagnar visar dem nu i sin etikett under Omlopp.** Etiketten anger antalet
  vagnar och varje vagnklass en gång — *SJ 05 5 x A/B/Fv* — i stället för vagnsättets egen klass.

- **En anmärkning vid ett uppehåll anger nu om den hör till ankomsten eller avgången.** Tjänstehäftena och
  tågklareringslistorna skriver ut ett uppehålls ankomst och avgång på var sin rad, så under **Tåg** frågar
  rutan bredvid varje **Anmärkning** vilken av dem den gäller: **Ank** för något som möter eller ska göras
  vid ankomsten, **Avg** för något som ska göras före eller vid avgången. Där tåget bara ankommer eller bara
  avgår finns bara den ena att välja. Där det passerar kan ingen anmärkning skrivas — kryssa i **Ank** eller
  **Avg** först — men en som redan finns där går fortfarande att ta bort.

  En anmärkning som skrevs med en tidigare version, eller som följde med en XPLN-import, angav ingetdera och
  skrevs därför varken ut i häftena eller i listorna. Första gången en plan öppnas får varje sådan anmärkning
  avgången där tåget avgår, och ankomsten där det bara ankommer.

- **Varje block på en tågsida i ett tjänstehäfte har nu ett färgat streck längs vänsterkanten.** Dragfordon
  markeras med rött, planerade vagnsätt med grönt, godsvagnar med fraktsedlar med blått och tidtabellen med
  grått, så att blocken går att skilja åt med en blick, och där ett grått streck slutar tar det tågavsnittet
  slut. Strecken skrivs ut utan att bakgrundsgrafik behöver slås på, och varje block behåller sin rubrik, så
  en utskrift i svartvitt förlorar ingenting.

- **Dragfordon och planerade vagnsätt på en tågsida i ett tjänstehäfte visar nu sina spår.** Varje rad anger
  spåret fordonet står på vid början och spåret det lämnas på vid slutet — tågets eget spår, eller det som
  omloppet anger under **Hämta från** eller **Ställ på**. Kolumnen som anger fordonet har nu rubriken
  **Omlopp** i båda blocken, efter kortet som fordonets identitet står på. Med spåren i tabellen anger
  tidtabellen nedanför inte längre varje vagnsätt och dess spår: den säger *Växla vagnar till avgångsspåret
  före avgång.* eller *Växla vagnar till deras ankomstspår efter ankomst.*, inledd av de köromgångar eller
  dagar det gäller när vagnarna växlas bara vissa av dem tåget går. Tågklareringslistorna anger fortfarande
  varje vagnsätt och dess spår.

- **Häftet med allmänna instruktioner förklarar nu hur godset skrivs i de andra rapporterna.** Under
  **Godsflöden** på häftets sista sida säger en förklaring att varje lastgräns är ett högsta värde och vad
  märket efter ett tal räknar, vad jordgloben står för, vad *och lokala destinationer* och *och vidare* lägger
  till en destination, och vad ett färgat regionnamn betyder.

- **Första sidan i häftet med allmänna instruktioner har plats för ett längre program.** Programmet sätts med
  mindre avstånd mellan raderna, punkterna och dagrubrikerna — textstorleken är densamma — vilket ger plats
  för fyra eller fem punkter till. Ett program som är för långt för sidan tappar sina sista punkter, och de är
  slutet på träffen, precis det som folk slår upp.

- **Bygg automatiskt fyller nu på de omlopp du redan har innan det gör nya.** Tågen som ännu inte ingår i
  ett omlopp erbjuds först de befintliga omloppen: ett omlopp fortsätter med det som fortsätter det där
  det ankommer, i den kategori det redan går i, och ett tomt omlopp som du själv har gjort fylls innan
  något nytt skapas. Bara de tåg som inte passar i något befintligt omlopp startar nya omlopp, så att
  bygga igen efter att ha lagt till några tåg förlänger de fordon som redan går i stället för att sätta nya
  i trafik. Godsflöden lämnas som de är. Resultatet bredvid knappen anger nu hur många tågavsnitt de
  befintliga omloppen tog emot, hur många av dem som förlängdes och hur många omlopp som byggdes.

### Rättningar

- **Förklaringen av godsflödenas märken rinner inte längre av häftet med de allmänna instruktionerna.**
  Formuleringarna *och lokala destinationer* och *och vidare*, samt förklaringen av en region, sattes i
  en så smal kolumn att var och en gick på fyra rader, och på en bana med flera rangerbangårdar föll den
  sista av dem utanför sidans nederkant. Märkena och formuleringarna sätts nu som två listor under
  varandra, var och en över hela sidan.

- **En tidtabell som fortsätter på motstående sida i ett tjänstehäfte ser nu ut som alla andra.** När ett
  tågavsnitt är för långt för en sida flyttas dess tidtabell till motstående sida, och där skrevs den ut utan
  häftets egen utformning: med större text, utan de fetstilta stationerna och tiderna och utan linjerna
  mellan uppehållen, så en lång tidtabell kunde hamna nedanför sidans nederkant.

- **Att ändra ett tågs nummer eller kategori under Tåg lämnar inte längre ändringen kvar på en annan rad.**
  Båda ändringarna sorterar om listan, och numret du skrev eller kategorin du valde kunde bli stående på raden
  för det tåg som flyttade in på dess plats.

- **Dialogerna läser nu in ett tal medan du skriver det.** **Varaktighet (minuter)** för ett nytt
  växlingsuppdrag, **Minuter** att flytta eller kopiera tåg med och ett fordons **Nummer** under **Omlopp**
  lästes först när du lämnade fältet, så knappen som bekräftar dialogen — och varningen att ett
  fordonsnummer redan är upptaget — släpade efter tills du klickade någon annanstans.

- **Den grafiska tidtabellen ritar nu en driftplats spår i den ordning du har gett dem.** Den brydde sig inte
  om spårens **Ordning** under **Driftplatser** och kunde därför rita dem i en annan ordning än alla andra
  spårlistor i appen.

- **Ett omloppskort för ett fordon som går dagar som inte följer på varandra anger nu dagarna.** Ett kort för
  måndag, onsdag och fredag skrev ut *MondayShort,WednesdayShort,FridayShort* i stället för *M,O,F*.

## Version 0.6.0

### Ändringar

- **Tjänstetåg är en ny sorts tågkategori.** Ge en tågkategori typen **Tjänstetåg** under
  **Tågkategorier** för tåg som varken lämnar av eller tar upp något där de stannar: ett arbetståg, eller
  ett lok eller tågsätt som förs ut ur trafik. Ett sådant tåg får göra uppehåll där en driftplats varken
  utväxlar resande eller gods — till exempel en arbetsplats — och när dess tåglägen byggs får det inga
  uppehåll mellan sina ändpunkter, så det uppehåll som betyder något lägger du in själv. Namnge kategorin
  efter vad tågen gör: ett tåg som lämnar materialvagnar efter sig hanterar gods och hör hemma i en
  godskategori.

  En kategori i en plan gjord med en tidigare version som varken var person eller gods — vilket en
  XPLN-import kan lämna efter sig — visas nu som ett tjänstetåg, där den förut visades som ett persontåg.

- **Växlingsuppdrag är en ny sorts tåg.** Ge en tågkategori typen **Växlingsuppdrag** under
  **Tågkategorier**, så utförs tågen i den kategorin på en station under en viss tid i stället för att
  gå någonstans: vart och ett har ett enda uppehåll, där ankomsttiden är när arbetet börjar och
  avgångstiden när det slutar.

- **Godsflödena i ett växlingsuppdrag anger vilka vagnar som ska växlas.** Lägg till godsflöden i
  uppdraget under **Godsflöde** på samma sätt som för vilket godståg som helst. Ett flöde med
  uppdragets egen station som destination innehåller vagnar som har ankommit, och lokföraren får i
  uppdrag att växla ut dem till godskunderna, med uppgift om varifrån de kommer. Ett flöde till någon
  annanstans hämtas i stället in från godskunderna, med uppgift om vart vagnarna ska. Instruktionen
  skrivs ut i förarturhäftena och i stationsrapporterna.

- **Passagerarbiljetter går nu att skriva ut.** En ny rapport under **Rapporter** ger en tur- och
  returbiljett mellan varje par av driftplatser som har resandeutbyte, vikt på mitten, med den
  persontågsoperatör som har flest avgångar från försäljningsplatsen längst ned på båda halvorna.

- **Tidtabellsrapporten lägger nu flera sträckor på samma ark.** Tabeller som är för smala för att fylla
  bredden står bredvid varandra, så en kort bibana tar inte längre ett helt ark för sig själv.

- **De grafiska tidtabellerna går nu att skriva ut.** En ny rapport under **Rapporter** ritar varje
  sträcka i den fasta pappersskala som ställs in under **Inställningar → Grafisk tidtabell**, så att tider
  och lutningar går att mäta från ett ark till nästa.

- **Inställningar → Grafisk tidtabell är nu ordnad efter vad varje inställning påverkar.** Det som
  tidtabellen visar kommer först, och under det avstånden som används på skärmen, i bildpunkter, bredvid
  dem som används på papperet, i millimeter.

- **Du kan nu ange vad som ska göras med loket där ett tågavsnitt slutar.** När du redigerar ett
  tågavsnitt under **Omlopp** frågas om loket ska vändas och om det ska köras runt till andra änden, och
  vardera skrivs ut som en ankomstanmärkning för lokförare och tågklarerare.

- **Topologi-diagrammet ritar nu hela banans spår, med varje driftplats visad en enda gång.** Spåret är
  enkel- eller dubbelspårigt som sträckan verkligen är och i färgerna för de tidtabellssträckor som går
  över det, och grått där ingen sträcka alls täcker det.

- **Du kan nu ordna Topologi-diagrammet själv.** Dra en driftplats dit den hör hemma så följer spåren med;
  det du ordnar sparas med planen och skrivs ut i tjänstehäftena.

- **En godsdestination med gräns för både vagnar och axlar visar nu båda.** Vagnantalet försvann förut
  där det också stod ett axelantal — både i tjänstehäftena och i godsanmärkningarna — trots att de två
  fälten står sida vid sida under **Godsflöde** och vardera gränsen kan vara den som binder: sexton axlar
  är fyra boggivagnar men åtta tvåaxliga.

- **Tågsidorna i ett tjänstehäfte säger nu detsamma på mindre plats.** Kolumnen med köromgångar har nu
  rubriken **Kör** — det den berättar om fordonet — i stället för ett långt ord över en kolumn med
  cirklar, och godsvagnarna har rubrikerna **Från** och **Till**, som fordonen ovanför dem redan hade.
  Begränsningarna under rubriken skrivs som siffror under ett enda **Max**: hastigheten med sin enhet, talet
  med en cirkel efter för axlar, en fyrkant för vagnar och längden som *2,5m*. Hur många vagnar eller axlar en
  destination tar har flyttats ut ur **Till** till en egen kolumn **Max**, där det läses rakt nedför sidan
  i stället för sist i en rad ortnamn — och den kolumnen visas bara när något på sidan alls är begränsat.

- **Knapparna som gäller ett helt omlopp står nu i en egen kolumn.** Under **Omlopp** har de flyttats till
  en kolumn **Åtgärder** mellan fordonen och tågen, så att varje rads tåg börjar på samma ställe.

- **Rapportmenyn har en ny ordning**, från de allmänna instruktionerna till passagerarbiljetterna.

### Rättningar

- **Den installerade appen fungerar nu utan internetanslutning.** Hjälptexterna, texterna under Om
  och Versionsnyheter samt katalogen med färdiga tågkategorier hämtades från webben varje gång de
  visades och blev därför tomma när du var utan uppkoppling. De sparas nu tillsammans med resten av
  appen när den installeras.

## Version 0.5.1

### Ändringar

- **Vad som ska göras med loken visas nu i förarnas tjänstehäften och i tågklareringslistorna.** Vilket
  lok som ska användas, vad som ska kopplas till och från, och att hämta det från — eller köra tillbaka
  det till — uppställningsspåret räknades hela tiden fram ur fordonsomloppen men skrevs aldrig ut; nu står
  de bland de övriga anteckningarna vid det uppehåll de hör till, och både föraren och tågklareraren ser
  dem. Nytt bland dem är beskedet för ett lok som måste gå runt till andra änden av tåget, eller vändas,
  innan tåget går tillbaka.

- **Häftet med allmänna instruktioner skriver nu ut hela din text, på sidor som går att läsa.** En sida
  räknades som rymligare än den verkligen är, så det som gick över nederkanten föll tyst bort; texten
  fortsätter nu på nästa sida i stället, och en sida slutar aldrig med en ensam rubrik. **Topologi** och
  **Rangerbangårdar** kommer nu på häftets allra sista sida, precis som i tjänstehäftena, och programmet
  på första sidan är satt i häftets egna storlekar i stället för webbläsarens.

## Version 0.5.0

### Ändringar

- **Ett vändtåg står inte längre och väntar på lokrundgång.** Kryssa i den nya rutan **Vändtåg?** på ett
  lok under **Omlopp** där det framför ett tåg som kan köras från båda ändarna — ett tåg med manövervagn
  eller ytterligare ett lok i andra änden — så räknar **Uppdatera tider** bort rundgången och låter tåget
  stå den kortaste uppehållstiden i stället, vilket tidigarelägger alla följande uppehåll. Ett
  motorvagnståg behandlas likadant utan något att kryssa i, och ett uppehåll som du medvetet gjort längre
  lämnas som du har satt det.

- **Ett spår kan nu ange vilken väg genom driftplatsen det är avsett för.** Varje spår kan ange den
  **föregående** driftplats ett tåg kommer från, den **nästa** det fortsätter till, eller båda, med rutan
  **båda riktningarna**, och ett nytt tåg läggs på det spår som passar dess väg bäst. Det är detta ett
  **dubbelspår** behöver: ge de två spåren samma par driftplatser omvänt, så håller sig varje riktning
  till sitt spår. Där två spår passar lika bra tar ett persontåg som stannar ett spår med plattform, medan
  ett tåg som kör igenom tar huvudspåret; lämna kolumnerna tomma, så ändras ingenting mot förut.

- **Ett tåg kan nu kopieras i motsatt riktning och upprepas.** Kryssa i **Motsatt riktning?**, så kör
  kopian sträckan baklänges, med alla gångtider och uppehåll behållna, förberedelse- och avslutningstiden
  bytta ände och ett nummer ur motsatt riktnings serie. Kopieringsdialogen har nu också valet **Upprepa
  tåg**, så ett tåg kan skapas för sig, justeras tills det går som det ska, och först därefter upprepas
  över dagen.

- **Ett spår kan nu ange hur lång dess plattform är.** Varje spår på en driftplats med resandeutbyte har
  en **plattformslängd** i meter — över noll betyder att resande kan stiga på och av där — och ett nytt
  persontåg läggs på ett spår med plattform där driftplatsen har någon. Kryssa i **Resande?**, så får
  varje spår en meter plattform att justera, och en plan gjord före detta behandlas likadant första gången
  den öppnas, så den fungerar precis som förut tills du kortar av eller nollställer de spår som i själva
  verket saknar plattform. Ett persontåg som gör uppehåll för resandeutbyte på ett spår utan plattform
  listas nu under **Konflikter**: ge antingen spåret en plattformslängd eller kryssa ur uppehållets **Ank**
  och **Avg**, vilket säger att tåget inte byter något där. Kontrollen kan stängas av under
  **Inställningar › Validering**.

### Rättningar

- **Att byta namn på banan ändrar nu namnet överallt där det visas.** Framsidan på häftet med de allmänna
  instruktionerna, namnet i övre listen och filnamnet en plan sparas under fortsatte alla visa det banan
  hette förut. En plan som bytt namn tidigare rättas nästa gång den öppnas.

## Version 0.4.2

### Ändringar

- **Nu går det att lägga in ett tåg mitt i ett omlopp.** Mellan tågavsnitten på en rad finns nu små skarvar
  som visar var fordonet står och hur länge, och före det första avsnittet en som visar varifrån det måste
  hämtas; klicka på en av dem för att lägga in ett tåg i luckan, så erbjuds bara de tåg fordonet faktiskt
  hinner med. En tur som inte för fordonet tillbaka läggs in ändå och rapporteras som en konflikt tills du
  lägger in returen — så passas en tur och retur in i ett uppehåll. En skarv där omloppet är brutet, som en
  import kan lämna det, är gulmarkerad.

- **Appen har fått en egen ikon** — fronten på ett modernt tåg mot en mörkblå platta — i stället för märket
  som följer med verktygen den är byggd med. Ikonen syns i webbläsarens flik, och på hemskärmen eller i
  Start-menyn för den som installerar appen.

- **Nu ryms tolv omloppskort på ett ark i stället för tio.** Korten är 48 mm breda i stället för 50, så sex
  får plats i bredd på ett liggande A4-ark, och arket har fortfarande en marginal som vanliga skrivare når.
  Korten är lika höga som förut och innehållet är oförändrat.

- **Raderna i tidtabellen står nu längre isär.** Det finns en sjundedel mer luft runt varje rad, så en rad
  är lättare att följa tvärs över sidan och en station lättare att hitta i kolumnen. Texten och kolumnerna
  är oförändrade, så bladet rymmer samma tåg; en sida tar nu trettionio rader i stället för fyrtiofem.

### Rättningar

- **Den utskrivna tidtabellen tappar inte längre de sista raderna på en sida.** Båda riktningarna av en
  sträcka placerades på samma sida även när de inte båda fick plats, och raderna som blev över klipptes
  bort — rapporten på skärmen sattes i en större stil än den utskrivna, så dess rader var nästan två
  tredjedelar högre än de som räknades. De två sätts nu likadant, hur mycket som ryms mäts på en verklig
  sida i stället för att räknas fram ur stilstorleken, och tre rader hålls fria nederst på varje sida.

- **Godsflödeslistan namnger nu destinationerna vagnarna går till.** Under **Godsflöde › Godståg** stod det
  bara "Vagnar till" i listan att välja ur, utan destinationerna, så posterna gick inte att skilja åt.
  Underfliken och dess kolumn heter nu **Godsdestinationer** i stället för *Godsbeskrivningar*.

## Version 0.4.1

### Ändringar

- **Tågklareringslistorna kan nu sparas som dokument stationsägarna kan redigera.** Välj
  *Tågklareringslistor* på menyn Exportera, så får varje bemannad station ett eget dokument i
  OpenDocument-format, avsett för att skicka varje ägare deras egen lista före träffen så att de kan lägga
  till de lokala instruktioner bara de känner till; är fler än en station bemannad kommer dokumenten
  tillsammans i en zip-fil. Var sidorna bryts lämnas till ordbehandlaren, så sidorna bryts vettigt även
  efter att ägaren skrivit — stationens namn, telefonnumren till stationerna den klarerar tåg till och från
  och kolumnrubrikerna upprepas högst upp på varje sida, men den del av dygnet en sida täcker går inte att
  ange, så sidorna numreras i stället. De utskrivna bladen på menyn Rapporter är oförändrade och är
  fortfarande de man arbetar från under en köromgång.

- **Ett tåg som dras av två lok samtidigt talar nu om vilka två.** Konflikten namngav bara tåget och
  minuterna, så var båda bokade över exakt samma sträcka löd dess två halvor ordagrant lika. Den markeras
  nu också bara på de två omlopp som håller det dubbelbokade arbetet, i stället för på varje omlopp som kör
  det tåget någonstans under dagen.

- **Två lok som delar på ett tåg mellan köromgångar rapporteras inte längre som en konflikt.** Bara
  klockslagen jämfördes, så ett lok på udda köromgångar och ett annat på jämna — hela poängen med att lägga
  upp det så — rapporterades som dubbeldragning. Nu rapporteras det bara där båda är bokade på någon
  gemensam köromgång, och konflikten namnger de köromgångarna.

## Version 0.4.0

### Brytande ändringar

- **Ett fordon du skapar identifieras nu av sin operatör och sitt nummer.** Under en och samma köromgång
  får kombinationen tillhöra bara ett fordon, vilken sorts fordon det än är, så en vagnsats och ett lok kan
  inte längre båda vara *DB 5*; ett fordon utan operatör identifieras av numret ensamt, och två fordon får
  dela identitet så länge de köromgångar de går inte överlappar. Ett **importerat** fordon identifieras
  fortfarande av det externa id det importerades med, så en importerad plan ger inga nya konflikter av
  detta. Att lägga till eller ändra ett fordon avvisar nu en identitet som ett annat fordon redan har och
  kräver ett nummer, medan befintliga planer behålls precis som de är, med varje fordon som delar identitet
  listat bland konflikterna.

### Ändringar

- **Det finns en ny rapport: tågklareringslistan.** Ett eget häfte per bemannad station med de tåg
  stationen hanterar i tidsordning — ett tåg som står där förekommer två gånger, ankomster på vit botten
  och avgångar på ljusgul, eftersom att klarera in ett tåg och att klarera ut det är två skilda handlingar,
  och tåg som bara passerar tas också med. Varje sida har stationens namn, den del av dygnet sidan täcker
  och telefonnumren till stationerna i andra änden av tågklareringssträckorna, och varje rad har en ruta
  per köromgång att pricka av. Varje station börjar på ny sida, så bunten kan delas och lämnas ut; skrivs
  ut från menyn Rapporter.

- **Fälten för att lägga till och ändra ett fordon har fått ny ordning,** densamma på båda ställena: typ av
  fordon, typ av dragkraft, antal enheter, operatör, nummer, klass, köromgångar och sist det externa id:t.
  Fältet som tidigare hette *Företag* heter nu *Operatör*.

- **Ett externt id kan rättas men inte längre hittas på.** Det externa id:t är det namn ett tåg eller ett
  fordon bär i systemet det importerades från, så det som importerats med ett id har kvar sitt fält och kan
  rättas där, medan det som aldrig haft något id nu inte har någon ruta att skriva i. Ett fordon du skapar
  i planeraren får därför inget externt id alls, där det tidigare fick ett påhittat av klass och nummer.

- **Minsta tiden mellan två användningar av samma spår kontrolleras nu.** Inställningen fanns, men
  ingenting använde den: lämnad på 0, där den börjar, ändras ingenting i kontrollen. Sätt den till 5, så
  måste spåret dessutom vara ledigt i fem minuter mellan två tåg — exakt fem räcker, fyra gör det inte —
  och konflikten anger hur kort mellanrummet faktiskt är och hur långt det måste vara.

- **En driftplats kan nu ha egna instruktioner.** Ändringsformuläret har ett fält **Instruktioner**, skrivet
  i Markdown bredvid en förhandsvisning, för hur just den driftplatsen körs på den här träffen: vilka spår
  som används till vad, hur växlingen är upplagd och vad lokförarna och de som bemannar platsen annars
  behöver veta. Fältet erbjuds på en station eller ett industriområde och visas i driftplatsens Info-vy; det
  erbjuds inte där det inte finns något att instruera om.

- **En plats där gods hanteras utan bemanning kan nu kräva en nyckel.** Välj den bemannade station som
  förvarar nyckeln under **Låsnyckel förvaras vid**, och namnge nyckeln om stationen förvarar flera — ett
  godståg som stannar på båda får då vid avgången beskedet *hämta nyckel A1 för att låsa upp Bruket*, och
  vid nästa uppehåll där *lämna nyckel A1 från Bruket*. Nyckeln hämtas vid det sista uppehållet före
  arbetet och lämnas tillbaka vid det första efter det, och ett tåg som bara passerar får inget besked.
  Markera platsen som bemannad, eller ta bort bemanningen från stationen som förvarar nyckeln, så slutar
  nyckeln gälla — **Konflikter** talar om vilken ändring som gjorde det, och nyckeln behålls, så att den
  gäller direkt igen om du ångrar ändringen.

### Rättningar

- **Två sträckor som utgår från samma driftplats ritades som om de aldrig möttes.** Började en
  tidtabellssträcka på just den första driftplatsen på en annan, förband ingenting de två i
  Topologi-diagrammet. Den andra lämnar nu den driftplatsen som vilken gren som helst, i samma fasta vinkel.

- **Varje gränsvärde för kontrollerna anger nu vilken klocka det mäts mot.** Minsta tiden mellan två
  användningar av samma spår saknade helt enhet, och de två tåghastigheterna angav bara *klockminuter*. Alla
  tre anger nu snabbklocksminuter — den klocka tågen går efter, inte verklig tid.

- **Längder och distanser skrivs nu ut i meter,** liksom täljaren i tåghastigheterna, så att *m* inte kan
  tas för en minut. Minsta uppehåll vid en station anges nu också i snabbklocksminuter.

## Version 0.3.5

### Rättningar

- **En sparad plan kunde vägra att öppnas.** Att öppna en plan som appen just hade sparat avbröts med ett
  felmeddelande om ett land, och ingenting lästes in. En redan sparad plan öppnas som den är; du behöver
  inte göra något med den.

- **En sparad planfil är omkring sju gånger mindre.** Att spara skrev planen i en annan form än den som
  hålls i webbläsaren, så varje uppehåll skrevs två gånger, och varje tågkategori, operatör och land om igen
  vid varje tåg, fordon och förartur som använde det. En fil som tog 8 MB tar nu drygt 1 MB; en plan sparad
  av en tidigare version går fortfarande att öppna.

## Version 0.3.4

### Ändringar

- **Rutorna Ank och Avg på ett uppehåll följer nu var tåget verkligen kan stanna.** Ett persontåg behöver en
  driftplats som tar emot resande och ett godståg en som tar emot gods, och ingetdera kan stanna på en
  signalreglerad driftplats; där tåget inte kan stanna visas båda rutorna tomma och går inte att kryssa i,
  och uppehållet blir en genomfart. Inget av det du planerat kastas bort — slå på utbytet igen så finns
  uppehållen där — och ett magasin har alltid utbyte av både resande och gods, eftersom det representerar
  allt utanför banan.

- **Ett uppehåll som något hänger på går inte längre att ta bort.** Tågets eget första och sista uppehåll,
  och ändarna på varje tågavsnitt som ett fordonsomlopp, en förartur eller ett godsflöde planerats över,
  behåller nu sin ruta ikryssad och låst, och håller du pekaren över den sägs det vad som håller den. Där
  ett tågavsnitt slutar någonstans tåget inte kan stanna sägs det rent ut, så att du kan flytta uppehållet
  eller tågavsnittet.

- **En tågkategori bär nu de förberedelse- och avslutstider som dess tåg planeras med,** så du behöver inte
  längre skriva samma två tal för varje tåg. Bredvid vart och ett av fälten finns en knapp *Tillämpa på
  nytt* som ger den tiden till alla tåg kategorin redan har och berättar hur många som ändrades; de två är
  skilda åtgärder, och att tillämpa på nytt flyttar bara minuterna allra ytterst på ett tåg.

- **Operatörerna är lättare att läsa på framsidan av ett tjänstehäfte.** Raden sätts nu i dubbel storlek, så
  att en logotyp är stor nog att kännas igen med en blick och en signatur stor nog att läsas tvärs över ett
  bord. Har alla operatörer en logotyp utelämnas ordet *Operatör*; saknar någon av dem logotyp anges alla
  med signatur, i fetstil och med etiketten kvar.

### Rättningar

- **Ett tjänstehäfte kunde skriva ut ett tågavsnitt utanför sidans nederkant.** Varje sida räknades med
  ungefär hälften mer utrymme än en A5-sida faktiskt har, och det som hamnar utanför sidkanten klipps bort
  utan förvarning, så det andra tågavsnittet på en sådan sida saknade slutet av sin tidtabell eller saknades
  helt. Tågavsnitt mäts nu mot vad sidan verkligen rymmer, så vissa häften behöver ett ark mer än förut.

- **Topologi-diagrammet kunde skriva signaturerna för två driftplatser ovanpå varandra.** Driftplatserna
  placerades enbart efter avståndet mellan dem, så två som ligger nära varandra på en lång sträcka ritades
  nästan på samma ställe. De ritas nu aldrig närmare varandra än vad deras signaturer behöver, och en lång
  signatur vid diagrammets kant klipps inte längre bort.

- **En gren i Topologi-diagrammet kunde ritas rakt genom en annan sträcka.** En gren faller bort i en fast
  vinkel, så en gren som mötte en sträcka i vägen ritades helt enkelt tvärs över den. De grenar som lämnar
  en sträcka längst bort ritas nu först, så en lång gren kan nu ritas under en kort gren som lämnar sträckan
  längre bort.

- **En plan kunde visa sina tåg under tågkategorier som fliken Tågkategorier inte hade.** Flera kategorier
  kunde också tas för en och samma, så att deras tåg samlades under en enda rubrik och två tåg av olika
  kategorier med samma nummer rapporterades som ett nummer använt två gånger. När en plan öppnas fylls
  listan över kategorier nu på med de kategorier som tågen använder, och varje kategori hålls isär från de
  andra.

- **Två företag som aldrig hade fått ett eget nummer togs för samma operatör,** så tåg från olika företag
  som delade tågnummer rapporterades som ett nummer använt två gånger. Varje företag får nu ett eget nummer
  när en plan öppnas eller sparas; ett företag från Module Registry behåller det nummer det kom med.

- **En plan lagrade sina tågkategorier, företag och länder på mer än ett ställe** — var och en skrevs där
  den först påträffades, oftast inne i det första tåg som använde den. Var och en skrivs nu en gång, i sin
  egen lista, och allt som använder den behåller bara en hänvisning; länder kopieras inte längre in i planen
  alls, så en rättelse av ett lands språk når nu även planer som sparats dessförinnan.

- **Ett tjänstehäfte angav bara tågnumret i rubriken för ett tågavsnitt.** Ett tåg identifieras lika mycket
  av kategorins prefix och suffix som av numret — Gt 1234, inte 1234 — och rubriken är allt en lokförare har
  att jämföra med tidtabellen. Den visar nu hela tågidentiteten, efter operatörens signatur.

## Version 0.3.3

### Ändringar

- **Konflikter går nu att läsa där de visas.** En rad med konflikter — ett tåg eller en tågkategori under
  **Tåg**, ett omlopp eller ett av dess fordon under **Omlopp**, en tjänst under **Tjänster** — har nu en
  varningssymbol, och ett klick på den öppnar meddelandena i en lista som går att läsa. Symbolen får sin
  färg av den allvarligaste konflikten och räknar dem; tidigare fanns de bara i en ruta som visades när
  muspekaren vilade på raden.
- **En tågkategori visar konflikterna för tågen i den**, så att de inte längre döljs när kategorin fälls
  ihop.
- **Fliken Tåg öppnas nu på listan över tågkategorier**, med tågen dolda tills du öppnar en kategori.
  *Expandera alla* öppnar alla på en gång, och en kategori öppnas av sig själv när du lägger till eller
  flyttar ett tåg dit.
- **Att redigera ett tågavsnitt i ett omlopp visar nu vilka slags fordon omloppet gäller** — lok, tågsätt
  eller vagnsätt. Varje slag nämns en gång, och pekar du på det visas fordonen själva.

### Rättningar

- **Appen kunde sluta spara ditt arbete utan att säga till.** En plan som appen inte kunde skriva ut — ett
  tåg med färre än två uppehåll, eller en tidtabellssträcka där alla bandelar tagits bort — fick sparandet
  att misslyckas tyst, så allt som gjordes därefter låg kvar på skärmen men sparades aldrig. Båda planerna
  går nu att spara, och ett misslyckat sparande sägs direkt i överraden.

- **En sparad planfil är omkring 40 % mindre.** Varje uppehåll skrevs två gånger — en gång i sitt tåg och en
  gång under spåret det ligger på — och den andra kopian drog med sig stora delar av resten av planen. En
  plan sparad med en tidigare version går fortfarande att öppna.

- **Ett tåg som lämnats utan dragkraft på en del av sitt lopp rapporteras nu.** Kontrollen frågade bara om
  ett lok eller tågsätt körde tåget *någonstans*, så när ett omlopp kortades av i ena änden blev resten av
  tåget utan dragkraft utan att något sades. Nu kontrolleras varje sträcka för varje köromgång tåget körs,
  och konflikten säger mellan vilka driftplatser och för vilka köromgångar; planer som såg rena ut kan
  rapportera detta nu.

## Version 0.3.2

### Ändringar

- Under **Godsflöde › Godsbeskrivningar** kan ett ursprung eller en destination nu vara vilken driftplats
  som helst som utväxlar gods, inte bara en station — ett industriområde hanterar alltid godsvagnar men gick
  tidigare inte att välja. Samma listor säger nu **driftplats** där de sa *station*.
- Ett tågs uppehåll listas alltid i den **ordning tåget går** genom dem.
- Att ändra en tid för ett uppehåll i fliken **Tåg** **tar nu med sig resten av tåget**: en **avgång**
  verkar framåt, åt det håll tåget går, och en **ankomst** bakåt, så att gången fram till ändringen följer
  med. Tiderna på andra sidan ligger kvar, gång- och uppehållstiderna behålls, och ändringen avvisas om den
  skulle föra tåget utanför planens drifttider.
- Ett tåg vars tågväg **hoppar över en driftplats** — två uppehåll i följd utan någon sträcka emellan —
  rapporteras nu som en konflikt. Den kan stängas av under **Inställningar › Validering**.
- Ett tågavsnitt i ett **omlopp** går nu att **redigera**: pennan öppnar dess från- och tilluppehåll, så ett
  omlopp kan formas om utan att allt efter det tas bort. Ett angränsande tågavsnitt som ansluter följer med;
  ett vars eget tåg inte gör uppehåll på den nya driftplatsen lämnas orört, och glappet rapporteras som en
  konflikt att lösa.
- **Lägg till tåg** kan nu skapa **returtåget** samtidigt. Kryssa i *Retur?*, så skapas tåget tillbaka
  tillsammans med det första, med samma sträcka i motsatt riktning, samma tågsort och hastighet och nästa
  nummer i motsatt riktning; avgången är antingen *så tidigt som möjligt* eller en tid du skriver in.
  Tillsammans med *Upprepa?* upprepas båda riktningarna.

### Rättningar

- **Kilometertalen** i den utskrivna tidtabellen och längs den grafiska tidtabellen avrundas nu till hela
  kilometer, och en bibana visar samma kilometertal som banan den utgår från vid förgreningsstationen.
- Allt som läser ett tågs tågväg följer nu **den ordning tåget kör sina uppehåll**, inte den ordning de
  matades in. För ett tåg vars uppehåll lagts in i fel ordning sicksackade **grafisk tidtabell**, kunde den
  utskrivna **tidtabellen** visa en avgång där tåget ankommer, kedjade **bygg automatiskt** inte tåget alls,
  mätte **upprepa tåg** intervallet från fel uppehåll, och att räkna om tiderna misslyckades helt.
  Importerade planer har aldrig berörts.
- **Tåghastigheten kontrolleras nu även på den sista sträckan**, in till den driftplats där tåget slutar
  sitt lopp.

## Version 0.3.1

### Ändringar

- Avsnittet **Dragfordon** på uppslaget för ett tågavsnitt i häftet Förartjänster har nu sin rubrik på det
  valda språket. Det var den enda rubriken i häftet som inte var översatt.
- Dragfordonet skrivs nu ut för varje tågavsnitt som har ett. I planer importerade med en tidigare version
  visade en del tågavsnitt ett dragfordon under **Tjänster** men inget i häftet.
- Anteckningar om tåg i samma riktning talar nu om vilket tåg som passerar det andra — **Förbigår GD 42757
  12:02-12:05** eller **Förbigås av GD 42757 12:02** — i stället för det tidigare *"Möter GD 42757 i samma
  riktning"*, som aldrig sa vilket tåg som kom före. Två tåg som bara står på samma station samtidigt ger
  ingen anteckning alls.
- Ett möte som inte varar någon tid — det andra tåget passerar utan uppehåll — skrivs som en enda tid i
  stället för ett intervall från en tid till sig själv.
- Ett tåg som börjar eller slutar sin gång på en station redovisas inte längre som mött, korsat eller
  förbigånget där. De tiderna är när dess lokförare anmäler sig eller avslutar tjänsten.

## Version 0.3.0

### Ändringar

- En ny rapport, **Förartjänster**, skriver ut ett A5-häfte per tjänst. Framsidan visar tjänstens nummer,
  vilka köromgångar eller dagar den körs, dess start- och sluttid och stationer, en svårighetsgrad,
  bemanningsbehov och eventuella tjänsteanteckningar; varje tågavsnitt får sedan sin egen sida med vilka
  dragfordon som ska användas, vilka vagnsätt som ska tas med, till vilka destinationer godsvagnar ska tas
  med, samt tidtabellen, var och en i sitt eget block.
- En ny rapport, **Allmänna instruktioner**, är ett separat häfte med träffens program och de instruktioner
  som gäller för banan under hela träffen — körinstruktioner, signalgivning, radio- och telefonanvändning,
  vad man gör vid förseningar och vem man frågar — och delas ut en gång till alla. Det inleds med träffens
  namn och datum, sedan programmet varje deltagare behöver veta före den första köromgången, sedan
  instruktionerna över så många sidor som de behöver, brutna mellan stycken och aldrig med en rubrik kvar
  ensam.
- Sista sidan i båda häftena visar banans spårplan och tabellen över rangerbangårdar, så att även de som
  aldrig håller i ett tjänstehäfte — framför allt stationspersonalen — får en överblick över banan.
- Både programmet och instruktionerna skrivs under **Inställningar › Information** och kan formateras med
  Markdown. Båda häftena skrivs ut i A5: A4 liggande, dubbelsidigt, vikt på mitten, med tomma sidor tillagda
  där det behövs så att arken viks rätt.
- Tjänster kan nu graderas **Lätt**, **Medel** eller **Van**, visat färgkodat på häftet, kan ange att de
  behöver två eller tre personer — till exempel en lokförare och en konduktör — och kan fästas med ett
  **fast nummer** som automatisk omnumrering lämnar orört.
- Planen kontrolleras nu även så att varje tågavsnitt med ett lok eller tågsätt tilldelat har en förartjänst
  som täcker det under varje köromgång det körs. En tjänst med fast nummer måste ha ett nummer, och inga två
  sådana får samma nummer.
- Företag kan nu ha en uppladdad **logotyp**, visad i rapporter i stället för textsignaturen.
- Stationer kan nu markeras som den **rangerbangård** som betjänar en annan orts lokalgods, och banan listar
  varje rangerbangård och vad den täcker på tjänstehäftets sista sida.
- Varje tidtabellssträcka kan nu ges en **färg**, som används för att rita den i Topologi-diagrammet.
- En ny **avståndsfaktor** (Inställningar › Tid & hastighet) låter en bana visa en större, mer förebildslik
  kilometersiffra i rapporter och den grafiska tidtabellen än det avstånd som faktiskt är modellerat, utan
  att påverka någon körtidsberäkning.
- Appen håller nu flera öppna webbläsarflikar eller -fönster synkroniserade med varandra. **Observera** att
  detta bara fungerar mellan fönster på samma dator i samma webbläsare.
- Inställningar kan nu spara träffens **gäller från**- och **gäller till**-datum, utskrivna som en
  giltighetsrad på rapporter; lämna dem tomma om ingen träff är bokad ännu.
- En ny inställning, **utöka plantider automatiskt?** (Inställningar › Allmänt), utvidgar planens start-
  eller sluttid för att täcka ett tåg i stället för att blockera ändringen. Avstängd som standard.
- En ny knapp, **uppdatera alla tider**, i den grafiska tidtabellen räknar om alla tåg i tidtabellen på en
  gång, i stället för att man först måste välja ut en delmängd.
- Spårbeläggningskontrollen kan nu valfritt ta hänsyn till ett lok eller tågsätt som står på ett spår mellan
  två tåg, såvida det inte är bokat till eller från uppställning (Inställningar › Validering). Avstängd som
  standard, eftersom det bara är meningsfullt på banor där uppställning modelleras avsiktligt.
- Varje uppehåll i fliken **Tåg** har nu ett fält för **Anmärkning** — en notering som skrivs ut vid det
  uppehållet, till exempel "vänta på mötande tåg". Anmärkningen visas färdigformaterad och byter till den
  råa märkningen så snart du går in i fältet, så skriv `*sakta*` för kursiv och `**första**` för fet stil.

### Rättningar

- Att lägga till ett nytt tåg sätter nu dess standardstarttid med hänsyn till den angivna
  förberedelsetiden, så att den inte börjar före planens starttid.

## Version 0.2.4

### Ändringar

- En ny flik **Tjänster** låter dig planera förartjänster — det arbete en lokförare utför under en
  köromgång, som en följd av de tågavsnitt hen kör. Varje tjänst är en rad: dess beteckning, företag och
  köromgångar till vänster, tågavsnitten i körordning till höger.
- Lägg till de tågavsnitt en förare kör med **Lägg till tågavsnitt**. Listan visar de dragfordonssträckor en
  förare kan ta härnäst — de som inte krockar i tid med tjänsten och, när den har ett tågavsnitt, de som
  avgår vid eller efter att det ankommer. Tågavsnitten behöver inte börja på samma station: föraren går helt
  enkelt dit nästa börjar.
- Samma tågavsnitt kan köras av flera tjänster så länge de går på olika köromgångar, så en tjänst kan täcka
  de udda köromgångarna och en annan de jämna.
- Där två tågavsnitt för samma tåg i en tjänst körs av olika dragfordon visar fliken en anteckning vid
  stationen där dragfordonet byts — du behöver inte skriva den för hand.
- Tjänster som importeras från XPLN delar nu de tågavsnitt som är definierade i fordonens köromgångar, så
  varje tågavsnitt visar det dragfordon som kör det.
- Planen kontrolleras så att inget tågavsnitt körs av två tjänster under samma köromgång och ingen tjänst
  har tågavsnitt som överlappar i tid. Kontrollen kan stängas av under **Inställningar › Validering**.

## Version 0.2.2

### Rättningar

- Två tåg som aldrig går under samma köromgång rapporteras inte längre som ett möte på en enkelspårig
  sträcka. Ett tåg som går köromgång 1, 3, 5 och ett som går 2, 4, 6 är aldrig ute samtidigt.
- Konfliktkontrollen på dubbelspåriga och flerspåriga sträckor är nu exakt: en sträcka flaggas endast när
  fler tåg befinner sig på den samtidigt än den har spår, och endast tåg som går under en gemensam köromgång
  räknas.

## Version 0.2.1

### Ändringar

- Konfliktvarningar visas nu där du kan åtgärda dem: tågkonflikter i den grafiska tidtabellen och på fliken
  **Tåg**, fordons- och omloppskonflikter på fliken **Omlopp**.
- På fliken **Omlopp** markerar en fordonskonflikt nu bara det berörda fordonet, och en omloppskonflikt bara
  det omloppet.
- Kontrollen att ett fordon återvänder till sin utgångspunkt omfattar nu även vagngrupper och gods, inte
  bara lok och tågsätt.

## Version 0.2.0

### Ändringar

- Namnet på den plan du arbetar med visas nu överst i fönstret.
- Den grafiska tidtabellen visar nu staplar över lokförarbehovet, vilket gör det lättare att se hur många
  förare som behövs under köromgången.
- En ny **Topologi**-vy (under fliken **Sträckor**) visar ett schematiskt diagram över tidtabellens sträckor
  och deras grenar.

### Rättningar

- Sträckor behåller nu som standard den ordning du angav dem i. Du kan fortfarande sortera på valfri kolumn.
- Konflikter hänvisar inte längre till tåg som du inte kan hitta: när ett tåg tas bort tas dess
  stationsuppehåll bort tillsammans med det, så inga överblivna uppehåll eller falska konflikter blir kvar.

## Version 0.1.0

Första förhandsversionen av Tidtabellplaneraren. Du kan:

- Definiera spårplaner med stationer, spår och sträckor.
- Skapa och redigera tågtidtabeller med automatisk tidsberäkning.
- Tilldela lokomotiv och tågsätt till tåg.
- Bygga fordonsomlopp och skriva ut omloppskort.
- Planera godsflöden mellan stationer.
- Visa grafiska tidtabeller (tid-avståndsdiagram).
- Validera tidtabeller för konflikter och inkonsekvenser.
- Generera utskrifter: tågkort, stationsböcker och tjänstgöringslistor.
- Arbeta på engelska, tyska, danska, norska och svenska.
