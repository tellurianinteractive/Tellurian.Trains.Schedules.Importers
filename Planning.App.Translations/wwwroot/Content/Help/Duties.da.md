Fanen **Tjenester** er stedet, hvor du planlægger **førertjenester** – det arbejde, en lokofører udfører i løbet af en
køresession, som en række af de togafsnit, vedkommende kører.

En tjeneste opbygges af de **togafsnit, der allerede er defineret i køretøjsomløbene**: de trækkende
afsnit, et lokomotiv eller togsæt kører, og **rangeropgaverne**, som kræver en fører, uanset om
et eget lokomotiv er booket til dem. Det samme tog kan opdeles i flere afsnit
(for eksempel hvor lokomotivet skiftes ved en elektrificeringsgrænse), og en tjeneste sammenkæder blot
de afsnit, en enkelt fører tager.

### Opbygning af en tjeneste

Hver tjeneste er en række: den første kolonne viser dens **identitet**, **selskab** og de **køresessioner**, den
kører. Den anden kolonne viser **togafsnittene** i den rækkefølge, føreren kører dem.

- **Ny tjeneste** tilføjer en tom tjeneste.
- **+ togafsnit** tilføjer det næste afsnit. Vælgeren tilbyder de afsnit, en fører kan tage som det næste:
  dem, der ikke støder sammen i tid med de afsnit, der allerede er i tjenesten, og – når tjenesten har
  et afsnit – dem, der begynder, når det slutter, eller senere. Tilføjer du afsnittene i kørselsrækkefølge, holdes listen til
  de naturlige fortsættelser, men rækkefølgen, du tilføjer dem i, har ingen betydning. En rangeropgave
  skrives med stationen én gang og de tidspunkter, arbejdet foregår mellem, da den ikke går nogen steder.
- Afsnit behøver **ikke** mødes på den samme station: mellem to afsnit går føreren derhen, hvor det næste
  afsnit begynder.

Et afsnit kan køres af **flere tjenester**, så længe de kører på **forskellige køresessioner** – så én
tjeneste kan dække de ulige køresessioner og en anden de lige køresessioner på det samme afsnit.

### Køresessioner, selskab og noter

Brug **rediger**-knappen (blyanten) til at angive tjenestens identitet, kørende selskab og de køresessioner,
den kører, og til at tilføje frie **tjenestenoter**, der gælder hele tjenesten (adskilt fra
noterne pr. ophold, som vises andre steder).

### Skift af trækkraftenhed

Når to på hinanden følgende afsnit af **det samme tog** i en tjeneste køres af **forskellige trækkraftenheder**,
bliver føreren hos toget, mens lokomotivet skiftes. Fanen viser en afledt
note ved den station, hvor det sker – du indtaster den ikke selv.

### Validering

Når **Indstillinger › Validering › Førertjenester** er aktiveret, kontrolleres planen, så intet togafsnit
køres af to tjenester på en fælles køresession, og ingen tjeneste har afsnit, der overlapper i tid. Konflikter
vises i valideringsindikatoren og åbnes her.

Den samme indstilling kontrollerer, at nogen er sat på hvert afsnit, der kræver en fører: hvert afsnit, et
lokomotiv eller togsæt kører, og hver **rangeropgave**, på hver køresession, det kører. En opgave kontrolleres,
uanset om et lokomotiv er booket til den, fordi den ikke behøver noget eget lokomotiv, men altid
skal have nogen til at udføre den.
