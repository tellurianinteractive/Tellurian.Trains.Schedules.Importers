Fanen **Tjenester** er stedet der du planlegger **førertjenester** – arbeidet én lokfører utfører i løpet av en
kjøresesjon, som en rekke av togavsnittene vedkommende kjører.

En tjeneste bygges av **togavsnittene som allerede er definert i kjøretøyomløpene**: de trekkende
avsnittene et lokomotiv eller togsett kjører, og **skifteoppgavene**, som trenger en fører uansett om
et eget lokomotiv er booket til dem. Det samme toget kan deles i flere avsnitt
(for eksempel der lokomotivet byttes ved en elektrifiseringsgrense), og en tjeneste kjeder bare sammen
avsnittene én enkelt fører tar.

### Bygge en tjeneste

Hver tjeneste er en rad: den første kolonnen viser dens **identitet**, **selskap** og **kjøresesjonene** den
kjører. Den andre kolonnen lister **togavsnittene** i den rekkefølgen føreren kjører dem.

- **Ny tjeneste** legger til en tom tjeneste.
- **+ togavsnitt** legger til neste avsnitt. Velgeren tilbyr avsnittene en fører kan ta som neste:
  de som ikke kolliderer i tid med avsnittene som allerede er i tjenesten, og – når tjenesten har
  et avsnitt – de som begynner når det slutter eller senere. Legger du til avsnittene i kjørerekkefølge, holdes listen til
  de naturlige fortsettelsene, men rekkefølgen du legger dem til i spiller ingen rolle. En skifteoppgave
  skrives med stasjonen én gang og tidene arbeidet pågår mellom, siden den ikke går noe sted.
- Avsnitt trenger **ikke** å møtes på samme stasjon: mellom to avsnitt går føreren dit der neste
  avsnitt begynner.

Et avsnitt kan kjøres av **flere tjenester**, så lenge de går på **ulike kjøresesjoner** – slik at én
tjeneste kan dekke de odde og en annen de like kjøresesjonene på samme avsnitt.

### Kjøresesjoner, selskap og merknader

Bruk **rediger**-knappen (blyanten) for å angi tjenestens identitet, trafikkerende selskap og kjøresesjonene
den kjører, og for å legge til frie **tjenestemerknader** som gjelder hele tjenesten (atskilt fra
merknadene per opphold som vises andre steder).

### Bytte av trekkraftenhet

Når to påfølgende avsnitt av **samme tog** i en tjeneste kjøres av **ulike trekkraftenheter**,
blir føreren hos toget mens lokomotivet byttes. Fanen viser en avledet
merknad ved stasjonen der dette skjer – du legger den ikke inn selv.

### Validering

Når **Innstillinger › Validering › Førertjenester** er slått på, kontrolleres planen slik at ingen togavsnitt
kjøres av to tjenester på en felles kjøresesjon, og ingen tjeneste har avsnitt som overlapper i tid. Konflikter
vises i valideringsindikatoren og åpnes her.

Den samme innstillingen kontrollerer at noen er satt opp på hvert avsnitt som trenger en fører: hvert avsnitt et
lokomotiv eller togsett kjører, og hver **skifteoppgave**, på hver kjøresesjon den går. En oppgave kontrolleres
uansett om et lokomotiv er booket til den, fordi den ikke trenger noe eget lokomotiv, men alltid
trenger noen til å utføre den.
