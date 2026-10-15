Der Reiter **Import** lädt einen Plan oder einen Teil eines Plans in die Anwendung.

### Unterstützte Dateien

- **`.json`** – ein Planungsdokument, das zuvor aus Timetable Planner gespeichert wurde.
- **`.ods` / `.xlsx`** – eine XPLN-Fahrplantabelle, die in das Planungsmodell importiert wird.

Wählen Sie eine Datei mit **Fahrplan öffnen**. Große Dateien (bis 50 MB) werden unterstützt.

### Teilimport

*Kommt*: Mit dem Teilimport können Sie auswählen, welche Teile eines Plans importiert werden.

### Importmeldungen

Nach dem Import werden etwaige Meldungen aufgelistet, gruppiert nach Schweregrad (Information, Warnung, Fehler).
Die Liste öffnet sich automatisch, wenn Fehler vorliegen. Die Meldungen berichten über Referenzprobleme (etwa
ein Halt, der auf einen fehlenden Bahnhof verweist) und Fahrplankonflikte, sodass sie auch dann einen Blick wert sind,
wenn der Import gelingt. Diese Meldungen zeigen meist Importprobleme mit XPLN-Dateien.

Ein erfolgreicher Import wird zum aktiven Plan, und die Zusammenfassung zeigt die Anzahl der geladenen Züge, Strecken
und Bahnhöfe.
