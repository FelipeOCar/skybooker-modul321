# SkyBooker

**Felipe Carvalho · Modul 321 · Praktische Projektarbeit**

Flugverwaltung und Passagierbuchungen mit drei ASP.NET-Core-Microservices. Das Projekt enthält den bearbeitbaren C#-Quellcode, eine Visual-Studio-Solution, VS-Code-Konfiguration, Docker Compose, automatisierte Tests und Abgabeunterlagen.

## Schnellstart mit Visual Studio

1. ZIP vollständig entpacken, beispielsweise nach `C:\Projekte\SkyBooker_Felipe_Carvalho`.
2. Docker Desktop starten, **Linux-Container** verwenden.
3. **`Start_Entwicklung.cmd`** ausführen. Warten auf «Datenbanken bereit».
4. **`SkyBooker.sln` doppelklicken**. Nicht nur den Ordner öffnen.
5. Rechtsklick auf die **Solution** → **Startprojekte konfigurieren / Eigenschaften** → **Mehrere Startprojekte**.
6. **AuthService**, **FlightService** und **BookingService** auf **Starten** setzen. **SkyBooker.Shared** und **SkyBooker.Tests** auf **Keine** lassen.
7. **F5** drücken. Bei den Webprojekten das Startprofil **http** verwenden.

| Dienst | Swagger | Datenbank |
| --- | --- | --- |
| AuthService | http://127.0.0.1:32510/swagger | SQLite, Tabelle `User` |
| FlightService | http://127.0.0.1:32511/swagger | MongoDB, Collection `flights` |
| BookingService | http://127.0.0.1:32512/swagger | SQL Server, Tabelle `Booking` |

Es gibt keine fest eingerichteten Benutzer. Zuerst registrieren, dann anmelden. Am einfachsten: **`Test_API.cmd`** ausführen oder die gesamte Postman-Collection starten.

## Start mit Visual Studio Code

1. Ebenfalls zuerst **`Start_Entwicklung.cmd`** ausführen.
2. **`SkyBooker.code-workspace`** öffnen und die empfohlenen C#-Erweiterungen installieren.
3. Unter **Ausführen und Debuggen** das Profil **SkyBooker – alle Dienste** auswählen.
4. **F5** drücken. Alle drei Dienste starten gemeinsam.

Einzelne Dienste lassen sich ebenfalls über die gleichnamigen Profile starten. Der Build läuft jeweils vor dem Start. Quellcode direkt in `src/` ändern, bei Bedarf Debugging stoppen und mit F5 neu starten.

## Komplett in Docker starten

**`Start_Docker.cmd`** führt die Einrichtung aus, baut die Images und startet alle Container. Visual Studio und ein lokal installiertes .NET SDK sind für diesen Modus nicht nötig. Docker Desktop muss laufen. Der erste Start benötigt Internet und kann einige Minuten dauern.

**`Stop_Docker.cmd`** beendet die Container und behält die Daten. Für den Wechsel von Docker zu F5 wieder `Start_Entwicklung.cmd` ausführen. Das stoppt die API-Container und lässt die Datenbanken laufen. Vor einem Wechsel von F5 zu Docker die drei lokalen APIs mit Shift+F5 stoppen.

## Voraussetzungen

- Windows x64 mit Docker Desktop und aktivierter Virtualisierung/WSL 2. Der SQL-Server-Container in dieser Lösung benötigt x64.
- Für F5: Visual Studio 2022 ab 17.14 mit **ASP.NET und Webentwicklung** und **.NET 9 SDK**, oder VS Code mit .NET 9 SDK und C#-Erweiterung.
- Für Git-Verwaltung: Git für Windows. Für die geforderte API-Demo: Postman.
- Ausreichend Arbeitsspeicher für Docker und die IDE, praktisch mindestens 8 GB RAM am Rechner; SQL Server benötigt davon eigenen freien Speicher.
- Die Ports `32510`, `32511`, `32512`, `32517` und `32533` müssen frei sein.

## Testen

| Aktion | Ausführung |
| --- | --- |
| Unit- und API-Integrationstests | `Test_Unit.cmd` oder `dotnet test SkyBooker.sln -c Release` |
| Die laufenden APIs prüfen | `Test_API.cmd` |
| Postman | `postman/SkyBooker.postman_collection.json` importieren und gesamte Collection in Reihenfolge starten |
| Einzelne Aufrufe in der IDE | `api/SkyBooker.http` |
| Automatische Prüfung nach GitHub-Push | `.github/workflows/ci.yml` |

Der API-Test legt pro Durchlauf einen neuen Testbenutzer, einen Flug und eine Buchung an. Die Daten bleiben für die Live-Demo erhalten. Die Prüfergebnisse und Grenzen stehen in [docs/TESTPROTOKOLL.md](docs/TESTPROTOKOLL.md).

## Umsetzung

- JWT schützt sämtliche Flight- und Booking-Endpunkte. Tokens gelten 15 Minuten.
- FluentValidation prüft Registrierungen, Login-Daten, Flüge und Buchungen.
- Passwörter werden mit `PasswordHasher` gesalzen gehasht, nicht im Klartext gespeichert.
- **AO1:** Serilog protokolliert Anfragen und fachliche Ereignisse in allen drei Diensten.
- **AO4:** BookingService validiert die Flugnummer über einen typisierten HTTP-Client beim FlightService.
- Jeder Dienst besitzt seine Datenbank und nutzt Entity Framework Core. Für MongoDB wird der offizielle EF-Core-Provider verwendet.

**Umfang der Buchung:** Das Projekt prüft, ob der Flug existiert und noch nicht abgeflogen ist. Die optionale Sitzplatzreservierung/Überbuchungssperre AO5 ist nicht implementiert. `available_seats` ist Flugstammdatum und wird bei Buchungen nicht reduziert.

## Weiterbearbeiten und abgeben

- [Start, Konfiguration und Fehlersuche](docs/START_UND_FEHLERSUCHE.md)
- [Architektur, Datenmodell und Entscheidungen](docs/ARCHITEKTUR.md)
- [Bewertungskriterien und Nachweise](docs/BEWERTUNG.md)
- [GitHub veröffentlichen und weiterarbeiten](docs/GITHUB.md)
- [Zeitplanung und Arbeitsaufteilung](docs/ZEITPLAN.md)
- [Live-Demo und Vortragsablauf](docs/DEMO.md)
- `docs/SkyBooker_Zeitplan.xlsx`: bearbeitbarer Gantt-Plan
- `docs/SkyBooker_Praesentation.pptx`: Präsentation mit Sprechernotizen

Das lokale Git-Repository enthält echte Commits der Erstellung. Es ist noch nicht mit GitHub verbunden. Vor dem ersten eigenen Commit die Git-E-Mail gemäss `docs/GITHUB.md` einstellen.

**Auftragsquelle:** https://github.com/INFEFZ/Modul321/blob/main/G4_Exams/2-practical-work.md
