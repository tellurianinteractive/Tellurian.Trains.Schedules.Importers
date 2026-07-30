# Versionshinweise

## Version 0.3.1

- Der Abschnitt **Triebfahrzeuge** auf einer Zugteilseite im Heft Lokführerdienste hat seine
  Überschrift jetzt in der gewählten Sprache. Es war die einzige Überschrift im Heft ohne
  Übersetzung, sodass der Abschnitt nicht als die Triebfahrzeuge zu erkennen war.
- Das Triebfahrzeug wird jetzt für jeden Zugteil gedruckt, der eines hat. In Plänen, die mit
  einer früheren Version importiert wurden, zeigten manche Zugteile unter **Dienste** ein
  Triebfahrzeug, im Heft aber keines.
- Hinweise zu Zügen in gleicher Richtung sagen jetzt, welcher Zug am anderen vorbeikommt —
  **Überholt GD 42757 12:02-12:05** oder **Wird überholt von GD 42757 12:02** — statt des
  bisherigen *"Trifft GD 42757 in gleicher Richtung"*, das nie sagte, welcher Zug vorankam. Zwei
  Züge, die nur gleichzeitig im selben Bahnhof stehen, ergeben gar keinen Hinweis mehr, denn keiner
  ist am anderen vorbeigekommen.
- Eine Begegnung ohne Dauer — der andere Zug fährt ohne Halt durch — wird als eine einzelne Uhrzeit
  gedruckt statt als Zeitraum von einer Uhrzeit zu sich selbst.
- Ein Zug, der in einem Bahnhof seine Fahrt beginnt oder beendet, wird dort nicht mehr als
  getroffen, gekreuzt oder überholt aufgeführt. Diese Zeiten sind der Dienstantritt und das
  Dienstende seines Lokführers, nicht die Fahrt des Zuges.

## Version 0.3.0

- Ein neuer Bericht, **Lokführerdienste**, druckt für jeden Dienst ein A5-Heft. Die
  Titelseite zeigt die Dienstnummer, in welchen Sitzungen oder an welchen Tagen er
  läuft, seine Start- und Endzeit und -bahnhöfe, einen Schwierigkeitsgrad, den
  Besetzungsbedarf und etwaige Diensthinweise. Jeder Zugteil erhält seine eigene
  Seite, mit den zu verwendenden Triebfahrzeugen, den mitzuführenden Wagengruppen und
  den Zielen, zu denen Güterwagen mitgeführt werden, sowie den Fahrplan – jeweils in
  einem eigenen, klar abgegrenzten Block dargestellt. Die letzte Seite jedes Heftes
  zeigt den Gleisplan der Anlage und eine Tabelle der Rangierbahnhöfe, zum leichten
  Nachschlagen während des Betriebs.
- Ein neuer Bericht, **Allgemeine Anweisungen**, ist ein eigenes gedrucktes Heft mit
  dem Programm des Treffens und Anweisungen, die für eine Anlage während des ganzen
  Treffens gelten. Hier kann der Organisator des Treffens frei schreiben, was er
  möchte – zum Beispiel Fahranweisungen, Signalgebung, Funk- und Telefonverkehr,
  Verhalten bei Verspätung und wen man fragt – und es wird einmal an alle
  ausgegeben.
- Sowohl das Programm als auch die Anweisungen werden unter **Einstellungen ›
  Information** geschrieben und lassen sich mit Markdown formatieren – Überschriften,
  Listen, Fett- und Kursivschrift –, sodass auch ein langer Anweisungstext im Druck
  lesbar bleibt.
- Das Heft beginnt mit dem Namen des Treffens, den Daten seiner Gültigkeit und dem
  Druckdatum, gefolgt vom Programm: Sitzungszeiten, Pausen und Mahlzeiten – das, was
  jeder Teilnehmer vor der ersten Sitzung wissen muss.
- Danach folgen die Anweisungen über so viele Seiten, wie sie benötigen. Umbrochen wird
  zwischen Absätzen, und eine Überschrift bleibt immer bei dem Text, den sie einleitet.
- Die letzte Seite zeigt den Gleisplan der Anlage und die Tabelle der Rangierbahnhöfe,
  damit auch diejenigen, die nie ein Dienstheft in der Hand halten – vor allem das
  Bahnhofspersonal –, einen Überblick über die Anlage bekommen.
- Das Heft wird im selben A5-Format wie die Diensthefte gedruckt: A4 quer, beidseitig,
  in der Mitte gefaltet, mit Leerseiten dort, wo sie nötig sind, damit die Bogen richtig
  gefaltet werden.
- Dienste können jetzt mit **Leicht**, **Mittel** oder **Erfahren** bewertet werden,
  im Heft farblich gekennzeichnet, sodass ein Teilnehmer einen zu seiner Erfahrung
  passenden Dienst wählen kann.
- Ein Dienst kann jetzt angeben, dass er zwei oder drei Personen benötigt – zum
  Beispiel einen Lokführer und einen Schaffner –, und dies wird im Heft angezeigt.
- Ein Dienst kann mit einer **festen Nummer** versehen werden, sodass die
  automatische Neunummerierung ihn unverändert lässt, zum Beispiel für
  Sonderdienste, die ausgegeben werden, bevor eine Sitzung beginnt.
- Der Plan wird jetzt auch geprüft, damit jeder Zugteil mit zugewiesener Lokomotive
  oder zugewiesenem Triebzug in jeder Sitzung, in der er fährt, von einem Dienst
  abgedeckt ist – ein Teil, für den niemand eingeteilt ist, wird sitzungsweise
  gemeldet. Ein Dienst mit fester Nummer wird ebenfalls geprüft: Er muss eine Nummer
  haben, und keine zwei Dienste mit fester Nummer dürfen dieselbe Nummer erhalten.
- Unternehmen können jetzt ein hochgeladenes **Logo** haben, das in Berichten
  anstelle der Textsignatur angezeigt wird.
- Stationen können jetzt als der **Rangierbahnhof** gekennzeichnet werden, der den
  Ortsgüterverkehr eines anderen Ortes bedient; die Anlage listet automatisch jeden
  Rangierbahnhof und was er abdeckt auf, gezeigt auf der letzten Seite des
  Diensthefts. Das hilft Stationspersonal und Güterzugführern zu wissen, wohin Wagen
  mit einem bestimmten Frachtziel geschickt werden sollen.
- Jedem Fahrplanabschnitt kann jetzt eine **Farbe** zugewiesen werden, mit der er im
  Topologie-Diagramm gezeichnet wird.
- Ein neuer **Entfernungsfaktor** (unter Einstellungen › Zeit & Geschwindigkeit)
  lässt eine Anlage in Berichten und im grafischen Fahrplan eine andere – meist
  größere, vorbildgetreuere – Kilometerangabe zeigen, als tatsächlich modelliert
  ist, ohne dass dies eine Fahrzeitberechnung beeinflusst.
- Die App hält jetzt mehrere geöffnete Browser-Tabs oder -Fenster miteinander
  synchron. **Hinweis**: Dies funktioniert nur zwischen Fenstern auf demselben
  Rechner im selben Browser.
- Einstellungen können jetzt das **Gültig ab**- und **Gültig bis**-Datum des
  Treffens speichern, gedruckt als Gültigkeitszeile auf Berichten; leer lassen,
  solange noch kein Treffen gebucht ist.
- Eine neue Option, **Planzeiten automatisch erweitern?** (unter Einstellungen ›
  Allgemein), erweitert die Start- oder Endzeit des Plans, um einen Zug abzudecken,
  anstatt die Änderung zu blockieren, wenn die eigene Zeit des Zuges außerhalb davon
  liegt. Standardmäßig aus.
- Eine neue Schaltfläche, **Alle Zeiten aktualisieren**, im grafischen Fahrplan
  berechnet alle Züge des Fahrplans auf einmal neu, statt vorher eine Teilmenge
  auswählen zu müssen.
- Die Gleisbelegungsprüfung kann jetzt optional berücksichtigen, dass eine
  Lokomotive oder ein Triebzug zwischen zwei Zügen auf einem Gleis steht, es sei
  denn, sie ist zum oder vom Abstellgleis gebucht (unter Einstellungen ›
  Validierung). Standardmäßig aus, da dies nur auf Anlagen sinnvoll ist, auf denen
  das Abstellen bewusst modelliert wird – dort eingeschaltet, deckt sie einen
  dritten Zug auf, der unbemerkt ein Gleis nutzt, das ein stehendes Fahrzeug bereits
  belegt.
- Jeder Halt im Reiter **Züge** hat jetzt ein Feld **Bemerkung** – ein Hinweis, der bei
  diesem Halt gedruckt wird, zum Beispiel „Gegenzug abwarten“. Die Bemerkung erscheint
  fertig formatiert und zeigt die eingegebene Auszeichnung, sobald man in das Feld geht, so
  dass sich das Wesentliche hervorheben lässt: `*langsam*` für kursiv, `**erstes**` für
  fett. Wird das Feld geleert, verschwindet die Bemerkung wieder.

### Fehlerbehebungen

- Beim Hinzufügen eines neuen Zuges wird die Standardstartzeit jetzt unter
  Berücksichtigung der angegebenen Vorbereitungszeit gesetzt, sodass er nicht vor
  der Startzeit des Plans beginnt.

## Version 0.2.4

- Eine neue Registerkarte **Dienste** ermöglicht die Planung von Fahrerdiensten – die
  Arbeit, die ein Triebfahrzeugführer während einer Sitzung verrichtet, als Folge der
  Zugteile, die er fährt. Jeder Dienst ist eine Zeile: links Bezeichnung, Unternehmen und
  Sitzungen, rechts die Zugteile in Fahrreihenfolge.
- Fügen Sie die Zugteile mit **Zugteil hinzufügen** hinzu. Die Auswahl zeigt die
  Triebfahrzeugabschnitte, die ein Fahrer als Nächstes übernehmen könnte – solche, die
  zeitlich nicht mit dem Dienst kollidieren, und, sobald er einen Zugteil hat, solche,
  die bei oder nach seiner Ankunft abfahren. Zugteile müssen nicht an derselben Station
  beginnen: zwischen zwei Zugteilen geht der Fahrer einfach dorthin, wo der nächste
  beginnt.
- Derselbe Zugteil kann von mehreren Diensten gefahren werden, solange sie an
  verschiedenen Sitzungen laufen, sodass ein Dienst die ungeraden und ein anderer die
  geraden Sitzungen abdecken kann.
- Wo zwei Zugteile desselben Zuges in einem Dienst von verschiedenen Triebfahrzeugen
  gefahren werden, zeigt die Registerkarte nun einen Hinweis an der Station, an der das
  Triebfahrzeug gewechselt wird – Sie geben ihn nicht von Hand ein.
- Sie können jedem Dienst eine Bezeichnung und ein Unternehmen geben, die Sitzungen
  wählen, an denen er läuft, und freie Anmerkungen hinzufügen, die für den ganzen Dienst
  gelten.
- Aus XPLN importierte Dienste teilen sich nun die in den Fahrzeugumläufen definierten
  Zugteile, sodass jeder Zugteil das Triebfahrzeug zeigt, das ihn fährt.
- Der Plan wird geprüft, damit kein Zugteil von zwei Diensten in derselben Sitzung
  gefahren wird und kein Dienst zeitlich überlappende Zugteile hat; etwaige Konflikte
  werden aufgelistet und auf der Registerkarte **Dienste** geöffnet. Sie können die
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
