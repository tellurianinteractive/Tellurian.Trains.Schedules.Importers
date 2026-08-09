# Versionshinweise

## Version 0.5.0

### Änderungen

- **Ein Wendezug wartet nicht mehr auf das Umsetzen der Lok.** Wo der Fahrweg eines Zuges wendet, war
  bisher immer so viel Zeit vorgesehen, dass die Lok ans andere Ende umsetzen kann — gleich, was den Zug
  befördert. Bearbeiten Sie eine Lok im Reiter Umläufe und setzen Sie das neue Häkchen **Wendezug?**, wo
  die Lok einen Zug befördert, der von beiden Enden gefahren werden kann: einen Zug mit Steuerwagen am
  anderen Ende oder mit einer zweiten Lok dort. Der Triebfahrzeugführer wechselt einfach den Führerstand,
  daher rechnet **Zeiten aktualisieren** das Umsetzen jetzt heraus, und der Zug hält stattdessen nur den
  Mindestaufenthalt, wodurch alle folgenden Halte früher liegen.

  Ein **Triebzug** wird ebenso behandelt, ohne dass etwas anzuhaken wäre — er wendet, wie er steht, was
  die Zeitberechnung bisher nicht berücksichtigt hat. Nehmen Sie das Häkchen zurück oder geben Sie dem
  Zug eine gewöhnliche Lok, so kommt die Zeit für das Umsetzen wieder. Ein Aufenthalt, den Sie bewusst
  länger als das Umsetzen selbst gemacht haben, bleibt so, wie Sie ihn gesetzt haben.

- **Ein Gleis kann jetzt angeben, für welchen Fahrweg durch die Betriebsstelle es vorgesehen ist.** Wo
  eine Betriebsstelle mehr als ein Gleis und eine Weiterfahrt hat, kann jedes ihrer Gleise die
  **vorherige** Betriebsstelle nennen, von der ein Zug kommt, die **nächste**, zu der er weiterfährt,
  oder beide — dazu das Kästchen **beide Richtungen**, wenn dasselbe Gleis auch der Gegenrichtung dient.
  Angeboten werden nur Betriebsstellen, die von hier über eine Strecke erreichbar sind, damit ein Gleis
  nur für einen Laufweg vorgesehen werden kann, den ein Zug auch nehmen kann.

  Ein neuer Zug kommt dann auf das Gleis, das zu seinem Laufweg am besten passt: ein Gleis, das genau
  angibt, woher der Zug kommt und wohin er weiterfährt, geht einem vor, das nur eines von beidem nennt,
  und dieses wiederum einem Gleis ohne Angabe; ein Gleis, das für einen anderen Laufweg vorgesehen ist,
  bleibt den Zügen vorbehalten, für die es gedacht ist. Genau das braucht eine **zweigleisige Strecke** —
  geben Sie dem einen Gleis die vorherige und die nächste Betriebsstelle der einen Richtung und dem
  anderen dasselbe Paar umgekehrt, dann bleibt jede Richtung auf ihrem Gleis. Lassen Sie die Spalten
  leer, ändert sich nichts gegenüber vorher.

  Passen zwei Gleise gleich gut zum Laufweg, nimmt ein Reisezug, der **hält**, ein Gleis mit Bahnsteig,
  und ein Zug, der **durchfährt** — wie jeder Zug ohne Reisendenwechsel — das Hauptgleis. Bisher nahm ein
  Reisezug an jeder Betriebsstelle ein Bahnsteiggleis, ob er dort hielt oder nicht.

- **Ein Zug lässt sich jetzt in der Gegenrichtung kopieren und mehrfach wiederholen.** Das Kopieren eines
  Zuges ergab eine einzige Kopie, die in dieselbe Richtung fuhr wie der Zug, aus dem sie kam. Mit
  **Gegenrichtung?** befährt die Kopie stattdessen den Laufweg rückwärts, von dort, wo der Zug endete,
  bis dorthin, wo er begann: alle Fahrzeiten und alle Halte bleiben erhalten, Vorbereitungs- und
  Abschlusszeit wechseln die Seite, und die Kopie erhält eine Nummer aus der Reihe der Gegenrichtung. Die
  Minuten zählen dann ab der letzten Abfahrt des kopierten Zuges: 20 Minuten legen die Rückleistung 20
  Minuten nach dem Abschluss des Zuges, aus dem sie zurückkehrt.

  Der Kopierdialog hat jetzt auch die Möglichkeit **Züge wiederholen**, die es beim Hinzufügen eines
  Zuges gibt: Endzeit und Abstand angeben, und je Abstand wird eine Kopie hinzugefügt, bis die Endzeit
  überschritten ist. Ein Zug lässt sich jetzt zuerst allein anlegen, so lange anpassen, bis er richtig
  fährt, und erst dann über den Tag wiederholen — bisher musste die ganze Reihe schon beim Anlegen des
  ersten Zuges bestellt werden.

- **Ein Gleis kann jetzt angeben, wie lang sein Bahnsteig ist.** Wo eine Betriebsstelle Reisende
  austauscht, hat jedes ihrer Gleise eine **Bahnsteiglänge** in Metern. Über null bedeutet, dass am Gleis
  ein Bahnsteig liegt und Reisende dort ein- und aussteigen können; null bedeutet, dass keiner vorhanden
  ist. Ein neuer Reisezug wird an jeder Betriebsstelle, die er anfährt, auf ein Gleis mit Bahnsteig
  gelegt — bevorzugt auf das Hauptgleis darunter — und nimmt das Hauptgleis, wo die Betriebsstelle keinen
  Bahnsteig hat. Ein Reisezug darf weiterhin an einem Gleis ohne Bahnsteig stehen: er tauscht dort
  einfach nichts aus, und genau das tut er, wenn er dort kreuzt, wo überhaupt keine Reisenden
  ausgetauscht werden.

  Wird **Reisende?** bei einer Betriebsstelle gesetzt, deren Gleise noch keinen Bahnsteig haben, erhält
  jedes Gleis einen Bahnsteig von einem Meter, den Sie anpassen. Ein Plan, der davor angelegt oder
  importiert wurde, wird beim ersten Öffnen genauso behandelt und arbeitet damit unverändert weiter — die
  Gleise, die in Wahrheit keinen Bahnsteig haben, kürzen oder leeren Sie danach. Eine Betriebsstelle, bei
  der bereits ein Bahnsteig eingetragen ist, bleibt unangetastet.

  Ein Reisezug, der zum Reisendenwechsel an einem Gleis ohne Bahnsteig hält, steht jetzt unter
  **Konflikte**. Sie entscheiden, welcher der beiden Fälle vorliegt: dem Gleis eine Bahnsteiglänge geben
  oder beim Halt **An** und **Ab** abwählen, womit der Zug dort nur steht und nichts austauscht. Nichts
  wird für Sie berichtigt, denn nur Sie wissen, was zutrifft. Wo eine Betriebsstelle nur an einem Gleis
  einen Bahnsteig hat — das Übliche auf einem kleinen Bahnhof — können zwei kreuzende Reisezüge ihn nicht
  beide haben, und gemeldet wird der Zug ohne. Die Prüfung lässt sich unter
  **Einstellungen › Validierung** abschalten.

### Fehlerbehebungen

- **Ein neuer Anlagenname wird jetzt überall gezeigt, wo der Name steht.** Der Anlagenname unter
  **Einstellungen › Allgemein** wurde allein in den Einstellungen geändert: die Titelseite des Hefts
  mit den allgemeinen Anweisungen, der Name in der oberen Leiste und der Dateiname, unter dem ein Plan
  gespeichert wird, zeigten weiterhin, wie die Anlage vorher hieß. Alle folgen jetzt dem Namen, so wie
  er eingegeben wird, und ein zuvor umbenannter Plan wird beim nächsten Öffnen richtiggestellt.

## Version 0.4.2

### Änderungen

- **Ein Zug lässt sich jetzt mitten in einen Umlauf einfügen.** Bisher konnte ein Umlauf nur vorwärts
  gebaut werden: Ein Zug ließ sich allein am Ende des Umlaufs anhängen. Zwischen den Zugteilen einer
  Zeile stehen nun kleine Übergänge, die zeigen, wo das Fahrzeug steht und wie lange, und vor dem
  ersten Teil einer, der zeigt, woher es gebracht werden muss. Ein Klick darauf fügt einen Zug in die
  Lücke ein — angeboten werden nur die Züge, die das Fahrzeug in der freien Zeit tatsächlich schafft.
  Eine Fahrt, die das Fahrzeug nicht dorthin zurückbringt, wo der Umlauf weitergeht, wird trotzdem
  eingefügt und als Konflikt gemeldet, bis die Rückfahrt eingefügt ist; so wird eine Hin- und
  Rückfahrt Fahrt für Fahrt in eine Standzeit eingepasst. Ein Übergang, an dem der Umlauf
  unterbrochen ist, wie ihn ein Import hinterlassen kann, ist gelb markiert; ein Klick bietet die
  Züge an, die die Lücke schließen.

- **Die App hat ein eigenes Symbol.** Bisher trug sie das Zeichen, das mit den Werkzeugen mitkommt, mit
  denen sie gebaut ist, und das nichts darüber sagte, wofür die App da ist. Jetzt zeigt sie die Front
  eines modernen Zuges auf einer dunkelblauen Fläche. Das Symbol erscheint im Tab des Browsers sowie
  auf dem Startbildschirm oder im Startmenü, wenn die App installiert wird, sodass sie von allem
  anderen zu unterscheiden ist, was gerade offen ist.

- **Auf ein Blatt passen jetzt zwölf Umlaufkarten statt zehn.** Die Karten waren 50 mm breit, sodass
  quer über ein A4-Blatt im Querformat nur fünf nebeneinander standen und am rechten Rand eine
  Handbreit Papier ungenutzt blieb. Sie sind jetzt 48 mm breit, also passen sechs nebeneinander und
  zwölf auf das Blatt, und das Blatt hat ringsum einen Rand, den gewöhnliche Drucker erreichen. Die
  Karten sind so hoch wie zuvor, und ihr Inhalt ist unverändert — sie sind nur etwas schmaler, sodass
  bei jedem gedruckten Umlauf ein Sechstel des Schneidens und Sortierens entfällt.

- **Die Zeilen im Fahrplan stehen jetzt weiter auseinander.** Die Zeilen lagen so dicht beieinander,
  dass das Auge beim Verfolgen einer Zeile mit Zeiten die Spur verlor — und genau dafür wird das Blatt
  gelesen. Um jede Zeile ist jetzt ein Siebtel mehr Luft, sodass sich eine Zeile leichter über die
  Seite verfolgen und ein Bahnhof leichter in der Spalte finden lässt. Die Schrift ist so groß wie
  zuvor und die Spalten sind so breit wie zuvor, das Blatt fasst also dieselben Züge — gewachsen ist
  allein der Abstand zwischen den Zeilen. Auf eine Seite gehen jetzt neununddreißig Zeilen statt
  fünfundvierzig, und das ist so viel Luft, wie sich geben ließ, ohne dass die häufigste Strecke ein
  zweites Blatt bräuchte.

### Fehlerbehebungen

- **Der gedruckte Fahrplan verliert die letzten Zeilen einer Seite nicht mehr.** Beide Richtungen eines
  Fahrplanabschnitts wurden auf dieselbe Seite gesetzt, auch wenn dort nicht beide Platz hatten, und
  die zweite brach mitten in ihrer Liste der Stationen ab — die Zeilen, für die kein Platz mehr war,
  wurden abgeschnitten, statt auf die nächste Seite zu wandern.

  Wie viel auf eine Seite passt, war für das Blatt aus dem Drucker berechnet, doch der Bericht am
  Bildschirm war in einer größeren Schrift gesetzt als der gedruckte, sodass seine Zeilen fast zwei
  Drittel höher standen als die gezählten. Der Bericht am Bildschirm und das gedruckte Blatt sind jetzt
  gleich gesetzt, sodass die Seite am Bildschirm ein wahres Bild des Papiers ist: Was sie am Bildschirm
  füllt, füllt sie auch auf dem Papier, und was keinen Platz findet, wandert auf die nächste Seite,
  statt abgeschnitten zu werden. Wie viel hineinpasst, wird jetzt an einer wirklichen Seite gemessen
  statt aus der Schriftgröße errechnet, und am Fuß jeder Seite bleiben drei Zeilen frei, sodass ein
  Abschnitt, der eine Zeile oder zwei zu lang ist, eine eigene Seite bekommt, statt über den Rand zu
  laufen.

- **Die Wagenstromliste nennt jetzt die Ziele, zu denen die Wagen gehen.** Unter **Güterverkehr ›
  Güterzüge** stand in der Auswahlliste nur „Wagen nach“ ohne die Ziele, sodass sich die Einträge nicht
  unterscheiden ließen. Die Ziele sind wieder da, und die Unterregisterkarte und ihre Spalte heißen
  jetzt **Güterziele** statt *Güterbeschreibungen*, denn Ziele sind es, was sie enthalten.

## Version 0.4.1

### Änderungen

- **Die Zugmeldelisten lassen sich jetzt als Dokumente speichern, die die Bahnhofsbetreiber bearbeiten
  können.** Über *Zugmeldelisten* im Menü Export erhält jeder besetzte Bahnhof ein eigenes Dokument im
  OpenDocument-Format, das LibreOffice Writer und die meisten anderen Textverarbeitungen öffnen. Es ist
  dafür gedacht, jedem Bahnhofsbetreiber vor dem Treffen seine eigene Liste zu schicken, damit er die
  örtlichen Anweisungen ergänzen kann, die nur er kennt — deshalb ein Dokument je Bahnhof und nicht ein
  Dokument mit den Blättern aller. Ist mehr als ein Bahnhof besetzt, kommen die Dokumente gemeinsam in
  einer ZIP-Datei, darin eine Datei je Bahnhof.

  Nichts im Dokument legt fest, wo seine Seiten enden. Der Name des Bahnhofs und die Telefonnummern der
  Bahnhöfe, zu und von denen er Züge meldet, wiederholen sich am Kopf jeder Seite, ebenso die
  Spaltenköpfe, aber wo die Seiten umbrechen, bleibt der Textverarbeitung überlassen. Ein Betreiber,
  der drei Züge und eine lange Anmerkung einfügt, erhält daher Seiten, die weiterhin sinnvoll umbrechen
  und weiterhin ihre Köpfe tragen, statt Text, der über Umbrüche läuft, die nur bis zum Beginn seiner
  Eingabe richtig waren. Schriftgrößen und Hervorhebungen sind benannte Formatvorlagen, sodass sich das
  Aussehen des ganzen Dokuments auf einmal ändern lässt und nicht Zeile für Zeile.

  Das Einzige, was ein solches Dokument nicht tragen kann, ist der Tagesabschnitt, den jede Seite
  abdeckt und den das gedruckte Blatt in seinem Kopf nennt: er hängt davon ab, welche Zeilen auf welche
  Seite fallen, was erst nach dem Umbruch feststeht — und nach der ersten Änderung wieder falsch wäre.
  Stattdessen ist jede Seite numeriert, und ihre erste und letzte Zeile sagen weiterhin, was sie
  abdeckt.

  Die gedruckten Blätter sind unverändert und bleiben die, mit denen während einer Fahrrunde gearbeitet
  wird: Druck weiterhin über das Menü Berichte.

- **Ein Zug, der zugleich von zwei Loks gezogen wird, sagt jetzt, von welchen beiden.** Der Konflikt
  nannte den Zug und die Minuten und ließ die Loks aus, und waren beide über genau denselben Abschnitt
  gebucht, lauteten seine zwei Hälften Wort für Wort gleich — er meldete also einen doppelt bespannten
  Zug, ohne zu sagen, was zu lösen ist. Jetzt wird die Lok auf jeder Seite genannt.

  Er wird auch nur noch an den zwei Umläufen angezeigt, die die doppelte Arbeit halten. Vorher stand er
  an jedem Umlauf, der diesen Zug irgendwo am Tag führt, sodass eine Lok, die den Zug auf einem ganz
  anderen Abschnitt übernimmt und deren eigener Umlauf in Ordnung ist, für einen Konflikt gemeldet
  wurde, an dem sie keinen Anteil hat.

- **Zwei Loks, die sich einen Zug über die Fahrrunden teilen, gelten nicht mehr als Konflikt.** Es
  wurden nur die Uhrzeiten verglichen, sodass eine Lok, die den Zug in den ungeraden Fahrrunden
  übernimmt, und eine andere in den geraden — nie am selben Tag beim Treffen, und genau der Sinn dieser
  Aufteilung — als ein von zwei Loks zugleich gezogener Zug gemeldet wurde. Gemeldet wird jetzt nur
  noch, wo beide für eine gemeinsame Fahrrunde gebucht sind, und der Konflikt nennt die Fahrrunden,
  wenn es nur einige von ihnen sind. Zwei Loks in einem Umlauf sind Doppeltraktion und waren ohnehin
  nie der Konflikt.

## Version 0.4.0

### Grundlegende Änderungen

- **Ein selbst angelegtes Fahrzeug wird jetzt durch seinen Betreiber und seine Nummer identifiziert.**
  Beide zusammen bezeichnen ein einziges wirkliches Fahrzeug, daher darf die Kombination in ein und
  derselben Fahrrunde nur einem Fahrzeug gehören — gleich welcher Art. Ein Wagensatz und eine Lokomotive
  können nicht mehr beide *DB 5* sein. Ein Fahrzeug ohne Betreiber wird allein durch seine Nummer
  identifiziert. Zwei Fahrzeuge dürfen weiterhin denselben Betreiber und dieselbe Nummer tragen, solange
  sich die Fahrrunden, in denen sie verkehren, nicht überschneiden — dann sind sie nie gleichzeitig beim
  Treffen.

  Ein **importiertes** Fahrzeug wird weiterhin durch die externe Id identifiziert, mit der es importiert
  wurde und die im Fahrplan seiner Herkunft bereits eindeutig ist; ein importierter Fahrplan meldet
  deswegen keine neuen Konflikte.

  Beim Anlegen oder Bearbeiten eines Fahrzeugs im Reiter Umläufe wird eine Identität, die ein anderes
  Fahrzeug bereits hat, jetzt abgelehnt, und eine Nummer muss angegeben werden. Vor dieser Regel
  erstellte Fahrpläne bleiben genau so, wie sie sind — es wird nichts für Sie umnummeriert — und jedes
  Fahrzeug, das sich eine Identität teilt, steht unter den Konflikten, jeweils einmal, damit Sie sehen,
  was eine neue Nummer braucht.

### Änderungen

- **Es gibt einen neuen Bericht: die Zugmeldeliste.** Ein eigener Satz Blätter für jeden Bahnhof, der
  besetzt ist — alle besetzten Bahnhöfe und alle Schattenbahnhöfe, ob besetzt oder nicht — mit den
  Zügen, die dieser Bahnhof abwickelt, in zeitlicher Reihenfolge. Ein Zug, der dort steht, erscheint
  zweimal, einmal für die Ankunft und einmal für die Abfahrt, denn einen Zug einzulassen und ihn zum
  nächsten Bahnhof abzulassen sind zwei verschiedene Handlungen im Abstand einiger Minuten. Ankünfte
  stehen auf Weiß, Abfahrten auf hellem Gelb, damit die beiden nie verwechselt werden. Züge, die nur
  durchfahren, stehen ebenfalls darauf, denn auch sie müssen durchgelassen werden. Jede Seite trägt den
  Namen des Bahnhofs, den Tagesabschnitt, den sie abdeckt, und die Telefonnummern der Bahnhöfe am
  anderen Ende der Zugmeldeabschnitte; jede Zeile hat je Fahrrunde ein Kästchen zum Abhaken, grau
  hinterlegt für die Fahrrunden, in denen der Zug nicht verkehrt. Jeder Bahnhof beginnt auf einer neuen
  Seite, sodass der Stapel einfach geteilt und ausgegeben werden kann. Druck über das Menü Berichte.

- **Die Felder zum Anlegen und Bearbeiten eines Fahrzeugs haben eine neue Reihenfolge,** an beiden
  Stellen dieselbe: Fahrzeugart, Traktionsart, Anzahl Einheiten, Betreiber, Nummer, Klasse, Fahrrunden
  und zuletzt die externe Id — was das Fahrzeug ist, dann was es identifiziert, dann wie es beschrieben
  wird und wann es verkehrt. Das bisher mit *Gesellschaft* bezeichnete Feld heißt jetzt *Betreiber*.

- **Eine externe Id lässt sich berichtigen, aber nicht mehr erfinden.** Die externe Id ist der Name, den
  ein Zug oder ein Fahrzeug in dem System trägt, aus dem er importiert wurde — sie bedeutet also nur
  dort etwas, wo sie herkommt. Was mit einer Id importiert wurde, hat sein Feld weiterhin — im Reiter
  Züge und im Fahrzeugdialog im Reiter Umläufe — und kann dort berichtigt werden; was nie eine Id hatte,
  bekommt jetzt kein Eingabefeld mehr. Ein im Planer angelegtes Fahrzeug erhält daher gar keine externe
  Id, wo ihm früher eine aus Klasse und Nummer erfundene gegeben wurde.

- **Die kleinste Zeit zwischen zwei Nutzungen desselben Gleises wird jetzt geprüft.** Die Einstellung
  gab es, aber nichts wertete sie aus. Bei 0 — wo sie beginnt und bleibt, bis Sie sie ändern — ändert
  sich an der Prüfung nichts: Zwei Züge stehen in Konflikt, wo sie gleichzeitig auf demselben Gleis
  stehen, und einer, der genau bei der Abfahrt eines anderen ankommt, ist eine Ablösung und kein
  Konflikt. Setzen Sie sie etwa auf 5, muss das Gleis dazwischen außerdem fünf Minuten frei sein, damit
  ein Fahrplan gemeldet wird, der das Gleis schneller wendet, als die Station es schafft. Genau fünf
  freie Minuten genügen, vier nicht.

  Ein solcher Konflikt nennt, wie kurz der Abstand tatsächlich ist und wie lang er sein müsste, statt
  eine Überschneidung zu behaupten, die die Zeiten gar nicht zeigen.

- **Eine Betriebsstelle kann jetzt eigene Anweisungen tragen.** Das Formular zum Anlegen und Bearbeiten
  einer Betriebsstelle hat das Feld **Anweisungen**, in Markdown geschrieben und neben einer Vorschau
  gezeigt wie die allgemeinen Anweisungen in den Einstellungen. Es ist dafür da, wie genau diese
  Betriebsstelle bei diesem Treffen betrieben wird — welche Gleise wofür genutzt werden, wie das
  Rangieren organisiert ist und was die Lokführer und das Personal vor Ort dort sonst wissen müssen. Wie
  die Betriebsstelle allgemein bedient wird, und jede sonstige Beschreibung von ihr, muss ihr Eigentümer
  bereitstellen; das gehört nicht in das Feld. Was Sie schreiben, wird mit der Betriebsstelle gespeichert
  und in ihrer Info-Ansicht gezeigt.

  Das Feld wird bei einer Station oder einem Industriegebiet angeboten, wo Reisende und/oder Güter
  ausgetauscht werden. Es wird nicht angeboten, wo es nichts anzuweisen gibt: an einer signalgesteuerten
  Stelle fahren die Züge nur vorbei, und eine sonstige Stelle bedient niemand, sodass ein Zug dort genau
  das tut, was sein Halt vorsieht, und nicht mehr.

- **Eine Stelle, an der ohne Personal Güter bedient werden, kann jetzt einen Schlüssel verlangen.** Wo
  die Weichen einer unbesetzten Station oder eines Industriegebiets verschlossen sind, wählen Sie im
  Bearbeitungsformular unter **Schlüssel hinterlegt in** den besetzten Bahnhof, der den Schlüssel
  verwahrt, und geben ihm eine Bezeichnung, wenn der Bahnhof mehrere verwahrt.

  Mehr ist nicht zu planen. Einem Güterzug, der in dem Bahnhof mit dem Schlüssel hält und später an der
  Stelle hält, die der Schlüssel aufschließt, wird bei der Abfahrt dort gesagt: *Schlüssel A1 zum
  Aufschließen von Bruket abholen*; beim nächsten Halt dort heißt es bei der Ankunft *Schlüssel A1 von
  Bruket abgeben*. Ein Zug, der an einer der beiden Stellen nur vorbeifährt, bekommt keinen Hinweis,
  denn er schließt nichts auf. Der Schlüssel wird beim letzten Halt im verwahrenden Bahnhof vor der
  Arbeit geholt und beim ersten danach abgegeben, sodass ein Zug, der dort zweimal hält, ihn nicht eine
  Fahrt länger mitführen muss.

  Ein Schlüssel gilt nur, solange beide Seiten stimmen. Markieren Sie die Stelle selbst als besetzt oder
  nehmen Sie die Besetzung vom Bahnhof, der den Schlüssel verwahrt, dann gilt der Schlüssel nicht mehr:
  es entstehen keine Hinweise daraus, und unter **Konflikte** steht, welche der beiden Änderungen es war.
  Der Schlüssel bleibt erhalten, statt verworfen zu werden — machen Sie die Änderung rückgängig, gilt er
  sofort wieder — und er bleibt im Formular stehen, wo Sie ihn auf einen anderen Bahnhof richten oder
  entfernen können.

### Fehlerbehebungen

- **Zwei Strecken, die von derselben Betriebsstelle ausgehen, wurden gezeichnet, als träfen sie sich
  nie.** Begann ein Fahrplanabschnitt genau an der ersten Betriebsstelle eines anderen, verband die
  beiden im Topologie-Diagramm nichts: jeder wurde als eigene Linie gezeichnet, ohne Abzweigung
  dazwischen. Der zweite verlässt diese Betriebsstelle jetzt wie jede andere Abzweigung und fällt im
  selben festen Winkel von ihr ab.

- **Jeder Grenzwert der Prüfungen nennt jetzt die Uhr, nach der er gemessen wird.** Die kleinste Zeit
  zwischen zwei Nutzungen desselben Gleises hatte gar keine Einheit, und die beiden
  Zuggeschwindigkeiten nannten nur *Uhr-Minuten*, was sich in beide Richtungen lesen ließ. Alle drei
  stehen jetzt in Schnelluhr-Minuten — der schnellen Uhr, nach der die Züge fahren, nicht der
  wirklichen Zeit. Die schnelle Uhr heißt in der ganzen App jetzt so, statt *Zeitraffer* oder
  *Modelluhr*.

- **Längen und Entfernungen sind jetzt in Metern ausgeschrieben,** ebenso der Zähler der
  Zuggeschwindigkeiten, damit das *m* nicht als Minute gelesen werden kann. Der Mindesthalt an einer
  Station steht jetzt ebenfalls in Schnelluhr-Minuten.

## Version 0.3.5

### Fehlerbehebungen

- **Gespeicherte Fahrpläne ließen sich unter Umständen nicht öffnen.** Beim Öffnen eines gerade gespeicherten Fahrplans trat ein Fehler auf,
der beim Benennen eines Landes auftrat. Es wurden keine Daten geladen – es gab keine Möglichkeit, diesen Fehler zu beheben. Eine Datei wird stückweise gelesen,
sobald sie eintrifft, und das Lesen der darin enthaltenen Länder führte zu diesem Problem. Ein bereits gespeicherter Fahrplan wird direkt geöffnet,
sodass Sie nichts weiter tun müssen.

- **Eine gespeicherte Fahrplandatei ist etwa siebenmal kleiner.** Das Speichern eines Fahrplans in einer Datei erfolgte in einem anderen Format als im Browser,
sodass die Einsparungen der letzten beiden Versionen nicht in der Datei ankamen:
Jeder Halt wurde doppelt gespeichert, und jede Zugkategorie, jeder Betreiber und jedes Land wurde für jeden Zug, jedes Fahrzeug und jede Aufgabe erneut gespeichert.
Eine Datei, die zuvor 8 MB groß war, benötigt nun nur noch etwas über 1 MB und lässt sich entsprechend schneller speichern und öffnen.
Ein mit einer früheren Version gespeicherter Fahrplan lässt sich weiterhin öffnen.
## Version 0.3.4

- **Die Felder Ank und Abf eines Halts richten sich jetzt danach, wo der Zug tatsächlich halten
  kann.** Ein Zug hält, um etwas auszutauschen, und braucht dafür einen Ort, der das kann: ein
  Reisezug dort, wo die Betriebsstelle Reisende annimmt, ein Güterzug dort, wo sie Fracht annimmt, und
  beides nicht an einer signalgesteuerten Betriebsstelle. Wo der Zug nicht halten kann, werden beide
  Felder leer und gesperrt gezeigt, und der Halt ist im Fahrplan wie im Bildfahrplan eine Durchfahrt.
  Nichts von dem, was Sie geplant haben, geht verloren — schalten Sie den Austausch wieder ein, und die
  Halte sind wieder da. Ein Schattenbahnhof hat immer beides, da er für alles außerhalb der Anlage
  steht; seine beiden Felder werden daher gesetzt und gesperrt gezeigt.

- **Ein Halt, an dem etwas hängt, lässt sich nicht mehr entfernen.** Ein Zugteil läuft von einem Halt,
  an dem der Zug abfährt, zu einem, an dem er ankommt, also müssen beide Enden Halte sein. Der erste
  und der letzte Halt des Zuges selbst sowie die Enden jedes Zugteils, über den ein Fahrzeugumlauf,
  ein Dienst oder ein Frachtfluss geplant ist, behalten ihr Feld nun gesetzt und gesperrt; der
  Mauszeiger darauf sagt, was es hält. Wo ein Zugteil dort endet, wo sein Zug nicht halten kann — ein
  Plan aus der Zeit vor dieser Regel —, wird das offen gesagt, damit Sie den Halt oder den Zugteil
  verschieben können.

- **Eine Zugkategorie trägt jetzt die Vorbereitungs- und Abschlusszeiten, mit denen ihre Züge geplant
  werden.** Jeder neue Zug der Kategorie wird so viele Minuten vor der Abfahrt bereitgestellt und so
  viele Minuten nach der Ankunft abgestellt, sodass Sie dieselben zwei Zahlen nicht mehr für jeden Zug
  eingeben müssen. Neben jedem der beiden Felder steht eine Schaltfläche *Erneut anwenden*, die diese
  eine Zeit allen Zügen gibt, die die Kategorie bereits hat, und meldet, wie viele geändert wurden.
  Beides sind getrennte Aktionen, sodass Sie die Vorbereitungszeit ändern können, ohne die
  Abschlusszeit anzurühren. Das erneute Anwenden verschiebt nur die Minuten ganz an den Enden eines
  Zuges: Er fährt, hält und kommt weiterhin genau zu den Zeiten, zu denen er es tat.

- **Die Betreiber sind auf der Titelseite eines Dienstheftes leichter zu lesen.** Die Zeile ist jetzt
  doppelt so groß gesetzt wie bisher, sodass ein Logo auf einen Blick zu erkennen und eine Signatur
  über einen Tisch hinweg zu lesen ist. Haben alle Betreiber des Dienstes ein Logo, entfällt das Wort
  *Betreiber* — die Logos sagen es selbst. Fehlt einem von ihnen das Logo, stehen weiterhin alle als
  Signatur da, fett und mit der Beschriftung davor.

### Fehlerbehebungen

- **Ein Dienstheft konnte einen Zugteil über den unteren Seitenrand hinaus drucken.** Der Bericht
  berechnet vor dem Druck, wie viele Zugteile auf eine Seite passen, und rechnete dabei mit rund der
  Hälfte mehr Platz, als eine A5-Seite tatsächlich hat. Was über den Seitenrand hinausragt, wird
  kommentarlos abgeschnitten: Dem zweiten Zugteil einer solchen Seite fehlte das Ende seines Fahrplans
  — oder er fehlte ganz, sodass ein Lokführer einen Dienst in der Hand hielt, dessen letzter Zug fehlte.
  Zugteile werden jetzt an dem gemessen, was die Seite wirklich fasst, und ein Zugteil, der nicht mehr
  passt, kommt auf die nächste Seite. Manche Hefte brauchen dadurch ein Blatt mehr als bisher.

- **Das Topologie-Diagramm konnte die Signaturen zweier Betriebsstellen übereinander drucken.** Die
  Betriebsstellen wurden allein nach ihrem Abstand gesetzt, sodass zwei nah beieinander liegende
  Betriebsstellen auf einer langen Strecke fast an derselben Stelle gezeichnet wurden und ihre
  Signaturen ineinander liefen. Sie werden jetzt nie enger gezeichnet, als es ihre beiden Signaturen
  brauchen, während der Rest der Strecke seine wahren Verhältnisse behält. Auch eine lange Signatur am
  Rand des Diagramms wird nicht mehr abgeschnitten.

- **Eine Abzweigung im Topologie-Diagramm konnte quer durch eine andere Strecke gezeichnet werden.**
  Eine Abzweigung fällt in einem festen Winkel von der Strecke ab, die sie verlässt; traf sie dabei auf
  eine Strecke im Weg, kam sie nie an ihr vorbei, wie weit sie im Diagramm auch nach unten geschoben
  wurde — sie wurde einfach quer darüber gezeichnet. Die Abzweigungen, die eine Strecke am weitesten
  hinten verlassen, werden jetzt zuerst gezeichnet, was den dahinter liegenden einen freien Weg nach
  unten lässt. Eine lange Abzweigung kann daher jetzt unter einer kurzen gezeichnet werden, die die
  Strecke weiter hinten verlässt.

- **Ein Plan konnte seine Züge unter Zugkategorien zeigen, die das Register Zugkategorien nicht
  führte.** Ein Zug trägt seine Kategorie bei sich, deshalb öffnete sich ein von einer früheren Version
  gespeicherter Plan mit nach Kategorie gruppierten Zügen, während die Liste der Kategorien leer war:
  das Kategorien-Auswahlfeld hatte nichts anzubieten, und kein Zug ließ sich in eine andere Kategorie
  verschieben. Mehrere Kategorien konnten außerdem für ein und dieselbe gehalten werden, sodass ihre
  Züge unter einer einzigen Überschrift zusammenkamen und zwei Züge verschiedener Kategorien mit
  derselben Nummer als eine doppelt vergebene Nummer gemeldet wurden. Beim Öffnen eines Plans wird die
  Liste der Kategorien nun aus den Kategorien seiner Züge vervollständigt, und jede Kategorie bleibt
  von den anderen getrennt.

- **Zwei Gesellschaften ohne eigene Nummer wurden für denselben Betreiber gehalten.** Eine Gesellschaft
  wird an einer Nummer erkannt, die die App für sie führt, und ein Plan konnte mehrere enthalten, die
  nie eine bekommen hatten. Züge verschiedener Gesellschaften mit derselben Zugnummer wurden dann als
  eine doppelt vergebene Nummer gemeldet. Jede Gesellschaft erhält nun eine eigene Nummer, sobald ein
  Plan geöffnet oder gespeichert wird; eine Gesellschaft aus dem Module Registry behält die Nummer, mit
  der sie gekommen ist.

- **Ein Plan speicherte seine Zugkategorien, Gesellschaften und Länder an mehr als einer Stelle.** Jede
  wurde dort geschrieben, wo sie beim Speichern zuerst angetroffen wurde — meist im ersten Zug, der sie
  verwendete —, während die Liste, in die sie gehört, nicht mehr als einen Verweis darauf enthielt. So
  konnte ein Plan Züge in Kategorien bekommen, die das Register Zugkategorien nicht kannte. Jede wird
  jetzt einmal geschrieben, in ihrer eigenen Liste, und alles, was sie verwendet, behält nur einen
  Verweis. Länder werden gar nicht mehr in den Plan kopiert, sodass eine Korrektur der Sprachen eines
  Landes jetzt auch Pläne erreicht, die davor gespeichert wurden. Ein von einer früheren Version
  gespeicherter Plan wird wie bisher gelesen und beim nächsten Speichern in Ordnung gebracht.

- **Ein Dienstheft nannte in der Überschrift eines Zugteils nur die Zugnummer.** Ein Zug wird durch
  Präfix und Suffix seiner Zugkategorie ebenso bezeichnet wie durch seine Nummer — Gt 1234, nicht
  1234 — und ein Lokführer, der das Heft mit dem Fahrplan oder mit dem Ausgerufenen vergleicht, hat
  nur diese Überschrift. Die Überschrift trägt jetzt die vollständige Zugbezeichnung mit Präfix und
  Suffix, hinter der Signatur des Betreibers.

## Version 0.3.3

- **Konflikte lassen sich jetzt dort lesen, wo sie angezeigt werden.** Eine Zeile mit Konflikten — ein
  Zug oder eine Zugkategorie unter **Züge**, ein Umlauf oder eines seiner Fahrzeuge unter **Umläufe**,
  ein Dienst unter **Dienste** — trägt jetzt ein Warnsymbol, und ein Klick darauf öffnet die Meldungen
  als lesbare Liste. Das Symbol nimmt die Farbe des schwersten Konflikts an und zählt sie, wenn es mehr
  als einer ist. Bisher standen die Meldungen nur in einem Kurzinfofenster, das erschien, während der
  Zeiger auf der Zeile ruhte — leicht zu übersehen und schwer zu lesen.
- **Eine Zugkategorie zeigt die Konflikte der Züge in ihr**, sodass sie beim Zuklappen der Kategorie
  nicht mehr verschwinden.
- **Der Reiter Züge öffnet jetzt mit der Liste der Zugkategorien**; die Züge einer Kategorie bleiben
  verborgen, bis Sie sie aufklappen, wodurch ein Plan mit vielen Zügen übersichtlicher wird. *Alle
  aufklappen* öffnet alle auf einmal, und eine Kategorie klappt von selbst auf, wenn Sie ihr einen Zug
  hinzufügen oder einen Zug in sie verschieben.
- **Beim Bearbeiten eines Zugteils in einem Umlauf steht jetzt, für welche Fahrzeugarten der Umlauf
  gilt** — Lokomotive, Triebzug oder Wagengruppe. Teilen sich mehrere Fahrzeuge einen Umlauf, wird jede
  Art einmal genannt; zeigen Sie darauf, werden die Fahrzeuge selbst genannt.

### Fehlerbehebungen

- **Die App konnte aufhören, Ihre Arbeit zu speichern, ohne es zu sagen.** Der Plan wird beim Arbeiten
  laufend im Browser gespeichert. Konnte die App einen Plan nicht schreiben — ein Zug mit weniger als
  zwei Halten oder ein Laufweg unter **Strecken › Fahrplanabschnitte**, aus dem alle Streckenabschnitte
  entfernt wurden —, schlug dieses Speichern stillschweigend fehl. Alles danach blieb am Bildschirm
  stehen, wurde aber nie gesichert: Nach dem erneuten Öffnen des Browsers war der Plan wieder auf dem
  Stand davor — mit den Betriebsstellen, aber ohne die seither angelegten Strecken und Züge. Beide
  Pläne lassen sich jetzt speichern, und schlägt ein Speichern doch einmal fehl, sagt es die Kopfzeile
  sofort, sodass Sie die verursachende Änderung rückgängig machen können, statt die Arbeit zu verlieren.

- **Eine gespeicherte Plandatei ist rund 40 % kleiner.** Jeder Halt wurde zweimal geschrieben — einmal
  beim Zug und einmal unter dem Gleis, an dem er liegt —, und die zweite Fassung zog einen Großteil des
  übrigen Plans mit sich. Ein mit einer früheren Version gespeicherter Plan lässt sich weiterhin öffnen.

- **Ein Zug, der auf einem Teil seines Laufs ohne Triebfahrzeug bleibt, wird jetzt gemeldet.** Die
  Prüfung fragte nur, ob *irgendwo* eine Lokomotive oder ein Triebzug den Zug fuhr; wurde ein Umlauf an
  einem Ende gekürzt, blieb der Rest des Zuges kommentarlos ohne Fahrzeug. Jetzt wird jeder Abschnitt
  geprüft, den der Zug fährt, und zwar für jede Fahrrunde, in der er fährt; der Konflikt nennt,
  zwischen welchen Betriebsstellen und in welchen Fahrrunden dem Zug das Triebfahrzeug fehlt. Pläne,
  die sauber aussahen, können das jetzt melden — die Lücke war immer da.

## Version 0.3.2

- Unter **Güterverkehr › Güterbeschreibungen** kann eine Herkunft oder ein Ziel jetzt jede
  Betriebsstelle sein, die Güter austauscht, nicht nur ein Bahnhof. Ein Industriegebiet behandelt
  immer Güterwagen, war aber bisher nicht wählbar, sodass Güter von und zu einer Industrie so
  beschrieben werden mussten, als gingen sie zum nächstgelegenen Bahnhof.
- Dieselben Listen sagen jetzt **Betriebsstelle** statt *Bahnhof*, da sie nicht mehr nur Bahnhöfe
  enthalten.
- Das Ändern einer Haltzeit im Reiter **Züge** **nimmt jetzt den übrigen Zug mit**. Eine **Abfahrt** wirkt
  vorwärts, in Fahrtrichtung: lässt man einen Zug an einer Betriebsstelle fünf Minuten länger stehen,
  erreicht er jede folgende Betriebsstelle fünf Minuten später. Eine **Ankunft** wirkt rückwärts: soll der
  Zug fünf Minuten später ankommen, fährt er an jeder vorherigen Betriebsstelle fünf Minuten später ab, so
  dass der Lauf bis zur Änderung mitgeht. Die Zeiten auf der anderen Seite bleiben stehen, die Fahr- und
  Aufenthaltszeiten bleiben erhalten, und die Änderung wird abgelehnt — das Feld fällt zurück —, wenn sie
  den Zug aus den Betriebszeiten des Plans führen würde.
- Die Halte eines Zuges sind immer in der **Reihenfolge seines Laufwegs** aufgelistet.
- Ein Zug, dessen Laufweg eine **Betriebsstelle überspringt** — zwei aufeinanderfolgende Halte ohne
  Strecke dazwischen —, wird jetzt als Konflikt gemeldet. Die Prüfung lässt sich unter
  **Einstellungen › Validierung** abschalten.
- **Die Zuggeschwindigkeit wird jetzt auch auf der letzten Strecke geprüft**, bis zu der Betriebsstelle, an
  der der Zug endet. Diese Strecke wurde bisher übersprungen.

- Ein Zugteil in einem **Umlauf** lässt sich jetzt **bearbeiten**: Der Stift an einem Zugteil öffnet
  seinen Anfangs- und Endhalt, sodass ein Umlauf umgeformt werden kann, ohne alles danach zu
  entfernen. Ein benachbarter Zugteil, der an den geänderten anschließt, passt sich mit an — wird
  ein Teil von A–C auf A–B verkürzt, wird der Gegenlauf von selbst zu B–A. Ein Nachbar, dessen
  eigener Zug am neuen Halt nicht hält, bleibt unverändert, und die entstandene Lücke wird als
  Konflikt gemeldet.

- Alles, was den Laufweg eines Zuges liest, folgt jetzt **der Reihenfolge, in der der Zug seine Halte
  befährt**, nicht der Eingabereihenfolge. Bei einem Zug, dessen Halte in falscher Reihenfolge
  eingegeben wurden — ein Halt, der nach einem erst später erreichten hinzugefügt wurde — verlief die
  Linie im **Bildfahrplan** im Zickzack zwischen Halten, zwischen denen der Zug nie fährt, und der Zug
  konnte in der Spalte der falschen Richtung landen; der gedruckte **Fahrplan** konnte eine Abfahrt
  dort zeigen, wo der Zug ankommt; **Automatisch erstellen** verkettete den Zug gar nicht, da er
  scheinbar anderswo beginnt; **Zug wiederholen** maß den Abstand ab dem falschen Halt; und das
  Neuberechnen der Zeiten nach einer geänderten Halteabfolge schlug ganz fehl. Auch die Auswahl eines
  Zugteils beim Hinzufügen listet die Halte in Fahrtreihenfolge. Importierte Pläne waren nie
  betroffen — dort sind beide Reihenfolgen gleich.

- **Zug hinzufügen** kann jetzt den **Gegenzug** gleich mit anlegen. Mit *Gegenzug?* entsteht neben dem
  ersten Zug auch der Zug zurück vom Ziel: dieselbe Strecke in Gegenrichtung, dieselbe Zuggattung und
  Geschwindigkeit und die nächste Nummer der Gegenrichtung. Seine Abfahrt ist entweder *so früh wie
  möglich* — die Ankunft des ersten Zuges plus Abschluss- und Vorbereitungszeit — oder eine Zeit, die
  Sie eingeben und die vor oder nach der Abfahrt des ersten Zuges liegen darf. Zusammen mit
  *Wiederholen?* werden beide Richtungen wiederholt, sodass ein ganzer Verkehr in beiden Richtungen in
  einem Zug geplant wird.

### Fehlerbehebungen

- Die **Kilometerangaben** im gedruckten Fahrplan und am Bildfahrplan werden jetzt auf ganze Kilometer
  gerundet. Sie wurden mit einer Nachkommastelle gedruckt, und der Entfernungsfaktor unter
  **Einstellungen › Zeit & Geschwindigkeit** konnte aus einer Streckenlänge einen krummen
  Kilometerbruchteil machen. Eine Zweigstrecke zeigt jetzt außerdem am Abzweigbahnhof dieselbe
  Kilometerangabe wie die Strecke, von der sie abzweigt.

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
