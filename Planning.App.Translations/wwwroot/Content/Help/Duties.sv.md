På fliken **Tjänster** planerar du **förartjänster** – det arbete en lokförare utför under en
köromgång, som en följd av de tågavsnitt föraren kör.

En tjänst byggs av de **tågavsnitt som redan är definierade i fordonsomloppen**: de dragande
avsnitt som ett lok eller ett tågsätt kör, och **växlingsuppgifterna**, som behöver en förare oavsett om
ett eget lok är bokat för dem. Samma tåg kan delas upp på flera avsnitt
(till exempel där loket byts vid en elektrifieringsgräns), och en tjänst knyter helt enkelt ihop
de avsnitt som en enskild förare tar.

### Bygga en tjänst

Varje tjänst är en rad: den första kolumnen visar dess **identitet**, **företag** och de **köromgångar** den
körs. Den andra kolumnen listar **tågavsnitten** i den ordning föraren kör dem.

- **Ny tjänst** lägger till en tom tjänst.
- **+ tågavsnitt** lägger till nästa avsnitt. Väljaren erbjuder de avsnitt som en förare kan ta härnäst:
  de som inte krockar i tid med avsnitten som redan ingår i tjänsten och – när tjänsten har
  ett avsnitt – de som börjar när det tar slut eller senare. Lägger du till avsnitten i körordning hålls listan till
  de naturliga fortsättningarna, men i vilken ordning du lägger till dem spelar ingen roll. En växlingsuppgift
  skrivs med stationen en gång och de tider arbetet pågår mellan, eftersom den inte går någonstans.
- Avsnitten behöver **inte** mötas på samma station: mellan två avsnitt går föraren dit där nästa
  avsnitt börjar.

Ett avsnitt kan köras av **flera tjänster**, så länge de går på **olika köromgångar** – en
tjänst kan alltså täcka de udda köromgångarna och en annan de jämna köromgångarna på samma sträcka.

### Köromgångar, företag och anmärkningar

Med **redigera**-knappen (pennan) anger du tjänstens identitet, trafikerande företag och de
köromgångar den körs, och lägger till fria **tjänsteanmärkningar** som gäller hela tjänsten (skilda från
de anmärkningar per uppehåll som visas på andra ställen).

### Byte av dragfordon

När två på varandra följande avsnitt av **samma tåg** i en tjänst körs med **olika dragfordon**
stannar föraren kvar hos tåget medan loket byts. Fliken visar en härledd
anmärkning vid den station där detta sker – du anger den inte för hand.

### Validering

När **Inställningar › Validering › Förartjänster** är aktiverat kontrolleras planen så att inget tågavsnitt
körs av två tjänster på en gemensam köromgång och att ingen tjänst har avsnitt som överlappar i tid. Konflikter
visas i valideringsindikatorn och öppnas här.

Samma inställning kontrollerar att någon är schemalagd för varje avsnitt som behöver en förare: varje avsnitt som
ett lok eller ett tågsätt kör, och varje **växlingsuppgift**, på varje köromgång det går. En uppgift kontrolleras
oavsett om ett lok är bokat för den, eftersom den inte behöver något eget lok men alltid
behöver någon som utför den.
