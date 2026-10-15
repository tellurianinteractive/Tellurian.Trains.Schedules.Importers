Du bruker fanen **Innstillinger** til å styre hvordan gjeldende anleggs
ruteplan beregnes, tegnes og valideres.
Innstillingene lagres i planleggingsdokumentet.

### Generelt

Gi anlegget et navn og velg driftsvinduet.
Du kan velge å bruke **nummererte kjøresesjoner** eller **ukedager** når du trenger
ulik drift på ulike kjøresesjoner. Hvis alle tog, omløp og tjenester
går likt alle kjøresesjoner, trenger du ikke gjøre noe – alt gjelder da som standard **alle kjøresesjoner**.

**Bruk passasjerbilletter** betyr at treffet kjøres med billetter: en passasjer kjøper en der
reisen begynner og reiser på den. Når den er slått på, kan du på fanen **Driftssteder** merke
stasjonene der passasjerer bytter tog. Disse omstigningene vises deretter som en ankomstmerknad i
førertjenestene og kan listes opp i heftet med generelle instruksjoner.

Rapporter skrives ut på anleggets **standardspråk**: det første språket i standardlandet,
uansett hvilket språk du selv arbeider på. **Skriv ut rapporter på lokale språk** skriver i stedet ut hver førertjeneste på språket
til selskapet som kjører den, hvert omløpskort på språket til kjøretøyets selskap og hver stasjons
togekspederingsliste på språket i stasjonens land.
Alt som mangler et slikt språk, beholder standardspråket.

### Kjøresesjoner & dager

I noen planleggingsscenarier er det nyttig å kunne velge hvilke kjøresesjoner/dager som trafikkeres.
Du kan angi trafikkerte kjøresesjoner/dager for:
- Tog, for eksempel ett tog som går kjøresesjon 1–3 og et annet som går 4–6.
- Kjøretøy, for eksempel to eller tre dagers rotasjon av lokomotiver.
- Tjenester, når oppgavene er ulike mellom kjøresesjoner.

### Tid & hastighet

Angi hurtigklokkens fart og tilordne hastighetsklasser til skala og reelle hastigheter. Standardtidene for
stasjoner (korteste opphold, lokomotivomkjøring, togklarering) brukes når tider beregnes for
stasjoner som ikke har egne verdier.

### Validering

Velg hvilke kontroller som skal kjøres og hvilke terskler de bruker. Slår du av en kontroll, undertrykkes dens
meldinger uten at underliggende data fjernes.

### Grafisk ruteplan

**Felles innstillinger** styrer hva grafen viser – tidsaksens orientering og hvilke
etiketter (ankomst-/avgangsminutter, togkategori, selskap) som tegnes. De gjelder både på skjermen
og i den utskrevne rapporten. Merk at hvis tog fortsetter etter 24:00, brytes de rundt og fortsetter fra 00:00.

Under dem inneholder blokkene **på skjermen** og **utskrevet rapport** de samme typene avstander for
hver av de to: skjermavstander oppgis i piksler, utskriftsavstander i millimeter papir. Utskriftsmålene er i skala,
slik at utskrevne grafer kan sammenlignes og måles; antall sider følger av dem.

### Integrasjon

*Kommer*: API-nøkler og innstillinger for tilkobling til eksterne tjenester, for eksempel Module Registry.
