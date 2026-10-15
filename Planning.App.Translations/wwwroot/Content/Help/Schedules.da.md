Fanen **Omløb** er stedet, hvor du omsætter en køreplan til de køretøjs- og personaleplaner, der skal til for at køre
køresessionen.

Her opbygger du **køretøjsomløb** og tildeler dem til lokomotiver, vognsæt og godsstrømme,
så hvert tog får det udstyr, det har brug for, og hvert køretøj får et sammenhængende, fornuftigt
arbejde gennem dagen. Førertjenester binder det sammen til arbejde, som én person kan udføre.

### Omløb

Hvert omløb er en række: den første kolonne viser de **køretøjer**, der kører det, efterfulgt af
**togafsnittene** i arbejdsrækkefølge. Et vognsæt, der opfører sine vogne, viser, hvor mange der er, og hver
vognklasse én gang efter sit nummer, som i *SJ 05 5 x A/B/Fv*.

Brug **Byg automatisk** til at sammenkæde tog af samme kategori,
der fortsætter, hvor det forrige ankom (markér en kategori *Udeluk fra automatisk planlægning* på fanen **Togkategorier**
for at holde den uden for), eller **Nyt omløb** til at opbygge
et i hånden. Automatisk opbygning tilbyder først de endnu ikke planlagte tog til de omløb, der allerede findes: et arbejde
fortsætter med det, der fortsætter det i samme kategori, og et tomt omløb,
du har lavet, fyldes, før der oprettes et nyt, så et køretøj, der allerede vender, tager mere
arbejde, før et andet sættes ind. Kun de tog, der ikke passer til noget eksisterende omløb, starter nye,
og godsstrømme lades, som de er.
Et omløb, der køres af et togsæt eller af et lokomotiv med afkrydset **Vendetog?**, får sine ankomster
flyttet til det spor, dets næste tog afgår fra, fordi et sådant tog afgår fra det spor, det kom ind
på. Hvor arbejdet ender, hvor det begyndte, ankommer dets sidste tog til det spor, det første
afgår fra. Det sker, når togsættet eller lokomotivet tildeles, når feltet afkrydses, og når tog
tilføjes. Et omløb med et almindeligt lokomotiv eller uden noget køretøj endnu beholder sine spor: dets næste tog
kan sagtens bruge andre vogne, hvilket ofte afgøres senere i planlægningen.

Knappen **ankomstspor** på en række flytter det omløbs ankomster når som helst, uanset hvilket
køretøj der kører det, for eksempel når du ved, at et lokomotiv beholder sine vogne.

På en række:
- **+ tog** tilføjer det næste tog – vælg kun en del af det (et fra-/til-ophold),
  når et tog skal deles mellem køretøjer, for eksempel ved skift fra elektrisk til diesel-
  trækkraft.
- De små **led** mellem togafsnittene angiver, hvor køretøjet holder, og hvor længe, og før det første afsnit, hvorfra det
  skal hentes. Klik på et for at arbejde et tog ind i hullet: kun
  de tog, køretøjet faktisk kan nå i den tid, der er til rådighed, tilbydes. En etape, der ikke
  bringer køretøjet tilbage dertil, hvor arbejdet fortsætter, tilføjes alligevel og rapporteres som en konflikt,
  indtil du tilføjer etapen tilbage – sådan arbejdes en tur-retur-tur ind i et ophold, en etape ad
  gangen. Et led, hvor arbejdet er brudt, markeres med rav, og et klik på det tilbyder de tog,
  der bygger bro over det.
- **Blyanten** på et togafsnit ændrer, hvor stor en del af sit tog omløbet kører: vælg et nyt fra- eller
  til-ophold. Selve toget bliver; for at køre et andet tog fjerner du afsnittet og tilføjer det andet.
  Det tilstødende afsnit, der slutter sig til det, du ændrer, følger med, så arbejdet forbliver helt –
  forkort et afsnit fra A–C til A–B, og returkørslen bliver B–A af sig selv. Når nabotogets
  eget tog ikke har ophold ved det nye ophold, lades det, som det er, og hullet rapporteres som en konflikt,
  som du skal løse.
  Den samme dialog angiver, hvor køretøjerne holder, når det ikke er det spor, toget bruger: **Hent fra**
  angiver det spor, de hentes fra, før toget afgår, og **Sæt på** det spor, de sættes
  på, efter det er ankommet. Hver udskrives som en note til lokoføreren og toglederen i stedet for
  den sædvanlige note om at koble køretøjet til eller fra. Et vognsæts spor står allerede i
  lokoførerens vognsætstabel, så lokoførerens note siger kun, at vognene skal rangeres til afgangssporet eller
  til deres ankomstspor. Hvor et køretøj kun kører toget på nogle af de køresessioner eller dage, toget
  kører, begynder dets note med dem. Et spor, der er valgt ved en station, hvor afsnittet ikke
  længere begynder eller ender, glemmes.
- **+ køretøj** tildeler et køretøj og opretter et nyt, når det er nødvendigt; et omløb kan have
  flere køretøjer (såsom et lokomotiv og dets vognsæt). Godsstrømme vises som omløbskort
  i rapporterne i stedet for her. Et omløb, der kun indeholder **rangeropgaver**, behøver slet
  intet køretøj: sådan en opgave udføres af det, der holder på stationen, så det er færdigt snarere end
  ufærdigt at lade den være uden, og det rapporteres ikke som et omløb, der er efterladt uden køretøj. Tilføj et rejsende
  tog til det samme omløb, og der ønskes igen et køretøj.

Når du redigerer et lokomotiv, tilbydes **Vendetog?**. Afkryds det, hvor lokomotivet kører et tog, der kan
køres fra begge ender – et med styrevogn i den fjerne ende eller med et andet lokomotiv
dér. Hvor et sådant togs rute vender, skifter det retning ved at skifte førerrum, så **Opdatér tider**
giver det ikke længere tid til at køre lokomotivet rundt, og opholdet skrumper til mindste ophold.
Et togsæt behandles allerede på den måde og tilbydes intet felt.

#### Kørende køresessioner eller dage

Et omløbs kørende køresessioner bliver den delmængde af køresessioner, alle tog kører. Med et omløb, hvor de fleste tog
kører dagligt, og et par kun køresession 1–5, bliver hele omløbet 1–5.

#### Validering

Valideringsreglerne under **Indstillinger › Validering** (lokomotivdækning, førertjenester) kontrollerer,
at de omløb, du opbygger her, faktisk dækker togene i køreplanen.
