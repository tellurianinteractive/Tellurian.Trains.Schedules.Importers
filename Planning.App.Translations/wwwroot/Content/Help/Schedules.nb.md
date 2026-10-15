Fanen **Omløp** er stedet der du gjør en ruteplan om til kjøretøy- og personellplanene som trengs for å kjøre
kjøresesjonen.

Her bygger du **kjøretøyomløp** og tildeler dem til lokomotiver, vognsett og godsstrømmer,
slik at hvert tog får utstyret det trenger og hvert kjøretøy får et sammenhengende, fornuftig
arbeid gjennom dagen. Førertjenester knytter dette sammen til arbeid én person kan utføre.

### Omløp

Hvert omløp er en rad: den første kolonnen lister **kjøretøyene** som kjører det, fulgt av
**togavsnittene** i arbeidsrekkefølge. Et vognsett som lister vognene sine, viser hvor mange det er, og hver
vognklasse én gang etter nummeret, som i *SJ 05 5 x A/B/Fv*.

Bruk **Bygg automatisk** for å kjede sammen tog i samme kategori
som fortsetter der det forrige ankom (merk en kategori *Utelat fra automatisk planlegging* på fanen **Togkategorier**
for å holde den utenfor), eller **Nytt omløp** for å bygge
ett for hånd. Automatisk bygging tilbyr først de ennå ikke planlagte togene til omløpene som allerede finnes: et omløp
fortsetter med det som fortsetter det i samme kategori, og et tomt omløp
du har laget, fylles før noe nytt opprettes, slik at et kjøretøy som allerede snur, tar mer
arbeid før et annet settes inn. Bare togene som ikke passer i noe eksisterende omløp, starter nye,
og godsstrømmer lar vi stå som de er.
Et omløp som kjøres av et togsett, eller av et lokomotiv med avkrysset **Vendetog?**, får ankomstene sine
flyttet til sporet neste tog går fra, fordi et slikt tog avgår fra sporet det kom inn
på. Der arbeidet ender der det begynte, ankommer det siste toget til sporet det første
går fra. Dette skjer når togsettet eller lokomotivet tildeles, når krysset settes og når tog
legges til. Et omløp med et vanlig lokomotiv, eller uten kjøretøy ennå, beholder sporene sine: neste tog
kan godt bruke andre vogner, noe som ofte avgjøres senere i planleggingen.

Knappen **ankomstspor** på en rad flytter ankomstene i det omløpet når som helst, uansett hvilket
kjøretøy som kjører det, for eksempel når du vet at et lokomotiv beholder vognene sine.

På en rad:
- **+ tog** legger til neste tog – velg bare en del av det (et fra-/til-opphold)
  når et tog må deles mellom kjøretøy, for eksempel ved skifte fra elektrisk til dieseltrekkraft.
- De små **leddene** mellom togavsnittene sier hvor kjøretøyet står og hvor lenge, og før det første avsnittet hvor det
  må hentes fra. Klikk på ett for å arbeide inn et tog i gapet: bare
  togene kjøretøyet faktisk kan rekke på tiden som er tilgjengelig, tilbys. En etappe som ikke
  bringer kjøretøyet tilbake dit arbeidet fortsetter, legges likevel til og rapporteres som en konflikt
  helt til du legger til etappen tilbake – slik arbeides en tur-retur-tur inn i et opphold, én etappe om
  gangen. Et ledd der arbeidet er brutt, er merket med rav, og et klikk på det tilbyr togene
  som bygger bro over det.
- **Blyanten** på et togavsnitt endrer hvor stor del av toget omløpet kjører: velg et nytt fra- eller
  til-opphold. Selve toget blir; for å kjøre et annet tog fjerner du avsnittet og legger til det andre.
  Det tilstøtende avsnittet som henger sammen med det du endrer, følger med, slik at arbeidet forblir helt –
  forkort et avsnitt fra A–C til A–B, og returkjøringen blir B–A av seg selv. Når naboens
  eget tog ikke har opphold ved det nye oppholdet, blir det som det er, og gapet rapporteres som en konflikt
  du må løse.
  Den samme dialogen sier hvor kjøretøyene står når det ikke er sporet toget bruker: **Hent fra**
  angir sporet de hentes fra før toget avgår, og **Sett på** sporet de settes
  på etter at det har ankommet. Hver skrives ut som en merknad til lokføreren og togekspeditøren, i stedet for
  den vanlige merknaden om å koble kjøretøyet på eller av. Et vognsetts spor står allerede i
  lokførerens vognsetttabell, så lokførerens merknad sier bare at vognene skal skiftes til avgangssporet eller
  til ankomstsporet. Der et kjøretøy kjører toget bare på noen av kjøresesjonene eller dagene toget
  går, begynner merknaden med dem. Et spor som er valgt ved en stasjon der avsnittet ikke
  lenger begynner eller slutter, glemmes.
- **+ kjøretøy** tildeler et kjøretøy og oppretter et nytt ved behov; et omløp kan ha
  flere kjøretøy (for eksempel et lokomotiv og vognsettet). Godsstrømmer vises som omløpskort
  i rapportene i stedet for her. Et omløp som bare inneholder **skifteoppgaver**, trenger ikke noe kjøretøy
  i det hele tatt: en slik oppgave utføres av det som står på stasjonen, så det er fullstendig snarere enn
  uferdig å la den stå uten, og den rapporteres ikke som et omløp som er etterlatt uten kjøretøy. Legg til et reisende
  tog i det samme omløpet, så ønskes det et kjøretøy igjen.

Når du redigerer et lokomotiv, tilbys **Vendetog?**. Kryss av der lokomotivet kjører et tog som kan
kjøres fra begge ender – ett med styrevogn i den andre enden eller med et annet lokomotiv
der. Der et slikt togs trasé snur, bytter det retning ved å bytte førerrom, så **Oppdater tider**
gir det ikke lenger tid til å kjøre lokomotivet rundt, og oppholdet krymper til korteste opphold.
Et togsett behandles allerede slik og tilbys ikke noe felt.

#### Trafikkerte kjøresesjoner eller dager

Et omløps trafikkerte kjøresesjoner blir delmengden av kjøresesjoner alle togene trafikkerer. Med et omløp der de fleste togene
går daglig og et par bare kjøresesjon 1–5, blir hele omløpet 1–5.

#### Validering

Valideringsreglene under **Innstillinger › Validering** (lokomotivdekning, førertjenester) kontrollerer
at omløpene du bygger her faktisk dekker togene i ruteplanen.
