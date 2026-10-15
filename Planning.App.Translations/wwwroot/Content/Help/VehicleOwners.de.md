Der Reiter **Fahrzeugbesitzer** erfasst, wer welches rollende Material zum Treffen mitbringt.

Im modularen Betrieb werden Lokomotiven, Triebzüge und Wagen von verschiedenen Teilnehmern mitgebracht. Die Erfassung der
Besitzer jedes Stücks macht klar, wer was liefert, und gibt jedem Besitzer die Angaben, die er braucht, um
sein Material vor der ersten Fahrrunde auf der Anlage aufzustellen.

### Rollendes Material

Jede Lokomotive, jeder Triebzug, jede Wagengruppe und jede Rangierlok im Plan ist aufgeführt, mit:

- ihrem **Typ**, der **Fahrzeug**bezeichnung und der **Klasse** – bei einer Wagengruppe, die ihre Wagen auflistet, wird jede Wagen-
  klasse einmal angezeigt, wie in *A/B/Fv*,
- der **Anzahl** der Einheiten – oder aufgelisteten Wagen – nur angezeigt, wenn es mehr als eine gibt,
- der **ersten Fahrrunde** (oder dem **ersten Tag**), in der es im Einsatz ist,
- dem **Bahnhof**, dem **Gleis** und der **Abfahrts**zeit, wo es vor dieser Fahrrunde stehen soll – dem Beginn
  des ersten Zuges, den es in dieser Fahrrunde fährt. Ein Fahrzeug, dem noch kein Umlauf zugewiesen wurde, zeigt **Nicht im Einsatz**;
  eine Rangierlok zeigt nur die Stelle, an der sie stationiert ist,
- ihrem **Besitzer**, der **DCC-Adresse** des Besitzers, den Besitzern, die **Reserven** mitbringen, und einer **Bemerkung** zum Stück.

Haken Sie **Nur die ohne Eigentümer?** an, um zu sehen, was noch niemand mitzubringen angeboten hat.

Ein Fahrzeug, das **Nicht im Einsatz** ist, kann mit seiner Schaltfläche **Löschen** aus dem Plan entfernt werden, und
**Nicht eingesetzte löschen** entfernt alle aufgeführten – nur die, die bei angehaktem Filter
angezeigt werden. Ihre Besitzer werden mit entfernt; die Teilnehmer bleiben. Ein Fahrzeug mit Arbeit muss zuerst im Reiter **Umläufe**
aus seinen Umläufen genommen werden. Eine Rangierlok wird im Reiter **Betriebsstellen** gelöscht, wo sie stationiert ist.

### Besitzer eines Stücks

Öffnen Sie eine Zeile, um ihre Besitzer zu sehen und zu bearbeiten. Der erste Besitzer ist der **primäre**, der die Einheit mitbringt und
auf der Anlage aufstellt. Jeder weitere Besitzer bringt eine **Reserve** mit. **Als primär festlegen** verschiebt einen Reservebesitzer
an die Spitze; wird der primäre Besitzer gelöscht, wird der erste Reservebesitzer primär.

Tippen Sie die ersten Buchstaben eines Namens und wählen Sie den Teilnehmer aus der Liste, oder drücken Sie die **Eingabetaste**, um den
markierten zu übernehmen. Der Anfang jedes Wortes im Namen passt, sodass auch ein Nachname funktioniert. Ein Name, der auf
niemanden passt, wird als **Neuer Teilnehmer** angeboten – prüfen Sie die Schreibweise, bevor Sie ihn hinzufügen.

### DCC-Adressen

Eine Lokomotive, ein Triebzug oder eine Rangierlok braucht für jeden Besitzer eine **DCC-Adresse**, Reserven eingeschlossen, weil jede davon
auf der Anlage gefahren wird. Geben Sie **0** ein, wenn der Besitzer die Adresse noch nicht genannt hat; sie wird als
**Folgt vom Eigentümer** angezeigt. Ein Besitzer kann einer Traktionseinheit nicht ohne Adresse hinzugefügt werden, und eine Adresse kann
geändert, aber nicht gelöscht werden. Ein Warnzeichen markiert eine Traktionseinheit, bei der einem Besitzer noch eine fehlt – was
vorkommt, wenn eine Wagengruppe in eine Lokomotive geändert wird, nachdem ihre Besitzer erfasst wurden.

### Teilnehmer

Die Ansicht **Teilnehmer** listet alle auf, die als Besitzer genannt sind, und was jeder von ihnen mitbringt; das Stück, das sie
selbst mitbringen, ist fett dargestellt. Ein Name wird nur einmal gespeichert, sodass die Korrektur eines falsch geschriebenen Namens hier überall
korrigiert. Ein Teilnehmer, der noch rollendes Material mitbringt, kann nicht gelöscht werden.

### Drucken

**Berichte ▸ Mitgebrachte Fahrzeuge** druckt diese Liste im A4-Querformat, auf eine von drei Arten angeordnet, die
über den Seiten gewählt werden:

- **Nach Betriebsstelle** – eine Seite für jeden Bahnhof, zur Weitergabe an dessen Besitzer: die dort aufzustellenden Fahrzeuge,
  geordnet nach erster Fahrrunde und Abfahrt, mit ihren Besitzern und DCC-Adressen, und die dort stationierten Rangierloks
  zuletzt. Jede Reserve steht in einer eigenen Zeile, direkt unter der Einheit, für die sie einspringt. Die nicht eingesetzten Stücke folgen auf einer eigenen Seite.
- **Nach Eigentümer** – eine Seite für jeden Teilnehmer: was er mitbringt, die selbst aufgestellten Einheiten zuerst, mit
  DCC-Adresse und dem Aufstellungsort jedes Stücks. Die Stücke, die noch niemand mitbringt, stehen zuerst, auf einer Seite
  mit der Überschrift **Noch nicht gebucht**.
- **Nach DCC-Adresse** – alle mitgebrachten Lokomotiven und Triebzüge in einer Liste, niedrigste Adresse zuerst; die fehlenden
  Adressen folgen nach den bekannten. Wagengruppen haben keine Adresse und entfallen.

Jeder Bahnhof und jeder Besitzer beginnt auf einer neuen Seite und wird auf der nächsten Seite unter derselben Überschrift fortgesetzt, wenn eine
Seite nicht reicht. Hat der Besitzer einer Lokomotive oder eines Triebzuges keine DCC-Adresse angegeben oder
**0** eingegeben, wird die Adresse in Rot als **Angeben!** gedruckt.

Der Hintergrund einer Zeile zeigt, wann die Einheit gebraucht wird:

- **weiß** – an allen Fahrrunden (oder Tagen) im Einsatz oder gar nicht im Einsatz,
- **hellgrau** – eine Reserve,
- **hellblau**, **hellgrün** oder **hellrot** – zuerst in der ersten, zweiten oder dritten Fahrrunde im Einsatz,
  aber nicht in allen.
