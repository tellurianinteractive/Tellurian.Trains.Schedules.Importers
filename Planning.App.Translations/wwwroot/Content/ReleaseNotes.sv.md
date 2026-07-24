# Versionsnyheter

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
