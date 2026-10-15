Fanen **Grafisk ruteplan** tegner en togtegning (tid–veidiagram) for de
ruteplanstrekningene du velger.

### Velge hva som vises

Kryss av én eller flere **ruteplanstrekninger** som skal tegnes. Hver valgte strekning tegnes som sin egen
graf. Strekningene kommer fra fanen **Strekninger**; hvis ingen er definert, er det ingenting å tegne.

#### Vise den ene halvdelen

Når et **delepunkt** er angitt under **Innstillinger › Generelt**, vises en velger **Vis** med
**Hele grafen**, **Første halvdel** og **Siste halvdel**. Første halvdel går fra driftens start
til delepunktet; siste halvdel fra delepunktet til driftens slutt. Velger du en halvdel, tegnes bare den delen av
døgnet, noe som holder grafen lesbar på mindre skjermer – særlig med loddrett tidsakse.
Velgeren er skjult når ingen delepunkt er angitt.

### Kopiere tog

**Kopier tog** kopierer togene du velger. Angi antall minutter kopien forskyves med –
positivt er senere, negativt tidligere – og hvert valgte tog kopieres én gang.

Kryss av **Motsatt retning?**, så kjører kopien i stedet samme vei baklengs, fra der toget
sluttet til der det begynte. Alle kjøretider og alle opphold beholdes, forberedelses- og avslutningstidene
følger kopien til dens egne ender, og den får et nummer fra motsatt retnings nummerrekke. Minuttene
regnes deretter fra det kopierte togets siste avgang, så de sier hvor lenge etter at toget
er satt bort returkjøringen går.

Kryss av **Gjenta?** under **Gjenta tog** for å lage en serie i stedet for én enkelt kopi: den første kopien
lages med forskyvningen du oppga, og én til for hvert intervall helt til en kopi ville gå etter sluttidspunktet.
Det er den samme gjentakelsen som når et tog legges til, men på et tog som allerede finnes – slik at
det første toget kan justeres til det går som det skal før resten kopieres fra det.

En kopi som ville gå utenfor planens start- og sluttidspunkt, lages ikke.

### Lese grafen

Stasjonene fordeles langs den ene aksen og tiden langs den andre; hvert tog er en skrå linje, farget
etter kategorien sin. Orientering, avstand og etiketter styres fra
**Innstillinger › Grafisk ruteplan**.

### Nødvendige lokførere

Når **Forventet antall lokførere** er angitt under **Innstillinger › Generelt**, følger en stripe med søyler
grafenes tidsakse – over dem med vannrett tidsakse, til venstre for dem med loddrett. Hver
søyle viser hvor mange lokførere som trengs på det tidspunktet, på den travleste kjøresesjonen:

- hvert tog og hver skifteoppgave trenger en fører fra sin første ankomst til sin siste avgang,
  medregnet oppholdene underveis,
- en fører i en **tjeneste** er også bundet mellom togene tjenesten kjører – venter eller går til
  neste – og før det første toget eller etter det siste, når tjenestens start- eller sluttidspunkt
  er satt til å strekke seg utenfor dem.

Søylene farges etter antallet du forventer: røde når det trengs flere førere enn
forventet, grønne når akkurat så mange, gule når én eller to er til overs, og grå når tre eller flere
er til overs. En strek markerer det forventede antallet. Pek på en søyle for å se tidene og antallet.

### Start- og sluttidspunkt

Grafen viser tidsrammen som er definert i **Innstillinger › Generelt**.
Et spesielt alternativ får grafen til å vise et helt døgn fra 00:00 til 24:00 ved kontinuerlig drift.
Et tog hvis tider fortsetter etter midnatt, brytes rundt og fortsetter fra 00:00 i begynnelsen av aksen.
