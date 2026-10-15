Der Reiter **Strecken** definiert, wie Betriebsstellen verbunden sind und welche Verbindungen jeden
Fahrplan bilden. Er hat drei Unterbereiche, die Sie der Reihe nach bearbeiten.

### Streckenabschnitte

Ein **Streckenabschnitt** ist die Linie zwischen zwei benachbarten Betriebsstellen, mit Entfernung, Anzahl der
Gleise, Geschwindigkeit und Fahrzeit. Streckenabschnitte beschreiben das physische Netz und liefern die
Entfernungen, mit denen Zuggeschwindigkeiten geprüft und der Bildfahrplan gezeichnet wird.

Ein Streckenabschnitt hat eine **Richtung** (von einer Stelle zur nächsten). Alle Streckenabschnitte müssen
in derselben Richtung definiert sein, damit ein Zug sie ohne unbeabsichtigte Richtungswechsel befahren kann. Mit zum Beispiel
`A→B B→C D→C C→E E→F E→G` kann ein Zug `A→G`, `D→G`, `A→F` und `D→F` fahren, ohne die
Richtung zu wechseln; `A→D` oder `F→G` erfordert einen Richtungswechsel dort, wo die Linien aufeinandertreffen. Definieren Sie einen
Abschnitt falsch herum (sodass zwei Abschnitte aufeinander zeigen oder eine Schleife bilden), listet eine Warnung
die Abschnitte auf, die nicht übereinstimmen – beheben Sie das, indem Sie einen davon bearbeiten, sodass alle in dieselbe Richtung verlaufen.

Ein Streckenabschnitt, der Teil eines Fahrplanabschnitts ist, kann erst gelöscht werden, wenn er aus diesem
Fahrplanabschnitt entfernt wurde.

### Zugleitabschnitte

Ein **Zugleitabschnitt** verläuft zwischen zwei Stellen, an denen Züge abgefertigt werden – einem **besetzten** Bahnhof, einem
**Schattenbahnhof** oder einer Stelle, die von einem besetzten Bahnhof **gesteuert** wird – und führt geradewegs durch alle
unbesetzten Stellen dazwischen. So bilden `A→B` und `B→C` mit unbesetztem `B` einen Zugleitabschnitt `A→C`.

Eine Abzweigstelle, Kreuzungsstelle, ein unbesetzter Bahnhof oder ein Industriegebiet, das von einem besetzten Bahnhof **gesteuert** wird,
wird vom Fahrdienstleiter dieses Bahnhofs bedient, als wäre es sein eigener Bahnhof. Es beendet die Zugleitabschnitte, die
dort zusammentreffen, seine Züge stehen auf der Zugmeldeliste des steuernden Bahnhofs, mit seiner Signatur gekennzeichnet, und die
Bahnhöfe dahinter gehören zu denen, die der steuernde Bahnhof anruft. Ein **Blockposten** – eine signalgesteuerte
Stelle, die weder Abzweigstelle noch ein Ort ist, an dem Züge kreuzen können – bleibt Teil der Strecke.

Zugleitabschnitte werden automatisch aus den Streckenabschnitten erzeugt: Drücken Sie **Aus Streckenabschnitten
neu erzeugen**, wann immer Sie das Netz ändern oder ändern, welche Bahnhöfe besetzt sind. Markieren Sie einen Bahnhof
im Reiter **Betriebsstellen** als besetzt oder als Schattenbahnhof. Die Spalte *Über* zeigt die
unbesetzten Stellen, die ein Zugleitabschnitt durchquert.

#### Welche Züge gleichzeitig auf einem Zugleitabschnitt sein dürfen

Die Fahrdienstleiter an den beiden Enden vereinbaren, welche Züge auf dem Zugleitabschnitt zwischen ihnen sein dürfen, daher
beurteilt die Liste **Konflikte** jeden Zugleitabschnitt als Ganzes:

- Jede **signalgesteuerte Stelle** darauf, etwa ein Blockposten, teilt ihn in **Blockabschnitte**. Ein
  Abschnitt fasst je Gleis einen Zug gleichzeitig, gleichgültig in welcher Richtung die Züge fahren. Unbesetzte Stellen,
  die niemand steuert, teilen ihn nicht.
- Auf **eingleisiger Strecke** können Züge in Gegenrichtung nur an den Enden oder an einer
  signalgesteuerten Stelle mit der Markierung **Züge können kreuzen?** aufeinandertreffen. Sie können nie gleichzeitig zwischen zwei solchen Stellen sein,
  aber Züge in gleicher Richtung können dort aufeinander folgen, einer je Abschnitt.
- Ein Zug, der eine signalgesteuerte Stelle ohne Zeit dort passiert, belegt jeden Abschnitt, den er durchfährt,
  bis er ankommt, da nichts sagt, wann er durchfuhr.

Streckenabschnitte, die zu keinem Zugleitabschnitt gehören, werden stattdessen einzeln beurteilt.

### Fahrplanabschnitte

Ein **Fahrplanabschnitt** ist eine geordnete Folge zusammenhängender Streckenabschnitte, die Sie gemeinsam planen und
zeichnen wollen – typischerweise eine Linie von einem Endbahnhof zu einem anderen. Fahrplanabschnitte sind das, woraus Sie
im Reiter **Bildfahrplan** auswählen, und jeder wird zu einer Grafik.

Zum Aufbau geben Sie ihm eine Nummer (und eine optionale Beschreibung) und fügen dann seinem Laufweg Streckenabschnitte
einzeln hinzu. Angeboten werden nur Streckenabschnitte, die dort fortsetzen, wo der Laufweg gerade endet, sodass
der Laufweg stets zusammenhängend bleibt. Der Editor zeigt die entstehenden Bahnhöfe und die Gesamtentfernung.

### Empfohlene Arbeitsreihenfolge

Definieren Sie zuerst die Streckenabschnitte und fassen Sie sie dann zu den Fahrplanabschnitten zusammen, mit denen Sie arbeiten wollen.
