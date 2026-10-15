Der Reiter **Bildfahrplan** zeichnet einen Bildfahrplan (Weg-Zeit-Diagramm) für die
Fahrplanabschnitte, die Sie auswählen.

### Auswählen, was angezeigt wird

Haken Sie einen oder mehrere **Fahrplanabschnitte** zum Zeichnen an. Jeder gewählte Abschnitt wird als eigene
Grafik gezeichnet. Die Abschnitte stammen aus dem Reiter **Strecken**; sind keine definiert, gibt es nichts zu zeichnen.

#### Eine Hälfte anzeigen

Ist unter **Einstellungen › Allgemein** eine **Umbruchzeit** festgelegt, erscheint eine Auswahl **Anzeigen** mit
**Ganzer Bildfahrplan**, **Erste Hälfte** und **Letzte Hälfte**. Die erste Hälfte reicht vom Betriebsbeginn
bis zum Umbruch; die letzte Hälfte vom Umbruch bis zum Betriebsende. Wählen Sie eine Hälfte, wird nur dieser Teil des
Tages gezeichnet, was die Grafik auf kleineren Bildschirmen lesbar hält – besonders bei senkrechter Zeitachse.
Die Auswahl ist ausgeblendet, wenn keine Umbruchzeit festgelegt ist.

### Züge kopieren

**Züge kopieren** kopiert die Züge, die Sie auswählen. Geben Sie die Minuten an, um die die Kopie versetzt wird –
positiv ist später, negativ früher –, und jeder gewählte Zug wird einmal kopiert.

Haken Sie **Gegenrichtung?** an, fährt die Kopie stattdessen dieselbe Strecke rückwärts, von dort, wo der Zug
endete, dorthin, wo er begann. Alle Fahrzeiten und alle Halte bleiben erhalten, die Vorbereitungs- und Abschlusszeiten
folgen der Kopie an ihre eigenen Enden, und sie erhält eine Nummer aus der Nummernreihe der Gegenrichtung. Die Minuten
werden dann ab der letzten Abfahrt des kopierten Zuges gezählt, geben also an, wie lange nach dem Abstellen dieses Zuges
die Rückleistung abfährt.

Haken Sie unter **Züge wiederholen** **Wiederholen?** an, um statt einer einzelnen Kopie eine Serie zu erzeugen: Die erste Kopie
entsteht mit der angegebenen Versetzung, und alle weiteren je Intervall, bis eine Kopie nach der Endzeit abfahren würde.
Das ist dieselbe Wiederholung wie beim Hinzufügen eines Zuges, aber für einen bereits vorhandenen Zug – so kann
der erste Zug angepasst werden, bis er richtig fährt, bevor der Rest von ihm kopiert wird.

Eine Kopie, die außerhalb von Start- und Endzeit des Plans verkehren würde, wird nicht erzeugt.

### Den Bildfahrplan lesen

Die Bahnhöfe sind entlang der einen Achse verteilt und die Zeit entlang der anderen; jeder Zug ist eine geneigte Linie, gefärbt
nach seiner Kategorie. Ausrichtung, Abstände und Beschriftungen werden unter
**Einstellungen › Bildfahrplan** gesteuert.

### Benötigte Triebfahrzeugführer

Ist unter **Einstellungen › Allgemein** **Erwartete Triebfahrzeugführer** festgelegt, folgt ein Streifen aus Balken der
Zeitachse der Grafiken – oberhalb bei waagerechter Zeitachse, links davon bei senkrechter. Jeder
Balken zeigt, wie viele Triebfahrzeugführer zu dieser Zeit benötigt werden, an der am stärksten belasteten Fahrrunde:

- Jeder Zug und jede Rangieraufgabe braucht einen Führer von der ersten Ankunft bis zur letzten Abfahrt,
  einschließlich der Halte unterwegs,
- ein Führer in einem **Dienst** ist auch zwischen den vom Dienst gefahrenen Zügen gebunden – wartend oder auf dem Weg zum
  nächsten – sowie vor dem ersten Zug oder nach dem letzten, wenn Beginn oder Ende des Dienstes
  über diese hinaus gesetzt wurde.

Die Balken werden nach der erwarteten Zahl eingefärbt: rot, wenn mehr Führer benötigt werden als
erwartet, grün bei genau dieser Zahl, gelb, wenn ein oder zwei übrig sind, und grau, wenn drei oder mehr
übrig sind. Eine Linie markiert die erwartete Zahl. Zeigen Sie auf einen Balken, um seine Zeiten und die Anzahl zu sehen.

### Start- und Endzeit

Der Bildfahrplan zeigt den unter **Einstellungen › Allgemein** festgelegten Zeitrahmen.
Eine besondere Option lässt den Bildfahrplan für durchgehenden Betrieb einen vollen Tag von 00:00 bis 24:00 zeigen.
Ein Zug, dessen Zeiten über Mitternacht hinausgehen, wird umgebrochen und setzt sich ab 00:00 am Anfang der Achse fort.
