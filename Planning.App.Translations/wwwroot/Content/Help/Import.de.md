Der Reiter **Import** lädt einen Plan oder einen Teil eines Plans in die Anwendung.

### Unterstützte Dateien

- **`.json`** – ein Planungsdokument, das zuvor aus Timetable Planner gespeichert wurde.
- **`.ods` / `.xlsx`** – eine XPLN-Fahrplantabelle, die in das Planungsmodell importiert wird.
- **`.json`** – ein Hallenplan aus dem Modulzeichner oder eine Anlage, die ein anderes Programm in derselben Form
  geschrieben hat. Daraus wird ein neuer Plan mit den Betriebsstellen, Gleisen und Strecken des Hallenplans; jeder
  Streckenabschnitt erhält eine Fahrzeit aus seiner Länge.

Wählen Sie eine Datei mit **Fahrplan öffnen**. Große Dateien (bis 50 MB) werden unterstützt.

### Eine neuere Revision des Hallenplans

Wurde der geöffnete Plan aus einem Hallenplan erstellt und öffnen Sie eine andere Revision desselben Hallenplans,
wählen Sie: den geöffneten Plan damit aktualisieren oder daraus einen neuen Plan erstellen. Beim Aktualisieren werden
neue Längen sofort übernommen, ebenso Fahrzeiten, sofern Sie keine eigenen eingegeben haben.

Änderungen, die Halte verschieben oder hinzufügen, stehen unter **Zu bestätigen**: ein Gleis, das entfällt, eine
Betriebsstelle anderer Art, eine, die entfällt oder zwischen zwei anderen eingefügt wird. **Prüfen** zeigt, was die
Änderung mit den Zügen macht; **Übernehmen** führt sie aus. Änderungen, die die Reihenfolge der Betriebsstellen ändern,
sind nicht übernehmbar, weil die Züge ihnen nicht folgen können; nehmen Sie sie von Hand vor. Öffnen Sie dieselbe Datei
erneut, wird angezeigt, was noch offen ist.

### Teilimport

*Kommt*: Mit dem Teilimport können Sie auswählen, welche Teile eines Plans importiert werden.

### Importmeldungen

Nach dem Import werden etwaige Meldungen aufgelistet, gruppiert nach Schweregrad (Information, Warnung, Fehler).
Die Liste öffnet sich automatisch, wenn Fehler vorliegen. Die Meldungen berichten über Referenzprobleme (etwa
ein Halt, der auf einen fehlenden Bahnhof verweist) und Fahrplankonflikte, sodass sie auch dann einen Blick wert sind,
wenn der Import gelingt. Diese Meldungen zeigen meist Importprobleme mit XPLN-Dateien.

Ein erfolgreicher Import wird zum aktiven Plan, und die Zusammenfassung zeigt die Anzahl der geladenen Züge, Strecken
und Bahnhöfe.
