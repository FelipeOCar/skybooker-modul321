# Start und Fehlersuche

## Konfiguration

`Setup.cmd` erzeugt zwei lokale Dateien im selben Ordner wie die Solution:

- `.env`: zufälliger JWT-Schlüssel und Datenbank-Passwörter für Docker Compose.
- `skybooker.local.json`: dieselben Werte als .NET-Konfiguration für F5.

Das erneute Ausführen übernimmt bestehende Passwörter aus `.env`. Beide Dateien sind von Git ausgeschlossen. `.env` behalten, solange die zugehörigen Datenbank-Volumes bestehen. Beim Kopieren des Quellcodes auf einen neuen Rechner erzeugt Setup neue Werte und neue lokale Datenbanken.

`LocalConfiguration.Load` liest die lokale JSON-Datei direkt aus dem Solution-Verzeichnis ein. Umgebungsvariablen haben Vorrang. Visual Studio muss nach Setup deshalb nicht neu geöffnet werden. In Docker kommen die Werte ausschliesslich aus der Container-Umgebung.

| Einstellung | Zweck |
| --- | --- |
| `Jwt__Key` | Mindestens 64 UTF-8-Bytes; gemeinsamer Signaturschlüssel |
| `Jwt__Issuer` / `Jwt__Audience` | Standardwerte `SkyBooker.Auth` / `SkyBooker.Services` |
| `ConnectionStrings__AuthDb` | SQLite-Verbindung |
| `ConnectionStrings__FlightDb` | MongoDB-Verbindung bei lokalem F5-Start |
| `Mongo__Host`, `Mongo__Username`, `Mongo__Password` | MongoDB-Konfiguration in Docker |
| `ConnectionStrings__BookingDb` | SQL-Server-Verbindung |
| `Services__FlightUrl` | Ziel des BookingService für die Flugprüfung |

Die SQLite-Daten liegen bei F5 in `src/AuthService/data/auth.db`, bei Docker im Volume `auth-data`. Beim Wechsel der Startart sind daher die Benutzerlisten getrennt. In der jeweils gewählten Umgebung neu registrieren; MongoDB und SQL Server verwenden weiterhin ihre Container-Volumes.

## JWT in Swagger

1. Im AuthService `POST /api/register` mit Username, EMail und Passwort ausführen.
2. `POST /api/login` mit Username und Passwort ausführen.
3. `token` und `passengerId` aus der Antwort übernehmen.
4. In Swagger des FlightService bzw. BookingService **Authorize** öffnen und im Feld **Bearer** nur den Token einfügen.
5. Bei Buchungen `passengerId` aus der eigenen Login-Antwort verwenden.

Das Passwort benötigt 12–128 Zeichen, Gross- und Kleinbuchstaben, eine Ziffer und ein Sonderzeichen. Beispiel nur für die lokale Demo: `SkyBooker!2026`.

Der geforderte `GET /api/login` verwendet HTTP Basic. In Postman unter Authorization **Basic Auth** wählen. In Swagger den **Basic**-Eintrag verwenden. Zugangsdaten gehören nicht in URL-Parameter. Die lokale Demo verwendet HTTP auf Loopback; bei einer Veröffentlichung ausserhalb des eigenen Rechners HTTPS einrichten.

## Häufige Fehler

| Meldung / Symptom | Vorgehen |
| --- | --- |
| JWT-Konfiguration fehlt | `Setup.cmd` im selben entpackten Ordner wie `SkyBooker.sln` ausführen. |
| Kein passendes SDK | .NET 9 SDK installieren. `dotnet --list-sdks` prüfen. Nur die Runtime reicht nicht. |
| Ordneransicht, kein Startprojekt | Datei `SkyBooker.sln` über Datei → Öffnen → Projekt/Projektmappe öffnen. |
| Port bereits belegt | Alte F5-Ausführung mit Shift+F5 stoppen. Bei Docker `Stop_Docker.cmd` verwenden. |
| SQL-/Mongo-Anmeldung schlägt fehl | Die zur bestehenden Datenbank gehörende `.env` wiederherstellen. Ein neues Passwort ändert alte Datenbank-Benutzer nicht automatisch. |
| Docker nicht erreichbar | Docker Desktop starten und auf laufende Engine warten. Linux-Container verwenden. |
| API startet, verschwindet aber sofort | In der jeweiligen Konsole die erste Fehlermeldung lesen. Datenbanken zuerst mit `Start_Entwicklung.cmd` starten. |
| 401 trotz Login | Neu anmelden und Token ohne Anführungszeichen kopieren. Alle Dienste müssen denselben Schlüssel verwenden. |
| 403 bei Buchung | `passengerId` passt nicht zum angemeldeten Benutzer. Wert aus Login-Antwort einsetzen. |
| 422 bei Buchung | Flugnummer fehlt oder Flug ist bereits abgeflogen. Zuerst einen zukünftigen Flug anlegen. |
| 503 bei Buchung | FlightService nicht erreichbar oder dessen Antwort ungültig. Es wird keine Buchung gespeichert. |
| 409 bei Registrierung / Flug | Benutzername/E-Mail bzw. Flugnummer existiert bereits. Anderen Wert benutzen. |
| PowerShell findet Startdatei nicht | Im Terminal `.\Start_Entwicklung.cmd` eingeben, nicht `\Start_Entwicklung.cmd`. |

Diagnose aus dem Hauptordner:

```powershell
docker compose ps
docker compose logs --tail=80 auth-service flight-service booking-service
docker compose logs --tail=80 mongodb sqlserver
dotnet --list-sdks
```

`Stop_Docker.cmd` löscht keine Daten. Ein absichtlicher vollständiger Datenreset würde zusätzlich die Volumes löschen; dieser Schritt ist deshalb kein Bestandteil des normalen Startablaufs.
