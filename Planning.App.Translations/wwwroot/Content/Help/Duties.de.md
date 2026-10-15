Im Reiter **Dienste** planen Sie **Lokführerdienste** – die Arbeit, die ein Triebfahrzeugführer während einer
Fahrrunde verrichtet, als Folge der Zugabschnitte, die er fährt.

Ein Dienst wird aus den **Zugabschnitten aufgebaut, die in den Fahrzeugumläufen bereits definiert sind**: den Traktionsabschnitten, die eine Lokomotive oder ein Triebzug fährt, und den **Rangieraufgaben**, die einen Führer brauchen, gleichgültig, ob
eine eigene Lokomotive dafür gebucht ist. Derselbe Zug kann auf mehrere Abschnitte aufgeteilt sein
(zum Beispiel dort, wo die Lokomotive an einer Elektrifizierungsgrenze wechselt), und ein Dienst reiht einfach
die Abschnitte aneinander, die ein einzelner Führer übernimmt.

### Einen Dienst aufbauen

Jeder Dienst ist eine Zeile: Die erste Spalte zeigt seine **Kennung**, die **Gesellschaft** und die **Fahrrunden**, an denen er
verkehrt; die zweite Spalte listet die **Zugabschnitte** in der Reihenfolge auf, in der der Führer sie fährt.

- **Neuer Dienst** fügt einen leeren Dienst hinzu.
- **+ Zugabschnitt** hängt den nächsten Abschnitt an. Die Auswahl bietet die Abschnitte an, die ein Führer als Nächstes übernehmen könnte:
  die, die zeitlich nicht mit den bereits im Dienst enthaltenen Abschnitten kollidieren und – sobald der Dienst
  einen Abschnitt hat – die, die zum Ende dieses Abschnitts oder danach beginnen. Fügen Sie Abschnitte in Fahrtreihenfolge hinzu, bleibt diese Liste bei
  den natürlichen Fortsetzungen, aber die Reihenfolge, in der Sie sie hinzufügen, spielt keine Rolle. Eine Rangieraufgabe
  wird mit dem Bahnhof einmal und den Zeiten erfasst, zwischen denen die Arbeit läuft, da sie nirgendwohin fährt.
- Abschnitte müssen **nicht** am selben Bahnhof zusammentreffen: Zwischen zwei Abschnitten geht der Führer dorthin, wo der nächste
  Abschnitt beginnt.

Ein Abschnitt kann von **mehreren Diensten** gefahren werden, solange diese an **verschiedenen Fahrrunden** verkehren – so kann ein
Dienst die ungeraden und ein anderer die geraden Fahrrunden desselben Abschnitts abdecken.

### Fahrrunden, Gesellschaft und Anmerkungen

Mit dem **Bearbeiten**-Symbol (Bleistift) legen Sie die Kennung des Dienstes, die betreibende Gesellschaft und die Fahrrunden fest,
an denen er verkehrt, und fügen freie **Dienstanmerkungen** hinzu, die für den ganzen Dienst gelten (getrennt von den
Anmerkungen je Halt, die an anderer Stelle angezeigt werden).

### Wechsel der Traktionseinheit

Werden zwei aufeinanderfolgende Abschnitte **desselben Zuges** in einem Dienst von **verschiedenen Traktionseinheiten** gefahren, bleibt der Führer beim Zug, während die Lokomotive gewechselt wird. Der Reiter zeigt einen abgeleiteten
Hinweis an dem Bahnhof, an dem dies geschieht – Sie geben ihn nicht von Hand ein.

### Validierung

Ist **Einstellungen › Validierung › Lokführerdienste** aktiviert, wird der Plan daraufhin geprüft, dass kein Zugabschnitt
von zwei Diensten an einer gemeinsamen Fahrrunde gefahren wird und kein Dienst zeitlich überlappende Abschnitte hat. Konflikte
werden im Validierungsindikator aufgelistet und öffnen sich hier.

Dieselbe Einstellung prüft, dass für jeden Abschnitt, der einen Führer braucht, jemand eingeteilt ist: jeder Abschnitt, den eine
Lokomotive oder ein Triebzug fährt, und jede **Rangieraufgabe**, an jeder Fahrrunde, an der sie verkehrt. Eine Aufgabe wird
geprüft, gleichgültig, ob eine Lokomotive dafür gebucht ist, da sie keine eigene Lokomotive braucht, aber immer
jemanden, der sie ausführt.
