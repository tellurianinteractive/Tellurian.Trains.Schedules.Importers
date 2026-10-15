Fanen **Regioner** definerer destinasjonene *utenfor* anlegget ditt – de innenlandske regionene og
utenlandske landene vogner kan ledes til. Regioner brukes til styring av godsstrømmer: de lar et
togs destinasjonsmerknad si hvor vognene er på vei hinsides den modellerte jernbanen.

### Hva en region er

Hver region har:

- et **navn**, skrevet på anleggets standardspråk og vist overalt der regionen omtales,
- et **land** den hører til – en ny region får anleggets standardland, men en region
  som står for et utenlandsk mål kan i stedet settes til det landet,
- en **bakgrunnsfarge**, som brukes til å vise regionen som en farget brikke i merknader (tekstfargen
  kontrasteres automatisk for lesbarhetens skyld).

### Hvordan regioner brukes

En region er i seg selv bare en etikett. Den får betydning først når den knyttes til en
**stasjon** – vanligvis en **skyggestasjon**, en endestasjon som representerer eksterne stasjoner og
verden hinsides den modellerte jernbanen. En stasjon kan knyttes til null, én eller flere regioner;
vanlige stasjoner trenger sjelden noen.

Når en godsdestinasjon er satt til å *inkludere regioner*, lister destinasjonsmerknaden for den stasjonen
opp regionene som fargede brikker – noe som forteller operatøren at vogner som bringes dit fortsetter videre til de
regionene eller landene.

Definer regionene du trenger her, og tildel dem deretter til skyggestasjonene dine på fanen
**Driftssteder**.

### Administrere regioner

Listen viser hver region som en farget brikke ved siden av navn og land.

- **Legg til ny** oppretter en region. Gi den et navn, velg land og velg en farge fra
  paletten.
- **Rediger** endrer en regions navn, land eller farge.
- **Slett** fjerner en region. Det blokkeres så lenge en stasjon har regionen tildelt – fjern
  tildelingen på fanen **Driftssteder** først.
- **Standardregioner** legger til standardsettet av regioner, navngitt på anleggets standardspråk og
  praktisk når du begynner på et nytt anlegg.

Regioner lagres sammen med anlegget, og denne katalogen er kilden til regionvalgene som vises
når en stasjon redigeres.
