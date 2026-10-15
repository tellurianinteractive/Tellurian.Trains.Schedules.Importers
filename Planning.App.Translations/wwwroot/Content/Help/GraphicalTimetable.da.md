Fanen **Grafisk køreplan** tegner en togtegning (tid–vejdiagram) for de
køreplansstrækninger, du vælger.

### Valg af, hvad der skal vises

Afkryds en eller flere **køreplansstrækninger**, der skal tegnes. Hver valgt strækning tegnes som sin egen
graf. Strækningerne kommer fra fanen **Strækninger**; hvis ingen er defineret, er der intet at tegne.

#### Visning af den ene halvdel

Når et **delepunkt** er angivet under **Indstillinger › Generelt**, vises en vælger **Vis** med
**Hele grafen**, **Første halvdel** og **Sidste halvdel**. Første halvdel løber fra driftens start
til delepunktet; sidste halvdel fra delepunktet til driftens slutning. Vælger du en halvdel, tegnes kun den del af
døgnet, hvilket gør grafen læsbar på mindre skærme – især med lodret tidsakse.
Vælgeren er skjult, når der ikke er angivet noget delepunkt.

### Kopiering af tog

**Kopiér tog** kopierer de tog, du vælger. Angiv det antal minutter, kopien forskydes med –
positivt er senere, negativt tidligere – og hvert valgt tog kopieres én gang.

Afkryds **Modsat retning?**, og kopien kører i stedet samme rute baglæns, fra hvor toget
sluttede, til hvor det begyndte. Alle køretider og alle ophold bevares, forberedelses- og afslutningstiderne
følger kopien til dens egne ender, og den får et nummer fra den modsatte retnings nummerrække. Minutterne
regnes derefter fra det kopierede togs sidste afgang, så de angiver, hvor længe efter at toget
er stillet væk, returkørslen kører.

Afkryds **Gentag?** under **Gentag tog** for at lave en serie i stedet for en enkelt kopi: den første kopi
laves ved den forskydning, du angav, og en mere for hvert interval, indtil en kopi ville afgå efter sluttidspunktet.
Det er den samme gentagelse som ved tilføjelse af et tog, men på et tog, der allerede findes – så
det første tog kan justeres, til det kører, som det skal, før resten kopieres fra det.

En kopi, der ville køre uden for planens start- og sluttidspunkt, laves ikke.

### Aflæsning af grafen

Stationerne fordeles langs den ene akse og tiden langs den anden; hvert tog er en skrå linje, farvet
efter sin kategori. Orientering, afstand og etiketter styres fra
**Indstillinger › Grafisk køreplan**.

### Krævede lokoførere

Når **Forventet antal lokoførere** er angivet under **Indstillinger › Generelt**, følger en række søjler
graferne tidsakse – over dem med vandret tidsakse, til venstre for dem med lodret. Hver
søjle viser, hvor mange lokoførere der er brug for på det tidspunkt, på den travleste køresession:

- hvert tog og hver rangeropgave kræver en fører fra sin første ankomst til sin sidste afgang,
  inklusive opholdene undervejs,
- en fører i en **tjeneste** er også bundet mellem de tog, tjenesten kører – venter eller går til
  det næste – og før det første tog eller efter det sidste, når tjenestens start- eller sluttidspunkt
  er sat til at række ud over dem.

Søjlerne farves efter det antal, du forventer: røde, når der er brug for flere førere end
forventet, grønne, når præcis så mange, gule, når en eller to er til overs, og grå, når tre eller flere
er til overs. En streg markerer det forventede antal. Peg på en søjle for at se dens tider og antal.

### Start- og sluttidspunkt

Grafen viser den tidsramme, der er defineret i **Indstillinger › Generelt**.
En særlig mulighed får grafen til at vise et helt døgn fra 00:00 til 24:00 ved uafbrudt drift.
Et tog, hvis tider fortsætter efter midnat, ombrydes og fortsætter fra 00:00 i begyndelsen af aksen.
