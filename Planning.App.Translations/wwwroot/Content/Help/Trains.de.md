Im Reiter **Züge** bauen Sie Züge und ihre Zeiten auf.

Ein Zug fährt eine Strecke entlang und hält unterwegs an Betriebsstellen. Für jeden Zug legen Sie
Nummer und Kategorie sowie seine **Bahnhofshalte** fest – die Stellen, die er passiert oder an denen er hält, mit
Ankunfts- und Abfahrtszeiten. Die Standardbahnhofszeiten aus **Einstellungen › Zeit & Geschwindigkeit**
dienen als Ausgangspunkt und können je Halt überschrieben werden.

Hier gezeichnete Züge erscheinen als Linien im **Bildfahrplan**, gefärbt nach ihrer Kategorie, und
werden anhand der Validierungsregeln unter **Einstellungen › Validierung** geprüft (Geschwindigkeiten, Gleisnutzung, Zugnummern
und so weiter).

## Je Zug oder je Betriebsstelle

Der Reiter hat zwei Ansichten derselben Halte:

- **Pro Zug** listet die Züge nach Kategorie auf. Öffnen Sie einen Zug, um seine Halte zu bearbeiten, jeweils einen Zug.
- **Pro Betriebsstelle** listet jeden Zug auf, der an einer Stelle hält, in der Reihenfolge, in der sie
  dort eintreffen: ob er beginnt, endet, hält oder durchfährt, woher er kommt und wohin er weiterfährt, seine Zeiten und
  das Gleis, das er benutzt. Wählen Sie in der Liste ein anderes Gleis, um den Zug dorthin zu verlegen. Das ist die Ansicht, um
  die Gleise eines stark befahrenen Bahnhofs zu ordnen, wo die Ansicht pro Zug bedeuten würde, jeden Zug zu öffnen, der dort hält.
  Ein Konflikt an der Stelle, etwa zwei Züge gleichzeitig auf einem Gleis, ist in der Zeile markiert.

## Was macht einen Zug im Plan eindeutig?

Züge werden anhand ihrer **Zugnummer** identifiziert.
Diese Nummern müssen innerhalb eines Plans eindeutig sein.
Geben Sie eine betreibende Gesellschaft an,
entweder in der **Zugkategorie** oder bei bestimmten Zügen,
muss eine Zugnummer innerhalb
dieser Gesellschaft eindeutig sein. So können Sie **DB 123** und **DSB 123** haben,
und es sind zwei verschiedene Züge.

## Start- und Endzeit der Arbeit an einem Zug
Zwei Zeiten haben eine besondere Bedeutung, damit der Planer Vorbereitungs- und Abschlusszeit angeben kann:
- Die **erste Ankunft** ist die Startzeit der Vorbereitungszeit.
  Zu dieser Zeit beginnt ein Führer die Arbeit an einem Zug. Das ist die Zeit, um eine Lokomotive aus der Abstellung zu holen
  oder zu prüfen, dass alle Wagenkarten zu den Wagen im Zug passen, und so weiter.
- Die **letzte Abfahrt** ist das Ende der Abschlusszeit.
  Zu dieser Zeit beendet ein Führer die Arbeit an einem Zug. Das ist die Zeit, die Lokomotive in die Abstellung zu fahren, und so weiter.

## Eine Haltezeit bearbeiten

Eine Zeit wird nie für sich allein geändert – der Rest des Zuges folgt und behält die Fahr- und
Haltezeiten, die er bereits hat. Eine **Abfahrt** wirkt vorwärts, in Fahrtrichtung des Zuges: Lassen Sie einen Zug fünf
Minuten länger stehen, erreicht er jede spätere Stelle fünf Minuten später. Eine **Ankunft** wirkt rückwärts: Bitten Sie den
Zug, fünf Minuten später anzukommen, verlässt er jede frühere Stelle fünf Minuten später. Die Zeiten
auf der anderen Seite des Haltes, den Sie bearbeiten, bleiben, wo sie sind, sodass sich an diesem Halt ändert, wie
lange der Zug dort steht. Eine Änderung, die den Zug außerhalb der Betriebszeiten des Plans führen würde,
wird als Ganzes abgelehnt, und das Feld fällt auf die bereits gespeicherte Zeit zurück.

## Halte und Durchfahrten

Die Felder **An** und **Ab** an einem Halt sagen, ob der Zug dort hält. Haken Sie keines an, fährt er
durch. Zwei Dinge nehmen Ihnen die Wahl ab:

- **Wo der Zug gar nicht halten kann**, sind beide Felder geleert und deaktiviert. Das gilt für jede
  signalgesteuerte Stelle und jeden Ort, der nichts austauscht, was der Zug befördert – ein Personenzug
  braucht, dass **Reisende** für den Ort angehakt ist, ein Güterzug **Fracht** (siehe **Betriebsstellen**).
  Ein Schattenbahnhof tauscht immer beides. Wird der Austausch wieder eingeschaltet, kehrt jeder zuvor geplante Halt
  zurück, sodass durch Ausprobieren nichts verloren geht.
- **Wo etwas vom Halt abhängt**, ist das Feld angehakt und deaktiviert. Ein Zugabschnitt läuft von einem
  Halt, an dem der Zug abfährt, zu einem Halt, an dem er ankommt, also müssen beide Enden Halte sein: der erste und
  letzte Halt des Zuges selbst sowie die Enden jedes Abschnitts, über den ein Fahrzeugumlauf, ein Lokführerdienst oder ein Wagenstrom
  geplant ist. Um das Feld freizugeben, verschieben Sie den Halt oder den Abschnitt, der dort endet.

## Bemerkungen

Die **Bemerkung** an einem Halt ist ein eigener Hinweis von Ihnen, gedruckt für den Triebfahrzeugführer und das Bahnhofspersonal
neben den Hinweisen, die der Plan selbst errechnet. Verwenden Sie *kursiv* und **fett** zur Hervorhebung; die Auszeichnung
erscheint, wenn Sie den Hinweis bearbeiten.

Die Berichte drucken Ankunft und Abfahrt eines Haltes als getrennte Zeilen, daher muss eine Bemerkung sagen, zu welcher
von beiden sie gehört. Das Feld neben dem Hinweis ist, wo Sie wählen: **An** für etwas, das beim Einfahren zu beachten oder
zu erledigen ist, **Ab** für etwas, das vor oder bei der Abfahrt erledigt wird. Wo der Zug nur ankommt oder
nur abfährt, ist diese Hälfte die einzige, die angeboten wird. Wo er durchfährt, kann überhaupt keine Bemerkung geschrieben werden
– haken Sie zuerst **An** oder **Ab** an, dann gibt es einen Ort, um eine anzuzeigen.

## Dialog „Zug hinzufügen“

Das Hinzufügen von Zügen soll einfach sein.
Sie fügen Züge im Dialog **Zug hinzufügen** hinzu. Es gibt mehrere Möglichkeiten:
- Einen einzelnen Zug anlegen.
- Einen einzelnen Zug mit einem Gegenzug in Gegenrichtung anlegen.
- Eine Anzahl wiederholter Züge mit einem vorgegebenen Intervall anlegen.
- Kombination aus Gegenzug und wiederholten Zügen.

Ein Gegenzug fährt dieselbe Strecke zurück, mit derselben Kategorie und Geschwindigkeit, und erhält die nächste
freie Zugnummer der Gegenrichtung. Seine Abfahrt ist entweder **so bald wie möglich** – die
Ankunft des ersten Zuges plus die Zeit, ihn abzuschließen und die Rückfahrt vorzubereiten – oder eine Zeit, die Sie eingeben und
die vor oder nach der Abfahrt des ersten Zuges liegen darf. In Verbindung mit wiederholten Zügen
werden zuerst beide Richtungen erzeugt und dann wiederholt, sodass die beiden Richtungen paarweise nummeriert werden.

Das Anlegen von Zügen schlägt fehl, wenn ein Teil eines Zuges außerhalb von Start- und
Endzeit des Plans liegt. In den **Einstellungen** können Sie jedoch festlegen, dass sich Start- und Endzeit des Plans
mit hinzugefügten Zügen erweitern.

## Verkehrende Fahrrunden oder Tage
Die Planung der Züge kann die Möglichkeit nutzen festzulegen, an welchen Fahrrunden (oder Wochentagen) ein Zug verkehrt.
Standard ist immer **Alle/Täglich**. Die verschiedenen Fahrrunden-Muster definieren Sie unter **Einstellungen › Fahrrunden & Tage**.
Es gibt gängige vordefinierte Muster, aber Sie können eigene hinzufügen.
