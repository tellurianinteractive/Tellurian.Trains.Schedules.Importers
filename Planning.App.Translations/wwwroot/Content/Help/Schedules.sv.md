På fliken **Omlopp** gör du om en tidtabell till de fordons- och personalplaner som behövs för att köra
köromgången.

Här bygger du **fordonsomlopp** och tilldelar dem lok, vagnsätt och godsflöden,
så att varje tåg får den utrustning det behöver och varje fordon får ett sammanhängande, vettigt
arbete under dagen. Förartjänster knyter ihop detta till arbete som en person kan utföra.

### Omlopp

Varje omlopp är en rad: den första kolumnen listar de **fordon** som kör det, följt av
**tågavsnitten** i arbetsordning. Ett vagnsätt som listar sina vagnar visar hur många de är och varje
vagnsklass en gång efter sitt nummer, som i *SJ 05 5 x A/B/Fv*.

Använd **Bygg automatiskt** för att kedja ihop tåg i samma kategori
som fortsätter där det föregående anlände (markera en kategori *Uteslut från automatisk schemaläggning* på fliken **Tågkategorier**
för att hålla den utanför), eller **Nytt omlopp** för att bygga ett
för hand. Automatisk byggning erbjuder först de ännu inte inplanerade tågen till de omlopp som redan finns: ett omlopp
fortsätter med det som fortsätter det i samma kategori, och ett tomt omlopp
som du har gjort fylls innan något nytt skapas, så att ett fordon som redan vänder tar sig an
mer arbete innan ett annat sätts in. Bara de tåg som inte passar något befintligt omlopp startar nya,
och godsflöden lämnas som de är.
Ett omlopp som körs av ett tågsätt, eller av ett lok som är ikryssat **Vändtåg?**, får sina ankomster
flyttade till det spår dess nästa tåg avgår från, eftersom ett sådant tåg lämnar på det spår det kom in
på. Där arbetet slutar där det började ankommer dess sista tåg till det spår det första
avgår från. Det sker när tågsättet eller loket tilldelas, när rutan kryssas i och när tåg
läggs till. Ett omlopp med ett vanligt lok, eller utan något fordon ännu, behåller sina spår: dess nästa tåg
kan mycket väl använda andra vagnar, vilket ofta avgörs senare i planeringen.

Knappen **ankomstspår** på en rad flyttar det omloppets ankomster när som helst, oavsett vilket
fordon som kör det, till exempel när du vet att ett lok behåller sina vagnar.

På en rad:
- **+ tåg** lägger till nästa tåg – välj bara en del av det (ett från-/till-uppehåll)
  när ett tåg måste delas mellan fordon, till exempel vid byte från eldrift till diesel-
  drift.
- De små **lederna** mellan tågavsnitten anger var fordonet står och hur länge, och före det första avsnittet var det
  måste hämtas ifrån. Klicka på en för att lägga in ett tåg i luckan: bara
  de tåg fordonet faktiskt hinner med på den tid som finns erbjuds. En etapp som inte
  för fordonet tillbaka dit arbetet fortsätter läggs ändå till och rapporteras som en konflikt
  tills du lägger till etappen tillbaka – så läggs en tur och retur in i ett uppehåll, en etapp i
  taget. En led som arbetet är avbrutet vid markeras med bärnsten, och ett klick på den erbjuder de tåg
  som överbryggar den.
- **Pennan** på ett tågavsnitt ändrar hur stor del av sitt tåg omloppet kör: välj ett nytt från- eller
  till-uppehåll. Själva tåget finns kvar; för att köra ett annat tåg tar du bort avsnittet och lägger till det andra.
  Det angränsande avsnitt som ansluter till det du ändrar följer med, så att arbetet förblir helt –
  förkorta ett avsnitt från A–C till A–B och returturen blir B–A av sig själv. När grannens
  eget tåg inte har uppehåll vid det nya uppehållet lämnas det som det är, och luckan rapporteras som en konflikt
  som du får lösa.
  Samma dialogruta anger var fordonen står när det inte är det spår tåget använder: **Hämta från**
  anger det spår de hämtas från innan tåget avgår, och **Ställ på** det spår de ställs
  på efter att det anlänt. Vart och ett skrivs ut som en anmärkning för lokföraren och tågklareraren, i stället för
  den vanliga anmärkningen om att koppla på eller av fordonet. Ett vagnsätts spår finns redan i
  lokförarens vagnsättstabell, så lokförarens anmärkning säger bara att vagnarna ska växlas till avgångsspåret eller
  till sitt ankomstspår. Där ett fordon kör tåget bara under några av de köromgångar eller dagar tåget
  går, börjar dess anmärkning med dem. Ett spår som valts vid en station där avsnittet inte
  längre börjar eller slutar glöms.
- **+ fordon** tilldelar ett fordon och skapar ett nytt vid behov; ett omlopp kan ha
  flera fordon (till exempel ett lok och dess vagnsätt). Godsflöden visas som omloppskort
  i rapporterna i stället för här. Ett omlopp som bara innehåller **växlingsuppgifter** behöver inget fordon
  alls: en sådan uppgift utförs av det som står på stationen, så att lämna den utan fordon är komplett
  snarare än ofullständigt, och den rapporteras inte som ett omlopp som lämnats utan fordon. Lägg till ett resande
  tåg i samma omlopp, så behövs ett fordon igen.

När du redigerar ett lok erbjuds **Vändtåg?**. Kryssa i den där loket kör ett tåg som kan
köras från vilken ände som helst – ett med manövervagn i den bortre änden, eller med ett andra lok
där. Där ett sådant tågs sträckning vänder byter det riktning genom att byta hytt, så **Uppdatera tider**
ger det inte längre tid att köra loket runt, och uppehållet krymper till kortaste uppehåll.
Ett tågsätt behandlas redan så och erbjuds ingen ruta.

#### Trafikerade köromgångar eller dagar

Ett omlopps trafikerade köromgångar blir den delmängd av köromgångar som alla tåg trafikerar. Med ett omlopp där de flesta tåg
går dagligen och ett par bara köromgång 1–5 blir hela omloppet 1–5.

#### Validering

Valideringsreglerna under **Inställningar › Validering** (loktäckning, förartjänster) kontrollerar
att de omlopp du bygger här faktiskt täcker tågen i tidtabellen.
