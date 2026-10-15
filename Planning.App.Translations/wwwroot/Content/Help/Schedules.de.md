Im Reiter **Umläufe** machen Sie aus einem Fahrplan die Fahrzeug- und Personalpläne, die nötig sind, um die
Fahrrunde zu fahren.

Hier erstellen Sie **Fahrzeugumläufe** und weisen sie Lokomotiven, Wagengruppen und Wagenströmen zu,
sodass jeder Zug die Ausrüstung erhält, die er braucht, und jedes Fahrzeug im Laufe des Tages einen durchgehenden, sinnvollen
Einsatz hat. Lokführerdienste fügen dies zu Arbeit zusammen, die eine Person leisten kann.

### Umlauf

Jeder Umlauf ist eine Zeile: Die erste Spalte listet das **Fahrzeug / die Fahrzeuge** auf, die ihn fahren, gefolgt von den
**Zugabschnitten** in Arbeitsreihenfolge. Eine Wagengruppe, die ihre Wagen auflistet, zeigt, wie viele es sind, und jede
Wagenklasse einmal nach ihrer Nummer, wie in *SJ 05 5 x A/B/Fv*.

Mit **Automatisch erstellen** verketten Sie Züge derselben Kategorie,
die dort fortsetzen, wo der vorige angekommen ist (markieren Sie eine Kategorie im Reiter **Zugkategorien** mit *Von automatischer Umlaufbildung ausschließen*,
um sie davon auszunehmen), oder mit **Neuer Umlauf** bauen Sie
einen von Hand auf. Das automatische Erstellen bietet die noch nicht eingeplanten Züge zuerst den bereits vorhandenen Umläufen an: Ein Umlauf
wird mit dem fortgesetzt, was ihn in derselben Kategorie fortsetzt, und ein von Ihnen angelegter leerer Umlauf
wird gefüllt, bevor ein neuer entsteht, sodass ein Fahrzeug, das bereits wendet, mehr Arbeit übernimmt,
bevor ein anderes in Dienst gestellt wird. Nur die Züge, die in keinen vorhandenen Umlauf passen, beginnen neue,
und Wagenströme bleiben, wie sie sind.
Bei einem Umlauf, der von einem Triebzug oder von einer Lokomotive mit angehaktem **Wendezug?** gefahren wird, werden die Ankünfte
auf das Gleis verlegt, von dem sein nächster Zug abfährt, weil ein solcher Zug auf dem Gleis abfährt, auf dem er eingefahren ist.
Endet der Umlauf, wo er begann, kommt sein letzter Zug auf dem Gleis an, von dem der erste
abfährt. Das geschieht, wenn der Triebzug oder die Lokomotive zugewiesen wird, wenn der Haken gesetzt wird und wenn Züge
hinzugefügt werden. Ein Umlauf mit einer gewöhnlichen Lokomotive oder noch ohne Fahrzeug behält seine Gleise: Sein nächster Zug
kann gut andere Wagen verwenden, was oft erst später in der Planung entschieden wird.

Die Schaltfläche **Ankunftsgleise** in einer Zeile verlegt die Ankünfte dieses Umlaufs jederzeit, gleichgültig welches
Fahrzeug ihn fährt, zum Beispiel sobald Sie wissen, dass eine Lokomotive ihre Wagen behält.

In einer Zeile:
- **+ Zug** hängt den nächsten Zug an – wählen Sie nur einen Teil davon (einen Von-/Bis-Halt),
  wenn ein Zug auf Fahrzeuge aufgeteilt werden muss, zum Beispiel bei einem Wechsel von elektrischer auf Dieseltraktion.
- Die kleinen **Gelenke** zwischen den Zugabschnitten sagen, wo das Fahrzeug steht und wie lange, und vor dem ersten Abschnitt, woher es
  geholt werden muss. Klicken Sie auf eines, um einen Zug in die Lücke einzuarbeiten: Angeboten werden nur
  die Züge, die das Fahrzeug in der verfügbaren Zeit tatsächlich schaffen könnte. Eine Teilleistung, die das Fahrzeug nicht
  dorthin zurückbringt, wo der Umlauf weitergeht, wird dennoch hinzugefügt und als Konflikt gemeldet,
  bis Sie die Rückleistung hinzufügen – so wird eine Hin-und-zurück-Fahrt in einen Aufenthalt eingearbeitet, eine Teilleistung
  nach der anderen. Ein Gelenk, an dem der Umlauf unterbrochen ist, ist bernsteinfarben markiert, und ein Klick darauf bietet die Züge an,
  die es überbrücken.
- Der **Stift** an einem Zugabschnitt ändert, wie viel von seinem Zug der Umlauf fährt: Wählen Sie einen neuen Von- oder
  Bis-Halt. Der Zug selbst bleibt; um einen anderen Zug zu fahren, entfernen Sie den Abschnitt und fügen den anderen hinzu.
  Der benachbarte Abschnitt, der an den geänderten anschließt, folgt mit, sodass der Umlauf ganz bleibt –
  kürzen Sie einen Abschnitt von A–C auf A–B, wird die Rückfahrt von selbst zu B–A. Hält der eigene Zug des Nachbarn
  nicht am neuen Halt, bleibt er, wie er ist, und die Lücke wird als Konflikt gemeldet,
  den Sie auflösen müssen.
  Derselbe Dialog sagt, wo die Fahrzeuge stehen, wenn das nicht das Gleis ist, das der Zug benutzt: **Holen von**
  nennt das Gleis, von dem sie vor der Abfahrt des Zuges geholt werden, und **Abstellen auf** das Gleis, auf das sie
  nach der Ankunft gestellt werden. Beides wird als Hinweis für den Triebfahrzeugführer und den Fahrdienstleiter gedruckt, anstelle des
  üblichen Hinweises zum An- oder Abkuppeln des Fahrzeugs. Die Gleise einer Wagengruppe stehen bereits in der
  Wagengruppentabelle des Triebfahrzeugführers, sodass dessen Hinweis nur sagt, die Wagen zum Abfahrtsgleis oder
  zu ihrem Ankunftsgleis zu rangieren. Fährt ein Fahrzeug den Zug nur an einigen der Fahrrunden oder Tage, an denen der Zug
  verkehrt, beginnt sein Hinweis mit diesen. Ein an einem Bahnhof gewähltes Gleis, an dem der Abschnitt
  nicht mehr beginnt oder endet, wird vergessen.
- **+ Fahrzeug** weist ein Fahrzeug zu und legt bei Bedarf ein neues an; ein Umlauf kann
  mehrere Fahrzeuge tragen (etwa eine Lokomotive und ihre Wagengruppe). Wagenströme werden in den Berichten als Umlaufkarten
  gezeigt, nicht hier. Ein Umlauf, der nichts als **Rangieraufgaben** enthält, braucht gar kein Fahrzeug:
  Eine solche Aufgabe wird von dem erledigt, was am Bahnhof steht, sodass es vollständig statt
  unfertig ist, sie ohne Fahrzeug zu lassen, und sie wird nicht als Umlauf ohne Fahrzeug gemeldet. Fügen Sie demselben Umlauf einen fahrenden
  Zug hinzu, wird wieder ein Fahrzeug verlangt.

Beim Bearbeiten einer Lokomotive wird **Wendezug?** angeboten. Haken Sie es an, wenn die Lokomotive einen Zug fährt, der von
beiden Enden gefahren werden kann – einen mit Steuerwagen am anderen Ende oder mit einer zweiten Lokomotive
dort. Wendet die Strecke eines solchen Zuges, wechselt er die Richtung durch Führerstandswechsel, sodass **Zeiten aktualisieren**
ihm nicht mehr die Zeit für das Umsetzen der Lokomotive gibt und der Halt auf den Mindesthalt schrumpft.
Ein Triebzug wird bereits so behandelt und bekommt kein Feld angeboten.

#### Verkehrende Fahrrunden oder Tage

Die verkehrenden Fahrrunden eines Umlaufs werden zur Teilmenge der Fahrrunden, an denen alle Züge verkehren. Bei einem Umlauf, dessen meiste Züge
täglich verkehren und von dem ein Paar nur an Fahrrunden 1–5 verkehrt, wird der ganze Umlauf 1–5.

#### Validierung

Die Validierungsregeln unter **Einstellungen › Validierung** (Lokomotivabdeckung, Lokführerdienste) prüfen,
dass die hier gebildeten Umläufe die Züge im Fahrplan tatsächlich abdecken.
