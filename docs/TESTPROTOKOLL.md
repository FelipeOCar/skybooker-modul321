# Testprotokoll

**Projekt:** SkyBooker · **Projektzuordnung:** Felipe Carvalho  
**Letzter automatisierter Testlauf:** 2026-09-28T13:44:06.0150555+02:00

## Ergebnis

**43 Tests ausgeführt, 43 bestanden, 0 fehlgeschlagen, 0 übersprungen.** Alle fünf Projekte (drei APIs, gemeinsame Bibliothek und Tests) wurden im Release-Modus erfolgreich kompiliert. Der Build meldete keine Compilerwarnungen und keine Compilerfehler. Die NuGet-Wiederherstellung mit gesperrten Paketversionen war ebenfalls erfolgreich.

Prüfumgebung: Ubuntu 24.04 x64, .NET SDK 9.0.318, .NET-Runtime 9.0.20. Die tatsächlichen Einzelresultate aus der TRX-Datei stehen in [TESTERGEBNISSE.csv](TESTERGEBNISSE.csv). Das ist ein hier ausgeführter Testlauf, kein bereits auf Felipes Windows-Rechner durchgeführter Abnahmetest.

## Was automatisiert geprüft wurde

| Bereich | Prüfungen |
| --- | --- |
| Registrierung und Login | Passwort-Hash statt Klartext, gleiche Passwörter mit unterschiedlichem Salt, Duplikate, falsches Passwort, Token-Claims |
| FluentValidation | Registrierung, Login, Flugnummer, Orte, Zeitangaben, Sitzplätze, Passagierdaten und Ticketanzahl |
| JWT-Middleware | Fehlender Token, falsche Signatur, falscher Issuer, falsche Audience, Ablauf, abweichender Algorithmus |
| Auth-API | Registrierung/Login über HTTP, GET-Login mit Basic, ungültige Basic-Header, No-Store |
| Flight-API | Erstellen, erzeugte ID, Lesen, doppelte Flugnummer, ungültige ID |
| Booking-API | Flugprüfung über den echten typisierten HTTP-Client, Erstellen/Lesen, eigene Buchungen, fremde PassengerId, unbekannter Flug ohne Speicherung |
| HTTP-Client | Erfolg, unbekannter Flug, ungültige Antwort, nicht erfolgreiche Antworten, Netzfehler und Timeout |
| Swagger | Generierung des OpenAPI-Dokuments und Vorhandensein aller Pflicht-Routen |

Die API-Integrationstests verwenden ASP.NET TestServer und die echten Controller, Manager, Validatoren sowie die JWT-Middleware. Auth und Booking nutzen dabei SQLite im Arbeitsspeicher; Flight nutzt den EF-Core-InMemory-Provider. Der Booking-HTTP-Client ruft den FlightController über TestServer auf. Damit werden HTTP-Verhalten und Fachlogik geprüft, aber weder die echte SQL-Server-Speicherung noch die MongoDB-Abfrageübersetzung ersetzt oder bewiesen.

## Zusätzlich geprüft

- Compose-YAML und die referenzierten Dockerfile-Pfade geprüft.
- JSON-Konfigurationen, Postman-Collection und Projektdateien erfolgreich eingelesen.
- AuthService als eigener Prozess mit SQLite gestartet; Readiness-Endpunkt antwortete erfolgreich.
- Alle 15 PowerPoint-Folien gerendert und visuell auf Lesbarkeit/Überlappungen geprüft; native bearbeitbare Tabellen und Sprechernotizen enthalten.
- Excel-Gantt gerendert; Summen, Start-/Endformeln und Verschiebung nach einer geänderten Dauer geprüft. Geplantes Budget: zwölf Stunden.

## Noch auf dem Zielsystem zu prüfen

**Ein vollständiger Lauf mit Docker, MongoDB und SQL Server wurde hier nicht erfolgreich ausgeführt.** Docker steht in der Prüfumgebung nicht zur Verfügung. Der zusätzliche direkte Start von MongoDB 8.0.16 scheiterte an einer Dateisystemoperation mit `Operation not permitted`. In der Folge konnte sich der FlightService nicht verbinden. Deshalb sind der geplante 20-Aufrufe-Durchlauf mit echten Diensten, der Paralleltest des MongoDB-Unique-Index und der Ausfalltest gegen separat gestartete Dienste hier nicht als bestanden ausgewiesen.

Ebenfalls noch offen: Windows-PowerShell-Startskripte tatsächlich unter Windows ausführen, F5 in Visual Studio/VS Code, Container-Build und -Healthchecks, SQL-Server-Initialisierung, MongoDB-Persistenz und GitHub Actions nach dem Push. Eine vollständig fehlerfreie Zielumgebung oder eine bestimmte Bewertung wird durch die automatisierten Tests nicht garantiert.

## Reproduzierbare Abnahme auf Windows

1. Docker Desktop mit Linux-Containern starten.
2. `Start_Docker.cmd` ausführen und auf gesunde Dienste warten.
3. `Test_Unit.cmd` ausführen: erwartet 43 erfolgreiche Tests.
4. `Test_API.cmd` ausführen: erwartet 20 erfolgreiche Statusprüfungen. Das Skript führt die Requests aus der mitgelieferten Postman-Collection aus.
5. Dieselbe Collection in Postman importieren und mit dem Collection Runner ausführen; dadurch laufen auch deren JavaScript-Tests in Postman.
6. Optional den Dienstausfall nach `DEMO.md` vorführen: 503 und keine neue Buchung.
7. Für F5: `Stop_Docker.cmd`, danach `Start_Entwicklung.cmd`; Solution öffnen und alle drei Webprojekte starten. Erneut `Test_API.cmd` ausführen.
8. Nach dem GitHub-Push beide CI-Jobs unter Actions auf Erfolg prüfen.

Die API-Prüfung legt pro Durchlauf neue Demo-Daten an. Lokales Stoppen entfernt keine Volumes. Den tatsächlichen Windows-/Docker-Abnahmelauf mit Datum und Ergebnis vor der Abgabe hier ergänzen.
