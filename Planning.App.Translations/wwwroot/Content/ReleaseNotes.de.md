# Versionshinweise

## Version 0.2.4

- Eine neue Registerkarte **Schichten** ermöglicht die Planung von Fahrerschichten – die
  Arbeit, die ein Triebfahrzeugführer während einer Sitzung verrichtet, als Folge der
  Zugteile, die er fährt. Jede Schicht ist eine Zeile: links Bezeichnung, Unternehmen und
  Sitzungen, rechts die Zugteile in Fahrreihenfolge.
- Fügen Sie die Zugteile mit **Zugteil hinzufügen** hinzu. Die Auswahl zeigt die
  Triebfahrzeugabschnitte, die ein Fahrer als Nächstes übernehmen könnte – solche, die
  zeitlich nicht mit der Schicht kollidieren, und, sobald sie einen Zugteil hat, solche,
  die bei oder nach ihrer Ankunft abfahren. Zugteile müssen nicht an derselben Station
  beginnen: zwischen zwei Zugteilen geht der Fahrer einfach dorthin, wo der nächste
  beginnt.
- Derselbe Zugteil kann von mehreren Schichten gefahren werden, solange sie an
  verschiedenen Sitzungen laufen, sodass eine Schicht die ungeraden und eine andere die
  geraden Sitzungen abdecken kann.
- Wo zwei Zugteile desselben Zuges in einer Schicht von verschiedenen Triebfahrzeugen
  gefahren werden, zeigt die Registerkarte nun einen Hinweis an der Station, an der das
  Triebfahrzeug gewechselt wird – Sie geben ihn nicht von Hand ein.
- Sie können jeder Schicht eine Bezeichnung und ein Unternehmen geben, die Sitzungen
  wählen, an denen sie läuft, und freie Anmerkungen hinzufügen, die für die ganze Schicht
  gelten.
- Aus XPLN importierte Schichten teilen sich nun die in den Fahrzeugumläufen definierten
  Zugteile, sodass jeder Zugteil das Triebfahrzeug zeigt, das ihn fährt.
- Der Plan wird geprüft, damit kein Zugteil von zwei Schichten in derselben Sitzung
  gefahren wird und keine Schicht zeitlich überlappende Zugteile hat; etwaige Konflikte
  werden aufgelistet und auf der Registerkarte **Schichten** geöffnet. Sie können die
  Prüfung unter **Einstellungen › Validierung** ein- oder ausschalten.

## Version 0.2.2

### Fehlerbehebungen

- Zwei Züge, die nie in derselben Betriebssitzung fahren, werden nicht mehr als
  Begegnung auf einer eingleisigen Strecke gemeldet. Ein Zug, der in den Sitzungen
  1, 3, 5 fährt, und einer, der in 2, 4, 6 fährt, können jetzt dasselbe Gleis nutzen,
  ohne dass eine falsche Warnung erscheint, da sie nie gleichzeitig unterwegs sind.
- Die Konfliktprüfung auf zweigleisigen (und mehrgleisigen) Strecken ist jetzt genau:
  Eine Strecke wird nur gemeldet, wenn sich mehr Züge gleichzeitig auf ihr befinden,
  als sie Gleise hat, und nur Züge gezählt werden, die in einer gemeinsamen Sitzung
  fahren.

## Version 0.2.1

- Konfliktwarnungen werden jetzt dort angezeigt, wo Sie sie beheben können.
  Zugkonflikte erscheinen nur im Bildfahrplan und auf der Registerkarte **Züge**;
  Fahrzeug- und Umlaufkonflikte nur auf der Registerkarte **Umläufe**.
- Auf der Registerkarte **Umläufe** hebt ein Fahrzeugkonflikt jetzt nur das
  betroffene Fahrzeug hervor und ein Umlaufkonflikt nur den betreffenden Umlauf,
  sodass klar ist, was Aufmerksamkeit erfordert.
- Die Prüfung, ob ein Fahrzeug zu seinem Ausgangspunkt zurückkehrt, umfasst jetzt
  auch Wagengruppen und Fracht, nicht nur Lokomotiven und Triebzüge, sodass eine am
  Ende der Betriebssitzung fehl am Platz stehende Wagengruppe oder Fracht jetzt
  gemeldet wird.

## Version 0.2.0

- Der Name des Plans, an dem Sie gerade arbeiten, wird jetzt in der oberen Leiste
  angezeigt, sodass Sie immer sehen, welches Dokument geöffnet ist.
- Der grafische Fahrplan zeigt jetzt Balken für den Lokführerbedarf, sodass sich
  leichter erkennen lässt, wie viele Lokführer während der Betriebssitzung
  benötigt werden.
- Eine neue Ansicht **Topologie** (unter der Registerkarte **Strecken**) zeigt ein
  schematisches Diagramm der Fahrplanstrecken und ihrer Abzweigungen.

### Fehlerbehebungen

- Strecken behalten jetzt standardmäßig die Reihenfolge, in der Sie sie eingegeben
  haben, sodass die Liste beim Überprüfen Ihrer Eingaben leichter zu verfolgen ist.
  Sie können weiterhin nach jeder Spalte sortieren.
- Konflikte verweisen nicht mehr auf Züge, die Sie nicht finden können: Wird ein Zug
  gelöscht, werden seine Halte mit entfernt, sodass keine verwaisten Halte oder
  falschen Konflikte zurückbleiben.

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
