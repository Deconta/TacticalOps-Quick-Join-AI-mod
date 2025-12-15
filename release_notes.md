# Tactical Ops Quick Join - AI Mod Release Notes

This release includes significant improvements to the server list functionality, sorting, network stability, and map previews. Several long-standing bugs have been addressed to provide a more robust and user-friendly experience.

## Key Changes and Fixes:

### 1. Verbesserte Sortierlogik der Serverliste
*   **Robuste manuelle Sortierung:** Die Serverliste verwendet jetzt einen neuen, manuellen Sortiermechanismus, der zuverlässiger ist als die vorherige DataGridView-Sortierung.
*   **Favoriten immer oben:** Server, die als Favoriten markiert sind, werden nun immer an erster Stelle der Liste angezeigt, unabhängig von der gewählten Spaltensortierung.
*   **Standardmäßige Sortierung:** Die Standard-Sortierung für nicht-favorisierte Server erfolgt nach der Spieleranzahl (absteigend).
*   **Spaltensortierung:** Die Liste kann durch Klicken auf die Spaltenüberschriften nach beliebigen Kriterien sortiert werden.
*   **Behobener Fehler bei Favoritensternen:** Ein Problem wurde behoben, bei dem Favoritensterne aufgrund eines Zeichenkonflikts (U+2B50 anstelle von U+2605) nicht korrekt erkannt oder angezeigt wurden.

### 2. Live-Sortierung während der Aktualisierung
*   Die Serverliste wird jetzt während des Ladevorgangs dynamisch (alle 500 Millisekunden) sortiert, was ein flüssigeres und reaktionsfreudigeres Benutzererlebnis bietet, anstatt nur einmal nach Abschluss des gesamten Ladevorgangs.

### 3. Netzwerkstabilität und präzise Ping-Anzeige
*   **Behobener Freeze bei Serverabfrage:** Ein kritischer Fehler in der asynchronen UDP-Netzwerklogik, der dazu führte, dass die Anwendung bei der Abfrage bestimmter Server hängen blieb ("Querying details..."), wurde behoben.
*   **Genauere Ping-Berechnung:** Die Berechnung der Ping-Werte wurde korrigiert, um die Round-Trip-Zeit präzise zu messen, was zu realistischeren Ping-Anzeigen führt.
*   **Korrektes Timeout-Handling:** Netzwerkabfragen verwenden jetzt ein zuverlässiges Timeout, um ein unendliches Hängenbleiben bei nicht antwortenden Servern zu verhindern.
*   **Verbesserte Fehlerbehandlung:** Fehler bei Netzwerkoperationen werden jetzt protokolliert, anstatt stillschweigend ignoriert zu werden.

### 4. Korrekturen der Karten-Vorschau (Map Preview)
*   **Wiederherstellung der intelligenten Namensfindung:** Die ursprüngliche, komplexere Logik zur unscharfen Namensfindung von Karten wurde wiederhergestellt. Dies stellt sicher, dass Karten-Vorschauen für eine größere Vielfalt von Server-Kartennamen korrekt angezeigt werden.
*   **Stabilität des Vorschau-Fensters:** Die Anzeige des Karten-Vorschaufensters wurde robuster gemacht, um `NullReferenceException`-Fehler während der Anzeige, insbesondere bei fehlgeschlagenen Bildladevorgängen, zu verhindern.

### 5. Code-Qualität und Wartung
*   Entfernung der ungenutzten `ServerRowComparer`-Klasse.
*   Bereinigung der Event-Handler-Abonnements in `FormMain.Designer.cs`.
*   Behebung von Compilerwarnungen bezüglich der Null-Sicherheit von Variablen.

Wir empfehlen allen Benutzern, auf diese aktualisierte Version zu wechseln, um von den Verbesserungen der Stabilität und Benutzerfreundlichkeit zu profitieren.
