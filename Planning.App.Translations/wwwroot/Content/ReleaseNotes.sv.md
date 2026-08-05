# Versionsnyheter

## Version 0.4.0

### Brytande ändringar

- **Ett fordon du skapar identifieras nu av sin operatör och sitt nummer.** De två tillsammans pekar ut
  ett enda verkligt fordon, så under en och samma köromgång får kombinationen tillhöra bara ett fordon
  — vilken sorts fordon det än är. En vagnsats och ett lok kan inte längre båda vara *DB 5*. Ett fordon
  utan operatör identifieras av numret ensamt. Två fordon får fortfarande ha samma operatör och nummer
  så länge de köromgångar de går inte överlappar, eftersom de då aldrig är på träffen samtidigt.

  Ett **importerat** fordon identifieras även fortsättningsvis av det externa id det importerades med,
  vilket redan är unikt i planen det kom från, så en importerad plan ger inga nya konflikter av detta.

  Att lägga till eller ändra ett fordon under fliken Omlopp avvisar nu en identitet som ett annat fordon
  redan har, och ett nummer måste anges. Planer gjorda före den här regeln behålls precis som de är —
  ingenting numreras om åt dig — och varje fordon som delar identitet listas bland konflikterna, en
  gång vardera, så att du ser vad som behöver ett nytt nummer.

### Ändringar

- **Det finns en ny rapport: tågklareringslistan.** Ett eget häfte per station som är bemannad — alla
  bemannade stationer, och alla skuggstationer oavsett om de är bemannade — med de tåg stationen
  hanterar i tidsordning. Ett tåg som står på stationen förekommer två gånger, en gång för ankomsten
  och en gång för avgången, eftersom att klarera in ett tåg och att klarera ut det mot nästa station är
  två skilda handlingar som görs med några minuters mellanrum. Ankomster har vit bakgrund och avgångar
  ljusgul, så att de aldrig kan förväxlas. Tåg som bara passerar tas också med, eftersom även de måste
  klareras förbi. Varje sida har stationens namn, den del av dygnet sidan täcker och telefonnumren till
  stationerna i andra änden av tågklareringssträckorna, och varje rad har en ruta per köromgång att
  pricka av allteftersom, gråmarkerad för de köromgångar tåget inte går. Varje station börjar på ny
  sida, så bunten kan helt enkelt delas och lämnas ut. Skrivs ut från menyn Rapporter.

- **Fälten för att lägga till och ändra ett fordon har fått ny ordning,** densamma på båda ställena:
  typ av fordon, typ av dragkraft, antal enheter, operatör, nummer, klass, köromgångar och sist det
  externa id:t — vad fordonet är, sedan vad som identifierar det, sedan hur det beskrivs och när det
  går. Fältet som tidigare hette *Företag* heter nu *Operatör*.

- **Ett externt id kan rättas men inte längre hittas på.** Det externa id:t är det namn ett tåg eller ett
  fordon bär i systemet det importerades från, så det betyder något bara där det kommer från något. Det
  som importerats med ett id har kvar sitt fält — under fliken Tåg, och i fordonsdialogen under fliken
  Omlopp — och kan rättas där; det som aldrig haft något id har nu ingen ruta att skriva i. Ett fordon
  du skapar i planeraren får därför inget externt id alls, där det tidigare fick ett påhittat av klass
  och nummer.

- **Minsta tiden mellan två användningar av samma spår kontrolleras nu.** Inställningen fanns, men
  ingenting använde den. Lämnad på 0 — där den börjar, och där den stannar tills du ändrar den — ändras
  ingenting i kontrollen: två tåg är i konflikt där de står på samma spår samtidigt, och ett som kommer
  just när ett annat går är en avlösning, inte en konflikt. Sätt den till exempelvis 5 och spåret måste
  dessutom vara ledigt i fem minuter mellan dem, så att en plan som vänder spåret snabbare än
  driftplatsen hinner med blir rapporterad. Exakt fem lediga minuter räcker; fyra gör det inte.

  En sådan konflikt anger hur kort mellanrummet faktiskt är och hur långt det måste vara, i stället för
  att påstå att de två tågen överlappar när tiderna visar att de inte gör det.

- **En driftplats kan nu ha egna instruktioner.** Formuläret för att lägga till och ändra en driftplats
  har ett fält **Instruktioner**, skrivet i Markdown och visat bredvid en förhandsvisning, precis som de
  allmänna instruktionerna i Inställningar. Det är till för hur just den driftplatsen körs på den här
  träffen — vilka spår som används till vad, hur växlingen är upplagd och vad lokförarna och de som
  bemannar platsen annars behöver veta där. Hur driftplatsen körs i allmänhet, och annan beskrivning av
  den, är ägarens sak att tillhandahålla och hör inte hemma i fältet. Det du skriver sparas med
  driftplatsen och visas i dess Info-vy.

  Fältet erbjuds på en station eller ett industriområde, där resande och/eller gods utväxlas. Det
  erbjuds inte där det inte finns något att instruera om: tågen passerar bara en signalreglerad plats,
  och ingen bemannar en annan plats, så tåget gör där vad uppehållet säger och inget mer.

- **En plats där gods hanteras utan bemanning kan nu kräva en nyckel.** Där växlarna på en obemannad
  station eller ett industriområde är låsta kan du i ändringsformuläret välja den bemannade station som
  förvarar nyckeln, under **Låsnyckel förvaras vid**, och namnge nyckeln om stationen förvarar flera.

  Mer än så behöver inte planeras. Ett godståg som stannar på stationen med nyckeln och senare stannar
  på platsen som nyckeln låser upp får vid avgången därifrån beskedet *hämta nyckel A1 för att låsa upp
  Bruket*; nästa gång tåget stannar där säger ankomsten *lämna nyckel A1 från Bruket*. Ett tåg som bara
  passerar någon av platserna får inget besked, eftersom det inte låser upp något. Nyckeln hämtas vid
  det sista uppehållet på stationen före arbetet och lämnas tillbaka vid det första efter det, så ett
  tåg som stannar där två gånger slipper bära med sig den ett extra varv.

  En nyckel betyder något bara så länge båda ändarna håller. Markera platsen själv som bemannad, eller
  ta bort bemanningen från stationen som förvarar nyckeln, så slutar nyckeln gälla: inga besked skapas
  av den, och **Konflikter** talar om vilken av de två ändringarna som gjorde det. Nyckeln behålls i
  stället för att kastas, så om du ångrar ändringen gäller den direkt igen, och den ligger kvar i
  formuläret där du kan peka den mot en annan station eller ta bort den.

### Rättningar

- **Två sträckor som utgår från samma driftplats ritades som om de aldrig möttes.** Började en
  tidtabellssträcka på just den första driftplatsen på en annan, förband ingenting de två i
  Topologi-diagrammet: var och en ritades som en egen linje, utan gren mellan dem. Den andra lämnar nu
  den driftplatsen som vilken gren som helst och faller bort från den i samma fasta vinkel.

- **Varje gränsvärde för kontrollerna anger nu vilken klocka det mäts mot.** Minsta tiden mellan två
  användningar av samma spår saknade helt enhet, och de två tåghastigheterna angav bara *klockminuter*,
  vilket kunde läsas på båda sätten. Alla tre anger nu snabbklocksminuter — den klocka tågen går efter,
  inte verklig tid.

- **Längder och distanser skrivs nu ut i meter,** liksom täljaren i tåghastigheterna, så att *m* inte
  kan tas för en minut. Minsta uppehåll vid en station anges nu också i snabbklocksminuter.

## Version 0.3.5

### Rättningar

- **En sparad plan kunde vägra att öppnas.** Att öppna en plan som appen just hade sparat avbröts med
  ett felmeddelande om ett land, och ingenting lästes in — det fanns ingen väg förbi. En fil läses en
  bit i taget medan den kommer in, och läsningen av länderna i den snubblade på det. En redan sparad
  plan öppnas som den är; du behöver inte göra något med den.

- **En sparad planfil är omkring sju gånger mindre.** Att spara en plan till fil skrev den i en annan
  form än den som hålls i webbläsaren, så vinsterna från de två senaste versionerna nådde aldrig fram
  till filen: varje uppehåll skrevs två gånger, och varje tågkategori, operatör och land om igen vid
  varje tåg, fordon och förartur som använde det. En fil som tog 8 MB tar nu drygt 1 MB, och sparas
  och öppnas i motsvarande grad snabbare. En plan sparad av en tidigare version går fortfarande att
  öppna.

## Version 0.3.4

- **Rutorna Ank och Avg på ett uppehåll följer nu var tåget verkligen kan stanna.** Ett tåg stannar
  för att utväxla något, och behöver därför någonstans att utväxla det: ett persontåg där driftplatsen
  tar emot resande, ett godståg där den tar emot gods, och inget av det på en signalreglerad
  driftplats. Där tåget inte kan stanna visas båda rutorna tomma och går inte att kryssa i, och
  uppehållet blir en genomfart i tidtabellen och i grafen. Inget av det du planerat kastas bort — slå
  på utbytet igen så finns uppehållen där. Ett magasin har alltid utbyte av både resande och gods,
  eftersom det representerar allt utanför banan, så dess två rutor visas ikryssade och låsta.

- **Ett uppehåll som något hänger på går inte längre att ta bort.** En tågdel går från ett uppehåll
  där tåget avgår till ett där det ankommer, så båda ändarna måste vara uppehåll. Tågets eget första
  och sista uppehåll, och ändarna på varje tågdel som ett fordonsomlopp, en förartur eller ett
  godsflöde planerats över, behåller nu sin ruta ikryssad och låst; håll pekaren över den så sägs det
  vad som håller den. Där en tågdel slutar någonstans tåget inte kan stanna — en plan gjord före den
  här regeln — sägs det rent ut, så att du kan flytta uppehållet eller tågdelen.

- **En tågkategori bär nu de förberedelse- och avslutstider som dess tåg planeras med.** Varje nytt
  tåg i kategorin görs klart så många minuter innan det avgår och avvecklas så många minuter efter att
  det ankommit, så du behöver inte längre skriva samma två tal för varje tåg. Bredvid vart och ett av
  de två fälten finns en knapp *Tillämpa på nytt* som ger den tiden till alla tåg kategorin redan har,
  och berättar hur många som ändrades. De två är skilda åtgärder, så du kan ändra förberedelsetiden
  utan att röra avslutstiden. Att tillämpa på nytt flyttar bara minuterna allra ytterst på ett tåg:
  det avgår, uppehåller sig och ankommer fortfarande precis på de tider det gjorde.

- **Operatörerna är lättare att läsa på framsidan av ett tjänstehäfte.** Raden sätts nu i dubbel
  storlek mot förut, så att en logotyp är stor nog att kännas igen med en blick och en signatur stor
  nog att läsas tvärs över ett bord. Har alla operatörer i tjänsten en logotyp utelämnas ordet
  *Operatör* — logotyperna säger det själva. Saknar någon av dem logotyp anges alla fortfarande med
  signatur, i fetstil och med etiketten kvar.

### Rättningar

- **Ett tjänstehäfte kunde skriva ut en tågdel utanför sidans nederkant.** Rapporten räknar före
  utskriften ut hur många tågdelar som får plats på en sida, och räknade med ungefär hälften mer
  utrymme än en A5-sida faktiskt har. Det som hamnar utanför sidkanten klipps bort utan förvarning:
  den andra tågdelen på en sådan sida saknade slutet av sin tidtabell — eller saknades helt, så att en
  lokförare stod med en tjänst där det sista tåget fattades. Tågdelar mäts nu mot vad sidan verkligen
  rymmer, och en tågdel som inte får plats flyttas till nästa sida. Vissa häften behöver därför ett ark
  mer än förut.

- **Topologi-diagrammet kunde skriva signaturerna för två driftplatser ovanpå varandra.** Driftplatserna
  placerades enbart efter avståndet mellan dem, så två som ligger nära varandra på en lång sträcka
  ritades nästan på samma ställe och deras signaturer gick in i varandra. De ritas nu aldrig närmare
  varandra än vad deras två signaturer behöver, medan resten av sträckan behåller sina verkliga
  proportioner. En lång signatur vid diagrammets kant klipps inte längre bort heller.

- **En gren i Topologi-diagrammet kunde ritas rakt genom en annan sträcka.** En gren faller bort från
  den sträcka den lämnar i en fast vinkel, så en gren som mötte en sträcka i vägen kunde aldrig ta sig
  förbi den, hur långt ner i diagrammet den än sköts — den ritades helt enkelt tvärs över den. De grenar
  som lämnar en sträcka längst bort ritas nu först, vilket ger dem bakom en fri väg nedåt. En lång gren
  kan därför nu ritas under en kort gren som lämnar sträckan längre bort.

- **En plan kunde visa sina tåg under tågkategorier som fliken Tågkategorier inte hade.** Ett tåg bär
  med sig sin kategori, så en plan sparad av en tidigare version öppnades med tågen grupperade efter
  kategori medan listan över kategorier var tom: kategorimenyn hade ingenting att erbjuda, och inget
  tåg kunde flyttas till en annan kategori. Flera kategorier kunde också tas för en och samma, så att
  deras tåg samlades under en enda rubrik och två tåg av olika kategorier med samma nummer
  rapporterades som ett nummer använt två gånger. När en plan öppnas fylls listan över kategorier nu på
  med de kategorier som tågen använder, och varje kategori hålls isär från de andra.

- **Två företag som aldrig hade fått ett eget nummer togs för samma operatör.** Ett företag skiljs från
  de andra på ett nummer som appen håller för det, och en plan kunde innehålla flera som aldrig hade
  fått något. Tåg från olika företag som delade tågnummer rapporterades då som ett nummer använt två
  gånger. Varje företag får nu ett eget nummer när en plan öppnas eller sparas; ett företag som kommer
  från Module Registry behåller det nummer det kom med.

- **En plan lagrade sina tågkategorier, företag och länder på mer än ett ställe.** Var och en skrevs
  där den först påträffades vid sparandet — oftast inne i det första tåg som använde den — medan listan
  den hör hemma i inte innehöll mer än en hänvisning till den. Det är så en plan kunde få tåg i
  kategorier som fliken Tågkategorier inte kände till. Var och en skrivs nu en gång, i sin egen lista,
  och allt som använder den behåller bara en hänvisning. Länder kopieras inte längre in i planen alls,
  så en rättelse av ett lands språk når nu även planer som sparats dessförinnan. En plan sparad av en
  tidigare version läses som förut och rättas nästa gång den sparas.

- **Ett tjänstehäfte angav bara tågnumret i rubriken för en tågdel.** Ett tåg identifieras lika mycket
  av kategorins prefix och suffix som av numret — Gt 1234, inte 1234 — och en lokförare som jämför
  häftet med tidtabellen, eller med det som ropas ut, har bara den rubriken att gå efter. Rubriken
  visar nu hela tågidentiteten, prefix och suffix inräknade, efter operatörens signatur.

## Version 0.3.3

- **Konflikter går nu att läsa där de visas.** En rad med konflikter — ett tåg eller en tågkategori
  under **Tåg**, ett omlopp eller ett av dess fordon under **Omlopp**, en tjänst under **Tjänster** —
  har nu en varningssymbol, och ett klick på den öppnar meddelandena i en lista som går att läsa.
  Symbolen får sin färg av den allvarligaste konflikten och räknar dem när de är fler än en. Tidigare
  fanns meddelandena bara i en ruta som visades när muspekaren vilade på raden — lätt att missa och
  svår att läsa.
- **En tågkategori visar konflikterna för tågen i den**, så att de inte längre döljs när kategorin
  fälls ihop.
- **Fliken Tåg öppnas nu på listan över tågkategorier**, med tågen i varje kategori dolda tills du
  öppnar den, så att en plan med många tåg blir lättare att överblicka. *Expandera alla* öppnar alla
  på en gång, och en kategori öppnas av sig själv när du lägger till ett tåg i den eller flyttar ett
  tåg dit.
- **Att redigera en tågdel i ett omlopp visar nu vilka slags fordon omloppet gäller** — lok, tågsätt
  eller vagnsätt. Delar flera fordon på samma omlopp nämns varje slag en gång, och pekar du på det
  visas fordonen själva.

### Rättningar

- **Appen kunde sluta spara ditt arbete utan att säga till.** Planen sparas i webbläsaren medan du
  arbetar, och en plan som appen inte kunde skriva ut — ett tåg med färre än två uppehåll, eller en
  sträckning under **Sträckor › Tidtabellssträckor** där alla bandelar tagits bort — fick det
  sparandet att misslyckas tyst. Allt som gjordes därefter låg kvar på skärmen men sparades aldrig, så
  när webbläsaren öppnades igen låg planen kvar som före: med driftplatserna men utan de sträckor och
  tåg som lagts till sedan dess. Båda planerna går nu att spara, och om ett sparande ändå misslyckas
  säger överraden det direkt, så att du kan ångra ändringen i stället för att förlora arbetet.

- **En sparad planfil är omkring 40 % mindre.** Varje uppehåll skrevs två gånger — en gång i sitt tåg och
  en gång under spåret det ligger på — och den andra kopian drog med sig stora delar av resten av planen.
  En plan sparad med en tidigare version går fortfarande att öppna.

- **Ett tåg som lämnats utan dragkraft på en del av sitt lopp rapporteras nu.** Kontrollen frågade
  bara om ett lok eller tågsätt körde tåget *någonstans*, så när ett omlopp kortades av i ena änden
  blev resten av tåget utan dragkraft utan att något sades. Nu kontrolleras varje sträcka tåget kör,
  för varje köromgång det körs, och konflikten säger mellan vilka driftplatser och för vilka
  köromgångar tåget saknar dragkraft. Planer som såg rena ut kan rapportera detta nu — luckan har
  alltid funnits där.

## Version 0.3.2

- Under **Godsflöde › Godsbeskrivningar** kan ett ursprung eller en destination nu vara vilken
  driftplats som helst som utväxlar gods, inte bara en station. Ett industriområde hanterar
  alltid godsvagnar men gick tidigare inte att välja, så gods till och från en industri fick
  beskrivas som om det gick till närmaste station.
- Samma listor säger nu **driftplats** där de sa *station*, eftersom de inte längre bara
  innehåller stationer.
- Att ändra en tid för ett uppehåll i fliken **Tåg** **tar nu med sig resten av tåget**. En **avgång**
  verkar framåt, åt det håll tåget går: låt ett tåg stå fem minuter längre vid en driftplats, och det
  kommer fram fem minuter senare till alla senare driftplatser. En **ankomst** verkar bakåt: begär att
  tåget ska ankomma fem minuter senare, så avgår det fem minuter senare från alla tidigare driftplatser,
  så att gången fram till ändringen följer med. Tiderna på andra sidan ligger kvar, gång- och
  uppehållstiderna behålls, och ändringen avvisas — och fältet återgår — om den skulle föra tåget utanför
  planens drifttider.
- Ett tågs uppehåll listas alltid i den **ordning tåget går** genom dem.
- Ett tåg vars tågväg **hoppar över en driftplats** — två uppehåll i följd utan någon sträcka emellan —
  rapporteras nu som en konflikt. Den kan stängas av under **Inställningar › Validering**.
- **Tåghastigheten kontrolleras nu även på den sista sträckan**, in till den driftplats där tåget slutar
  sitt lopp. Den sträckan hoppades tidigare över.

- En tågdel i ett **omlopp** går nu att **redigera**: pennan på en tågdel öppnar dess från- och
  tilluppehåll, så ett omlopp kan formas om utan att allt efter det tas bort. En angränsande tågdel
  som ansluter till den du ändrar följer med — korta av en del från A–C till A–B, så blir returen
  B–A av sig själv. En angränsande del vars eget tåg inte gör uppehåll på den nya driftplatsen
  lämnas orörd, och glappet rapporteras som en konflikt att lösa.

- Allt som läser ett tågs tågväg följer nu **den ordning tåget kör sina uppehåll**, inte den ordning de
  matades in. För ett tåg vars uppehåll lagts in i fel ordning — ett uppehåll tillagt efter ett som
  tåget kommer till först senare — sicksackade **grafisk tidtabell** mellan uppehåll som tåget aldrig
  kör mellan och kunde placera tåget i fel riktnings kolumn; den utskrivna **tidtabellen** kunde visa
  en avgång där tåget ankommer; **bygg automatiskt** kedjade inte tåget alls, eftersom det såg ut att
  starta där det inte startar; **upprepa tåg** mätte intervallet från fel uppehåll; och att räkna om
  tiderna efter en ändrad uppehållsbild misslyckades helt. Att välja en del av ett tåg vid tillägg
  till ett omlopp visar också uppehållen i körordning. Importerade planer har aldrig berörts — där är
  de båda ordningarna desamma.

- **Lägg till tåg** kan nu skapa **returtåget** samtidigt. Kryssa i *Retur?* så skapas tåget tillbaka
  från destinationen tillsammans med det första, med samma sträcka i motsatt riktning, samma tågsort
  och hastighet, och nästa nummer i motsatt riktning. Avgången är antingen *så tidigt som möjligt* —
  det första tågets ankomst plus efterarbets- och förberedelsetiden — eller en tid du skriver in, som
  får ligga både före och efter det första tågets avgång. Tillsammans med *Upprepa?* upprepas båda
  riktningarna, så en hel trafik i båda riktningarna planeras på en gång.

### Rättningar

- **Kilometertalen** i den utskrivna tidtabellen och längs den grafiska tidtabellen avrundas nu till
  hela kilometer. De skrevs ut med en decimal, och avståndsfaktorn under **Inställningar › Tid &
  hastighet** kunde göra en sträckas längd till en udda del av en kilometer. En bibana visar nu också
  samma kilometertal som banan den utgår från vid förgreningsstationen.

## Version 0.3.1

- Avsnittet **Dragfordon** på ett tågdelsuppslag i häftet Förartjänster har nu sin rubrik på
  det valda språket. Det var den enda rubriken i häftet som inte var översatt, så avsnittet
  gick inte att känna igen som dragfordonen.
- Dragfordonet skrivs nu ut för varje tågdel som har ett. I planer som importerats med en
  tidigare version visade en del tågdelar ett dragfordon under **Tjänster** men inget i häftet.
- Anteckningar om tåg i samma riktning talar nu om vilket tåg som passerar det andra —
  **Förbigår GD 42757 12:02-12:05** eller **Förbigås av GD 42757 12:02** — i stället för det
  tidigare *"Möter GD 42757 i samma riktning"*, som aldrig sa vilket tåg som kom före. Två tåg som
  bara står på samma station samtidigt ger ingen anteckning alls, eftersom inget av dem har
  passerat det andra.
- Ett möte som inte varar någon tid — det andra tåget passerar utan uppehåll — skrivs som en enda
  tid i stället för ett intervall från en tid till sig själv.
- Ett tåg som börjar eller slutar sin gång på en station redovisas inte längre som mött, korsat
  eller förbigånget där. De tiderna är när dess lokförare anmäler sig eller avslutar tjänsten, inte
  när tåget är i gång.

## Version 0.3.0

- En ny rapport, **Förartjänster**, skriver ut ett A5-häfte per tjänst. Framsidan visar
  tjänstens nummer, vilka köromgångar eller dagar den körs, dess start- och sluttid och
  stationer, en svårighetsgrad, bemanningsbehov och eventuella tjänsteanteckningar.
  Varje tågdel får sin egen sida, med vilka dragfordon som ska användas, vilka vagnsätt
  som ska tas med och till vilka destinationer godsvagnar ska tas med, samt tidtabellen
  – var och en visad i sitt eget tydligt avgränsade block. Häftets sista sida visar
  banans spårplan och en tabell över rangerbangårdar, för enkel referens under
  körningen.
- En ny rapport, **Allmänna instruktioner**, är ett separat utskrivet häfte med träffens
  program och instruktioner som gäller för en bana under hela träffen. Här är
  träffarrangören fri att skriva vad som helst – till exempel körinstruktioner,
  signalgivning, radio-/telefonanvändning, vad man gör vid förseningar och vem man
  frågar – och delas ut en gång till alla.
- Både programmet och instruktionerna skrivs under **Inställningar › Information** och
  kan formateras med Markdown – rubriker, listor, fet och kursiv stil – så att även en
  lång instruktionstext blir läsbar i utskrift.
- Häftet inleds med träffens namn, vilka datum den gäller och utskriftsdatum, följt av
  programmet: köromgångarnas tider, raster och måltider – det varje deltagare behöver
  veta före den första köromgången.
- Instruktionerna följer sedan över så många sidor som de behöver. Sidbrytningar sker
  mellan stycken, och en rubrik hålls alltid ihop med texten den inleder.
- Sista sidan visar banans spårplan och tabellen över rangerbangårdar, så att även de
  som aldrig håller i ett tjänstehäfte – framför allt stationspersonalen – får en
  överblick över banan.
- Häftet skrivs ut i samma A5-format som tjänstehäftena: A4 liggande, dubbelsidigt,
  vikt på mitten, med tomma sidor tillagda där det behövs så att arken viks rätt.
- Tjänster kan nu graderas **Lätt**, **Medel** eller **Van**, visat färgkodat på
  häftet, så att en deltagare kan välja en tjänst som matchar sin erfarenhet.
- En tjänst kan nu ange att den behöver två eller tre personer – till exempel en
  lokförare och en konduktör – och detta visas på häftet.
- En tjänst kan fästas med ett **fast nummer** så att automatisk omnumrering lämnar
  den orörd, till exempel särskilda tjänster som delas ut innan en köromgång börjar.
- Planen kontrolleras nu även så att varje tågdel med ett lok eller tågsätt tilldelat
  har en förartjänst som täcker den under varje köromgång den körs – en del som ingen
  är schemalagd att köra rapporteras, köromgång för köromgång. En tjänst med fast
  nummer kontrolleras också: den måste ha ett nummer, och inga två tjänster med fast
  nummer kan ges samma nummer.
- Företag kan nu ha en uppladdad **logotyp**, visad i rapporter i stället för
  textsignaturen.
- Stationer kan nu markeras som den **rangerbangård** som betjänar en annan orts
  lokalgods; banan listar automatiskt varje rangerbangård och vad den täcker, visat på
  tjänstehäftets sista sida. Detta hjälper stationspersonal och godstågförare att veta
  vart vagnar med en viss godsdestination ska skickas.
- Varje tidtabellssträcka kan nu ges en **färg**, som används för att rita den i
  Topologi-diagrammet.
- En ny **avståndsfaktor** (under Inställningar › Tid & hastighet) låter en bana visa
  en annan – vanligtvis större, mer förebildslik – kilometersiffra i rapporter och den
  grafiska tidtabellen än det avstånd som faktiskt är modellerat, utan att det
  påverkar någon körtidsberäkning.
- Appen håller nu flera öppna webbläsarflikar eller -fönster synkroniserade med
  varandra. **Observera** att detta bara fungerar mellan fönster på samma dator i
  samma webbläsare.
- Inställningar kan nu spara träffens **gäller från**- och **gäller till**-datum,
  utskrivna som en giltighetsrad på rapporter; lämna dem tomma om ingen träff är
  bokad ännu.
- En ny inställning, **utöka plantider automatiskt?** (under Inställningar ›
  Allmänt), utvidgar planens start- eller sluttid för att täcka ett tåg i stället för
  att blockera ändringen när tågets egen tid hamnar utanför den. Avstängd som
  standard.
- En ny knapp, **uppdatera alla tider**, i den grafiska tidtabellen räknar om alla
  tåg i tidtabellen på en gång, i stället för att man först måste välja ut en
  delmängd.
- Spårbeläggningskontrollen kan nu valfritt ta hänsyn till ett lok eller tågsätt som
  står på ett spår mellan två tåg, såvida det inte är bokat till eller från
  uppställning (under Inställningar › Validering). Avstängd som standard, eftersom
  det bara är meningsfullt på banor där uppställning modelleras avsiktligt – slå på
  den där för att upptäcka ett tredje tåg som i tysthet använder ett spår som ett
  stillastående fordon redan upptar.
- Varje uppehåll i fliken **Tåg** har nu ett fält för **Anmärkning** – en notering som
  skrivs ut vid det uppehållet, till exempel ”vänta på mötande tåg”. Anmärkningen visas
  färdigformaterad och byter till den råa märkningen så snart du går in i fältet, så att du
  kan framhäva det som är viktigt: skriv `*sakta*` för kursiv och `**första**` för fet stil.
  Tömmer du fältet försvinner anmärkningen igen.

### Rättningar

- Att lägga till ett nytt tåg sätter nu dess standardstarttid med hänsyn till den
  angivna förberedelsetiden, så att den inte börjar före planens starttid.

## Version 0.2.4

- En ny flik **Tjänster** låter dig planera förartjänster – det arbete en lokförare utför
  under en köromgång, som en följd av de tågdelar hen kör. Varje tjänst är en rad: dess
  beteckning, företag och köromgångar till vänster, tågdelarna i körordning till höger.
- Lägg till de tågdelar en förare kör med **Lägg till tågdel**. Listan visar de
  dragfordonssträckor en förare kan ta härnäst – de som inte krockar i tid med tjänsten
  och, när den har en tågdel, de som avgår vid eller efter att den ankommer. Tågdelarna
  behöver inte börja på samma station: mellan två tågdelar går föraren helt enkelt dit
  nästa börjar.
- Samma tågdel kan köras av flera tjänster så länge de går på olika köromgångar, så en
  tjänst kan täcka de udda köromgångarna och en annan de jämna.
- Där två tågdelar för samma tåg i en tjänst körs av olika dragfordon visar fliken nu en
  anteckning vid stationen där dragfordonet byts – du behöver inte skriva den för hand.
- Du kan ge varje tjänst en beteckning och ett företag, välja de köromgångar den körs och
  lägga till fria anteckningar som gäller hela tjänsten.
- Tjänster som importeras från XPLN delar nu de tågdelar som är definierade i fordonens
  köromgångar, så varje tågdel visar det dragfordon som kör den.
- Planen kontrolleras så att ingen tågdel körs av två tjänster under samma köromgång och
  ingen tjänst har tågdelar som överlappar i tid; eventuella konflikter listas och öppnas
  på fliken **Tjänster**. Du kan slå på eller av kontrollen under
  **Inställningar › Validering**.

## Version 0.2.2

### Rättningar

- Två tåg som aldrig går under samma köromgång rapporteras inte längre som ett möte på
  en enkelspårig sträcka. Ett tåg som går köromgång 1, 3, 5 och ett som går 2, 4, 6 kan
  nu dela samma spår utan falsk varning, eftersom de aldrig är ute samtidigt.
- Konfliktkontrollen på dubbelspåriga (och flerspåriga) sträckor är nu exakt: en
  sträcka flaggas endast när fler tåg befinner sig på den samtidigt än den har spår,
  och endast tåg som går under en gemensam köromgång räknas.

## Version 0.2.1

- Konfliktvarningar visas nu där du kan åtgärda dem. Tågkonflikter visas endast i
  den grafiska tidtabellen och på fliken **Tåg**; fordons- och omloppskonflikter
  visas endast på fliken **Omlopp**.
- På fliken **Omlopp** markerar en fordonskonflikt nu bara det berörda fordonet, och
  en omloppskonflikt markerar bara det omloppet, så att det tydligt framgår vad som
  behöver åtgärdas.
- Kontrollen att ett fordon återvänder till sin utgångspunkt omfattar nu även
  vagngrupper och gods, inte bara lok och tågsätt, så att en vagngrupp eller gods som
  blir kvar på fel plats vid köromgångens slut nu rapporteras.

## Version 0.2.0

- Namnet på den plan du arbetar med visas nu överst i fönstret, så att du alltid
  ser vilket dokument som är öppet.
- Den grafiska tidtabellen visar nu staplar över lokförarbehovet, vilket gör det
  lättare att se hur många förare som behövs under köromgången.
- En ny **Topologi**-vy (under fliken **Sträckor**) visar ett schematiskt diagram
  över tidtabellens sträckor och deras grenar.

### Rättningar

- Sträckor behåller nu som standard den ordning du angav dem i, så att listan är
  lättare att följa när du kontrollerar dina uppgifter. Du kan fortfarande sortera
  på valfri kolumn.
- Konflikter hänvisar inte längre till tåg som du inte kan hitta: när ett tåg tas
  bort tas dess stationsuppehåll bort tillsammans med det, så inga överblivna
  uppehåll eller falska konflikter blir kvar.

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
