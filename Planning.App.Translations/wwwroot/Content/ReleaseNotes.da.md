# Versionsnyheder

## Version 1.4.1

### Ændringer

- **Farvekodede vognsæt- og turnuskort.** Den diagonale streg på turnuskortet er nu lysegrøn for passagervognsæt, lyseblå for godsvognsæt og lyseorange for togsæt. Vognsætkortet har en streg i samme farve under vognsættets navn og udskriver nu vognsættets bemærkning, også når den blot gentager det eksterne id.

## Version 1.4.0

### Nyt

- **Bemanding af bemandede stationer.** En bemandet station kan nu angive sit personale, hvor hver person har en eller flere roller: togleder, stillværksbetjent, rangerleder, rangerer og elev. En lang session kan deles i op til tre hold med det samme personale på hvert hold. Hver person på hvert hold tæller som én deltager. Afsnittet findes under fanen Driftssteder og vises kun, når stationen er bemandet.
- **Rapporten Arbejdsplan.** En ny rapport i rapportmenuen udskriver et ark for hver bemandet station på sprogene i dens land: en tabel for hver person med en række for hver session eller dag og en kolonne for hvert hold, hvor første hold begynder, når første session begynder. Stationens ejer skrives ind på toglederens første hold i første session.

## Version 1.3.12

### Ændringer

- **Vognsætkortene viser hver vogn som en boks.** Vognene tegnes som bokse, tre ved siden af hinanden, hver en tredjedel af den bredde, kortet skrives på, i togets rækkefølge. Vogne med angivne oplysninger får også en boks, med klasse og nummer kun et mellemrum fra hinanden; typebogstavet (P eller G) og positionsnummeret udelades.

## Version 1.3.11

### Ændringer

- **Færre passagerbilletter.** Billetrapporten udskriver nu billetter i forhold til antallet af persontog, der standser på hver station, hver til en tilfældigt valgt destination, i stedet for én til hver anden station. Den nye indstilling ”Tog pr. billet” (Indstillinger, Passagerer; standard 4) angiver, hvor mange standsende tog en billet svarer til. Returbillet­terne går stadig frem og tilbage.
- **Omløbsnote, også hvor standsningen ikke er markeret.** Et tog, der vender på et sted, får nu sin omløbsnote og tiden til den, selv når kaldet dér er markeret som gennemkørsel. Et tog kan ikke vende uden at holde stille, så ruten afgør, ikke markeringerne Ank/Afg.

## Version 1.3.10

### Ændringer

- **Et lokomotiv med en godsstrøm kører om.** Et tog med en godsstrøm regnes nu for at have vogne med, også når intet vognsæt er tildelt det, så et lokomotiv, der skal vende med det, får sin omløbsnote og tiden til det. Før blev et sådant lokomotiv anset for at køre alene.

## Version 1.3.9

### Nyt

- **Vognsætkort.** En ny rapport, Vognsætkort, udskriver et A6-kort for hvert vognsæt, der er markeret med Udskriv vognsætkort? i sin køretøjsdialog: dets identitet, art og vogne, til at følge dets omløbskort. Fire kort er der plads til på et A4-ark i liggende format, og hvert kort udskrives på vognsættets operatørs sprog.

## Version 1.3.8

### Ændringer

- **Et togsæt får ingen note om at koble til eller fra.** Et togsæt er selve toget, så der er intet at koble det til eller fra. Lokomotiver beholder deres noter.

## Version 1.3.7

### Ændringer

- **Ens noter siges én gang.** Noter, der lyder ens bortset fra de sessioner, de gælder for, er nu én note for disse sessioner samlet, og angiver ingen sessioner, hvor de tilsammen dækker alle de sessioner, toget kører.
- **Møder i samme sessioner deler én kvalificering.** I en note om krydsning eller overhaling opregnes de møder, der sker i de samme sessioner, sammen med sessionerne angivet én gang efter dem, og grupperne adskilles med semikolon.
- **Et vognsæt, der følger et push/pull-tog, behøver ingen egen note.** Hvor et push/pull-lokomotiv i samme omløb flytter toget til eller fra et andet spor, udelades noterne om at hente eller rangere vognsættet, da noten om toget allerede siger det. Vognsættet beholder sine noter for de sessioner, lokomotivet ikke kører.
- **Et omløbskort for én enkelt dag navngiver den fuldt ud.** Hvor et kort kun gælder én dag, står der nu "Lørdag" i stedet for "L". Flere dage forkortes stadig.

## Version 1.3.6

### Ændringer

- **En rapport om køretøjsfordeling viser, hvem der kører hvad og hvornår.** En ny rapport under **Rapporter** giver hvert køretøj en linje på en tidsskala under en overskrift for dets selskab. Hvert togafsnit, køretøjet kører, er en kasse i togets farve med togets nummer og de minutter, det afgår og ankommer, og forberedelses- og afslutningstiden omkring den er grå. Første kolonne navngiver køretøjet som fanen Omløb. Tidsskalaen, driftsvinduet og delingen ved pausetidspunktet følger den grafiske køreplans udskriftsindstillinger.

## Version 1.3.5

### Ændringer

- **Aflæsning og læsning ved samme stop giver én note.** Hvor et godsvognssæt både aflæsses og læsses igen, hvor et togafsnit slutter, står der nu i ankomstnoten, at vognene skal aflæsses og læsses igen med anden last, i stedet for to separate noter. Et nyt afkrydsningsfelt **Er det containere?** på togafsnittet ændrer noten til at aflæsse containere og læsse andre containere.

## Version 1.3.4

### Ændringer

- **Oversigten i rapporten Medbragte køretøjer viser, hvad der mangler.** I oversigten efter ejer siger en ny kolonne Kommentar med rødt, når en deltager ikke har oplyst alle DCC-adresser, og med almindelig tekst, når numre på lokomotiver, togsæt eller rangerlokomotiver mangler.

## Version 1.3.3

### Fejlrettelser

- **Rapporten Medbragte køretøj åbner igen.** Efter at oversigtssiden blev tilføjet, kunne opstillingerne efter ejer og efter driftssted ikke vises og gav en fejl. De åbner nu med oversigten som tænkt.

## Version 1.3.2

### Ændringer

- **Et blindspor er ikke længere en side for sig på dispatcherlisten.** På en station, hvis dispatching er delt efter retning, hører et sidespor uden nogen at ringe til i den anden ende nu til den retning, hvor sporet løber samme vej ved stationen, begge væk fra den eller begge hen mod den. Et sidespor, stationen selv dispatcher, f.eks. et industriområde, tæller også med. Dets rækker står på den side, og har det et telefonnummer, står nummeret der ved siden af naboerne. Et sidespor, der løber begge veje, eller som ingen anden retning passer til, beholder sin egen side.
- **Rapporten Medbragte køretøjer åbner med en oversigt.** Opstillingerne efter ejer og efter driftssted begynder nu med en oversigtsside: en række for hver deltager med antallet af enheder, vedkommende medbringer, reserver medregnet, eller for hvert driftssted med dets ejer og antallet af enheder, der er stillet op der, reserver ikke medregnet. Alle kan finde deres egen side i bunken. Enheder, ingen medbringer, og enheder, der ikke er i drift, er udeladt. Listen efter DCC-adresse behøver ingen oversigt og har ingen.

## Version 1.3.1

### Ændringer

- **Togklarering og lokomotivomløb sætter det korteste ophold.** Tiderne **Togklarering** og **Lokomotivomløb** er reel tid og angives nu som ét tal: under 10 er det minutter, og fra 10 er det sekunder, og enheden vises efter feltet. 1 er ét minut, og 90 er halvfems sekunder. Tiden omregnes til hurtigur-tid med hurtigurets hastighed. Den er det korteste ophold, et tog må have for at blive klareret på en bemandet station, eller for at køre lokomotivet rundt, og lægges ikke oven i det mindste ophold: den længste af dem gælder. En ny indstilling, **Validér nødvendige opholdstider?**, rapporterer et tog med kortere ophold. Forlæng opholdet, eller sænk den krævede tid, for at fjerne advarslen.

## Version 1.3.0

### Ændringer

- **Omløb afgøres ud fra ruten.** Indstillingen **Omløb** på et togafsnit og på et stop er nu **Ud fra ruten**, **Aldrig** eller **Altid**. Som standard får lokomotivføreren besked om at køre lokomotivet om, hvor toget afgår i modsat retning af den, det kom fra — inden for et tog eller over i det næste tog, som samme lokomotiv kører — medmindre lokomotivet kan vende, som det står (et togsæt eller et vendbart tog). Vælg **Aldrig**, hvor der ikke skal omløb til, eller **Altid** for at bede om et omløb, som ruten ikke kræver. Et omløb, du tidligere havde afkrydset, bevares som **Altid**. **Opdater tider** følger samme valg, når der afsættes tid til omløbet.

- **Angiv, at et tog er et vendetog, når du opretter det.** En ny indstilling **Vendetog?** i dialogen Tilføj tog og i fanen Tog angiver, at toget kan køres fra begge ender (et togsæt eller et vendetog). Et sådant tog får ingen tid til lokomotivets omløb, hvor det skifter retning, og ingen omløbsbemærkning, selv før et køretøj er tildelt. Når et køretøj kører toget, er det køretøjet, der afgør. Returtog og kopier arver indstillingen. Et tog, der er angivet som vendetog, men køres af et køretøj, der skal køres om, får en advarsel.

- **Spor og køretøjer har længder.** Angiv en **Længde** for et køretøj i køretøjsdialogen (for et vognsæt hele sættet) og for et spor i fanen Driftssteder. Tog, der holder på et spor med længde, kontrolleres så efter deres længder i stedet for at tælle som dobbeltbooking: dem, der kan være der samtidig, må dele sporet, og der vises en advarsel, når de er for lange tilsammen. En anden advarsel vises, når et tog eller vognsæt ikke kan forlade sporet, fordi et andet holder i vejen — det, der kom ind sidst i en ende, skal ud først, hvad enten det er et tog eller et vognsæt sat på et sidespor. En ny indstilling **Kun fra den ene ende?** på et spor er til blindspor. Et tog uden køretøjer med kendt længde regnes med sin største længde i meter, hvis den er angivet; alt uden længde kontrolleres som før.

- **Et køretøj, der hentes fra et andet spor, starter dér.** I rapporten Medbragte køretøjer og i fanen Køretøjsejere vises et lokomotiv, togsæt eller vognsæt, hvis første togafsnit henter det fra et andet spor, nu på det spor, hvor det skal stilles op før start, i stedet for på afgangssporet.

### Rettelser

- **Stop efter midnat vises i tidsorden.** I fanen Tog sorteres stoppene på et driftssted nu efter klokkeslæt, så et stop kl. 00:30 på et tog, der kører over midnat, vises blandt stoppene tidligt om morgenen i stedet for efter dem sent om aftenen.

## Version 1.2.5

### Rettelser

- **Et lokomotiv, der henter holdende vogne, er ikke længere en konflikt.** Når et lokomotiv kommer i ét tog og kører videre i næste tog med de vogne, der holder på sporet, meldes dets eget stop dér ikke længere som overlappende med vognene.

## Version 1.2.4

### Ændringer

- **Tognumre i kategorioverskrifterne.** I fanen Tog viser hver kategorioverskrift nu også kategoriens præfiks samt det laveste og højeste tognummer i den, for eksempel *P 101–148*, så det er lettere at finde et bestemt tognummer.
- **Dobbeltspor som to enkeltspor.** En ny indstilling i fanen Indstillinger, **Behandl dobbeltspor som to enkeltspor?**, får også tog, der kører samme vej på et dobbeltspor, til at holde afstand, så hver strækning rummer ét tog pr. retning. Den er slået fra som standard og kræver strækningsvalidering.

## Version 1.2.3

### Ændringer

- **Tilføj stop ved togets ender.** I fanen Tog står **Tilføj første stop** nu over stoppene, og lokationslisten ved siden af hver tilføj-knap tilbyder slutstoppets egen lokation og alle lokationer ved siden af den, på begge sider, også den toget kom fra eller kører til. Et stop, der bryder ruten, meldes som en konflikt på toget.

## Version 1.2.2

### Ændringer

- **Tilføj et stop i begyndelsen af et tog.** I fanen Tog kan du nu tilføje et stop i begyndelsen af et tog og ikke kun i slutningen, med **Tilføj første stop**. Ved begge knapper vælger du lokationen på en liste ved siden af.

## Version 1.2.1

### Ændringer

- **Trækkraftenheder i fanen Tog.** Kolonnen Køretøjer viser ikke længere enheder, der kører et tog fra et andet selskab med samme nummer. Tognumre er kun entydige inden for et selskab.

## Version 1.2.0

### Ændringer

- **Togekspeditionsliste pr. retning.** En bemandet station har en ny indstilling i fanen Driftssteder, **Togekspeditionsliste pr. retning?**. Når den er afkrydset, opdeles stationens togekspeditionsliste i én liste for hver retning, hvilket er bekvemt på en travl station. Et tog meldes ind på listen for den retning, det kommer fra, og meldes ud på listen for den retning, det kører mod, så et gennemkørende tog står på begge lister, ankomsten på den ene og afgangen på den anden. Hver liste viser retningen i overskriften og kun de nabostationer med telefonnumre, der ligger på dens egen side. De redigerbare ekspeditionsdokumenter opdeles på samme måde, ét dokument pr. retning.

- **Køretøjer, der løftes på ved afgang.** I rapporten Medbragte køretøjer vises et køretøj, der løftes på sporet lige før afgang, nu stående på et bord i stedet for på et spor.

- **Antal enheder.** Et køretøj tæller altid som mindst én enhed, også i planer gemt tidligere uden.

## Version 1.1.2

### Ændringer

- **Køretøjer, der kører på andre tidspunkter.** Et lokomotiv eller vognsæt, der allerede kører noget i alle køresessioner, tilbydes nu, når du tildeler et køretøj til et omløb, der kører på andre tidspunkter. Tidligere var det skjult, så et lokomotiv ikke kunne tilføjes som forreste lokomotiv i et skub-træk-tog, mens det også kørte et natttog for sig selv. Et køretøj skjules kun, når alt det allerede kører overlapper omløbet i tid, og det tager kun de køresessioner, hvor det er ledigt.

- **Trækkraftenheder i togets stop.** Et togs stop viser de lokomotiver og togsæt, der kører det, med de køresessioner de kører det i, når det ikke er alle togets.

## Version 1.1.0

### Ændringer

- **Et ekstra lokomotiv på et tog.** Et lokomotiv kan nu føjes til omløbet for det lokomotiv, det kører sammen med, i stedet for at få et eget omløb, der gentager de samme tog. Angiv en **Placering i toget**, når du tilføjer det, eller når du redigerer det i fanen Omløb: **Forrest** fører toget, **Bagest** står i den anden ende, så toget har et lokomotiv i hver ende, og **Forstærkning** kobles til det forreste lokomotiv. Et lokomotiv bagest betyder, at toget ikke omløbes, og afgangen får en note om, at der er et lokomotiv i hver ende. Rapporten Togsammensætninger tegner en ekstra lokomotivboks i togets fjerne ende, og der vises en advarsel, når et lokomotiv bagest i nogle køresessioner ikke har noget lokomotiv foran toget.

- **Kortere noter for flere lokomotiver eller vognsæt.** Lokomotiver eller vognsæt, der får samme besked i de samme køresessioner, nævnes i én note ("kør SJ 04 Rc6 og SJ 05 Rc6 til henstilling"), og noterne om kørsel til henstilling gentager ikke længere ankomstsporet.

## Version 1.0.8

### Ændringer

- **Øvrige køretøjer.** Fanen Køretøjsejere har en ny underfane, **Øvrige køretøjer**, til køretøjer, der skal registreres, men ikke indgår i driften: hverken planlagte eller rangerlokomotiver. Angiv type (lokomotiv, togsæt, vognsæt eller andet), en identitet med egne ord, ejer, nummer og DCC-adresse. De udskrives til deres ejer i rapporten Medbragte køretøjer, uden dag, station eller tid.

- **Rangerlokomotivets operatør.** Når man skifter operatør på et rangerlokomotiv, afbrydes det ikke længere, hvis nummeret allerede er optaget af et andet køretøj hos operatøren. Operatøren skiftes, konflikten rapporteres, og du kan stige nummeret til et ledigt.

## Version 1.0.7

### Ændringer

- **Køretøjer, der skifter mellem køreplaner.** Et lokomotiv eller vognsæt, der kører én køreplan nogle køresessioner eller dage og en anden køreplan de øvrige, regnes ikke længere for at holde ubrugt på den første køreplans sidste spor de dage, det kører den anden. Det fjerner falske sporkonflikter, når to køretøjer skiftes til at køre to køreplaner, og når køresessionerne var angivet som dage.

## Version 1.0.6

### Ændringer

- **Startetiketter.** Virksomhedens signatur står nu på en række for sig på startetiketten, og togets præfiks, nummer og suffiks står altid sammen på rækken under, så tognummeret ikke længere brydes nogle steder og andre ikke.

- **Køretøjsbemærkninger.** Bemærkningen, der er angivet på et køretøj i køreplanen, vises nu på fanen Køretøjsejere og i rapporten over køretøjsbidrag. En bemærkning, der kun gentager køretøjets eksterne id, udelades.

## Version 1.0.5

### Ændringer

- **Lokomotiver, der overtager i blokke af køresessioner.** Når ét lokomotiv kører køresession 1–3 og et andet køresession 4–6, regnes det første nu for fjernet fra sporene, når det andet overtager, så de to ikke længere er i konflikt på et spor, hvor begge holder. Lokomotiver, der skiftes til at køre (1, 3, 5 og 2, 4, 6), regnes stadig for at holde ubrugt de dage, de ikke kører. Køresessioner ud over banens højeste antal køresessioner ses der bort fra.

## Version 1.0.3

### Ændringer

- **Træk af tog på fanen Sporbelægning.** Når et tog trækkes til et andet spor, opstår der ikke længere fejlen "More than one sibling … has the same key value", hvis en station har to spor med samme nummer.

- **Offlinefunktionen kunne ikke installeres.** Appens service worker forsøgte at gemme en konfigurationsfil, som webværten ikke leverer, og det standsede hele installationen. Filen er nu udeladt. Hvis du har installeret appen tidligere, anbefaler vi, at du **afinstallerer den og installerer den igen**, så den nye version overtager, og offlinefunktionen virker.

## Version 1.0.2

### Ændringer

- **Lokomotiver og togsæt på fanen Tog.** Den nye kolonne **Køretøjer** viser, hvilket lokomotiv eller togsæt der kører hvert tog, så du med det samme kan se, hvilke tog der er planlagt med det samme køretøj.

- **Flyt eller kopiér til et starttidspunkt.** I dialogen til at flytte og kopiere kan du nu angive det første valgte togs nye starttid (tt:mm) i stedet for et antal minutter. En forhåndsvisning viser, hvor langt togene flyttes.

- **Første start understreget på omløbskort.** På omløbskortene er starttidspunktet, hvor køretøjet begynder sin første arbejdsdag, nu understreget.

- **Hjælp til valideringsindstillingerne.** Hver valideringsindstilling har nu et **?**, der forklarer, hvad reglen rapporterer, og valgmuligheden, der får holdende køretøjer til at optage sporet, er rykket ind under den validering af stationsspor, den afhænger af.

## Version 1.0.1

### Ændringer

- **Køretøjer, der holder natten over.** Køretøjer, der holder på et spor natten over og kører med samme tog om morgenen, meldes ikke længere som holdende på sporet samtidig med hinanden.

## Version 1.0.0

### Ændringer

- **Togkategorien på startsedler.** En startseddel angiver nu togets kategori over tognummeret, i et bånd i kategoriens egen farve.

- **Hvid tekst på flere farver.** Tekst på farvet baggrund, for eksempel kategoribåndene, regionernes markeringer og diagrammerne for tjenester og omløb, er nu hvid også på middellyse farver som pink, ikke kun på mørke. Lyse farver som gul og orange beholder sort tekst.

- **Overlappende holdende køretøjer meldes.** Når *Køretøjer optager sporet, mens de holder?* er slået til, meldes køretøjer fra forskellige omløb, der holder på samme spor på samme tid eller tættere end minimumstiden mellem sporbrug, nu som en informationsmeddelelse på begge ophold. Køretøjer, der kommer eller kører med samme tog, meldes ikke, da de holder sammen med vilje.

## Version 0.9.18

### Nye funktioner

- **Startsedler.** Den nye rapport **Startsedler** udskriver et kort for et togs første afgang, foldet på midten og stillet på startsporet, så det kan læses fra begge sider. Det viser station, spor, tognummer, afgangstid, tjeneste og, hvor lokomotivet eller togsættet kom som et andet tog, hvilket tog det var. Vælg, hvilke tog der får et – **Ingen**, **Første** eller **Alle** – på togkategorien og på hver bemandet station. *Første* er toget, der starter dagen for et lokomotiv eller et togsæt, og et tog får kun en seddel, hvis både dets togkategori og dets station tillader det; det snævreste valg gælder.

- **Skub vognene fra et stop.** På fanen Tog angiver den nye kolonne **Skub**, at vognene skubbes fra det stop og frem, ofte i stedet for omløb. Det giver en bemærkning ved afgangen, til lokomotivføreren og i fjernstyringslisten: *Skub vogne til* efterfulgt af, hvor vognene skal hen — godsstrømmenes kommende destinationer, ellers hvor vognsættets togafsnit slutter — eller blot *Skub vognene.*, når ingen af dem siger noget. Fra togets sidste stop skubbes der ikke.

## Version 0.9.17

### Fejlrettelser

- **Fyldigere sider i ekspeditionslisterne.** Noternes bredde måles nu i den rapportskrifttype, du har valgt, i stedet for at blive antaget, så en lang note ikke får en linje, den ikke bruger, og der er plads til flere rækker på en side. Siderne har også 6 mm margen foroven og forneden i stedet for 10 mm.

### Ændringer

- **Noten ved ankomst af et godsflow følger, hvor vognene skal hen.** På en skyggestation lyder den *Sortér ankomne vogne efter stationens instruktioner.* På andre stationer lyder den *Rangér ankomne vogne.*, når nogle af vognene skal til andre destinationer end stationen, og *Rangér ankomne vogne til godskunderne.*, når de alle er til stationen, og flowet rangerer dem. Ellers er der ingen note. Sætningen *Frakobl alle vogne.* er fjernet.

## Version 0.9.16

### Fejlrettelser

- **Et vognsæt, der er delt i to, vises nu i tjenesterne.** Når en tjeneste har et helt tog som én del, men togets vognsæt er planlagt i to dele, f.eks. for at blive læsset halvvejs, var vognsætsafsnittet på tjenestesiden tomt. Begge strækninger af vognsættet vises nu.

### Ændringer

- **Tydeligere formulering ved aflæsning af vogne.** Valget på et togafsnit hedder nu *Aflæs vognene og bring godset tilbage?*, og noten ved ankomst siger, at vognene skal aflæses, og godset bringes tilbage til den station, hvor de blev læsset.

## Version 0.9.15

### Fejlrettelser

- **Ekspeditionslister har nu alle tog, der passerer et sted, stationen styrer.** En station, der styrer en ubemandet sporforgrening, f.eks. Steinsnes med Triangeln, manglede de tog dér, som også standser på stationen selv, f.eks. godstog, der vender i Tveiten og passerer Triangeln to gange. Alle passager vises nu, i begge retninger.
- **Første session er gråtonet for ekspeditioner, der ikke kan nås på den.** På ekspeditionslisterne er første sessions afkrydsningsfelt gråtonet, når træffet starter senere end driftsdøgnet, og toget begyndte tidligere, eller ekspeditionen ligger efter midnat.
- **Fyldigere sider i ekspeditionslisterne.** Bemærkninger antages ikke længere at være bredere, end de er, så færre rækker får en ekstra linje, og de udskrevne sider udnyttes bedre.

## Version 0.9.14

### Fejlrettelser

- **Et vognsæt, der slutter døgnet læsset, rapporteres ikke længere.** Når et godsvognsæt aflæsses og senere læsses igen i sit omløb, bliver det simpelthen stående læsset til næste kørselsdag, hvor det aflæsses først. Før blev dette rapporteret både som aflæsning uden forudgående læsning og som vogne efterladt læsset til sidst. Et vognsæt, der aldrig aflæsses, eller aldrig læsses før det aflæsses, rapporteres stadig.

## Version 0.9.13

### Ændringer

- **Køretøjer, der holder natten over, regnes ikke længere som stående mere end et døgn.** På fanen Sporbelægning vises køretøjer, der slutter døgnet på et spor og kører fra det næste døgn, nu som ét ophold fra sidste ankomst til første afgang. Før blev et køretøj, der ankom efter midnat, vist som stående fra ankomsten til døgnets slutning plus fra døgnets start til afgangen, så tre timer kunne se ud som 27.

## Version 0.9.12

### Ændringer

- **Ny fane: Sporbelægning.** Viser hvert spor på hver station gennem døgnet, med tiden langs aksen. Tog er farvede blokke fra ankomst til afgang, og køretøjer, der bliver stående mellem deres tog, er stiplede, skraverede blokke (når *Køretøjer optager sporet, mens de holder?* er slået til). Blokke, der er på samme spor samtidig, lægges i egne linjer oven på hinanden, så de kan skelnes, og blokke, der indgår i en sporkonflikt, har rød kant; hold musen over en blok for at se detaljerne. Træk et tog til et andet spor på samme station for at flytte det. Du kan vælge, hvilke stationer der vises, og zoome tidsaksen.

## Version 0.9.11

### Ændringer

- **Sporkonflikter med holdende køretøjer markeres på sporene igen.** Når *Køretøjer optager sporet, mens de holder?* er slået til, markeres en konflikt mellem et tog og køretøjer, der holder på sporet, nu også på de tog, der er involveret, i fanen Tog og i den grafiske køreplan. Det ventende togs ophold på sporet fremhæves, og det samme gør den stiplede linje for de holdende køretøjer, der er i vejen; hold musen over en af dem for at se meddelelsen. Kun det ophold, der reelt støder sammen, markeres: tog og køretøjer, der aldrig kører på samme køresession, rapporteres ikke. Konflikten vises stadig også på køretøjernes omløb i fanen Omløb.

## Version 0.9.10

### Ændringer

- **Kun den konfliktende del af et tog fremhæves i den grafiske køreplan.** En konflikt, for eksempel et overbooket spor, markerer nu kun opholdet på det spor eller strækningen mellem sporene, hvor den opstår, i stedet for togets hele linje. Hold musen over den fremhævede del for at se meddelelsen.

## Version 0.9.9

### Ændringer

- **Rangeropgaver uden lokomotiv lader ikke længere køretøjer holde på et spor.** Når *Køretøjer optager sporet, mens de holder?* er slået til, regnes køretøjer, der kun indgår i en rangeropgave uden trækkraft, ikke længere for at holde på sporet før og efter den. En sådan opgave optager ikke noget spor, så køretøjerne gav sporkonflikter, der ikke var reelle.

## Version 0.9.7

### Ændringer

- **Sporkonflikter med holdende køretøjer vises på omløbet.** Når *Køretøjer optager sporet, mens de holder?* er slået til, markeres en konflikt mellem et tog og køretøjer, der holder på sporet, nu på køretøjernes omløb i fanen Omløb, hvor den oftest rettes, og ikke længere på togene i fanen Tog og i den grafiske køreplan. Konflikter mellem to tog vises stadig på togene.

## Version 0.9.6

### Ændringer

- **Minutetiketter for gennemkørende tog i den grafiske køreplan.** Indstillingerne for at vise ankomst- og afgangsminutter gælder nu også for tog, der kører gennem en station uden at stoppe. Når et gennemkørende tog har samme ankomst- og afgangstid, får det én enkelt etiket, afgangsminuttet, så snart en af indstillingerne er afkrydset.

- **Køretøjer optager sporet, mens de holder.** En ny indstilling under Validering, *Køretøjer optager sporet, mens de holder?*, får sporkontrollen til at regne med, hvor lokomotiver, togsæt og vognsæt holder mellem deres tog. Et køretøj, der ankommer og ikke flyttes, optager sit ankomstspor, indtil det afgår igen. Et køretøj, der sættes på eller hentes fra et andet spor, optager det spor fra ankomst til næste afgang, mens ankomstsporet stadig er optaget af det ankommende tog. Et køretøj, der er ankommet og ikke har nogen yderligere afgang, optager sit spor resten af døgnet, og et køretøj uden ankomst før sin første afgang optager det fra døgnets begyndelse. Opstilling og aflæsning frigiver sporet, indtil køretøjet hentes igen. Et tog, der tager et spor, hvor et køretøj holder, rapporteres som en konflikt. Den grafiske køreplan, på skærmen og på print, markerer hvert ophold med en stiplet linje langs sporet.

- **Vognsæt kan opstilles eller løftes af.** Når du redigerer en togdel for et vognsæt, tilbyder *Vognsæt før afgang* og *Vognsæt efter ankomst* nu de samme valg som for lokomotiver: på spor, opstilling eller løft af og på. Valget udskrives som en note i begge ender, de to ender holdes i takt, en uoverensstemmelse rapporteres, og et vognsæt, der er væk fra banen, optager ikke noget spor.

## Version 0.9.5

### Ændringer

- **Tog kan flyttes og få nye tider hen over midnat i en døgnkøreplan.** Når banen kører over midnat, kan et tog, der starter 00:10, nu flyttes 20 minutter tidligere og starte 23:50 og køre videre forbi midnat, og omvendt. Det virker i dialogerne Flyt og Kopiér og ved ændring af tider på fanen Tog, hvor 23:50 skrevet over 00:10 nu betyder 20 minutter tidligere. Køreplaner, der ikke kører over midnat, afviser stadig en start før 00:00.

## Version 0.9.4

### Ændringer

- **Et togs rute ændres ikke længere, når du ændrer et tidspunkt.** Rækkefølgen af togets stop fastlægges nu, når stoppene tilføjes, i stedet for at blive udledt af tiderne. Tidligere kunne en tidsændring (for eksempel nogle ekstra minutter på en afgang, efter at toget havde skiftet retning) flytte et stop til togets slutning, mens de senere stop blev stående uændret. Tider, der ikke længere stiger, rapporteres i stedet som en konflikt. Tidligere gemte planer nummereres én gang i den rækkefølge, stoppene kører nu.

## Version 0.9.3

### Ændringer

- **Fulde ugedagsnavne på tjenestens forside.** Forsiden på en lokomotivførertjeneste skriver nu altid ugedagene fuldt ud, også når banen bruger korte navne, da en forkortelse let overses, når siderne læses i bunkevis.

- **Tidtabelstrækninger vises i nummerorden.** Redigeringen af tidtabelstrækninger viser nu strækningerne i numerisk orden, så 2 kommer før 10.

- **Omnummerering af en tidtabelstrækning ødelægger ikke længere grafisk tidtabel.** Efter omnummerering kunne en strækning forblive synlig i den grafiske tidtabel, selv om dens felt var fjernet. Hvilke strækninger der vises, afhænger ikke længere af deres numre. Valget af strækninger i den grafiske tidtabel nulstilles én gang af denne opdatering.

## Version 0.9.1

### Ændringer

- **Køretøjernes kørsler i tjenester passer med toget.** I tjenester viser traktions- og vognsættabellerne ikke længere dage, hvor toget ikke kører, eller hvor tjenesten ikke køres. De viser nu kun de dage, køretøjet rent faktisk kører toget inden for tjenesten, som turnusrapporten allerede gjorde.

- **Tjenester, der køres på dage, hvor deres tog ikke kører.** En tjeneste, der køres en dag, hvor et af dens tog ikke kører, rapporteres nu som en konflikt.

- **Læsning og aflæsning af godsvognsæt.** Et vognsæt har nu valget *Bruges til* (persontrafik eller godstrafik). For et godsvognsæt kan et togafsnit afkrydses *Læs vognene?* eller *Aflæs vognene og bring godset tilbage?*, hvilket giver en note ved ankomst. Aflæsningsnoten nævner stationen, hvor vognene sidst blev læsset i vognsættets omløb, så et andet tog kan føre dem tilbage. Læsning og aflæsning kontrolleres: et godsvognsæt skal læsses før aflæsning, ikke læsses igen før det er aflæst og ikke stå læsset ved omløbets slutning.

## Version 0.9.0

### Ændringer

- **Manglende køretøjsnumre fremhæves i rapporten over bidragydere af køretøjer.** En enhed, der medbringes uden nummer, viser nu *Angiv!* i nummerkolonnen, som en manglende DCC-adresse allerede gjorde, så arket får nogen til at oplyse det. Antalskolonnen viser nu altid antallet af enheder, også når det er én.

- **Oprangering under det vognsæt, den hører til.** I tjenester står vognene nu lige under rækken, der nævner vognsættet, i stedet for under hele tabellen. Betegnelsen gentager ikke længere turnusnummeret.

- **Pænere turnuskolonne.** Turnus er nu venstrejusteret i rapporten over bidragydere af køretøjer.

## Version 0.8.9

### Ændringer

- **Store planer åbner hurtigere.** Vinduet vises nu, før planen kontrolleres for konflikter, i stedet for at
  fryse, til kontrollen er færdig. Konfliktikonet i topbjælken drejer, indtil det første resultat er klar.
  Selve kontrollen er desuden cirka fire gange hurtigere på en stor plan.

- **En besked, mens en rapport oprettes.** Rapporter, der tager tid at bygge, viser nu *Opretter rapporten …*, til de er klar.

- **Oprangering i tjenester.** Hvor et vognsæt angiver sine vogne, viser tjenesten dem nu i rækkefølge, ét rektangel pr. vogn, under vognsættets tabel.

- **Lange turnuskort deles på flere kort.** En turnus med flere tog, end der er plads til på ét kort, fortsætter nu på et andet kort. Det første kort slutter med *Vend for at fortsætte* i rødt, og det næste har *Fortsættelse fra forrige* i rødt lige over sit første tog.

### Rettelser

- **Stationens ekspeditionsliste håndterer ekspeditioner efter midnat.** De står nu blandt de små timer efter uret og vises på den session eller dag, der kommer efter den, toget startede på.

- **Noter begynder ikke længere med *Daglig:*, når de gælder alle sessioner eller dage, toget kører.**

## Version 0.8.7

### Ændringer

- **Et tog kan låses inde på et spor, så andre tog kan passere.** Sæt kryds i **Kan aflåses?** for et spor på et
  sted med godsudveksling, for eksempel et industriområde. Et tog, der holder der, regnes som indlåst og
  blokerer ikke længere strækningen, når stedet fjernstyres fra en bemandet station (indlåst fra ankomsten),
  eller når stedet har en låsenøgle, og toget tidligere på sin køretur har stoppet ved stationen, der
  opbevarer nøglen. Det er nok, at ét af kravene er opfyldt. Hvordan det gøres i driften, ligger uden for planen.

### Rettelser

- **Tog, der kører efter midnat, slipper ikke længere forbi konfliktadvarslerne.** Et tog, der står på et
  spor eller er på strækningen mellem to stationer efter midnat, har samme klokkeslæt som et, der er der
  tidligere samme døgn, men advarslerne så dem ikke sammen. Nu gør de det med hensyn til sessionerne: et
  tidspunkt efter midnat hører til næste dag, så to tog, der begge kører sessionerne 1, 3 og 5, advares ikke
  mod hinanden for et tidspunkt, der falder på sessionerne 2, 4 og 6.

## Version 0.8.6

### Ændringer

- **Tjenester viser sværhedsgrad.** På fanen **Tjenester** får en tjeneste, der er blevet graderet, en lille farvet
  måler ved siden af sin betegnelse: grøn for let, orange for middel og rød for erfaren. Hold musen over
  den for at se graden. En tjeneste, der ikke er blevet graderet, viser intet ikon.

## Version 0.8.5

### Ændringer

- **Bekræftelser vises midt i vinduet.** Spørgsmålet, før noget slettes, eller før en ny bane erstatter den
  nuværende, åbner nu i en boks midt på skærmen, så man ikke skal rulle for at finde den. Tryk på **Esc**
  for at annullere. Et klik uden for boksen annullerer også en enkel sletning, men ikke det udførlige
  spørgsmål ved sletning af et driftssted eller spørgsmålet ved udskiftning af banen.

## Version 0.8.4

### Nye funktioner

- **Rangerlokomotiver kan stationeres på et driftsted.** Under fanen **Driftssteder** åbnes en station, et
  industriområde eller et andet driftsted med **Rediger**, og der klikkes på **Tilføj rangerlokomotiv**. Et
  rangerlokomotiv står der til den rangering, stedet har brug for: det kører ikke i noget omløb og sættes
  aldrig på et tog. Det vises sammen med det øvrige rullende materiel under **Køretøjsejere**, hvor ejer,
  DCC-adresse og køretøjsnummer angives som for ethvert andet lokomotiv, og rapporten over medbragte
  køretøjer udskriver det på siden for den station, hvor det er stationeret.

## Version 0.8.3

### Ændringer

- **Rapporten Medbragte køretøjer er ordnet efter, hvordan hver side bruges.** Ordnet **Pr. ejer** viser
  en deltagers side det, vedkommende medbringer, efter den første køresession eller dag, det skal bruges,
  derefter efter station, afgang og spor. Ordnet **Pr. driftssted** viser en stations side køretøjerne efter
  spornummer, spor 2 før spor 10, og på hvert spor efter afgang.
- **Rækkernes farver er enklere.** På en ejers side er kun et køretøj, der ikke skal bruges den første
  køresession eller dag, lysegult. På en stations side er et køretøj, der ikke er i drift alle køresessioner
  eller dage, lysegult, når det starter en ulige køresession eller dag, og lyseblåt, når det starter en lige.
  Reserver er stadig lysegrå.
- **Lokomotiver og togsæt kan få et køretøjsnummer.** Under **Køretøjsejere**, **Rullende materiel**, kan hver
  ejer af et lokomotiv eller et togsæt angive nummeret på det køretøj, de medbringer, også reserverne, ved
  siden af dets DCC-adresse. Rapporten Medbragte køretøjer viser det lige efter omløbet. Et vognsæts
  numre er fortsat vognenes.

## Version 0.8.2

### Ændringer

- **Togene i en kategori kan omnummereres.** Ændr **Startnummer** på fanen **Togkategorier**, og klik på
  **Omnummerér** ved siden af. Alle tog i kategorien flyttes lige meget, så toget med det laveste nummer
  får det første nummer på eller over startnummeret. Intet tog skifter mellem ulige og lige, og huller og
  par af numre bevares: med startnummer 100 bliver tog 1, 2 og 3 til 101, 102 og 103. En rangerkategori
  har ingen ulige og lige numre, så dens første opgave får selve startnummeret.
- **En enhed, der hentes fra opstilling før sit første tog, skal sættes til opstilling efter sit
  sidste.** Hvor et lokomotiv eller et togsæt hentes fra opstilling, eller løftes på, før sit første tog i
  en køresession, viser valideringen nu en advarsel, medmindre det også sættes til opstilling, eller løftes
  af, efter sit sidste tog, hvor det vender tilbage til den station — i samme køresession, eller i en
  senere, når det kører i omløb over flere køresessioner.

## Version 0.8.1

### Ændringer

- **Et lokomotiv eller et togsæt kan sættes til opstilling eller løftes af mellem to tog.** Når du
  redigerer et togafsnit i et vognløb, angiver **Trækkraft før afgang** og **Trækkraft efter ankomst**,
  hvor trækkraften er: på sporet, på opstilling, eller løftet af anlægget, for eksempel op på et bord under
  en lang ventetid. Lokomotivfører og fjernstyringsleder får en bemærkning om at køre den til eller fra
  opstilling, eller at løfte den af eller på. Valget sættes også på togafsnittet før eller efter, så en
  afløftet enhed løftes på igen før næste tog. Hvor de to ikke stemmer overens, viser valideringen en
  advarsel.
- **Hent fra og Sæt på holdes i trit med togafsnittet før eller efter.** Vælger du det spor, køretøjerne
  sættes på efter ankomsten, henter næste togafsnit dem fra det spor, og omvendt. Hvor et spor er angivet
  i den ene ende, men ikke mødes i den anden, viser valideringen en advarsel.
- **Bemærkningerne om at hente fra eller sætte på et spor siger, hvad sporet bruges til.** Et spor med en
  anvendelse på fanen **Driftssteder** nævnes med den, som i »spor 5 (Remise)«. Et spor uden nummer nævnes
  kun med sin anvendelse.

## Version 0.8.0

### Ændringer

- **Fanen Tog kan vise kaldene ved ét driftssted.** På fanen **Tog** kan der nu vælges mellem **Pr. tog**
  og **Pr. driftssted**. Pr. driftssted viser alle tog, der kalder ved det valgte driftssted, i den
  rækkefølge de kommer dertil: om toget starter, slutter, holder eller kører igennem, hvor det kommer fra
  og fortsætter til, dets tider og dets spor. Vælg et andet spor i listen for at flytte toget dertil, uden
  at åbne hvert tog for sig. En konflikt ved driftsstedet, for eksempel to tog på samme spor på samme tid,
  markeres på rækken. Fanen husker den valgte visning og det valgte driftssted.
- **Et driftssted, som tog kalder ved, kan slettes.** På fanen **Driftssteder** er **Slet** ikke længere
  spærret, mens tog kalder der. I stedet får du først vist alt, hvad sletningen ville ændre, og du
  bekræfter eller fortryder. Togene mister deres kald der: et tog, der kører igennem, kører nu lige forbi,
  og et, der starter eller slutter der, starter eller slutter nu ved sit næste eller forrige kald. Hvor
  driftsstedet ligger mellem præcis to naboer, erstattes dets to sporstrækninger af én, der forbinder
  naboerne, lige så lang og med samme køretid som de to tilsammen, og køreplansstrækningerne gennem det
  kører over den i stedet. Sletningen afvises, med årsagerne listet, så længe en omløbsplan, en
  lokførertjeneste eller en godsstrøm starter eller slutter der, eller så længe et tog vender der eller
  kører igennem det som et forgreningspunkt.

### Rettelser

- **En rulleliste viser ikke længere et andet valg end det, der blev truffet.** Når valgmulighederne i en
  liste ændrede sig, men den valgte værdi ikke gjorde, kunne listen vise en anden post end den gemte.

## Version 0.7.9

### Ændringer

- **Togsammensætninger angiver videresendte vogne efter, hvor de kommer fra.** En godsstrøm med
  **Oprindelsesdriftssteder** vises nu som "Vogne fra" sine oprindelser i sit rektangel i stedet for med
  sine destinationer, så vognene kan findes efter, hvor de kommer fra. Andre godsstrømme på samme plads i
  toget viser stadig deres destinationer, og begrænsningen tæller stadig dem alle med.

## Version 0.7.8

### Ændringer

- **Der advares om et tog, der ankommer til eller afgår fra et spor, som ikke er planlagt.** Et tog med
  ankomst eller afgang på et spor, hvor feltet **Planlagt?** ikke er afkrydset på fanen **Driftssteder**,
  vises nu under **Konflikter** med tog, driftssted, tid og spor. Et tog, der blot venter på et sådant
  spor, for eksempel på en krydsning, vises ikke, og det gør en rangeropgave heller ikke. Intet ændres
  for dig: flyt enten standsningen til et planlagt spor på fanen **Tog**, eller sæt kryds i sporets felt
  **Planlagt?**. Kontrollen kan slås fra under **Indstillinger › Validering**.

## Version 0.7.7

### Ændringer

- **Køretøjer, der ikke er i drift, kan slettes.** På fanen **Køretøjsejere** har et køretøj, der vises som
  **Ikke i drift**, en knap **Slet**, og **Slet dem, der ikke er i drift** fjerner alle sådanne viste
  køretøjer — kun de viste, når **Kun dem uden ejer?** er afkrydset. Begge spørger først. Køretøjets ejere
  fjernes sammen med det; deltagerne bliver. Et køretøj med opgaver skal først tages ud af sine omløb på
  fanen **Omløb**.
- **Rapporten Medbragte køretøjer benævner hvert køretøj, som fanen Omløb gør.** Den separate kolonne
  **Klasse** er væk: kolonnen **Omløb** har nu operatørens signatur, nummer og klasse (f.eks. "SJ 01 Rc"),
  eller det eksterne id for et køretøj, der har et, som på fanen **Omløb**.

## Version 0.7.6

### Ændringer

- **Listen over køretøjsejere benævner hvert køretøj, som fanen Omløb gør.** På fanen **Køretøjsejere** er
  de separate kolonner **Køretøj** og **Klasse** nu én kolonne **Køretøj** med operatørens signatur, nummer
  og klasse (f.eks. "DB 05 BR 218"), eller for et vognsæt de vogne, det angiver (f.eks. "SJ 05 5 x A/B/Fv").
  Et køretøj med eksternt id vises med dette id, som på fanen **Omløb**.
- **Rapporten Medbragte køretøjer tegner vognene i et vognsæt.** Et vognsæt, der angiver sine vogne, viser
  dem nu i sin bemærkning, et rektangel pr. vogn med klasse og nummer, i den rækkefølge de står i toget,
  som rapporten **Togsammensætninger** tegner dem. Et langt vognsæt fortsætter på flere linjer.

## Version 0.7.5

### Ændringer

- **En ny tjeneste begynder med sit første togafsnit.** **Ny tjeneste** på fanen **Tjenester** åbner nu
  straks dialogen **Tilføj togafsnit**, så tjenesten med det samme får sin plads i diagrammet i stedet for
  at stå tom nederst. Lukkes dialogen, uden at et afsnit tilføjes, efterlades ingen tom tjeneste.
- **Tilføj tog tilbyder kun steder, hvor kategorien standser.** I dialogen **Tilføj tog** er fra- og
  tildriftsstederne begrænset til dem i den valgte togkategoris standsningsmønster. En kategori uden
  standsningsmønster begrænser intet. Skiftes kategorien, ryddes et driftssted, der ikke længere er blandt
  valgene.
- **Et døgnåbent træf kan begynde når som helst på døgnet.** Når **Kører over midnat?** er markeret på
  fanen **Indstillinger**, angiver **Første køresession starter** tidspunktet, hvor den første køresession
  eller dag begynder. Hvert køretøj starter der, hvor det står på det tidspunkt: et togafsnit på den første
  køresession, der afgår tidligere, er ikke der, hvor det starter, og et køretøj uden noget senere den
  køresession starter på den næste, det kører. Fanen **Køretøjsejere** og rapporten **Medbragte køretøjer**
  viser starten på denne måde.

## Version 0.7.4

### Ændringer

- **Et tjenestehæfte viser hvert tog én gang.** Hvor en tjeneste deler et tog op i flere afsnit — fordi
  trækkraftenheden skiftes, eller vogne kobles til eller fra undervejs — men lokomotivføreren bliver på
  toget hele vejen, udskriver hæftet nu toget én gang, fra hvor føreren tager det, til hvor føreren
  forlader det, i stedet for én side pr. afsnit. Blokkene for trækkraftenheder og vognsæt viser, hvilket
  køretøj der kører hvilken del af toget.
- **Godsvogne, der står sammen, er én række i et tjenestehæfte.** Godsstrømme, der kobles til på samme
  station på samme plads i toget, deler nu én række i blokken med godsvogne med fragtbreve, på samme måde
  som togsammensætningerne viser dem: hvert sted nævnt én gang, regionerne efter stederne og én største
  last for hele gruppen, summen af destinationernes. Rækkerne ordnes efter den station, hvor vognene kobles
  til, i den rækkefølge toget når frem, og derefter efter plads.
- **Togsammensætninger viser hele toget, hvor et vognsæt kobles til.** Et tog får stadig kun en række,
  hvor et vognsæt kobles til, eller godsstrømsvogne tages med, men rækken viser nu hvert vognsæt, toget
  afgår med, også dem, det allerede har med, så pladserne for dem, der kobles til dér, giver mening.
- **Togsammensætninger indrammer hvert vognsæt.** Et vognsæts omløb og dets vogne står nu sammen i én
  skraveret ramme, så de læses som én gruppe under ét omløb i stedet for, at omløbet læses som endnu et
  vognsæt ved siden af dem.
- **Én største last pr. godsrektangel i togsammensætninger.** Et godsrektangel med flere destinationer
  angiver nu summen af deres største last én gang, til sidst, i stedet for ét tal pr. destination.
- **En vogn, der tilføjes et vognsæt, er en personvogn.** En ny vogn i dialogen **Rediger køretøj** er nu
  fra starten en personvogn, ikke en godsvogn.

### Fejlrettelser

- **Tider efter midnat havner på det rigtige døgn.** På et anlæg med **Kører over midnat?** afkrydset
  havner en tid efter midnat, der indtastes på et tog, som kører over midnat — f.eks. 00:10 skrevet over
  23:55 — nu på det næste døgn, så togets ophold bliver stående i den rækkefølge, toget kører. Planer, der
  er gemt tidligere med sådanne tider, rettes, når de åbnes.

## Version 0.7.3

### Ændringer

- **Ankomstspor kan lægges, hvor næste tog afgår.** En ny knap for **ankomstspor** på hvert omløb på
  fanen **Omløb** flytter omløbets ankomster til det spor, næste tog afgår fra, uanset køretøj. Et omløb
  med togsæt eller vendetog rettes stadig af sig selv, og nu også når **Vendetog?** markeres på et
  lokomotiv, der allerede er tildelt.
- **Listen over driftssteder er lettere at arbejde i.** Knapperne for et driftssted står nu lige efter
  dets navn i stedet for yderst på den brede række, så det er tydeligt, hvilket driftssted de hører til.
  Et klik på selve navnet åbner informationen om driftsstedet. Hver anden række er skraveret, og rækken
  under musemarkøren fremhæves tydeligt.
- **En station viser de steder, den betjener med gods.** På fanen **Driftssteder** viser informationen
  om en station nu de steder, hvis gods betjenes fra den, og informationen om et sted viser, hvilken
  station der betjener det. De udskrevne ark for driftssteder viser også de betjente steder.
- **Togsammensætninger viser ankommende godsstrømsvogne.** Et tog, der ankommer med godsstrømsvogne, som
  kobles fra på en station, får nu sin egen række på stationens ark, med sin afgangstid, hvor toget kører
  videre, og med ét stiplet rektangel pr. plads i toget, der angiver, hvor vognene kom fra. Kun
  godsstrømme med **Kobl fra?** afkrydset vises, undtagen på en skyggestation, hvor alle ankommende vogne
  vises. Skyggestationer får nu også deres egne ark.
- **Togsammensætninger viser hvert vognsæt, hvor det kobles til.** Et vognsæt, der ikke angiver sine
  vogne, tegnes nu også, alene ved sit skraverede omløbsrektangel. Et vognsæt vises kun, hvor det kobles
  til: ved den første afgang i sit omløb og senere kun, hvor det udtrykkeligt kobles til — med en
  bemærkning om tilkobling, hentet fra et andet spor eller koblet til et tog, der allerede kører. Et
  vognsæt, der bliver ved sit lokomotiv fra tog til tog, vises ikke igen. Hvor et vognsæt står i toget,
  angives pr. tilkobling: **Plads** i dialogen **Rediger togafsnit** på fanen **Omløb**, så flere vognsæt,
  der kobles til på samme station, tegnes i den rækkefølge.
- **Togsammensætninger fylder mindre.** Destinationerne, og oprindelsesstederne for ankommende vogne, står
  nu i forlængelse af hinanden som en kommasepareret liste i deres rektangel i stedet for én pr. linje.
  Kolonnen **Omløb** er væk: hvert vognsæts omløb står i et skraveret rektangel foran dets vogne, hvilket
  giver sammensætningerne mere bredde. Kolonnen **Til** hedder nu **Til/fra** og angiver *til*, hvor et
  afgående tog skal hen, og *fra*, hvor et ankommende tog kom fra.
- **Togsammensætninger tegnes, som togene kører.** Hvert tog begynder nu med et lokomotivrektangel i den
  ende, det kører mod, med en pil, og vognene følger efter det, så rækkefølgen på papiret er rækkefølgen
  på sporet. Lokomotivet rummer dagene eller køresessionerne, toget og dets tider på stationen
  (**06:00-06:45**) og til sidst togets største last og erstatter fem kolonner. Et tog, der kører i sin
  banestræknings definerede retning, peger mod højre; et, der kører imod den, peger mod venstre. Hver side
  følges af sit spejlbillede til den anden side af sporene: udskriv dobbeltsidet, og vend arket til den
  side, der passer med det, du ser. Nabodriftsstederne nævnes i hver sin ende af sammensætningens
  overskrift.
- **Regioner sidst i togsammensætninger.** Hvor flere destinationer deler en plads i toget, angives alle
  stederne først og deres regioner efter dem, hver region én gang.

## Version 0.7.2

### Nye funktioner

- **Driftssteder kan nu udskrives.** En ny rapport under **Rapporter** giver hvert driftssted på
  anlægget sit eget ark på A4 stående: dets egenskaber, dets regioner, instruktionerne for, hvordan det
  betjenes på træffet, og dets spor med længder, perroner, ruter og anvendelse. Kun de egenskaber, der
  gælder for den slags driftssted, vises, og de driftstider, der vises, er dem, der gælder — driftsstedets
  egne, ellers anlæggets. Instruktionerne udskrives her for første gang. Et driftssted med mere, end der er
  plads til på ét ark, fortsætter på det næste, og det næste driftssted begynder stadig på et nyt ark. Med
  **Udskriv rapporter på lokale sprog** afkrydset udskrives hvert ark på sproget i driftsstedets land.

### Ændringer

- **Kun de driftstider, der gælder, tilbydes.** På fanen **Driftssteder** tilbydes tiden for at køre
  lokomotivet rundt kun på stationer, skyggestationer medregnet, og tiden for togklarering kun på
  bemandede stationer, da det kræver en togekspeditør på vagt at klarere et tog. En tid, der er angivet
  tidligere, hvor den ikke længere gælder, bevares, men hverken vises eller udskrives.

## Version 0.7.1

### Nye funktioner

- **Rapporter udskrives på banens sprog.** Alle rapporter udskrives nu på banens standardsprog, det
  første sprog i dens standardland, uanset hvilket sprog du selv arbejder på. De udskrevne ark læses
  af deltagerne på træffet, ikke af dig. Datoer og tal følger også landet.

  Sæt kryds ved **Udskriv rapporter på lokale sprog** under **Indstillinger › Generelt**, så udskrives
  hver førertjeneste på sproget for det selskab, der kører den, hvert omløbskort på sproget for
  køretøjets selskab og hver stations togekspeditionsliste på sproget for stationens land. Det gælder
  også togekspeditionslister, der gemmes som dokumenter. En tjeneste eller et kort uden selskab får
  sproget for togenes operatører, hvis de alle har samme sprog. Alt uden et sprog, som appen kan
  udskrive på, beholder standardsproget.

- **En togkategori angiver, hvor dens tog standser.** Fanen **Togkategorier** har et
  **standsningsmønster**: kryds ved de driftssteder, hvor kategoriens tog standser undervejs. Et nyt tog,
  der oprettes på fanen **Tog**, får en standsning ved hvert afkrydset driftssted, det passerer, og kører
  igennem de øvrige — der hvor toget begynder, og der hvor det slutter, er standsninger, uanset hvad der
  er krydset af, for det er der, det gøres klar og sættes væk. Kun de driftssteder, kategorien
  overhovedet kan standse ved, tilbydes.

  Et tog, der standser et sted, mønsteret ikke angiver, vises under **Konflikter** med tog, driftssted og
  tid. Intet rettes for dig: kun du kan afgøre, om det er toget, der standser, hvor det burde køre
  igennem, eller mønsteret, der mangler et driftssted, hvor kategoriens tog standser. Kontrollen kan slås
  fra under **Indstillinger › Validering**.

  **Hent fra togene** sætter kryds ved de driftssteder, hvor kategoriens eksisterende tog standser, og det
  gøres for dig, første gang en plan fra en tidligere version åbnes — hver kategori får det mønster, dens
  tog har kørt hele tiden, så intet rapporteres, som ikke var en fejl før. Sætter du ingen kryds, står
  kategorien ubundet: dens tog standser da alle steder, hvor de kan aflevere det, de fører med sig, som
  før. En rangerkategori har intet standsningsmønster, da dens opgaver ikke kører nogen steder.

- **En rangeropgave behøver intet lokomotiv af sin egen, men den behøver en lokomotivfører.** En opgave
  udføres lige så ofte af det toglokomotiv, der allerede står på stationen, eller af et rangerlokomotiv,
  du ikke har oprettet som køretøj, som af et, der er booket til den. Et **omløb**, der kun indeholder
  rangeropgaver, er derfor færdigt uden tildelt køretøj og vises ikke længere under **Konflikter** som et
  omløb uden køretøj. Læg et tog, der kører et sted hen, ind i det samme omløb, så kræves der køretøj
  igen.

  Hvad en opgave derimod behøver, er nogen til at udføre den, så den tilbydes nu på fanen **Tjenester**
  som ethvert andet togafsnit, med eller uden eget lokomotiv — skrevet med stationen én gang og de
  tidspunkter, arbejdet løber imellem, da den ikke kører nogen steder. En opgave, ingen tjeneste dækker,
  vises under **Konflikter** for de sessioner, den efterlades ubemandet, side om side med de afsnit, et
  lokomotiv trækker.

  I et trykt tjenestehæfte har opgavens side overskriften **Rangeropgave** med operatørens signatur, ikke
  et tognummer, ingen bruger, og hvor et tog har sin ruteplan, har en opgave sin egen blok:
  **Arbejdstider**, én linje med stationen og de tidspunkter, arbejdet **starter** og **slutter**, med
  rangerinstruktionerne nedenunder. Der angives intet spor — en opgave udføres over hele stationen, ikke
  fra ét spor.

- **Søjlerne for lokomotivførere regner med ventetiden i en tjeneste.** Søjlerne over den grafiske
  køreplan regnede kun en lokomotivfører som nødvendig, mens et tog eller en rangeropgave blev kørt. En
  lokomotivfører med et ophold mellem to tog i sin **tjeneste** er alligevel optaget imens, på vej til
  eller i venten på det næste, så den tid regnes nu også med — på de sessioner, tjenesten køres. Det gør
  også et starttidspunkt, du har angivet før tjenestens første tog, eller et sluttidspunkt efter dens sidste.

- **Tog på linjen kontrolleres, som togekspeditørerne ser det.** Listen **Konflikter** så før på én
  banestrækning ad gangen, så to tog kunne krydse på en ubemandet station, eller følge efter hinanden
  forbi en, uden at der blev sagt noget. Nu ser den på hver togledelsesstrækning som helhed. Et
  signalstyret sted, for eksempel en blokpost, deler togledelsesstrækningen i afsnit, der hver kan rumme
  ét tog pr. spor. På enkeltsporet bane kan tog i modsat retning kun mødes ved enderne eller på et
  signalstyret sted, hvor tog kan krydse. Tog i samme retning kan følge efter hinanden, ét pr. afsnit. Se
  **Togledelsesstrækninger** i hjælpen på fanen **Strækninger**.

- **En bemandet station kan fjernstyre en forgrening eller en ubemandet station.** Feltet **Styres fra**
  på fanen **Driftssteder**, der hidtil kun fandtes på signalstyrede steder, tilbydes nu også på
  ubemandede stationer og industriområder. Kun bemandede stationer tilbydes som styrende station. En
  fjernstyret forgrening, et krydsningssted, en ubemandet station eller et industriområde betjenes af den
  stations togekspeditør som sit eget: togledelsesstrækninger ender dér, dets tog står på den styrende
  stations togekspeditionsliste i tidsrækkefølge blandt stationens egne, med sin signatur foran sporet,
  og overskriften nævner det. Stationerne bag det er blandt dem, den styrende station ringer til, og de
  ringer til den styrende station. En blokpost, et signalstyret sted, der hverken er forgrening eller
  krydsningssted, forbliver en del af linjen. Tryk på **Gendan ud fra banestrækninger** på fanen
  **Strækninger** efter at have angivet en styrende station.

- **Angiv, hvor tog kan krydse.** Et signalstyret sted har et nyt afkrydsningsfelt **Tog kan krydse?** på
  fanen **Driftssteder**. Sæt kryds, hvor ét tog kan vente, mens et andet passerer. Antallet af spor kan
  ikke afgøre det, for en forgrening har to spor for at vide, hvilken vej et tog skal, uanset om tog kan
  krydse dér. Et signalstyret sted, der hverken er forgrening eller afkrydset, er en blokpost.
  Importerede steder starter uden kryds, så sæt kryds ved krydsningsstederne efter importen.

### Ændringer

- **Lokale destinationer nævnes ved navn.** En godsdestination med **Og lokale?** afkrydset lyder ikke
  længere *Stilkøbing og lokale destinationer*: den nævner stederne, *Stilkøbing, Vig, Rubjerg* —
  stationen efterfulgt af hvert sted, hvis gods betjenes derfra (**Godsbetjenes fra** på fanen
  **Driftssteder**). En station, der ikke betjener noget, nævnes alene. Udtrykket er også fjernet fra
  godsforklaringen i de generelle instruktioner, da intet udskriver det længere.

## Version 0.7.0

### Nye funktioner

- **Tabellen over rangerbanegårde kan nu placeres i de almene instruktioner.** Tabellen over
  rangerbanegårde udskrives på anlægssiden bagest i hæftet med de almene instruktioner. På et anlæg med
  mange rangerbanegårde fyldte den siden og skubbede forklaringen af godsstrømmenes mærker ud. Skriv

  ```
  <ShuntingYards/>
  ```

  på en linje for sig under **Indstillinger**, dér i teksten hvor den hører hjemme, for i stedet at
  udskrive den der. Anlægssiden udelader den så, så den aldrig udskrives to gange. Forhåndsvisningen ved
  siden af teksten viser en kasse, hvor tabellen kommer. Skrives den ingen steder, bliver tabellen på
  anlægssiden som før.

- **Køretøjsejere: hvem der medbringer hvilket rullende materiel til træffet, og hvor det skal stilles op.**
  Fanen **Køretøjsejere** viser hvert lokomotiv, togsæt og vognsæt — med antallet af enheder, hvor der er
  mere end én, og for et vognsæt, der angiver sine vogne, hver vognklasse én gang — med den første køresession (eller dag), det er i drift, og den station, det spor og den
  afgang, hvor det skal stå inden da. Åbn en række for at tilføje ejere: den første medbringer køretøjet og
  stiller det op på anlægget, øvrige ejere medbringer reserver. Vælg en ejer ved at skrive de første
  bogstaver i navnet — eller i efternavnet — så et navn staves ens overalt; et navn, der ikke passer til
  nogen, tilbydes som en ny deltager. Hver ejer af et lokomotiv eller togsæt, også reserverne, skal angive
  en DCC-adresse; skriv **0**, når ejeren endnu ikke har oplyst den. Hvert køretøj og hver ejer har en
  bemærkning, og visningen **Deltagere** viser alle med det, de medbringer, og der rettes et forkert stavet
  navn én gang for alle.

  Rapporten **Medbragte køretøjer** under **Rapporter** udskriver den samme liste på A4 liggende, ordnet på
  en af tre måder, som vælges over siderne: **Pr. driftssted**, en side pr. station til dens ejer med de
  køretøjer, der skal stilles op der, i rækkefølge efter første køresession og afgang; **Pr. ejer**, en side
  pr. deltager med det, vedkommende medbringer, DCC-adresserne og hvor hvert køretøj starter; eller **Efter
  DCC-adresse**, alle medbragte lokomotiver og togsæt i én liste. Hver station eller ejer begynder på en ny
  side og fortsætter på den næste, når én side ikke er nok. Køretøjer, der ikke er i drift, vises sidst;
  dem, ingen medbringer endnu, kommer først pr. ejer under **Endnu ikke booket**. Rækkens baggrund viser,
  hvornår enheden bruges: hvid, når den er i drift i alle køresessioner, lysegrå for en reserve og ellers
  lyseblå, lysegrøn eller lyserød, når køretøjet først er i drift fra første, anden eller tredje
  køresession.

- **Et omløb kan angive, at køretøjerne står på et andet spor end toget.** Når et togafsnit redigeres
  under **Omløb**, angiver **Hent fra** det spor, køretøjerne står på, før toget afgår, og **Sæt på**
  det spor, de sættes på efter ankomsten — for eksempel et vognsæt, der efterlades på et sidespor på
  en mellemstation. Tjenestehæfterne og togekspeditionslisterne udskriver det som en bemærkning: *Før
  afgang, hent vognsæt 21 fra spor 3.* ved afgangen og *Efter ankomst, rangér vognsæt 21 til spor 3.*
  ved ankomsten, i stedet for bemærkningen om at koble køretøjet til eller fra. Hvor køretøjet kun
  kører med toget i nogle af de køresessioner eller dage, toget kører, indledes bemærkningen med dem —
  *1,3,5: Før afgang, hent …* — og et køretøj, der ikke kører med toget i nogen af dem, får ingen
  bemærkning. Et spor, som et omløb bruger på den måde, kan ikke slettes under **Driftssteder**.

- **Togsammensætninger kan nu udskrives.** En ny rapport under **Rapporter** giver hver bemandet station sin
  egen side på A4 liggende med alle tog, der afgår derfra med godsstrømsvogne eller med et vognsæt, der
  angiver sine vogne. Togene vises spor for spor og i afgangsrækkefølge på hvert spor. Ved siden af hvert tog
  tegnes dets sammensætning forfra som rektangler: et vognsæt som ét rektangel pr. vogn, i vognrækkefølge,
  med vognens litra og nummer; og godsstrømsvogne som ét rektangel pr. plads i toget med, hvor de skal hen —
  med *og lokale destinationer* og *og videre*, hvor godsdestinationen angiver det, og dens regioner i deres
  farver. Sammensætningen er den, toget afgår med, så vogne, det ankom med, vises lige så vel som dem, der
  kobles til på stationen. Et vognsæt, der kun kører med toget i nogle køresessioner eller dage, mærkes med
  dem, og godsstrømsvogne uden plads vises sidst som *Hvor som helst i toget*.

- **Et motorvognstog ankommer nu til det spor, det skal afgå fra.** Et motorvognstog — og et lokomotiv i
  vendetog — kører aldrig rundgang: det afgår fra netop det spor, det ankom til. Når et sådant køretøj
  kører tog efter tog, lægges hvert togs ankomstspor derfor på det spor, næste tog afgår fra, og — slutter
  omløbet, hvor det begyndte — lægges sidste togs ankomstspor på det spor, første tog afgår fra, så
  køretøjet står klar, hvor næste køresession henter det. Kun ankomstspor flyttes; det spor, et tog afgår
  fra, står, som du har angivet det. Et omløb, der trækkes af et almindeligt lokomotiv, røres ikke, for
  lokomotivet kører alene over stationen til det spor, næste tog står på. Sporene rettes, hver gang et tog
  føjes til et omløb — af **Byg automatisk** eller af dig, sidst i omløbet eller dér, hvor køretøjet holder
  — og hver gang du tildeler et køretøj til et omløb, så et omløb, der er bygget, før motorvognstoget var
  kendt, rettes, så snart motorvognstoget sættes på det. Et importeret omløb beholder de spor, det blev
  indlæst med. Ville flytningen stille to tog på samme spor samtidig, står det blandt konflikterne, du kan
  løse.

### Ændringer

- **Dage og køresessioner angives uden mellemrum.** Hvor en bemærkning eller en kolonne angiver, hvilke
  dage eller køresessioner noget gælder, skrives de nu *M,O,F* og *1,3,5* i stedet for *M, O, F* og
  *1, 3, 5*, så angivelsen fylder så lidt som muligt. Dage, der skrives helt ud — *Mandag, Onsdag,
  Fredag* — er uændrede.

- **Et vognsæt, der angiver sine vogne, viser dem nu i sin etiket under Omløb.** Etiketten angiver antallet
  af vogne og hver vognklasse én gang — *SJ 05 5 x A/B/Fv* — i stedet for vognsættets egen klasse.

- **En bemærkning ved et ophold angiver nu, om den hører til ankomsten eller afgangen.** Tjenestehæfterne og
  togekspeditionslisterne udskriver et opholds ankomst og afgang på hver sin linje, så under **Tog** spørger
  feltet ved siden af hver **Bemærkning**, hvilken af dem den gælder: **Ank** for noget, der mødes eller skal
  gøres ved indkørslen, **Afg** for noget, der skal gøres før eller ved afgangen. Hvor toget kun ankommer
  eller kun afgår, er kun den ene at vælge. Hvor det kører igennem, kan der ikke skrives nogen bemærkning —
  sæt først kryds i **Ank** eller **Afg** — men en, der allerede står der, kan stadig fjernes.

  En bemærkning, der blev skrevet med en tidligere version eller fulgte med en XPLN-import, angav ingen af
  delene og blev derfor hverken udskrevet i hæfterne eller i listerne. Første gang en plan åbnes, får hver
  sådan bemærkning afgangen, hvor toget afgår, og ankomsten, hvor det kun ankommer.

- **Hver blok på en togside i et tjenestehæfte har nu en farvet streg langs venstre kant.** Trækkraftenheder
  er markeret med rødt, planlagte vognsæt med grønt, godsvogne med fragtbreve med blåt og ruteplanen med gråt,
  så blokkene kan skelnes med et blik, og hvor en grå streg slutter, slutter det togafsnit. Stregerne
  udskrives, uden at baggrundsgrafik skal slås til, og hver blok beholder sin overskrift, så en udskrift i
  sort-hvid mister intet.

- **Trækkraftenheder og planlagte vognsæt på en togside i et tjenestehæfte viser nu deres spor.** Hver række
  angiver det spor, køretøjet står på ved starten, og det spor, det efterlades på ved slutningen — togets
  eget spor eller det, omløbet angiver under **Hent fra** eller **Sæt på**. Kolonnen, der angiver køretøjet,
  har nu overskriften **Omløb** i begge blokke, efter det kort, køretøjets identitet står på. Med sporene i
  tabellen angiver ruteplanen nedenfor ikke længere hvert vognsæt og dets spor: den siger *Rangér vogne til
  afgangssporet før afgang.* eller *Rangér vogne til deres ankomstspor efter ankomst.*, indledt med de
  køresessioner eller dage, det gælder, når vognene kun rangeres i nogle af dem, toget kører.
  Togekspeditionslisterne angiver stadig hvert vognsæt og dets spor.

- **Hæftet med generelle instruktioner forklarer nu, hvordan gods skrives i de andre rapporter.** Under
  **Godsstrømme** på hæftets sidste side siger en forklaring, at hver lastgrænse er et maksimum, og hvad
  mærket efter et tal tæller, hvad globussen står for, hvad *og lokale destinationer* og *og videre* føjer til
  en destination, og hvad et farvet regionsnavn betyder.

- **Forsiden af hæftet med generelle instruktioner har plads til et længere program.** Programmet sættes med
  mindre afstand mellem linjerne, punkterne og dagsoverskrifterne — skriftstørrelsen er den samme — hvilket
  giver plads til fire eller fem punkter mere. Et program, der er for langt til siden, mister sine sidste
  punkter, og de er slutningen på træffet, netop det, folk slår op.

- **Byg automatisk udvider nu de omløb, du allerede har, før det laver nye.** Togene, der endnu ikke er i
  et omløb, tilbydes først de eksisterende omløb: et omløb fortsætter med det, der fortsætter det, hvor
  det ankommer, i den kategori, det allerede kører i, og et tomt omløb, du selv har lavet, fyldes, før et
  nyt oprettes. Kun de tog, der ikke passer ind i noget eksisterende omløb, starter nye omløb, så når du
  bygger igen efter at have tilføjet nogle tog, forlænges de køretøjer, der allerede kører, i stedet for at
  sætte nye i drift. Godsstrømme lades være, som de er. Resultatet ved siden af knappen angiver nu, hvor
  mange togafsnit de eksisterende omløb fik, hvor mange af dem der blev forlænget, og hvor mange omløb der
  blev bygget.

### Fejlrettelser

- **Forklaringen af godsstrømmenes mærker løber ikke længere ud af hæftet med de almene
  instruktioner.** Formuleringerne *og lokale destinationer* og *og videre* samt forklaringen af en
  region stod i en så smal kolonne, at hver af dem fyldte fire linjer, og på et anlæg med flere
  rangerbanegårde faldt den sidste af dem ud under sidens bund. Mærkerne og formuleringerne sættes nu
  som to lister under hinanden, hver i hele sidens bredde.

- **En ruteplan, der fortsætter på modstående side i et tjenestehæfte, ser nu ud som alle andre.** Når et
  togafsnit er for langt til én side, flyttes dets ruteplan til modstående side, og der blev den udskrevet
  uden hæftets eget layout: med større skrift, uden de fede stationer og tider og uden stregerne mellem
  opholdene, så en lang ruteplan kunne løbe ud over sidens bund.

- **At ændre et togs nummer eller kategori under Tog efterlader ikke længere ændringen på en anden linje.**
  Begge ændringer sorterer listen om, og det nummer, du skrev, eller den kategori, du valgte, kunne blive
  stående på linjen for det tog, der rykkede ind på dets plads.

- **Dialogerne læser nu et tal, mens du skriver det.** **Varighed (minutter)** for en ny rangeropgave,
  **Minutter** at flytte eller kopiere tog med og et køretøjs **Nummer** under **Omløb** blev først læst, når
  du forlod feltet, så knappen, der bekræfter dialogen — og advarslen om, at et køretøjsnummer allerede er
  optaget — haltede bagefter, indtil du klikkede et andet sted.

- **Den grafiske køreplan tegner nu et driftssteds spor i den rækkefølge, du har givet dem.** Den så bort fra
  sporenes **Rækkefølge** under **Driftssteder** og kunne derfor tegne dem i en anden rækkefølge end alle
  andre sporlister i appen.

- **Et omløbskort for et køretøj, der kører på dage, som ikke følger efter hinanden, nævner nu dagene.** Et
  kort for mandag, onsdag og fredag udskrev *MondayShort,WednesdayShort,FridayShort* i stedet for *M,O,F*.

## Version 0.6.0

### Ændringer

- **Tjenestetog er en ny slags togkategori.** Giv en togkategori typen **Tjenestetog** under
  **Togkategorier** for tog, der hverken afleverer eller optager noget, hvor de standser: et arbejdstog,
  eller et lokomotiv eller togsæt, der køres ud af drift. Et sådant tog må standse, hvor en driftsplads
  hverken udveksler rejsende eller gods — for eksempel en arbejdsplads — og når dets køreplan bygges, får
  det ingen ophold mellem sine endepunkter, så det ophold, der betyder noget, lægger du selv ind. Navngiv
  kategorien efter, hvad togene gør: et tog, der efterlader materialevogne, udveksler gods og hører
  hjemme i en godskategori.

  En kategori i en plan lavet med en tidligere version, som hverken var person eller gods — hvilket en
  XPLN-import kan efterlade — vises nu som et tjenestetog, hvor den før blev vist som et persontog.

- **Rangeropgaver er en ny slags tog.** Giv en togkategori typen **Rangeropgave** under
  **Togkategorier**, så udføres togene i den kategori på én station i et bestemt tidsrum i stedet for at
  køre nogen steder: hvert af dem har kun ét ophold, hvor ankomsttiden er, når arbejdet begynder, og
  afgangstiden, når det slutter.

- **Godsstrømmene i en rangeropgave angiver, hvilke vogne der skal rangeres.** Tilføj godsstrømme til
  opgaven under **Godsstrøm** på samme måde som til ethvert andet godstog. En strøm med opgavens egen
  station som destination indeholder vogne, der er ankommet, og lokomotivføreren får besked på at
  rangere dem ud til godskunderne med angivelse af, hvor de kommer fra. En strøm til et andet sted
  hentes i stedet ind fra godskunderne med angivelse af, hvor vognene skal hen. Instruktionen udskrives
  i lokomotivførernes turhæfter og i stationsrapporterne.

- **Passagerbilletter kan nu udskrives.** En ny rapport under **Rapporter** giver en returbillet mellem
  hvert par af driftssteder med passagerudveksling, foldet på midten og med den operatør, der har flest
  persontogsafgange fra salgsstedet, nederst på begge halvdele.

- **Køreplansrapporten lægger nu flere strækninger på ét ark.** Tabeller, der er for smalle til at fylde
  bredden, står ved siden af hinanden, så en kort sidebane ikke længere tager et helt ark for sig selv.

- **De grafiske køreplaner kan nu udskrives.** En ny rapport under **Rapporter** tegner hver strækning i
  den faste papirskala, der indstilles under **Indstillinger → Grafisk køreplan**, så tider og hældninger
  kan måles fra det ene ark til det næste.

- **Indstillinger → Grafisk køreplan er nu ordnet efter, hvad hver indstilling påvirker.** Det, køreplanen
  viser, kommer først, og under det afstandene på skærmen, i billedpunkter, ved siden af dem på papiret, i
  millimeter.

- **Du kan nu angive, hvad der skal ske med lokomotivet, hvor et togafsnit slutter.** Når du redigerer et
  togafsnit under **Omløb**, spørges der, om lokomotivet skal drejes, og om det skal køres om til den
  anden ende, og hver af dem udskrives som en ankomstbemærkning for lokomotivfører og fjernstyringsleder.

- **Topologi-diagrammet tegner nu hele anlæggets spor, med hvert driftssted vist én eneste gang.** Sporet
  er enkelt- eller dobbeltsporet, som strækningen virkelig er, og i farverne på de køreplansstrækninger,
  der går over det — gråt, hvor ingen strækning dækker det.

- **Du kan nu selv arrangere Topologi-diagrammet.** Træk et driftssted hen, hvor det hører til, så følger
  sporene med; det, du arrangerer, gemmes med planen og udskrives i tjenestehæfterne.

- **En godsdestination med grænse for både vogne og aksler viser nu begge.** Vogntallet forsvandt før,
  hvor der også stod et aksletal — både i tjenestehæfterne og i godsbemærkningerne — selv om de to felter
  står side om side under **Godsstrøm**, og hver af grænserne kan være den, der binder: seksten aksler er
  fire bogievogne, men otte toakslede.

- **Togsiderne i et tjenestehæfte siger nu det samme på mindre plads.** Kolonnen med køresessioner har
  nu overskriften **Kører** — det, den fortæller om køretøjet — i stedet for et langt ord over en kolonne
  med cirkler, og godsvognene har overskrifterne **Fra** og **Til**, som køretøjerne ovenfor allerede
  havde. Begrænsningerne under overskriften skrives som tal under et enkelt **Maks**: hastigheden med sin
  enhed, tallet med en cirkel efter for aksler, en firkant for vogne og længden som *2,5m*. Hvor mange vogne eller aksler
  en destination kan tage, er flyttet ud af **Til** til sin egen kolonne **Maks**, hvor det læses lige ned
  ad siden i stedet for til sidst i en række stednavne — og den kolonne vises kun, når noget på siden
  overhovedet er begrænset.

- **Knapperne, der gælder et helt omløb, står nu i deres egen kolonne.** Under **Omløb** er de flyttet til
  en kolonne **Handlinger** mellem køretøjerne og togene, så hver rækkes tog begynder samme sted.

- **Rapportmenuen har en ny rækkefølge**, fra de generelle instruktioner til passagerbilletterne.

### Fejlrettelser

- **Den installerede app virker nu uden internetforbindelse.** Hjælpeteksterne, teksterne under Om
  og Versionsnyheder samt kataloget med færdige togkategorier blev hentet fra nettet, hver gang de
  blev vist, og stod derfor tomme, når du var uden forbindelse. De gemmes nu sammen med resten af
  appen, når den installeres.

## Version 0.5.1

### Ændringer

- **Hvad der skal gøres med lokomotiverne, står nu i førernes tjenestehæfter og i
  togekspeditionslisterne.** Hvilket lokomotiv der skal bruges, hvad der skal kobles til og fra, og at det
  skal hentes fra — eller køres tilbage til — opstillingssporet, blev hele tiden regnet ud af
  materielomløbene, men aldrig skrevet ud; nu står de blandt de øvrige noter ved den standsning, de hører
  til, og både føreren og togekspedienten ser dem. Nyt blandt dem er beskeden til et lokomotiv, der skal
  køres rundt til den anden ende af toget, eller vendes, før toget kører tilbage.

- **Hæftet med generelle instruktioner udskriver nu hele din tekst, på sider der kan læses.** En side blev
  regnet for rummeligere, end den faktisk er, så det der løb forbi bundkanten faldt stiltiende bort;
  teksten fortsætter nu på den næste side i stedet, og en side slutter aldrig med en overskrift alene.
  **Topologi** og **Rangerbanegårde** kommer nu på hæftets allersidste side ligesom i tjenestehæfterne, og
  programmet på forsiden er sat i hæftets egne størrelser i stedet for browserens.

## Version 0.5.0

### Ændringer

- **Et vendetog står ikke længere og venter på lokomotivrundgang.** Sæt kryds i den nye boks **Vendetog?**
  på et lokomotiv under **Omløb**, hvor det fremfører et tog, der kan køres fra begge ender — et tog med
  styrevogn eller endnu et lokomotiv i den anden ende — så regner **Opdatér tider** rundgangen fra og lader
  toget stå det korteste ophold i stedet, hvilket fremrykker alle følgende ophold. Et motorvognstog
  behandles på samme måde uden noget at sætte kryds i, og et ophold, du bevidst har gjort længere, bliver
  stående, som du har sat det.

- **Et spor kan nu angive, hvilken vej gennem driftsstedet det er beregnet til.** Hvert spor kan angive det
  **forrige** driftssted, et tog kommer fra, det **næste**, det fortsætter til, eller begge — med feltet
  **begge retninger** — og et nyt tog lægges på det spor, der passer bedst til dets vej. Det er netop, hvad
  en **dobbeltsporet strækning** har brug for: giv de to spor samme par driftssteder omvendt, så holder
  hver retning sig til sit spor. Hvor to spor passer lige godt, tager et persontog, der standser, et spor
  med perron, mens et tog, der kører igennem, tager hovedsporet; lad kolonnerne stå tomme, så ændres intet
  i forhold til før.

- **Et tog kan nu kopieres i modsat retning og gentages.** Sæt flueben i **Modsat retning?**, så kører
  kopien strækningen baglæns, med alle køretider og ophold bevaret, forberedelses- og afslutningstiden
  byttet ende og et nummer fra den modsatte retnings række. Kopidialogen har nu også valget **Gentag tog**,
  så et tog kan oprettes for sig, justeres, til det kører, som det skal, og først derefter gentages hen
  over dagen.

- **Et spor kan nu angive, hvor lang dets perron er.** Hvert spor på et driftssted, der udveksler
  passagerer, har en **perronlængde** i meter — over nul betyder, at passagerer kan stige på og af der — og
  et nyt passagertog lægges på et spor med perron, hvor driftsstedet har en. Sæt flueben i
  **Passagerer?**, så får hvert spor en perron på én meter, som du kan justere, og en plan oprettet før
  dette behandles på samme måde, første gang den åbnes, så den fungerer nøjagtig som før, indtil du
  afkorter eller nulstiller de spor, der i virkeligheden ingen perron har. Et passagertog, der standser for
  passagerudveksling ved et spor uden perron, står nu under **Konflikter**: giv enten sporet en
  perronlængde eller fjern fluebenene i standsningens **Ank** og **Afg**, hvilket siger, at toget intet
  udveksler der. Kontrollen kan slås fra under **Indstillinger › Validering**.

### Fejlrettelser

- **At give anlægget et nyt navn ændrer nu navnet alle de steder, det vises.** Forsiden på hæftet med de
  generelle instruktioner, navnet i den øverste linje og filnavnet, en plan gemmes under, blev alle ved med
  at vise, hvad anlægget hed før. En plan, der har fået nyt navn tidligere, rettes næste gang den åbnes.

## Version 0.4.2

### Ændringer

- **Nu kan et tog sættes ind midt i et omløb.** Mellem togafsnittene på en række er der nu små samlinger,
  der viser, hvor køretøjet holder og hvor længe, og før det første afsnit en, der viser, hvorfra det skal
  hentes; klik på en af dem for at sætte et tog ind i hullet, så tilbydes kun de tog, køretøjet faktisk kan
  nå. En tur, der ikke bringer køretøjet tilbage, sættes ind alligevel og rapporteres som en konflikt,
  indtil du sætter returen ind — sådan passes en tur-retur ind i et ophold. En samling, hvor omløbet er
  brudt, som en import kan efterlade det, er markeret med gult.

- **Appen har fået sit eget ikon** — fronten af et moderne tog på en mørkeblå flade — i stedet for mærket,
  der følger med de værktøjer, den er bygget med. Ikonet ses i browserens faneblad og på hjemmeskærmen
  eller i Start-menuen for den, der installerer appen.

- **Der er nu plads til tolv omløbskort på et ark i stedet for ti.** Kortene er 48 mm brede i stedet for
  50, så seks kan være ved siden af hinanden på et liggende A4-ark, og arket har stadig en margen, som
  almindelige printere kan nå. Kortene er lige så høje som før, og indholdet er uændret.

- **Rækkerne i køreplanen står nu længere fra hinanden.** Der er nu en syvendedel mere luft omkring hver
  linje, så en række er lettere at følge tværs over siden, og en station lettere at finde i kolonnen.
  Skriften og kolonnerne er uændrede, så bladet rummer de samme tog; en side tager nu niogtredive linjer i
  stedet for femogfyrre.

### Fejlrettelser

- **Den udskrevne køreplan mister ikke længere de sidste rækker på en side.** Begge retninger af en
  strækning blev sat på samme side, også når de ikke begge kunne være der, og rækkerne, der blev til
  overs, blev klippet af — rapporten på skærmen var sat i en større skrift end den udskrevne, så dens
  rækker var næsten to tredjedele højere end dem, der blev talt. De to sættes nu ens, hvor meget der kan
  være måles på en virkelig side i stedet for at blive regnet ud fra skriftstørrelsen, og tre linjer holdes
  frie nederst på hver side.

- **Godsstrømslisten nævner nu de destinationer, vognene skal til.** Under **Godsstrøm › Godstog** stod der
  kun "Vogne til" i listen, man vælger fra, uden destinationerne, så posterne ikke kunne skelnes fra
  hinanden. Underfanen og dens kolonne hedder nu **Godsdestinationer** i stedet for *Godsbeskrivelser*.

## Version 0.4.1

### Ændringer

- **Togekspeditionslisterne kan nu gemmes som dokumenter, stationsejerne kan redigere.** Vælg
  *Togekspeditionslister* i menuen Eksportér, så får hver bemandet station sit eget dokument i
  OpenDocument-format, tænkt til at sende hver ejer deres egen liste før træffet, så de kan tilføje de
  lokale instruktioner, kun de kender; er mere end én station bemandet, kommer dokumenterne samlet i en
  zip-fil. Hvor siderne brydes, er overladt til tekstbehandleren, så siderne brydes fornuftigt også efter,
  at ejeren har skrevet — stationens navn, telefonnumrene til de stationer, den ekspederer tog til og fra,
  og kolonneoverskrifterne gentages øverst på hver side, men den del af døgnet, en side dækker, kan ikke
  angives, så siderne nummereres i stedet. De udskrevne ark i menuen Rapporter er uændrede og er fortsat
  dem, man arbejder fra under en køresession.

- **Et tog, der trækkes af to lokomotiver på én gang, siger nu hvilke to.** Konflikten nævnte kun toget og
  minutterne, så var begge booket over nøjagtig samme strækning, lød dens to halvdele ord for ord ens. Den
  markeres nu også kun på de to omløb, der holder det dobbeltbookede arbejde, i stedet for på hvert omløb,
  der kørte det tog et sted på dagen.

- **To lokomotiver, der deles om et tog mellem køresessioner, rapporteres ikke længere som en konflikt.**
  Kun klokkeslættene blev sammenlignet, så et lokomotiv på ulige køresessioner og et andet på lige — hele
  pointen med at lægge det sådan an — blev rapporteret som dobbelttrækning. Nu rapporteres det kun, hvor
  begge er booket på en fælles køresession, og konflikten nævner de køresessioner.

## Version 0.4.0

### Brydende ændringer

- **Et køretøj, du opretter, identificeres nu af sin operatør og sit nummer.** På én og samme køresession
  må kombinationen kun tilhøre ét køretøj, uanset hvilken slags køretøj det er, så et vognsæt og et
  lokomotiv kan ikke længere begge være *DB 5*; et køretøj uden operatør identificeres af nummeret alene,
  og to køretøjer må dele identitet, så længe de køresessioner, de kører, ikke overlapper. Et
  **importeret** køretøj identificeres fortsat af det eksterne id, det blev importeret med, så en
  importeret plan giver ingen nye konflikter af dette. At tilføje eller rette et køretøj afviser nu en
  identitet, som et andet køretøj allerede har, og kræver et nummer, mens eksisterende planer bevares
  præcis som de er, med hvert køretøj, der deler identitet, blandt konflikterne.

### Ændringer

- **Der er en ny rapport: togekspeditionslisten.** Et sæt ark for hver bemandet station med de tog,
  stationen ekspederer, i tidsrækkefølge — et tog, der holder der, optræder to gange, ankomster på hvid
  baggrund og afgange på lysegul, fordi det at ekspedere et tog ind og at ekspedere det videre er to
  forskellige handlinger, og tog, der blot kører igennem, er også med. Hver side har stationens navn, den
  del af døgnet siden dækker, og telefonnumrene til stationerne i den anden ende af
  togekspeditionsstrækningerne, og hver række har et felt pr. køresession til at krydse af. Hver station
  begynder på en ny side, så bunken kan deles og uddeles; udskrives fra menuen Rapporter.

- **Felterne til at tilføje og rette et køretøj har fået ny rækkefølge,** den samme begge steder:
  køretøjstype, trækkrafttype, antal enheder, operatør, nummer, klasse, køresessioner og til sidst det
  eksterne id. Feltet, der før hed *Selskab*, hedder nu *Operatør*.

- **Et eksternt id kan rettes, men ikke længere opfindes.** Det eksterne id er det navn, et tog eller et
  køretøj bærer i det system, det blev importeret fra, så det, der er importeret med et id, har stadig sit
  felt og kan rettes der, mens det, der aldrig har haft et id, nu intet felt har at skrive i. Et køretøj,
  du opretter i planlæggeren, får derfor slet intet eksternt id, hvor det før fik et opdigtet af klasse og
  nummer.

- **Den mindste tid mellem to anvendelser af samme spor kontrolleres nu.** Indstillingen fandtes, men intet
  brugte den: står den på 0, hvor den begynder, ændres intet i kontrollen. Sæt den til 5, og sporet skal
  desuden være frit i fem minutter mellem to tog — præcis fem er nok, fire er ikke — og konflikten angiver,
  hvor kort mellemrummet faktisk er, og hvor langt det skulle være.

- **Et driftssted kan nu have sine egne instruktioner.** Redigeringsformularen har feltet
  **Instruktioner**, skrevet i Markdown ved siden af en forhåndsvisning, til hvordan netop det driftssted
  køres på dette træf: hvilke spor der bruges til hvad, hvordan rangeringen er tilrettelagt, og hvad
  lokoførerne og dem, der bemander stedet, ellers har brug for at vide. Feltet tilbydes på en station eller
  et industriområde og vises i driftsstedets Info-visning; det tilbydes ikke, hvor der intet er at
  instruere om.

- **Et sted, hvor der køres gods uden bemanding, kan nu kræve en nøgle.** Vælg den bemandede station, der
  opbevarer nøglen, under **Nøgle opbevares på**, og navngiv nøglen, hvis stationen opbevarer flere — et
  godstog, der standser begge steder, får da ved afgangen beskeden *hent nøgle A1 til oplåsning af Bruket*
  og ved næste standsning der *aflever nøgle A1 fra Bruket*. Nøglen hentes ved den sidste standsning før
  arbejdet og afleveres ved den første derefter, og et tog, der blot kører forbi, får ingen besked. Markér
  stedet som bemandet, eller tag bemandingen af den station, der opbevarer nøglen, så holder nøglen op med
  at gælde — **Konflikter** fortæller, hvilken ændring der gjorde det, og nøglen bevares, så den gælder
  straks igen, hvis du fortryder ændringen.

### Fejlrettelser

- **To strækninger, der udgår fra samme driftssted, blev tegnet, som om de aldrig mødtes.** Begyndte en
  køreplanstrækning netop på det første driftssted på en anden, var der ingenting, der bandt de to sammen i
  Topologi-diagrammet. Den anden forlader nu det driftssted som enhver anden gren, i samme faste vinkel.

- **Hver grænseværdi for kontrollerne angiver nu, hvilket ur den måles efter.** Den mindste tid mellem to
  anvendelser af samme spor manglede helt en enhed, og de to toghastigheder angav kun *ur-minutter*. Alle
  tre angiver nu hurtigursminutter — det ur, togene kører efter, ikke virkelig tid.

- **Længder og distancer skrives nu ud i meter,** ligesom tælleren i toghastighederne, så *m* ikke kan
  tages for et minut. Mindste ophold på en station angives nu også i hurtigursminutter.

## Version 0.3.5

### Fejlrettelser

- **En gemt plan kunne nægte at åbne.** At åbne en plan, som appen lige havde gemt, blev afbrudt med en
  fejl om et land, og der blev ikke indlæst noget. En allerede gemt plan åbnes, som den er; du behøver ikke
  gøre noget ved den.

- **En gemt planfil er omkring syv gange mindre.** Gemningen skrev planen i en anden form end den, der
  holdes i browseren, så hvert ophold blev skrevet to gange, og hver togkategori, hver operatør og hvert
  land igen ved hvert tog, hvert køretøj og hver tjeneste, der brugte det. En fil, der fyldte 8 MB, fylder
  nu godt 1 MB; en plan gemt af en tidligere version kan stadig åbnes.

## Version 0.3.4

### Ændringer

- **Felterne Ank og Afg på et stop følger nu, hvor toget faktisk kan standse.** Et persontog har brug for
  et driftssted, der tager imod passagerer, og et godstog et, der tager imod gods, og ingen af delene kan
  lade sig gøre på et signalstyret driftssted; hvor toget ikke kan standse, vises begge felter tomme og kan
  ikke sættes, og stoppet er en gennemkørsel. Intet af det, du har planlagt, smides væk — slå udvekslingen
  til igen, så er stoppene der — og en skyggebanegård har altid udveksling af både passagerer og gods, da
  den repræsenterer alt uden for anlægget.

- **Et stop, som noget afhænger af, kan ikke længere fjernes.** Togets eget første og sidste stop, og
  enderne på hvert togafsnit, som et materielomløb, en tjeneste eller et godsflow er planlagt over,
  beholder nu deres felt sat og låst, og holder du markøren over det, fortælles det, hvad der holder det.
  Hvor et togafsnit slutter et sted, toget ikke kan standse, siges det ligeud, så du kan flytte stoppet
  eller togafsnittet.

- **En togkategori bærer nu de forberedelses- og afslutningstider, dens tog planlægges med,** så du ikke
  længere skal skrive de samme to tal for hvert tog. Ved siden af hvert felt er der en knap *Anvend igen*,
  som giver den ene tid til alle de tog, kategorien allerede har, og fortæller hvor mange der blev ændret;
  de to er hver sin handling, og at anvende igen flytter kun minutterne yderst på et tog.

- **Operatørerne er lettere at læse på forsiden af et tjenestehæfte.** Linjen sættes nu i dobbelt
  størrelse, så et logo er stort nok til at genkendes med et blik og en signatur stor nok til at læses
  tværs over et bord. Har alle operatører et logo, udelades ordet *Operatør*; mangler en af dem et logo,
  står alle med signatur, med fed skrift og med etiketten bevaret.

### Fejlrettelser

- **Et tjenestehæfte kunne udskrive et togafsnit ud over sidens nederste kant.** Hver side blev regnet med
  omkring halvdelen mere plads, end en A5-side faktisk har, og det, der går ud over sidekanten, skæres væk
  uden varsel, så det andet togafsnit på en sådan side manglede slutningen af sin køreplan eller manglede
  helt. Togafsnit måles nu mod det, siden faktisk rummer, så nogle hæfter får et ark mere end før.

- **Topologi-diagrammet kunne skrive signaturerne for to driftssteder oven på hinanden.** Driftsstederne
  blev placeret alene efter afstanden mellem dem, så to, der ligger tæt på hinanden på en lang strækning,
  blev tegnet næsten samme sted. De tegnes nu aldrig tættere på hinanden, end deres signaturer har brug
  for, og en lang signatur ved diagrammets kant bliver ikke længere skåret væk.

- **En gren i Topologi-diagrammet kunne tegnes tværs gennem en anden strækning.** En gren falder væk i en
  fast vinkel, så en gren, der mødte en strækning i vejen, blev simpelthen tegnet tværs over den. De grene,
  der forlader en strækning længst ude, tegnes nu først, så en lang gren kan nu blive tegnet under en kort
  gren, der forlader strækningen længere ude.

- **En plan kunne vise sine tog under togkategorier, som fanen Togkategorier ikke havde.** Flere kategorier
  kunne også tages for en og samme, så deres tog blev samlet under en enkelt overskrift, og to tog af
  forskellige kategorier med samme nummer blev meldt som ét nummer brugt to gange. Når en plan åbnes,
  fyldes listen over kategorier nu op med de kategorier, togene bruger, og hver kategori holdes adskilt fra
  de andre.

- **To selskaber, der aldrig havde fået deres eget nummer, blev taget for den samme operatør,** så tog fra
  forskellige selskaber, der delte tognummer, blev meldt som ét nummer brugt to gange. Hvert selskab får nu
  sit eget nummer, når en plan åbnes eller gemmes; et selskab fra Module Registry beholder det nummer, det
  kom med.

- **En plan gemte sine togkategorier, selskaber og lande flere steder** — hver enkelt blev skrevet der,
  hvor den først blev mødt, som regel inde i det første tog, der brugte den. Hver enkelt skrives nu én
  gang, i sin egen liste, og alt, der bruger den, beholder kun en henvisning; lande kopieres slet ikke
  længere ind i planen, så en rettelse af et lands sprog nu også når planer, der er gemt forinden.

- **Et tjenestehæfte angav kun tognummeret i overskriften for et togafsnit.** Et tog identificeres lige så
  meget af kategoriens præfiks og suffiks som af nummeret — Gt 1234, ikke 1234 — og overskriften er alt, en
  lokofører har at sammenligne med køreplanen. Den viser nu hele togidentiteten, efter operatørens
  signatur.

## Version 0.3.3

### Ændringer

- **Konflikter kan nu læses dér, hvor de vises.** En række med konflikter — et tog eller en togkategori
  under **Tog**, et omløb eller et af dets køretøjer under **Omløb**, en tjeneste under **Tjenester** — har
  nu et advarselssymbol, og et klik på det åbner meddelelserne som en læsbar liste. Symbolet får farve
  efter den alvorligste konflikt og tæller dem; hidtil stod de kun i et lille felt, der kom frem, mens
  markøren hvilede på rækken.
- **En togkategori viser konflikterne for togene i den**, så de ikke længere skjules, når kategorien
  lukkes.
- **Fanen Tog åbner nu på listen over togkategorier**, hvor togene er skjult, indtil du åbner en kategori.
  *Udvid alle* åbner dem alle på én gang, og en kategori åbner af sig selv, når du føjer et tog til den
  eller flytter et tog derind.
- **Når et togafsnit i et omløb redigeres, står der nu, hvilke slags køretøjer omløbet gælder** —
  lokomotiv, togsæt eller vognsæt. Hver slags nævnes én gang, og peger du på den, nævnes køretøjerne selv.

### Fejlrettelser

- **Appen kunne holde op med at gemme dit arbejde uden at sige det.** En plan, appen ikke kunne skrive ud —
  et tog med færre end to standsninger eller en køreplansstrækning, hvor alle banestykker var fjernet — fik
  lagringen til at mislykkes lydløst, så alt derefter blev stående på skærmen, men blev aldrig gemt. Begge
  planer kan nu gemmes, og mislykkes en lagring alligevel, siger den øverste linje det med det samme.

- **En gemt planfil er omkring 40 % mindre.** Hver standsning blev skrevet to gange — én gang i sit tog og
  én gang under det spor, den ligger på — og den anden kopi trak store dele af resten af planen med sig. En
  plan gemt med en tidligere version kan stadig åbnes.

- **Et tog, der er efterladt uden trækkraft på en del af sit løb, rapporteres nu.** Kontrollen spurgte kun,
  om et lokomotiv eller togsæt kørte toget *et eller andet sted*, så når et omløb blev afkortet i den ene
  ende, stod resten af toget uden trækkraft, uden at der blev sagt noget. Nu kontrolleres hver strækning
  for hver køresession, toget køres, og konflikten siger, mellem hvilke driftssteder og i hvilke
  køresessioner; planer, der så rene ud, kan nu rapportere dette.

## Version 0.3.2

### Ændringer

- Under **Godsstrøm › Godsbeskrivelser** kan en oprindelse eller en destination nu være et hvilket som
  helst driftssted, der udveksler gods, ikke kun en station — et industriområde håndterer altid godsvogne,
  men kunne ikke vælges før. De samme lister siger nu **driftssted**, hvor de sagde *station*.
- Et togs ophold vises altid i den **rækkefølge, toget kører** dem.
- At ændre en tid for et ophold i fanen **Tog** **tager nu resten af toget med sig**: en **afgang** virker
  fremad, den vej toget kører, og en **ankomst** baglæns, så løbet frem til ændringen følger med. Tiderne
  på den anden side bliver stående, køre- og opholdstiderne bevares, og ændringen afvises, hvis den ville
  føre toget uden for planens driftstider.
- Et tog, hvis togvej **springer et driftssted over** — to ophold i rækkefølge uden en strækning imellem —
  rapporteres nu som en konflikt. Den kan slås fra under **Indstillinger › Validering**.
- Et togafsnit i et **omløb** kan nu **redigeres**: pennen åbner dets fra- og til-stop, så et omløb kan
  formes om, uden at alt efter det fjernes. Et tilstødende togafsnit, der slutter til det, du ændrer,
  følger med; et naboafsnit, hvis eget tog ikke standser på det nye stop, står uændret, og hullet
  rapporteres som en konflikt, du selv løser.
- **Tilføj tog** kan nu oprette **returtoget** samtidig. Sæt kryds i *Retur?*, så oprettes toget tilbage
  sammen med det første, med samme strækning i modsat retning, samme togart og hastighed og det næste
  nummer i den modsatte retning; afgangen er enten *så tidligt som muligt* eller et tidspunkt, du
  indtaster. Sammen med *Gentag?* gentages begge retninger.

### Fejlrettelser

- **Kilometertallene** i den udskrevne køreplan og langs den grafiske køreplan afrundes nu til hele
  kilometer, og en sidebane viser samme kilometertal som den bane, den udgår fra, ved forgreningsstationen.
- Alt, der læser et togs togvej, følger nu **den rækkefølge, toget kører sine stop i**, ikke den
  rækkefølge, de blev indtastet. For et tog, hvis stop er indtastet i forkert rækkefølge, gik linjen i den
  **grafiske køreplan** i siksak, kunne den udskrevne **køreplan** vise en afgang, hvor toget ankommer,
  kædede **byg automatisk** slet ikke toget, målte **gentag tog** intervallet fra det forkerte stop, og
  genberegning af tiderne mislykkedes helt. Importerede planer har aldrig været berørt.
- **Toghastigheden kontrolleres nu også på den sidste strækning**, ind til det driftssted, hvor toget
  slutter sit løb.

## Version 0.3.1

### Ændringer

- Afsnittet **Trækkraftenheder** på siden for et togafsnit i hæftet Førertjenester har nu sin overskrift på
  det valgte sprog. Det var den eneste overskrift i hæftet uden oversættelse.
- Trækkraftenheden udskrives nu for hvert togafsnit, der har en. I planer importeret med en tidligere
  version viste nogle togafsnit en trækkraftenhed under **Tjenester**, men ingen i hæftet.
- Noter om tog i samme retning fortæller nu, hvilket tog der kommer forbi det andet — **Overhaler GD 42757
  12:02-12:05** eller **Overhales af GD 42757 12:02** — i stedet for det hidtidige *"Møder GD 42757 i samme
  retning"*, der aldrig sagde, hvilket tog der kom foran. To tog, der blot står på samme station samtidig,
  giver ingen note overhovedet.
- Et møde uden varighed — det andet tog kører igennem uden ophold — skrives som ét klokkeslæt i stedet for
  et interval fra et tidspunkt til sig selv.
- Et tog, der begynder eller afslutter sin kørsel på en station, medtages ikke længere som mødt, krydset
  eller overhalet der. De tidspunkter er, når dets lokofører møder ind eller går af.

## Version 0.3.0

### Ændringer

- En ny rapport, **Førertjenester**, udskriver ét A5-hæfte pr. tjeneste. Forsiden viser tjenestens nummer,
  hvilke sessioner eller dage den kører, dens start- og sluttidspunkt og -stationer, en sværhedsgrad,
  bemandingsbehov og eventuelle tjenestenoter; hvert togafsnit får derefter sin egen side med hvilke
  trækkraftenheder der skal bruges, hvilke vognsæt der skal medbringes, til hvilke destinationer der skal
  medbringes godsvogne, samt køreplanen, hver i sin egen blok.
- En ny rapport, **Generelle instruktioner**, er et separat hæfte med træffets program og de instruktioner,
  der gælder for anlægget i hele træffets varighed — køreinstruktioner, signalgivning, radio- og
  telefonbrug, hvad man gør ved forsinkelser og hvem man spørger — og det uddeles én gang til alle. Det
  indledes med træffets navn og datoer, så programmet, enhver deltager har brug for at vide før den første
  session, og derefter instruktionerne over så mange sider, som de har brug for, brudt mellem afsnit og
  aldrig med en overskrift efterladt alene.
- Sidste side i begge hæfter viser anlæggets sporplan og tabellen over rangerbanegårde, så også de, der
  aldrig har et tjenestehæfte i hånden — først og fremmest stationspersonalet — får et overblik over
  anlægget.
- Både programmet og instruktionerne skrives under **Indstillinger › Information** og kan formateres med
  Markdown. Begge hæfter udskrives i A5: A4 liggende, dobbeltsidet, foldet på midten, med tomme sider
  tilføjet hvor det er nødvendigt, så arkene foldes korrekt.
- Tjenester kan nu graderes **Let**, **Middel** eller **Erfaren**, vist farvekodet på hæftet, kan angive,
  at de kræver to eller tre personer — for eksempel en lokofører og en konduktør — og kan fastgøres til et
  **fast nummer**, som automatisk omnummerering lader urørt.
- Planen kontrolleres nu også, så hvert togafsnit med et lokomotiv eller togsæt tildelt har en
  førertjeneste, der dækker det i hver session, det kører. En tjeneste med fast nummer skal have et nummer,
  og ingen to sådanne må få samme nummer.
- Selskaber kan nu have et uploadet **logo**, vist på rapporter i stedet for tekstsignaturen.
- Stationer kan nu markeres som den **rangerbanegård**, der betjener en anden lokalitets lokalgods, og
  anlægget lister hver rangerbanegård og hvad den dækker på tjenestehæftets sidste side.
- Hver køreplansstrækning kan nu tildeles en **farve**, som bruges til at tegne den i Topologi-diagrammet.
- En ny **afstandsfaktor** (Indstillinger › Tid & hastighed) lader et anlæg vise et større, mere
  forbilledetro kilometertal i rapporter og den grafiske køreplan end den afstand, der faktisk er
  modelleret, uden at det påvirker nogen køretidsberegning.
- Appen holder nu flere åbne browserfaner eller -vinduer synkroniseret med hinanden. **Bemærk**, at dette
  kun virker mellem vinduer på samme maskine i samme browser.
- Indstillinger kan nu gemme træffets **gælder fra**- og **gælder til**-datoer, trykt som en gyldighedslinje
  på rapporter; lad dem stå tomme, hvis intet træf er booket endnu.
- En ny indstilling, **udvid plantider automatisk?** (Indstillinger › Generelt), udvider planens start-
  eller sluttidspunkt for at dække et tog i stedet for at blokere ændringen. Slået fra som standard.
- En ny knap, **opdatér alle tider**, i den grafiske køreplan genberegner alle tog i køreplanen på én gang
  i stedet for først at skulle vælge en delmængde.
- Sporbelægningskontrollen kan nu valgfrit tage højde for, at et lokomotiv eller togsæt holder på et spor
  mellem to tog, medmindre det er booket til eller fra opstilling (Indstillinger › Validering). Slået fra
  som standard, da det kun giver mening på anlæg, hvor opstilling er modelleret bevidst.
- Hvert ophold i fanen **Tog** har nu et felt til **Bemærkning** — en note, der udskrives ved det ophold,
  for eksempel "vent på modkørende tog". Bemærkningen vises færdigformateret og skifter til den rå
  opmærkning, så snart du går ind i feltet, så skriv `*langsomt*` for kursiv og `**første**` for fed.

### Fejlrettelser

- Når man tilføjer et nyt tog, sættes dets standardstarttidspunkt nu under hensyn til den angivne
  forberedelsestid, så det ikke starter før planens starttidspunkt.

## Version 0.2.4

### Ændringer

- En ny fane **Tjenester** lader dig planlægge førertjenester — det arbejde, en lokofører udfører i løbet
  af en session, som en række af de togafsnit, føreren kører. Hver tjeneste er en række: dens betegnelse,
  firma og sessioner til venstre, togafsnittene i køreorden til højre.
- Tilføj de togafsnit, en fører kører, med **Tilføj togafsnit**. Listen viser de trækkraftstrækninger, en
  fører kan tage som det næste — dem, der ikke støder sammen i tid med tjenesten, og, når den har et
  togafsnit, dem, der afgår ved eller efter, at det ankommer. Togafsnittene behøver ikke starte på samme
  station: føreren går ganske enkelt hen, hvor det næste starter.
- Det samme togafsnit kan køres af flere tjenester, så længe de kører i forskellige sessioner, så én
  tjeneste kan dække de ulige sessioner og en anden de lige.
- Hvor to togafsnit for samme tog i en tjeneste køres af forskellige trækkraftenheder, viser fanen en note
  ved stationen, hvor trækkraftenheden skiftes — du indtaster den ikke i hånden.
- Tjenester importeret fra XPLN deler nu de togafsnit, der er defineret i køretøjernes omløb, så hvert
  togafsnit viser den trækkraftenhed, der kører det.
- Planen kontrolleres, så intet togafsnit køres af to tjenester i samme session, og ingen tjeneste har
  togafsnit, der overlapper i tid. Kontrollen kan slås fra under **Indstillinger › Validering**.

## Version 0.2.2

### Fejlrettelser

- To tog, der aldrig kører i samme køresession, rapporteres ikke længere som et møde på en enkeltsporet
  strækning. Et tog, der kører session 1, 3, 5, og et, der kører 2, 4, 6, er aldrig ude samtidig.
- Konfliktkontrollen på dobbeltsporede og flersporede strækninger er nu præcis: en strækning markeres kun,
  når der er flere tog på den samtidig, end den har spor, og kun tog, der kører i en fælles session, tælles
  med.

## Version 0.2.1

### Ændringer

- Konfliktadvarsler vises nu, hvor du kan rette dem: togkonflikter i den grafiske køreplan og på fanen
  **Tog**, køretøjs- og omløbskonflikter på fanen **Omløb**.
- På fanen **Omløb** fremhæver en køretøjskonflikt nu kun det pågældende køretøj, og en omløbskonflikt kun
  det pågældende omløb.
- Kontrollen af, at et køretøj vender tilbage til sit udgangspunkt, omfatter nu også vognsæt og gods, ikke
  kun lokomotiver og togsæt.

## Version 0.2.0

### Ændringer

- Navnet på den plan, du arbejder med, vises nu øverst i vinduet.
- Den grafiske køreplan viser nu søjler for lokomotivførerbehovet, hvilket gør det lettere at se, hvor
  mange førere der er brug for gennem køresessionen.
- En ny **Topologi**-visning (under fanen **Strækninger**) viser et skematisk diagram over køreplanens
  strækninger og deres grene.

### Fejlrettelser

- Strækninger bevarer nu som standard den rækkefølge, du indtastede dem i. Du kan stadig sortere efter
  enhver kolonne.
- Konflikter henviser ikke længere til tog, du ikke kan finde: når et tog slettes, fjernes dets stop sammen
  med det, så der ikke er forældreløse stop eller falske konflikter tilbage.

## Version 0.1.0

Første forhåndsvisning af Køreplanlæggeren. Du kan:

- Definere sporplaner med stationer, spor og strækninger.
- Oprette og redigere togkøreplaner med automatisk tidsberegning.
- Tildele lokomotiver og togstammer til tog.
- Bygge køretøjsomløb og udskrive omløbskort.
- Planlægge godsstrømme mellem stationer.
- Vise grafiske køreplaner (tid-afstands-diagrammer).
- Validere køreplaner for konflikter og inkonsistenser.
- Generere udskrifter: togkort, stationsbøger og vagtplaner.
- Arbejde på engelsk, tysk, dansk, norsk og svensk.
