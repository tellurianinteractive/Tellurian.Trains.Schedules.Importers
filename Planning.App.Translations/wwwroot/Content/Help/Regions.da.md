Fanen **Regioner** definerer destinationerne *uden for* dit anlæg – de indenlandske regioner og
fremmede lande, som vogne kan styres til. Regioner bruges til styring af godsstrømme: de gør det muligt for et
togs destinationsnote at sige, hvor vognene er på vej hen hinsides den modellerede jernbane.

### Hvad en region er

Hver region har:

- et **navn**, skrevet på anlæggets standardsprog og vist overalt, hvor regionen omtales,
- et **land**, den hører til – en ny region får som standard anlæggets standardland, men en region,
  der står for et udenlandsk mål, kan i stedet sættes til dét land,
- en **baggrundsfarve**, som bruges til at vise regionen som en farvet brik i noter (tekstfarven
  kontrasteres automatisk for læsbarhedens skyld).

### Hvordan regioner bruges

En region er i sig selv blot en etiket. Den får først betydning, når den knyttes til en
**station** – normalt en **skyggebanegård**, en endestation, der repræsenterer eksterne stationer og
verden hinsides den modellerede jernbane. En station kan knyttes til nul, en eller flere regioner;
almindelige stationer har sjældent brug for nogen.

Når en godsdestination er sat til at *inkludere regioner*, viser destinationsnoten for den station
dens regioner som farvede brikker – hvilket fortæller operatøren, at vogne, der bringes dertil, fortsætter videre til disse
regioner eller lande.

Definér de regioner, du har brug for, her, og tildel dem derefter til dine skyggestationer på fanen
**Driftssteder**.

### Administration af regioner

Listen viser hver region som en farvet brik ved siden af dens navn og land.

- **Tilføj ny** opretter en region. Giv den et navn, vælg dens land, og vælg en farve fra
  paletten.
- **Rediger** ændrer en regions navn, land eller farve.
- **Slet** fjerner en region. Det blokeres, så længe en station har regionen tildelt – fjern
  tildelingen på fanen **Driftssteder** først.
- **Standardregioner** tilføjer standardsættet af regioner, navngivet på anlæggets standardsprog og
  praktisk, når du starter et nyt anlæg.

Regioner gemmes sammen med anlægget, og dette katalog er kilden til de regionsvalg, der vises,
når en station redigeres.
