# Bewertungskriterien und Nachweise

Projektverantwortung: **Felipe Carvalho**

Die folgende Zuordnung ist ein Nachweisverzeichnis, keine zugesagte Punktzahl. Der vollständige Docker-Start, die GitHub-CI und die Live-Präsentation müssen auf der Zielumgebung bzw. nach dem Push tatsächlich vorgeführt werden.

| Kriterium | Punkte | Umsetzung / Nachweis |
| --- | ---: | --- |
| Entwicklungswerkzeug und Repository | 2 | `SkyBooker.sln`, `.vscode/`, lokales Git-Repository |
| Änderungen mit Commits dokumentiert | 2 | `git log --oneline`, nachvollziehbare Erstellungsschritte |
| Coderichtlinien | 2 | `.editorconfig`, englische C#-Bezeichner, PascalCase-Typen, async/await, klare Schichten |
| FlightService vollständig | 2 | `src/FlightService`, drei Pflicht-Endpunkte und Lookup |
| BookingService vollständig | 2 | `src/BookingService`, drei Pflicht-Endpunkte |
| AuthService vollständig | 2 | `src/AuthService`, Registrierung, GET- und POST-Login |
| Flug-Datenbank | 2 | EF-Core-MongoDB-Modell, Collection `flights`, eindeutiger Index |
| Buchungs-Datenbank | 2 | SQL-Server-DbContext, Tabelle `Booking`, Identity und CHECK-Constraint |
| Auth-Datenbank | 2 | SQLite-DbContext, Tabelle `User`, eindeutige Indizes und Passwort-Hashes |
| JWT | 2 | `SkyBooker.Shared/ApiSetup.cs`, `AuthService/Services/TokenService.cs` |
| Docker-Veröffentlichung | 2 | Drei Dockerfiles, Compose, Healthchecks, Volumes; Zielsystemlauf nachweisen |
| FluentValidation | 2 | Validatoren und Controller-Prüfung in allen drei Diensten |
| OpenAPI / Swagger | 2 | Pro Dienst `/swagger`, DTOs, Statuscodes und Security-Schemata |
| Postman | 2 | `postman/SkyBooker.postman_collection.json`, 20 Prüfaufrufe |
| Unit-Tests | 2 | `tests/SkyBooker.Tests`, zusätzlich API-Integrationstests |
| Optional 1: AO1 Logging | 4 | Serilog zentral konfiguriert und in allen drei Diensten aktiv |
| Optional 2: AO4 HTTP-Kommunikation | 4 | `BookingService/Services/FlightClient.cs`, Flugnummernprüfung vor Speicherung |
| Gantt und Zeitplanung | 2 | `SkyBooker_Zeitplan.xlsx`, `ZEITPLAN.md`, zwölf Stunden geplant |
| Präsentationsaufbau / Medien | 2 | PowerPoint, Architektur, Codebezug, geplante Live-Demo |
| Lesbare Folien | 2 | Bearbeitbare PowerPoint mit einheitlichem Layout |
| Lösung erläutert | 2 | Architektur-/Datenmodell-Dokumentation und Sprechernotizen |
| Live-Demo | 2 | Ablauf in `DEMO.md`; von Felipe vorzuführen |
| Fazit | 2 | Präsentationsabschluss und fachliche Grenzen |
| **Maximal** | **50** | Endgültige Bewertung durch die Lehrperson |

Es werden genau AO1 und AO4 als Zusatzanforderungen ausgewiesen. Andere mögliche Zusatzanforderungen werden nicht als umgesetzt beansprucht.

**Quelle:** https://github.com/INFEFZ/Modul321/blob/main/G4_Exams/2-practical-work.md
