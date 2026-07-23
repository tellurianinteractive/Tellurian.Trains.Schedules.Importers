# Versionshinweise

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
