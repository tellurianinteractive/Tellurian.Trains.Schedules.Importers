# Versionshinweise

## Version 0.8.3

### Änderungen

- **Der Bericht Mitgebrachte Fahrzeuge ist danach geordnet, wofür jede Seite gebraucht wird.** **Nach
  Eigentümer** geordnet, listet die Seite eines Teilnehmenden das Mitgebrachte nach der ersten Sitzung oder
  dem ersten Tag, an dem es gebraucht wird, dann nach Bahnhof, Abfahrt und Gleis. **Nach Betriebsstelle**
  geordnet, listet die Seite eines Bahnhofs die Fahrzeuge nach Gleisnummer, Gleis 2 vor Gleis 10, und auf
  jedem Gleis nach Abfahrt.
- **Die Farben der Zeilen sind einfacher.** Auf der Seite eines Eigentümers ist nur ein Fahrzeug hellgelb,
  das in der ersten Sitzung oder am ersten Tag nicht gebraucht wird. Auf der Seite eines Bahnhofs ist ein
  Fahrzeug, das nicht in allen Sitzungen oder an allen Tagen im Einsatz ist, hellgelb, wenn es in einer
  ungeraden Sitzung oder an einem ungeraden Tag beginnt, und hellblau, wenn es in einer geraden beginnt.
  Reservefahrzeuge bleiben hellgrau.
- **Lokomotiven und Triebzüge können eine Fahrzeugnummer erhalten.** Unter **Fahrzeugbesitzer**,
  **Rollendes Material**, kann jeder Eigentümer einer Lokomotive oder eines Triebzugs neben der DCC-Adresse
  die Nummer des Fahrzeugs eintragen, das er mitbringt, auch die der Reservefahrzeuge. Der Bericht
  Mitgebrachte Fahrzeuge druckt sie direkt nach dem Umlauf. Die Nummern einer Wagengruppe sind weiterhin die
  ihrer Wagen.

## Version 0.8.2

### Änderungen

- **Die Züge einer Kategorie können neu nummeriert werden.** Ändern Sie auf dem Reiter
  **Zugkategorien** die **Startnummer** und klicken Sie daneben auf **Neu nummerieren**. Alle Züge der
  Kategorie werden um denselben Betrag verschoben, sodass der Zug mit der niedrigsten Nummer die erste
  Nummer ab der Startnummer erhält. Kein Zug wechselt zwischen ungerade und gerade, und Lücken und
  Nummernpaare bleiben erhalten: Mit der Startnummer 100 werden die Züge 1, 2 und 3 zu 101, 102 und 103.
  Eine Rangierkategorie kennt kein Ungerade und Gerade, daher erhält ihre erste Aufgabe die Startnummer
  selbst.
- **Ein Fahrzeug, das vor seinem ersten Zug vom Abstellgleis geholt wird, muss nach seinem letzten
  abgestellt werden.** Wird eine Lok oder ein Triebzug vor seinem ersten Zug einer Fahrrunde vom
  Abstellgleis geholt oder aufgegleist, zeigt die Prüfung jetzt eine Warnung, sofern es nicht auch nach
  seinem letzten Zug, wo es zu diesem Bahnhof zurückkehrt, abgestellt oder abgehoben wird — in derselben
  Fahrrunde oder in einer späteren, wenn es über mehrere Fahrrunden umläuft.

## Version 0.8.1

### Änderungen

- **Eine Lok oder ein Triebzug kann zwischen zwei Zügen abgestellt oder abgehoben werden.** Beim
  Bearbeiten eines Zugteils in einem Fahrzeugplan geben **Triebfahrzeug vor Abfahrt** und
  **Triebfahrzeug nach Ankunft** an, wo das Triebfahrzeug ist: auf dem Gleis, auf dem Abstellgleis, oder
  von der Anlage abgehoben, etwa auf einen Tisch während einer langen Wartezeit. Lokführer und
  Fahrdienstleiter erhalten einen Vermerk, es zum oder vom Abstellgleis zu fahren, abzuheben oder
  aufzugleisen. Die Wahl gilt auch für den Zugteil davor oder danach, sodass ein abgehobenes Fahrzeug vor
  dem nächsten Zug wieder aufgegleist wird. Stimmen beide nicht überein, zeigt die Prüfung eine Warnung.
- **Holen von und Abstellen auf bleiben mit dem Zugteil davor oder danach im Einklang.** Wählen Sie das
  Gleis, auf das die Fahrzeuge nach der Ankunft gestellt werden, holt der nächste Zugteil sie von diesem
  Gleis, und umgekehrt. Ist ein Gleis an einem Ende angegeben, am anderen aber nicht getroffen, zeigt die
  Prüfung eine Warnung.
- **Die Vermerke zum Holen von oder Abstellen auf einem Gleis nennen, wofür das Gleis dient.** Ein Gleis
  mit einer Verwendung auf dem Reiter **Betriebsstellen** wird mit ihr genannt, wie in „Gleis 5
  (Lokschuppen)“. Ein Gleis ohne Nummer wird nur mit seiner Verwendung genannt.

## Version 0.8.0

### Änderungen

- **Der Reiter Züge kann die Halte an einer Betriebsstelle zeigen.** Auf dem Reiter **Züge** kann jetzt
  zwischen **Pro Zug** und **Pro Betriebsstelle** gewählt werden. Pro Betriebsstelle listet jeden Zug, der
  die gewählte Betriebsstelle berührt, in der Reihenfolge, in der er dort ankommt: ob er beginnt, endet,
  hält oder durchfährt, woher er kommt und wohin er weiterfährt, seine Zeiten und sein Gleis. Wählen Sie in
  der Liste ein anderes Gleis, um den Zug dorthin zu verlegen, ohne jeden Zug einzeln zu öffnen. Ein
  Konflikt an der Betriebsstelle, etwa zwei Züge gleichzeitig auf einem Gleis, wird in der Zeile markiert.
  Der Reiter merkt sich die gewählte Ansicht und Betriebsstelle.
- **Eine Betriebsstelle, die von Zügen berührt wird, lässt sich löschen.** Auf dem Reiter
  **Betriebsstellen** ist **Löschen** nicht mehr gesperrt, solange Züge dort halten oder durchfahren.
  Stattdessen wird Ihnen zuerst alles gezeigt, was das Löschen ändern würde, und Sie bestätigen oder
  brechen ab. Die Züge verlieren ihre Halte dort: ein durchfahrender Zug fährt jetzt einfach vorbei, und
  einer, der dort beginnt oder endet, beginnt oder endet jetzt an seinem nächsten oder vorigen Halt. Liegt
  die Betriebsstelle zwischen genau zwei Nachbarn, werden ihre beiden Streckenabschnitte durch einen
  ersetzt, der die Nachbarn verbindet, so lang und mit derselben Fahrzeit wie die beiden zusammen, und die
  Fahrplanstrecken durch sie führen über diesen. Das Löschen wird mit Angabe der Gründe abgelehnt, solange
  ein Fahrzeugumlauf, ein Triebfahrzeugführerdienst oder ein Wagenstrom dort beginnt oder endet, oder
  solange ein Zug dort wendet oder sie als Abzweig durchfährt.

### Fehlerbehebungen

- **Eine Auswahlliste zeigt keine andere Wahl mehr als die getroffene.** Änderten sich die angebotenen
  Einträge einer Liste, der gewählte Wert aber nicht, konnte die Liste einen anderen Eintrag als den
  gespeicherten zeigen.

## Version 0.7.9

### Änderungen

- **Zugbildungen benennen weitergeleitete Wagen nach ihrer Herkunft.** Ein Wagenstrom mit
  **Herkunftsbetriebsstellen** wird in seinem Rechteck jetzt als „Wagen von“ seinen Herkunftsorten gezeigt
  statt mit seinen Zielen, sodass die Wagen danach gefunden werden können, woher sie kommen. Andere
  Wagenströme an derselben Stelle im Zug zeigen weiterhin ihre Ziele, und die Begrenzung zählt weiterhin
  alle mit.

## Version 0.7.8

### Änderungen

- **Ein Zug, der an einem nicht geplanten Gleis ankommt oder abfährt, wird gemeldet.** Ein Zug mit
  Ankunft oder Abfahrt an einem Gleis, dessen Kästchen **Geplant?** auf dem Reiter **Betriebsstellen**
  nicht angekreuzt ist, erscheint jetzt unter **Konflikte**, mit Zug, Betriebsstelle, Zeit und Gleis. Ein
  Zug, der an einem solchen Gleis nur wartet, etwa auf eine Kreuzung, erscheint nicht, ebenso wenig eine
  Rangieraufgabe. Nichts wird für Sie geändert: verlegen Sie entweder den Halt auf dem Reiter **Züge** an
  ein geplantes Gleis, oder kreuzen Sie das Kästchen **Geplant?** des Gleises an. Die Prüfung lässt sich
  unter **Einstellungen › Validierung** abschalten.

## Version 0.7.7

### Änderungen

- **Nicht eingesetzte Fahrzeuge lassen sich löschen.** Auf dem Reiter **Fahrzeugbesitzer** hat ein
  Fahrzeug, das als **Nicht im Einsatz** gezeigt wird, eine Schaltfläche **Löschen**, und
  **Nicht eingesetzte löschen** entfernt alle solchen aufgeführten Fahrzeuge — nur die gezeigten, wenn
  **Nur die ohne Eigentümer?** angekreuzt ist. Beide fragen zuerst nach. Die Eigentümer des Fahrzeugs werden
  mit entfernt; die Teilnehmer bleiben. Ein Fahrzeug mit Aufgaben muss zuerst auf dem Reiter **Umläufe** aus
  seinen Umläufen genommen werden.
- **Der Bericht Mitgebrachte Fahrzeuge benennt jedes Fahrzeug wie der Reiter Umläufe.** Die eigene Spalte
  **Klasse** entfällt: Die Spalte **Umlauf** enthält jetzt Kürzel des Betreibers, Nummer und Klasse
  (z. B. „SJ 01 Rc“), bei einem Fahrzeug mit externer Id diese Id, wie auf dem Reiter **Umläufe**.

## Version 0.7.6

### Änderungen

- **Die Liste der Fahrzeugbesitzer benennt jedes Fahrzeug wie der Reiter Umläufe.** Auf dem Reiter
  **Fahrzeugbesitzer** sind die getrennten Spalten **Fahrzeug** und **Klasse** jetzt eine Spalte
  **Fahrzeug** mit Kürzel des Betreibers, Nummer und Klasse (z. B. „DB 05 BR 218“), bei einer Wagengruppe
  mit den aufgeführten Wagen (z. B. „SJ 05 5 x A/B/Fv“). Ein Fahrzeug mit externer Id wird mit dieser Id
  gezeigt, wie auf dem Reiter **Umläufe**.
- **Der Bericht Mitgebrachte Fahrzeuge zeichnet die Wagen einer Wagengruppe.** Eine Wagengruppe, die ihre
  Wagen aufführt, zeigt sie jetzt in ihrer Bemerkung, ein Rechteck je Wagen mit Klasse und Nummer, in der
  Reihenfolge, in der sie im Zug stehen, so wie der Bericht **Zugbildungen** sie zeichnet. Eine lange
  Wagengruppe wird auf weitere Zeilen umbrochen.

## Version 0.7.5

### Änderungen

- **Ein neuer Dienst beginnt mit seinem ersten Zugabschnitt.** **Neuer Dienst** auf dem Reiter **Dienste**
  öffnet jetzt sofort den Dialog **Zugabschnitt hinzufügen**, sodass der Dienst gleich seinen Platz im
  Diagramm erhält, statt leer ganz unten zu stehen. Wird der Dialog geschlossen, ohne einen Abschnitt
  hinzuzufügen, bleibt kein leerer Dienst zurück.
- **Zug hinzufügen bietet nur Orte an, an denen die Kategorie hält.** Im Dialog **Zug hinzufügen** sind
  Start- und Zielbetriebsstelle auf die im Haltemuster der gewählten Zugkategorie beschränkt. Eine Kategorie
  ohne Haltemuster schränkt nichts ein. Ein Kategoriewechsel leert eine Betriebsstelle, die nicht mehr zur
  Auswahl steht.
- **Ein Treffen rund um die Uhr kann zu jeder Tageszeit beginnen.** Ist **Läuft über Mitternacht?** auf dem
  Reiter **Einstellungen** angekreuzt, gibt **Erste Fahrrunde beginnt** die Zeit an, zu der die erste
  Fahrrunde oder der erste Tag beginnt. Jedes Fahrzeug beginnt dort, wo es zu dieser Zeit steht: Ein
  Zugabschnitt der ersten Fahrrunde, der früher abfährt, ist nicht sein Startpunkt, und ein Fahrzeug ohne
  spätere Fahrt in dieser Fahrrunde beginnt in der nächsten, in der es fährt. Der Reiter
  **Fahrzeugbesitzer** und der Bericht **Mitgebrachte Fahrzeuge** zeigen den Start auf diese Weise.

## Version 0.7.4

### Änderungen

- **Ein Dienstheft zeigt jeden Zug einmal.** Wo ein Dienst einen Zug in mehrere Abschnitte teilt — weil
  das Triebfahrzeug wechselt oder unterwegs Wagen an- oder abgekuppelt werden —, der Lokführer aber die
  ganze Zeit auf dem Zug bleibt, druckt das Heft den Zug jetzt einmal, von dort, wo der Lokführer ihn
  übernimmt, bis dort, wo er ihn verlässt, statt einer Seite je Abschnitt. Die Blöcke der Triebfahrzeuge
  und Wagengruppen zeigen, welches Fahrzeug welchen Teil des Zuges fährt.
- **Güterwagen, die zusammen stehen, sind eine Zeile im Dienstheft.** Wagenströme, die am selben Bahnhof
  an derselben Position im Zug angekuppelt werden, teilen sich jetzt eine Zeile im Block der Güterwagen mit
  Frachtbriefen, so wie die Zugbildungen sie zeigen: jeder Ort einmal genannt, die Regionen nach den Orten
  und eine Höchstlast für die ganze Gruppe, die Summe der Höchstlasten ihrer Ziele. Die Zeilen sind nach
  dem Bahnhof geordnet, an dem die Wagen angekuppelt werden, in der Reihenfolge, in der der Zug ihn
  erreicht, und dann nach der Position.
- **Zugbildungen zeigen den ganzen Zug, wo eine Wagengruppe angekuppelt wird.** Ein Zug erhält weiterhin
  nur dort eine Zeile, wo eine Wagengruppe angekuppelt wird oder Wagen aus Wagenströmen mitgenommen werden,
  aber die Zeile zeigt jetzt jede Wagengruppe, mit der der Zug abfährt, auch die, die er schon mitführt,
  sodass die Positionen der dort angekuppelten verständlich sind.
- **Zugbildungen rahmen jede Wagengruppe ein.** Der Umlauf einer Wagengruppe und ihre Wagen stehen jetzt
  zusammen in einem schattierten Rahmen, sodass sie als eine Gruppe unter einem Umlauf gelesen werden statt
  der Umlauf als eine weitere Wagengruppe daneben.
- **Eine Höchstlast je Güterrechteck in den Zugbildungen.** Ein Güterrechteck mit mehreren Zielen nennt
  jetzt die Summe ihrer Höchstlasten einmal, zuletzt, statt einer Zahl je Ziel.
- **Ein Wagen, der einer Wagengruppe hinzugefügt wird, ist ein Personenwagen.** Ein neuer Wagen im Dialog
  **Fahrzeug bearbeiten** ist jetzt zunächst ein Personenwagen, kein Güterwagen.

### Fehlerbehebungen

- **Zeiten nach Mitternacht landen am richtigen Tag.** Auf einer Anlage mit angekreuztem **Läuft über
  Mitternacht?** kommt eine Zeit nach Mitternacht, die bei einem Zug über Mitternacht eingegeben wird —
  etwa 00:10 anstelle von 23:55 —, jetzt auf den nächsten Tag, sodass die Halte des Zuges in der
  Reihenfolge bleiben, in der er fährt. Früher gespeicherte Pläne mit solchen Zeiten werden beim Öffnen
  berichtigt.

## Version 0.7.3

### Änderungen

- **Ankunftsgleise lassen sich dorthin legen, wo der nächste Zug abfährt.** Eine neue Schaltfläche
  für **Ankunftsgleise** an jedem Umlauf auf der Registerkarte **Umläufe** legt die Ankünfte dieses
  Umlaufs auf das Gleis, von dem der nächste Zug abfährt, unabhängig vom Fahrzeug. Ein Umlauf mit
  Triebzug oder Wendezug wird weiterhin von selbst angepasst, und jetzt auch, wenn **Wendezug?** bei einer
  bereits zugewiesenen Lok angekreuzt wird.
- **In der Liste der Betriebsstellen lässt es sich leichter arbeiten.** Die Schaltflächen einer
  Betriebsstelle stehen jetzt direkt hinter ihrem Namen statt am fernen Ende der breiten Zeile, sodass
  klar ist, zu welcher Betriebsstelle sie gehören. Ein Klick auf den Namen selbst öffnet die Informationen
  zur Betriebsstelle. Jede zweite Zeile ist schattiert, und die Zeile unter dem Mauszeiger wird deutlich
  hervorgehoben.
- **Ein Bahnhof zeigt die Orte, die er mit Gütern bedient.** Auf der Registerkarte **Betriebsstellen**
  listen die Informationen zu einem Bahnhof jetzt die Orte auf, deren Güter von ihm aus bedient werden, und
  die Informationen zu einem Ort zeigen, welcher Bahnhof ihn bedient. Die gedruckten Blätter der
  Betriebsstellen führen die bedienten Orte ebenfalls auf.
- **Zugbildungen zeigen ankommende Wagen aus Wagenströmen.** Ein Zug, der mit Wagen aus Wagenströmen
  ankommt, die an einem Bahnhof abgekuppelt werden, erhält jetzt eine eigene Zeile auf dem Blatt dieses
  Bahnhofs, mit seiner Abfahrtszeit, wo der Zug weiterfährt, und mit einem gestrichelten Rechteck je
  Position im Zug, das angibt, woher die Wagen kamen. Gezeigt werden nur Wagenströme mit angekreuztem
  **Abkuppeln?**, außer in einem Schattenbahnhof, wo alle ankommenden Wagen gezeigt werden.
  Schattenbahnhöfe erhalten jetzt ebenfalls eigene Blätter.
- **Zugbildungen zeigen jede Wagengruppe dort, wo sie angekuppelt wird.** Eine Wagengruppe, die ihre
  Wagen nicht aufführt, wird jetzt ebenfalls gezeichnet, allein durch ihr schattiertes Umlauf-Rechteck.
  Eine Wagengruppe wird nur dort gezeigt, wo sie angekuppelt wird: bei der ersten Abfahrt ihres Umlaufs
  und danach nur, wo sie ausdrücklich angekuppelt wird — mit einem Kuppelvermerk, von einem anderen Gleis
  geholt oder an einen bereits fahrenden Zug gekuppelt. Eine Wagengruppe, die von Zug zu Zug bei ihrer
  Lok bleibt, wird nicht erneut gezeigt. Wo eine Wagengruppe im Zug steht, wird je Ankupplung festgelegt:
  **Position** im Dialog **Zugabschnitt bearbeiten** auf dem Reiter **Umläufe**, sodass mehrere
  Wagengruppen, die am selben Bahnhof angekuppelt werden, in dieser Reihenfolge gezeichnet werden.
- **Zugbildungen brauchen weniger Platz.** Die Ziele, und die Herkunftsorte ankommender Wagen, laufen
  jetzt als eine kommagetrennte Liste in ihrem Rechteck weiter statt einer pro Zeile. Die Spalte
  **Umlauf** entfällt: Der Umlauf jeder Wagengruppe steht in einem schattierten Rechteck vor ihren Wagen,
  was den Zugbildungen mehr Breite gibt. Die Spalte **Nach** heißt jetzt **Nach/von** und sagt *nach*,
  wohin ein abfahrender Zug fährt, und *von*, woher ein ankommender Zug kam.
- **Zugbildungen werden so gezeichnet, wie die Züge fahren.** Jeder Zug beginnt jetzt mit einem
  Lok-Rechteck an dem Ende, auf das er zufährt, mit einem Pfeil, und die Wagen folgen dahinter, sodass
  die Reihenfolge auf dem Papier die Reihenfolge auf dem Gleis ist. Die Lok enthält die Tage oder
  Fahrrunden, den Zug und seine Zeiten am Bahnhof (**06:00-06:45**) und zuletzt die Höchstlast des Zuges
  und ersetzt fünf Spalten. Ein Zug, der in der festgelegten Richtung seines Streckenabschnitts fährt,
  zeigt nach rechts; einer, der ihr entgegen fährt, nach links. Auf jede Seite folgt ihr Spiegelbild für
  die andere Seite der Gleise: Drucken Sie doppelseitig und drehen Sie das Blatt auf die Seite, die zu dem
  passt, was Sie sehen. Die benachbarten Betriebsstellen stehen an beiden Enden der Überschrift der
  Zugbildung.
- **Regionen zuletzt in Zugbildungen.** Wo mehrere Ziele eine Position im Zug teilen, werden erst alle
  Orte aufgeführt und danach ihre Regionen, jede Region einmal.

## Version 0.7.2

### Neue Funktionen

- **Betriebsstellen können jetzt gedruckt werden.** Ein neuer Bericht unter **Berichte** gibt jeder
  Betriebsstelle der Anlage ein eigenes Blatt im A4-Hochformat: ihre Eigenschaften, ihre Regionen, die
  Anweisungen, wie sie beim Treffen bedient wird, und ihre Gleise mit Längen, Bahnsteigen, Fahrwegen und
  Verwendung. Aufgeführt werden nur die Eigenschaften, die für die Art der Betriebsstelle gelten, und die
  gezeigten Betriebszeiten sind die geltenden — die der Betriebsstelle selbst, sonst die der Anlage. Die
  Anweisungen werden hier zum ersten Mal gedruckt. Eine Betriebsstelle, die mehr enthält, als auf ein Blatt
  passt, wird auf dem nächsten fortgesetzt, und die nächste Betriebsstelle beginnt trotzdem auf einem neuen
  Blatt. Ist **Berichte in Landessprachen drucken** angekreuzt, wird jedes Blatt in der Sprache des Landes
  der Betriebsstelle gedruckt.

### Änderungen

- **Nur die geltenden Betriebszeiten werden angeboten.** Auf der Registerkarte **Betriebsstellen** wird die
  Zeit für das Umsetzen der Lok nur bei Bahnhöfen angeboten, Schattenbahnhöfe eingeschlossen, und die Zeit
  für die Zugabfertigung nur bei besetzten Bahnhöfen, da dafür ein Fahrdienstleiter im Dienst sein muss.
  Eine früher eingetragene Zeit, wo sie nicht mehr gilt, bleibt erhalten, wird aber weder angezeigt noch gedruckt.

## Version 0.7.1

### Neue Funktionen

- **Berichte werden in der Sprache der Anlage gedruckt.** Alle Berichte werden jetzt in der
  Standardsprache der Anlage gedruckt, der ersten Sprache ihres Standardlandes, gleich in welcher
  Sprache Sie selbst arbeiten. Die gedruckten Blätter lesen die Teilnehmer am Treffen, nicht Sie.
  Datum und Zahlen richten sich ebenfalls nach dem Land.

  Mit **Berichte in Landessprachen drucken** unter **Einstellungen › Allgemein** wird jeder
  Lokführerdienst in der Sprache des Unternehmens gedruckt, das ihn fährt, jede Umlaufkarte in der
  Sprache des Unternehmens des Fahrzeugs und die Zugmeldeliste jedes Bahnhofs in der Sprache seines
  Landes. Das gilt auch für Zugmeldelisten, die als Dokumente gespeichert werden. Ein Dienst oder eine
  Karte ohne Unternehmen erhält die Sprache der Betreiber der Züge, sofern sie alle dieselbe haben.
  Alles ohne eine Sprache, in der die App drucken kann, behält die Standardsprache.

- **Eine Zugkategorie sagt, wo ihre Züge halten.** Der Reiter **Zugkategorien** hat ein **Haltemuster**:
  Kreuze bei den Betriebsstellen, an denen Züge der Kategorie unterwegs halten. Ein neuer Zug, der im
  Reiter **Züge** angelegt wird, erhält an jeder angekreuzten Betriebsstelle, die er befährt, einen Halt
  und durchfährt die übrigen — wo ein Zug beginnt und wo er endet, ist immer ein Halt, ganz gleich, was
  angekreuzt ist, denn dort wird er bereitgestellt und abgestellt. Angeboten werden nur die
  Betriebsstellen, an denen die Kategorie überhaupt halten kann.

  Ein Zug, der anderswo hält, erscheint unter **Konflikte**, mit Zug, Betriebsstelle und Zeit. Nichts wird
  für Sie berichtigt: nur Sie können entscheiden, ob der Zug hält, wo er durchfahren sollte, oder ob dem
  Muster eine Betriebsstelle fehlt, an der die Züge der Kategorie halten. Die Prüfung lässt sich unter
  **Einstellungen › Validierung** abschalten.

  **Aus den Zügen übernehmen** kreuzt die Betriebsstellen an, an denen die vorhandenen Züge der Kategorie
  halten, und das geschieht für Sie, sobald ein Plan aus einer früheren Version zum ersten Mal geöffnet
  wird — jede Kategorie erhält das Muster, das ihre Züge die ganze Zeit gefahren sind, sodass nichts
  gemeldet wird, was vorher kein Fehler war. Kreuzen Sie nichts an, bleibt die Kategorie ungebunden: ihre
  Züge halten dann überall dort, wo sie abgeben können, was sie befördern, wie bisher. Eine
  Rangierkategorie hat kein Haltemuster, da ihre Aufgaben nirgendwohin fahren.

- **Eine Rangieraufgabe braucht keine eigene Lok, aber sie braucht einen Lokführer.** Eine Aufgabe wird
  ebenso oft von der Zuglok erledigt, die ohnehin im Bahnhof steht, oder von einer Rangierlok, die Sie
  nicht als Fahrzeug angelegt haben, wie von einer eigens dafür eingeteilten. Ein **Umlauf**, der nichts
  als Rangieraufgaben enthält, ist deshalb ohne zugewiesenes Fahrzeug vollständig und erscheint nicht
  mehr unter **Konflikte** als Umlauf ohne Fahrzeug. Nehmen Sie einen fahrenden Zug in denselben Umlauf
  auf, wird wieder ein Fahrzeug verlangt.

  Was eine Aufgabe sehr wohl braucht, ist jemanden, der sie ausführt, und so wird sie nun im Reiter
  **Dienste** wie jeder andere Zugabschnitt angeboten, mit oder ohne eigene Lok — mit der Betriebsstelle
  einmal geschrieben und den Zeiten, zwischen denen die Arbeit läuft, denn sie fährt nirgendwohin. Eine
  Aufgabe, die kein Dienst abdeckt, erscheint unter **Konflikte** für die Sessionen, in denen sie
  unbesetzt bleibt, neben den Abschnitten, die eine Lok zieht.

  Im gedruckten Dienstheft trägt die Seite der Aufgabe die Überschrift **Rangieraufgabe** mit der Signatur
  des Betreibers, nicht eine Zugnummer, die niemand verwendet, und wo ein Zug seinen Fahrplan hat, hat eine
  Aufgabe einen eigenen Block: **Arbeitszeiten**, eine Zeile mit der Betriebsstelle und den Zeiten, zu
  denen die Arbeit **beginnt** und **endet**, darunter die Rangieranweisungen. Ein Gleis wird nicht
  genannt — die Arbeit wird im ganzen Bahnhof getan, nicht von einem Gleis aus.

- **Die Balken für Lokführer rechnen die Wartezeit in einem Dienst mit.** Die Balken über dem Bildfahrplan
  zählten einen Lokführer nur als benötigt, solange ein Zug oder eine Rangieraufgabe gefahren
  wurde. Ein Lokführer, dessen **Dienst** eine Lücke zwischen zwei Zügen hat, ist währenddessen trotzdem
  gebunden, wartend oder auf dem Weg zum nächsten, deshalb zählt diese Zeit jetzt auch mit – an den Sitzungen,
  an denen der Dienst gefahren wird. Ebenso ein selbst gesetzter Dienstbeginn vor dem ersten Zug des
  Dienstes oder ein Dienstende nach seinem letzten.

- **Züge auf der Strecke werden so geprüft, wie die Fahrdienstleiter sie sehen.** Die Liste
  **Konflikte** betrachtete bisher einen Streckenabschnitt nach dem anderen, sodass zwei Züge an einem
  unbesetzten Bahnhof kreuzen oder einander an einem vorbei folgen konnten, ohne dass es gemeldet wurde.
  Jetzt betrachtet sie jeden Zugleitabschnitt als Ganzes. Eine signalgesteuerte Stelle wie eine
  Blockstelle teilt den Zugleitabschnitt in Abschnitte, die je Gleis einen Zug aufnehmen können. Auf
  eingleisiger Strecke können sich Züge in Gegenrichtung nur an den Enden begegnen oder an einer
  signalgesteuerten Stelle, an der Züge kreuzen können. Züge in derselben Richtung können einander
  folgen, einer je Abschnitt. Siehe **Zugleitabschnitte** in der Hilfe auf dem Reiter **Strecken**.

- **Ein besetzter Bahnhof kann eine Abzweigstelle oder einen unbesetzten Bahnhof fernsteuern.** Das Feld
  **Gesteuert von** auf dem Reiter **Betriebsstellen**, bisher nur bei signalgesteuerten Stellen, wird
  jetzt auch bei unbesetzten Bahnhöfen und Industriegebieten angeboten. Als steuernder Bahnhof werden nur
  besetzte Bahnhöfe angeboten. Eine ferngesteuerte Abzweigstelle, Kreuzungsstelle, ein unbesetzter
  Bahnhof oder ein Industriegebiet wird vom Fahrdienstleiter dieses Bahnhofs wie ein eigener bedient:
  Zugleitabschnitte enden dort, seine Züge stehen auf der Zugmeldeliste des steuernden Bahnhofs in
  zeitlicher Reihenfolge zwischen denen des Bahnhofs, mit seiner Signatur vor dem Gleis, und die
  Überschrift nennt ihn. Die Bahnhöfe dahinter gehören zu denen, die der steuernde Bahnhof anruft, und
  sie rufen den steuernden Bahnhof an. Eine Blockstelle, eine signalgesteuerte Stelle, die weder
  Abzweigstelle noch Kreuzungsstelle ist, bleibt Teil der Strecke. Klicken Sie auf **Aus
  Streckenabschnitten neu erzeugen** auf dem Reiter **Strecken**, nachdem Sie einen steuernden Bahnhof
  festgelegt haben.

- **Angeben, wo Züge kreuzen können.** Eine signalgesteuerte Stelle hat auf dem Reiter
  **Betriebsstellen** ein neues Kontrollkästchen **Züge können kreuzen?**. Kreuzen Sie es dort an, wo ein
  Zug warten kann, während ein anderer vorbeifährt. Die Zahl der Gleise verrät das nicht, denn eine
  Abzweigstelle hat zwei Gleise, damit klar ist, wohin ein Zug fährt, ob dort Züge kreuzen können oder
  nicht. Eine signalgesteuerte Stelle, die weder Abzweigstelle noch angekreuzt ist, ist eine Blockstelle.
  Importierte Stellen beginnen ohne Kreuz, kreuzen Sie also nach dem Import die Kreuzungsstellen an.

### Änderungen

- **Lokale Ziele werden genannt.** Ein Güterziel mit angekreuztem **Und lokal?** lautet nicht mehr
  *Stilkøbing und lokale Ziele*: Es nennt die Orte, *Stilkøbing, Vig, Rubjerg* — den Bahnhof, gefolgt
  von jeder Betriebsstelle, deren Güter von dort bedient werden (**Güterbedienung von** auf dem Reiter
  **Betriebsstellen**). Ein Bahnhof, der nichts bedient, wird allein genannt. Die Wendung ist auch aus
  der Güterlegende in den allgemeinen Anweisungen verschwunden, da nichts sie mehr druckt.

## Version 0.7.0

### Neue Funktionen

- **Die Tabelle der Rangierbahnhöfe lässt sich jetzt in den allgemeinen Anweisungen platzieren.** Die
  Tabelle der Rangierbahnhöfe wird auf der Anlagenseite hinten im Heft der allgemeinen Anweisungen
  gedruckt. Auf einer Anlage mit vielen Rangierbahnhöfen füllte sie diese Seite und verdrängte die
  Erklärung der Güterfluss-Zeichen. Schreibe

  ```
  <ShuntingYards/>
  ```

  unter **Einstellungen** in eine eigene Zeile, an der Stelle im Text, an die sie gehört, um sie
  stattdessen dort zu drucken. Die Anlagenseite lässt sie dann weg, sodass sie nie zweimal gedruckt
  wird. Die Vorschau neben dem Text zeigt ein Feld, wo die Tabelle stehen wird. Ohne Eintrag bleibt die
  Tabelle wie bisher auf der Anlagenseite.

- **Fahrzeugbesitzer: wer welches rollende Material zum Treffen mitbringt und wo es aufgestellt wird.** Der
  Reiter **Fahrzeugbesitzer** führt jede Lokomotive, jeden Triebzug und jede Wagengruppe auf — mit der
  Anzahl der Einheiten, wo es mehr als eine sind, und bei einer Wagengruppe, die ihre Wagen aufführt, mit
  jeder Wagenklasse einmal — sowie mit der ersten Fahrrunde (oder dem ersten Tag), in
  der sie im Einsatz ist, und mit dem Bahnhof, dem Gleis und der Abfahrt, wo sie davor stehen soll. Öffnen
  Sie eine Zeile, um Eigentümer hinzuzufügen: Der erste bringt das Fahrzeug mit und stellt es auf der Anlage
  auf, weitere Eigentümer bringen Reservefahrzeuge mit. Wählen Sie einen Eigentümer, indem Sie die ersten
  Buchstaben des Namens — oder des Nachnamens — eingeben, damit ein Name überall gleich geschrieben wird;
  ein Name, der zu niemandem passt, wird als neuer Teilnehmer angeboten. Jeder Eigentümer einer Lokomotive
  oder eines Triebzugs, auch eines Reservefahrzeugs, muss eine DCC-Adresse angeben; geben Sie **0** ein,
  wenn der Eigentümer sie noch nennen muss. Jedes Fahrzeug und jeder Eigentümer hat eine Bemerkung, und die
  Ansicht **Teilnehmer** zeigt alle mit dem, was sie mitbringen; dort wird ein falsch geschriebener Name ein
  für alle Mal korrigiert.

  Der Bericht **Mitgebrachte Fahrzeuge** unter **Berichte** druckt dieselbe Liste auf A4 quer, auf eine von
  drei Arten geordnet, die über den Seiten gewählt wird: **Nach Betriebsstelle**, eine Seite je Bahnhof für
  seinen Eigentümer, mit den dort aufzustellenden Fahrzeugen nach erster Fahrrunde und Abfahrt geordnet;
  **Nach Eigentümer**, eine Seite je Teilnehmer mit dem, was er mitbringt, den DCC-Adressen und wo jedes
  Fahrzeug startet; oder **Nach DCC-Adresse**, alle mitgebrachten Lokomotiven und Triebzüge in einer Liste.
  Jeder Bahnhof und jeder Eigentümer beginnt auf einer neuen Seite und wird auf der nächsten fortgesetzt,
  wenn eine Seite nicht reicht. Fahrzeuge, die nicht im Einsatz sind, stehen am Ende; die noch niemand
  mitbringt, stehen nach Eigentümer geordnet zuerst, unter **Noch nicht gebucht**. Der Hintergrund einer
  Zeile zeigt, wann die Einheit gebraucht wird: weiß, wenn sie in allen Fahrrunden im Einsatz ist, hellgrau
  für eine Reserve und sonst hellblau, hellgrün oder hellrot, wenn das Fahrzeug erst ab der ersten, zweiten
  oder dritten Fahrrunde im Einsatz ist.

- **Ein Umlauf kann angeben, dass die Fahrzeuge auf einem anderen Gleis stehen als der Zug.** Beim
  Bearbeiten eines Zugabschnitts unter **Umläufe** nennt **Holen von** das Gleis, auf dem die
  Fahrzeuge vor der Abfahrt stehen, und **Abstellen auf** das Gleis, auf das sie nach der Ankunft
  gestellt werden — etwa eine Wagengruppe, die an einem Zwischenbahnhof auf einem Nebengleis bleibt. Die
  Diensthefte und die Zugmeldelisten drucken das als Vermerk: *Vor Abfahrt Wagengruppe 21 von Gleis 3
  holen.* bei der Abfahrt und *Nach Ankunft Wagengruppe 21 auf Gleis 3 rangieren.* bei der Ankunft,
  anstelle des Vermerks zum An- oder Abkuppeln des Fahrzeugs. Fährt das Fahrzeug nur in einigen der
  Fahrrunden oder an einigen der Tage mit, an denen der Zug fährt, beginnt der Vermerk mit diesen —
  *1,3,5: Vor Abfahrt … holen.* — und ein Fahrzeug, das in keiner davon mitfährt, erhält keinen
  Vermerk. Ein Gleis, das ein Umlauf so verwendet, kann unter **Betriebsstellen** nicht gelöscht
  werden.

- **Zugbildungen können jetzt gedruckt werden.** Ein neuer Bericht unter **Berichte** gibt jedem besetzten
  Bahnhof eine eigene Seite auf A4 quer mit allen Zügen, die dort mit Wagen aus Wagenströmen oder mit einer
  Wagengruppe abfahren, die ihre Wagen aufführt. Die Züge stehen Gleis für Gleis und auf jedem Gleis in der
  Reihenfolge der Abfahrt. Neben jedem Zug wird seine Bildung von der Zugspitze aus als Rechtecke gezeichnet:
  eine Wagengruppe als ein Rechteck je Wagen, in Wagenreihung, mit Gattung und Nummer des Wagens; und Wagen
  aus Wagenströmen als ein Rechteck je Position im Zug mit ihren Zielen — mit *und lokale Ziele* und *und
  weiter*, wo das Güterziel es angibt, und seinen Regionen in ihren Farben. Gezeigt wird die Bildung, mit der
  der Zug abfährt, also auch die Wagen, mit denen er angekommen ist, nicht nur die am Bahnhof angekuppelten.
  Eine Wagengruppe, die nur in einigen Fahrrunden oder an einigen Tagen im Zug läuft, ist mit diesen
  gekennzeichnet, und Wagen aus Wagenströmen ohne Position stehen am Ende, als *Beliebig im Zug*.

- **Ein Triebzug kommt jetzt auf dem Gleis an, von dem er abfahren soll.** Ein Triebzug — und eine Lok im
  Wendezug — wird nie umgesetzt: er fährt von genau dem Gleis ab, auf dem er angekommen ist. Fährt ein
  solches Fahrzeug einen Zug nach dem anderen, wird deshalb das Ankunftsgleis jedes Zuges auf das Gleis
  gelegt, von dem sein nächster Zug abfährt, und — endet der Umlauf dort, wo er begonnen hat — das
  Ankunftsgleis des letzten Zuges auf das Abfahrtsgleis des ersten, sodass das Fahrzeug dort bereitsteht,
  wo die nächste Fahrrunde es abholt. Nur Ankunftsgleise werden verlegt; das Gleis, von dem ein Zug
  abfährt, bleibt so, wie Sie es festgelegt haben. Ein Umlauf mit gewöhnlicher Lok bleibt unberührt, denn
  die Lok fährt allein über den Bahnhof zu dem Gleis, auf dem ihr nächster Zug steht. Die Gleise werden
  jedes Mal richtiggestellt, wenn ein Zug zu einem Umlauf hinzukommt — durch **Automatisch erstellen** oder
  durch Sie, am Ende des Umlaufs oder dort, wo das Fahrzeug steht — und jedes Mal, wenn Sie einem Umlauf ein
  Fahrzeug zuweisen, sodass ein vor der Zuweisung erstellter Umlauf berichtigt wird, sobald der Triebzug
  darauf gesetzt wird. Ein importierter Umlauf behält die Gleise, mit denen er eingelesen wurde. Stünden
  durch die Verlegung zwei Züge gleichzeitig auf demselben Gleis, wird das unter den Konflikten aufgeführt,
  damit Sie es lösen können.

### Änderungen

- **Tage und Fahrrunden werden ohne Leerzeichen aufgeführt.** Wo ein Vermerk oder eine Spalte nennt, an
  welchen Tagen oder in welchen Fahrrunden etwas gilt, steht jetzt *Mo,Mi,Fr* und *1,3,5* statt
  *Mo, Mi, Fr* und *1, 3, 5*, damit die Angabe nicht mehr Platz einnimmt als nötig. Ausgeschriebene
  Tagesnamen — *Montag, Mittwoch, Freitag* — bleiben unverändert.

- **Eine Wagengruppe, die ihre Wagen aufführt, zeigt sie jetzt in ihrer Beschriftung unter Umläufe.** Die
  Beschriftung nennt die Anzahl der Wagen und jede Wagenklasse einmal — *SJ 05 5 x A/B/Fv* — anstelle der
  eigenen Klasse der Wagengruppe.

- **Eine Bemerkung an einem Halt sagt jetzt, ob sie zur Ankunft oder zur Abfahrt gehört.** Die Diensthefte
  und die Zugmeldelisten drucken Ankunft und Abfahrt eines Halts in getrennten Zeilen, deshalb fragt unter
  **Züge** das Feld neben jeder **Bemerkung**, für welche der beiden sie gilt: **An** für etwas, das bei der
  Einfahrt anzutreffen oder zu tun ist, **Ab** für etwas, das vor oder bei der Abfahrt zu tun ist. Wo der Zug
  nur ankommt oder nur abfährt, steht nur diese Hälfte zur Wahl. Wo er durchfährt, lässt sich keine
  Bemerkung schreiben — kreuzen Sie zuerst **An** oder **Ab** an —, eine bereits vorhandene lässt sich aber
  weiterhin löschen.

  Eine Bemerkung, die mit einer früheren Version geschrieben oder durch einen XPLN-Import übernommen wurde,
  gab keines von beiden an und wurde daher weder in den Heften noch in den Listen gedruckt. Beim ersten
  Öffnen eines Plans erhält jede solche Bemerkung die Abfahrt, wo der Zug abfährt, und die Ankunft, wo er nur
  ankommt.

- **Jeder Block einer Zugseite im Dienstheft hat jetzt einen farbigen Balken am linken Rand.** Triebfahrzeuge
  sind rot markiert, geplante Wagengruppen grün, Güterwagen mit Frachtbriefen blau und der Fahrplan grau,
  sodass sich die Blöcke auf einen Blick unterscheiden lassen, und wo ein grauer Balken aufhört, endet dieser
  Zugabschnitt. Die Balken werden gedruckt, ohne dass Hintergrundgrafiken eingeschaltet sein müssen, und
  jeder Block behält seine Überschrift, sodass ein Schwarzweißdruck nichts verliert.

- **Triebfahrzeuge und geplante Wagengruppen auf einer Zugseite im Dienstheft zeigen jetzt ihre Gleise.**
  Jede Zeile nennt das Gleis, auf dem das Fahrzeug zu Beginn steht, und das Gleis, auf dem es am Ende
  abgestellt wird — das eigene Gleis des Zuges oder das, das der Umlauf unter **Holen von** oder
  **Abstellen auf** nennt. Die Spalte mit dem Fahrzeug heißt jetzt in beiden Blöcken **Umlauf**, nach der
  Karte, auf der die Kennung des Fahrzeugs steht. Da die Gleise in der Tabelle stehen, nennt der Fahrplan
  darunter nicht mehr jede Wagengruppe und ihr Gleis: Er sagt *Wagen vor Abfahrt zum Abfahrtsgleis
  rangieren.* oder *Wagen nach Ankunft zu ihrem Ankunftsgleis rangieren.*, angeführt von den Fahrrunden oder
  Tagen, an denen die Wagen rangiert werden, wenn das nur einige derer sind, an denen der Zug fährt. Die
  Zugmeldelisten nennen weiterhin jede Wagengruppe und ihr Gleis.

- **Das Heft mit den allgemeinen Anweisungen erklärt jetzt, wie Güter in den anderen Berichten geschrieben
  werden.** Unter **Wagenströme** auf seiner letzten Seite sagt eine Legende, dass jede Lastgrenze ein
  Höchstwert ist und was das Zeichen hinter einer Zahl zählt, wofür der Globus steht, was *und lokale Ziele*
  und *und weiter* einem Ziel hinzufügen und was ein farbiger Regionsname bedeutet.

- **Die Titelseite des Heftes mit den allgemeinen Anweisungen hat Platz für ein längeres Programm.** Das
  Programm wird mit weniger Abstand zwischen Zeilen, Einträgen und Tagesüberschriften gesetzt — die
  Schriftgröße bleibt gleich —, was Platz für vier oder fünf weitere Einträge schafft. Ein Programm, das zu
  lang für die Seite ist, verliert seine letzten Einträge, und die sind das Ende des Treffens, also genau das,
  was man nachschlägt.

- **Automatisch erstellen ergänzt jetzt Ihre vorhandenen Umläufe, bevor es neue anlegt.** Die Züge, die
  noch in keinem Umlauf sind, werden zuerst den vorhandenen Umläufen angeboten: Ein Umlauf fährt mit dem
  weiter, was ihn dort fortsetzt, wo er ankommt, in der Kategorie, in der er bereits fährt, und ein
  leerer Umlauf, den Sie selbst angelegt haben, wird gefüllt, bevor ein neuer entsteht. Nur die Züge, die
  in keinen vorhandenen Umlauf passen, beginnen neue Umläufe, sodass ein erneutes Erstellen nach dem
  Hinzufügen einiger Züge die bereits umlaufenden Fahrzeuge verlängert, statt neue in Dienst zu stellen.
  Wagenströme bleiben, wie sie sind. Das Ergebnis neben der Schaltfläche nennt jetzt, wie viele
  Zugabschnitte die vorhandenen Umläufe übernommen haben, wie viele davon verlängert wurden und wie viele
  Umläufe erstellt wurden.

### Fehlerbehebungen

- **Die Erklärung der Güterfluss-Zeichen läuft nicht mehr aus dem Heft der allgemeinen Anweisungen
  heraus.** Die Formulierungen *und lokale Ziele* und *und weiter* sowie die Erklärung einer Region
  standen in einer so schmalen Spalte, dass jede vier Zeilen lang wurde, und auf einer Anlage mit
  mehreren Rangierbahnhöfen fiel die letzte davon unten von der Seite. Zeichen und Formulierungen stehen
  nun als zwei Listen untereinander, jede über die ganze Seitenbreite.

- **Ein Fahrplan, der auf der gegenüberliegenden Seite eines Dienstheftes weitergeht, sieht jetzt aus wie
  jeder andere.** Ist ein Zugabschnitt zu lang für eine Seite, rückt sein Fahrplan auf die gegenüberliegende
  Seite, und dort wurde er ohne die eigene Gestaltung des Heftes gedruckt: in größerer Schrift, ohne die fett
  gesetzten Bahnhöfe und Zeiten und ohne die Linien zwischen den Halten, sodass ein langer Fahrplan über den
  Seitenfuß hinauslaufen konnte.

- **Das Ändern der Nummer oder Kategorie eines Zuges unter Züge lässt die Änderung nicht mehr in einer
  anderen Zeile stehen.** Beide Änderungen sortieren die Liste neu, und die eingegebene Nummer oder die
  gewählte Kategorie konnte in der Zeile des Zuges stehen bleiben, der an ihre Stelle rückte.

- **Dialoge übernehmen eine Zahl jetzt schon während der Eingabe.** Die **Dauer (Minuten)** einer neuen
  Rangieraufgabe, die **Minuten**, um die Züge verschoben oder kopiert werden, und die **Nummer** eines
  Fahrzeugs unter **Umläufe** wurden erst beim Verlassen des Feldes gelesen, sodass die Schaltfläche, die den
  Dialog bestätigt — und die Warnung, dass eine Fahrzeugnummer schon vergeben ist —, hinterherhinkte, bis Sie
  woanders hinklickten.

- **Der Bildfahrplan zeichnet die Gleise einer Betriebsstelle jetzt in der Reihenfolge, die Sie ihnen gegeben
  haben.** Er übersah die **Reihenfolge** der Gleise unter **Betriebsstellen** und konnte sie daher anders
  anordnen als jede andere Gleisliste der App.

- **Eine Umlaufkarte für ein Fahrzeug, das an nicht aufeinanderfolgenden Tagen fährt, nennt diese Tage
  jetzt.** Eine Karte für Montag, Mittwoch und Freitag druckte *MondayShort,WednesdayShort,FridayShort* statt
  *Mo,Mi,Fr*.

## Version 0.6.0

### Änderungen

- **Dienstzüge sind eine neue Art von Zugkategorie.** Geben Sie einer Zugkategorie unter
  **Zugkategorien** den Typ **Dienstzug** für Züge, die an ihren Halten nichts abgeben und nichts
  aufnehmen: ein Bauzug, oder eine Lokomotive oder ein Triebzug, die aus dem Betrieb gefahren werden. Ein
  solcher Zug darf dort halten, wo eine Betriebsstelle weder Reisende noch Güter austauscht — etwa an
  einer Baustelle — und beim Erstellen seines Laufwegs erhält er zwischen seinen Endpunkten keine Halte,
  sodass Sie den entscheidenden Halt selbst setzen. Benennen Sie die Kategorie danach, was ihre Züge tun:
  ein Zug, der Materialwagen zurücklässt, tauscht Güter aus und gehört in eine Güterkategorie.

  Eine Kategorie in einem Plan einer früheren Version, die weder Reise- noch Güterkategorie war — was ein
  XPLN-Import hinterlassen kann — wird jetzt als Dienstzug angezeigt, wo sie zuvor als Reisezug erschien.

- **Rangieraufgaben sind eine neue Art von Zug.** Geben Sie einer Zugkategorie unter **Zugkategorien**
  den Typ **Rangieraufgabe**, dann werden die Züge dieser Kategorie an einem Bahnhof über eine bestimmte
  Zeitspanne ausgeführt, statt zu fahren: jeder hat nur einen Halt, dessen Ankunftszeit der Beginn der
  Arbeit und dessen Abfahrtszeit ihr Ende ist.

- **Die Wagenströme einer Rangieraufgabe legen fest, welche Wagen zu rangieren sind.** Fügen Sie der
  Aufgabe unter **Wagenstrom** Wagenströme hinzu wie jedem anderen Güterzug. Ein Strom mit dem eigenen
  Bahnhof der Aufgabe als Ziel enthält angekommene Wagen, und der Lokführer wird angewiesen, sie zu den
  Güterkunden zu rangieren, mit Angabe ihrer Herkunft. Ein Strom mit einem anderen Ziel wird stattdessen
  von den Güterkunden geholt, mit Angabe des Ziels der Wagen. Die Anweisung wird in den Dienstheften der
  Lokführer und in den Bahnhofsberichten gedruckt.

- **Fahrkarten lassen sich jetzt drucken.** Ein neuer Bericht unter **Berichte** liefert eine
  Rückfahrkarte zwischen je zwei Betriebsstellen mit Reisendenwechsel, in der Mitte zu falten, mit dem
  Betreiber der meisten Reisezugabfahrten von der Verkaufsstelle am Fuß beider Hälften.

- **Der Fahrplanbericht bringt jetzt mehrere Strecken auf ein Blatt.** Tabellen, die die Breite nicht
  füllen, stehen nebeneinander, sodass eine kurze Nebenbahn kein ganzes Blatt mehr für sich braucht.

- **Die Bildfahrpläne lassen sich jetzt drucken.** Ein neuer Bericht unter **Berichte** zeichnet jede
  Strecke in dem festen Papiermaßstab, der unter **Einstellungen → Bildfahrplan** eingestellt wird, sodass
  Zeiten und Neigungen von Blatt zu Blatt zu messen sind.

- **Einstellungen → Bildfahrplan ist jetzt danach geordnet, was jede Einstellung betrifft.** Was der
  Bildfahrplan zeigt, steht zuoberst, darunter die Abstände am Bildschirm, in Bildpunkten, neben denen auf
  dem Papier, in Millimetern.

- **Sie können jetzt angeben, was mit der Lok geschehen soll, wo ein Zugabschnitt endet.** Beim Bearbeiten
  eines Zugabschnitts unter **Umläufe** wird gefragt, ob die Lok gedreht und ob sie ans andere Zugende
  umgesetzt werden soll, und beides wird als Ankunftsvermerk für Lokführer und Fahrdienstleiter gedruckt.

- **Das Topologie-Diagramm zeichnet jetzt die Gleise der ganzen Anlage, wobei jede Betriebsstelle nur ein
  einziges Mal erscheint.** Die Gleise sind ein- oder zweigleisig, wie der Abschnitt wirklich ist, und in
  den Farben der Fahrplanabschnitte, die darüber verkehren — grau, wo kein Abschnitt sie abdeckt.

- **Sie können das Topologie-Diagramm jetzt selbst anordnen.** Ziehen Sie eine Betriebsstelle dorthin, wo
  sie hingehört, dann folgen die Gleise; Ihre Anordnung wird mit dem Plan gespeichert und in den
  Dienstheften gedruckt.

- **Ein Güterziel mit einer Grenze für Wagen und für Achsen zeigt jetzt beide.** Die Wagenzahl
  verschwand bisher überall dort, wo auch eine Achszahl stand — in den Dienstheften wie in den
  Gütervermerken —, obwohl beide Felder unter **Güterverkehr** nebeneinander stehen und jede der beiden
  Grenzen die bindende sein kann: sechzehn Achsen sind vier Drehgestellwagen, aber acht zweiachsige.

- **Die Zugseiten eines Dienstheftes sagen dasselbe jetzt auf weniger Raum.** Die Spalte der Fahrrunden
  trägt die Überschrift **Fährt** — das, was sie über das Fahrzeug sagt — statt eines langen Wortes über
  einer Spalte von Kreisen, und die Güterwagen sind mit **Von** und **Nach** überschrieben, wie die
  Fahrzeuge darüber schon zuvor. Die Beschränkungen unter der Überschrift stehen als Zahlen unter einem
  einzigen **Max.**: die Geschwindigkeit mit ihrer Einheit, die Zahl mit einem Kreis dahinter für Achsen, einem
  Quadrat für Wagen und die Länge als *2,5m*. Wie viele Wagen oder Achsen ein Ziel aufnimmt, ist aus **Nach** in eine eigene
  Spalte **Max.** gerückt, wo es die Seite hinunter gelesen wird statt am Ende einer Reihe von Ortsnamen —
  und diese Spalte erscheint nur, wenn auf der Seite überhaupt etwas beschränkt ist.

- **Die Schaltflächen für einen ganzen Umlauf stehen jetzt in einer eigenen Spalte.** Unter **Umläufe**
  sind sie in eine Spalte **Aktionen** zwischen die Fahrzeuge und die Züge gerückt, sodass die Züge jeder
  Zeile an derselben Stelle beginnen.

- **Das Menü Berichte hat eine neue Reihenfolge**, von den allgemeinen Anweisungen bis zu den Fahrkarten.

### Fehlerbehebungen

- **Die installierte App funktioniert jetzt ohne Internetverbindung.** Die eingebauten Hilfetexte,
  die Texte unter Über und Versionshinweise sowie der Katalog der fertigen Zugkategorien wurden bei
  jeder Anzeige aus dem Web geladen und blieben deshalb ohne Verbindung leer. Sie werden nun bei der
  Installation zusammen mit dem Rest der App gespeichert.

## Version 0.5.1

### Änderungen

- **Was mit den Loks zu tun ist, steht jetzt in den Dienstheften der Lokführer und in den
  Zugmeldelisten.** Welche Lok zu verwenden ist, was an- und abzukuppeln ist und dass sie vom Abstellgleis
  zu holen oder dorthin zurückzubringen ist, wurde immer schon aus den Fahrzeugumläufen ermittelt, aber
  nie gedruckt; jetzt stehen diese Hinweise bei den übrigen an dem Halt, zu dem sie gehören, und sowohl
  der Lokführer als auch der Fahrdienstleiter sieht sie. Neu darunter ist der Hinweis für eine Lok, die
  ans andere Ende des Zuges umgesetzt oder gedreht werden muss, bevor der Zug zurückfährt.

- **Das Heft mit den allgemeinen Anweisungen druckt jetzt Ihren ganzen Text, auf lesbaren Seiten.** Eine
  Seite wurde großzügiger gerechnet, als sie wirklich ist, sodass alles über den Seitenfuß hinaus
  stillschweigend entfiel; der Text läuft jetzt auf der nächsten Seite weiter, und eine Seite endet nie
  mit einer Überschrift allein. **Topologie** und **Rangierbahnhöfe** stehen jetzt wie in den
  Dienstheften auf der allerletzten Seite, und das Programm auf der Titelseite ist in den eigenen Größen
  des Heftes gesetzt statt in denen des Browsers.

## Version 0.5.0

### Änderungen

- **Ein Wendezug wartet nicht mehr auf das Umsetzen der Lok.** Setzen Sie das neue Häkchen **Wendezug?**
  bei einer Lok unter **Umläufe**, wo sie einen Zug befördert, der von beiden Enden gefahren werden kann —
  einen Zug mit Steuerwagen oder einer zweiten Lok am anderen Ende —, dann rechnet **Zeiten
  aktualisieren** das Umsetzen heraus und lässt den Zug stattdessen nur den Mindestaufenthalt halten,
  wodurch alle folgenden Halte früher liegen. Ein Triebzug wird ebenso behandelt, ohne dass etwas
  anzuhaken wäre, und ein Aufenthalt, den Sie bewusst länger gemacht haben, bleibt so, wie Sie ihn gesetzt
  haben.

- **Ein Gleis kann jetzt angeben, für welchen Fahrweg durch die Betriebsstelle es vorgesehen ist.** Jedes
  Gleis kann die **vorherige** Betriebsstelle nennen, von der ein Zug kommt, die **nächste**, zu der er
  weiterfährt, oder beide — dazu das Kästchen **beide Richtungen** —, und ein neuer Zug kommt auf das
  Gleis, das zu seinem Laufweg am besten passt. Genau das braucht eine **zweigleisige Strecke**: geben Sie
  den beiden Gleisen dasselbe Paar Betriebsstellen umgekehrt, dann bleibt jede Richtung auf ihrem Gleis.
  Passen zwei Gleise gleich gut, nimmt ein Reisezug, der hält, ein Gleis mit Bahnsteig, und ein Zug, der
  durchfährt, das Hauptgleis; lassen Sie die Spalten leer, ändert sich nichts gegenüber vorher.

- **Ein Zug lässt sich jetzt in der Gegenrichtung kopieren und mehrfach wiederholen.** Mit
  **Gegenrichtung?** befährt die Kopie den Laufweg rückwärts, wobei alle Fahrzeiten und Halte erhalten
  bleiben, Vorbereitungs- und Abschlusszeit die Seite wechseln und die Kopie eine Nummer aus der Reihe der
  Gegenrichtung erhält. Der Kopierdialog hat jetzt auch die Möglichkeit **Züge wiederholen**, sodass sich
  ein Zug zuerst allein anlegen, so lange anpassen, bis er richtig fährt, und erst dann über den Tag
  wiederholen lässt.

- **Ein Gleis kann jetzt angeben, wie lang sein Bahnsteig ist.** Jedes Gleis einer Betriebsstelle, die
  Reisende austauscht, hat eine **Bahnsteiglänge** in Metern — über null bedeutet, dass Reisende dort ein-
  und aussteigen können —, und ein neuer Reisezug kommt dort, wo die Betriebsstelle einen Bahnsteig hat,
  auf ein Gleis mit Bahnsteig. Wird **Reisende?** gesetzt, erhält jedes Gleis einen Bahnsteig von einem
  Meter, den Sie anpassen; ein davor angelegter Plan wird beim ersten Öffnen genauso behandelt und
  arbeitet unverändert weiter, bis Sie die Gleise kürzen oder leeren, die in Wahrheit keinen Bahnsteig
  haben. Ein Reisezug, der zum Reisendenwechsel an einem Gleis ohne Bahnsteig hält, steht jetzt unter
  **Konflikte**: geben Sie entweder dem Gleis eine Bahnsteiglänge oder wählen Sie beim Halt **An** und
  **Ab** ab, womit der Zug dort nichts austauscht. Die Prüfung lässt sich unter **Einstellungen ›
  Validierung** abschalten.

### Fehlerbehebungen

- **Ein neuer Anlagenname wird jetzt überall gezeigt, wo der Name steht.** Die Titelseite des Hefts mit
  den allgemeinen Anweisungen, der Name in der oberen Leiste und der Dateiname, unter dem ein Plan
  gespeichert wird, zeigten weiterhin, wie die Anlage vorher hieß. Ein zuvor umbenannter Plan wird beim
  nächsten Öffnen richtiggestellt.

## Version 0.4.2

### Änderungen

- **Ein Zug lässt sich jetzt mitten in einen Umlauf einfügen.** Zwischen den Zugabschnitten einer Zeile
  stehen nun kleine Übergänge, die zeigen, wo das Fahrzeug steht und wie lange, und vor dem ersten
  Abschnitt einer, der zeigt, woher es gebracht werden muss; ein Klick darauf fügt einen Zug in die Lücke
  ein, wobei nur die Züge angeboten werden, die das Fahrzeug tatsächlich schafft. Eine Fahrt, die das
  Fahrzeug nicht zurückbringt, wird trotzdem eingefügt und als Konflikt gemeldet, bis die Rückfahrt
  eingefügt ist — so wird eine Hin- und Rückfahrt in eine Standzeit eingepasst. Ein Übergang, an dem der
  Umlauf unterbrochen ist, wie ihn ein Import hinterlassen kann, ist gelb markiert.

- **Die App hat ein eigenes Symbol** — die Front eines modernen Zuges auf einer dunkelblauen Fläche —
  statt des Zeichens, das mit den Werkzeugen mitkommt, mit denen sie gebaut ist. Das Symbol erscheint im
  Tab des Browsers sowie auf dem Startbildschirm oder im Startmenü, wenn die App installiert wird.

- **Auf ein Blatt passen jetzt zwölf Umlaufkarten statt zehn.** Die Karten sind 48 mm breit statt 50, also
  passen sechs nebeneinander auf ein A4-Blatt im Querformat, und das Blatt hat weiterhin einen Rand, den
  gewöhnliche Drucker erreichen. Die Karten sind so hoch wie zuvor, und ihr Inhalt ist unverändert.

- **Die Zeilen im Fahrplan stehen jetzt weiter auseinander.** Um jede Zeile ist ein Siebtel mehr Luft,
  sodass sich eine Zeile leichter über die Seite verfolgen und ein Bahnhof leichter in der Spalte finden
  lässt. Schrift und Spalten sind unverändert, das Blatt fasst also dieselben Züge; auf eine Seite gehen
  jetzt neununddreißig Zeilen statt fünfundvierzig.

### Fehlerbehebungen

- **Der gedruckte Fahrplan verliert die letzten Zeilen einer Seite nicht mehr.** Beide Richtungen eines
  Abschnitts wurden auf dieselbe Seite gesetzt, auch wenn dort nicht beide Platz hatten, und die Zeilen,
  für die kein Platz mehr war, wurden abgeschnitten — der Bericht am Bildschirm war in einer größeren
  Schrift gesetzt als der gedruckte, sodass seine Zeilen fast zwei Drittel höher standen als die
  gezählten. Beide sind jetzt gleich gesetzt, wie viel hineinpasst wird an einer wirklichen Seite gemessen
  statt aus der Schriftgröße errechnet, und am Fuß jeder Seite bleiben drei Zeilen frei.

- **Die Wagenstromliste nennt jetzt die Ziele, zu denen die Wagen gehen.** Unter **Güterverkehr ›
  Güterzüge** stand in der Auswahlliste nur „Wagen nach“ ohne die Ziele, sodass sich die Einträge nicht
  unterscheiden ließen. Die Unterregisterkarte und ihre Spalte heißen jetzt **Güterziele** statt
  *Güterbeschreibungen*.

## Version 0.4.1

### Änderungen

- **Die Zugmeldelisten lassen sich jetzt als Dokumente speichern, die die Bahnhofsbetreiber bearbeiten
  können.** Über *Zugmeldelisten* im Menü Export erhält jeder besetzte Bahnhof ein eigenes Dokument im
  OpenDocument-Format, gedacht dafür, jedem Betreiber vor dem Treffen seine eigene Liste zu schicken,
  damit er die örtlichen Anweisungen ergänzen kann, die nur er kennt; ist mehr als ein Bahnhof besetzt,
  kommen die Dokumente gemeinsam in einer ZIP-Datei. Wo die Seiten umbrechen, bleibt der Textverarbeitung
  überlassen, sodass die Seiten auch nach der Eingabe des Betreibers sinnvoll umbrechen — der Name des
  Bahnhofs, die Telefonnummern der Bahnhöfe, zu und von denen er Züge meldet, und die Spaltenköpfe
  wiederholen sich am Kopf jeder Seite, doch der Tagesabschnitt, den eine Seite abdeckt, lässt sich nicht
  nennen, weshalb die Seiten stattdessen numeriert sind. Die gedruckten Blätter im Menü Berichte sind
  unverändert und bleiben die, mit denen während einer Fahrrunde gearbeitet wird.

- **Ein Zug, der zugleich von zwei Loks gezogen wird, sagt jetzt, von welchen beiden.** Der Konflikt
  nannte nur den Zug und die Minuten, sodass seine zwei Hälften Wort für Wort gleich lauteten, wenn beide
  über genau denselben Abschnitt gebucht waren. Er wird jetzt außerdem nur noch an den zwei Umläufen
  angezeigt, die die doppelte Arbeit halten, statt an jedem Umlauf, der diesen Zug irgendwo am Tag führt.

- **Zwei Loks, die sich einen Zug über die Fahrrunden teilen, gelten nicht mehr als Konflikt.** Es wurden
  nur die Uhrzeiten verglichen, sodass eine Lok in den ungeraden Fahrrunden und eine andere in den geraden
  — genau der Sinn dieser Aufteilung — als Doppeltraktion gemeldet wurde. Gemeldet wird jetzt nur noch, wo
  beide für eine gemeinsame Fahrrunde gebucht sind, und der Konflikt nennt diese Fahrrunden.

## Version 0.4.0

### Grundlegende Änderungen

- **Ein selbst angelegtes Fahrzeug wird jetzt durch seinen Betreiber und seine Nummer identifiziert.** In
  ein und derselben Fahrrunde darf die Kombination nur einem Fahrzeug gehören, gleich welcher Art, sodass
  ein Wagensatz und eine Lokomotive nicht mehr beide *DB 5* sein können; ein Fahrzeug ohne Betreiber wird
  allein durch seine Nummer identifiziert, und zwei Fahrzeuge dürfen sich eine Identität teilen, solange
  sich ihre Fahrrunden nicht überschneiden. Ein **importiertes** Fahrzeug wird weiterhin durch seine
  externe Id identifiziert, sodass ein importierter Fahrplan keine neuen Konflikte meldet. Beim Anlegen
  oder Bearbeiten eines Fahrzeugs wird eine bereits vergebene Identität jetzt abgelehnt und eine Nummer
  verlangt, während vorhandene Pläne genau so bleiben, wie sie sind — jedes Fahrzeug, das sich eine
  Identität teilt, steht unter den Konflikten.

### Änderungen

- **Es gibt einen neuen Bericht: die Zugmeldeliste.** Ein eigener Satz Blätter für jeden besetzten Bahnhof
  mit den Zügen, die er abwickelt, in zeitlicher Reihenfolge — ein Zug, der dort steht, erscheint zweimal,
  Ankünfte auf Weiß, Abfahrten auf hellem Gelb, denn einen Zug einzulassen und ihn abzulassen sind zwei
  verschiedene Handlungen, und Züge, die nur durchfahren, stehen ebenfalls darauf. Jede Seite trägt den
  Namen des Bahnhofs, den Tagesabschnitt, den sie abdeckt, und die Telefonnummern der Bahnhöfe am anderen
  Ende der Zugmeldeabschnitte; jede Zeile hat je Fahrrunde ein Kästchen zum Abhaken. Jeder Bahnhof beginnt
  auf einer neuen Seite, sodass der Stapel geteilt und ausgegeben werden kann; Druck über das Menü
  Berichte.

- **Die Felder zum Anlegen und Bearbeiten eines Fahrzeugs haben eine neue Reihenfolge,** an beiden Stellen
  dieselbe: Fahrzeugart, Traktionsart, Anzahl Einheiten, Betreiber, Nummer, Klasse, Fahrrunden und zuletzt
  die externe Id. Das bisher mit *Gesellschaft* bezeichnete Feld heißt jetzt *Betreiber*.

- **Eine externe Id lässt sich berichtigen, aber nicht mehr erfinden.** Die externe Id ist der Name, den
  ein Zug oder ein Fahrzeug in dem System trägt, aus dem er importiert wurde; was mit einer Id importiert
  wurde, hat sein Feld weiterhin und kann dort berichtigt werden, was nie eine Id hatte, bekommt jetzt
  kein Eingabefeld mehr. Ein im Planer angelegtes Fahrzeug erhält daher gar keine externe Id, wo ihm
  früher eine aus Klasse und Nummer erfundene gegeben wurde.

- **Die kleinste Zeit zwischen zwei Nutzungen desselben Gleises wird jetzt geprüft.** Die Einstellung gab
  es, aber nichts wertete sie aus: bei 0, wo sie beginnt, ändert sich an der Prüfung nichts. Setzen Sie
  sie auf 5, muss das Gleis zwischen zwei Zügen außerdem fünf Minuten frei sein — genau fünf genügen, vier
  nicht —, und der Konflikt nennt, wie kurz der Abstand tatsächlich ist und wie lang er sein müsste.

- **Eine Betriebsstelle kann jetzt eigene Anweisungen tragen.** Das Bearbeitungsformular hat das Feld
  **Anweisungen**, in Markdown geschrieben und neben einer Vorschau gezeigt, dafür, wie genau diese
  Betriebsstelle bei diesem Treffen betrieben wird: welche Gleise wofür genutzt werden, wie das Rangieren
  organisiert ist und was die Lokführer und das Personal vor Ort dort sonst wissen müssen. Das Feld wird
  bei einer Station oder einem Industriegebiet angeboten und in der Info-Ansicht der Betriebsstelle
  gezeigt; angeboten wird es nicht, wo es nichts anzuweisen gibt.

- **Eine Stelle, an der ohne Personal Güter bedient werden, kann jetzt einen Schlüssel verlangen.** Wählen
  Sie unter **Schlüssel hinterlegt in** den besetzten Bahnhof, der den Schlüssel verwahrt, und geben Sie
  ihm eine Bezeichnung, wenn der Bahnhof mehrere verwahrt — einem Güterzug, der an beiden Stellen hält,
  wird bei der Abfahrt gesagt *Schlüssel A1 zum Aufschließen von Bruket abholen* und beim nächsten Halt
  dort *Schlüssel A1 von Bruket abgeben*. Der Schlüssel wird beim letzten Halt vor der Arbeit geholt und
  beim ersten danach abgegeben; ein Zug, der nur vorbeifährt, bekommt keinen Hinweis. Markieren Sie die
  Stelle als besetzt oder nehmen Sie die Besetzung vom verwahrenden Bahnhof, dann gilt der Schlüssel nicht
  mehr — unter **Konflikte** steht, welche Änderung es war, und der Schlüssel bleibt erhalten, sodass er
  sofort wieder gilt, wenn Sie die Änderung rückgängig machen.

### Fehlerbehebungen

- **Zwei Strecken, die von derselben Betriebsstelle ausgehen, wurden gezeichnet, als träfen sie sich
  nie.** Begann ein Fahrplanabschnitt genau an der ersten Betriebsstelle eines anderen, verband die beiden
  im Topologie-Diagramm nichts. Der zweite verlässt diese Betriebsstelle jetzt wie jede andere Abzweigung,
  im selben festen Winkel.

- **Jeder Grenzwert der Prüfungen nennt jetzt die Uhr, nach der er gemessen wird.** Die kleinste Zeit
  zwischen zwei Nutzungen desselben Gleises hatte gar keine Einheit, und die beiden Zuggeschwindigkeiten
  nannten nur *Uhr-Minuten*. Alle drei stehen jetzt in Schnelluhr-Minuten — der schnellen Uhr, nach der
  die Züge fahren, nicht der wirklichen Zeit; sie heißt in der ganzen App jetzt so, statt *Zeitraffer*
  oder *Modelluhr*.

- **Längen und Entfernungen sind jetzt in Metern ausgeschrieben,** ebenso der Zähler der
  Zuggeschwindigkeiten, damit das *m* nicht als Minute gelesen werden kann. Der Mindesthalt an einer
  Station steht jetzt ebenfalls in Schnelluhr-Minuten.

## Version 0.3.5

### Fehlerbehebungen

- **Ein gespeicherter Fahrplan ließ sich unter Umständen nicht öffnen.** Das Öffnen eines gerade
  gespeicherten Fahrplans brach mit einem Fehler zu einem Land ab, und es wurde nichts geladen. Ein
  bereits gespeicherter Fahrplan öffnet sich, wie er ist; Sie müssen nichts weiter tun.

- **Eine gespeicherte Fahrplandatei ist etwa siebenmal kleiner.** Das Speichern schrieb den Fahrplan in
  einer anderen Form, als er im Browser gehalten wird, sodass jeder Halt doppelt geschrieben wurde und
  jede Zugkategorie, jeder Betreiber und jedes Land erneut bei jedem Zug, jedem Fahrzeug und jedem Dienst,
  die sie verwendeten. Eine Datei, die 8 MB groß war, braucht jetzt etwas über 1 MB; ein mit einer
  früheren Version gespeicherter Fahrplan lässt sich weiterhin öffnen.

## Version 0.3.4

### Änderungen

- **Die Felder Ank und Abf eines Halts richten sich jetzt danach, wo der Zug tatsächlich halten kann.**
  Ein Reisezug braucht eine Betriebsstelle, die Reisende annimmt, ein Güterzug eine, die Fracht annimmt,
  und beides gibt es an einer signalgesteuerten Betriebsstelle nicht; wo der Zug nicht halten kann, werden
  beide Felder leer und gesperrt gezeigt, und der Halt ist eine Durchfahrt. Nichts von dem, was Sie
  geplant haben, geht verloren — schalten Sie den Austausch wieder ein, und die Halte sind wieder da —,
  und ein Schattenbahnhof hat immer beides, da er für alles außerhalb der Anlage steht.

- **Ein Halt, an dem etwas hängt, lässt sich nicht mehr entfernen.** Der erste und der letzte Halt des
  Zuges selbst sowie die Enden jedes Zugabschnitts, über den ein Fahrzeugumlauf, ein Dienst oder ein
  Frachtfluss geplant ist, behalten ihr Feld gesetzt und gesperrt; der Mauszeiger darauf sagt, was es
  hält. Wo ein Zugabschnitt dort endet, wo sein Zug nicht halten kann, wird das offen gesagt, damit Sie
  den Halt oder den Zugabschnitt verschieben können.

- **Eine Zugkategorie trägt jetzt die Vorbereitungs- und Abschlusszeiten, mit denen ihre Züge geplant
  werden,** sodass Sie dieselben zwei Zahlen nicht mehr für jeden Zug eingeben müssen. Neben jedem Feld
  steht eine Schaltfläche *Erneut anwenden*, die diese eine Zeit allen Zügen der Kategorie gibt und
  meldet, wie viele geändert wurden; beides sind getrennte Aktionen, und das erneute Anwenden verschiebt
  nur die Minuten ganz an den Enden eines Zuges.

- **Die Betreiber sind auf der Titelseite eines Dienstheftes leichter zu lesen.** Die Zeile ist jetzt
  doppelt so groß gesetzt, sodass ein Logo auf einen Blick zu erkennen und eine Signatur über einen Tisch
  hinweg zu lesen ist. Haben alle Betreiber ein Logo, entfällt das Wort *Betreiber*; fehlt einem von ihnen
  das Logo, stehen alle als Signatur da, fett und mit der Beschriftung davor.

### Fehlerbehebungen

- **Ein Dienstheft konnte einen Zugabschnitt über den unteren Seitenrand hinaus drucken.** Jede Seite
  wurde mit rund der Hälfte mehr Platz gerechnet, als eine A5-Seite tatsächlich hat, und was über den
  Seitenrand hinausragt, wird kommentarlos abgeschnitten, sodass dem zweiten Zugabschnitt einer solchen
  Seite das Ende seines Fahrplans fehlte oder er ganz fehlte. Zugabschnitte werden jetzt an dem gemessen,
  was die Seite wirklich fasst; manche Hefte brauchen dadurch ein Blatt mehr als bisher.

- **Das Topologie-Diagramm konnte die Signaturen zweier Betriebsstellen übereinander drucken.** Die
  Betriebsstellen wurden allein nach ihrem Abstand gesetzt, sodass zwei nah beieinander liegende auf einer
  langen Strecke fast an derselben Stelle gezeichnet wurden. Sie werden jetzt nie enger gezeichnet, als es
  ihre Signaturen brauchen, und auch eine lange Signatur am Rand des Diagramms wird nicht mehr
  abgeschnitten.

- **Eine Abzweigung im Topologie-Diagramm konnte quer durch eine andere Strecke gezeichnet werden.** Eine
  Abzweigung fällt in einem festen Winkel ab, sodass eine, die auf eine Strecke im Weg traf, einfach quer
  darüber gezeichnet wurde. Die Abzweigungen, die eine Strecke am weitesten hinten verlassen, werden jetzt
  zuerst gezeichnet, sodass eine lange Abzweigung nun unter einer kurzen liegen kann, die die Strecke
  weiter hinten verlässt.

- **Ein Plan konnte seine Züge unter Zugkategorien zeigen, die das Register Zugkategorien nicht führte.**
  Mehrere Kategorien konnten außerdem für ein und dieselbe gehalten werden, sodass ihre Züge unter einer
  einzigen Überschrift zusammenkamen und zwei Züge verschiedener Kategorien mit derselben Nummer als eine
  doppelt vergebene Nummer gemeldet wurden. Beim Öffnen eines Plans wird die Liste der Kategorien nun aus
  den Kategorien seiner Züge vervollständigt, und jede Kategorie bleibt von den anderen getrennt.

- **Zwei Gesellschaften ohne eigene Nummer wurden für denselben Betreiber gehalten,** sodass Züge
  verschiedener Gesellschaften mit derselben Zugnummer als eine doppelt vergebene Nummer gemeldet wurden.
  Jede Gesellschaft erhält nun eine eigene Nummer, sobald ein Plan geöffnet oder gespeichert wird; eine
  Gesellschaft aus dem Module Registry behält die Nummer, mit der sie gekommen ist.

- **Ein Plan speicherte seine Zugkategorien, Gesellschaften und Länder an mehr als einer Stelle** — jede
  wurde dort geschrieben, wo sie zuerst angetroffen wurde, meist im ersten Zug, der sie verwendete. Jede
  wird jetzt einmal geschrieben, in ihrer eigenen Liste, und alles, was sie verwendet, behält nur einen
  Verweis; Länder werden gar nicht mehr in den Plan kopiert, sodass eine Korrektur der Sprachen eines
  Landes jetzt auch Pläne erreicht, die davor gespeichert wurden.

- **Ein Dienstheft nannte in der Überschrift eines Zugabschnitts nur die Zugnummer.** Ein Zug wird durch
  Präfix und Suffix seiner Zugkategorie ebenso bezeichnet wie durch seine Nummer — Gt 1234, nicht 1234 —
  und ein Lokführer hat zum Vergleich mit dem Fahrplan nur diese Überschrift. Sie trägt jetzt die
  vollständige Zugbezeichnung, hinter der Signatur des Betreibers.

## Version 0.3.3

### Änderungen

- **Konflikte lassen sich jetzt dort lesen, wo sie angezeigt werden.** Eine Zeile mit Konflikten — ein Zug
  oder eine Zugkategorie unter **Züge**, ein Umlauf oder eines seiner Fahrzeuge unter **Umläufe**, ein
  Dienst unter **Dienste** — trägt jetzt ein Warnsymbol, und ein Klick darauf öffnet die Meldungen als
  lesbare Liste. Das Symbol nimmt die Farbe des schwersten Konflikts an und zählt sie; bisher standen sie
  nur in einem Kurzinfofenster, das erschien, während der Zeiger auf der Zeile ruhte.
- **Eine Zugkategorie zeigt die Konflikte der Züge in ihr**, sodass sie beim Zuklappen der Kategorie nicht
  mehr verschwinden.
- **Der Reiter Züge öffnet jetzt mit der Liste der Zugkategorien**; die Züge bleiben verborgen, bis Sie
  eine Kategorie aufklappen. *Alle aufklappen* öffnet alle auf einmal, und eine Kategorie klappt von
  selbst auf, wenn Sie ihr einen Zug hinzufügen oder einen in sie verschieben.
- **Beim Bearbeiten eines Zugabschnitts in einem Umlauf steht jetzt, für welche Fahrzeugarten der Umlauf
  gilt** — Lokomotive, Triebzug oder Wagengruppe. Jede Art wird einmal genannt; zeigen Sie darauf, werden
  die Fahrzeuge selbst genannt.

### Fehlerbehebungen

- **Die App konnte aufhören, Ihre Arbeit zu speichern, ohne es zu sagen.** Konnte die App einen Plan nicht
  schreiben — ein Zug mit weniger als zwei Halten oder ein Fahrplanabschnitt, aus dem alle
  Streckenabschnitte entfernt wurden —, schlug dieses Speichern stillschweigend fehl, und alles danach
  blieb am Bildschirm stehen, wurde aber nie gesichert. Beide Pläne lassen sich jetzt speichern, und
  schlägt ein Speichern doch einmal fehl, sagt es die Kopfzeile sofort.

- **Eine gespeicherte Plandatei ist rund 40 % kleiner.** Jeder Halt wurde zweimal geschrieben — einmal
  beim Zug und einmal unter dem Gleis, an dem er liegt —, und die zweite Fassung zog einen Großteil des
  übrigen Plans mit sich. Ein mit einer früheren Version gespeicherter Plan lässt sich weiterhin öffnen.

- **Ein Zug, der auf einem Teil seines Laufs ohne Triebfahrzeug bleibt, wird jetzt gemeldet.** Die Prüfung
  fragte nur, ob *irgendwo* eine Lokomotive oder ein Triebzug den Zug fuhr; wurde ein Umlauf an einem Ende
  gekürzt, blieb der Rest des Zuges kommentarlos ohne Fahrzeug. Jetzt wird jeder Abschnitt für jede
  Fahrrunde geprüft, und der Konflikt nennt, zwischen welchen Betriebsstellen und in welchen Fahrrunden
  das Triebfahrzeug fehlt; Pläne, die sauber aussahen, können das jetzt melden.

## Version 0.3.2

### Änderungen

- Unter **Güterverkehr › Güterbeschreibungen** kann eine Herkunft oder ein Ziel jetzt jede Betriebsstelle
  sein, die Güter austauscht, nicht nur ein Bahnhof — ein Industriegebiet behandelt immer Güterwagen, war
  aber bisher nicht wählbar. Dieselben Listen sagen jetzt **Betriebsstelle** statt *Bahnhof*.
- Die Halte eines Zuges sind immer in der **Reihenfolge seines Laufwegs** aufgelistet.
- Das Ändern einer Haltzeit im Reiter **Züge** **nimmt jetzt den übrigen Zug mit**: eine **Abfahrt** wirkt
  vorwärts, in Fahrtrichtung, eine **Ankunft** rückwärts, sodass der Lauf bis zur Änderung mitgeht. Die
  Zeiten auf der anderen Seite bleiben stehen, die Fahr- und Aufenthaltszeiten bleiben erhalten, und die
  Änderung wird abgelehnt, wenn sie den Zug aus den Betriebszeiten des Plans führen würde.
- Ein Zug, dessen Laufweg eine **Betriebsstelle überspringt** — zwei aufeinanderfolgende Halte ohne
  Strecke dazwischen —, wird jetzt als Konflikt gemeldet. Die Prüfung lässt sich unter **Einstellungen ›
  Validierung** abschalten.
- Ein Zugabschnitt in einem **Umlauf** lässt sich jetzt **bearbeiten**: Der Stift öffnet seinen Anfangs-
  und Endhalt, sodass ein Umlauf umgeformt werden kann, ohne alles danach zu entfernen. Ein benachbarter
  Zugabschnitt, der an den geänderten anschließt, passt sich mit an; ein Nachbarabschnitt, dessen eigener
  Zug am neuen Halt nicht hält, bleibt unverändert, und die entstandene Lücke wird als Konflikt gemeldet.
- **Zug hinzufügen** kann jetzt den **Gegenzug** gleich mit anlegen. Mit *Gegenzug?* entsteht neben dem
  ersten Zug auch der Zug zurück: dieselbe Strecke in Gegenrichtung, dieselbe Zuggattung und
  Geschwindigkeit und die nächste Nummer der Gegenrichtung; seine Abfahrt ist entweder *so früh wie
  möglich* oder eine Zeit, die Sie eingeben. Zusammen mit *Wiederholen?* werden beide Richtungen
  wiederholt.

### Fehlerbehebungen

- Die **Kilometerangaben** im gedruckten Fahrplan und am Bildfahrplan werden jetzt auf ganze Kilometer
  gerundet, und eine Zweigstrecke zeigt am Abzweigbahnhof dieselbe Kilometerangabe wie die Strecke, von
  der sie abzweigt.
- Alles, was den Laufweg eines Zuges liest, folgt jetzt **der Reihenfolge, in der der Zug seine Halte
  befährt**, nicht der Eingabereihenfolge. Bei einem Zug, dessen Halte in falscher Reihenfolge eingegeben
  wurden, verlief die Linie im **Bildfahrplan** im Zickzack, konnte der gedruckte **Fahrplan** eine
  Abfahrt dort zeigen, wo der Zug ankommt, verkettete **Automatisch erstellen** den Zug gar nicht, maß
  **Zug wiederholen** den Abstand ab dem falschen Halt, und das Neuberechnen der Zeiten schlug ganz fehl.
  Importierte Pläne waren nie betroffen.
- **Die Zuggeschwindigkeit wird jetzt auch auf der letzten Strecke geprüft**, bis zu der Betriebsstelle,
  an der der Zug endet.

## Version 0.3.1

### Änderungen

- Der Abschnitt **Triebfahrzeuge** auf der Seite eines Zugabschnitts im Heft Lokführerdienste hat seine
  Überschrift jetzt in der gewählten Sprache. Es war die einzige Überschrift im Heft ohne Übersetzung.
- Das Triebfahrzeug wird jetzt für jeden Zugabschnitt gedruckt, der eines hat. In Plänen, die mit einer
  früheren Version importiert wurden, zeigten manche Zugabschnitte unter **Dienste** ein Triebfahrzeug, im
  Heft aber keines.
- Hinweise zu Zügen in gleicher Richtung sagen jetzt, welcher Zug am anderen vorbeikommt — **Überholt GD
  42757 12:02-12:05** oder **Wird überholt von GD 42757 12:02** — statt des bisherigen *"Trifft GD 42757
  in gleicher Richtung"*, das nie sagte, welcher Zug vorankam. Zwei Züge, die nur gleichzeitig im selben
  Bahnhof stehen, ergeben gar keinen Hinweis mehr.
- Eine Begegnung ohne Dauer — der andere Zug fährt ohne Halt durch — wird als eine einzelne Uhrzeit
  gedruckt statt als Zeitraum von einer Uhrzeit zu sich selbst.
- Ein Zug, der in einem Bahnhof seine Fahrt beginnt oder beendet, wird dort nicht mehr als getroffen,
  gekreuzt oder überholt aufgeführt. Diese Zeiten sind der Dienstantritt und das Dienstende seines
  Lokführers.

## Version 0.3.0

### Änderungen

- Ein neuer Bericht, **Lokführerdienste**, druckt für jeden Dienst ein A5-Heft. Die Titelseite zeigt die
  Dienstnummer, in welchen Sitzungen oder an welchen Tagen er läuft, seine Start- und Endzeit und
  -bahnhöfe, einen Schwierigkeitsgrad, den Besetzungsbedarf und etwaige Diensthinweise; jeder Zugabschnitt
  erhält dann seine eigene Seite mit den zu verwendenden Triebfahrzeugen, den mitzuführenden
  Wagengruppen, den Zielen, zu denen Güterwagen mitgeführt werden, und dem Fahrplan, jeweils in einem
  eigenen Block.
- Ein neuer Bericht, **Allgemeine Anweisungen**, ist ein eigenes Heft mit dem Programm des Treffens und
  den Anweisungen, die für die Anlage während des ganzen Treffens gelten — Fahranweisungen, Signalgebung,
  Funk- und Telefonverkehr, Verhalten bei Verspätung und wen man fragt — und wird einmal an alle
  ausgegeben. Es beginnt mit dem Namen des Treffens und seinen Daten, dann folgt das Programm, das jeder
  Teilnehmer vor der ersten Sitzung wissen muss, dann die Anweisungen über so viele Seiten, wie sie
  benötigen, umbrochen zwischen Absätzen und nie mit einer allein stehenden Überschrift.
- Die letzte Seite beider Hefte zeigt den Gleisplan der Anlage und die Tabelle der Rangierbahnhöfe, damit
  auch diejenigen, die nie ein Dienstheft in der Hand halten — vor allem das Bahnhofspersonal —, einen
  Überblick über die Anlage bekommen.
- Sowohl das Programm als auch die Anweisungen werden unter **Einstellungen › Information** geschrieben
  und lassen sich mit Markdown formatieren. Beide Hefte werden in A5 gedruckt: A4 quer, beidseitig, in der
  Mitte gefaltet, mit Leerseiten dort, wo sie nötig sind, damit die Bogen richtig gefaltet werden.
- Dienste können jetzt mit **Leicht**, **Mittel** oder **Erfahren** bewertet werden, im Heft farblich
  gekennzeichnet, können angeben, dass sie zwei oder drei Personen benötigen — zum Beispiel einen
  Lokführer und einen Schaffner —, und können mit einer **festen Nummer** versehen werden, die die
  automatische Neunummerierung unverändert lässt.
- Der Plan wird jetzt auch geprüft, damit jeder Zugabschnitt mit zugewiesener Lokomotive oder zugewiesenem
  Triebzug in jeder Sitzung, in der er fährt, von einem Dienst abgedeckt ist. Ein Dienst mit fester Nummer
  muss eine Nummer haben, und keine zwei solchen Dienste dürfen dieselbe Nummer erhalten.
- Unternehmen können jetzt ein hochgeladenes **Logo** haben, das in Berichten anstelle der Textsignatur
  angezeigt wird.
- Stationen können jetzt als der **Rangierbahnhof** gekennzeichnet werden, der den Ortsgüterverkehr eines
  anderen Ortes bedient, und die Anlage listet jeden Rangierbahnhof und was er abdeckt auf der letzten
  Seite des Diensthefts auf.
- Jedem Fahrplanabschnitt kann jetzt eine **Farbe** zugewiesen werden, mit der er im Topologie-Diagramm
  gezeichnet wird.
- Ein neuer **Entfernungsfaktor** (Einstellungen › Zeit & Geschwindigkeit) lässt eine Anlage in Berichten
  und im grafischen Fahrplan eine größere, vorbildgetreuere Kilometerangabe zeigen, als tatsächlich
  modelliert ist, ohne dass dies eine Fahrzeitberechnung beeinflusst.
- Die App hält jetzt mehrere geöffnete Browser-Tabs oder -Fenster miteinander synchron. **Hinweis**: Dies
  funktioniert nur zwischen Fenstern auf demselben Rechner im selben Browser.
- Einstellungen können jetzt das **Gültig ab**- und **Gültig bis**-Datum des Treffens speichern, gedruckt
  als Gültigkeitszeile auf Berichten; leer lassen, solange noch kein Treffen gebucht ist.
- Eine neue Option, **Planzeiten automatisch erweitern?** (Einstellungen › Allgemein), erweitert die
  Start- oder Endzeit des Plans, um einen Zug abzudecken, anstatt die Änderung zu blockieren.
  Standardmäßig aus.
- Eine neue Schaltfläche, **Alle Zeiten aktualisieren**, im grafischen Fahrplan berechnet alle Züge des
  Fahrplans auf einmal neu, statt vorher eine Teilmenge auswählen zu müssen.
- Die Gleisbelegungsprüfung kann jetzt optional berücksichtigen, dass eine Lokomotive oder ein Triebzug
  zwischen zwei Zügen auf einem Gleis steht, es sei denn, sie ist zum oder vom Abstellgleis gebucht
  (Einstellungen › Validierung). Standardmäßig aus, da dies nur auf Anlagen sinnvoll ist, auf denen das
  Abstellen bewusst modelliert wird.
- Jeder Halt im Reiter **Züge** hat jetzt ein Feld **Bemerkung** — ein Hinweis, der bei diesem Halt
  gedruckt wird, zum Beispiel „Gegenzug abwarten“. Die Bemerkung erscheint fertig formatiert und zeigt die
  eingegebene Auszeichnung, sobald man in das Feld geht: `*langsam*` für kursiv, `**erstes**` für fett.

### Fehlerbehebungen

- Beim Hinzufügen eines neuen Zuges wird die Standardstartzeit jetzt unter Berücksichtigung der angegebenen
  Vorbereitungszeit gesetzt, sodass er nicht vor der Startzeit des Plans beginnt.

## Version 0.2.4

### Änderungen

- Eine neue Registerkarte **Dienste** ermöglicht die Planung von Fahrerdiensten — die Arbeit, die ein
  Triebfahrzeugführer während einer Sitzung verrichtet, als Folge der Zugabschnitte, die er fährt. Jeder
  Dienst ist eine Zeile: links Bezeichnung, Unternehmen und Sitzungen, rechts die Zugabschnitte in
  Fahrreihenfolge.
- Fügen Sie die Zugabschnitte mit **Zugabschnitt hinzufügen** hinzu. Die Auswahl zeigt die
  Triebfahrzeugabschnitte, die ein Fahrer als Nächstes übernehmen könnte — solche, die zeitlich nicht mit
  dem Dienst kollidieren, und, sobald er einen Zugabschnitt hat, solche, die bei oder nach seiner Ankunft
  abfahren. Zugabschnitte müssen nicht an derselben Station beginnen: der Fahrer geht einfach dorthin, wo
  der nächste beginnt.
- Derselbe Zugabschnitt kann von mehreren Diensten gefahren werden, solange sie an verschiedenen Sitzungen
  laufen, sodass ein Dienst die ungeraden und ein anderer die geraden Sitzungen abdecken kann.
- Wo zwei Zugabschnitte desselben Zuges in einem Dienst von verschiedenen Triebfahrzeugen gefahren werden,
  zeigt die Registerkarte einen Hinweis an der Station, an der das Triebfahrzeug gewechselt wird — Sie
  geben ihn nicht von Hand ein.
- Aus XPLN importierte Dienste teilen sich nun die in den Fahrzeugumläufen definierten Zugabschnitte,
  sodass jeder Zugabschnitt das Triebfahrzeug zeigt, das ihn fährt.
- Der Plan wird geprüft, damit kein Zugabschnitt von zwei Diensten in derselben Sitzung gefahren wird und
  kein Dienst zeitlich überlappende Zugabschnitte hat. Die Prüfung lässt sich unter **Einstellungen ›
  Validierung** abschalten.

## Version 0.2.2

### Fehlerbehebungen

- Zwei Züge, die nie in derselben Betriebssitzung fahren, werden nicht mehr als Begegnung auf einer
  eingleisigen Strecke gemeldet. Ein Zug in den Sitzungen 1, 3, 5 und einer in 2, 4, 6 sind nie
  gleichzeitig unterwegs.
- Die Konfliktprüfung auf zweigleisigen und mehrgleisigen Strecken ist jetzt genau: Eine Strecke wird nur
  gemeldet, wenn sich mehr Züge gleichzeitig auf ihr befinden, als sie Gleise hat, und nur Züge gezählt
  werden, die in einer gemeinsamen Sitzung fahren.

## Version 0.2.1

### Änderungen

- Konfliktwarnungen werden jetzt dort angezeigt, wo Sie sie beheben können: Zugkonflikte im Bildfahrplan
  und auf der Registerkarte **Züge**, Fahrzeug- und Umlaufkonflikte auf der Registerkarte **Umläufe**.
- Auf der Registerkarte **Umläufe** hebt ein Fahrzeugkonflikt jetzt nur das betroffene Fahrzeug hervor und
  ein Umlaufkonflikt nur den betreffenden Umlauf.
- Die Prüfung, ob ein Fahrzeug zu seinem Ausgangspunkt zurückkehrt, umfasst jetzt auch Wagengruppen und
  Fracht, nicht nur Lokomotiven und Triebzüge.

## Version 0.2.0

### Änderungen

- Der Name des Plans, an dem Sie gerade arbeiten, wird jetzt in der oberen Leiste angezeigt.
- Der grafische Fahrplan zeigt jetzt Balken für den Lokführerbedarf, sodass sich leichter erkennen lässt,
  wie viele Lokführer während der Betriebssitzung benötigt werden.
- Eine neue Ansicht **Topologie** (unter der Registerkarte **Strecken**) zeigt ein schematisches Diagramm
  der Fahrplanstrecken und ihrer Abzweigungen.

### Fehlerbehebungen

- Strecken behalten jetzt standardmäßig die Reihenfolge, in der Sie sie eingegeben haben. Sie können
  weiterhin nach jeder Spalte sortieren.
- Konflikte verweisen nicht mehr auf Züge, die Sie nicht finden können: Wird ein Zug gelöscht, werden
  seine Halte mit entfernt, sodass keine verwaisten Halte oder falschen Konflikte zurückbleiben.

## Version 0.1.0

Erste Vorschau des Fahrplaners. Sie können:

- Gleispläne mit Bahnhöfen, Gleisen und Strecken definieren.
- Zugfahrpläne mit automatischer Zeitberechnung erstellen und bearbeiten.
- Lokomotiven und Triebwagen den Zügen zuweisen.
- Fahrzeugumläufe erstellen und Umlaufkarten drucken.
- Güterverkehr zwischen Bahnhöfen planen.
- Grafische Fahrpläne (Zeit-Weg-Diagramme) anzeigen.
- Fahrpläne auf Konflikte und Inkonsistenzen prüfen.
- Druckausgaben erzeugen: Zugkarten, Bahnhofsbücher und Dienstpläne.
- Auf Englisch, Deutsch, Dänisch, Norwegisch und Schwedisch arbeiten.
