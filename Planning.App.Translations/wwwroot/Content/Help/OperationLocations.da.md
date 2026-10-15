Fanen **Driftssteder** beskriver de driftsmæssige steder på din jernbane – de stationer og andre
steder, hvor tog kører, standser eller styres, sammen med deres spor.

### Hvordan vælger jeg type driftssted?

1. **Signalstyret**: hvis det er **ubemandet** og styres af **signaler** fra en tilstødende **station**, angiv **styres fra**.
   Det er en **forgrening**, hvor mere end to banestrækninger mødes, et **krydsningssted**, hvor du afkrydser
   **Tog kan krydse?**, begge dele eller ingen af dem – en mellemliggende **blokpost**.
2. **Industriområde**: hvis det er **ubemandet** og **kun gods**. Kan defineres som **styres fra** en tilstødende **station** og have en **nøgle** til oplåsning/låsning, se *Låsenøgler* nedenfor.
3. **Station**: Kan som valg være **bemandet**. En station kan udpeges som **skyggebanegård**.
   Hvis den er ubemandet, kan en station defineres som **styres fra** en tilstødende **station** og have en **nøgle** til oplåsning/låsning, se *Låsenøgler* nedenfor.
4. **Andet**: ikke **bemandet**, ikke styret af **signaler**, men skal være i køreplanen, som et driftsmæssigt sted af typen:

En ubemandet station, et industriområde eller en signalstyret forgrening eller et krydsningssted, der **styres
fra** en bemandet station, betjenes af den stations togleder, som var det dennes eget: dens tog står på
den stations togekspeditionsliste. En blokpost deler blot linjen i afsnit. Afkryds selv **Tog kan
krydse?**: sporene kan ikke afgøre det, da en forgrening har to, uanset om tog kan krydse. Hvor ingen styrer et ubemandet sted, er det blot en del af linjen mellem
toglederne, og tog kan ikke mødes dér. Se *Togledelsesstrækninger* på fanen **Strækninger**.

#### Spor på et driftssted

De spor, du definerer her, bruges ved planlægning af togophold på driftssteder.
- Et driftssted **skal** have mindst ét **køreplansspor**.
- Sporfordelingen for tog valideres, så et spor kun kan være optaget af ét tog ad gangen.

#### Perroner

Hvor et sted udveksler **passagerer**, har hvert spor også en **perronlængde** i meter, med én
decimal. En længde over nul betyder, at der er en perron ved sporet; nul betyder, at der ingen er, og
passagerer hverken kan stige på eller af dér. Kolonnen vises kun, hvor passagerer udveksles,
da en perron ikke betyder noget andre steder.

Et nyt passagertog stilles på et spor med perron – helst det **hovedspor** af dem, der har – ved hvert
sted, det **standser** ved. Hvor stedet ikke har nogen perron, og hvor toget blot kører igennem, tager
det hovedsporet som ethvert andet tog. Det er, hvad der sker på et sted, der ikke udveksler
passagerer: toget holder dér uden at udveksle noget, hvilket netop er, hvad et møde eller en
overhaling er.

Afkryds **Passagerer?** for et sted, hvis spor endnu ikke har perroner, og hvert spor får
en perron på én meter, som du derefter justerer. Det samme gøres for en plan, der er lavet eller importeret, før perronlængder fandtes, første gang den åbnes: hvert spor på et sted, der udveksler passagerer, får én
meter, så planen fortsætter med at virke præcis som før, og du forkorter eller rydder de spor, der i virkeligheden
ikke har nogen perron. Et sted, hvor én perron allerede er registreret, lades i fred, og det samme gælder et
spor, du tilføjer til et sådant sted: du angiver selv, om det spor har en perron.

Et passagertog, der standser for at udveksle passagerer ved et spor uden perron, opføres under
**Konflikter**, og du afgør, hvilket af to ting det er: giv sporet en perronlængde, eller ryd
opholdets felter **An** og **Afg** på fanen **Tog**, hvilket siger, at toget blot holder dér
uden at udveksle noget. Intet rettes for dig, da kun du ved, hvilket det er. Hvor et sted har en
perron ved kun ét spor – det sædvanlige på en lille station – kan to passagertog, der mødes
dér, ikke begge få den, og det, der mangler den, er det, der rapporteres. Hele kontrollen kan slås
fra under **Indstillinger › Validering**.

#### Hvilket spor et tog stilles på

Hvor et sted har mere end ét spor og et sted at fortsætte hen, kan hvert spor angive, hvilken vej
gennem stedet det er til: det **forrige** sted, et tog kommer fra, det **næste**, det kører
videre til, eller begge. Afkryds **begge retninger**, og det samme spor gælder også for tog, der kører den anden vej.
Kun de steder, der nås ad en strækning herfra, tilbydes, så et spor kun kan angives til en
rute, et tog faktisk kan tage.

Det er, hvad et **dobbeltspor** har brug for: giv det ene spor den ene retnings forrige og næste steder og det andet spor
det samme par omvendt, lad begge retninger stå uafkrydsede, og hver retning får
sit eget spor. Lad kolonnerne stå tomme, og intet ændres fra før.

Et nyt tog stilles på det spor, der passer bedst til dets rute. Et spor, der angiver præcis, hvorfra toget
er kommet, og hvor det kører videre til, slår et, der kun angiver det ene, hvilket igen slår et spor,
der intet angiver; et spor, der er angivet til et sted, toget aldrig berører, bruges slet ikke, medmindre hvert
spor på stedet er sådan, og så holder toget alligevel på det bedste af dem. Hvor to spor passer til
ruten lige godt, tager et passagertog, der **standser**, et spor med perron, og et tog, der kører
**igennem** – og ethvert tog uden passagerer at udveksle – tager hovedsporet. Kun spor i
køreplanen tages i betragtning, så længe der er et.

Kolonnerne vises kun, hvor der er et valg at træffe. Intet kasseres, hvor de er
skjult: tag et sted ned til ét spor, og det, sporene var angivet til, er der igen, så snart
du tilføjer et til. Slet et sted, og de spor, der er angivet til tog til og fra det, frigives.

#### Hvor kan tog mødes/overhale?

- På en **bemandet** station.
- På et ubemandet **signalstyret sted**.

#### Hvor tog standser

Stedets type afgør, om et tog må standse, oven i hvordan du indstiller hvert ophold.
- Du definerer, om et driftssted har udveksling af **passagerer** og/eller **gods**:
- En **skyggebanegård** udveksler altid begge, uanset hvad de to felter siger, fordi den står for
  alt hinsides det modellerede anlæg – så felterne vises afkrydsede og deaktiverede for en.

Et tog har også brug for noget at udveksle. Et passagertog standser kun, hvor **passagerer** udveksles, og
et godstog kun, hvor **gods** udveksles.
Et blandet tog med både passager- og godsvogne kan standse ved driftssteder, der udveksler **passagerer** og/eller **gods**.

Hvor et tog ikke kan standse, vises dets felter An og Afg på fanen **Tog** ryddede og
kan ikke afkrydses. Intet kasseres: slå udvekslingen til igen, og ethvert tidligere planlagt ophold
er der igen.

### Arbejde med listen

Listen viser hvert steds signatur, navn, type, ejer, om det er en skyggestation (endestation for eksterne stationer), og hvor
mange spor det har. For hver række kan du:

- **Info** – se alle oplysninger, inklusive sporene, skrivebeskyttet.
- **Rediger** – åbne en formular på stedet med felterne for stedets type.
- **Slet** – fjerne stedet. Først vises alt, hvad sletningen ville ændre, og
  du bekræfter eller annullerer. Tog, der har ophold dér, mister de ophold: et tog, der kører igennem,
  kører nu lige forbi, og et, der begynder eller ender dér, begynder eller ender nu ved sit næste eller forrige
  ophold. Hvor stedet ligger mellem præcis to naboer, erstattes dets to banestrækninger af én, der
  forbinder naboerne (lige så lang og med samme køretid som de to tilsammen), og køreplansstrækningerne
  gennem det løber i stedet over den. Sletningen afvises, med årsagerne opført,
  så længe et køretøjsomløb, en førertjeneste eller en godsstrøm begynder eller ender dér, eller så længe et tog ville
  få en rute, der ikke hænger sammen – et, der vender dér, eller kører igennem som
  forgrening.

**Tilføj ny** spørger først om typen (station, signalstyret eller andet sted) og åbner derefter
redigeringsformularen for den type. Et nyt sted gemmes, når dets navn og en unik signatur er udfyldt.

#### Instruktioner

En **station** eller et **industriområde** har et felt til **instruktioner**, skrevet i Markdown og vist ved siden af en levende
forhåndsvisning. Det er til, hvordan *dette* sted betjenes ved *dette* træf: hvilke spor der bruges til hvad, hvordan
rangeringen er tilrettelagt, og hvad lokoførerne og dem, der bemander stedet, ellers behøver at vide. Hvordan
stedet betjenes generelt, og enhver anden beskrivelse af det, skal ejeren levere og
hører ikke hjemme her.

Feltet vises ikke, hvor der intet er at instruere om: tog kører kun forbi et signalstyret
sted, og ingen betjener et andet sted, så et tog dér gør, hvad dets ophold siger, og ikke mere.

#### Redigering af spor

Redigeringsformularen viser stedets spor, hvor du kan tilføje, redigere og slette dem. Et spor kan kun
slettes, så længe intet tog bruger det; ellers er dets sletknap deaktiveret og viser, hvor mange
tog der bruger det. Flyt eller fjern først de stationsophold.

### Stationer og regioner

En station kan knyttes til en eller flere **regioner** fra anlæggets regionskatalog (administreres på fanen
**Regioner**). Det bruges mest til skyggebanegårde, som repræsenterer eksterne stationer eller regioner
hinsides den modellerede jernbane. Hvis anlægget endnu ikke har nogen regioner, kan du tilføje standardsættet fra formularen.

### Passagerskiftestationer

Hvor træffet køres med **passagerbilletter** – slået til under **Indstillinger › Generelt** – er nogle
stationer de steder, hvor en passager forlader ét tog og fortsætter med det næste. Afkryds **Omstigningsstation?**
på hver af dem.

Intet andet behøver at blive planlagt. Et passagertog, der **standser** ved en sådan, får ved ankomsten besked om at *udveksle
omstigende passagerer her*. Et tog, der kun kører forbi, overdrager ingen, og et godstog har ingen at overdrage, så
ingen af dem får nogen besked.

Omstigningsstationerne kan også opføres i hæftet med generelle instruktioner ved at skrive `<Interchanges/>` på
en linje for sig selv i de stående instruktioner under **Indstillinger › Information**. De udskrives, hvor
du placerer mærket, og ingen andre steder.

Afkrydsningen tilbydes kun, hvor den har betydning: træffet skal bruge billetter, og stationen skal udveksle
**passagerer**. Slå billetterne fra, og markeringerne bevares, ligesom en låsenøgle – intet
afledes af dem i mellemtiden, og slår du billetterne til igen, kommer omstigningsstationerne straks tilbage.

### Låsenøgler

Hvor gods udveksles, men ingen er på vagt – en ubemandet station eller et industriområde – er
sporskifterne som regel sikret med hængelås, og nøglen opbevares på en bemandet station langs linjen. Sæt **Nøgle opbevares på**
til den station, og giv nøglen et navn, hvis stationen opbevarer mere end én.

Intet andet behøver at blive planlagt. Et godstog, der standser ved den station, der opbevarer nøglen, og senere standser ved
det sted, nøglen låser op, får ved sin afgang fra nøglestationen besked om at *hente nøgle A1
til oplåsning af Bruket*; når det næste gang har ophold ved den station på tilbagevejen, får det ved ankomsten besked om at *aflevere nøgle
A1 fra Bruket*. Et tog, der kun kører forbi et af stederne, får ingen besked, da det ikke låser noget op.

Nøglen hentes ved det sidste ophold på den opbevarende station før arbejdet og afleveres ved det første
efter det, så et tog, der har ophold dér to gange, ikke bliver bedt om at slæbe nøglen rundt på et ekstra besøg.

En nøgle betyder kun noget, så længe begge ender holder. Markér selve stedet som bemandet, eller fjern
bemandingen fra stationen, der opbevarer nøglen, og nøglen gælder ikke længere: der laves ingen noter ud fra den, og
**Konflikter** siger, hvilken af de to ændringer der gjorde det. Selve nøglen bevares, den kasseres ikke, så hvis
ændringen fortrydes, kommer den straks tilbage – og den bliver på formularen, hvor du kan pege den på en anden
station eller rydde den helt.

#### Rangerlokomotiver

En station, et industriområde eller et andet sted kan have **rangerlokomotiver**: lokomotiver, der er stationeret dér til
den rangering, stedet har brug for. Åbn stedet med **Rediger**, og klik på **Tilføj rangerlokomotiv** under
**Rangerlokomotiver**. Angiv for hvert operatør, nummer, klasse og trækkrafttype og en bemærkning, hvis du vil. Et
rangerlokomotiv har intet omløb og sættes aldrig på et tog. Det er der på hver køresession, så dets operatør og
nummer må ikke bruges af noget andet køretøj; et nummer, der allerede er optaget, afvises.

Hvem der medbringer et rangerlokomotiv, dets DCC-adresse og dets enhedsnummer indtastes på fanen **Køretøjsejere**, som for
ethvert lokomotiv. Et signalstyret sted kan ikke have rangerlokomotiver, så et sted med rangerlokomotiver kan ikke
ændres til et sådant. Sletning af et sted sletter også dets rangerlokomotiver, hvilket du får besked om, før du bekræfter.
