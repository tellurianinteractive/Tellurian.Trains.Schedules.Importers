Fanen **Driftssteder** beskriver de driftsmessige stedene på jernbanen din – stasjonene og andre
steder der tog kjører, stopper eller styres, sammen med sporene deres.

### Hvordan velger jeg type driftssted?

1. **Signalstyrt**: hvis det er **ubemannet** og styres av **signaler** fra en tilstøtende **stasjon**, angi **styres fra**.
   Det er en **forgrening** der mer enn to banestrekninger møtes, et **kryssingssted** der du krysser av
   **Tog kan krysse?**, begge deler, eller ingen av dem – en mellomliggende **blokkpost**.
2. **Industriområde**: hvis det er **ubemannet** og **bare for gods**. Kan defineres som **styres fra** en tilstøtende **stasjon** og ha en **nøkkel** for opplåsing/låsing, se *Låsenøkler* nedenfor.
3. **Stasjon**: Kan valgfritt være **bemannet**. En stasjon kan utpekes som **skyggestasjon**.
   Hvis den er ubemannet, kan en stasjon defineres som **styres fra** en tilstøtende **stasjon** og ha en **nøkkel** for opplåsing/låsing, se *Låsenøkler* nedenfor.
4. **Annet**: ikke **bemannet**, ikke styrt av **signaler**, men må være i ruteplanen, som et driftsmessig sted av typen:

En ubemannet stasjon, et industriområde eller en signalstyrt forgrening eller et kryssingssted som **styres
fra** en bemannet stasjon, betjenes av den stasjonens togekspeditør som om det var dens eget: togene står på
den stasjonens togekspederingsliste. En blokkpost deler bare linjen i delstrekninger. Kryss selv av **Tog kan
krysse?**: sporene kan ikke avgjøre det, siden en forgrening har to uansett om tog kan krysse. Der ingen styrer et ubemannet sted, er det bare en del av linjen mellom
togekspeditørene, og tog kan ikke møtes der. Se *Togledelsesstrekninger* på fanen **Strekninger**.

#### Spor på et driftssted

Sporene du definerer her, brukes når du planlegger togoppholdene ved driftssteder.
- Et driftssted **må** ha minst ett **ruteplanspor**.
- Sporfordelingen for tog valideres, så et spor bare kan være opptatt av ett tog om gangen.

#### Plattformer

Der et sted utveksler **passasjerer**, har hvert spor også en **plattformlengde** i meter, med én
desimal. En lengde over null betyr at det er en plattform langs sporet; null betyr at det ikke er noen, og
passasjerer verken kan gå på eller av der. Kolonnen vises bare der passasjerer utveksles,
siden en plattform ikke betyr noe andre steder.

Et nytt persontog settes på et spor med plattform – helst **hovedsporet** av dem – ved hvert
sted det **stopper**. Der stedet ikke har plattform, og der toget bare kjører gjennom, tar
det hovedsporet som ethvert annet tog. Det er det som skjer på et sted som ikke utveksler
passasjerer: toget står der uten å utveksle noe, noe som er nøyaktig hva et møte eller en
forbikjøring er.

Kryss av **Passasjerer?** for et sted der sporene ennå ikke har plattformer, så får hvert spor
en plattform på én meter, som du deretter justerer. Det samme gjøres med en plan som ble laget eller importert før plattformlengder fantes, første gang den åpnes: hvert spor på et sted som utveksler passasjerer, får én
meter, slik at planen fortsetter å virke akkurat som før, og du forkorter eller tømmer sporene som i virkeligheten
ikke har plattform. Et sted der én plattform allerede er registrert, lar vi være i fred, og det samme gjelder et
spor du legger til på et slikt sted: du angir selv om det sporet har en plattform.

Et persontog som stopper for å utveksle passasjerer ved et spor uten plattform, listes opp under
**Konflikter**, og du avgjør hvilken av to ting det er: gi sporet en plattformlengde, eller tøm
oppholdets felter **Ank** og **Avg** på fanen **Tog**, som sier at toget bare står der
uten å utveksle noe. Ingenting rettes for deg, siden bare du vet hva det er. Der et sted har en
plattform ved bare ett spor – det vanlige på en liten stasjon – kan to persontog som møtes
der ikke begge få den, og det som mangler den, er det som rapporteres. Hele kontrollen kan slås
av under **Innstillinger › Validering**.

#### Hvilket spor et tog settes på

Der et sted har mer enn ett spor og et sted å fortsette til, kan hvert spor si hvilken vei
gjennom stedet det er for: det **forrige** stedet et tog kommer fra, det **neste** det går
videre til, eller begge. Kryss av **begge retninger**, så gjelder det samme sporet også for tog som går den andre veien.
Bare stedene som nås med en strekning herfra, tilbys, så et spor bare kan angis for en
trasé et tog faktisk kan ta.

Det er dette et **dobbeltspor** trenger: gi ett spor den ene retningens forrige og neste steder og det andre sporet
det samme paret omvendt, la begge retninger stå uten kryss, så får hver retning
sitt eget spor. La kolonnene stå tomme, så endres ingenting fra før.

Et nytt tog settes på sporet som passer best til traseen. Et spor som angir nøyaktig hvor toget
har kommet fra og hvor det går videre, slår ett som bare angir ett av dem, som igjen slår et spor
som ikke angir noe; et spor som er angitt for et sted toget aldri berører, brukes ikke i det hele tatt, med mindre hvert
spor på stedet er slik, og da står toget på det beste av dem likevel. Der to spor passer
traseen like godt, tar et persontog som **stopper**, et spor med plattform, og et tog som kjører
**gjennom** – og ethvert tog uten passasjerer å utveksle – tar hovedsporet. Bare spor i
ruteplanen vurderes så lenge det finnes noe.

Kolonnene vises bare der det er et valg å ta. Ingenting kastes der de er
skjult: ta et sted ned til ett spor, så er det sporene var angitt for, tilbake så snart
du legger til ett til. Slett et sted, så frigjøres sporene som er angitt for tog til og fra det.

#### Hvor kan tog møtes/kjøre forbi?

- På en **bemannet** stasjon.
- På et ubemannet **signalstyrt sted**.

#### Hvor tog stopper

Stedstypen avgjør om et tog kan stoppe, i tillegg til hvordan du setter opp hvert opphold.
- Du definerer om et driftssted har utveksling av **passasjerer** og/eller **gods**:
- En **skyggestasjon** utveksler alltid begge, uansett hva de to feltene sier, fordi den står for
  alt hinsides det modellerte anlegget – så feltene vises avkrysset og deaktivert for én.

Et tog trenger også noe å utveksle. Et persontog stopper bare der **passasjerer** utveksles, og
et godstog bare der **gods** utveksles.
Et blandet tog med både person- og godsvogner kan stoppe ved driftssteder som utveksler **passasjerer** og/eller **gods**.

Der et tog ikke kan stoppe, vises feltene Ank og Avg på fanen **Tog** tømt og
kan ikke krysses av. Ingenting kastes: slå utvekslingen på igjen, så er alle opphold som er planlagt tidligere,
der igjen.

### Arbeide med listen

Listen viser hvert steds signatur, navn, type, eier, om det er en skyggestasjon (endestasjon for eksterne stasjoner), og hvor
mange spor det har. For hver rad kan du:

- **Info** – se alle opplysninger, inkludert sporene, skrivebeskyttet.
- **Rediger** – åpne et skjema på stedet med feltene for stedets type.
- **Slett** – fjerne stedet. Først vises alt slettingen ville endret, og
  du bekrefter eller avbryter. Tog som har opphold der, mister de oppholdene: et tog som kjører gjennom,
  kjører nå rett forbi, og ett som begynner eller slutter der, begynner eller slutter nå ved sitt neste eller forrige
  opphold. Der stedet ligger mellom nøyaktig to naboer, erstattes de to banestrekningene av én som
  forbinder naboene (like lang, og med samme kjøretid som de to til sammen), og ruteplanstrekningene
  gjennom det går i stedet over den. Slettingen avvises, med årsakene listet opp,
  så lenge et kjøretøyomløp, en førertjeneste eller en godsstrøm begynner eller slutter der, eller så lenge et tog ville
  fått en trasé som ikke henger sammen – ett som snur der, eller kjører gjennom som
  forgrening.

**Legg til ny** spør først om typen (stasjon, signalstyrt eller annet sted) og åpner deretter
redigeringsskjemaet for den typen. Et nytt sted lagres når navnet og en unik signatur er fylt ut.

#### Instruksjoner

En **stasjon** eller et **industriområde** har et felt for **instruksjoner**, skrevet i Markdown og vist ved siden av en levende
forhåndsvisning. Det er for hvordan *dette* stedet drives ved *dette* treffet: hvilke spor som brukes til hva, hvordan
skiftingen er lagt opp, og hva lokførerne og de som bemanner stedet ellers trenger å vite. Hvordan
stedet drives generelt, og enhver annen beskrivelse av det, skal eieren levere, og
hører ikke hjemme her.

Feltet vises ikke der det ikke er noe å instruere om: tog kjører bare forbi et signalstyrt
sted, og ingen arbeider på et annet sted, så et tog der gjør det oppholdet sier, og ikke mer.

#### Redigere spor

Redigeringsskjemaet lister stedets spor, der du kan legge til, redigere og slette dem. Et spor kan bare
slettes så lenge ingen tog bruker det; ellers er slettknappen deaktivert og viser hvor mange
tog som bruker det. Flytt eller fjern de stasjonsoppholdene først.

### Stasjoner og regioner

En stasjon kan knyttes til én eller flere **regioner** fra anleggets regionkatalog (administreres på fanen
**Regioner**). Det brukes mest for skyggestasjoner, som representerer eksterne stasjoner eller regioner
hinsides den modellerte jernbanen. Hvis anlegget ennå ikke har noen regioner, kan du legge til standardsettet fra skjemaet.

### Omstigningsstasjoner

Der treffet kjøres med **passasjerbilletter** – slått på under **Innstillinger › Generelt** – er noen
stasjoner stedene der en passasjer forlater ett tog og fortsetter med det neste. Kryss av **Omstigningsstasjon?**
på hver av dem.

Ingenting annet trenger å planlegges. Et persontog som **stopper** ved en slik, får ved ankomst beskjed om å *utveksle
omstigende passasjerer her*. Et tog som bare kjører forbi, overleverer ingen, og et godstog har ingen å overlevere, så
ingen av dem får noen beskjed.

Omstigningsstasjonene kan også listes opp i heftet med generelle instruksjoner ved å skrive `<Interchanges/>` på
en egen linje i de stående instruksjonene under **Innstillinger › Informasjon**. De skrives ut der
du plasserer taggen, og ingen andre steder.

Krysset tilbys bare der det betyr noe: treffet må bruke billetter, og stasjonen må utveksle
**passasjerer**. Slår du av billettene, beholdes merkingene, akkurat som en låsenøkkel – ingenting
utledes av dem i mellomtiden, og slår du på billettene igjen, kommer omstigningsstasjonene straks tilbake.

### Låsenøkler

Der gods utveksles, men ingen er på vakt – en ubemannet stasjon eller et industriområde – er
sporskiftene vanligvis sikret med hengelås, og nøkkelen oppbevares på en bemannet stasjon langs linjen. Sett **Nøkkel oppbevares ved**
til den stasjonen, og gi nøkkelen et navn hvis stasjonen oppbevarer mer enn én.

Ingenting annet trenger å planlegges. Et godstog som stopper ved stasjonen som oppbevarer nøkkelen og senere stopper ved
stedet nøkkelen låser opp, får ved avgangen fra nøkkelstasjonen beskjed om å *hente nøkkel A1
for opplåsing av Bruket*; når det neste gang har opphold ved den stasjonen på tilbaketuren, får det ved ankomst beskjed om å *levere nøkkel
A1 fra Bruket*. Et tog som bare kjører forbi et av stedene, får ingen beskjed, siden det ikke låser opp noe.

Nøkkelen hentes ved det siste oppholdet ved den oppbevarende stasjonen før arbeidet og leveres tilbake ved det første
etter det, så et tog som har opphold der to ganger, blir ikke bedt om å dra nøkkelen med seg på et ekstra besøk.

En nøkkel betyr bare noe så lenge begge endene holder. Merk selve stedet som bemannet, eller ta
bemanningen bort fra stasjonen som oppbevarer nøkkelen, så gjelder ikke nøkkelen lenger: ingen merknader lages av den, og
**Konflikter** sier hvilken av de to endringene som gjorde det. Selve nøkkelen beholdes, den kastes ikke, så å angre
endringen bringer den straks tilbake – og den blir stående i skjemaet, der du kan peke den mot en annen
stasjon eller tømme den helt.

#### Skiftelok

En stasjon, et industriområde eller et annet sted kan ha **skiftelok**: lokomotiver som er stasjonert der for
den skiftingen stedet trenger. Åpne stedet med **Rediger** og klikk **Legg til skiftelok** under
**Skiftelok**. Angi operatør, nummer, klasse og trekkrafttype for hvert, og en merknad hvis du vil. Et
skiftelok har ikke noe omløp og settes aldri på et tog. Det er der på hver kjøresesjon, så operatøren og
nummeret kan ikke brukes av noe annet kjøretøy; et nummer som allerede er opptatt, avvises.

Hvem som tar med et skiftelok, DCC-adressen og enhetsnummeret oppgis på fanen **Kjøretøyseiere**, som for
ethvert lokomotiv. Et signalstyrt sted kan ikke ha skiftelok, så et sted med skiftelok kan ikke
endres til et slikt. Slettes et sted, slettes skiftelokene også, noe du får beskjed om før du bekrefter.
