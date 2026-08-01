# Versionshinweise

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
