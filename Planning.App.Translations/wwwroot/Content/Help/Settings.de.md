Im Reiter **Einstellungen** steuern Sie, wie der Fahrplan der aktuellen Anlage
berechnet, gezeichnet und validiert wird.
Die Einstellungen werden im Planungsdokument gespeichert.

### Allgemein

Benennen Sie die Anlage und legen Sie ihr Betriebsfenster fest.
Sie können wahlweise **nummerierte Fahrrunden** oder **Wochentage** verwenden, wenn Sie
an verschiedenen Fahrrunden unterschiedlichen Betrieb brauchen. Verkehren alle Züge, Umläufe und Dienste
an allen Fahrrunden gleich, müssen Sie nichts tun – alles gilt dann standardmäßig für **alle Fahrrunden**.

**Fahrkarten verwenden** bedeutet, dass das Treffen mit Fahrkarten betrieben wird: Ein Reisender kauft eine dort, wo die
Reise beginnt, und fährt damit. Ist dies gesetzt, können Sie im Reiter **Betriebsstellen** die
Bahnhöfe markieren, an denen Reisende den Zug wechseln. Diese Umsteigebahnhöfe erscheinen dann als Ankunftshinweis in den
Lokführerdiensten und können im Heft mit den allgemeinen Anweisungen aufgeführt werden.

Berichte werden in der **Standardsprache** der Anlage gedruckt: der ersten Sprache ihres Standardlandes,
unabhängig davon, in welcher Sprache Sie selbst arbeiten. **Berichte in lokalen Sprachen drucken** druckt stattdessen jeden Lokführerdienst in der Sprache
der Gesellschaft, die ihn fährt, jede Umlaufkarte in der Sprache der Gesellschaft des Fahrzeugs und die Zugmeldeliste
jedes Bahnhofs in der Sprache des Landes des Bahnhofs.
Alles ohne eine solche Sprache behält die Standardsprache.

### Fahrrunden & Tage

In manchen Planungsszenarien ist es nützlich, die Fahrrunden/Tage wählen zu können, an denen gefahren wird.
Sie können verkehrende Fahrrunden/Tage festlegen für:
- Züge, zum Beispiel einen Zug, der Fahrrunde 1–3 fährt, und einen anderen, der 4–6 fährt.
- Fahrzeuge, zum Beispiel zwei- oder dreitägigen Umlauf von Lokomotiven.
- Dienste, wenn sich die Aufgaben je Fahrrunde unterscheiden.

### Zeit & Geschwindigkeit

Legen Sie die Schnelluhr-Geschwindigkeit fest und ordnen Sie Geschwindigkeitsklassen Maßstabs- und Realgeschwindigkeiten zu. Die Standardzeiten der
Bahnhöfe (Mindesthalt, Lokumlauf, Zugfreigabe) werden bei der Berechnung der Zeiten für
Bahnhöfe verwendet, die sie nicht selbst überschreiben.

### Validierung

Wählen Sie, welche Prüfungen laufen und welche Schwellenwerte sie verwenden. Schalten Sie eine Prüfung aus, werden ihre
Meldungen unterdrückt, ohne die zugrunde liegenden Daten zu entfernen.

### Bildfahrplan

Die **Gemeinsamen Einstellungen** steuern, was die Grafik zeigt – die Ausrichtung der Zeitachse und welche
Beschriftungen (Ankunfts-/Abfahrtsminuten, Zugkategorie, Gesellschaft) gezeichnet werden. Sie gelten sowohl am Bildschirm
als auch im gedruckten Bericht. Beachten Sie: Wenn Züge über 24:00 hinaus fahren, werden sie umgebrochen und setzen sich ab 00:00 fort.

Darunter enthalten die Blöcke **am Bildschirm** und **gedruckter Bericht** dieselben Arten von Abständen für
jeden der beiden: Bildschirmabstände sind in Pixeln, Druckabstände in Millimetern Papier angegeben. Die Druckmaße sind maßstabsgetreu,
sodass gedruckte Grafiken verglichen und gemessen werden können; die Seitenzahl ergibt sich daraus.

### Integration

*Kommt*: API-Schlüssel und Einstellungen für die Anbindung externer Dienste, etwa der Module Registry.
