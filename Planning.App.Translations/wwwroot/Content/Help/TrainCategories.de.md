Der Reiter **Zugkategorien** definiert, welche Arten von Zügen Sie fahren und wie sie dargestellt werden.

Eine Kategorie gruppiert Züge, die eine Klassifizierung teilen – zum Beispiel Schnellzug, Nahverkehr, Güterzug oder
Rangieren. Jede Kategorie hat:

- ein **Präfix**, das in Zugnummern und Beschriftungen verwendet wird,
- eine **Farbe**, mit der die Linie des Zuges im Bildfahrplan gezeichnet wird,
- eine **Klassifizierung**, die Validierung und Sortierung beeinflusst,
- eine **Gesellschaft**, die alle Züge der Kategorie betreibt und für einzelne Züge überschrieben werden kann,
- eine **Vorbereitungszeit** und eine **Abschlusszeit** in Minuten, die für jeden neuen Zug der
  Kategorie verwendet werden,
- ein **Haltemuster**, das angibt, wo ihre Züge unterwegs halten.

Richten Sie die benötigten Kategorien ein, bevor Sie sie im Reiter **Züge** Zügen zuweisen, damit jeder
Zug sein Aussehen und seine Regeln von seiner Kategorie erben kann.

## Vorbereitungs- und Abschlusszeiten

Ein Zug wird einige Minuten vor der Abfahrt bereitgemacht und einige Minuten nach der
Ankunft abgestellt. Das ist echte Arbeit: In dieser Zeit sind die Fahrzeuge und der Triebfahrzeugführer gebunden, und sie wird
überall mitgezählt, wo die Anwendung nach Zusammenstößen sucht.

Die beiden hier festgelegten Zeiten werden jedem Zug gegeben, den Sie danach in der Kategorie anlegen. Die Schaltfläche
**Erneut anwenden** neben jeder davon gibt diese Zeit allen Zügen, die die Kategorie bereits
hat, und sagt Ihnen, wie viele geändert wurden. Das erneute Anwenden der Vorbereitungszeit verschiebt die erste Ankunft jedes Zuges;
das erneute Anwenden der Abschlusszeit verschiebt die letzte Abfahrt jedes Zuges. Keines von beiden ändert, wo
oder wann ein Zug fährt – jede Abfahrt und Ankunft unterwegs bleibt genau, wie sie war. Die beiden sind
getrennte Aktionen, sodass Sie die eine ändern und die andere unverändert lassen können.

## Haltemuster

Das Haltemuster ist die Liste der Betriebsstellen, an denen Züge der Kategorie unterwegs halten.
Haken Sie eine Stelle an, und die Züge halten dort; lassen Sie sie ohne Haken, und sie fahren durch. Angeboten werden nur die
Stellen, an denen Züge der Kategorie überhaupt halten könnten – einer Personenzugkategorie die Stellen,
an denen Reisende ein- und aussteigen, einer Güterzugkategorie die Stellen, an denen Wagen getauscht werden.

Das Muster ist das, wonach der Laufweg eines neuen Zuges gebildet wird. Legen Sie einen Zug über **Züge** an, erhält er einen Halt
an jeder angehakten Stelle, die er passiert, und fährt durch den Rest. Wo ein Zug beginnt und wo er
endet, sind immer Halte, was auch immer angehakt ist: Dort wird er bereitgemacht und abgestellt.

Haken Sie nichts an, hat die Kategorie **kein Muster**, und nichts ist eingeschränkt: Ihre Züge halten
überall, wo sie abgeben können, was sie befördern, so wie Laufwege gebildet wurden, bevor es
Muster gab. Eine Kategorie, deren Züge ohne Halt fahren, richten Sie ein, indem Sie nur die Stellen anhaken, an denen sie halten.

Ein Zug, der an einer Stelle hält, die das Muster nicht nennt, wird unter **Konflikte** gemeldet, mit
Zug, Stelle und Uhrzeit. Nichts wird für Sie berichtigt, weil nur Sie sagen können, was von
beidem falsch ist: der Zug, der hält, wo er durchfahren sollte, oder das Muster, dem eine
Stelle fehlt, die seine Züge bedienen.

### Das Muster ausfüllen

**Aus den Zügen übernehmen** hakt die Stellen an, an denen die Züge halten, die die Kategorie bereits hat,
und ersetzt, was angehakt war. So wird eine Kategorie eingerichtet, sobald ihre ersten Züge geplant sind,
und es wird beim ersten Öffnen eines Plans, der vor den Haltemustern angelegt wurde, für Sie erledigt: Jede
Kategorie ohne Muster erhält das, nach dem ihre Züge die ganze Zeit gefahren sind, sodass nichts gemeldet wird,
was zuvor kein Fehler war.

**Leeren** entfernt das Muster wieder und lässt die Kategorie uneingeschränkt. Beachten Sie: Wird der Plan danach
geöffnet, wird es erneut aus den Zügen gefüllt, sodass eine Kategorie mit Zügen nicht lange
ohne Muster bleiben kann.

Eine Rangierkategorie hat kein Haltemuster. Ihre Aufgaben werden an einem Ort ausgeführt und führen nirgendwohin, sodass
es keinen Laufweg gibt, den ein Muster formen könnte.
