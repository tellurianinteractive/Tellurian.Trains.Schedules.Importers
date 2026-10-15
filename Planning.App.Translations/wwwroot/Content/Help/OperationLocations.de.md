Der Reiter **Betriebsstellen** beschreibt die betrieblichen Orte Ihrer Bahn – die Bahnhöfe und anderen
Stellen, an denen Züge fahren, halten oder gesteuert werden, samt ihren Gleisen.

### Wie wähle ich die Art einer Betriebsstelle?

1. **Signalgesteuert**: wenn sie **unbesetzt** ist und durch **Signale** von einem benachbarten **Bahnhof** gesteuert wird, legen Sie **gesteuert von** fest.
   Es ist eine **Abzweigstelle**, an der mehr als zwei Streckenabschnitte zusammentreffen, eine **Kreuzungsstelle**, bei der Sie
   **Züge können kreuzen?** anhaken, beides oder keines von beidem – ein dazwischenliegender **Blockposten**.
2. **Industriegebiet**: wenn es **unbesetzt** und **nur für Güter** ist. Kann als **gesteuert von** einem benachbarten **Bahnhof** definiert werden und einen **Schlüssel** zum Auf-/Abschließen haben, siehe *Schlüssel* unten.
3. **Bahnhof**: Kann wahlweise **besetzt** sein. Ein Bahnhof kann als **Schattenbahnhof** bestimmt werden.
   Ist er unbesetzt, kann ein Bahnhof als **gesteuert von** einem benachbarten **Bahnhof** definiert werden und einen **Schlüssel** zum Auf-/Abschließen haben, siehe *Schlüssel* unten.
4. **Sonstige**: nicht **besetzt**, nicht durch **Signale** gesteuert, muss aber im Fahrplan stehen, als betrieblicher Ort der Art:

Ein unbesetzter Bahnhof, ein Industriegebiet oder eine signalgesteuerte Abzweig- oder Kreuzungsstelle, die von einem besetzten Bahnhof **gesteuert**
wird, wird vom Fahrdienstleiter dieses Bahnhofs bedient, als wäre sie seine eigene: Ihre Züge stehen auf der
Zugmeldeliste dieses Bahnhofs. Ein Blockposten teilt die Strecke nur in Abschnitte. Haken Sie **Züge können
kreuzen?** selbst an: Die Gleise können das nicht erkennen, denn eine Abzweigstelle hat zwei, gleichgültig ob Züge kreuzen können. Wo niemand eine unbesetzte Stelle steuert, ist sie nur Teil der Strecke zwischen
den Fahrdienstleitern, und Züge können sich dort nicht begegnen. Siehe *Zugleitabschnitte* im Reiter **Strecken**.

#### Gleise einer Betriebsstelle

Die hier definierten Gleise werden bei der Planung der Zughalte an Betriebsstellen verwendet.
- Eine Betriebsstelle **muss** mindestens ein **Fahrplangleis** haben.
- Die Gleiszuweisung für Züge wird validiert, sodass ein Gleis nur von einem Zug gleichzeitig belegt sein kann.

#### Bahnsteige

Wo eine Stelle **Reisende** austauscht, hat jedes Gleis auch eine **Bahnsteiglänge** in Metern, mit einer
Dezimalstelle. Eine Länge über null bedeutet, dass es am Gleis einen Bahnsteig gibt; null bedeutet, dass keiner da ist, und
Reisende dort weder ein- noch aussteigen können. Die Spalte wird nur angezeigt, wo Reisende ausgetauscht werden,
da ein Bahnsteig anderswo nichts bedeutet.

Ein neuer Personenzug wird an jeder Stelle, an der er **hält**, auf ein Gleis mit Bahnsteig gestellt – bevorzugt auf das **Hauptgleis**
unter ihnen. Wo die Stelle keinen Bahnsteig hat und wo der Zug nur durchfährt, nimmt er
das Hauptgleis wie jeder andere Zug. Das geschieht an einer Stelle, die keine Reisenden austauscht:
Der Zug steht dort, ohne etwas auszutauschen, und genau das ist eine Begegnung oder ein
Überholen.

Haken Sie **Reisende?** für eine Stelle an, deren Gleise noch keine Bahnsteige haben, erhält jedes Gleis
einen Bahnsteig von einem Meter, den Sie anschließend anpassen. Dasselbe geschieht beim ersten Öffnen mit einem Plan, der vor der Einführung der Bahnsteiglängen angelegt oder importiert wurde: Jedes Gleis einer Stelle, die Reisende austauscht, erhält einen
Meter, sodass der Plan genau wie zuvor weiterfunktioniert, und Sie kürzen oder leeren die Gleise, die in Wahrheit
keinen Bahnsteig haben. Eine Stelle, an der bereits ein Bahnsteig erfasst ist, bleibt unberührt, ebenso ein
Gleis, das Sie einer solchen Stelle hinzufügen: Sie legen selbst fest, ob dieses Gleis einen Bahnsteig hat.

Ein Personenzug, der zum Austausch von Reisenden an einem Gleis ohne Bahnsteig hält, wird unter
**Konflikte** aufgeführt, und Sie entscheiden, welche von zwei Möglichkeiten es ist: Geben Sie dem Gleis eine Bahnsteiglänge, oder leeren Sie
die Felder **An** und **Ab** des Haltes im Reiter **Züge**, was besagt, dass der Zug dort nur steht,
ohne etwas auszutauschen. Nichts wird für Sie berichtigt, da nur Sie wissen, was zutrifft. Hat eine Stelle nur an einem Gleis einen
Bahnsteig – die übliche Anordnung in einem kleinen Bahnhof –, können zwei sich dort begegnende Personenzüge
ihn nicht beide haben, und der, der ihn nicht hat, wird gemeldet. Die ganze Prüfung kann unter
**Einstellungen › Validierung** abgeschaltet werden.

#### Auf welches Gleis ein Zug gestellt wird

Hat eine Stelle mehr als ein Gleis und eine Weiterfahrtmöglichkeit, kann jedes Gleis angeben, für welchen Weg
durch die Stelle es gilt: die **vorherige** Stelle, von der ein Zug kommt, die **nächste**, zu der er weiter
fährt, oder beides. Haken Sie **beide Richtungen** an, gilt dasselbe Gleis auch für Züge in der Gegenrichtung.
Angeboten werden nur die Stellen, die von hier über einen Streckenabschnitt erreicht werden, sodass ein Gleis nur für einen
Weg benannt werden kann, den ein Zug tatsächlich nehmen kann.

Das braucht eine **zweigleisige Strecke**: Geben Sie einem Gleis die vorherige und nächste Stelle der einen Richtung und dem anderen Gleis
dasselbe Paar umgekehrt, lassen Sie beide Richtungen ohne Haken, und jede Richtung erhält
ihr eigenes Gleis. Lassen Sie die Spalten leer, ändert sich gegenüber früher nichts.

Ein neuer Zug wird auf das Gleis gestellt, das am besten zu seinem Laufweg passt. Ein Gleis, das genau benennt, woher der Zug
gekommen ist und wohin er weiterfährt, schlägt eines, das nur eines von beiden benennt, und das wiederum schlägt ein Gleis,
das nichts benennt; ein Gleis, das für eine Stelle benannt ist, die der Zug nie berührt, wird gar nicht verwendet, es sei denn, jedes
Gleis der Stelle ist so, und dann steht der Zug trotzdem auf dem besten davon. Passen zwei Gleise
gleich gut zum Laufweg, nimmt ein Personenzug, der **hält**, ein Gleis mit Bahnsteig, und ein Zug, der
**durchfährt** – und jeder Zug ohne Reisende zum Austausch –, nimmt das Hauptgleis. Es werden nur Gleise im
Fahrplan berücksichtigt, solange es eines gibt.

Die Spalten werden nur angezeigt, wo es eine Wahl zu treffen gibt. Nichts geht verloren, wo sie
ausgeblendet sind: Reduzieren Sie eine Stelle auf ein Gleis, ist das, wofür die Gleise benannt waren, wieder da, sobald
Sie ein weiteres hinzufügen. Löschen Sie eine Stelle, werden die Gleise freigegeben, die für Züge von und zu ihr benannt waren.

#### Wo können Züge sich begegnen/überholen?

- In einem **besetzten** Bahnhof.
- An einer unbesetzten **signalgesteuerten Stelle**.

#### Wo Züge halten

Die Art der Stelle entscheidet, ob ein Zug halten darf, zusätzlich zu Ihrer Einstellung jedes Haltes.
- Sie legen fest, ob eine Betriebsstelle **Reisende** und/oder **Fracht** austauscht:
- Ein **Schattenbahnhof** tauscht immer beides aus, was die beiden Felder auch sagen, weil er für
  alles jenseits der modellierten Anlage steht – daher werden die Felder bei einem angehakt und deaktiviert angezeigt.

Ein Zug braucht außerdem etwas zum Austauschen. Ein Personenzug hält nur dort, wo **Reisende** ausgetauscht werden, und
ein Güterzug nur dort, wo **Fracht** ausgetauscht wird.
Ein gemischter Zug mit Personen- und Güterwagen kann an Betriebsstellen halten, die **Reisende** und/oder **Fracht** austauschen.

Wo ein Zug nicht halten kann, sind seine Felder An und Ab im Reiter **Züge** geleert und
nicht anhakbar. Nichts geht verloren: Schalten Sie den Austausch wieder ein, sind alle zuvor geplanten Halte
wieder da.

### Mit der Liste arbeiten

Die Liste zeigt Signatur, Name, Art, Eigentümer jeder Stelle, ob sie ein Schattenbahnhof ist (Endstelle für externe Bahnhöfe), und wie
viele Gleise sie hat. Für jede Zeile können Sie:

- **Info** – alle Angaben ansehen, einschließlich der Gleise, schreibgeschützt.
- **Bearbeiten** – ein Formular an Ort und Stelle mit den Feldern für die Art der Stelle öffnen.
- **Löschen** – die Stelle entfernen. Zuerst wird Ihnen alles gezeigt, was das Löschen ändern würde, und
  Sie bestätigen oder brechen ab. Züge, die dort halten, verlieren diese Halte: Ein durchfahrender Zug
  fährt nun geradewegs vorbei, und einer, der dort beginnt oder endet, beginnt oder endet nun an seinem nächsten bzw. vorherigen
  Halt. Liegt die Stelle zwischen genau zwei Nachbarn, werden ihre zwei Streckenabschnitte durch einen ersetzt, der
  die Nachbarn verbindet (so lang und mit derselben Fahrzeit wie beide zusammen), und die Fahrplanabschnitte
  durch sie laufen stattdessen darüber. Das Löschen wird abgelehnt, mit aufgelisteten Gründen,
  solange ein Fahrzeugumlauf, ein Lokführerdienst oder ein Wagenstrom dort beginnt oder endet oder solange ein Zug
  einen nicht zusammenhängenden Laufweg erhielte – einen, der dort umkehrt oder als
  Abzweigstelle durchfährt.

**Neu hinzufügen** fragt zuerst nach der Art (Bahnhof, signalgesteuert oder sonstige Stelle) und öffnet dann
das Bearbeitungsformular für diese Art. Eine neue Stelle wird gespeichert, sobald ihr Name und eine eindeutige Signatur ausgefüllt sind.

#### Anweisungen

Ein **Bahnhof** oder **Industriegebiet** hat ein Feld für **Anweisungen**, in Markdown geschrieben und neben einer Live-Vorschau angezeigt. Es dient dazu, wie *diese* Stelle bei *diesem* Treffen betrieben wird: welche Gleise wofür genutzt werden, wie
das Rangieren organisiert ist und was die Triebfahrzeugführer und die Besetzung der Stelle sonst wissen müssen. Wie
die Stelle allgemein betrieben wird und jede sonstige Beschreibung davon hat der Eigentümer zu liefern; das
gehört nicht hierher.

Das Feld wird nicht angezeigt, wo es nichts anzuweisen gibt: Züge fahren an einer signalgesteuerten
Stelle nur vorbei, und an einer sonstigen Stelle arbeitet niemand, sodass ein Zug dort tut, was sein Halt sagt, und nicht mehr.

#### Gleise bearbeiten

Das Bearbeitungsformular listet die Gleise der Stelle auf, die Sie hinzufügen, bearbeiten und löschen können. Ein Gleis kann nur
gelöscht werden, solange kein Zug darauf verweist; andernfalls ist seine Löschschaltfläche deaktiviert und zeigt, wie viele
Züge es nutzen. Weisen Sie diese Bahnhofshalte zuerst neu zu oder entfernen Sie sie.

### Bahnhöfe und Regionen

Ein Bahnhof kann mit einer oder mehreren **Regionen** aus dem Regionskatalog der Anlage verknüpft werden (verwaltet im Reiter
**Regionen**). Das dient hauptsächlich Schattenbahnhöfen, die externe Bahnhöfe oder Regionen
jenseits der modellierten Bahn darstellen. Hat die Anlage noch keine Regionen, können Sie die Standardmenge aus dem Formular hinzufügen.

### Umsteigebahnhöfe

Wird das Treffen mit **Fahrkarten** betrieben – unter **Einstellungen › Allgemein** eingeschaltet –, sind manche
Bahnhöfe die Orte, an denen ein Reisender einen Zug verlässt und mit dem nächsten weiterfährt. Haken Sie **Umsteigebahnhof?**
an jedem davon an.

Mehr muss nicht geplant werden. Ein Personenzug, der an einem solchen **hält**, wird bei der Ankunft angewiesen, *umsteigende
Reisende hier auszutauschen*. Ein Zug, der nur vorbeifährt, übergibt niemanden, und ein Güterzug hat niemanden zu übergeben, sodass
beide keinen Hinweis erhalten.

Die Umsteigebahnhöfe können auch im Heft mit den allgemeinen Anweisungen aufgeführt werden, indem Sie `<Interchanges/>` in
einer eigenen Zeile in den ständigen Anweisungen unter **Einstellungen › Information** schreiben. Sie werden dort gedruckt, wo
Sie das Tag platzieren, und sonst nirgends.

Der Haken wird nur angeboten, wo er etwas bedeutet: Das Treffen muss Fahrkarten verwenden, und der Bahnhof muss
**Reisende** austauschen. Schalten Sie die Fahrkarten aus, bleiben die Markierungen erhalten, genau wie ein Schlüssel – in der Zwischenzeit wird
nichts daraus abgeleitet, und schalten Sie die Fahrkarten wieder ein, sind die Umsteigebahnhöfe sofort wieder da.

### Schlüssel

Wo Fracht ausgetauscht wird, aber niemand im Dienst ist – ein unbesetzter Bahnhof oder ein Industriegebiet –, sind die
Weichen meist mit Vorhängeschlössern gesichert, und der Schlüssel wird in einem besetzten Bahnhof an der Strecke aufbewahrt. Setzen Sie **Schlüssel hinterlegt in**
auf diesen Bahnhof und geben Sie dem Schlüssel einen Namen, wenn der Bahnhof mehr als einen aufbewahrt.

Mehr muss nicht geplant werden. Ein Güterzug, der im schlüsselhaltenden Bahnhof hält und später an der Stelle hält,
die der Schlüssel aufschließt, wird bei seiner Abfahrt vom schlüsselhaltenden Bahnhof angewiesen, *Schlüssel A1
zum Aufschließen von Bruket abzuholen*; hält er auf dem Rückweg das nächste Mal in diesem Bahnhof, wird er bei der Ankunft angewiesen, *Schlüssel
A1 von Bruket abzugeben*. Ein Zug, der an beiden Stellen nur vorbeifährt, erhält keinen Hinweis, da er nichts aufschließt.

Der Schlüssel wird beim letzten Halt im aufbewahrenden Bahnhof vor der Arbeit geholt und beim ersten
danach zurückgegeben, sodass ein Zug, der dort zweimal hält, nicht gebeten wird, den Schlüssel für einen zusätzlichen Besuch mitzuführen.

Ein Schlüssel hat nur Bedeutung, solange beide Enden zutreffen. Markieren Sie die Stelle selbst als besetzt oder nehmen Sie die
Besetzung vom Bahnhof, der den Schlüssel aufbewahrt, gilt der Schlüssel nicht mehr: Es werden keine Hinweise daraus erzeugt, und
**Konflikte** sagt, welche der beiden Änderungen das bewirkt hat. Der Schlüssel selbst bleibt erhalten, er wird nicht verworfen, sodass das Rückgängigmachen
dieser Änderung ihn sofort zurückbringt – und er bleibt im Formular, wo Sie ihn auf einen anderen
Bahnhof richten oder ganz leeren können.

#### Rangierloks

Ein Bahnhof, ein Industriegebiet oder eine sonstige Stelle kann **Rangierloks** haben: Lokomotiven, die dort für
das nötige Rangieren der Stelle stationiert sind. Öffnen Sie die Stelle mit **Bearbeiten** und klicken Sie unter
**Rangierloks** auf **Rangierlok hinzufügen**. Geben Sie für jede Betreiber, Nummer, Klasse und Traktionsart an, und wenn Sie möchten, eine Bemerkung. Eine
Rangierlok hat keinen Umlauf und wird nie auf einen Zug gestellt. Sie ist in jeder Fahrrunde da, daher dürfen ihr Betreiber und
ihre Nummer von keinem anderen Fahrzeug verwendet werden; eine bereits vergebene Nummer wird abgelehnt.

Wer eine Rangierlok mitbringt, ihre DCC-Adresse und ihre Einheitennummer werden im Reiter **Fahrzeugbesitzer** erfasst, wie bei
jeder Lokomotive. Eine signalgesteuerte Stelle kann keine Rangierloks haben, daher kann eine Stelle mit Rangierloks nicht
in eine solche geändert werden. Wird eine Stelle gelöscht, werden auch ihre Rangierloks gelöscht, worüber Sie vor der Bestätigung informiert werden.
