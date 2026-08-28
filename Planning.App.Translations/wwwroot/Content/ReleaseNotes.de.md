# Versionshinweise

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
